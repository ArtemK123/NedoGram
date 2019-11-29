using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class ShowAllChatsRequest : Request
    {
        public ShowAllChatsRequest(string sender)
            : base(sender)
        {}

        public ShowAllChatsRequest() { }
    }
}
