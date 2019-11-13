using System.Collections.Generic;
using ChatCommon.Extensibility;
using ChatServer.Extensibility;

namespace ChatServer.Domain
{
    internal class UserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> users = new Dictionary<string, User>();

        public bool Add(User user)
        {
            if (!users.ContainsKey(user.Name))
            {
                users.Add(user.Name, user);
                return true;
            }

            return false;
        }

        public bool Update(User user)
        {
            User oldUser;
            if (!users.TryGetValue(user.Name, out oldUser))
            {
                return false;
            }

            users[oldUser.Name] = user;
            return true;
        }

        public User GetByName(string name)
        {
            return users[name];
        }
    }
}
