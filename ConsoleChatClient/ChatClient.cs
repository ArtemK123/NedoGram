﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using ChatCommon;
using ChatCommon.Encryption;
using ChatCommon.Extensibility;
using ConsoleChatClient.Domain;
using ConsoleChatClient.Domain.Actions;

namespace ConsoleChatClient
{
    internal class ChatClient : IChatClient, IDisposable
    {
        public string UserName = "Undefined username";

        private readonly IEncryption aesEncryption;
        private readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;
        private readonly RSACryptoServiceProvider rsa;
        private byte[] serverRsaPublicKey;
        private byte[] myRsaPublicKey;
        private byte[] aesServerKey;

        public ChatClient(
            ITcpWrapper tcpClient,
            IEncryption aesEncryption,
            ICoding coding)
        {
            this.tcpClient = tcpClient;
            this.aesEncryption = aesEncryption;
            this.coding = coding;
            rsa = new RSACryptoServiceProvider(4096);
            myRsaPublicKey = rsa.ExportRSAPublicKey();
        }

        public void Listen()
        {
            try
            {
                // read server credentials

                ReadServerPublicKey();

                Message messageWithKey = new Message();

                messageWithKey.Headers.Add("action", "connect");
                messageWithKey.Headers.Add("content-type", "bytes/key");
                messageWithKey.Headers.Add("algorithm", "aes");

                aesServerKey = aesEncryption.GetKey();

                messageWithKey.Body = aesServerKey;

                tcpClient.Send(rsa.Encrypt(coding.Encode(JsonSerializer.Serialize(messageWithKey)), false));

                // try to login until successful

                bool successfulAction = false;

                do
                {
                    LoginMenuAction action = GetLoginAction();

                    Dictionary<LoginMenuAction, Func<bool>> loginHandlers = new Dictionary<LoginMenuAction, Func<bool>>();
                    loginHandlers.Add(LoginMenuAction.Login, this.Login);
                    loginHandlers.Add(LoginMenuAction.Register, this.Register);
                    loginHandlers.Add(LoginMenuAction.Exit, this.Exit);

                    successfulAction = loginHandlers[action]();
                } while (!successfulAction);

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

        private bool Login()
        {
            Console.WriteLine("Write your username");
            UserName = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Write your password");

            string password = Console.ReadLine();

            Message connectMessage = new Message(new Dictionary<string, string>(), new byte[0]);
            connectMessage.Headers.Add("action", "login");
            connectMessage.Headers.Add("user", UserName);
            connectMessage.Headers.Add("password", GetPasswordHash(password));

            string messageInJson = JsonSerializer.Serialize(connectMessage);

            byte[] messageBytes = coding.Encode(messageInJson);

            byte[] encryptedMessage = aesEncryption.Encrypt(messageBytes);

            byte[] decryptedMessageBytes = aesEncryption.Decrypt(encryptedMessage);

            string decryptedMessageInJson = coding.Decode(decryptedMessageBytes);

            Console.WriteLine($"Encrypted and decrypted: {decryptedMessageInJson}");

            return false;

            //tcpClient.Send(data);

            //byte[] rawResponse = tcpClient.GetMessage();

            //aesEncryption.SetKey(aesServerKey);
            //Message response = ParseMessage(rawResponse);

            //Console.WriteLine(JsonSerializer.Serialize(response));

            //return response.Headers.ContainsKey("code") && response.Headers["code"] == "200";
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

        private void ReadServerPublicKey()
        {
            byte[] serverPublicKey = tcpClient.GetMessage();
            if (ImportPublicKey(serverPublicKey) > 0)
            {
                this.serverRsaPublicKey = serverPublicKey;
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
            while (true)
            {
                Console.WriteLine(ConstantsProvider.LoginMenuItems + Environment.NewLine);

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

        public void Dispose()
        {
            tcpClient?.Dispose();
            rsa?.Dispose();
            Environment.Exit(0);
        }



        private Message ParseMessage(byte[] rawMessage)
        {
            byte[] decryptedConnectionMessage = aesEncryption.Decrypt(rawMessage);

            string connectionMessageInJson = coding.Decode(decryptedConnectionMessage);

            return JsonSerializer.Deserialize<Message>(connectionMessageInJson);
        }

        private string GetPasswordHash(string password)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                return Convert.ToBase64String(md5.ComputeHash(coding.Encode(password)));
            }
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

                Message messageObj = new Message(headers, coding.Encode(input));

                string messageInJson = JsonSerializer.Serialize(messageObj);

                byte[] encryptedData = aesEncryption.Encrypt(coding.Encode(messageInJson));
                Console.WriteLine($"Encrypted and derypted: {aesEncryption.Decrypt(encryptedData)}");

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

                    string messageInJson = coding.Decode(aesEncryption.Decrypt(rawMessage));

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
