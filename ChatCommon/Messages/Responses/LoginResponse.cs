using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class LoginResponse : Response
    {
        public LoginResponse(StatusCode code, string message = "") : base(code, ClientAction.Login, message) { }
        
        public LoginResponse() { }
    }
}