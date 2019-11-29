using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class UserAlreadyExistsException : NedoGramException
    {
        public UserAlreadyExistsException(string userName) 
            : base("User already exists")
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }
}