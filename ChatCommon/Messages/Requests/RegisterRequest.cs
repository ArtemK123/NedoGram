using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class RegisterRequest : Request
    {
        public RegisterRequest(string sender, string password)
            : base(sender)
        {
            Password = password;
        }

        public RegisterRequest()
        {
        }

        public string Password { get; set; }
    }
}
