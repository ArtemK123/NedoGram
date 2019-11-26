using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class Response : Message
    {
        public Response() : base("server")
        {
        }

        public Response(string sender) : base(sender)
        {
        }

        public Response(StatusCode statusCode, ClientAction action, string message = "", string sender = "server")
            : base(sender)
        {
            Code = statusCode;
            Message = message;
            Action = action;

        }

        public StatusCode Code { get; set; }

        public ClientAction Action { get; set; }

        public string Message { get; set; }
    }
}
