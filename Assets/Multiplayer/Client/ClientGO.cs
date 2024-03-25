using Sirenix.OdinInspector;
using System;
using System.Net;
using UnityEngine;

namespace Core.Multiplayer
{
    public class ClientGO : MonoBehaviour
    {
        [SerializeField] public Client client;

        [Button("Create Client")]
        private void CreateClient()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            client = new(ipHost.AddressList[0]);
        }

        [Button("Connect To Server")]
        private async void Connect()
        {
            Debug.Log("Starting connection...");
            float t = Server.Time;
            await client.ConnectToServer();
            Debug.Log("Connection time: " + (Server.Time - t));
        }

        [Button("Disconnect From Server")]
        private void Disconnect()
        {
            client.DisconnectFromServer();
        }
    }
}