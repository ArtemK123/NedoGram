namespace ChatServer.Extensibility
{
    internal interface IUserRepository
    {
        bool Add(User user);

        bool Update(User user);

        User GetByName(string name);
    }
}