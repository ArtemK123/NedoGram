using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
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
        private readonly RSACryptoServiceProvider rsa;

        public ChatClient(
            ITcpWrapper tcpClient,
            string userName,
            IEncryption encryption,
            ICoding coding)
        {
            this.tcpClient = tcpClient;
            UserName = userName;
            this.encryption = encryption;
            this.coding = coding;
            rsa = new RSACryptoServiceProvider();
        }

        public void Listen()
        {
            try
            {
                byte[] serverPublicKey = tcpClient.GetMessage();

                int temp;
                rsa.ImportRSAPublicKey(serverPublicKey, out temp);

                byte[] data = rsa.Encrypt(coding.Encode(UserName), true);

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
            rsa?.Dispose();
            Environment.Exit(0);
        }

        private void SendMessage()
        {
            Console.WriteLine("Write your message: ");

            while (true)
            {
                string input = Console.ReadLine();

                var headers = new Dictionary<string, string>();
                headers.Add("action", "connect");
                headers.Add("content-type", "json/aes");
                headers.Add("sender", UserName);

                Message messageObj = new Message(headers, input);

                string messageInJson = JsonSerializer.Serialize(messageObj);

                byte[] encryptedData = encryption.Encrypt(messageInJson);
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

                    string messageInJson = encryption.Decrypt(rawMessage);

                    Message message = JsonSerializer.Deserialize<Message>(messageInJson);

                    Console.WriteLine($"{message.Headers["sender"]}: {message.Body}");
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
