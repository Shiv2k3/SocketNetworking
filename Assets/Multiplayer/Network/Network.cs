using Core.Multiplayer.DataTransmission;
using Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
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

        public async Task<bool> HostLobby(string lobbyName, string lobbyPassword, bool visible, byte maxClients)
        {
            if (Online)
            {
                Debug.LogError("Network is already ONLINE");
                return false;
            }

            Initalize();

            string ip = Environment.GetEnvironmentVariable("OPENLOBBYIP", EnvironmentVariableTarget.User);
            int port = int.Parse(Environment.GetEnvironmentVariable("OPENLOBBYPORT", EnvironmentVariableTarget.User));

            Socket s = new(SocketType.Stream, ProtocolType.Tcp);
            var serverIP = IPAddress.Loopback;// IPAddress.Parse(ip);
            var rep = new IPEndPoint(serverIP, port);
            await s.ConnectAsync(rep);
            NetworkClient client = new(s);

            HostRequest req = new(lobbyName, lobbyPassword, visible, maxClients);
            await client.Send(req.Payload);
            var trms = await client.TryGetTransmission();
            bool success = trms is not null && new Reply(trms).ReplyCode == Reply.Code.LobbyCreated;

            if(success)
            {
                Debug.Log("Lobby was deployed successfully");
                Online = true;
                Host = client;
                return true;
            }
            else
            {
                Debug.LogError("Lobby deployment unsuccessful");
                client.Disconnect();
                return false;
            }
        }
        public void JoinLobby()
        {
            throw null;
        }
        private void Initalize()
        {
            Interfaces = new();
            ModuleTransmissions = new();
            ModulesMap = new();
            MapAllModules();

            void MapAllModules()
            {
                var allMods = FindObjectsByType<NetworkedModule>(FindObjectsSortMode.None);
                allMods.OrderBy(x => x.name.ToString());
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

            // Send brocast data
            await SendBroadcast();

            // Distribute the received module transmissions
            DistrubuteTransmissions();

            // Receive transmission
            await ReceiveTransmissions();

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
                    await Interfaces[clientID].Send(trms.Payload);
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