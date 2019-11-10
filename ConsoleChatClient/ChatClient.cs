using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ChatCommon;
using ChatCommon.Extensibility;

namespace ConsoleChatClient
{
    internal class ChatClient : IChatClient, IDisposable
    {
        private const int bufferSize = 64;
        public readonly string ServerIp;
        public readonly int ServerPort;
        public readonly string UserName;

        private readonly TcpClient tcpClient;
        private readonly IEncryption encryption;
        private readonly ICoding coding;

        private NetworkStream stream = null;

        public ChatClient(string serverIp, int serverPort, string userName, IEncryption encryption, ICoding coding)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            UserName = userName;
            this.encryption = encryption;
            this.coding = coding;
            tcpClient = new TcpClient();
        }

        public void Listen()
        {
            try
            {
                tcpClient.Connect(ServerIp, ServerPort);
                stream = tcpClient.GetStream();

                string message = UserName;
                byte[] data = coding.Encode(UserName);
                stream.Write(data, 0, data.Length);

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();
                Console.WriteLine($"Hi, {UserName}");
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Dispose();
            }
        }
        public void Dispose()
        {
            stream?.Close();
            tcpClient?.Close();
            Environment.Exit(0);
        }

        private void SendMessage()
        {
            Console.WriteLine("Write your message: ");

            while (true)
            {
                string input = Console.ReadLine();

                string message = $"{UserName}: {input}";

                byte[] encryptedData = encryption.Encrypt(message);
                Console.WriteLine($"Encrypted and derypted: {encryption.Decrypt(encryptedData)}");

                stream.Write(encryptedData, 0, encryptedData.Length);
            }
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[bufferSize];
                    List<byte> encryptedMessage = new List<byte>();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(buffer, 0, buffer.Length);
                        encryptedMessage.AddRange(buffer);
                    } while (stream.DataAvailable);

                    string message = encryption.Decrypt(encryptedMessage.Where((byte b) => b != 0).ToArray());

                    Console.WriteLine(message.Trim());
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    Console.WriteLine("Connection lost!");
                    Console.ReadLine();
                    this.Dispose();
                }
            }
        }
    }
}
