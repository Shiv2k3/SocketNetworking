using Core.Multiplayer;
using System;
using UnityEngine;

namespace Demo
{
    public class MovementModule : NetworkedModule
    {
        public float speed;
        public byte input;
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
            byte[] d = new byte[] { input = k };
            Outgoing.Enqueue(d);
        }
        public Vector2 ve;
        protected override void ServerModulate()
        {
            if (Incoming.Count >= 1)
            {
                // de-q incoming
                ArraySegment<byte> data = Incoming.Dequeue();
                
                byte wasdInput = this.input = data[0];
                int w = wasdInput & 1;
                int a = wasdInput & 2;
                int s = wasdInput & 4;
                int d = wasdInput & 8;
                Vector2 input = new(w - s, d - a);

                // do stuff
                Vector2 v = ve = speed * Time.deltaTime * input;
                transform.position += new Vector3(v.x, 0, v.y);
            }

            // en-q outgoing
            ArraySegment<byte> x = BitConverter.GetBytes(transform.position.x);
            ArraySegment<byte> z = BitConverter.GetBytes(transform.position.z);

            Outgoing.Enqueue(x);
            Outgoing.Enqueue(z);
        }
    }

}
