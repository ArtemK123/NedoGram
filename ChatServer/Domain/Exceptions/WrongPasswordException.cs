using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class WrongPasswordException : NedoGramException
    {
        public WrongPasswordException(string userName, string givenPassword, string actualPassword) 
            : base("Wrong password")
        {
            GivenPassword = givenPassword;
            ActualPassword = actualPassword;
            UserName = userName;
        }

        public string UserName { get; set; }

        public string GivenPassword { get; set; }

        public string ActualPassword { get; set; }
    }
}