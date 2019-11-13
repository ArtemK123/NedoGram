using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using ChatCommon;
using ChatCommon.Extensibility;
using ConsoleChatClient.Domain;
using ConsoleChatClient.Domain.Actions;

namespace ConsoleChatClient
{
    internal class ChatClient : IChatClient, IDisposable
    {
        public string UserName = "Undefined username";

        private readonly IEncryption encryption;
        private readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;
        private readonly RSACryptoServiceProvider rsa;
        private byte[] serverPublicKey;
        private byte[] myPublicKey;
        private byte[] myPrivateKey;

        public ChatClient(
            ITcpWrapper tcpClient,
            IEncryption encryption,
            ICoding coding)
        {
            this.tcpClient = tcpClient;
            this.encryption = encryption;
            this.coding = coding;
            rsa = new RSACryptoServiceProvider(2048);
            myPublicKey = rsa.ExportRSAPublicKey();
            myPrivateKey = rsa.ExportRSAPrivateKey();
        }

        public void Listen()
        {
            try
            {
                // read server credentials

                ReadServerPublicKey();

                // try to login until successful

                bool successfulLogin = false;

                do
                {
                    LoginMenuAction action = GetLoginAction();

                    Dictionary<LoginMenuAction, Func<bool>> loginHandlers = new Dictionary<LoginMenuAction, Func<bool>>();
                    loginHandlers.Add(LoginMenuAction.Login, this.Login);
                    loginHandlers.Add(LoginMenuAction.Register, this.Register);
                    loginHandlers.Add(LoginMenuAction.Exit, this.Exit);

                    successfulLogin = loginHandlers[action]();
                } while (!successfulLogin);

                // Open main menu and message receiving in different threads
                
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();


                // Get menu action

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

        //private void ConnectToServer()
        //{
        //    LoginMenuAction action = GetLoginAction();

        //    Dictionary<LoginMenuAction, Func<bool>> loginHandlers = new Dictionary<LoginMenuAction, Func<bool>>();
        //    loginHandlers.Add(LoginMenuAction.Login, this.Login);
        //    loginHandlers.Add(LoginMenuAction.Register, this.Register);
        //    loginHandlers.Add(LoginMenuAction.Exit, this.Exit);

        //    loginHandlers[action]();
        //}

        private void ReadServerPublicKey()
        {
            byte[] serverPublicKey = tcpClient.GetMessage();
            if (ImportPublicKey(serverPublicKey) > 0)
            {
                this.serverPublicKey = serverPublicKey;
            }
        }

        private int ImportPublicKey(byte[] key)
        {
            int bytesParsed;
            rsa.ImportRSAPublicKey(key, out bytesParsed);
            return bytesParsed;
        }

        private LoginMenuAction GetLoginAction()
        {
            Console.WriteLine(ConstantsProvider.LoginMenuItems);
            while (true)
            {
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                    {
                        return LoginMenuAction.Login;
                    }
                    case "2":
                    {
                        return LoginMenuAction.Register;
                    }
                    case "0":
                    {
                        return LoginMenuAction.Exit;
                    }
                }
            }
        }

        private bool Login()
        {
            Console.WriteLine("Write your username");
            UserName = Console.ReadLine();

            Message connectMessage = new Message(new Dictionary<string, string>(), "");
            connectMessage.Headers.Add("action", "login");
            connectMessage.Headers.Add("content-type", "empty");
            connectMessage.Headers.Add("user", UserName);
            //var md5 = new MD5CryptoServiceProvider();
            //string hash = Convert.ToBase64String(md5.ComputeHash(coding.Encode("silly password")));
            //connectMessage.Headers.Add("password", hash);
            //md5.Dispose();

            byte[] messageBytes = coding.Encode(JsonSerializer.Serialize(connectMessage));

            byte[] data = rsa.Encrypt(messageBytes, false);

            tcpClient.Send(data);

            return true;
        }

        private bool Register()
        {
            return true;
        }

        private bool Exit()
        {
            Dispose();
            return true;
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
