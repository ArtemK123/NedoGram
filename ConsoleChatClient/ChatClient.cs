using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using ChatCommon;
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
        private UserState userState = UserState.Offline;
        private string chatName;

        private Dictionary<ClientAction, Action> userActionHandlers;
        private Dictionary<Guid, Action<string>> responseHandlers;

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

                while (userState != UserState.Offline)
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
            switch (userState)
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
                    throw new NedoGramException("Wrong user state");
                }
            }
        }

        private ClientAction GetLoginAction()
        {
            while (true)
            {
                Console.WriteLine(Environment.NewLine + ConstantsStore.LoginMenuItems + Environment.NewLine);

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
            while (true)
            {
                Console.WriteLine(Environment.NewLine + ConstantsStore.MainMenuTitle + Environment.NewLine + ConstantsStore.MainMenu);
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                    {
                        return ClientAction.ShowAllChats;
                    }
                    case "2":
                    {
                        return ClientAction.CreateChat;
                    }
                    case "3":
                    {
                        return ClientAction.EnterChat;
                    }
                    case "0":
                    {
                        return ClientAction.Exit;
                    }
                    default:
                    {
                        Console.WriteLine("Wrong input");
                        break;
                    }
                }
            }
        }

        private ClientAction GetChatMenuAction()
        {
            while (true)
            {
                Console.WriteLine(Environment.NewLine + chatName);
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
                { ClientAction.Login, LoginRequestHandler },
                { ClientAction.Register, RegisterRequestHandler },
                { ClientAction.ShowAllChats, ShowAllChatsRequestHandler },
                { ClientAction.CreateChat, CreateChatRequestHandler },
                { ClientAction.EnterChat, EnterChatRequestHandler },
                { ClientAction.SendMessage, SendMessageRequestHandler },
                { ClientAction.ShowUsers, ShowAllUsersRequestHandler },
                { ClientAction.GoToMainMenu, GotoMainMenuRequestHandler },
                { ClientAction.GoToChatMenu, GoToChatRequestHandler },
                { ClientAction.Exit, ExitRequestHandler }
            };
        }

        private void LoginRequestHandler()
        {
            Console.WriteLine("Write your username");
            UserName = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Write your password");
            string password = Console.ReadLine();

            LoginRequest loginRequest = new LoginRequest();
            loginRequest.Sender = UserName;
            loginRequest.Password = GetPasswordHash(password);

            responseHandlers.Add(loginRequest.Id, LoginResponseHandler);
            SendMessageAesEncrypted(loginRequest, serverKey);

            while (responseHandlers.ContainsKey(loginRequest.Id)) { }
        }

        private void LoginResponseHandler(string responseInJson)
        {
            LoginResponse response = JsonSerializer.Deserialize<LoginResponse>(responseInJson);

            if (response.Code == StatusCode.Ok)
            {
                userState = UserState.Authorized;
                Console.WriteLine(ConstantsStore.SuccessfulSignIn);
                return;
            }

            Console.WriteLine($"Error while signing in - {response.Message}");
        }

        private void RegisterRequestHandler()
        {
            Console.WriteLine("Enter your nickname");

            string userName = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Write your password");

            string password = Console.ReadLine();

            Console.WriteLine(Environment.NewLine + "Write your password again");

            string passwordAgain = Console.ReadLine();

            if (password != passwordAgain)
            {
                Console.WriteLine("Passwords don`t match");
                return;
            }

            var registerRequest = new RegisterRequest(userName, GetPasswordHash(password));

            responseHandlers.Add(registerRequest.Id, LoginResponseHandler);
            SendMessageAesEncrypted(registerRequest, serverKey);

            while (responseHandlers.ContainsKey(registerRequest.Id)) { }
        }

        private void RegisterResponseHandler(string responseInJson)
        {
            RegisterResponse response = JsonSerializer.Deserialize<RegisterResponse>(responseInJson);

            if (response.Code == StatusCode.Ok)
            {
                UserName = response.UserName;
                userState = UserState.Authorized;
                Console.WriteLine("Signed up successfully");
                return;
            }

            Console.Write($"Problems were occured while signing up - {response.Message}");
        }

        private void ShowAllChatsRequestHandler()
        {
            var request = new ShowChatsRequest(UserName);

            responseHandlers.Add(request.Id, LoginResponseHandler);
            SendMessageAesEncrypted(request, serverKey);

            while (responseHandlers.ContainsKey(request.Id)) { }
        }

        private void ShowAllChatsResponseHandler(string responseInJson)
        {
            ShowAllChatsResponse response = JsonSerializer.Deserialize<ShowAllChatsResponse>(responseInJson);

            Console.WriteLine("Available chats:");

            if (response?.ChatNames == null)
            {
                return;
            }
            foreach (string chatName in response.ChatNames)
            {
                Console.WriteLine(chatName);
            }
        }

        private void CreateChatRequestHandler()
        {
            Console.WriteLine(Environment.NewLine + "Write a chat name:");
            string newChatName = Console.ReadLine();

            var request = new CreateChatRequest(newChatName, UserName);

            responseHandlers.Add(request.Id, LoginResponseHandler);
            SendMessageAesEncrypted(request, serverKey);

            while (responseHandlers.ContainsKey(request.Id)) { }
        }

        private void CreateChatResponseHandler(string responseInJson)
        {
            CreateChatResponse response = JsonSerializer.Deserialize<CreateChatResponse>(responseInJson);

            if (response.Code != StatusCode.Ok)
            {
                Console.WriteLine(response.Message);
                return;
            }

            chatKey = response.Key;
            chatName = response.ChatName;
            userState = UserState.InChat;
            Console.WriteLine("Chat was created successfully");
            Console.WriteLine($"Connected to the chat {chatName}");
        }
        
        private void EnterChatRequestHandler()
        {
            Console.WriteLine("EnterChatHandler");
        }

        private void EnterChatResponseHandler(string responseInJson)
        {
            Console.WriteLine("EnterChatHandler");
        }

        private void SendMessageRequestHandler()
        {
            throw new NotImplementedException();
        }

        private void SendMessageResponseHandler(string responseInJson)
        {
            throw new NotImplementedException();
        }

        private void ShowAllUsersRequestHandler()
        {
            throw new NotImplementedException();
        }

        private void ShowAllUsersResponseHandler(string responseInJson)
        {
            throw new NotImplementedException();
        }

        private void GotoMainMenuRequestHandler()
        {
            throw new NotImplementedException();
        }

        private void GotoMainMenuResponseHandler(string responseInJson)
        {
            throw new NotImplementedException();
        }

        private void GoToChatRequestHandler()
        {
            throw new NotImplementedException();
        }

        private void GoToChatResponseHandler(string responseInJson)
        {
            throw new NotImplementedException();
        }

        private void ExitRequestHandler()
        {
            Dispose();
        }

        private void ExitResponseHandler(string responseInJson)
        {
            Dispose();
        }

        private void KeyExchange()
        {
            //todo: add handling of an unsuccessful key exchange

            // read server credentials
            ReadServerPublicKey();

            // send aes key and iv to the server
            AesKeyExchangeRequest keyExchangeRequest = new AesKeyExchangeRequest(aesEncryption.GetKey(), aesEncryption.GetIV(), UserName);

            tcpClient.Send(rsa.Encrypt(coding.GetBytes(JsonSerializer.Serialize(keyExchangeRequest)), false));

            byte[] rawResponse = tcpClient.GetMessage();

            Response response = ParseMessage<Response>(rawResponse, serverKey);

            if (response.Code == StatusCode.Ok)
            {
                userState = UserState.Connected;
            }
            else
            {
                Console.WriteLine(response.Message);
            }
        }

        private string ParseMessageToJson(byte[] rawMessage, byte[] aesKey)
        {
            aesEncryption.SetKey(aesKey);
            byte[] decryptedConnectionMessage = aesEncryption.Decrypt(rawMessage);
            return coding.Decode(decryptedConnectionMessage);
        }

        private T ParseMessage<T>(byte[] rawMessage, byte[] aesKey)
            => JsonSerializer.Deserialize<T>(ParseMessageToJson(rawMessage, aesKey));

        private void SendMessageAesEncrypted<T>(T message, byte[] key) where T : Message
        {
            aesEncryption.SetKey(key);

            string messageInJson = JsonSerializer.Serialize(message);
            tcpClient.Send(aesEncryption.Encrypt(coding.GetBytes(messageInJson)));
        }

        private void ReadServerPublicKey()
        {
            byte[] key = tcpClient.GetMessage();
            int bytesParsed;
            rsa.ImportRSAPublicKey(key, out bytesParsed);
        }

        private string GetPasswordHash(string password)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                return Convert.ToBase64String(md5.ComputeHash(coding.GetBytes(password)));
            }
        }

        private void ReceiveMessage()
        {
            try
            {
                while (true)
                {
                    byte[] rawMessage = tcpClient.GetMessage();

                    string messageInJson = ParseMessageToJson(rawMessage, serverKey);

                    Message message = JsonSerializer.Deserialize<Message>(messageInJson);

                    if (message.MessageType == MessageType.Response)
                    {
                        Response response = JsonSerializer.Deserialize<Response>(messageInJson);

                        Console.WriteLine($"{response.Sender} - {response.Action}");
                        responseHandlers[response.RequestId].Invoke(messageInJson);
                        responseHandlers.Remove(response.RequestId);
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Connection is closed");
                Dispose();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error = {exception.Message}");
            }
        }
    }
}
