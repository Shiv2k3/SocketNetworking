using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using Core.Multiplayer.Data;

namespace Core.Multiplayer
{
    public class Client
    {
        public const int MSGSIZE = Server.DATALENGTH;

        public IPAddress IP { get; }
        public Socket Socket { get; private set; }
        public IPEndPoint EndPoint { get; }
        public int TickRate { get; private set; }

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

            //Debug.Log($"Server SYNCING");
            //Payload load = new(Payload.DataType.Time, new byte[2]);
            //Socket.Receive(load.data, 2, SocketFlags.None);
            //load.DecodeTime(out var tickRate, out var currentTick);
            //Debug.Log("Tick rate: " + tickRate + ", current tick: " + currentTick);

            //Debug.Log("Server SYNCED");
        }

        public async Task SendMessage(string msg)
        {
            TextMessage message = new(msg);
            await Socket.SendAsync(message.payload.Stream, SocketFlags.None);
        }

        public void DisconnectFromServer()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Debug.Log($"Server DISCONNECTED");
        }

    }
}