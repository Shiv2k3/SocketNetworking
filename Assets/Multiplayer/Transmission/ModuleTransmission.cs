using Core.Util;
using System;

namespace Core.Multiplayer.DataTransmission
{
    /// <summary>
    /// Repersents a transmission between two networked modules of the same type
    /// </summary>
    public class ModuleTransmission : Transmission
    {
        // 2B ID
        private new const int HEADERSIZE = 2;

        /// <summary>
        /// The ID module
        /// </summary>
        public ushort ModuleID { get => OL.GetUshort(0, 1, Stream); set => OL.SetUshort(value, 0, 1, Stream); }

        /// <summary>
        /// Module stream
        /// </summary>
        private readonly ArraySegment<byte> Stream;

        /// <summary>
        /// Actual module data
        /// </summary>
        public readonly ArraySegment<byte> Data;

        /// <summary>
        /// Constructs tranmission for data
        /// </summary>
        /// <param name="ID">Module's ID</param>
        /// <param name="data">The data being transmitted</param>
        public ModuleTransmission(ushort ID, ArraySegment<byte> data) : base(typeof(ModuleTransmission), (ushort)(data.Count + HEADERSIZE))
        {
            if (data.Count + HEADERSIZE > MAXBYTES)
                throw new("Data is too large");

            // Setup containers
            Stream = base.Body;
            Data = Stream.Slice(HEADERSIZE);

            // Setup data
            ModuleID = ID;
            for (int i = 0; i < data.Count; i++)
            {
                Data[i] = data[i];
            }
        }

        /// <summary>
        /// Constructs module transmission using transmission
        /// </summary>
        /// <param name="trms">The base transmission</param>
        public ModuleTransmission(Transmission trms) : base(trms)
        {
            Stream = base.Body;
            Data = Stream.Slice(HEADERSIZE);
        }
        
    }
}