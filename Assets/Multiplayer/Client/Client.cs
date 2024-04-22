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
        public int CurrentTick { get; private set; }
        public bool Connected => Socket.Connected;
        
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

            Debug.Log($"Server SYNCING");
            Payload load = new(Payload.DataType.Time, new byte[2]);
            await Socket.ReceiveAsync(load.Data, SocketFlags.None);
            TimeMessage tm = new(load);
            Debug.Log("Tick rate: " + tm.TickRate + ", current tick: " + tm.CurrentTick);

            Debug.Log("Server SYNCED");
        }
        public async Task SendMessage(Payload payload)
        {
            await Socket.SendAsync(payload.Stream, SocketFlags.None);
        }
        public async Task SendMessage(string msg)
        {
            TextMessage message = new(msg);
            await Socket.SendAsync(message.payload.Stream, SocketFlags.None);
        }

        private byte[] streamHead;
        private void CheckSocket(Socket s)
        {
            // Exit if nothing to read
            if (s.Available < Payload.HEADERLENGTH)
                return;

            // Complete previous session
            if (streamHead != null)
            {
                Payload header = new Payload(streamHead);
                byte[] data = new byte[header.Length];
                s.Receive(data);
                header.UpdateData(data);

                // Reset header
                streamHead = null;
            }

            // Read new data
            if (s.Available >= Payload.HEADERLENGTH)
            {
                // get header
                streamHead = new byte[Payload.HEADERLENGTH];
                int count = s.Receive(streamHead);
                Payload header = new Payload(streamHead);

                // Get data if all of it is avaiable
                if (s.Available >= header.Length)
                {
                    // get the rest of the data
                    byte[] data = new byte[header.Length];
                    s.Receive(data);
                    header.UpdateData(data);

                    // Reset header
                    streamHead = null;
                }
                else
                {
                    // Get it next time
                    return;
                }

                // Execute message
                switch (header.Type)
                {
                    case Payload.DataType.Time:
                        TimeMessage tm = new(header);
                        TickRate = tm.TickRate;
                        CurrentTick = tm.CurrentTick;
                        break;
                    case Payload.DataType.Text:
                        Debug.Log(new TextMessage(header).Message);
                        break;
                    case Payload.DataType.Input:
                        Debug.LogError("Server shouldn't send input messages");
                        break;
                    case Payload.DataType.Disconnect:
                        DisconnectFromServer();
                        Debug.LogWarning("Client DISCONNECTED");
                        break;
                    default:
                        break;
                }
            }
        }

        public void DisconnectFromServer()
        {
            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Debug.Log($"Server DISCONNECTED");
        }

    }
}