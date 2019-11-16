using ChatCommon;
using ChatServer.Domain;
using System;
using System.Collections.Generic;

namespace ChatServer.Extensibility
{
    internal interface IChat
    {
        Guid Id { get; }

        string Name { get; set; }

        IReadOnlyCollection<User> GetUsers();

        void AddUser(User user);

        byte[] GetKey();

        void SetKey(byte[] key);

        void SendMessage(Message message);
    }
}
