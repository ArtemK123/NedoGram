using ChatCommon;
using ChatServer.Extensibility;
using System;
using System.Collections.Generic;

namespace ChatServer.Domain
{
    internal class Chat : IChat
    {
        List<User> users = new List<User>();

        public Chat(User creator, string name)
        {

        }

        public Guid Id => throw new NotImplementedException();

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte[] Key { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public User Creator => throw new NotImplementedException();

        public void AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<User> GetUsers()
        {
            throw new NotImplementedException();
        }

        public bool RemoveUser(string userName)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(Message message)
        {
            throw new NotImplementedException();
        }

        bool IChat.AddUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
