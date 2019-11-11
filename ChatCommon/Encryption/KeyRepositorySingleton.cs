using System.Collections.Generic;
using ChatCommon.Extensibility;

namespace ChatCommon.Encryption
{
    public class KeyRepositorySingleton : IKeyRepository
    {
        private static KeyRepositorySingleton instance;
        
        private readonly Dictionary<string, byte[]> keys = new Dictionary<string, byte[]>();

        public static KeyRepositorySingleton GetInstance()
        {
            if (instance == null)
            {
                instance = new KeyRepositorySingleton();
            }

            return instance;
        }

        public void AddOrUpdate(string user, byte[] key)
        {
            if (keys.ContainsKey(user))
            {
                keys[user] = key;
            }
            else
            {
                keys.Add(user, key);
            }
        }

        public byte[] Get(string user) => keys[user];

        private KeyRepositorySingleton()
        {
        }
    }
}
