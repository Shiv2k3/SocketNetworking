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
        public bool IsLocal { get => _local; private set => _local = value; }
        [SerializeField] private bool _local;

        public bool Online { get => _online; private set => _online = value; }
        [SerializeField] private bool _online;
        
        private Client Server { get; set; }

        public delegate void OnReceivedDelegate(Transmission t);
        public OnReceivedDelegate MessageReceivedEvent;

        public void EnqueueTransmission(Transmission t) => _queue.Enqueue(t);
        private Queue<Transmission> _queue;

        [Button("Connect"), HideIf("@Online")]
        protected override void SingletonAwakened()
        {
            if (Online) return;

            base.SingletonAwakened();
            Debug.Log("Connecting to OpenLobby");

            // Get ip info
            string ip = Environment.GetEnvironmentVariable("OPENLOBBYIP", EnvironmentVariableTarget.User);
            int port = int.Parse(Environment.GetEnvironmentVariable("OPENLOBBYPORT", EnvironmentVariableTarget.User));
            var address = IsLocal ? IPAddress.Loopback : IPAddress.Parse(ip);

            // Create socket
            var lep = new IPEndPoint(IPAddress.Any, port);
            var rep = new IPEndPoint(address, port);
            Server = new Client(lep, rep);
            
            // Init
            _queue = new();
            Online = true;

            // Exit
            Debug.Log("Successfully connected to OpenLobby");
        }

        private void Update()
        {
            if (!Online) return;

            // Receive transmissions
            (bool success, Transmission trms) = Server.TryGetTransmission();
            while (success)
            {
                if (MessageReceivedEvent != null)
                {
                    MessageReceivedEvent.Invoke(trms);
                    (success, trms) = Server.TryGetTransmission();
                }
            }

            // Send transmissions
            while (_queue.Count != 0)
            {
                var t = _queue.Dequeue();
                Server.Send(t.Payload);
            }
        }

        [Button("Disconnect"), HideIf("@!Online")]
        protected override void SingletonDestroyed()
        {
            base.SingletonDestroyed();
            if (!Online) return;

            Server.Disconnect();
            Server = null;
            _queue = null;
            Online = false;

            Debug.Log("OpenLobby has been disconnected");
        }
    }
}
