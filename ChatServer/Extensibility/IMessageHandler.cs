using ChatCommon;
using System;
using ChatCommon.Messages;

namespace ChatServer.Extensibility
{
    interface IMessageSenderService
    {
        void SendToChat(string chatName, Message message);

        void SendToUser(string userName, Message message);
    }
}
