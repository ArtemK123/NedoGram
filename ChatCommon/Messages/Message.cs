using System;

namespace ChatCommon.Messages
{
    public abstract class Message
    {
        protected Message()
        {
        }

        protected Message(string sender)
        {
            Id = Guid.NewGuid();
            Sender = sender;
        }

        public Guid Id { get; set; }

        public string Sender { get; set; }
    }
}
