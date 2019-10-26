using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    class Program
    {
        private const string host = "127.0.0.1";
        static string userName;
        private static int port = FindAvailablePort(9000);
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            //Console.Write("Введите свое имя: ");
            //userName = Console.ReadLine();
            userName = "Artem";
            client = new TcpClient();
            try
            {
                client.Connect(host, 8888); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(); //старт потока
                Console.WriteLine("Добро пожаловать, {0}", userName);
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

        private static int FindAvailablePort(int fromPort)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            int port = fromPort;
            while (IsPortAlreadyUsed(tcpConnInfoArray, port))
            {
                port++;
            }

            return port;
        }

        private static bool IsPortAlreadyUsed(TcpConnectionInformation[] tcpConnInfoArray, int port)
        {
            return tcpConnInfoArray.FirstOrDefault(tcpConnectionInfo => tcpConnectionInfo.LocalEndPoint.Port == port) != null;
        }

        // отправка сообщений
        static void SendMessage()
        {
            Console.WriteLine("Введите сообщение: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
        }

        // получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message); //вывод сообщения
                }
                catch
                {
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close(); //отключение потока
            if (client != null)
                client.Close(); //отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }
};