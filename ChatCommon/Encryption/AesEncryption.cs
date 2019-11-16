using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
            byte[] encrypted = new byte[plainTextBytes.Length];

            byte[] tempData = new byte[plainTextBytes.Length];

            // Create an AesManaged object
            // with the specified key and IV.

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesNative.CreateEncryptor(aesNative.Key, aesNative.IV);


            using (var input = new MemoryStream(plainTextBytes))
            using (var output = new MemoryStream())
            {
                using (var cryptStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                {
                    var buffer = new byte[64];
                    var read = input.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        cryptStream.Write(buffer, 0, read);
                        read = input.Read(buffer, 0, buffer.Length);
                    }
                    cryptStream.FlushFinalBlock();
                    encrypted = output.ToArray();
                }
            }

            // Create the streams used for encryption.
            //using (MemoryStream msEncrypt = new MemoryStream())
            //{
            //    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            //    {
            //        csEncrypt.Write(plainTextBytes);
            //    }

            //    encrypted = msEncrypt.ToArray();
            //}

            return encrypted;
        }

        public byte[] Decrypt(byte[] encryptedText)
        {
            // Declare the byte[] used to hold
            // the decrypted text.
            List<byte> decrypted = new List<byte>();
            
            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesNative.CreateDecryptor(aesNative.Key, aesNative.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(encryptedText))
            {
                using (CryptoStream cryptoStream = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    // todo: create stream reader and move this logic there
                    
                    var buffer = new byte[1024];
                    var read = cryptoStream.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        decrypted.AddRange(buffer.Take(read));
                        read = cryptoStream.Read(buffer, 0, buffer.Length);
                    }
                    cryptoStream.Flush();
                }
            }

            return decrypted.ToArray();
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
