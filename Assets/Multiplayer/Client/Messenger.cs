using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Multiplayer
{
    public class Messenger : ClientGO
    {
        [SerializeField] private string message;

        protected override void CreateClient()
        {
            base.CreateClient();
            message = "Hello server, this is client #" + Random.Range(69, 421);
        }

        [Button("Send Message")]
        private void SendMessage()
        {
            _ = client.SendMessage(message);
        }

    }
}