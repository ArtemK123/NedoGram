using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    class Program
    {
        private const string Host = "127.0.0.1";
        private const int ServerPort = 8888;

        static string userName;
        static TcpClient tcpClient;
        static NetworkStream stream;
        static readonly Encoding encoding = new UnicodeEncoding(false, true, true);

        static void Main(string[] args)
        {
            Console.Write("Welcome to chat. Please, enter your name: ");
            userName = Console.ReadLine();
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(Host, ServerPort);
                stream = tcpClient.GetStream();

                string message = userName;
                byte[] data = encoding.GetBytes(message);
                stream.Write(data, 0, data.Length);

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();
                Console.WriteLine($"Hi, {userName}");
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        static void SendMessage()
        {
            Console.WriteLine("Write your message: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = encoding.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; 
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(encoding.GetString(data, 0, bytes));
                    } while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message); 
                }
                catch
                {
                    Console.WriteLine("Connection lost!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            stream?.Close();
            tcpClient?.Close();
            Environment.Exit(0);
        }
    }
};