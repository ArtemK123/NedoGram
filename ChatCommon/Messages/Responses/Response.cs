namespace ChatCommon.Messages.Responses
{
    public class Response : Message
    {
        public Response() : base("server")
        {
        }

        public Response(int statusCode, string message, string sender = "server")
            : base(sender)
        {
            StatusCode = statusCode;
            Message = message;
        }

        public int StatusCode { get; set; }

        public string Message { get; set; }
    }
}
