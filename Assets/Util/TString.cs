using System;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Core.Util
{
    /// <summary>
    /// String encoded into an array
    /// </summary>
    public class TString
    {
        public const int HEADERSIZE = 1; // 1B length
        public string Value { get => Encoding.ASCII.GetString(Body); set => Encoding.ASCII.GetBytes(value, Body.AsSpan()); }
        public byte Length { get => Stream[0]; private set => Body[0] = value; }

        private readonly ArraySegment<byte> Stream;
        private readonly ArraySegment<byte> Body;

        /// <summary>
        /// Encodes a string into an array from an index
        /// </summary>
        /// <param name="value">The string to encode</param>
        /// <param name="arr">The backstore</param>
        /// <param name="start">The starting index at the backstore</param>
        /// <exception cref="ArgumentException">Length of arr or value was invalid</exception>
        public TString(string value, ArraySegment<byte> arr, int start)
        {
            if (value.Length > byte.MaxValue)
                throw new ArgumentException("String length must be less than 255");
            if (arr.Count - start < value.Length + HEADERSIZE)
                throw new ArgumentException("The array is not big enough for the encoding, don't forget to count HEADERSIZE of TString");

            Stream  = arr.Slice(start, HEADERSIZE + value.Length);
            Body = Stream.Slice(HEADERSIZE, value.Length);

            Stream[0] = (byte)value.Length;
            Encoding.ASCII.GetBytes(value, Body.AsSpan());
        }

        /// <summary>
        /// Reconstructs string using a backstore
        /// </summary>
        /// <param name="arr">The backstore to use for decoding the string</param>
        /// <param name="start">The starting index of the encoding in arr</param>
        public TString(ArraySegment<byte> arr, int start)
        {
            Stream = arr.Slice(start, HEADERSIZE + arr[start]);
            Body = Stream.Slice(HEADERSIZE, Length);
        }
    }

}