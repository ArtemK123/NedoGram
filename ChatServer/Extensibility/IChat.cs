using ChatCommon;
using ChatServer.Domain;
using System;
using System.Collections.Generic;
using ChatCommon.Messages;
using ChatServer.Domain.Entities;

namespace ChatServer.Extensibility
{
    internal interface IChat
    {
        Guid Id { get; }

        User Creator { get; }

        string Name { get; }

        byte[] Key { get; set; }

        IReadOnlyCollection<User> GetUsers();

        void AddUser(User user);

        void RemoveUser(string userName);
    }
}
