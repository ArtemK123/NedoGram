using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using ChatCommon.Extensibility;

namespace ChatCommon
{
    public class TcpClientWrapper : ITcpWrapper
    {
        private readonly TcpClient tcpClient;
        private readonly NetworkStream stream;

        public TcpClientWrapper(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            stream = tcpClient.GetStream();
        }

        public TcpClientWrapper(string host, int port)
            : this(new TcpClient(host, port))
        {

        }

        public byte[] GetMessage()
        {
            byte[] buffer = new byte[64];
            List<byte> message = new List<byte>();
            int bytes = 0;
            do
            {
                bytes = stream.Read(buffer, 0, buffer.Length);
                message.AddRange(buffer.Take(bytes));
            } while (stream.DataAvailable);

            return message.ToArray();
            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    stream.CopyTo(memoryStream);
            //    return memoryStream.ToArray();
            //}
        }

        public void Send(byte[] message)
        {
            stream.Write(message, 0, message.Length);
        }

        public void Dispose()
        {
            stream?.Close();
            tcpClient?.Close();
        }
    }
}
