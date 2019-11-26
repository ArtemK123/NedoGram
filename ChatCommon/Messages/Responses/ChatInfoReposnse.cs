using System.Collections.Generic;
using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class ChatInfoReposnse : Response
    {
        public ChatInfoReposnse(string chatName, IReadOnlyCollection<string> userNames, byte[] key)
            : base(StatusCode.Ok)
        {
            ChatName = chatName;
            UserNames = userNames;
            Key = key;
        }

        public ChatInfoReposnse() {}

        public string ChatName { get; set; }

        public IReadOnlyCollection<string> UserNames { get; set; }

        public byte[] Key { get; set; }
    }
}