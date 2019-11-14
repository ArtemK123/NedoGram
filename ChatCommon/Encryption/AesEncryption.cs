using System.IO;
using System.Security.Cryptography;
using ChatCommon.Extensibility;

namespace ChatCommon.Encryption
{
    public class AesEncryption : IEncryption
    {
        private readonly AesManaged aesNative;

        public AesEncryption(byte[] key = null, byte[] iv = null)
        {
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
            byte[] encrypted = null;

            // Create an AesManaged object
            // with the specified key and IV.
        
            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesNative.CreateEncryptor(aesNative.Key, aesNative.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainTextBytes);
                }
                // Return the encrypted bytes from the memory stream.

                encrypted = msEncrypt.ToArray();
            }

            return encrypted;
        }

        public byte[] Decrypt(byte[] encryptedText)
        {
            // Declare the byte[] used to hold
            // the decrypted text.
            byte[] decrypted = null;

            // Create an AesManaged object
            // with the specified key and IV.
            {
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesNative.CreateDecryptor(aesNative.Key, aesNative.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(encryptedText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        csDecrypt.Read(encryptedText, 0, encryptedText.Length);
                    }

                    decrypted = msDecrypt.ToArray();
                }
            }
            return decrypted;
        }


        public byte[] GetKey() => aesNative.Key;

        public void SetKey(byte[] key) => aesNative.Key = key;

        public void GenerateKey() => aesNative.GenerateKey();

        public void Dispose()
        {
            aesNative?.Dispose();
        }
    }
}
