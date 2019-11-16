namespace ChatCommon
{
    public interface ICoding
    {
        string Decode(byte[] message);

        byte[] GetBytes(string message);
    }
}
