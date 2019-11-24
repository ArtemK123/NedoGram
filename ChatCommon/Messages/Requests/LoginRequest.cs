using ChatCommon.Actions;

namespace ChatCommon.Messages.Requests
{
    public class LoginRequest : Request
    {
        public LoginRequest()
        {
            Action = ClientAction.Login;
        }

        public string Password { get; set; }
    }
}
