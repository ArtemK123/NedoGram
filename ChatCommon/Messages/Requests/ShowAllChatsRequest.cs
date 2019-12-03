using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class ShowAllChatsRequest : Request
    {
        public ShowAllChatsRequest(string sender)
            : base(sender)
        {
            Action = ClientAction.ShowAllChats;
        }

        public ShowAllChatsRequest() { }
    }
}
