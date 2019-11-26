using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class CreateChatRequest : Request
    {
        public CreateChatRequest(string chatName, string sender)
            : base(sender, ClientAction.CreateChat)
        {
            ChatName = chatName;
        }

        public CreateChatRequest()
        {
            Action = ClientAction.CreateChat;
        }

        public string ChatName { get; set; }
    }
}