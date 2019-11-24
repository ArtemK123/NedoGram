using ChatCommon.Actions;

namespace ChatCommon.Messages.Requests
{
    public class LoginRequest : Request
    {
        public LoginRequest()
        {
            Action = UserAction.Login;
        }

        public string Password { get; set; }
    }
}
