using System.Collections.Generic;
using ChatServer.Extensibility;

namespace ChatServer.Domain
{
    class ChatRepository : IChatRepository
    {
        public void AddChat(IChat chat)
        {
            throw new System.NotImplementedException();
        }

        public IChat GetChat(string name)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<IChat> GetChats()
        {
            throw new System.NotImplementedException();
        }
    }
}
