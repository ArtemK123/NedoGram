using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class ErrorResponse : Response
    {
        public ErrorResponse(StatusCode code, string message, string sender = "server")
            : base(code, sender)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}