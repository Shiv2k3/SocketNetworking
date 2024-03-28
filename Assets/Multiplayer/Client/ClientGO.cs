using Core.Util;
using Sirenix.OdinInspector;
using System.Net;
using UnityEngine;

namespace Core.Multiplayer
{
    public class ClientGO : MonoBehaviour
    {
        [SerializeField] private string message;

        private Client client;
        [Button("New Client")]
        private void CreateClient()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            client = new(ipHost.AddressList[0]);
            message = "Hello server, this is client #" + Random.Range(69, 421);
        }

        [Button("Connect To Server")]
        private async void Connect()
        {
            Debug.Log("Starting connection...");
            float t = OL.Time;
            await client.ConnectToServer();
            Debug.Log("Connection time: " + (OL.Time - t));
        }

        [Button("Send Message")]
        private void SendMessage()
        {
            _ = client.SendMessage(message);
        }

        [Button("Disconnect Server")]
        private void Disconnect()
        {
            client.DisconnectFromServer();
        }
    }
}