using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class UserAlreadySignedInException : NedoGramException
    {
        public UserAlreadySignedInException(string userName)
            : base("User with this name is already signed in")
        {
            UserName = userName;
        }

        public string UserName { get; set; }
    }
}