﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ChatCommon;
using ChatCommon.Actions;
using ChatCommon.Constants;
using ChatCommon.Encryption;
using ChatCommon.Extensibility;
using ChatCommon.Messages;
using ChatCommon.Messages.Requests;
using ChatCommon.Messages.Responses;
using ChatServer.Domain;

namespace ChatServer
{
    public class ClientInstance : IDisposable
    {
        protected internal Guid Id { get; }

        internal User user;
        internal readonly ICoding coding;
        private readonly ITcpWrapper tcpClient;
        private readonly ServerInstance server;
        private readonly AesEncryption aesEncryption;
        internal byte[] clientAesKey;

        internal readonly Dictionary<ClientAction, Action<string>> RequestHandlers;

        public ClientInstance(ITcpWrapper tcpClient, ServerInstance serverInstance, ICoding coding)
        {
            Id = Guid.NewGuid();
            server = serverInstance;
            this.tcpClient = tcpClient;
            this.coding = coding;
            aesEncryption = new AesEncryption();
            user = new User();
            RequestHandlers = new Dictionary<ClientAction, Action<string>>()
            {
                { ClientAction.Login, LoginHandler },
                { ClientAction.Register, RegisterHandler }
            };
        }

        public void Process()
        {
            try
            {
                Console.WriteLine($"New connection - {Id}");

                Console.WriteLine($"Key exchange");
                KeyExchange();

                Console.WriteLine(
                    $"Key exchanged successfully. Connection configured. Id-{Id}; Key-{Convert.ToBase64String(aesEncryption.GetKey())}");

                // handle requests from client

                while (true)
                {
                    byte[] rawMessage = tcpClient.GetMessage();

                    string messageInJson = ParseMessageToJson(rawMessage, clientAesKey);

                    Request request = JsonSerializer.Deserialize<Request>(messageInJson);

                    Console.WriteLine($"{request.Sender} - {request.Action}");
                    RequestHandlers[request.Action].Invoke(messageInJson);
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"User left. Username: {user?.Name}; id: {Id}");
                if (user != null && user.State == UserState.Authorized)
                {
                    server.UserRepository.UpdateState(user.Name, UserState.Offline);
                }
                server.RemoveConnection(this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public void Dispose()
        {
            tcpClient?.Dispose();
        }

        internal void SendMessageAesEncrypted<T>(T message, byte[] aesKey) where T : Message
        {
            aesEncryption.SetKey(aesKey);
            tcpClient.Send(aesEncryption.Encrypt(coding.GetBytes(JsonSerializer.Serialize(message))));
        }

        private void KeyExchange()
        {
            while (true)
            {
                // send to the client server public key [and other credentials]

                tcpClient.Send(server.rsa.ExportRSAPublicKey());

                // get key for symmetric encryption from client

                byte[] encryptedMessageWithKey = tcpClient.GetMessage();

                try
                {
                    byte[] messageWithKeyBytes = server.rsa.Decrypt(encryptedMessageWithKey, false);
                    string messageWithKeyInJson = coding.Decode(messageWithKeyBytes);
                    AesKeyExchangeMessage aesKeyExchangeMessage = JsonSerializer.Deserialize<AesKeyExchangeMessage>(messageWithKeyInJson);

                    aesEncryption.SetKey(aesKeyExchangeMessage.Key);
                    aesEncryption.SetIV(aesKeyExchangeMessage.IV);

                    clientAesKey = aesEncryption.GetKey();

                    Response response = new Response(StatusCode.Ok);

                    SendMessageAesEncrypted(response, clientAesKey);
                    user.State = UserState.Connected;
                    return;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        private void LoginHandler(string requestInJson)
        {
            LoginRequest request = JsonSerializer.Deserialize<LoginRequest>(requestInJson);

            bool successful;
            User storedUser = null;
            try
            {
                user = server.UserRepository.GetByName(request.Sender);

                successful = user.Password == request.Password;
            }
            catch (Exception)
            {
                successful = false;
            }

            if (successful)
            {
                SendMessageAesEncrypted(new Response(StatusCode.Ok), clientAesKey);
                server.UserRepository.UpdateState(user.Name, UserState.Authorized);
                Console.WriteLine($"{request.Sender} signed in");
            }
            else
            {
                SendMessageAesEncrypted(new Response(StatusCode.Error, "Wrong email or password"), clientAesKey);
                Console.WriteLine($"{request.Sender} - unsuccessful try to sign in");
            }
        }

        private void RegisterHandler(string requestInJson)
        {
            try
            {
                RegisterRequest registerRequest = JsonSerializer.Deserialize<RegisterRequest>(requestInJson);
                User newUser = new User(registerRequest.Sender, registerRequest.Password, UserState.Authorized);
                server.UserRepository.Add(newUser);

                SendMessageAesEncrypted(new Response(StatusCode.Ok), clientAesKey);
                user = newUser;
                Console.WriteLine($"{user.Name} signed up successfully.");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{user.Name} - error occured while signing up: {exception}");
                SendMessageAesEncrypted(new Response(StatusCode.Error, "Error while signing up"), clientAesKey);
            }
        }

        private void CreateChatHandler(Message message)
        {
            //string chatName = message.Headers["chatName"];
            //IChat newChat = new Chat(user, chatName);
            //newChat.AddUser(user);
            //user.State = UserState.InChat;
            //server.UserRepository.UpdateState(user.Name, UserState.InChat);
            //server.ChatRepository.AddChat(newChat);
            //SendSuccessResponse(newChat.Key);
        }

        private void GetAllChatsHandler()
        {
            //Message response = new Message();
            //response.Headers.Add("action", "provideAllChats");
            //response.Headers.Add("sender", "server");
            //response.Headers.Add("content-type", "json");
            //response.Headers.Add("encryption", "aes");

            //IReadOnlyCollection<IChat> chats = server.ChatRepository.GetChats();

            //response.Body = coding.GetBytes(JsonSerializer.Serialize(chats.Select(chat => chat.Name)));
        }

        private void EnterChatHandler(Message message)
        {
            //IChat chat = server.ChatRepository.GetChat(message.Headers["chatName"]);
            //if (chat == null)
            //{
            //    SendErrorResponse();
            //    return;
            //}
            //chat.AddUser(user);
            //user.CurrentChat = chat;
            //server.UserRepository.UpdateState(user.Name, UserState.InChat);
            //SendSuccessResponse(chat.Key);
        }

        private void MessageHandler(Message message, byte[] rawMessage)
        {
            //server.MessageSender.SendToChat(message.Headers["chat"], message);

            //server.BroadcastMessage(rawMessage, this);
        }

        private void ExitChatHandler(Message message)
        {
            user.CurrentChat.RemoveUser(user.Name);
            user.CurrentChat = null;
            server.UserRepository.UpdateState(user.Name, UserState.Authorized);
            //SendSuccessResponse();
        }

        private string ParseMessageToJson(byte[] rawMessage, byte[] aesKey)
        {
            aesEncryption.SetKey(aesKey);

            byte[] decryptedConnectionMessage = aesEncryption.Decrypt(rawMessage);

            return coding.Decode(decryptedConnectionMessage);
        }
    }
}
