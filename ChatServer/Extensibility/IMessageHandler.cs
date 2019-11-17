using ChatCommon;
using System;

namespace ChatServer.Extensibility
{
    interface IMessageSenderService
    {
        void SendToChat(string chatName, Message message);

        void SendToUser(string userName, Message message);
    }
}
