using ChatCommon.Constants;

namespace ChatCommon.Messages.Responses
{
    public class AesKeyExchangeResponse : Response
    {
        public AesKeyExchangeResponse(StatusCode code) : base(code, ClientAction.KeyExchange) { }

        public AesKeyExchangeResponse() { }
    }
}