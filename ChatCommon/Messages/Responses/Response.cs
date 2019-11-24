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

        public Response(StatusCode statusCode, string sender = "server")
            : base(sender)
        {
            Code = statusCode;
        }

        public StatusCode Code { get; set; }
    }
}
