using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    internal class User
    {
        public string Name { get; set; }

        public string Password { get; set; }

        public byte[] PublicKey { get; set; }
    }
}
