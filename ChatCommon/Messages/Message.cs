using System;
using ChatCommon.Constants;

namespace ChatCommon.Messages
{
    public abstract class Message
    {
        protected Message()
        {
            Id = Guid.NewGuid();
        }
        
        public Guid Id { get; set; }

        public MessageType MessageType { get; set; }

        public string Sender { get; set; }
    }
}
