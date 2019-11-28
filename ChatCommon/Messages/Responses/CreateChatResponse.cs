namespace ChatCommon.Messages.Responses
{
    public class CreateChatResponse : Response
    {
        public CreateChatResponse() : base("server") { }

        public string ChatName { get; set; }

        public byte[] Key { get; set; }
    }
}
