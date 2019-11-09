using ChatCommon;
using ChatCommon.Extensibility;
using System;
using System.Text;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //IEncryption encryption = new AesEncryption();
            //ICoding unicode = new Coding(new UnicodeEncoding(false, false, true));
            //string text = "hello world";

            //byte[] encrypted = encryption.Encrypt(text);
            //byte[] encoded = unicode.Encode(text);

            //StringBuilder encryptedBuilder = new StringBuilder();
            //StringBuilder encodedBuilder = new StringBuilder();

            //foreach(var b in encrypted)
            //{
            //    encryptedBuilder.Append(b);
            //}
            //foreach(var b in encoded)
            //{
            //    encodedBuilder.Append(b);
            //}

            //Console.WriteLine($"Encrypted bytes: {encryptedBuilder.ToString()}");
            //Console.WriteLine($"Encoded bytes: {encodedBuilder.ToString()}");

            Console.ReadKey();
        }
    }
}
