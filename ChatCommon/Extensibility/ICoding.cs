namespace ChatCommon
{
    public interface ICoding
    {
        string Decode(byte[] message);

        byte[] Encode(string message);
    }
}
