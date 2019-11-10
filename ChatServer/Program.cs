using System;
using System.Threading;

namespace ChatServer
{
    class ServerProgram
    {
        class Program
        {
            static ServerInstance server; 
            static Thread listenThread;

            static void Main(string[] args)
            {
                try
                {
                    server = new ServerInstance();
                    listenThread = new Thread(() => server.Listen(8888));
                    listenThread.Start();
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