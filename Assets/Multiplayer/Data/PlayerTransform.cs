using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.Multiplayer.Data
{
    public class PlayerTransform : PayloadData
    {
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }

        public PlayerTransform(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;

            byte[] data = new byte[24];
            for (int i = 0; i < 3; i++)
            {
                var posBytes = BitConverter.GetBytes(position[i]);
                var rotByte = BitConverter.GetBytes(rotation[i]);
                for (int b = 0; b < posBytes.Length; b++)
                {
                    data[i * posBytes.Length + b] = posBytes[b];
                    data[i * posBytes.Length + b + 12] = rotByte[b];
                }
            }


            payload = new(Payload.DataType.Transform, data);
        }

        public PlayerTransform(in Payload payload)
        {
            Assert.IsTrue(payload.Type == Payload.DataType.Transform, "Incorrect payload type");

            Vector3 position = Vector3.zero;
            Vector3 rotation = Vector3.zero;
            for (int i = 0; i < 3; i++)
            {
                position[i] = BitConverter.ToSingle(payload.Data.AsSpan(i * 4, 4));
                rotation[i] = BitConverter.ToSingle(payload.Data.AsSpan(i * 4 + 12, 4));
            }
            Position = position;
            Rotation = rotation;

            this.payload = payload;
        }
    }
}
