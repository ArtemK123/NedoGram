using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ConsoleChatTestServer
{
    class Program
    {
        const int port = 8888; // порт для прослушивания подключений
        static void Main(string[] args)
        {
            TcpListener server = null;
            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);

                // запуск слушателя
                server.Start();

                while (true)
                {
                    Console.WriteLine("Ожидание подключений... ");

                    // получаем входящее подключение
                    client = server.AcceptTcpClient();
                    Console.WriteLine("Подключен клиент. Выполнение запроса...");

                    // получаем сетевой поток для чтения и записи
                    stream = client.GetStream();

                    // сообщение для отправки клиенту
                    string response = "Successfully connected to the TCP server";
                    // преобразуем сообщение в массив байтов
                    byte[] data = Encoding.UTF8.GetBytes(response);

                    // отправка сообщения
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("Sent response: {0}", response);

                    // закрываем поток
                    stream?.Close();
                    // закрываем подключение
                    client?.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server?.Stop();
            }
        }
    }
}
