using System;

namespace Core.Util
{
    public class OL
    {
        /// <summary>
        /// Sets the bytes from ushort into arr at index i1 & i2
        /// </summary>
        public static void SetUshort(ushort value, int i1, int i2, byte[] arr)
        {
            arr[i1] = (byte)(value >> 8);
            arr[i2] = (byte)value;
        }

        /// <summary>
        /// Gets ushort from arr by using bytes at index i1 & i2
        /// </summary>
        /// <returns></returns>
        public static ushort GetUshort(int i1, int i2, byte[] arr)
        {
            return (ushort)(arr[i1] << 8 | arr[i2]);
        }

        /// <summary>
        /// Sets the bytes from ushort into arr at index i1 & i2
        /// </summary>
        public static void SetUshort(ushort value, int i1, int i2, ArraySegment<byte> arr)
        {
            arr[i1] = (byte)(value >> 8);
            arr[i2] = (byte)value;
        }

        /// <summary>
        /// Gets ushort from arr by using bytes at index i1 & i2
        /// </summary>
        /// <returns></returns>
        public static ushort GetUshort(int i1, int i2, ArraySegment<byte> arr)
        {
            return (ushort)(arr[i1] << 8 | arr[i2]);
        }
    }
}