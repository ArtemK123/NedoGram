using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class ShowUsersInChatRequest : Request
    {
        public ShowUsersInChatRequest()
        {
            Action = ClientAction.ShowUsersInChat;
        }

        public string ChatName { get; set; }
    }
}