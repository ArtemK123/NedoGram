using System.Collections.Generic;
using System.Linq;
using ChatCommon;
using ChatServer.Domain.Entities;
using ChatServer.Domain.Exceptions;
using ChatServer.Extensibility;

namespace ChatServer.Domain.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly List<User> users = new List<User>();

        public void Add(User user)
        {
            if (user.Name.ToLower() == "server")
            {
                throw new UserNamedAsServerException();
            }

            if (users.Any(storedUser => storedUser.Name == user.Name))
            {
                throw new UserAlreadyExistsException(user.Name);
            }

            users.Add(user);
        }

        public User GetByName(string name)
        {
            return users.FirstOrDefault(user => user.Name == name);
        }

        public bool Contains(string userName)
        {
            return users.Any(user => user.Name == userName);
        }

        public void UpdateState(string userName, UserState state)
        {
            User user = users.FirstOrDefault(storedUser => storedUser.Name == userName);

            if (user == null)
            {
                throw new UserNotFoundException(userName);
            }
            user.State = state;
        }
    }
}
