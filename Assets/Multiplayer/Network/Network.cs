using Core.Multiplayer.DataTransmission;
using Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;
#nullable enable

namespace Core.Multiplayer
{
    /// <summary>
    /// Responsible for sending data and distributing received data to designated network entities
    /// </summary>
    public partial class Network : Singleton<Network>
    {
        private const bool LOCALDEV = false;

        public bool IsHost { get; private set; }
        public bool Online { get; private set; }

        private NetworkClient OpenLobby = null;
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

            Socket opl = new(SocketType.Stream, ProtocolType.Tcp);

            var serverIP = LOCALDEV ? IPAddress.Loopback : IPAddress.Parse(ip);
            var rep = new IPEndPoint(serverIP, port);
            var lep = new IPEndPoint(IPAddress.Any, port);
            await opl.ConnectAsync(rep);
            OpenLobby = new(opl);

            HostRequest req = new(lobbyName, lobbyPassword, visible, maxClients);
            await OpenLobby.Send(req.Payload);
            var trms = await OpenLobby.TryGetTransmission();
            bool success = trms is not null && new Reply(trms).ReplyCode == Reply.Code.LobbyCreated;

            if (success)
            {
                Debug.Log("Lobby was deployed successfully");
                Online = true;
                IsHost = true;

                Socket listener = new(SocketType.Stream, ProtocolType.Tcp);
                if (!LOCALDEV)
                {
                    listener.Bind(lep);
                    listener.Listen(10);
                }

                Host = new(listener);

                return true;
            }
            else
            {
                Debug.LogError("Lobby deployment unsuccessful");
                OpenLobby.Disconnect();
                Deinitalize();
                return false;
            }
        }
        public void JoinLobby()
        {
            throw null;
        }
        public async Task<StringArray?> SendQuery(string name)
        {
            if (Online)
            {
                LobbyQuery lq = new(name);
                await OpenLobby.Send(lq.Payload);
                Debug.Log("Sent query request, awaiting reply");
                Transmission t;
                while ((t = await OpenLobby.TryGetTransmission()) != null)
                {
                    Transmission.Types type = (Transmission.Types)t.TypeID;
                    try
                    {
                        switch (type)
                        {
                            case Transmission.Types.Reply:
                                Reply r = new(t);
                                Debug.LogError("Received reply code from OpenLobby: " + r.ReplyCode);
                                return null;
                            case Transmission.Types.Query:
                                lq = new(t);
                                Debug.Log("Received query result");
                                return lq.Lobbies;

                            default: throw new IncorrectTransmission();
                        }
                    }
                    catch (IncorrectTransmission)
                    {
                        Debug.LogError($"Wrong transmission type received: {type}");
                        return null;
                    }
                    catch (Exception) { throw; }
                }
            }

            Debug.LogError("Network or OpenLobby is OFFLINE, cannot send queries");
            return null;
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