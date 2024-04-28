using Core.Multiplayer.DataTransmission;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for uploading received data from network to it's modules and vice versa
    /// </summary>
    public class NetworkEntity : MonoBehaviour
    {
        public ushort ID { get; private set; }
        public void InitEntity(in ushort EntityID) => ID = EntityID;

        private readonly List<NetworkModule> Modules = new();
        public void UploadData(byte moduleInex, byte[] data) => Modules[moduleInex].InData(data);

        [SerializeField] private int TickRate = 64;
        private void Awake()
        {
            InvokeRepeating(nameof(Tick), 0, 1f / TickRate);
        }

        private void Tick()
        {
            // Enqueue output data from modules into network queue
            for (byte i = 0; i < Modules.Count; i++)
            {
                NetworkModule module = Modules[i];
                Network.I.EnqueueTransmission(new ModuleTransmission(ID, i, module.OutData()));
            }
        }

    }
}