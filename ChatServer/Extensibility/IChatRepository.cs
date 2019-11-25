using System.Collections.Generic;

namespace ChatServer.Extensibility
{
    internal interface IChatRepository
    {
        IReadOnlyCollection<IChat> GetChats();

        IChat GetChat(string chatName);

        bool AddChat(IChat chat);
    }
}
