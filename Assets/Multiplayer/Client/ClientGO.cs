using Core.Util;
using Sirenix.OdinInspector;
using System.Net;
using UnityEngine;

namespace Core.Multiplayer
{
    public class ClientGO : MonoBehaviour
    {
        protected Client client;

        [Button("New Client")]
        protected virtual void CreateClient()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            client = new(ipHost.AddressList[0]);
        }

        [Button("Connect To Server")]
        private async void Connect()
        {
            Debug.Log("Starting connection...");
            float t = OL.Time;
            await client.ConnectToServer();
            Debug.Log("Connection time: " + (OL.Time - t));
        }

        [Button("Disconnect Server")]
        private async void Disconnect()
        {
            await client.SendMessage(new Data.Payload(Data.Payload.DataType.Disconnect, new byte[0]));
            client.DisconnectFromServer();
        }
    }
}