using System.Net.Sockets;

namespace Core.Multiplayer.Data
{
    public class RawMessage : SocketAsyncEventArgs
    {
        public RawMessage(int count)
        {
            SetBuffer(new byte[count], 0, count);
        }
        public RawMessage(byte[] data)
        {
            SetBuffer(data);
        }
    }
}