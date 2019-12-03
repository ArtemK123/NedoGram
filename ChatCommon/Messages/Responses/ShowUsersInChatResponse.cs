using System.Collections.Generic;

namespace ChatCommon.Messages.Responses
{
    public class ShowUsersInChatResponse : Response
    {
        public IReadOnlyCollection<string> UserNames { get; set; }
    }
}