using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ChatCommon;

namespace ChatServer
{
    public class ServerInstance
    {
        private static TcpListener tcpListener;
        private readonly List<ClientInstance> clients = new List<ClientInstance>();
        public Encoding Encoding { get; } = new UnicodeEncoding(false, true, true);

        public void Listen(int port)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                Console.WriteLine($"Server running on the port {port}. Waiting for new clients...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    
                    Console.WriteLine("New client connected");

                    ClientInstance clientInstance = new ClientInstance(
                        new TcpClientWrapper(tcpClient),
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
                    clientInstance.SendMessage(message);
                }
                Console.WriteLine($"{sender.UserName} sent message");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop();

            foreach (ClientInstance client in clients)
            {
                client.Close();
            }
            Environment.Exit(0);
        }
    }
}
