using System;
using System.Text;
using ChatCommon;

namespace ConsoleChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Write your name");
                string userName = Console.ReadLine();
                IChatClient client = new ChatClient(
                    new TcpClientWrapper("127.0.0.1", 8888),
                    userName,
                    new AesEncryption(
                        GetBytes(
                            "61-84-54-FA-46-F0-2E-FC-7A-AE-B6-6A-3E-A5-A3-67-4C-FB-6C-08-2F-55-AD-85-C2-50-33-FB-52-AB-EC-D0"),
                        GetBytes("B6-B9-EC-D6-C6-A0-EC-A8-1D-C7-B7-61-73-E0-A8-68")),
                    new Coding(new UnicodeEncoding(false, false, true)));

                client.Listen();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        static byte[] GetBytes(string dashSeparetedKey)
        {
            string[] symbols = dashSeparetedKey.Split('-');

            byte[] array = new byte[symbols.Length];
            for (int i = 0; i < symbols.Length; i++)
            {
                array[i] = Convert.ToByte(symbols[i], 16);
            }

            return array;
        }
    }
};