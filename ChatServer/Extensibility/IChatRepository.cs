using System.Collections.Generic;

namespace ChatServer.Extensibility
{
    internal interface IChatRepository
    {
        IReadOnlyCollection<IChat> GetChats();

        IChat GetChat(string name);

        void AddChat(IChat chat);
    }
}
