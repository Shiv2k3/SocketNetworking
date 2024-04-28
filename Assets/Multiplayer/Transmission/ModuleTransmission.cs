using System;

namespace Core.Multiplayer.DataTransmission
{
    /// <summary>
    /// Repersents a transmission between two networked modules of the same type
    /// </summary>
    public class ModuleTransmission : Transmission
    {
        // 2B entityID + 1B index
        private new const int HEADERSIZE = 3;

        /// <summary>
        /// The ID of the parent entity
        /// </summary>
        public ushort EntityID { get => (ushort)((Data[0] << 8) | Data[1]); }

        /// <summary>
        /// The module's index in the parent list
        /// </summary>
        public byte ModuleIndex { get => Data[2]; }

        /// <summary>
        /// Module data
        /// </summary>
        public new readonly ArraySegment<byte> Data;

        /// <summary>
        /// Constructs tranmission for data
        /// </summary>
        /// <param name="EntityID">Module's parent entityID</param>
        /// <param name="ModuleIndex">Module index in parent's modules list</param>
        /// <param name="data">The data being transmitted</param>
        public ModuleTransmission(ushort EntityID, byte ModuleIndex, byte[] data) : base(typeof(ModuleTransmission), (ushort)(data.Length + HEADERSIZE))
        {
            if (data.Length + HEADERSIZE > MAXBYTES)
                throw new("Data is too large");

            // Setup header
            base.Data[0] = (byte)(EntityID << 8 & ushort.MaxValue << 8);
            base.Data[1] = (byte)(EntityID & ushort.MaxValue >> 8);
            base.Data[2] = ModuleIndex;

            // Set data
            Data = base.Data.Slice(HEADERSIZE);
            data.CopyTo(Data.AsSpan());
        }
    }
}