using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class ShowChatsRequest : Request
    {
        public ShowChatsRequest(string sender)
            : base(sender, ClientAction.ShowAllChats)
        {}

        public ShowChatsRequest() { }
    }
}
