namespace ChatCommon.Extensibility
{
    interface IEncryption
    {
        byte[] Encrypt(string plainText);

        string Decrypt(byte[] encryptedText);
    }
}
