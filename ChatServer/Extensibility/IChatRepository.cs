using System.Collections.Generic;

namespace ChatServer.Extensibility
{
    internal interface IChatRepository
    {
        void AddChat(IChat chat);

        IReadOnlyCollection<IChat> GetChats();

        IChat GetChat(string chatName);
    }
}
