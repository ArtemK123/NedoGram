﻿using ChatCommon.Constants;

namespace ChatCommon.Messages.Requests
{
    public class CreateChatRequest : Request
    {
        public CreateChatRequest(string chatName, string sender)
            : base(sender)
        {
            ChatName = chatName;
            Action = ClientAction.CreateChat;
        }

        public CreateChatRequest()
        {
            Action = ClientAction.CreateChat;
        }

        public string ChatName { get; set; }
    }
}