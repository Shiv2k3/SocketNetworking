using Core.Multiplayer.DataTransmission;
using Core.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for sending data and distributing received data to designated network entities
    /// </summary>
    public partial class Network : Singleton<Network>
    {
        public bool IsHost { get; private set; }
        public bool Online { get; private set; }

        private NetworkClient Host = null;
        private List<NetworkClient> Interfaces = null;
        private Queue<ModuleTransmission> ModuleTransmissions = null;

        private Dictionary<ushort, NetworkedModule> ModulesMap = null;

        /// <summary>
        /// Starts the host network
        /// </summary>
        public IPAddress StartNetwork()
        {
            if (Online) throw new("Network is already ONLINE");

            Initalize();

            IPEndPoint lep = new(Dns.GetHostAddresses(Dns.GetHostName())[0], 4567);
            Socket listener = new(lep.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(lep);
            listener.Listen(10);

            Host = new(listener);

            IsHost = true;
            Online = true;
            Debug.Log("Host Network STARTED");
            return lep.Address;
        }

        /// <summary>
        /// Starts the client network
        /// </summary>
        /// <param name="hostAddress">The address of the host</param>
        public async void StartNetwork(IPAddress hostAddress)
        {
            if (Online) throw new("Network is already ONLINE");

            Initalize();

            IPEndPoint rep = new(hostAddress, 4567);
            Socket host = new(rep.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            await host.ConnectAsync(rep);

            Host = new(host);
            Interfaces.Add(Host);

            IsHost = false;
            Online = true;
            Debug.Log("Client Network STARTED");
        }

        private void Initalize()
        {
            Interfaces = new();
            ModuleTransmissions = new();
            ModulesMap = new();
            MapAllModules();

            void MapAllModules()
            {
                var allMods = FindObjectsOfType<NetworkedModule>();
                foreach (var module in allMods)
                {
                    ushort ID = (ushort)ModulesMap.Count;
                    ModulesMap.Add(ID, module);
                    module.OnStart(ID);
                }
            }

        }
        private void Deinitalize()
        {
            foreach (var client in Interfaces)
            {
                client.Disconnect();
            }
            Interfaces = null;

            foreach (var module in ModulesMap)
            {
                module.Value.OnServerClosed();
            }
            ModulesMap = null;

            ModuleTransmissions = null;
            NetworkClient.Broadcasts.Clear();
        }

        public async void Disconnect()
        {
            Host.Disconnect();
            IsHost = false;
            Online = false;

            await Task.Yield();

            Deinitalize();

            Debug.Log("Network OFFLINE");
        }
        private async void Update()
        {
            // guard !connected
            if (!Online) return;

            // Receive transmission
            await ReceiveTransmissions();

            // Distribute the received module transmissions
            DistrubuteTransmissions();

            // Send brocast data
            await SendBroadcast();

            // Accept new connections
            if (IsHost) await AcceptRequests();
        }

        private bool _listening;
        /// <summary>
        /// Tries to establish new connections
        /// </summary>
        private async Task AcceptRequests()
        {
            if (_listening) return;

            try
            {
                _listening = true;
                Socket clientSocket = await Host.Socket.AcceptAsync();
                Debug.Log("Client CONNECTED");
                Interfaces.Add(new(clientSocket));
                _listening = false;
            }
            catch (ObjectDisposedException)
            {
                _listening = false;
            }
            catch (Exception e) { throw e; }
        }

        public void EnqueueBroadcast(Transmission trms) => NetworkClient.Broadcasts.Enqueue(trms);
        bool _sending;
        private async Task SendBroadcast()
        {
            if (_sending) return;
            _sending = true;

            while (NetworkClient.Broadcasts.Count != 0)
            {
                var trms = NetworkClient.Broadcasts.Dequeue();
                for (int clientID = 0; clientID < Interfaces.Count; clientID++)
                {
                    try
                    {
                        await Interfaces[clientID].Send(trms.Payload);

                    }
                    catch (SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.HostDown)
                        {
                            Debug.LogWarning("Endpoint CLOSED");
                            Disconnect();
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }

            _sending = false;
        }

        bool _receiving;
        /// <summary>
        /// Represents incoming transmissions
        /// </summary>
        private async Task ReceiveTransmissions()
        {
            if (_receiving) return;
            _receiving = true;

            foreach (var i in Interfaces)
            {
                var trms = await i.TryGetTransmission();
                
                // Cancel if disconnected
                if (!Online)
                {
                    _receiving = false;
                    return;
                }

                if (trms != null)
                    ModuleTransmissions.Enqueue(new(trms));
            }
            _receiving = false;
        }

        private void DistrubuteTransmissions()
        {
            while (ModuleTransmissions.Count != 0)
            {
                var mTrms = ModuleTransmissions.Dequeue();
                ModulesMap[mTrms.ModuleID].UploadData(mTrms.Data);
            }
        }

        public ushort ReportModule(NetworkedModule networkedModule)
        {
            ushort id = (ushort)ModulesMap.Count;
            ModulesMap.Add(id, networkedModule);
            return id;
        }
    }
}