using ChatServer.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatServer.Domain
{
    internal class Chat : IChat
    {
        private readonly HashSet<User> users = new HashSet<User>();

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
            try
            {
                users.Add(user);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public IReadOnlyCollection<User> GetUsers()
            => users.ToArray();

        public bool RemoveUser(string userName)
        {
            User user = users.FirstOrDefault(storedUser => storedUser.Name == userName);
            if (user != null)
            {
                users.Remove(user);
                return true;
            }

            return false;
        }
    }
}
