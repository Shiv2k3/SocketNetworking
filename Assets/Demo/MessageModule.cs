using System;
using System.Text;
using UnityEngine;
using Core.Multiplayer.Connections;

namespace Core.Demo
{
    public class MessageModule : Module
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
