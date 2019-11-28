using System;
using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class Response : Message
    {
        public Response()
        {
            Sender = "server";
            MessageType = MessageType.Response;
        }

        public Response(StatusCode statusCode, ClientAction action, string message = "") : this()
        {
            Code = statusCode;
            Message = message;
            Action = action;
        }

        public StatusCode Code { get; set; }

        public ClientAction Action { get; set; }

        public Guid RequestId { get; set; }

        public string Message { get; set; }
    }
}
