using ChatServer.Domain;

namespace ChatServer.Extensibility
{
    internal interface IUserRepository
    {
        bool Add(User user);

        bool UpdateState(string userName, UserState state);

        User GetByName(string name);

        bool Contains(string name);
    }
}