﻿using Core.Multiplayer.DataTransmission;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Multiplayer
{
    /// <summary>
    /// Repersents a single networked behaviour
    /// </summary>
    public abstract class NetworkedModule : MonoBehaviour
    {
        [SerializeField] private int TickRate = 64;
        public void OnStart(ushort id)
        {
            ID = id;
            InvokeRepeating(nameof(Tick), 0, 1f / TickRate);
        }
        protected virtual void Awake()
        {
            if (Network.I.Online)
            {
                OnStart(Network.I.ReportModule(this));
            }
        }
        public void OnServerClosed()
        {
            CancelInvoke(nameof(Tick));
        }
        protected virtual bool Tick()
        {
            if (!Network.I.Online) return false;

            Modulate();
            while (Outgoing.Count != 0)
            {
                var trms = new ModuleTransmission(ID, Outgoing.Dequeue());
                Network.I.EnqueueBroadcast(trms);
            }

            return true;
        }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public ushort ID { get; private set; }

        /// <summary>
        /// Queue with incoming data
        /// </summary>
        protected readonly Queue<ArraySegment<byte>> Incoming = new();

        /// <summary>
        /// Queue with outoging data
        /// </summary>
        protected readonly Queue<ArraySegment<byte>> Outgoing = new();

        /// <summary>
        /// Execute module behaviour
        /// </summary>
        public void Modulate()
        {
            if (Network.I.IsHost)
            {
                ServerModulate();
            }
            else
            {
                ClientModulate();
            }
        }

        /// <summary>
        /// Execute server side behaviour
        /// </summary>
        protected abstract void ServerModulate();

        /// <summary>
        /// Execute client side behaviour
        /// </summary>
        protected abstract void ClientModulate();

        /// <summary>
        /// Upload data from network
        /// </summary>
        /// <param name="data"></param>
        public void UploadData(ArraySegment<byte> data)
        {
            Incoming.Enqueue(data);
        }
    }
}