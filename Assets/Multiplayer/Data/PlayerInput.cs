using Core.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.Multiplayer.Data
{
    [System.Serializable]
    public class PlayerInput : PayloadData
    {
        public Vector2 Horizontal;
        public Vector2 Rotation;
        public bool Jump;
        public bool Crouch;
        public bool Sprint;

        public PlayerInput(Vector2 horizontal, Vector2 rotation, bool jump, bool crouch, bool sprint)
        {
            Horizontal = horizontal;
            Rotation = rotation;
            Jump = jump;
            Crouch = crouch;
            Sprint = sprint;

            byte[] data = new byte[7];
            data[0] = OL.FloatToByte(horizontal.x);
            data[1] = OL.FloatToByte(horizontal.y);
            data[2] = OL.FloatToByte(rotation.x);
            data[3] = OL.FloatToByte(rotation.y);
            data[4] = jump ? byte.MaxValue : byte.MinValue;
            data[5] = crouch ? byte.MaxValue : byte.MinValue;
            data[6] = sprint ? byte.MaxValue : byte.MinValue;

            payload = new(Payload.DataType.Input, data);

        }
        public PlayerInput(in Payload payload)
        {
            Assert.IsTrue(payload.Type == Payload.DataType.Input, "Incorrect payload type");

            Horizontal.x = OL.ByteToFloat(payload.Data[0]);
            Horizontal.y =OL. ByteToFloat(payload.Data[1]);
            Rotation.x = OL.ByteToFloat(payload.Data[2]);
            Rotation.y = OL.ByteToFloat(payload.Data[3]);
            Jump = payload.Data[4] > byte.MinValue;
            Crouch = payload.Data[5] > byte.MinValue;
            Sprint = payload.Data[6] > byte.MinValue;

            this.payload = payload;
        }

    }
}
