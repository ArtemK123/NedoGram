using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class UserNotFoundException : NedoGramException
    {
        public UserNotFoundException(string userName)
            : base("User is not found")
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }
}