using System.Collections.Generic;

namespace ChatCommon
{
    public class Message
    {
        public Dictionary<string, string> Headers { get; set; }

        public byte[] Body { get; set; }

        public Message()
        {
            Headers = new Dictionary<string, string>();
            Body = new byte[0];
        }

        public Message(Dictionary<string, string> headers, byte[] body)
        {
            this.Headers = headers;
            this.Body = body;
        }
    }
}
