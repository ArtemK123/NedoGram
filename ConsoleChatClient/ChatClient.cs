using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using ChatCommon;
using ChatCommon.Actions;
using ChatCommon.Constants;
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

        private Dictionary<ClientAction, Action> userActionHandlers;

        public ChatClient(
            ITcpWrapper tcpClient,
            AesEncryption aesEncryption,
            ICoding coding)
        {
            this.tcpClient = tcpClient;
            this.aesEncryption = aesEncryption;
            this.coding = coding;
            rsa = new RSACryptoServiceProvider(4096);
            serverKey = this.aesEncryption.GetKey();
            InitializeHandlers();
        }

        public void Listen()
        {
            try
            { 
                KeyExchange();

                Console.WriteLine(ConstantsStore.WelcomeMessage);

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();

                while (state != UserState.Offline)
                {
                    ClientAction action = GetUserAction();
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

        private ClientAction GetUserAction()
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

        private ClientAction GetLoginAction()
        {
            while (true)
            {
                Console.WriteLine(ConstantsStore.LoginMenuItems + Environment.NewLine);

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                    {
                        return ClientAction.Login;
                    }
                    case "2":
                    {
                        return ClientAction.Register;
                    }
                    case "0":
                    {
                        return ClientAction.Exit;
                    }
                }
            }
        }

        private ClientAction GetMainMenuAction()
        {
            Console.WriteLine(Environment.NewLine + ConstantsStore.MainMenuTitle + Environment.NewLine + ConstantsStore.MainMenu);
            string actionNumber = Console.ReadLine();

            throw new NotImplementedException();
        }

        private ClientAction GetChatMenuAction()
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
                        return ClientAction.SendMessage;
                    }
                    case "2":
                    {
                        return ClientAction.ShowUsers;
                    }
                    case "0":
                    {
                        return ClientAction.GoToMainMenu;
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
            userActionHandlers = new Dictionary<ClientAction, Action>
            {
                { ClientAction.Login, this.LoginHandler },
                { ClientAction.Register, this.RegisterHandler },
                { ClientAction.ShowChats, this.ShowAllChatsHandler },
                { ClientAction.CreateChat, this.CreateChatHandler },
                { ClientAction.EnterChat, this.EnterChatHandler },
                { ClientAction.SendMessage, this.SendMessageHandler },
                { ClientAction.ShowUsers, this.ShowAllUsersHandler },
                { ClientAction.GoToMainMenu, this.GotoMainMenuHandler },
                { ClientAction.GoToChatMenu, this.GoToChatHandler },
                { ClientAction.Exit, this.ExitHandler },
            };
        }

        private void LoginHandler()
        {
            while (true)
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
                Response response = ParseMessage<Response>(rawResponse);

                if (response.Code == StatusCode.Ok)
                {
                    state = UserState.Authorized;
                    Console.WriteLine(ConstantsStore.SuccessfulSignIn);
                    return;
                }

                ErrorResponse errorResponse = ParseMessage<ErrorResponse>(rawResponse);
                Console.WriteLine($"Error while signing in - {errorResponse.Message}");
            }
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
            //todo: add handling of an unsuccessful key exchange

            // read server credentials
            ReadServerPublicKey();

            // send aes key and iv to the server
            AesKeyExchangeMessage keyExchangeMessage = new AesKeyExchangeMessage(aesEncryption.GetKey(), aesEncryption.GetIV(), UserName);

            tcpClient.Send(rsa.Encrypt(coding.GetBytes(JsonSerializer.Serialize(keyExchangeMessage)), false));

            byte[] rawResponse = tcpClient.GetMessage();

            Response response = ParseMessage<Response>(rawResponse);

            if (response.Code == StatusCode.Ok)
            {
                state = UserState.Connected;
            }
            else
            {
                ErrorResponse responseWithError = ParseMessage<ErrorResponse>(rawResponse);
                Console.WriteLine(responseWithError.Message);
            }
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

                //Message messageObj = new Message(headers, coding.GetBytes(input));

                //string messageInJson = JsonSerializer.Serialize(messageObj);

                //byte[] encryptedData = aesEncryption.Encrypt(coding.GetBytes(messageInJson));
                //Console.WriteLine($"Encrypted and derypted: {aesEncryption.Decrypt(encryptedData)}");

                //tcpClient.Send(encryptedData);
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

                    //Console.WriteLine($"{message.Headers["sender"]}: {message.Body}");
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
