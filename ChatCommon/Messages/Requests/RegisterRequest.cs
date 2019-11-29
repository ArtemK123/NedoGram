using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class RegisterRequest : Request
    {
        public RegisterRequest(string sender, string password)
            : base(sender)
        {
            Password = password;
            Action = ClientAction.Register;
        }

        public RegisterRequest()
        {
            Action = ClientAction.Register;
        }

        public string Password { get; set; }
    }
}
