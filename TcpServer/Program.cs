using System;
using System.Threading;

namespace ChatServer
{
    class ServerProgram
    {
        class Program
        {
            static ServerInstance server; // сервер
            static Thread listenThread; // потока для прослушивания

            static void Main(string[] args)
            {
                try
                {
                    server = new ServerInstance();
                    listenThread = new Thread(server.Listen);
                    listenThread.Start(); //старт потока
                }
                catch (Exception ex)
                {
                    server.Disconnect();
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
};