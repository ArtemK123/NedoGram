using ChatCommon.Constants;

namespace ChatCommon.Messages.Notifications
{
    public class Notification : Message
    {
        public Notification()
        {
            MessageType = MessageType.Notification;
        }

        public byte[] EncryptedMessage { get; set; }
    }
}
