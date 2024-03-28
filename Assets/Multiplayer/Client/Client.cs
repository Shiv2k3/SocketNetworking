using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Text;

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

            Debug.Log($"Server CONNECTED");
        }

        public async Task SendMessage(string msg)
        {
            Data.RawMessage m = new(Encoding.UTF8.GetBytes(msg));
            await Socket.SendAsync(m.MemoryBuffer, SocketFlags.None);
        }

        public void DisconnectFromServer()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Debug.Log($"Server DISCONNECTED");
        }

    }
}