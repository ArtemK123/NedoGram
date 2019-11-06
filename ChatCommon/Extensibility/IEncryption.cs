namespace ChatCommon.Extensibility
{
    public interface IEncryption
    {
        byte[] Encrypt(string plainText);

        byte[] Encrypt(byte[] message, ICoding coding);

        byte[] DecryptInBytes(byte[] encryptedText, ICoding coding);

        string Decrypt(byte[] encryptedText);
    }
}
