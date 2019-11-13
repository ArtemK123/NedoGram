using System;
using System.Text.Json;
using ChatCommon;
using ChatCommon.Extensibility;

namespace ChatServer
{
    public class ClientInstance
    {
        protected internal string Id { get; }

        public string UserName { get; private set; } = "Undefined UserName";

        public readonly ICoding Coding;

        private readonly ITcpWrapper tcpClient;

        private readonly ServerInstance server; 

        public ClientInstance(ITcpWrapper tcpClient, ServerInstance serverInstance, ICoding coding)
        {
            Id = Guid.NewGuid().ToString();
            this.tcpClient = tcpClient;
            server = serverInstance;
            Coding = coding;
        }
        public void Process()
        {
            try
            {
                // send to the client server public key [and other credentials]

                tcpClient.Send(server.rsa.ExportRSAPublicKey());

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
            return false;
        }

        internal bool RegisterHandler(Message message)
        {
            return false;
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
            byte[] decryptedConnectionMessage = server.rsa.Decrypt(rawMessage, false);

            string connectionMessageInJson = Coding.Decode(decryptedConnectionMessage);

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
