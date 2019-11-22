using System;

namespace ConsoleChatClient
{
    public interface IChatClient : IDisposable
    {
        void Listen();
    }
}
