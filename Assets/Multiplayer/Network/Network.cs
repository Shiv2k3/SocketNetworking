using System;
using Core.Util;
using System.Net;
using System.Linq;
using UnityEngine;
using System.Net.Sockets;
using System.Threading.Tasks;
using OpenLobby.Utility.Utils;
using OpenLobby.Utility.Network;
using System.Collections.Generic;
using Core.Multiplayer.Transmissions;
using OpenLobby.Utility.Transmissions;

namespace Core.Multiplayer.Connections
{
    /// <summary>
    /// Responsible for sending data and distributing received data to designated network entities
    /// </summary>
    public partial class Network : Singleton<Network>
    {
        public bool IsHost { get; private set; }
        public bool Online { get; private set; }

        // Connections
        private Client Host = null;
        private List<Client> ClientInterfaces = null;

        // Transmissions
        private Queue<ModuleTransmission> Broadcasts = null;
        private Queue<ModuleTransmission> ModuleTransmissions = null;
        private Dictionary<ushort, Module> ModulesMap = null;
        public void HostLobby(string lobbyName, string lobbyPassword, bool visible, byte maxClients)
        {
            // Exit if already online
            if (Online)
            {
                throw new InvalidAction("Network is already ONLINE");
            }

            // Check if OpenLobby is available
            if (!OpenLobby.I.Online)
            {
                throw new InvalidAction("OpenLobby isn't online");
            }

            // Init members
            InitalizeMembers();
            
            // Send request
            HostRequest req = new(lobbyName, lobbyPassword, visible, maxClients);
            OpenLobby.I.MessageReceivedEvent += OnReply;
            OpenLobby.I.EnqueueTransmission(req);

            void OnReply(Transmission t)
            {
                if (t.Type != Transmission.TransmisisonType.Reply) return;

                // Decode
                var reply = new Reply(t);
                switch (reply.ReplyCode)
                {
                    case Reply.Code.HostingSuccess:
                        {
                            // Log
                            Debug.Log("OpenLobby has hosted the lobby");

                            // Get IP info and Start listening
                            int port = OpenLobby.I.Server.LocalPort;
                            IPEndPoint lep = new(IPAddress.Any, 0);
                            Host = new(lep);

                            // Init
                            Online = true;
                            IsHost = true;
                            Debug.Log($"Listening on " + lep.ToString());

                            // Unscubscribe
                            OpenLobby.I.MessageReceivedEvent -= OnReply;
                            break;
                        }
                    case Reply.Code.HostingError:
                        {
                            Debug.Log("OpenLobby was unable to host the lobby");
                            DeinitalizeMembers();
                            break;
                        }
                }
                
                ReceiveClients();
            }
        }
        public void JoinLobby(string lobbyID, string password, Action<IPEndPoint> OnComplete)
        {
            // Exit if already online
            if (Online)
            {
                throw new InvalidAction("Network is already ONLINE");
            }

            // Check if OpenLobby is available
            if (!OpenLobby.I.Online)
            {
                throw new InvalidAction("OpenLobby isn't online");
            }

            // Init members
            InitalizeMembers();

            // Send request
            JoinRequest req = new(lobbyID, password);
            OpenLobby.I.MessageReceivedEvent += OnReply;
            OpenLobby.I.EnqueueTransmission(req);

            void OnReply(Transmission t)
            {
                if (t.Type != Transmission.TransmisisonType.Join && t.Type != Transmission.TransmisisonType.Reply) return;
                OpenLobby.I.MessageReceivedEvent -= OnReply;

                if (t.Type == Transmission.TransmisisonType.Reply)
                {
                    var reply = new Reply(t);
                    switch (reply.ReplyCode)
                    {
                        case Reply.Code.JoinError:
                            {
                                Debug.LogError("There was a problem with joining the lobby");
                                OnComplete(new IPEndPoint(IPAddress.Any, 0));
                                break;
                            }
                        case Reply.Code.WrongPassword:
                            {
                                Debug.LogError("The wrong password was entered");
                                OnComplete(new IPEndPoint(IPAddress.Any, 0));
                                break;
                            }
                    }
                }

                var jr = new JoinRequest(t, true);
                var split = jr.HostAddress.Value.Split(":");
                string ip = split[0];
                string port = split[1];
                IPEndPoint ep = new(IPAddress.Parse(ip), int.Parse(port));
                OnComplete(ep);
            }
        }

        public void SendLobbyQuery(string name, Action<StringArray> onComplete)
        {
            OpenLobby.I.MessageReceivedEvent += OnReply;
            LobbyQuery lq = new(name);
            OpenLobby.I.EnqueueTransmission(lq);
            Debug.Log("Lobby query has been sent to OpenLobby");

            void OnReply(Transmission t)
            {
                if (t.Type != Transmission.TransmisisonType.Query && t.Type != Transmission.TransmisisonType.Reply) return;
                OpenLobby.I.MessageReceivedEvent -= OnReply;

                if (t.Type == Transmission.TransmisisonType.Reply)
                {
                    Reply r = new(t);
                    Debug.LogError("Received reply form OpenLobby: " + r.ReplyCode);
                    return;
                }

                LobbyQuery lq = new(t, true);
                onComplete(lq.Lobbies);
            }
        }
        private async void ReceiveClients()
        {
            try
            {
                Client client = await Host.Accept();
                ClientInterfaces.Add(client);
                Debug.Log("Client CONNECTED");
                if (Online)
                {
                    ReceiveClients();
                }
            }
            catch {}
        }
        private void InitalizeMembers()
        {
            ClientInterfaces = new();
            ModuleTransmissions = new();
            Broadcasts = new();
            ModulesMap = new();
            MapAllModules();

            void MapAllModules()
            {
                var allMods = FindObjectsByType<Module>(FindObjectsSortMode.None);
                allMods.OrderBy(x => x.name.ToString());
                foreach (var module in allMods)
                {
                    ushort ID = (ushort)ModulesMap.Count;
                    ModulesMap.Add(ID, module);
                    module.OnStart(ID);
                }
            }

        }
        private void DeinitalizeMembers()
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
            Broadcasts.Clear();
        }
        public async void Disconnect()
        {
            Host.Disconnect();
            IsHost = false;
            Online = false;

            await Task.Yield();

            DeinitalizeMembers();

            Debug.Log("Network OFFLINE");
        }

        private void Update()
        {
            // guard !connected
            if (!Online) return;
            
            // Receive transmissions
            ReceiveTransmissions();

            // Send brocast data
            SendBroadcast();

            // Distribute the received module transmissions
            DistrubuteTransmissions();
        }

        public void EnqueueBroadcast(ModuleTransmission mt) => Broadcasts.Enqueue(mt);
        private void SendBroadcast()
        {
            while (Broadcasts.Count != 0)
            {
                var trms = Broadcasts.Dequeue();
                for (int clientID = 0; clientID < ClientInterfaces.Count; clientID++)
                {
                    ClientInterfaces[clientID].Send(trms.Payload);
                }
            }
        }
        private void ReceiveTransmissions()
        {
            // Client transmissions
            if (Online)
            {
                foreach (Client client in ClientInterfaces)
                {
                    (bool success, Transmission trms) = client.TryGetTransmission();
                    if (success)
                    {
                        ModuleTransmission mt = new(trms);
                        ModuleTransmissions.Enqueue(mt);
                    }
                }
            }
        }
        private void DistrubuteTransmissions()
        {
            while (ModuleTransmissions.Count != 0)
            {
                var mTrms = ModuleTransmissions.Dequeue();
                ModulesMap[mTrms.ModuleID].UploadData(mTrms.Data);
            }
        }
        public ushort ReportModule(Module networkedModule)
        {
            ushort id = (ushort)ModulesMap.Count;
            ModulesMap.Add(id, networkedModule);
            return id;
        }
    }
}