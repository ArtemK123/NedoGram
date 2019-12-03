using ChatCommon.Messages.Notifications;

namespace ChatServer.Extensibility
{
    interface INotificationSender
    {
        void SendToChat(string chatName, Notification notification);

        void SendToUser(string userName, Notification notification);
    }
}
