using System;
using System.Collections.Generic;
using System.Linq;
using ChatServer.Extensibility;

namespace ChatServer.Domain
{
    internal class ChatRepository : IChatRepository
    {
        private readonly HashSet<IChat> chats = new HashSet<IChat>();

        public bool AddChat(IChat chat)
        {
            try
            {
                chats.Add(chat);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public IChat GetChat(string chatName) => chats.FirstOrDefault(storedChat => storedChat.Name == chatName);

        public IReadOnlyCollection<IChat> GetChats() => chats.ToArray();
    }
}
