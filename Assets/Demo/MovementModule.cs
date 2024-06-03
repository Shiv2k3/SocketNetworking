using Core.Multiplayer;
using System;
using UnityEngine;

namespace Demo
{
    public class MovementModule : NetworkedModule
    {
        public float speed = 1f;

        private byte input;
        private Vector2 inputV;

        protected override void ClientModulate()
        {
            if (Incoming.Count >= 2)
            {
                // de-q incoming position
                ArraySegment<byte> x = Incoming.Dequeue();
                ArraySegment<byte> z = Incoming.Dequeue();

                // Set position
                transform.position = new Vector3(BitConverter.ToSingle(x), transform.position.y, BitConverter.ToSingle(z));
            }

            // Send input data
            byte k = 0;
            {
                k |= (byte)(Input.GetKey(KeyCode.W) ? 0b0001 : 0);
                k |= (byte)(Input.GetKey(KeyCode.A) ? 0b0010 : 0);
                k |= (byte)(Input.GetKey(KeyCode.S) ? 0b0100 : 0);
                k |= (byte)(Input.GetKey(KeyCode.D) ? 0b1000 : 0);
            }

            // enq input
            byte[] d = new byte[1] { k };
            if (k != input)
            {
                Outgoing.Enqueue(d);
                input = k;
            }
        }

        protected override void ServerModulate()
        {
            if (Incoming.Count >= 1)
            {
                // de-q incoming
                ArraySegment<byte> data = Incoming.Dequeue();
                
                // Get input
                input = data[0];
                int w = input & 0b0001;
                int a = input & 0b0010;
                int s = input & 0b0100;
                int d = input & 0b1000;
                inputV = new Vector2(d - a, w - s);
                if (inputV != Vector2.zero) inputV.Normalize();
            }

            // Send position if it will change
            if (inputV != Vector2.zero)
            {
                Vector2 v = speed * deltaTick * inputV;
                if (v != Vector2.zero)
                {
                    // Update pos
                    transform.position += new Vector3(v.x, 0, v.y);

                    // en-q outgoing
                    ArraySegment<byte> x = BitConverter.GetBytes(transform.position.x);
                    ArraySegment<byte> z = BitConverter.GetBytes(transform.position.z);
                    Outgoing.Enqueue(x);
                    Outgoing.Enqueue(z);
                }
            }

        }
    }

}
