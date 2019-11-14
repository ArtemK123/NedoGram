using ChatCommon;
using ChatCommon.Extensibility;
using System;
using System.Text;
using ChatCommon.Encryption;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //IEncryption encryption = new AesEncryption();
                //ICoding wrapedUnicode = new Coding(new UnicodeEncoding(false, false, true));
                //UnicodeEncoding defaultUnicode = new UnicodeEncoding(false, false, true);

                //string text = "hello world";

                //byte[] encrypted = encryption.Encrypt(text);
                //byte[] encoded = wrapedUnicode.Encode(text);

                //Console.WriteLine($"Encrypted bytes: {BytesToString(encrypted)}");
                //Console.WriteLine($"Encoded bytes: {BytesToString(encoded)}");
                //Console.WriteLine($"Encrypted then decrypted: {encryption.Decrypt(encrypted)}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        static string BytesToString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b);
            }
            return builder.ToString();
        }
    }
}
