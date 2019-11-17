using ChatCommon;
using ChatServer.Extensibility;
using System;

namespace ChatServer.Domain
{
    internal class User
    {
        public string Name { get; }

        public string Password { get; set; }

        public UserState State { get; set; }

        public IChat CurrentChat { get; set; }

        public User(string name = "Unknown user")
        {
            Name = name;
            State = UserState.Offline;
        }
    }
}
