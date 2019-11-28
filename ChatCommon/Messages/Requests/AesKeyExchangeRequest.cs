using ChatCommon.Constants;
using ChatCommon.Messages.Requests;

namespace ChatCommon.Messages
{
    public class AesKeyExchangeRequest : Request
    {
        public AesKeyExchangeRequest(byte[] key, byte[] iv, string sender) 
            : base(sender)
        {
            Key = key;
            IV = iv;
        }

        public AesKeyExchangeRequest()
        {
        }

        public byte[] Key { get; set; }

        public byte[] IV { get; set; }
    }
}