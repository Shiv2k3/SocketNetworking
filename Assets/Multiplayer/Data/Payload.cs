using System;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.Multiplayer.Data
{
    public readonly unsafe struct Payload
    {
        private const int HEADERLENGTH = 2;
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
        public readonly DataType Type { get => (DataType)_stream[0]; }

        /// <summary>
        /// The length of the data
        /// </summary>
        public readonly int Length { get => _stream[1]; }

        /// <summary>
        /// The data stream
        /// </summary>
        private readonly byte* _stream;

        /// <summary>
        /// The payload
        /// <summary>
        private readonly byte* _data;

        /// <summary>
        /// Creates payload from data
        /// <summary> 
        public Payload(DataType type, byte[] data) : this()
        {
            // create stream
            byte[] arr = new byte[data.Length + HEADERLENGTH];
            _stream = (byte*)&arr;
            _data = _stream + HEADERLENGTH;
            
            // populate stream
            _stream[0] = (byte)type;
            Buffer.MemoryCopy(&data, _data, data.Length, data.Length);
        }
        /// <summary>
        /// Extracts payload from stream
        /// <summary>
        public Payload(byte[] stream) : this()
        {
            // Extract stream
            byte[] arr = new byte[stream.Length];
            _stream = (byte*)&arr;
            _data = _stream + HEADERLENGTH;
            Buffer.MemoryCopy(&stream, _stream, stream.Length, stream.Length);
        }
        
        public readonly void DecodeTime(out int tickRate, out int currentTick)
        {
            // tickrate 1 byte, current tick 1 byte, 2 bytes
            const int Length = 2;
            Assert.IsTrue(Type == DataType.Time && this.Length == Length);
            
            tickRate = _data[0];
            currentTick = _data[1];
        }
        public readonly void DecodeText(out string message)
        {
            Assert.IsTrue(Type == DataType.Text);
            
            message = Encoding.UTF8.GetString(_data, Length);
        }
        public readonly void DecodeInput(out Vector2Int movement, out int vertical, out bool sprint, out Vector2Int look)
        {
            // each bit repersents input for w, a, s, d, jump, crouch, shift, left, right, up, down, 11 bits or 2 bytes
            const int Length = 2;
            Assert.IsTrue(Type == DataType.Input && this.Length == Length);

            int w      = _data[0] & 1;
            int a      = _data[0] & 2;
            int s      = _data[0] & 4;
            int d      = _data[0] & 8;
            int jump   = _data[0] & 16;
            int crouch = _data[0] & 32;
            int shift  = _data[0] & 64;
            int left   = _data[0] & 128;
            int right  = _data[1] & 1;
            int up     = _data[1] & 2;
            int down   = _data[1] & 4;

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
                position[i] = BytesToFloat(_data, i * 4);
                rotation[i] = BytesToFloat(_data, i * 4 + 12);
            }

            static float BytesToFloat(byte* arr, int s) => arr[s] << 24 | arr[s + 1] << 16 | arr[s + 2] << 8 | arr[s + 3];
        }
    }
}
