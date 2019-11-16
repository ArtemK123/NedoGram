using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using ChatCommon;
using ChatCommon.Encryption;
using ChatCommon.Extensibility;
using ChatServer.Domain;
using ChatServer.Extensibility;

namespace ChatServer
{
    public class ClientInstance
    {
        protected internal Guid Id { get; }

        internal User user;

        internal readonly ICoding coding;

        private readonly ITcpWrapper tcpClient;

        private readonly ServerInstance server;

        private readonly AesEncryption aesEncryption;

        private byte[] clientAesKey;

        public ClientInstance(ITcpWrapper tcpClient, ServerInstance serverInstance, ICoding coding)
        {
            Id = Guid.NewGuid();
            server = serverInstance;
            this.tcpClient = tcpClient;
            this.coding = coding;
            aesEncryption = new AesEncryption();
            user = new User();
        }
        public void Process()
        {
            try
            {
                Console.WriteLine($"New connection - {Id}");

                bool successfulExchange = false;

                do
                {
                    Console.WriteLine($"Key exchange");
                    successfulExchange = KeyExchange();
                } while (!successfulExchange);

                Console.WriteLine(
                    $"Key exchanged successfully. Connection configured. Id-{Id}; Key-{Convert.ToBase64String(aesEncryption.GetKey())}");
                user.State = UserState.Connected;

                // handle requests from client

                while (true)
                {
                    byte[] rawMessage = tcpClient.GetMessage();

                    Message message = ParseMessage(rawMessage);

                    Console.WriteLine(JsonSerializer.Serialize(message));

                    switch (message.Headers["action"].ToLower())
                    {
                        case "login":
                        {
                            LoginHandler(message);
                            break;
                        }
                        case "register":
                        {
                            RegisterHandler(message);
                            break;
                        }
                        case "getChats":
                        {
                            GetAllChatsHandler();
                            break;
                        }
                        case "enterChat":
                        {
                            EnterChatHandler(message);
                            break;
                        }
                        case "exitChat":
                        {
                            ExitChatHandler(message);
                            break;
                        }
                        case "message":
                        {
                            SendMessageHandler(message, rawMessage);
                            break;
                        }
                        default:
                        {
                            InvalidActionHandler(message);
                            break;
                        }
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"User left. Username: {user.Name}; id: {Id}");
                if (user.State == UserState.Authorized)
                {
                    server.userRepository.UpdateState(user.Name, UserState.Unknown);
                }
                server.RemoveConnection(this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        internal bool LoginHandler(Message message)
        {
            // todo: should be refactored

            bool successful = false;
            User storedUser = null;
            try
            {
                storedUser = server.userRepository.GetByName(message.Headers["user"]);
                user = storedUser;

                successful = user.Password == message.Headers["password"];
            }
            catch (Exception)
            {
                successful = false;
            }

            Message response = new Message();

            if (successful)
            {
                response.Headers.Add("code", "200");
                server.userRepository.UpdateState(user.Name, UserState.Authorized);
            }
            else
            {
                response.Headers.Add("code", "403");
            }

            SendMessageWithServerAesEncryption(response);

            return false;
        }

        internal void GetAllChatsHandler()
        {
            Message response = new Message();
            response.Headers.Add("action", "provideAllChats");
            response.Headers.Add("sender", "server");
            response.Headers.Add("content-type", "json");
            response.Headers.Add("encryption", "aes");

            IReadOnlyCollection<IChat> chats = server.chatRepository.GetChats();

            response.Body = coding.GetBytes(JsonSerializer.Serialize(chats.Select(chat => chat.Name)));
        }

        internal void EnterChatHandler(Message message)
        {
            IChat chat = server.chatRepository.GetChat(message.Headers["chatName"]);
            if (chat == null)
            {
                SendErrorResponse();
                return;
            }
            chat.AddUser(user);
            user.CurrentChat = chat;
            server.userRepository.UpdateState(user.Name, UserState.InChat);
            SendSuccessResponse();
        }

        internal void ExitChatHandler(Message message)
        {
            user.CurrentChat.RemoveUser(user.Name);
            user.CurrentChat = null;
            server.userRepository.UpdateState(user.Name, UserState.Authorized);
            SendSuccessResponse();
        }

        internal bool RegisterHandler(Message message)
        {
            return true;
        }

        internal bool SendMessageHandler(Message message, byte[] rawMessage)
        {
            server.BroadcastMessage(rawMessage, this);
            return true;
        }

        internal bool InvalidActionHandler(Message message)
        {
            Message response = new Message();
            response.Headers.Add("code", "400");
            response.Headers.Add("reason", "Unsupported action");
            SendMessageWithServerAesEncryption(response);

            return false;
        }

        internal void SendSuccessResponse()
        {
            Message response = new Message();
            response.Headers.Add("code", "200");
            response.Headers.Add("result", "successful");
            response.Headers.Add("sender", "server");
            SendMessageWithServerAesEncryption(response);
        }

        internal void SendErrorResponse()
        {
            Message response = new Message();
            response.Headers.Add("code", "500");
            response.Headers.Add("reason", "default error from server");
            response.Headers.Add("result", "unsuccessful");
            response.Headers.Add("sender", "server");
            SendMessageWithServerAesEncryption(response);
        }

        internal Message ParseMessage(byte[] rawMessage)
        {
            byte[] decryptedConnectionMessage = aesEncryption.Decrypt(rawMessage);

            string connectionMessageInJson = coding.Decode(decryptedConnectionMessage);

            return JsonSerializer.Deserialize<Message>(connectionMessageInJson);
        }

        internal void SendMessage(byte[] message)
        {
            tcpClient.Send(message);
        }

        private void SendMessageWithServerAesEncryption(Message message)
        {
            aesEncryption.SetKey(clientAesKey);
            tcpClient.Send(aesEncryption.Encrypt(coding.GetBytes(JsonSerializer.Serialize(message))));
        }

        private bool KeyExchange()
        {
            // send to the client server public key [and other credentials]

            tcpClient.Send(server.rsa.ExportRSAPublicKey());

            // get key for symmetric encryption from client

            byte[] encryptedMessageWithKey = tcpClient.GetMessage();

            try
            {
                byte[] messageWithKeyBytes = server.rsa.Decrypt(encryptedMessageWithKey, false);
                string messageWithKeyInJson = coding.Decode(messageWithKeyBytes);
                Message messageWithKey = JsonSerializer.Deserialize<Message>(messageWithKeyInJson);

                aesEncryption.SetKey(messageWithKey.Body);
                aesEncryption.SetIv(Convert.FromBase64String(messageWithKey.Headers["iv"]));

                clientAesKey = messageWithKey.Body;
            }
            catch (Exception)
            {
                // todo: return meaningful error descriptions

                return false;
            }

            return true;
        }

        protected internal void Close()
        {
            tcpClient?.Dispose();
        }
    }
}
