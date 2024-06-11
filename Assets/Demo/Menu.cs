using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Core.Demo
{
    public class Menu : MonoBehaviour
    {
        public TMP_InputField LobbyName;
        public TMP_InputField LobbyPassword;
        public TMP_InputField MaxPlayers;
        public Toggle Public;

        public Button HostButton;
        public Button DisconnectButton;

        private void Awake()
        {
            HostButton.onClick.AddListener(HostLobby);
            DisconnectButton.onClick.AddListener(Disconnect);
        }

        [Button("Host Lobby")]
        void HostLobby()
        {
            Multiplayer.Connections.Network.I.HostLobby(LobbyName.text, LobbyPassword.text, Public.isOn, byte.Parse(MaxPlayers.text));
        }

        [Button("Disconnect")]
        void Disconnect()
        {
            Multiplayer.Connections.Network.I.Disconnect();
        }

    }
}