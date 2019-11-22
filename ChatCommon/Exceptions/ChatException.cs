using System;

namespace ChatCommon.Exceptions
{
    public class ChatException : Exception
    {
        public ChatException()
        {

        }

        public ChatException(string message) : base(message)
        {

        }

    }
}
