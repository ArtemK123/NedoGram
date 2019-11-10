using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using ChatCommon;

namespace ChatServer
{
    public class ClientInstance
    {
        protected internal string Id { get; }
        protected internal NetworkStream Stream { get; private set; }
        public string UserName { get; private set; } = "Undefined UserName";

        public readonly ICoding Coding;

        readonly TcpClient client;
        readonly ServerInstance server; 
        public ClientInstance(TcpClient tcpClient, ServerInstance serverInstance, ICoding coding)
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
                Stream = client.GetStream();
                byte[] message = GetMessage();
                UserName = Coding.Decode(message);

                Console.WriteLine(UserName + " connected");

                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        server.BroadcastMessage(message, this);
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

        public void SendMessage(byte[] messageBuffer)
        {
            Stream.Write(messageBuffer, 0 , messageBuffer.Length);
        }

        private byte[] GetMessage()
        {
            byte[] buffer = new byte[64];
            List<byte> message = new List<byte>();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(buffer, 0, buffer.Length);
                message.AddRange(buffer);
            } while (Stream.DataAvailable);

            return message.Where((byte b) => b != 0).ToArray();
        }

        protected internal void Close()
        {
            Stream?.Close();
            client?.Close();
        }
    }
}
