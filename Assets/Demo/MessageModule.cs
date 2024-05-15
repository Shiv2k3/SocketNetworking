using Core.Multiplayer;
using System;
using System.Text;
using UnityEngine;

namespace Demo
{
    public class MessageModule : NetworkedModule
    {
        public string msg = "NONE";
        private int lastSent = "NONE".GetHashCode();

        protected override void ClientModulate()
        {
            if (Incoming.Count == 0) return;

            ArraySegment<byte> data = Incoming.Dequeue();
            msg = Encoding.Default.GetString(data);
            Debug.Log("Message from server: " + msg);
        }

        protected override void ServerModulate()
        {
            int hash = msg.GetHashCode();
            if (hash != lastSent)
            {
                lastSent = hash;
                var msgBytes = Encoding.Default.GetBytes(msg);
                Outgoing.Enqueue(msgBytes);
            }
        }
    }

}
