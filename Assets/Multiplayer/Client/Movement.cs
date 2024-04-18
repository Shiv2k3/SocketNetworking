using Core.Multiplayer.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Multiplayer
{
    public class Movement : ClientGO
    {
        [SerializeField] private Vector3 position;
        protected override void CreateClient()
        {
            base.CreateClient();
        }

        [Button("Send")]
        private void Send()
        {
            Vector2 hor = new(position.x, position.y);
            Vector2 vert = new Vector2(position.z, position.z);
            PlayerInput p = new PlayerInput(hor, vert, false, false, false);
            _ = client.SendMessage(p.payload);
        }
    }
}