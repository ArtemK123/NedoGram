using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class GoToMainMenuRequest : Request
    {
        public GoToMainMenuRequest()
        {
            Action = ClientAction.GoToMainMenu;
        }
    }
}