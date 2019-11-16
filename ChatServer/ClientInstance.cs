using System;
using System.Text.Json;
using ChatCommon;
using ChatCommon.Encryption;
using ChatCommon.Extensibility;

namespace ChatServer
{
    public class ClientInstance
    {
        protected internal string Id { get; }

        public string UserName { get; private set; } = "Undefined UserName";

        internal readonly ICoding coding;

        private readonly ITcpWrapper tcpClient;

        private readonly ServerInstance server;

        private readonly AesEncryption aesEncryption;

        public ClientInstance(ITcpWrapper tcpClient, ServerInstance serverInstance, ICoding coding)
        {
            Id = Guid.NewGuid().ToString();
            this.tcpClient = tcpClient;
            server = serverInstance;
            this.coding = coding;
            aesEncryption = new AesEncryption();
        }
        public void Process()
        {
            try
            {
                Console.WriteLine($"New connection - {Id}");

                // send to the client server public key [and other credentials]

                tcpClient.Send(server.rsa.ExportRSAPublicKey());

                // get key for symmetric encryption from client

                byte[] rawMessageWithKey = tcpClient.GetMessage();

                string messageWithKeyInJson = coding.Decode(server.rsa.Decrypt(rawMessageWithKey, false));

                Message messageWithKey = JsonSerializer.Deserialize<Message>(messageWithKeyInJson);

                // todo: message should be validated

                byte[] iv = Convert.FromBase64String(messageWithKey.Headers["iv"]);
                aesEncryption.SetKey(messageWithKey.Body);
                aesEncryption.SetIv(iv);


                Console.WriteLine($"Connection configured. Id-{Id}; Key-{Convert.ToBase64String(messageWithKey.Body)}");

                // handle requests from client

                while (true)
                {
                    byte[] rawMessage = tcpClient.GetMessage();

                    Message message = ParseMessage(rawMessage);

                    Console.WriteLine(JsonSerializer.Serialize(message));

                    if (!message.Headers.ContainsKey("action"))
                    {
                        continue;
                    }

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

                //UserName = connectonMessage.Headers["user"];

                //Console.WriteLine($"Received message: {connectionMessageInJson}");

            }
            catch (Exception exception)
            {
                Console.WriteLine($"{UserName}: left chat");
                Console.WriteLine(exception.Message);
            }
            finally
            {
                server.RemoveConnection(this);
                Close();
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
                user.PublicKey = message.Body;
                server.userRepository.Update(user);

                response.Headers.Add("code", "200");
            }
            else
            {
                response.Headers.Add("code", "403");
            }

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

        protected internal void Close()
        {
            tcpClient?.Dispose();
        }
    }
}
