using System;

namespace Core.Multiplayer.Data
{
    public struct Payload
    {
        /// <summary>
        /// The number of header bytes, 2b EntityID + 1b ModuleIndex + 2b Length
        /// </summary>
        public const int HEADERSIZE = 5;
        /// <summary>
        /// Maximum number of transmission bytes allowed
        /// </summary>
        public const int MAXBYTES = ushort.MaxValue;

        /// <summary>
        /// The ID of the entity
        /// </summary>
        public readonly ushort EntityID { get => (ushort)((Stream[0] << 8) | Stream[1]); }

        /// <summary>
        /// The module's index
        /// </summary>
        public readonly byte ModuleIndex { get => Stream[2]; }

        /// <summary>
        /// The number of data bytes
        /// </summary>
        public readonly ushort Length { get => (ushort)((Stream[3] << 8) | Stream[4]); }

        /// <summary>
        /// The data plus the header
        /// </summary>
        public readonly byte[] Stream;

        /// <summary>
        /// The data without a header
        /// <summary>
        public ArraySegment<byte> Data { get; private set; }

        /// <summary>
        /// Creates payload from data
        /// </summary>
        /// <param name="EntityID"> Type of data </param>
        /// <param name="data"> The data </param>
        public Payload(ushort EntityID, byte ModuleIndex, byte[] data)
        {
            if (data.Length > MAXBYTES)
                throw new("Data is too large");

            // create stream
            Stream = new byte[data.Length + HEADERSIZE];

            // Setup header
            Stream[0] = (byte)(EntityID << 8 & ushort.MaxValue << 8);
            Stream[1] = (byte)(EntityID & ushort.MaxValue >> 8);

            Stream[2] = ModuleIndex;

            Stream[3] = (byte)(data.Length << 8 & ushort.MaxValue << 8);
            Stream[4] = (byte)(data.Length & ushort.MaxValue >> 8);

            // populate data
            Array.Copy(data, 0, Stream, HEADERSIZE, data.Length);
            Data = new ArraySegment<byte>(Stream, HEADERSIZE, Stream.Length - HEADERSIZE);
        }
    }
}