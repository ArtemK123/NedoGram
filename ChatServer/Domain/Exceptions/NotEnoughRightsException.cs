using ChatCommon;
using ChatCommon.Constants;
using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    public class NotEnoughRightsException : NedoGramException
    {
        public NotEnoughRightsException(string userName, UserState userState, ClientAction action)
            : base("User does not have rights to do this action")
        {
            UserName = userName;
            UserState = userState;
            Action = action;
        }

        public string UserName { get; set; }

        public UserState UserState { get; set; }

        public ClientAction Action { get; set; }
    }
}