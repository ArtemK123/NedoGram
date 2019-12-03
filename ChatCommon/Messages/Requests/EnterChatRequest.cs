using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class EnterChatRequest : Request
    {
        public EnterChatRequest()
        {
            Action = ClientAction.EnterChat;
        }

        public string ChatName { get; set; }
    }
}