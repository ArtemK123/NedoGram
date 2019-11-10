using System;
using System.IO;
using System.Security.Cryptography;

namespace TestServer
{
    class CreateKeys
    {
        static void Create()
        {
            using (var aesAlg = new AesManaged())
            {
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();
                using (var keyWriter = new StreamWriter("D:/WpfChat/key.txt"))
                {
                    Console.WriteLine("Used Key for Encryption: " + BitConverter.ToString(aesAlg.Key));
                    keyWriter.WriteLine(BitConverter.ToString(aesAlg.Key));
                }
                using (var IvWriter = new StreamWriter("D:/WpfChat/iv.txt"))
                {
                    Console.WriteLine("Used IV for Encryption: " + BitConverter.ToString(aesAlg.IV));
                    IvWriter.WriteLine(BitConverter.ToString(aesAlg.IV));
                }
            }
        }
    }
}
