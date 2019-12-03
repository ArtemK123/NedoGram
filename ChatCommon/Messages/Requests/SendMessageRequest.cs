using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class SendMessageRequest : Request
    {
        public SendMessageRequest()
        {
            Action = ClientAction.SendMessage;
        }

        public byte[] EncryptedMessage { get; set; }

        public string ChatName { get; set; }
    }
}
