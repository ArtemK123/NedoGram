using ChatCommon;
using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class WrongUserStateException : NedoGramException
    {
        public WrongUserStateException(string userName, UserState givenUserState, UserState actualUserState)
            : base("Wrong user state")
        {
            UserName = userName;
            GivenUserState = givenUserState;
            ActualUserState = actualUserState;
        }

        public string UserName { get; set; }

        public UserState GivenUserState { get; set; }

        public UserState ActualUserState { get; set; }
    }
}