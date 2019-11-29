namespace ChatCommon.Messages.Responses
{
    public class LoginResponse : Response
    {
        public LoginResponse() 
        {
        }

        public string UserName { get; set; }
    }
}