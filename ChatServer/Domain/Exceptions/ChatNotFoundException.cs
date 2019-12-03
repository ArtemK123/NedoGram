using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    public class ChatNotFoundException : NedoGramException
    {
        public ChatNotFoundException(string chatName)
            : base("Chat is not found")
        {
            ChatName = chatName;
        }

        public string ChatName { get; set; }
    }
}