﻿using System.Collections.Generic;
using System.Linq;
using ChatServer.Extensibility;

namespace ChatServer.Domain
{
    internal class ChatRepository : IChatRepository
    {
        private readonly List<IChat> chats = new List<IChat>();

        public bool AddChat(IChat chat)
        {
            if (chats.Any(storedChat => storedChat.Name == chat.Name))
            {
                return false;
            }    
            
            chats.Add(chat);
            return true;
        }

        public IChat GetChat(string chatName) => chats.FirstOrDefault(storedChat => storedChat.Name == chatName);

        public IReadOnlyCollection<IChat> GetChats() => chats.ToArray();
    }
}