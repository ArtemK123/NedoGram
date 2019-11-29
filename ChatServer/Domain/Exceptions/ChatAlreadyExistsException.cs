using ChatCommon.Exceptions;

namespace ChatServer.Domain.Exceptions
{
    internal class ChatAlreadyExistsException : NedoGramException
    {
        public ChatAlreadyExistsException(string chatName)
            : base("Chat already exists")
        {
            ChatName = chatName;
        }

        public string ChatName { get; set; }
    }
}