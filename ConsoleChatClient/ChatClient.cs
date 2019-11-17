using System;
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

        private readonly AesEncryption aesEncryption;
        private readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;
        private readonly RSACryptoServiceProvider rsa;
        private byte[] serverKey;
        private byte[] chatKey;

        public ChatClient(
            ITcpWrapper tcpClient,
            AesEncryption aesEncryption,
            ICoding coding)
        {
            this.tcpClient = tcpClient;
            this.aesEncryption = aesEncryption;
            this.coding = coding;
            rsa = new RSACryptoServiceProvider(4096);
        }

        public void Listen()
        {
            try
            { 
                KeyExchange();

                // try to login until successful

                bool successfulAction = false;

                do
                {
                    Console.WriteLine(ConstantsProvider.WelcomeMessage);

                    MenuAction loginAction = GetLoginAction();

                    Dictionary<MenuAction, Func<bool>> loginHandlers = new Dictionary<MenuAction, Func<bool>>();
                    loginHandlers.Add(MenuAction.Login, this.LoginHandler);
                    loginHandlers.Add(MenuAction.Register, this.RegisterHandler);
                    loginHandlers.Add(MenuAction.Exit, this.ExitHandler);

                    successfulAction = loginHandlers[loginAction]();

                    string successMessage = successfulAction ? "successful" : "unsuccessful";
                    Console.WriteLine($"{loginAction.ToString()}: {successMessage}");
                } while (!successfulAction);

                // Open main menu and message receiving in different threads

                bool isExit = false;
                while (!isExit)
                {
                    Console.WriteLine(Environment.NewLine + ConstantsProvider.MainMenuTitle + Environment.NewLine + ConstantsProvider.MainMenu);
                    string actionNumber = Console.ReadLine();

                    switch (actionNumber)
                    {
                        case "1":
                            {
                                ShowAllChatsHandler();
                                break;
                            }
                        case "2":
                            {
                                CreateChatHandler();
                                break;
                            }
                        case "3":
                            {
                                EnterChatHandler();
                                break;
                            }
                        case "0":
                            {
                                ExitHandler();
                                isExit = true;
                                break;
                            }
                        default:
                            {
                                Console.WriteLine($"Unsopported action number - {actionNumber}");
                                break;
                            }
                    }
                }
                



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

        private void ShowAllChatsHandler()
        {
            Console.WriteLine("ShowAllChatsHandler");
        }
        private void CreateChatHandler()
        {
            Console.WriteLine("CreateChatHandler");
        }
        private void EnterChatHandler()
        {
            Console.WriteLine("EnterChatHandler");
        }

        private void KeyExchange()
        {
            // read server credentials
            ReadServerPublicKey();

            // send aes key and iv to the server
            Message messageWithKey = new Message();

            messageWithKey.Headers.Add("action", "connect");
            messageWithKey.Headers.Add("content-type", "bytes/key");
            messageWithKey.Headers.Add("algorithm", "aes");
            messageWithKey.Headers.Add("iv", Convert.ToBase64String(aesEncryption.GetIv()));

            serverKey = aesEncryption.GetKey();

            messageWithKey.Body = serverKey;

            tcpClient.Send(rsa.Encrypt(coding.GetBytes(JsonSerializer.Serialize(messageWithKey)), false));
        }

        private void SendMessageAesEncrypted(Message message, byte[] key)
        {
            aesEncryption.SetKey(key);
            tcpClient.Send(aesEncryption.Encrypt(coding.GetBytes(JsonSerializer.Serialize(message))));
        }

        private bool LoginHandler()
        {
            Console.WriteLine("Write your username");
            //UserName = Console.ReadLine();
            UserName = "test";
            Console.WriteLine($"Test name - {UserName}");

            Console.WriteLine(Environment.NewLine + "Write your password");

            //string password = Console.ReadLine();
            string password = "test";
            Console.WriteLine($"Test password - {password}");

            Message connectMessage = new Message(new Dictionary<string, string>(), new byte[0]);
            connectMessage.Headers.Add("action", "login");
            connectMessage.Headers.Add("user", UserName);
            connectMessage.Headers.Add("password", GetPasswordHash(password));

            string messageInJson = JsonSerializer.Serialize(connectMessage);

            byte[] messageBytes = coding.GetBytes(messageInJson);

            byte[] encryptedBytes = aesEncryption.Encrypt(messageBytes);
            
            byte[] decrypted = aesEncryption.Decrypt(encryptedBytes);

            string decryptedInJson = coding.Decode(decrypted);

            Message decryptedMessage = JsonSerializer.Deserialize<Message>(decryptedInJson);

            Console.WriteLine($"Encrypted and decrypted: {coding.Decode(aesEncryption.Decrypt(encryptedBytes))}");

            tcpClient.Send(encryptedBytes);

            byte[] rawResponse = tcpClient.GetMessage();

            aesEncryption.SetKey(serverKey);
            Message response = ParseMessage(rawResponse);

            Console.WriteLine(JsonSerializer.Serialize(response));

            return response.Headers.ContainsKey("code") && response.Headers["code"] == "200";
        }

        private bool RegisterHandler()
        {
            return true;
        }

        private bool ExitHandler()
        {
            Dispose();
            return true;
        }

        private void ReadServerPublicKey()
        {
            byte[] key = tcpClient.GetMessage();
            int bytesParsed;
            rsa.ImportRSAPublicKey(key, out bytesParsed);
        }

        private MenuAction GetLoginAction()
        {
            while (true)
            {
                Console.WriteLine(ConstantsProvider.LoginMenuItems + Environment.NewLine);

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                    {
                        return MenuAction.Login;
                    }
                    case "2":
                    {
                        return MenuAction.Register;
                    }
                    case "0":
                    {
                        return MenuAction.Exit;
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
                return Convert.ToBase64String(md5.ComputeHash(coding.GetBytes(password)));
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

                Message messageObj = new Message(headers, coding.GetBytes(input));

                string messageInJson = JsonSerializer.Serialize(messageObj);

                byte[] encryptedData = aesEncryption.Encrypt(coding.GetBytes(messageInJson));
                Console.WriteLine($"Encrypted and derypted: {aesEncryption.Decrypt(encryptedData)}");

                tcpClient.Send(encryptedData);
            }
        }

        private void ReceiveMessage()
        {
            // todo: should be implemented in differnet way

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
