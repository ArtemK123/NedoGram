using System;

namespace ConsoleChatClient.Domain
{
    internal class ConstantsProvider
    {
        public static readonly string WelcomeMessage = "Welcome!";

        public static readonly string LoginMenuItems = "Login menu:" + Environment.NewLine + 
                                                Environment.NewLine +
                                                "1. Sign in" + Environment.NewLine + 
                                                "2. Sign up" + Environment.NewLine + 
                                                "0. Exit";
    }
}
