using ChatCommon.Actions;

namespace ChatCommon.Messages.Requests
{
    public abstract class Request : Message
    {
        protected Request()
        {
        }

        protected Request(string sender, UserAction action)
            : base(sender)
        {
            Action = action;
        }

        public UserAction Action { get; protected set; }
    }
}
