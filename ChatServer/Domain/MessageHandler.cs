using System;
using System.Collections.Generic;
using System.Linq;
using ChatCommon;
using ChatCommon.Messages;
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
            IChat chat = server.ChatRepository.GetChat(chatName);
            IEnumerable<ClientInstance> clientsInChat = server.clients.Where(
                client => client.user.CurrentChat.Id == chat.Id 
                && client.user.State == UserState.InChat);
        
            foreach (ClientInstance client in clientsInChat)
            {
                client.SendMessageAesEncrypted(message, client.clientAesKey);
            }
        }

        public void SendToUser(string userName, Message message)
        {
            throw new NotImplementedException();
        }
    }
}
