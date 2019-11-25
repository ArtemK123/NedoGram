using ChatCommon;
using ChatServer.Domain;
using System;
using System.Collections.Generic;
using ChatCommon.Messages;

namespace ChatServer.Extensibility
{
    internal interface IChat
    {
        Guid Id { get; }

        User Creator { get; }

        string Name { get; }

        byte[] Key { get; set; }

        IReadOnlyCollection<User> GetUsers();

        bool AddUser(User user);

        bool RemoveUser(string userName);
    }
}
