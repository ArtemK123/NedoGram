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

        bool AddUser(User user);

        bool RemoveUser(string userName);

        byte[] GetKey();

        void SetKey(byte[] key);

        void SendMessage(Message message);
    }
}
