using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChatCommon.Extensibility;

namespace ChatCommon
{
    public class StreamHandler : IStreamHandler
    {
        public byte[] Read(Stream stream, int bufferSize = 64)
        {
            var buffer = new byte[bufferSize];
            List<byte> bytes = new List<byte>();
            int read = stream.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                bytes.AddRange(buffer.Take(read));
                read = stream.Read(buffer, 0, buffer.Length);
            }

            return bytes.ToArray();
        }

        public void Write(Stream stream, byte[] bytesToWrite, int bufferSize = 64)
        {
            using (var tempStream = new MemoryStream(bytesToWrite))
            {
                var buffer = new byte[bufferSize];
                var read = tempStream.Read(buffer, 0, buffer.Length);
                while (read > 0)
                {
                    stream.Write(buffer, 0, read);
                    read = tempStream.Read(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
