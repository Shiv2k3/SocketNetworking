using Core.Multiplayer.DataTransmission;
using Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

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
        private Action<StringArray> QueryReplyCallback = null;

        private NetworkClient Host = null;
        private List<NetworkClient> ClientInterfaces = null;
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
        public async void SendLobbyQuery(string name, Action<StringArray> onComplete)
        {
            if (OpenLobby is not null)
            {
                LobbyQuery lq = new(name);
                await OpenLobby.Send(lq.Payload);
                QueryReplyCallback ??= onComplete;
                Debug.Log("Lobby query has been sent to OpenLobby");
            }
            else
            {
                Debug.LogError("Connection to OpenLobby hasn't been made");
            }
        }

        private void Initalize()
        {
            ClientInterfaces = new();
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
            foreach (var client in ClientInterfaces)
            {
                client.Disconnect();
            }
            ClientInterfaces = null;

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
                ClientInterfaces.Add(new(clientSocket));
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
                for (int clientID = 0; clientID < ClientInterfaces.Count; clientID++)
                {
                    await ClientInterfaces[clientID].Send(trms.Payload);
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

            // Client transmissions
            foreach (NetworkClient client in ClientInterfaces)
            {
                Transmission trms = await client.TryGetTransmission();

                // Cancel if disconnected
                if (!Online)
                {
                    _receiving = false;
                    return;
                }

                if (trms != null)
                    ModuleTransmissions.Enqueue(new(trms));
            }

            // OpenLobby transmissions
            {
                Transmission trms = await OpenLobby.TryGetTransmission();
                if (trms is not null)
                {
                    Transmission.Types type = (Transmission.Types)trms.TypeID;
                    switch (type)
                    {
                        case Transmission.Types.Query:
                            {
                                LobbyQuery lq = new(trms);
                                QueryReplyCallback(lq.Lobbies);
                                break;
                            }
                        default: throw new IncorrectTransmission();
                    }
                }
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