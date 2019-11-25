using ChatCommon.Actions;

namespace ChatCommon.Messages.Requests
{
    public class Request : Message
    {
        public Request()
        {
        }

        public Request(string sender, ClientAction action)
            : base(sender)
        {
            Action = action;
        }

        public ClientAction Action { get; set; }
    }
}
