using Sirenix.OdinInspector;
using System.Net;
using UnityEngine;

namespace Demo
{
    public class Menu : MonoBehaviour
    {
        [SerializeField, ReadOnly] private string serverIp = IPAddress.Loopback.ToString();

        [Button("Host Server")]
        void HostServer()
        {
            serverIp = Core.Multiplayer.Network.I.StartNetwork().ToString();
        }

        [Button("Join Server")]
        void JoinServer()
        {
            var ip = Dns.GetHostAddresses(Dns.GetHostName())[0];
            serverIp = ip.ToString();
            Core.Multiplayer.Network.I.StartNetwork(ip);
        }

        [Button("Disconnect")]
        void Disconnect()
        {
            Core.Multiplayer.Network.I.Disconnect();
        }

    }
}