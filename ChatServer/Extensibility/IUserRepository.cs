using ChatCommon;
using ChatServer.Domain.Entities;

namespace ChatServer.Extensibility
{
    internal interface IUserRepository
    {
        void Add(User user);

        void UpdateState(string userName, UserState state);

        User GetByName(string name);

        bool Contains(string name);
    }
}