using System;
using System.IO;
using System.Text.Json;
using ChatCommon;
using ChatCommon.Encryption;
using ChatCommon.Extensibility;
using ChatServer.Domain;

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
            User user = null;
            try
            {
                user = server.userRepository.GetByName(message.Headers["user"]);

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
            }
            else
            {
                response.Headers.Add("code", "403");
            }

            SendMessageWithAesEncryption(response);

            return false;
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
            return false;
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

        private void SendMessageWithAesEncryption(Message message)
        {
            tcpClient.Send(aesEncryption.Encrypt(coding.Encode(JsonSerializer.Serialize(message))));
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
