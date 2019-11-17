﻿using System;
using System.Collections.Generic;
using System.Linq;
using ChatCommon;
using ChatServer.Extensibility;

namespace ChatServer.Domain
{
    internal class MessageHandler : IMessageSenderService
    {
        private ServerInstance server;

        public MessageHandler(ServerInstance server)
        {
            this.server = server;
        }

        public void SendToChat(string chatName, Message message)
        {
            IChat chat = server.chatRepository.GetChat(chatName);
            IEnumerable<ClientInstance> clientsInChat = server.clients.Where(
                client => client.user.CurrentChat.Id == chat.Id 
                && client.user.State == UserState.InChat);
        
            foreach (ClientInstance client in clientsInChat)
            {
                client.SendMessageWithServerAesEncryption(message);
            }
        }

        public void SendToUser(string userName, Message message)
        {
            throw new NotImplementedException();
        }
    }
}
