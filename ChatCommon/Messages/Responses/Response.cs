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

        public Response(StatusCode statusCode, string message = "", string sender = "server")
            : base(sender)
        {
            Code = statusCode;
            Message = message;
        }

        public StatusCode Code { get; set; }

        public string Message { get; set; }
    }
}
