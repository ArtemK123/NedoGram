using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ChatCommon;
using ChatCommon.Constants;
using ChatCommon.Encryption;
using ChatCommon.Exceptions;
using ChatCommon.Extensibility;
using ChatCommon.Messages;
using ChatCommon.Messages.Requests;
using ChatCommon.Messages.Responses;
using ChatServer.Domain;
using ChatServer.Extensibility;

namespace ChatServer
{
    public class ClientInstance : IDisposable
    {
        protected internal Guid Id { get; }

        internal User user;
        internal readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;
        private readonly ServerInstance server;
        private readonly AesEncryption aesEncryption;
        internal byte[] clientAesKey;

        internal readonly Dictionary<ClientAction, Action<string>> RequestHandlers;

        public ClientInstance(ITcpWrapper tcpClient, ServerInstance serverInstance, ICoding coding)
        {
            Id = Guid.NewGuid();
            server = serverInstance;
            this.tcpClient = tcpClient;
            this.coding = coding;
            aesEncryption = new AesEncryption();
            user = new User();
            RequestHandlers = new Dictionary<ClientAction, Action<string>>()
            {
                { ClientAction.Login, LoginHandler },
                { ClientAction.Register, RegisterHandler },
                { ClientAction.ShowAllChats, ShowAllChatsHandler },
                { ClientAction.CreateChat, CreateChatHandler }
            };
        }

        public void Process()
        {
            try
            {
                Console.WriteLine($"New connection - {Id}");

                Console.WriteLine($"Key exchange");
                KeyExchange();

                Console.WriteLine(
                    $"Key exchanged successfully. Connection configured. Id-{Id}; Key-{Convert.ToBase64String(aesEncryption.GetKey())}");

                // handle requests from client

                while (true)
                {
                    byte[] rawMessage = tcpClient.GetMessage();

                    string messageInJson = ParseMessageToJson(rawMessage, clientAesKey);

                    Request request = JsonSerializer.Deserialize<Request>(messageInJson);

                    Console.WriteLine($"{request.Sender} - {request.Action}");
                    RequestHandlers[request.Action].Invoke(messageInJson);
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"User left. Username: {user?.Name}; id: {Id}");
                if (user != null && user.State == UserState.Authorized)
                {
                    server.UserRepository.UpdateState(user.Name, UserState.Offline);
                }

                server.RemoveConnection(this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void Dispose()
        {
            tcpClient?.Dispose();
        }

        internal void SendMessageAesEncrypted<T>(T message, byte[] aesKey) where T : Message
        {
            aesEncryption.SetKey(aesKey);
            tcpClient.Send(aesEncryption.Encrypt(coding.GetBytes(JsonSerializer.Serialize(message))));
        }

        private void KeyExchange()
        {
            while (true)
            {
                // send to the client server public key [and other credentials]

                tcpClient.Send(server.rsa.ExportRSAPublicKey());

                // get key for symmetric encryption from client

                byte[] encryptedMessageWithKey = tcpClient.GetMessage();

                try
                {
                    byte[] messageWithKeyBytes = server.rsa.Decrypt(encryptedMessageWithKey, false);
                    string messageWithKeyInJson = coding.Decode(messageWithKeyBytes);
                    AesKeyExchangeRequest aesKeyExchangeRequest = JsonSerializer.Deserialize<AesKeyExchangeRequest>(messageWithKeyInJson);

                    aesEncryption.SetKey(aesKeyExchangeRequest.Key);
                    aesEncryption.SetIV(aesKeyExchangeRequest.IV);

                    clientAesKey = aesEncryption.GetKey();

                    AesKeyExchangeResponse response = new AesKeyExchangeResponse(StatusCode.Ok);

                    SendMessageAesEncrypted(response, clientAesKey);
                    user.State = UserState.Connected;
                    return;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        private void LoginHandler(string requestInJson)
        {
            LoginRequest request = JsonSerializer.Deserialize<LoginRequest>(requestInJson);

            bool successful;
            User storedUser = null;
            try
            {
                user = server.UserRepository.GetByName(request.Sender);

                successful = user.Password == request.Password && user.State == UserState.Offline;
            }
            catch (Exception)
            {
                successful = false;
            }

            if (successful)
            {
                SendMessageAesEncrypted(new LoginResponse(StatusCode.Ok), clientAesKey);
                server.UserRepository.UpdateState(user.Name, UserState.Authorized);
                Console.WriteLine($"{request.Sender} signed in");
            }
            else
            {
                SendMessageAesEncrypted(new LoginResponse(StatusCode.Error, "Wrong email or password"), clientAesKey);
                throw new Exception($"{request.Sender} - unsuccessful try to sign in");
            }
        }

        private void RegisterHandler(string requestInJson)
        {
            try
            {
                RegisterRequest registerRequest = JsonSerializer.Deserialize<RegisterRequest>(requestInJson);
                User newUser = new User(registerRequest.Sender, registerRequest.Password, UserState.Authorized);
                if (!server.UserRepository.Add(newUser))
                {
                    throw new NedoGramException("User with this name already exist");
                }

                SendMessageAesEncrypted(new RegisterResponse(StatusCode.Ok, newUser.Name), clientAesKey);
                user = newUser;
                Console.WriteLine($"{user.Name} signed up successfully.");
            }
            catch (Exception exception)
            {
                SendMessageAesEncrypted(new RegisterResponse(StatusCode.Error, "", "Error while signing up"), clientAesKey);
                throw new Exception($"{user.Name} - error occured while signing up: {exception}");
            }
        }

        private void ShowAllChatsHandler(string requestInJson)
        {
            IReadOnlyCollection<string> chatNames = server.ChatRepository.GetChats().Select(chat => chat.Name).ToArray();

            SendMessageAesEncrypted(new ShowAllChatsResponse(chatNames, StatusCode.Ok), clientAesKey);
        }

        private void CreateChatHandler(string requestInJson)
        {
            CreateChatRequest request = JsonSerializer.Deserialize<CreateChatRequest>(requestInJson);
            try
            { 
                User creator = server.UserRepository.GetByName(request.Sender);

                if (creator == null || creator.State != UserState.Authorized)
                {
                    throw new NedoGramException("Invalid user");
                }

                if (server.ChatRepository.GetChat(request.ChatName) != null)
                {
                    throw new NedoGramException("Chat with this name already exists");
                }

                aesEncryption.GenerateKey();
                byte[] key = aesEncryption.GetKey();

                IChat newChat = new Chat(creator, request.ChatName, key);
                newChat.AddUser(creator);

                user.State = UserState.InChat;
                server.UserRepository.UpdateState(creator.Name, UserState.InChat);

                server.ChatRepository.AddChat(newChat);

                var response = new CreateChatResponse
                {
                    ChatName =  newChat.Name,
                    Code = StatusCode.Ok,
                    Key = newChat.Key,
                    RequestId = request.Id,
                };

                SendMessageAesEncrypted(
                    response, 
                    clientAesKey);

                Console.WriteLine($"Chat created. ChatName - {newChat.Name}, Creator - {creator.Name}");
            }
            // todo: system exception should not be available to the client
            catch (Exception exception)
            {
                var errorResponse = new CreateChatResponse
                {
                    Code = StatusCode.Error,
                    RequestId = request.Id,
                    Message = exception.Message
                };

                SendMessageAesEncrypted(errorResponse, clientAesKey);
                throw;
            }
        }

        private void EnterChatHandler(Message message)
        {
            //IChat chat = server.ChatRepository.GetChat(message.Headers["chatName"]);
            //if (chat == null)
            //{
            //    SendErrorResponse();
            //    return;
            //}
            //chat.AddUser(user);
            //user.CurrentChat = chat;
            //server.UserRepository.UpdateState(user.Name, UserState.InChat);
            //SendSuccessResponse(chat.Key);
        }

        private void MessageHandler(Message message, byte[] rawMessage)
        {
            //server.MessageSender.SendToChat(message.Headers["chat"], message);

            //server.BroadcastMessage(rawMessage, this);
        }

        private void ExitChatHandler(Message message)
        {
            user.CurrentChat.RemoveUser(user.Name);
            user.CurrentChat = null;
            server.UserRepository.UpdateState(user.Name, UserState.Authorized);
            //SendSuccessResponse();
        }

        private string ParseMessageToJson(byte[] rawMessage, byte[] aesKey)
        {
            aesEncryption.SetKey(aesKey);

            byte[] decryptedConnectionMessage = aesEncryption.Decrypt(rawMessage);

            return coding.Decode(decryptedConnectionMessage);
        }
    }
}
