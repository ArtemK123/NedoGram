using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ChatCommon;
using ChatServer.Domain;
using ChatServer.Extensibility;

namespace ChatServer
{
    public class ServerInstance
    {
        private static TcpListener tcpListener;
        internal readonly List<ClientInstance> clients = new List<ClientInstance>();
        internal RSACryptoServiceProvider rsa;

        internal Encoding Encoding { get; } = new UnicodeEncoding(false, true, true);

        internal readonly IUserRepository userRepository = new UserRepository();
        internal readonly IChatRepository chatRepository = new ChatRepository();
        internal readonly IMessageSenderService messageSender;

        internal ServerInstance()
        {
            messageSender = new MessageHandler(this);
        }

        public void Listen(int port)
        {
            try
            {
                rsa = new RSACryptoServiceProvider(4096);

                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                Console.WriteLine($"Server running on the port {port}. Waiting for new clients...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    var tcpClientWrapper = new TcpClientWrapper(tcpClient);

                    ClientInstance clientInstance = new ClientInstance(
                        tcpClientWrapper,
                        this,
                        new Coding(new UnicodeEncoding(false, false, true)));

                    clients.Add(clientInstance);
                    Thread clientThread = new Thread(clientInstance.Process);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void RemoveConnection(ClientInstance clientInstance)
        {
            if (clientInstance != null)
            {
                clients.Remove(clientInstance);
            }
        }

        protected internal void BroadcastMessage(byte[] message, ClientInstance sender)
        {
            try
            {
                foreach (ClientInstance clientInstance in clients.Where(client => !client.Id.Equals(sender.Id)))
                {
                    clientInstance.SendMessageBytes(message);
                }

                Console.WriteLine($"{sender.user.Name} sent message");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        protected internal void Disconnect()
        {
            tcpListener?.Stop();
            rsa?.Dispose();

            foreach (ClientInstance client in clients)
            {
                client.Close();
            }

            Environment.Exit(0);
        }
    }
}