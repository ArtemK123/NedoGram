using System;

namespace ChatCommon.Extensibility
{
    public interface ITcpWrapper : IDisposable
    {
        byte[] GetMessage();

        void Send(byte[] message);
    }
}
