using System.IO;
using System.Security.Cryptography;
using ChatCommon.Extensibility;

namespace ChatCommon
{
    public class AesEncryption : IEncryption
    {
        public readonly byte[] key;
        public readonly byte[] iv;

        public AesEncryption(byte[] key = null, byte[] iv = null)
        { 
            this.key = key ?? GenerateKey();
            this.iv = iv ?? GenerateIv();
        }

        public byte[] Encrypt(string plainText)
        {
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (var aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
        public byte[] Encrypt(byte[] message, ICoding coding)
        {
            return Encrypt(coding.Decode(message));
        }


        public string Decrypt(byte[] encryptedText)
        {
            // Declare the string used to hold
            // the decrypted text.
            string plainText = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (var aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(encryptedText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            return plainText;
        }

        public byte[] DecryptInBytes(byte[] encryptedText, ICoding coding)
        {
            return coding.Encode(Decrypt(encryptedText));
        }

        public static byte[] GenerateKey()
        {
            using (var aesAlg = new AesManaged())
            {
                aesAlg.GenerateKey();
                return aesAlg.Key;
            }
        }

        public static byte[] GenerateIv()
        {
            using (var aesAlg = new AesManaged())
            {
                aesAlg.GenerateIV();
                return aesAlg.IV;
            }
        }
    }
}
