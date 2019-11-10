using System.Collections.Generic;

namespace ChatCommon
{
    public class Message
    {
        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }

        public Message()
        {
        }

        public Message(Dictionary<string, string> headers, string body)
        {
            this.Headers = headers;
            this.Body = body;
        }
    }
}
