using System.Collections.Generic;

namespace Core.Multiplayer
{
    /// <summary>
    /// Repersents a single networked behaviour
    /// </summary>
    public abstract class NetworkModule
    {
        /// <summary>
        /// Queue with incoming data
        /// </summary>
        protected readonly Queue<byte[]> Incoming = new();

        /// <summary>
        /// Accepts incoming data
        /// </summary>
        /// <param name="data">The data designated for this module</param>
        public void InData(in byte[] data)
        {
            Incoming.Enqueue(data);
        }

        /// <summary>
        /// Queue with outoging data
        /// </summary>
        protected readonly Queue<byte[]> Outgoing = new();

        /// <summary>
        /// The outgoing data
        /// </summary>
        public byte[] OutData()
        {
            return Outgoing.Dequeue();
        }

        /// <summary>
        /// Execute module behaviour
        /// </summary>
        public void Modulate()
        {
            if (Network.I.IsHost)
            {
                ServerrModulate();
                ClientModulate();
            }
            else
            {
                ClientModulate();
            }
        }

        /// <summary>
        /// Execute server side behaviour
        /// </summary>
        protected abstract void ServerrModulate();

        /// <summary>
        /// Execute client side behaviour
        /// </summary>
        protected abstract void ClientModulate();
    }
}