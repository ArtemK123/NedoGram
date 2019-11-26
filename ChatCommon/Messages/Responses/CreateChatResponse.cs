using System.Collections.Generic;
using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class CreateChatResponse : Response
    {
        public CreateChatResponse(string chatName, IReadOnlyCollection<string> userNames, byte[] key, StatusCode code, string message = "")
            : base(code, ClientAction.CreateChat, message)
        {
            ChatName = chatName;
            UserNames = userNames ?? new List<string>();
            Key = key ?? new byte[0];
        }

        public CreateChatResponse() { }

        public string ChatName { get; set; }

        public IReadOnlyCollection<string> UserNames { get; set; }

        public byte[] Key { get; set; }
    }
}
