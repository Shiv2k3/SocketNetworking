using Core.Multiplayer;
using TMPro;
using UnityEngine;
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

        async void RequestQuery()
        {
            Core.Util.StringArray r = await Network.I.SendQuery(searchName.text);
            if (r is not null)
            {
                while (ContentPanel.childCount != 0)
                {
                    Destroy(ContentPanel.GetChild(0));
                }

                for (int i = 0; i < r.Count.Value; i++)
                {
                    var card = Instantiate(LobbyCardPrefab, ContentPanel);
                    card.Name.text = r[i].Value;
                }
            }
        }
    }
}
