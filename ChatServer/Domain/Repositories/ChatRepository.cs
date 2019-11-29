using System.Collections.Generic;
using System.Linq;
using ChatServer.Domain.Exceptions;
using ChatServer.Extensibility;

namespace ChatServer.Domain.Repositories
{
    internal class ChatRepository : IChatRepository
    {
        private readonly List<IChat> chats = new List<IChat>();

        public void AddChat(IChat chat)
        {
            if (chats.Any(storedChat => storedChat.Name == chat.Name))
            {
                throw new ChatAlreadyExistsException(chat.Name);
            }    
            
            chats.Add(chat);
        }

        public IChat GetChat(string chatName) => chats.FirstOrDefault(storedChat => storedChat.Name == chatName);

        public IReadOnlyCollection<IChat> GetChats() => chats.ToArray();
    }
}
