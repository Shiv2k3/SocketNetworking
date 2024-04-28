using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Multiplayer.DataTransmission
{
    /// <summary>
    /// Repersents a base header-only transmission without any data transmission, inherieting classes should simply wrap over Data
    /// </summary>
    public class Transmission
    {
        /// <summary>
        /// Map of Transmission type to index
        /// </summary>
        private readonly Dictionary<Type, int> TransmissionMap = Assembly.GetAssembly(typeof(Transmission)).GetTypes().Where(x => x.IsClass && x.BaseType == typeof(Transmission) && !x.IsAbstract)
            .Select((value, index) => new { Key = index, Value = value }).ToDictionary(pair => pair.Value, pair => pair.Key);

        /// <summary>
        /// The number of header bytes, 2b TypeID + 2b Length
        /// </summary>
        public const int HEADERSIZE = 4;

        /// <summary>
        /// Maximum number of transmission bytes allowed
        /// </summary>
        public const int MAXBYTES = ushort.MaxValue - HEADERSIZE;

        /// <summary>
        /// The final payload
        /// </summary>
        private readonly byte[] Stream;

        /// <summary>
        /// Stream's data section
        /// </summary>
        protected readonly ArraySegment<byte> Data;

        /// <summary>
        /// Transmission type identifier
        /// </summary>
        public ushort TypeID { get => GetUshort(0, 1); protected set => SetUshort(value, 0, 1); }

        /// <summary>
        /// The number of bytes in data
        /// </summary>
        public ushort Length { get => GetUshort(2, 3); protected set => SetUshort(value, 2, 3); }

        /// <summary>
        /// Sets the bytes from ushort into Stream at index i1 & i2
        /// </summary>
        /// <param name="value">The value to set in Stream</param>
        private void SetUshort(ushort value, int i1, int i2)
        {
            Stream[i1] = (byte)(value >> 8);
            Stream[i2] = (byte)value;
        }

        /// <summary>
        /// Gets ushort from Stream by using bytes at index i1 & i2
        /// </summary>
        /// <returns></returns>
        private ushort GetUshort(int i1, int i2)
        {
            return (ushort)(Stream[i1] << 8 | Stream[i2]);
        }
        /// <summary>
        /// Create base class members
        /// </summary>
        /// <param name="dataLength">Length of data</param>
        protected Transmission(Type transmissionType, ushort dataLength)
        {
            Stream = new byte[dataLength + HEADERSIZE];
            Data = new(Stream, HEADERSIZE, dataLength);

            TypeID = (ushort)TransmissionMap[transmissionType];
            Length = dataLength;
        }

        /// <summary>
        /// Use payload header to create instance for payload intel
        /// </summary>
        /// <param name="payload"></param>
        public Transmission(byte[] header)
        {
            Stream = header;
            Data = new(Stream, HEADERSIZE, Length);
        }
        public Transmission(byte[] header, byte[] data)
        {
            Stream = header.Concat(data).ToArray();
            Data = new(Stream, HEADERSIZE, data.Length);
        }
        /// <summary>
        /// The final payload
        /// </summary>
        public byte[] Payload => Stream;
    }
}