using ChatServer.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ChatServer.Domain
{
    internal class Chat : IChat
    {
        private readonly List<User> users = new List<User>();

        public Chat(User creator, string name, byte[] aesKey)
        {
            Id = Guid.NewGuid();
            Creator = creator;
            Name = name;
            Key = aesKey;
        }

        public Guid Id { get; }

        public User Creator { get; }

        public string Name { get; }

        public byte[] Key { get; set; }

        public bool AddUser(User user)
        {
            if (users.Any(storedUser => storedUser.Name == user.Name))
            {
                return false;
            }

            users.Add(user);
            return true;
        }

        public IReadOnlyCollection<User> GetUsers()
            => users.ToArray();

        public bool RemoveUser(string userName)
        {
            User user = users.FirstOrDefault(storedUser => storedUser.Name == userName);
            if (user == null)
            {
                return false;

            }

            users.Remove(user);
            return true;
        }
    }
}
