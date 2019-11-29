using System.Collections.Generic;

namespace ChatCommon.Messages.Responses
{
    public class ShowAllChatsResponse : Response
    {
        public ShowAllChatsResponse()
        {
            ChatNames = new List<string>();
        }

        public IReadOnlyCollection<string> ChatNames { get; set; }
    }
}