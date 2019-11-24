using ChatCommon.Actions;

namespace ChatCommon.Messages.Requests
{
    public abstract class Request : Message
    {
        protected Request()
        {
        }

        protected Request(string sender, ClientAction action)
            : base(sender)
        {
            Action = action;
        }

        public ClientAction Action { get; protected set; }
    }
}
