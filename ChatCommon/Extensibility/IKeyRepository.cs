namespace ChatCommon.Extensibility
{
    public interface IKeyRepository
    {
        void AddOrUpdate(string user, byte[] key);

        byte[] Get(string user);
    }
}