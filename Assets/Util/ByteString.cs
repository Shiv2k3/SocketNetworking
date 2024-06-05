using System;
using System.Text;

namespace Core.Util
{
    /// <summary>
    /// String encoded into an array
    /// </summary>
    public class ByteString
    {
        public const int HEADERSIZE = 1; // 1B length
        public string Value { get => Encoding.ASCII.GetString(Body); set => Encoding.ASCII.GetBytes(value, Body.AsSpan()); }
        /// <summary>
        /// The length of the stream
        /// </summary>
        public byte StreamLength { get => Stream[0]; private set => Stream[0] = value; }

        private readonly ArraySegment<byte> Stream;
        private readonly ArraySegment<byte> Body;

        /// <summary>
        /// Encodes a string into an array from an index
        /// </summary>
        /// <param name="value">The string to encode</param>
        /// <param name="body">The backstore</param>
        /// <param name="start">The starting index at the backstore</param>
        /// <exception cref="ArgumentException">Length of arr or value was invalid</exception>
        public ByteString(string value, ArraySegment<byte> body, int start)
        {
            if (value.Length > byte.MaxValue + HEADERSIZE)
                throw new ArgumentException("String length must be less than 255 + HEADERSIZE");
            if (body.Count - start < value.Length + HEADERSIZE)
                throw new ArgumentException("The array is not big enough for the encoding, don't forget to count HEADERSIZE of TString");

            Stream  = body.Slice(start, HEADERSIZE + value.Length);
            Body = Stream.Slice(HEADERSIZE, value.Length);

            StreamLength = (byte)(value.Length + HEADERSIZE);
            Value = value;
        }

        /// <summary>
        /// Reconstructs string using a backstore
        /// </summary>
        /// <param name="body">The backstore to use for decoding the string</param>
        /// <param name="start">The starting index of the encoding in arr</param>
        public ByteString(ArraySegment<byte> body, int start)
        {
            Stream = body.Slice(start, body[start]);
            Body = Stream.Slice(HEADERSIZE, StreamLength - HEADERSIZE);
        }
    }

}