﻿using Core.Util;
using System;

namespace Core.Multiplayer.DataTransmission
{
    /// <summary>
    /// Repersents a base header-only transmission without any data transmission, inherieting classes should simply wrap over Data
    /// </summary>
    public partial class Transmission
    {
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
        protected readonly ArraySegment<byte> Body;

        /// <summary>
        /// Transmission type identifier
        /// </summary>
        public ushort TypeID { get => OL.GetUshort(0, 1, Stream); protected set => OL.SetUshort(value, 0, 1, Stream); }

        /// <summary>
        /// The number of bytes of data
        /// </summary>
        public ushort Length { get => OL.GetUshort(2, 3, Stream); protected set => OL.SetUshort(value, 2, 3, Stream); }

        /// <summary>
        /// Create base class members
        /// </summary>
        /// <param name="dataLength">Length of data</param>
        protected Transmission(Type transmissionType, ushort dataLength)
        {
            Stream = new byte[dataLength + HEADERSIZE];
            Body = new(Stream, HEADERSIZE, dataLength);

            TypeID = TransmissionIndex[transmissionType];
            Length = dataLength;
        }

        /// <summary>
        /// Use payload header to create instance for payload intel
        /// </summary>
        /// <param name="payload"></param>
        public Transmission(byte[] header)
        {
            Stream = header;
        }
        public Transmission(byte[] header, byte[] data)
        {
            if (header.Length != HEADERSIZE) throw new($"Incorrect header length: {header.Length}");

            Stream = new byte[HEADERSIZE + data.Length];
            for (int i = 0; i < Stream.Length; i++)
            {
                Stream[i] = i < HEADERSIZE ? header[i] : data[i - HEADERSIZE];
            }
            Body = new(Stream, HEADERSIZE, data.Length);
        }

        public Transmission(Transmission trms)
        {
            Stream = trms.Stream;
            Body = trms.Body;
        }

        /// <summary>
        /// The final payload
        /// </summary>
        public byte[] Payload { get => Stream; }
    }

}