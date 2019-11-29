using ChatCommon;
using ChatServer.Extensibility;

namespace ChatServer.Domain.Entities
{
    internal class User
    {
        public string Name { get; }

        public string Password { get; set; }

        public UserState State { get; set; }

        public IChat CurrentChat { get; set; }

        public User(string name, string password, UserState state = UserState.Offline, IChat currentChat = null)
        {
            Name = name;
            Password = password;
            State = state;
            CurrentChat = currentChat;
        }

        public User()
        {
            State = UserState.Offline;
        }
    }
}
