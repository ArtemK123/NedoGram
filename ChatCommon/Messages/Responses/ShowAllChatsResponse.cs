using System.Collections.Generic;
using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class ShowAllChatsResponse : Response
    {
        public ShowAllChatsResponse(IReadOnlyCollection<string> chatNames, StatusCode code, string message = "")
            : base(code, ClientAction.ShowAllChats, message)
        {
            ChatNames = chatNames ?? new List<string>();
        }

        public ShowAllChatsResponse() { }

        public IReadOnlyCollection<string> ChatNames { get; set; }
    }
}