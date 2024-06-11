using Core.Util;
using System.Net.Sockets;
using System.Net;
using System;
using OpenLobby.Utility.Network;
using UnityEngine;
using OpenLobby.Utility.Transmissions;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Core.Multiplayer
{
    public class OpenLobby : Singleton<OpenLobby>
    {
        public bool IsLocal { get => _local; set => _local = value; }
        [SerializeField] private bool _local;

        public bool Online { get; private set; }
        public Client Server { get; private set; }

        public delegate void OnReceivedDelegate(Transmission t);
        public OnReceivedDelegate MessageReceivedEvent;

        public void EnqueueTransmission(Transmission t) => _queue.Enqueue(t);
        private Queue<Transmission> _queue;

        [Button("Retry Connection"), HideIf("@Online")]
        protected override async void SingletonAwakened()
        {
            if (Online) return;

            base.SingletonAwakened();
            Debug.Log("Connecting to OpenLobby");

            // Get ip info
            string ip = Environment.GetEnvironmentVariable("OPENLOBBYIP", EnvironmentVariableTarget.User);
            int port = int.Parse(Environment.GetEnvironmentVariable("OPENLOBBYPORT", EnvironmentVariableTarget.User));
            var address = IsLocal ? IPAddress.Loopback : IPAddress.Parse(ip);

            // Create socket
            Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            var lep = new IPEndPoint(IPAddress.Any, 0);
            var rep = new IPEndPoint(address, port);

            // Connect
            socket.Bind(lep);
            await socket.ConnectAsync(rep);

            // Init
            Server = new(socket);
            _queue = new();

            // Exit
            Online = true;
            Debug.Log("Successfully connected to OpenLobby");
        }

        private void Update()
        {
            if (!Online) return;

            // Receive transmissions
            (bool success, Transmission trms) = Server.TryGetTransmission();
            while (success)
            {
                MessageReceivedEvent?.Invoke(trms);
                (success, trms) = Server.TryGetTransmission();
                Debug.Log("Received OpenLobby trms");
            }

            // Send transmissions
            while (_queue.Count != 0)
            {
                var t = _queue.Dequeue();
                Server.Send(t.Payload);
                Debug.Log("Sent OpenLobby trms");
            }
        }

        protected override void SingletonDestroyed()
        {
            base.SingletonDestroyed();
            if (!Online) return;

            Server.Disconnect();
            Online = false;
            Debug.Log("OpenLobby has been disconnected");
        }
    }
}
