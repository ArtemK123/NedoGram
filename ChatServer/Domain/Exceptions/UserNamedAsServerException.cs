using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class UserNamedAsServerException : NedoGramException
    {
        public UserNamedAsServerException() 
            : base("'server' name is reserved")
        {
        }
    }
}