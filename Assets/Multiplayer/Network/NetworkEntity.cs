using System.Collections.Generic;
using UnityEngine;

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for holding networked modules
    /// </summary>
    public class NetworkEntity : MonoBehaviour
    {
        private List<NetworkModule> Modules = new();

        public uint ID { get; private set; }

        public void InitEntity(in uint EntityID) => ID = EntityID;
    }
}