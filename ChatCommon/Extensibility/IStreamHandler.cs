using System.IO;

namespace ChatCommon.Extensibility
{
    public interface IStreamHandler
    {
        byte[] Read(Stream stream, int bufferSize = 64);

        void Write(Stream stream, byte[] bytesToWrite, int bufferSize = 64);
    }
}
