using System;
using System.Text;

namespace ChatCommon
{
    public class Coding : ICoding
    {
        public readonly Encoding encoding;

        public Coding(Encoding encoding)
        {
            this.encoding = encoding;
        }
        public string Decode(byte[] message)
        {
            string decoded = String.Empty;
            try
            {
                decoded = encoding.GetString(message);
            }
            catch (DecoderFallbackException)
            {
                Console.WriteLine($"Error while decoding message: ${message}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            return decoded;
        }

        public byte[] GetBytes(string message)
        {
            byte[] encoded = new byte[0];
            try
            {
                encoded = encoding.GetBytes(message);
            }
            catch (EncoderFallbackException)
            {
                Console.WriteLine($"Error while encoding message: ${message}");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            return encoded;
        }
    }
}
