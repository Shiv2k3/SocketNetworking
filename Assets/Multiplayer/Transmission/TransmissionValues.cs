using System;
using System.Collections.Generic;

namespace Core.Multiplayer.DataTransmission
{
    public partial class Transmission
    {
        public enum Types
        {
            // Server side types
            HostRequest,
            Reply,
            Query,

            // Client side types
            Module,
        }

        /// <summary>
        /// Map of index to Transmission type
        /// </summary>
        public static readonly Dictionary<ushort, Type> IndexTransmission = new()
        {
            {0, typeof(HostRequest) },
            {1, typeof(Reply) },
            {2, typeof(LobbyQuery) },
            {3, typeof(ModuleTransmission) },
        };

        /// <summary>
        /// Map of Transmission type to index
        /// </summary>
        public static readonly Dictionary<Type, ushort> TransmissionIndex = new()
        {
            {IndexTransmission[0], 0 },
            {IndexTransmission[1], 1 },
            {IndexTransmission[2], 2 },
            {IndexTransmission[3], 3 },
        };

    }

}