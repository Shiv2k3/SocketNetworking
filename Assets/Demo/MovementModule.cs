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
                transform.position = new Vector3(BitConverter.ToSingle(x), 0, BitConverter.ToSingle(z));
            }

            // Send input data
            byte k = 0;
            k |= (byte)(Input.GetKey(KeyCode.W) ? 1 : 0);
            k |= (byte)(Input.GetKey(KeyCode.A) ? 2 : 0);
            k |= (byte)(Input.GetKey(KeyCode.S) ? 4 : 0);
            k |= (byte)(Input.GetKey(KeyCode.D) ? 8 : 0);

            // enq input
            byte[] d = new byte[] { k };
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
                int w = input & 1;
                int a = input & 2;
                int s = input & 4;
                int d = input & 8;
                inputV = new(w - s, d - a);
            }

            // Send position if it will change
            Vector2 v = speed * Time.deltaTime * inputV;
            if (v != Vector2.zero)
            {
                // en-q outgoing
                ArraySegment<byte> x = BitConverter.GetBytes(transform.position.x);
                ArraySegment<byte> z = BitConverter.GetBytes(transform.position.z);
                Outgoing.Enqueue(x);
                Outgoing.Enqueue(z);

                transform.position += new Vector3(v.x, 0, v.y);
            }

        }
    }

}
