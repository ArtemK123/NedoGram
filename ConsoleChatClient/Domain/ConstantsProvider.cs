using System;

namespace ConsoleChatClient.Domain
{
    internal class ConstantsProvider
    {
        public static readonly string ProgramName = "Nedo Gram v1.0";

        public static readonly string WelcomeMessage = "Welcome!";

        public static readonly string LoginMenuTitle = "Login menu";

        public static readonly string LoginMenuItems =
            "1. Sign in" + Environment.NewLine +
            "2. Sign up" + Environment.NewLine +
            "0. Exit";

        public static readonly string MainMenu =
            "1. Show all chats" + Environment.NewLine +
            "2. Create new chat" + Environment.NewLine +
            "3. Connect to chat" + Environment.NewLine +
            "0. Exit";

        public static readonly string ChatMenu =
            "1. Send message" + Environment.NewLine +
            "2. Show all users in the chat" + Environment.NewLine +
            "0. Go to the main menu";
    }
}
