using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using OpenLobby.Utility.Utils;

namespace Core.Demo
{
    public class LobbyMenu : MonoBehaviour
    {
        public Button search;
        public TMP_InputField searchName;
        public RectTransform ContentPanel;
        public LobbyCard LobbyCardPrefab;
        public PasswordPanel PasswordPanel;

        private void Awake()
        {
            search.onClick.AddListener(RequestQuery);
        }

        void RequestQuery()
        {
            Action<StringArray> onComplete = new(Lobbies =>
            {
                for (int i = 0; i < Lobbies.Count.Value / 2; i++)
                {
                    var id = Lobbies[i * 2];
                    var name = Lobbies[i * 2 + 1];

                    var card = Instantiate(LobbyCardPrefab, ContentPanel);
                    
                    card.Name.text = name;
                    card.Join.onClick.AddListener(() => { PasswordPanel.Enable(id); });
                }
                Debug.Log("Received and updated lobby list");
            });

            Multiplayer.Connections.Network.I.SendLobbyQuery(searchName.text, onComplete);
            foreach (Transform item in ContentPanel.transform)
            {
                Destroy(item.gameObject);
            }

        }
    }
}
