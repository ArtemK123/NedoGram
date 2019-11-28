using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class UserAlreadyExistException : NedoGramException
    {
        public UserAlreadyExistException(string userName) 
            : base("User already exists")
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }
}