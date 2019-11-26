using System.Collections.Generic;
using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class ShowChatsResponse : Response
    {
        public ShowChatsResponse(IReadOnlyCollection<string> chatNames, StatusCode code, string sender = "server")
            : base(code, sender)
        {
            ChatNames = chatNames;
        }

        public ShowChatsResponse() { }

        public IReadOnlyCollection<string> ChatNames { get; set; }
    }
}