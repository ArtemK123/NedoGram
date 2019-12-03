using System;
using System.Collections.Generic;
using System.Linq;
using ChatCommon;
using ChatCommon.Messages;
using ChatCommon.Messages.Notifications;
using ChatServer.Extensibility;

namespace ChatServer.Domain
{
    internal class NotificationSender : INotificationSender
    {
        private ServerInstance server;

        public NotificationSender(ServerInstance server)
        {
            this.server = server;
        }

        public void SendToChat(string chatName, Notification notification)
        {
            IChat chat = server.ChatRepository.GetChat(chatName);

            IEnumerable<ClientInstance> clientsInChat =
                server.clients
                    .Where(client =>
                        chat.GetUsers().Any(user => user.Name == client.CurrentUserName && client.CurrentUserName != notification.Sender));

            foreach (ClientInstance client in clientsInChat)
            {
                client.SendMessageAesEncrypted(notification, client.ClientAesKey);
            }
        }

        public void SendToUser(string userName, Notification notification)
        {
            throw new NotImplementedException();
        }
    }
}
