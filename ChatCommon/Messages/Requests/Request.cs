using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class Request : Message
    {
        public Request()
        {
            MessageType = MessageType.Request;
        }

        public Request(string sender)
            : this()
        {
            Sender = sender;
        }

        public ClientAction Action { get; set; }
    }
}
