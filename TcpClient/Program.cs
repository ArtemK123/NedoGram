using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace ChatClient
{
    class Program
    {
        private const string Host = "127.0.0.1";
        private const int ServerPort = 8888;

        static string userName;
        static TcpClient tcpClient;
        static NetworkStream stream;
        static readonly Encoding encoding = new UnicodeEncoding(false, true, true);

        static void Main(string[] args)
        {
            Console.Write("Welcome to chat. Please, enter your name: ");
            userName = Console.ReadLine();
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(Host, ServerPort);
                stream = tcpClient.GetStream();

                string message = userName;
                byte[] data = encoding.GetBytes(message);
                stream.Write(data, 0, data.Length);

                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();
                Console.WriteLine($"Hi, {userName}");
                SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        static void SendMessage()
        {
            Console.WriteLine("Write your message: ");

            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = encoding.GetBytes(message);
                AesManaged aesManaged = new AesManaged();
                byte[] encryptedData = EncryptStringToBytes_Aes(data, aesManaged.Key, aesManaged.IV);
                aesManaged.Dispose();
                stream.Write(encryptedData, 0, encryptedData.Length);
            }
        }

        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; 
                    StringBuilder messageBuilder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        messageBuilder.Append(encoding.GetString(data, 0, bytes));
                    } while (stream.DataAvailable);


                    AesManaged aesManaged = new AesManaged();
                    string decryptedMessage = DecryptStringFromBytes_Aes(encoding.GetBytes(messageBuilder.ToString()), aesManaged.Key, aesManaged.IV);

                    Console.WriteLine(decryptedMessage); 
                }
                catch
                {
                    Console.WriteLine("Connection lost!");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        static byte[] EncryptStringToBytes_Aes(byte[] plainText, byte[] key, byte[] iv)
        {
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
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

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        static void Disconnect()
        {
            stream?.Close();
            tcpClient?.Close();
            Environment.Exit(0);
        }
    }
};