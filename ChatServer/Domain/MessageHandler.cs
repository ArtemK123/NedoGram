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
                client => client.CurrentUser.CurrentChat.Id == chat.Id 
                && client.CurrentUser.State == UserState.InChat);
        
            foreach (ClientInstance client in clientsInChat)
            {
                client.SendMessageAesEncrypted(message, client.ClientAesKey);
            }
        }

        public void SendToUser(string userName, Message message)
        {
            throw new NotImplementedException();
        }
    }
}
