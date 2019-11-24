namespace ChatCommon.Messages
{
    public abstract class Message
    {
        protected Message()
        {
        }

        protected Message(string sender)
        {
            Sender = sender;
        }

        public string Sender { get; set; }
    }
}
