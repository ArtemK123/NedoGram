using ChatCommon;
using ChatServer.Domain;
using System;
using System.Collections.Generic;

namespace ChatServer.Extensibility
{
    internal interface IChat
    {
        Guid Id { get; }

        User Creator { get; }

        byte[] Key { get; set; }

        string Name { get; set; }

        IReadOnlyCollection<User> GetUsers();

        bool AddUser(User user);

        bool RemoveUser(string userName);

        void SendMessage(Message message);
    }
}
