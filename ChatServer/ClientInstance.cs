using System;
using System.Text;
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

        private readonly ITcpWrapper client;
        private readonly ServerInstance server; 
        public ClientInstance(ITcpWrapper tcpClient, ServerInstance serverInstance, ICoding coding)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverInstance;
            Coding = coding;
        }
        public void Process()
        {
            try
            {
                byte[] rawConnectionMessage = client.GetMessage();

                byte[] decryptedConnectionMessage =  server.rsa.Decrypt(rawConnectionMessage, false);

                string connectionMessageInJson = Coding.Decode(decryptedConnectionMessage);

                Message connectonMessage = JsonSerializer.Deserialize<Message>(connectionMessageInJson);

                UserName = connectonMessage.Headers["user"];

                Console.WriteLine($"Received message: {connectionMessageInJson}");

                while (true)
                {
                    try
                    {
                        byte[] rawMessage = client.GetMessage();
                        server.BroadcastMessage(rawMessage, this);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"{UserName}: left chat");
                        throw e;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                server.RemoveConnection(this);
                Close();
            }
        }

        public void SendMessage(byte[] message)
        {
            client.Send(message);
        }

        protected internal void Close()
        {
            client?.Dispose();
        }
    }
}
