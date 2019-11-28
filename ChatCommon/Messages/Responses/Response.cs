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

        public StatusCode Code { get; set; }

        public Guid RequestId { get; set; }

        public string Message { get; set; }
    }
}
