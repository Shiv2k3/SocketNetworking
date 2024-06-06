using Core.Util;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Network = Core.Multiplayer.Network;

namespace Demo
{
    public class LobbyMenu : MonoBehaviour
    {
        public Button search;
        public TMP_InputField searchName;
        public RectTransform ContentPanel;
        public LobbyCard LobbyCardPrefab;

        private void Awake()
        {
            search.onClick.AddListener(RequestQuery);
        }

        void RequestQuery()
        {
            Action<StringArray> onComplete = new(Lobbies =>
            {
                for (int i = 0; i < Lobbies.Count.Value; i++)
                {
                    var card = Instantiate(LobbyCardPrefab, ContentPanel);
                    card.Name.text = Lobbies[i].Value;
                }
            });

            Network.I.SendLobbyQuery(searchName.text, onComplete);
            foreach (Transform item in ContentPanel.transform)
            {
                Destroy(item.gameObject);
            }

        }
    }
}
