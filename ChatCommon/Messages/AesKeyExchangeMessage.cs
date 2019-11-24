namespace ChatCommon.Messages
{
    public class AesKeyExchangeMessage : Message
    {
        public AesKeyExchangeMessage(byte[] key, byte[] iv, string sender) 
            : base(sender)
        {
            Key = key;
            IV = iv;
        }

        public AesKeyExchangeMessage()
        {
        }

        public byte[] Key { get; set; }

        public byte[] IV { get; set; }
    }
}