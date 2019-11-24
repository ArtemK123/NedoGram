﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using ChatCommon;
using ChatCommon.Actions;
using ChatCommon.Encryption;
using ChatCommon.Exceptions;
using ChatCommon.Extensibility;
using ChatCommon.Messages;
using ChatCommon.Messages.Requests;
using ChatCommon.Messages.Responses;
using ConsoleChatClient.Domain;

namespace ConsoleChatClient
{
    internal class ChatClient : IChatClient
    {
        public string UserName = "Undefined username";

        private readonly AesEncryption aesEncryption;
        private readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;
        private readonly RSACryptoServiceProvider rsa;
        private byte[] serverKey;
        private byte[] chatKey;
        private UserState state = UserState.Offline;
        private string chatName = null;

        private Dictionary<UserAction, Action> userActionHandlers;

        public ChatClient(
            ITcpWrapper tcpClient,
            AesEncryption aesEncryption,
            ICoding coding)
        {
            this.tcpClient = tcpClient;
            this.aesEncryption = aesEncryption;
            this.coding = coding;
            rsa = new RSACryptoServiceProvider(4096);
            InitializeHandlers();
        }

        public void Listen()
        {
            try
            { 
                KeyExchange();

                state = UserState.Connected;

                Console.WriteLine(ConstantsStore.WelcomeMessage);

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();

                while (state != UserState.Offline)
                {
                    UserAction action = GetUserAction();
                    userActionHandlers[action].Invoke();

                };
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

        private UserAction GetUserAction()
        {
            switch (state)
            {
                case UserState.Connected:
                {
                    return GetLoginAction();
                }
                case UserState.Authorized:
                {
                    return GetMainMenuAction();
                }
                case UserState.InChat:
                {
                    return GetChatMenuAction();
                }
                default:
                {
                    throw new ChatException("Wrong user state");
                }
            }
        }

        private UserAction GetLoginAction()
        {
            while (true)
            {
                Console.WriteLine(ConstantsStore.LoginMenuItems + Environment.NewLine);

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                    {
                        return UserAction.Login;
                    }
                    case "2":
                    {
                        return UserAction.Register;
                    }
                    case "0":
                    {
                        return UserAction.Exit;
                    }
                }
            }
        }

        private UserAction GetMainMenuAction()
        {
            Console.WriteLine(Environment.NewLine + ConstantsStore.MainMenuTitle + Environment.NewLine + ConstantsStore.MainMenu);
            string actionNumber = Console.ReadLine();

            throw new NotImplementedException();
        }

        private UserAction GetChatMenuAction()
        {
            while (true)
            {
                Console.WriteLine(chatName);
                Console.WriteLine(ConstantsStore.ChatMenu);

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                    {
                        return UserAction.SendMessage;
                    }
                    case "2":
                    {
                        return UserAction.ShowUsers;
                    }
                    case "0":
                    {
                        return UserAction.GoToMainMenu;
                    }
                    default:
                    {
                        Console.WriteLine("Wrong input");
                        break;
                    }
                }
            }
        }

        private void InitializeHandlers()
        {
            userActionHandlers = new Dictionary<UserAction, Action>
            {
                { UserAction.Login, this.LoginHandler },
                { UserAction.Register, this.RegisterHandler },
                { UserAction.ShowChats, this.ShowAllChatsHandler },
                { UserAction.CreateChat, this.CreateChatHandler },
                { UserAction.EnterChat, this.EnterChatHandler },
                { UserAction.SendMessage, this.SendMessageHandler },
                { UserAction.ShowUsers, this.ShowAllUsersHandler },
                { UserAction.GoToMainMenu, this.GotoMainMenuHandler },
                { UserAction.GoToChatMenu, this.GoToChatHandler },
                { UserAction.Exit, this.ExitHandler },
            };
        }

        private void LoginHandler()
        {
            Console.WriteLine("Write your username");
            UserName = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Write your password");
            string password = Console.ReadLine();

            LoginRequest loginRequest = new LoginRequest();
            loginRequest.Sender = UserName;
            loginRequest.Password = GetPasswordHash(password);

            SendMessageAesEncrypted(loginRequest, serverKey);

            byte[] rawResponse = tcpClient.GetMessage();
            LoginResponse response = ParseMessage<LoginResponse>(rawResponse);

            if (response.StatusCode == 200)
            {
                state = UserState.Authorized;
                Console.WriteLine(ConstantsStore.SuccessfulSignIn);
            };
        }

        private T ParseMessage<T>(byte[] rawMessage)
        { 
            aesEncryption.SetKey(serverKey);
            byte[] decryptedConnectionMessage = aesEncryption.Decrypt(rawMessage);
            string connectionMessageInJson = coding.Decode(decryptedConnectionMessage);
            return JsonSerializer.Deserialize<T>(connectionMessageInJson);
        }

        private void RegisterHandler()
        {
            state = UserState.Authorized;
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

        private void SendMessageHandler()
        {
            throw new NotImplementedException();
        }

        private void ShowAllUsersHandler()
        {
            throw new NotImplementedException();
        }

        private void GotoMainMenuHandler()
        {
            throw new NotImplementedException();
        }

        private void GoToChatHandler()
        {
            throw new NotImplementedException();
        }

        private void ExitHandler()
        {
            Dispose();
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

        private void ReadServerPublicKey()
        {
            byte[] key = tcpClient.GetMessage();
            int bytesParsed;
            rsa.ImportRSAPublicKey(key, out bytesParsed);
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
