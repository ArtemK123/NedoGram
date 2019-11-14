using System;

namespace ChatCommon.Extensibility
{
    public interface IEncryption : IDisposable
    {
        byte[] Encrypt(byte[] plainTextBytes);

        byte[] Decrypt(byte[] encryptedText);

        byte[] GetKey();
        
        void SetKey(byte[] key);

        void GenerateKey();
    }
}
