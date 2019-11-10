using System;
using System.Threading;
using ChatCommon;
using ChatCommon.Extensibility;

namespace ConsoleChatClient
{
    internal class ChatClient : IChatClient, IDisposable
    {
        public readonly string UserName;

        private readonly IEncryption encryption;
        private readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;

        public ChatClient(ITcpWrapper tcpClient, string userName, IEncryption encryption, ICoding coding)
        {
            this.tcpClient = tcpClient;
            UserName = userName;
            this.encryption = encryption;
            this.coding = coding;
        }

        public void Listen()
        {
            try
            {
                string message = UserName;
                byte[] data = coding.Encode(UserName);
                tcpClient.Send(data);

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();
                Console.WriteLine($"Hi, {UserName}");
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            tcpClient?.Dispose();
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

                tcpClient.Send(encryptedData);
            }
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] rawMessage = tcpClient.GetMessage();

                    string message = encryption.Decrypt(rawMessage);

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
