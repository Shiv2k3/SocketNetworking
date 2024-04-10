using System;

namespace Core.Multiplayer.Data
{
    public readonly unsafe struct Payload
    {
        public const int HEADERLENGTH = 3;
        public const int MAXDATALENGTH = int.MaxValue >> 16;
        public enum DataType
        {
            /// <summary>
            /// Server time
            /// </summary>
            Time,

            /// <summary>
            /// Text message
            /// </summary>
            Text,

            /// <summary>
            /// Player input
            /// </summary>
            Input,

            /// <summary>
            /// Player transform
            /// </summary>
            Transform
        }

        /// <summary>
        /// Repersents <see cref="Type"/> of data
        /// </summary>
        public readonly DataType Type { get => (DataType)Stream[0]; }

        /// <summary>
        /// The length of the data
        /// </summary>
        public readonly int Length { get => Stream[1] | (Stream[2] << 8); }

        /// <summary>
        /// The data stream
        /// </summary>
        public readonly byte[] Stream;

        /// <summary>
        /// The payload
        /// <summary>
        public readonly byte[] Data;

        /// <summary>
        /// Creates payload from data
        /// </summary>
        /// <param name="type"> Type of data </param>
        /// <param name="data"> The data </param>
        public Payload(DataType type, byte[] data)
        {
            if (data.Length > MAXDATALENGTH)
                throw new("Data is too large");

            // create stream
            Stream = new byte[data.Length + HEADERLENGTH];
            Stream[0] = (byte)type;
            Stream[1] = (byte)(data.Length & byte.MaxValue);
            Stream[2] = (byte)(data.Length >> 8 & byte.MaxValue);

            // populate data
            Array.Copy(data, 0, Stream, HEADERLENGTH, data.Length);
            Data = new ArraySegment<byte>(Stream, HEADERLENGTH, Stream.Length - HEADERLENGTH).ToArray();
        }
        /// <summary>
        /// Creates payload from data
        /// </summary>
        /// <param name="type"> Type of data </param>
        /// <param name="data"> The data </param>
        /// <param name="count"> Amount to copy </param>
        public Payload(DataType type, byte[] data, int count)
        {
            if (data.Length > MAXDATALENGTH)
                throw new("Data is too large");

            // create stream
            Stream = new byte[count + HEADERLENGTH];
            Stream[0] = (byte)type;
            Stream[1] = (byte)(count & byte.MaxValue);
            Stream[2] = (byte)(count & (byte.MaxValue << 8));

            // populate data
            Array.Copy(data, 0, Stream, HEADERLENGTH, count);
            Data = new ArraySegment<byte>(Stream, HEADERLENGTH, Stream.Length - HEADERLENGTH).ToArray();
        }

        /// <summary>
        /// Extracts payload from stream
        /// <summary>
        public Payload(byte[] stream)
        {
            // Extract stream
            Stream = new byte[stream.Length];
            Array.Copy(stream, Stream, stream.Length);
            Data = new ArraySegment<byte>(Stream, HEADERLENGTH, Stream.Length - HEADERLENGTH).ToArray();
        }
        /// <summary>
        /// Extracts payload from stream using count
        /// </summary>
        /// <param name="stream">the payload stream</param>
        /// <param name="count">number of bytes to use</param>
        public Payload(byte[] stream, int count)
        {
            // Extract stream
            Stream = new byte[count];
            Array.Copy(stream, Stream, count);
            Data = new ArraySegment<byte>(Stream, HEADERLENGTH, count - HEADERLENGTH).ToArray();
        }
    }

    public class PayloadData
    {
        public Payload payload;
    }
}
