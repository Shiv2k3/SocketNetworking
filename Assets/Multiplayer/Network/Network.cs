using Core.Multiplayer.Data;
using Core.Util;
using System.Collections.Generic;

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for sending data and distributing received data to designated network entities
    /// </summary>
    public class Network : Singleton<Network>
    {
        private readonly Queue<Payload> Transmissions = new();
        public void QueueTransmission(NetworkEntity destination, NetworkModule module, in byte[] data)
        {
            Payload trms = new(destination.ID, module.Index, data);
            Transmissions.Enqueue(trms);
        }
        private void Update()
        {
            // Read sockets
            // Distribute the received data
            // Send queued data
        }
    }
}