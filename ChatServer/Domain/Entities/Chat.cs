using System;
using System.Collections.Generic;
using System.Linq;
using ChatServer.Domain.Exceptions;
using ChatServer.Extensibility;

namespace ChatServer.Domain.Entities
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

        public void AddUser(User user)
        {
            if (users.Any(storedUser => storedUser.Name == user.Name))
            {
                throw new UserAlreadyExistsException(user.Name);
            }

            users.Add(user);
        }

        public IReadOnlyCollection<User> GetUsers()
            => users.ToArray();

        public void RemoveUser(string userName)
        {
            User user = users.FirstOrDefault(storedUser => storedUser.Name == userName);
            if (user == null)
            {
                throw new UserNotFoundException(userName);

            }

            users.Remove(user);
        }
    }
}
