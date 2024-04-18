using Core.Multiplayer.Data;
using Core.Util;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

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

        [SerializeField, ReadOnly] private int _tick = 0;
        [SerializeField, ReadOnly] private float _lastTickTime = 0;
        [SerializeField, ReadOnly] private float _tickLength = 0;
        [SerializeField, ReadOnly] private bool _online = false;
        [SerializeField, ReadOnly] private bool _listening = false;

        private IPEndPoint _endPoint;
        private Socket _listener;
        private List<Socket> _clients;
        private Queue<Task> _tasks;

        [Button("Create")]
        private void CreateServer()
        {
            if (Application.isEditor && !Application.isPlaying)
                Debug.LogWarning("The game isn't running, some functions may not work properly");

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

            _tickLength = 1f / _tickRate;

            Debug.Log("Server CREATED");
        }

        [Button("Open")]
        private void OpenServer()
        {
            _listener.Bind(_endPoint);
            _listener.Listen(BACKLOG);
            Online = true;
            Debug.Log("Server OPENED");
        }
        private void Update()
        {
            // Wait for next tick time
            if (!Online || _lastTickTime + _tickLength > OL.Time)
            {
                return;
            }

            // next tick starts
            _lastTickTime = OL.Time;

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

            // Connect new clients
            TryListen();

            // Read messages
            foreach (var client in _clients)
            {
                CheckSocket(client);
            }

            // Update timings
            _tick = ++_tick % _tickRate;
        }

        byte[] streamHead;
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
                        Debug.LogError("Clients shouldn't send time messages");
                        break;
                    case Payload.DataType.Text:
                        Debug.Log(new TextMessage(header).Message);
                        break;
                    case Payload.DataType.Input:
                        Debug.Log(new PlayerInput(header).Horizontal);
                        break;
                    case Payload.DataType.Transform:
                        Debug.LogError("Clients shouldn't send transfrom messages");
                        break;
                    default:
                        break;
                }
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

                await clientSocket.SendAsync(new byte[2] { (byte)_tickRate, (byte)_tick }, SocketFlags.None);
                Debug.Log("Time SENT");
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
            _tick = 0;

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
