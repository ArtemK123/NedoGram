using System.IO;
using System.Security.Cryptography;
using ChatCommon.Extensibility;

namespace ChatCommon.Encryption
{
    public class AesEncryption : IEncryption
    {
        private readonly AesManaged aesNative;
        private readonly IStreamHandler streamHandler;

        public AesEncryption(byte[] key = null, byte[] iv = null)
        {
            streamHandler = new StreamHandler();
            aesNative = new AesManaged();
            if (key != null)
            {
                aesNative.Key = key;
            }

            if (iv != null)
            {
                aesNative.IV = iv;
            }
        }

        public byte[] Encrypt(byte[] plainTextBytes)
        {
            ICryptoTransform encryptor = aesNative.CreateEncryptor(aesNative.Key, aesNative.IV);

            using (var output = new MemoryStream())
            {
                using (var cryptStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                {
                    streamHandler.Write(cryptStream, plainTextBytes, 64);
                    return output.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] encryptedText)
        {
            ICryptoTransform decryptor = aesNative.CreateDecryptor(aesNative.Key, aesNative.IV);

            using (MemoryStream msDecrypt = new MemoryStream(encryptedText))
            {
                using (CryptoStream cryptoStream = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    return streamHandler.Read(cryptoStream);
                }
            }
        }

        public byte[] GetKey() => aesNative.Key;

        public byte[] GetIv() => aesNative.IV;

        public void SetKey(byte[] key) => aesNative.Key = key;

        public void SetIv(byte[] iv) => aesNative.IV = iv;

        public void GenerateKey() => aesNative.GenerateKey();

        public void Dispose()
        {
            aesNative?.Dispose();
        }
    }
}
