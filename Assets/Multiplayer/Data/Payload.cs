using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.Multiplayer.Data
{
    public unsafe struct Payload
    {
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
        public DataType Type { readonly get => (DataType)data[0]; }

        /// <summary>
        /// The data stream
        /// </summary>
        private byte* _stream;

        /// <summary>
        /// The payload
        /// <summary>
        private byte* _data;

        /// <summary>
        /// Creates payload from data
        /// <summary> 
        public Payload(DataType type, byte[] data) : this()
        {
            // create stream
            _stream = new byte[data.Length + 1];
            _data = _stream + 1;
            
            // populate stream
            _stream[0] = type;
            Buffer.MemoryCopy(data, _data, data.Length, data.Length);
        }
        /// <summary>
        /// Extracts payload from stream
        /// <summary>
        public Payload(byte[] stream) : this()
        {
            // Extract stream
            _stream = new byte[stream.Length];
            _data = _stream + 1;
            Buffer.MemoryCopy(stream, _stream, stream.Length, stream.Length);
        }
        
        public readonly void DecodeTime(out int tickRate, out int currentTick)
        {
            // tickrate 1 byte, current tick 1 byte, 2 bytes
            const int Lenth = 2;
            Assert.IsTrue(Type == DataType.Time && data.Length == Lenth);
            
            tickRate = data[0];
            currentTick = data[1];
        }
        public readonly void DecodeText(out string message)
        {
            Assert.IsTrue(Type == DataType.Text && data.Length > 0);
            
            message = Encoding.UTF8.GetString(data);
        }
        public readonly void DecodeInput(out Vector2Int movement, out int vertical, out bool sprint, out Vector2Int look)
        {
            // each bit repersents input for w, a, s, d, jump, crouch, shift, left, right, up, down, 11 bits or 2 bytes
            const int Length = 2;
            Assert.IsTrue(Type == DataType.Input && data.Length == Length);

            int w      = data[0] & 1;
            int a      = data[0] & 2;
            int s      = data[0] & 4;
            int d      = data[0] & 8;
            int jump   = data[0] & 16;
            int crouch = data[0] & 32;
            int shift  = data[0] & 64;
            int left   = data[0] & 128;
            int right  = data[1] & 1;
            int up     = data[1] & 2;
            int down   = data[1] & 4;

            movement = new(d - a, w - s);
            vertical = jump - crouch;
            sprint = shift == 1;
            look = new(right - left, up - down);
        }
        public readonly void DecodeTransform(out Vector3 position, out Vector3 rotation)
        {
            // 4 bytes for each position component and 4 bytes for each rotation component
            const int Length = 24;
            Assert.IsTrue(Type == DataType.Transform && data.Length == Length);

            position = Vector3.zero;
            rotation = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                position[i] = BytesToFloat(data, i * 4);
                rotation[i] = BytesToFloat(data, i * 4 + 12);
            }

            static float BytesToFloat(byte[] arr, int s) => arr[s] << 24 | arr[s + 1] << 16 | arr[s + 2] << 8 | arr[s + 3];
        }
    }
}
