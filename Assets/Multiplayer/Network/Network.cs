using Core.Multiplayer.DataTransmission;
using Core.Util;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for sending data and distributing received data to designated network entities
    /// </summary>
    public class Network : Singleton<Network>
    {
        private readonly Socket socket = new(SocketType.Dgram, ProtocolType.Udp);
        public bool IsHost { get; private set; }
        public void Connect(IPAddress ip, ushort port, bool isHost)
        {
            socket.Connect(ip, port);
            IsHost = isHost;
        }

        private async void Update()
        {
            // guard !connected
            if (!socket.Connected) return;

            // Receive transmission
            await ReceiveTransmissions();
            // Sort transmissions
            SortTransmissions();
            // Distribute the received module transmissions
            DistrubuteTransmissions();
            // Execute the received command transmissions
            ExecuteCommands();
            // Send queued data
            await SendTransmissions();
        }

        private readonly Queue<Transmission> OutTransmissions = new();
        public void EnqueueTransmission(Transmission trms) => OutTransmissions.Enqueue(trms);
        private async Task SendTransmissions()
        {
            while (OutTransmissions.Count != 0)
            {
                var trms = OutTransmissions.Dequeue();
                await socket.SendAsync(trms.Payload, SocketFlags.None);
            }
        }

        private readonly Queue<Transmission> InTransmissions = new();
        private Transmission incomingTrms;
        private async Task ReceiveTransmissions()
        {
            if (socket.Available <= Transmission.HEADERSIZE)
                return;

            // Complete previous transmission
            if (incomingTrms != null && socket.Available >= incomingTrms.Length)
            {
                await CompleteTransmission();
            }

            if (socket.Available <= Transmission.HEADERSIZE)
                return;

            // Get header of incoming transmission
            byte[] temp = new byte[Transmission.HEADERSIZE];
            await socket.ReceiveAsync(temp, SocketFlags.None);
            incomingTrms = new(temp);
            if (socket.Available >= incomingTrms.Length)
            {
                await CompleteTransmission();
            }

            async Task CompleteTransmission()
            {
                byte[] data = new byte[incomingTrms.Length];
                await socket.ReceiveAsync(data, SocketFlags.None);
                incomingTrms = new(incomingTrms.Payload, data);
                InTransmissions.Enqueue(incomingTrms);
                incomingTrms = null;
            }
        }

        private void SortTransmissions()
        {
            while (InTransmissions.Count != 0)
            {
                var trms = InTransmissions.Dequeue();
                switch (trms)
                {
                    case ModuleTransmission:
                        ModuleTransmissions.Enqueue(trms as ModuleTransmission);
                        break;
                    case CommandTransmission:
                        Commands.Enqueue(trms as CommandTransmission);
                        break;
                    default:
                        throw new("Unknown transmission type");
                }
            }
        }

        private readonly Dictionary<ushort, NetworkEntity> EntityMap = new();
        private readonly Queue<ModuleTransmission> ModuleTransmissions = new();
        private void DistrubuteTransmissions()
        {
            while (ModuleTransmissions.Count != 0)
            {
                var mTrms = ModuleTransmissions.Dequeue();
                EntityMap[mTrms.EntityID].UploadData(mTrms.ModuleIndex, mTrms.Data.ToArray());
            }
        }

        private readonly Queue<CommandTransmission> Commands = new();
        [SerializeField] private List<NetworkEntity> EntityPrefabs = new();
        private void ExecuteCommands()
        {
            while (Commands.Count != 0)
            {
                var cmd = Commands.Dequeue();
                switch (cmd.MCommand)
                {
                    case CommandTransmission.Command.Create:
                        var entity = Instantiate(EntityPrefabs[cmd.Index], transform);
                        ushort id = (ushort)EntityMap.Count;
                        entity.InitEntity(id);
                        EntityMap.Add(id, entity);
                        break;
                    case CommandTransmission.Command.Destroy:
                        Destroy(EntityMap[cmd.Index].gameObject);
                        EntityMap.Remove(cmd.Index);
                        break;
                    default:
                        break;
                }
            }
        }

    }
}