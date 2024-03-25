using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;

namespace Core.Multiplayer
{
    [System.Serializable]
    public class Client
    {
        public const int MSGSIZE = Server.DATALENGTH;

        public IPAddress IP { get; }
        public Socket Socket { get; private set; }
        public IPEndPoint EndPoint { get; }

        public Client(IPAddress ip)
        {
            IP = ip;
            EndPoint = new(IP, Server.PORT);
            Socket = new Socket(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Debug.Log($"Client CREATED");
        }

        public async Task ConnectToServer()
        {
            await Socket.ConnectAsync(EndPoint);

            Debug.Log($"Client CONNECTED");
        }

        public void DisconnectFromServer()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Debug.Log($"Client DISCONNECTED");
        }

    }
}