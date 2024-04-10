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
                CheckMessage(client);
            }

            // Update timings
            _tick = ++_tick % _tickRate;
        }

        private void CheckMessage(Socket s)
        {
            if (s.Available > 0)
            {
                byte[] stream = new byte[DATALENGTH];
                int count = s.Receive(stream);
                Payload payload = new Payload(stream, count);
                int expected = payload.Length + Payload.HEADERLENGTH;
                if (count != expected)
                {
                    byte[] stream2 = new byte[expected];
                    Array.Copy(stream, stream2, count);
                    s.Receive(stream2, count, expected - count, SocketFlags.None);
                }

                TextMessage msg = new(payload);
                Debug.Log("Client message: " + msg.Message);
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

                //clientSocket.Send(new byte[2] { (byte)_tickRate, (byte)_tick }, 2, SocketFlags.None);
                //Debug.Log("Time SENT");
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
