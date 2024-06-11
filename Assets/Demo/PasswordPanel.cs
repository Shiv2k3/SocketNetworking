using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Demo
{
    public class PasswordPanel : MonoBehaviour
    {
        public TMP_InputField password;
        public Button confirm;
        public Button exit;
        public void Enable(string lobbyID)
        {
            gameObject.SetActive(true);

            confirm.onClick.AddListener(confirmCall);
            exit.onClick.AddListener(exitCall);
            void confirmCall()
            {
                Multiplayer.Connections.Network.I.JoinLobby(lobbyID, password.text, (x) => { Debug.Log("Jonied Lobby at: " + x); });
                confirm.onClick.RemoveListener(confirmCall);
            }
            void exitCall()
            {
                password.text = "";
                gameObject.SetActive(false);
                exit.onClick.RemoveListener(exitCall);
            }
        }
    }
}
