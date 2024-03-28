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
using System.Text;

namespace Core.Multiplayer
{
    public class Server : Singleton<Server>
    {
        public const int MAXCLIENTS = 4;
        public const int PORT = 6969;
        public const int BACKLOG = 10;
        public const int DATALENGTH = 1024;

        public bool Online { get => _online; private set => _online = value; }
        public IPAddress IP { get; private set; }

        [SerializeField] private int _tickRate = 128;
        [SerializeField] private bool _online = false;
        [SerializeField] private bool _listening = false;

        private IPEndPoint _endPoint;
        private Socket _listener;
        private List<Socket> _clients;

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

            _endPoint = new IPEndPoint(IP, PORT);
            _listener = new Socket(IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            _clients = new(MAXCLIENTS);

            Online = true;

            Debug.Log("Server CREATED");
        }

        [Button("Open")]
        private async void OpenServer()
        {
            _listener.Bind(_endPoint);
            _listener.Listen(BACKLOG);
            Debug.Log("Server OPENED");

            float t = OL.Time;
            float tr = 1f / _tickRate;
            while (Online)
            {
                // Wait for new tick time
                if (t + tr < OL.Time)
                {
                    Debug.Log("New tick isn't avaiable during " + t);
                    continue;
                }

                // Check for disconnected clients
                var dcClients = new List<Socket>();
                foreach (var clientSock in _clients)
                {
                    if (!clientSock.IsConnected())
                        dcClients.Add(clientSock);
                }
                if (dcClients.Count > 0)
                {
                    _clients = _clients.Except(dcClients).ToList();
                    Debug.LogWarning(dcClients.Count + " Client(s) DISCONNECTED");
                }

                TryListen();
                foreach (var client in _clients)
                {
                    CheckMessage(client);
                }

                await Task.Delay(1);
                t = OL.Time;
            }

        }

        private void CheckMessage(Socket s)
        {
            if(s.Available > 0)
            {
                RawMessage m = new(DATALENGTH);
                s.Receive(m.Buffer);
                Debug.Log(Encoding.UTF8.GetString(m.Buffer));
            }
        }

        /// <summary>
        /// Tries to establish new connections
        /// </summary>
        private async void TryListen()
        {
            if (_listening) return;

            try
            {
                _listening = true;
                Socket clientSocket = await _listener.AcceptAsync();
                Debug.Log("Client CONNECTED");
                _clients.Add(clientSocket);
                _listening = false;
            }
            catch (ObjectDisposedException)
            {
                _listening = false;
            }
            catch (Exception e) { throw e; }
        }

        /// <summary>
        /// Closes the server and releases resources
        /// </summary>
        [Button("Close")]
        private void CloseServer()
        {
            if (!Online)
                throw new("Server OFFLINE");

            Online = false;

            foreach (var clientSocket in _clients)
            {
                clientSocket.Shutdown(SocketShutdown.Send);
                clientSocket.Close();
            }

            _listener.Close();
            _listening = false;

            _clients.Clear();
            _clients = null;

            Debug.Log("Server CLOSED");
        }
    }
}
