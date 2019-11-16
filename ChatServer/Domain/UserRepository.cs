using System.Collections.Generic;
using System.Linq;
using ChatServer.Extensibility;

namespace ChatServer.Domain
{
    internal class UserRepository : IUserRepository
    {
        private readonly List<User> users = new List<User>();

        public bool Add(User user)
        {
            if (user.Name.ToLower() == "server" || users.Any(storedUser => storedUser.Name == user.Name))
            {
                return false;
            }

            users.Add(user);
            return true;
        }

        public User GetByName(string name)
        {
            return users.FirstOrDefault(user => user.Name == name);
        }

        public bool Contains(string userName)
        {
            return users.Any(user => user.Name == userName);
        }

        public bool UpdateState(string userName, UserState state)
        {
            User user = users.FirstOrDefault(storedUser => storedUser.Name == userName);

            if (user == null)
            {
                return false;
            }
            user.State = state;
            return true;
        }
    }
}
