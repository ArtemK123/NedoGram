namespace ChatCommon.Messages.Responses
{
    public class CreateChatResponse : Response
    {
        public CreateChatResponse() { }

        public string ChatName { get; set; }

        public byte[] Key { get; set; }
    }
}
