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
using ChatServer.Domain.Entities;
using ChatServer.Domain.Exceptions;
using ChatServer.Extensibility;

namespace ChatServer
{
    public class ClientInstance : IDisposable
    {
        protected internal Guid Id { get; }

        internal User CurrentUser;
        internal readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;
        private readonly ServerInstance server;
        private readonly AesEncryption aesEncryption;
        internal byte[] ClientAesKey;

        internal readonly Dictionary<ClientAction, Action<string>> RequestHandlers;

        public ClientInstance(ITcpWrapper tcpClient, ServerInstance serverInstance, ICoding coding)
        {
            Id = Guid.NewGuid();
            server = serverInstance;
            this.tcpClient = tcpClient;
            this.coding = coding;
            aesEncryption = new AesEncryption();
            CurrentUser = new User();
            RequestHandlers = new Dictionary<ClientAction, Action<string>>
            {
                { ClientAction.Login, LoginHandler },
                { ClientAction.Register, RegisterHandler },
                { ClientAction.ShowAllChats, ShowAllChatsHandler },
                { ClientAction.CreateChat, CreateChatHandler },
                { ClientAction.EnterChat, EnterChatHandler },
                { ClientAction.ShowUsersInChat, ShowUsersInChatHandler },
                { ClientAction.GoToMainMenu, GoToMainMenuHandler },
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

                while (true)
                {
                    byte[] rawMessage = tcpClient.GetMessage();

                    string messageInJson = ParseMessageToJson(rawMessage, ClientAesKey);

                    Request request = JsonSerializer.Deserialize<Request>(messageInJson);

                    Console.WriteLine($"{request.Sender} - {request.Action}");
                    RequestHandlers[request.Action].Invoke(messageInJson);
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"User left. Username: {CurrentUser?.Name}; id: {Id}");
                if (CurrentUser != null && CurrentUser.State == UserState.Authorized)
                {
                    server.UserRepository.UpdateState(CurrentUser.Name, UserState.Offline);
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

                    ClientAesKey = aesEncryption.GetKey();

                    AesKeyExchangeResponse response = new AesKeyExchangeResponse {
                        Code = StatusCode.Ok,
                        RequestId = aesKeyExchangeRequest.Id
                    };

                    SendMessageAesEncrypted(response, ClientAesKey);
                    CurrentUser.State = UserState.Connected;
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
            
            var response = new LoginResponse
            {
                RequestId = request.Id
            };

            try
            {
                User storedUser = server.UserRepository.GetByName(request.Sender);
                if (storedUser == null)
                {
                    throw new UserNotFoundException(request.Sender);
                }

                if (storedUser.Password != request.Password)
                {
                    throw new WrongPasswordException(request.Sender, request.Password, storedUser.Password);
                }

                if (storedUser.State != UserState.Offline)
                {
                    throw new UserAlreadySignedInException(request.Sender);
                }

                CurrentUser = storedUser;
                CurrentUser.State = UserState.Authorized;
                server.UserRepository.UpdateState(storedUser.Name, UserState.Authorized);
                Console.WriteLine($"{request.Sender} signed in");

                response.Code = StatusCode.Ok;
                response.UserName = storedUser.Name;
                SendMessageAesEncrypted(response, ClientAesKey);
            }
            catch (NedoGramException nedoGramException)
            {
                response.Message = nedoGramException.Message;
                response.Code = StatusCode.ClientError;

                SendMessageAesEncrypted(response, ClientAesKey);

                Console.WriteLine($"{request.Action}: {request.Sender} - {nedoGramException.Message}");
            }
            catch (Exception systemException)
            {
                response.Message = "Internal error";
                response.Code = StatusCode.ServerError;

                SendMessageAesEncrypted(response, ClientAesKey);

                Console.WriteLine($"{request.Action}: {request.Sender} - {systemException.Message}");
            }
        }

        private void RegisterHandler(string requestInJson)
        {
            RegisterRequest registerRequest = JsonSerializer.Deserialize<RegisterRequest>(requestInJson);

            var response = new RegisterResponse
            {
                RequestId = registerRequest.Id
            };

            try
            {
                User newUser = new User(registerRequest.Sender, registerRequest.Password, UserState.Authorized);
                server.UserRepository.Add(newUser);

                response.Code = StatusCode.Ok;
                response.UserName = newUser.Name;

                CurrentUser = newUser;
                Console.WriteLine($"{CurrentUser.Name} signed up successfully.");
            }
            catch (NedoGramException nedoGramException)
            {
                response.Code = StatusCode.ClientError;
                response.Message = nedoGramException.Message;

                Console.WriteLine($"{registerRequest.Action}: {registerRequest.Sender} - {nedoGramException.Message}");
            }
            catch (Exception exception)
            {
                response.Code = StatusCode.ServerError;
                response.Message = "Internal error";

                Console.WriteLine($"{registerRequest.Action}: {registerRequest.Sender} - {exception.Message}");
            }
            finally
            {
                SendMessageAesEncrypted(response, ClientAesKey);
            }
        }

        private void ShowAllChatsHandler(string requestInJson)
        {
            ShowAllChatsRequest request = JsonSerializer.Deserialize<ShowAllChatsRequest>(requestInJson);
            IReadOnlyCollection<string> chatNames = server.ChatRepository.GetChats().Select(chat => chat.Name).ToArray();

            var response = new ShowAllChatsResponse
            {
                Code = StatusCode.Ok,
                ChatNames = chatNames,
                RequestId = request.Id
            };

            SendMessageAesEncrypted(response, ClientAesKey);
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

                CurrentUser.State = UserState.InChat;
                server.UserRepository.UpdateState(creator.Name, UserState.InChat);

                server.ChatRepository.AddChat(newChat);
                creator.CurrentChat = newChat;

                var response = new CreateChatResponse
                {
                    ChatName =  newChat.Name,
                    Code = StatusCode.Ok,
                    Key = newChat.Key,
                    RequestId = request.Id,
                };

                SendMessageAesEncrypted(
                    response, 
                    ClientAesKey);

                Console.WriteLine($"Chat created. ChatName - {newChat.Name}, Creator - {creator.Name}");
            }
            // todo: system exception should not be available to the client
            catch (Exception exception)
            {
                var errorResponse = new CreateChatResponse
                {
                    Code = StatusCode.ServerError,
                    RequestId = request.Id,
                    Message = exception.Message
                };

                SendMessageAesEncrypted(errorResponse, ClientAesKey);
                throw;
            }
        }

        private void EnterChatHandler(string requestInJson)
        {
            EnterChatRequest request = JsonSerializer.Deserialize<EnterChatRequest>(requestInJson);

            var response = new EnterChatResponse
            {
                RequestId = request.Id
            };

            try
            {
                User storedUser = server.UserRepository.GetByName(request.Sender);
                IChat chat = server.ChatRepository.GetChat(request.ChatName);
                if (chat == null)
                {
                    throw new ChatNotFoundException(request.ChatName);
                }

                if (storedUser.State != UserState.Authorized)
                {
                    throw new NotEnoughRightsException(request.Sender, storedUser.State, request.Action);
                }

                chat.AddUser(storedUser);
                storedUser.CurrentChat = chat;

                storedUser.State = UserState.InChat;

                response.Key = chat.Key;
                response.Code = StatusCode.Ok;
            }
            catch (NedoGramException customException)
            {
                response.Message = customException.Message;
                response.Code = StatusCode.ClientError;
            }
            catch (Exception)
            {
                response.Message = "Internal error";
                response.Code = StatusCode.ServerError;
            }
            finally
            {
                SendMessageAesEncrypted(response, ClientAesKey);
            }
        }

        private void ShowUsersInChatHandler(string requestInJson)
        {
            ShowUsersInChatRequest request = JsonSerializer.Deserialize<ShowUsersInChatRequest>(requestInJson);

            var response = new ShowUsersInChatResponse
            {
                RequestId = request.Id
            };

            try
            {
                IChat chat = server.ChatRepository.GetChat(request.ChatName);

                if (chat == null)
                {
                    throw new ChatNotFoundException(request.ChatName);
                }

                IReadOnlyCollection<string> userNames = chat.GetUsers().Select(user => user.Name).ToArray();

                response.UserNames = userNames;
                response.Code = StatusCode.Ok;

                SendMessageAesEncrypted(response, ClientAesKey);
            }
            catch (NedoGramException nedoGramException)
            {
                response.Message = nedoGramException.Message;
                response.Code = StatusCode.ClientError;

                SendMessageAesEncrypted(response, ClientAesKey);

                Console.WriteLine($"{request.Action}: {request.Sender} - {nedoGramException.Message}");
            }
            catch (Exception systemException)
            {
                response.Message = "Internal error";
                response.Code = StatusCode.ServerError;

                SendMessageAesEncrypted(response, ClientAesKey);

                Console.WriteLine($"{request.Action}: {request.Sender} - {systemException.Message}");
            }
        }

        private void GoToMainMenuHandler(string requestInJson)
        {
            GoToMainMenuRequest request = JsonSerializer.Deserialize<GoToMainMenuRequest>(requestInJson);

            var response = new ShowUsersInChatResponse
            {
                RequestId = request.Id
            };

            try
            {
                User user = server.UserRepository.GetByName(request.Sender);

                if (user == null)
                {
                    throw new UserNotFoundException(request.Sender);
                }

                if (user.State != UserState.InChat && user.State != UserState.Authorized)
                {
                    throw new NotEnoughRightsException(request.Sender, user.State, ClientAction.GoToMainMenu);
                }

                user.CurrentChat.RemoveUser(user.Name);
                user.CurrentChat = null;
                user.State = UserState.Authorized;
                response.Code = StatusCode.Ok;

                SendMessageAesEncrypted(response, ClientAesKey);
            }
            catch (NedoGramException nedoGramException)
            {
                response.Message = nedoGramException.Message;
                response.Code = StatusCode.ClientError;

                SendMessageAesEncrypted(response, ClientAesKey);

                Console.WriteLine($"{request.Action}: {request.Sender} - {nedoGramException.Message}");
            }
            catch (Exception systemException)
            {
                response.Message = "Internal error";
                response.Code = StatusCode.ServerError;

                SendMessageAesEncrypted(response, ClientAesKey);

                Console.WriteLine($"{request.Action}: {request.Sender} - {systemException.Message}");
            }
        }

        private void MessageHandler(Message message, byte[] rawMessage)
        {
            //server.MessageSender.SendToChat(message.Headers["chat"], message);

            //server.BroadcastMessage(rawMessage, this);
        }

        private string ParseMessageToJson(byte[] rawMessage, byte[] aesKey)
        {
            aesEncryption.SetKey(aesKey);

            byte[] decryptedConnectionMessage = aesEncryption.Decrypt(rawMessage);

            return coding.Decode(decryptedConnectionMessage);
        }
    }
}
