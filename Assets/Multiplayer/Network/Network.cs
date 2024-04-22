using Core.Util;
using System;
using System.Collections.Generic;

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for sending data and distributing received data to designated network entities
    /// </summary>
    public class Network : Singleton<Network>
    {
        private class Transmission
        {
            public uint EntityID;
            public uint ModuleIndex;
            public byte[] Data;

            public Transmission(uint entityID, uint moduleIndex, byte[] data)
            {
                EntityID = entityID;
                ModuleIndex = moduleIndex;
                Data = data ?? throw new ArgumentNullException(nameof(data));
            }
        }
        private Queue<Transmission> Transmissions = new();
        public void QueueTransmission(NetworkEntity destination, NetworkModule module, in byte[] data)
        {
            Transmission trms = new Transmission(destination.ID, module.Index, data);
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