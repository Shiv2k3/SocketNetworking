using System;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.Multiplayer.Data
{
    public readonly unsafe struct Payload
    {
        private const int HEADERLENGTH = 3;
        private const int MAXLENGTH = int.MaxValue >> 16;
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
            // check if data length is valid
            Assert.IsTrue(data.Length <= MAXLENGTH);

            // create stream
            Stream = new byte[data.Length + HEADERLENGTH];
            Stream[0] = (byte)type;
            Stream[1] = (byte)(data.Length & byte.MaxValue);
            Stream[2] = (byte)(data.Length & (byte.MaxValue << 8));

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
        public Payload(byte[] stream, int count)
        {
            // Extract stream
            Stream = new byte[count];
            Array.Copy(stream, Stream, count);
            Data = new ArraySegment<byte>(Stream, HEADERLENGTH, count - HEADERLENGTH).ToArray();
        }

        public readonly void DecodeTime(out int tickRate, out int currentTick)
        {
            // tickrate 1 byte, current tick 1 byte, 2 bytes
            const int Length = 2;
            Assert.IsTrue(Type == DataType.Time && this.Length == Length);
            
            tickRate = Data[0];
            currentTick = Data[1];
        }
        public readonly void DecodeText(out string message)
        {
            Assert.IsTrue(Type == DataType.Text);
            message = Encoding.UTF8.GetString(Data);
        }
        public readonly void DecodeInput(out Vector2Int movement, out int vertical, out bool sprint, out Vector2Int look)
        {
            // each bit repersents input for w, a, s, d, jump, crouch, shift, left, right, up, down, 11 bits or 2 bytes
            const int Length = 2;
            Assert.IsTrue(Type == DataType.Input && this.Length == Length);

            int w      = Data[0] & 1;
            int a      = Data[0] & 2;
            int s      = Data[0] & 4;
            int d      = Data[0] & 8;
            int jump   = Data[0] & 16;
            int crouch = Data[0] & 32;
            int shift  = Data[0] & 64;
            int left   = Data[0] & 128;
            int right  = Data[1] & 1;
            int up     = Data[1] & 2;
            int down   = Data[1] & 4;

            movement = new(d - a, w - s);
            vertical = jump - crouch;
            sprint = shift == 1;
            look = new(right - left, up - down);
        }
        public readonly void DecodeTransform(out Vector3 position, out Vector3 rotation)
        {
            // 4 bytes for each position component and 4 bytes for each rotation component
            const int Length = 24;
            Assert.IsTrue(Type == DataType.Transform && this.Length == Length);

            position = Vector3.zero;
            rotation = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                position[i] = BytesToFloat(Data, i * 4);
                rotation[i] = BytesToFloat(Data, i * 4 + 12);
            }

            static float BytesToFloat(byte[] arr, int s) => arr[s] << 24 | arr[s + 1] << 16 | arr[s + 2] << 8 | arr[s + 3];
        }
    }

    public class PayloadData { }
    public abstract class PayloadWrapper<T> where T : PayloadData
    {
        public abstract Payload Encode();
        public abstract T Decode();
    }

    public class Info : PayloadData
    {
        public bool moving;
    }
    public class MovementPayload : PayloadWrapper<Info>
    {
        public override Payload Encode()
        {
            throw new NotImplementedException();
        }
        public override Info Decode()
        {
            throw new NotImplementedException();
        }
    }
}