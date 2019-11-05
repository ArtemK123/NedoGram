using System;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public class ClientInstance
    {
        protected internal string Id { get; }
        protected internal NetworkStream Stream { get; private set; }
        public string UserName { get; private set; } = "Undefined UserName";

        readonly TcpClient client;
        readonly ServerInstance server; 
        public ClientInstance(TcpClient tcpClient, ServerInstance serverInstance)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverInstance;
        }
        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                string message = GetMessage();
                UserName = message;

                message = UserName + " connected";
                server.BroadcastMessage(message, this);
                Console.WriteLine(message);

                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = $"{UserName}: {message}";
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this);
                    }
                    catch(Exception e)
                    {
                        //message = $"{UserName}: left chat";
                        //Console.WriteLine(message);
                        //server.BroadcastMessage(message, this);
                        throw e;
                    }
                }
            }
            catch (DecoderFallbackException)
            {
                Console.WriteLine("Error while decoding message");
            }
            catch (EncoderFallbackException )
            {
                Console.WriteLine("Error while encoding message");
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
            this.Stream.Write(messageBuffer, 0 , messageBuffer.Length);
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();

            do
            {
                int bytes = Stream.Read(data, 0, data.Length);
                builder.Append(server.Encoding.GetString(data, 0, bytes));
            } while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()
        {
            Stream?.Close();
            client?.Close();
        }
    }
}
