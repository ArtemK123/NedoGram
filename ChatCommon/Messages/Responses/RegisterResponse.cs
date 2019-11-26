using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class RegisterResponse : Response
    {
        public RegisterResponse(StatusCode code, string userName, string message = "") 
            : base(code, ClientAction.Register, message)
        {
            UserName = userName;
        }

        public RegisterResponse() { }

        public string UserName { get; set; }
    }
}