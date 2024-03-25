using Core.Multiplayer.Data;
using Core.Util;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace Core.Multiplayer
{
    public class Server : Singleton<Server>
    {
        public const int MAXCLIENTS = 4;
        public const int PORT = 6969;
        public const int BACKLOG = 10;
        public const int DATALENGTH = 1024;

        [SerializeField] private int TickRate = 128;

        public bool Online { get; private set; }
        public IPAddress IP { get; private set; }

        private IPEndPoint m_endPoint;
        private Socket m_socket;

        private List<Socket> Clients;
        private RawMessage Data;

        [Button("Create")]
        private void CreateServer()
        {
            if (Online)
            {
                try
                {
                    CloseServer();
                }
                catch { }
            }

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IP = ipHost.AddressList[0];

            m_endPoint = new IPEndPoint(IP, PORT);
            m_socket = new Socket(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            Clients = new(MAXCLIENTS);

            Data = new RawMessage(DATALENGTH);

            Online = true;

            Debug.Log("Server CREATED");
        }

        [Button("Open")]
        private async void OpenServer()
        {
            m_socket.Bind(m_endPoint);
            m_socket.Listen(BACKLOG);
            Debug.Log("Server OPENED");

            float t = Time;
            float tr = 1f / TickRate;
            while (Online)
            {
                // Wait for new tick time
                if (t + tr <= Time)
                {
                    Debug.Log("New tick isn't avaiable during " + t);
                    continue;
                }

                // Check for disconnected clients
                var dcClients = new List<Socket>();
                foreach (var clientSock in Clients)
                {
                    if (!clientSock.IsConnected())
                        dcClients.Add(clientSock);
                }
                if (dcClients.Count > 0)
                {
                    Clients = Clients.Except(dcClients).ToList();
                    Debug.LogWarning(dcClients.Count + " Client(s) DISCONNECTED");
                }

                // Establish new connections
                await NewConnections();

                t = Time;
            }

        }

        private async Task NewConnections()
        {
            try
            {
                Socket clientSocket = await m_socket.AcceptAsync();
                Debug.Log("Added client");
                Clients.Add(clientSocket);
            }
            catch (ObjectDisposedException) { }
            catch (Exception e) { Debug.LogError(e); }
        }

        [Button("Close")]
        private void CloseServer()
        {
            if (!Online)
                throw new("Server OFFLINE");

            Online = false;

            foreach (var clientSocket in Clients)
            {
                clientSocket.Shutdown(SocketShutdown.Send);
                clientSocket.Close();
            }

            m_socket.Close();

            Clients.Clear();
            Clients = null;

            Data = null;

            Debug.Log("Server CLOSED");
        }

        public static float Time => DateTime.Now.Second + DateTime.Now.Millisecond / 1000f;
    }
}