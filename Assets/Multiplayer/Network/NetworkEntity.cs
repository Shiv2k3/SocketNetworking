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

        public ushort ID { get; private set; }

        public void InitEntity(in ushort EntityID) => ID = EntityID;
    }
}