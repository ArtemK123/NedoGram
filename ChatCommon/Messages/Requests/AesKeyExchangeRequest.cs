using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class AesKeyExchangeRequest : Request
    {
        public AesKeyExchangeRequest(byte[] key, byte[] iv, string sender) 
            : base(sender)
        {
            Key = key;
            IV = iv;
            Action = ClientAction.KeyExchange;
        }

        public AesKeyExchangeRequest()
        {
        }

        public byte[] Key { get; set; }

        public byte[] IV { get; set; }
    }
}