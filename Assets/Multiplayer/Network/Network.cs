using Core.Multiplayer.Data;
using Core.Util;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for sending data and distributing received data to designated network entities
    /// </summary>
    public class Network : Singleton<Network>
    {
        private readonly Socket socket = new(SocketType.Dgram, ProtocolType.Udp);
        public void Connect(IPAddress ip, ushort port)
        {
            socket.Connect(ip, port);
        }

        private readonly Queue<Payload> Transmissions = new();
        public void EnqueueTransmission(ushort EntityID, byte ModuleIndex, byte[] data)
        {
            Payload trms = new(EntityID, ModuleIndex, data);
            Transmissions.Enqueue(trms);
        }

        private void Update()
        {
            // guard !connected
            if (!socket.Connected) return;

            // Read sockets
            // Distribute the received data
            // Send queued data
        }
    }
}