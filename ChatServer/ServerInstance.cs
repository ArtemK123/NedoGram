﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ChatCommon;
using ChatServer.Domain;
using ChatServer.Domain.Repositories;
using ChatServer.Extensibility;

namespace ChatServer
{
    public class ServerInstance
    {
        private static TcpListener tcpListener;
        internal readonly List<ClientInstance> clients = new List<ClientInstance>();
        internal RSACryptoServiceProvider rsa;


        internal readonly IUserRepository UserRepository = new UserRepository();
        internal readonly IChatRepository ChatRepository = new ChatRepository();
        internal readonly INotificationSender NotificationSender;

        internal ServerInstance()
        {
            NotificationSender = new NotificationSender(this);

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

        protected internal void Disconnect()
        {
            tcpListener?.Stop();
            rsa?.Dispose();

            foreach (ClientInstance client in clients)
            {
                client.Dispose();
            }

            Environment.Exit(0);
        }
    }
}