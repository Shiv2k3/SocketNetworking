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

        private void FixedUpdate()
        {
            if (client is not null)
                Send();
        }

        [Button("Send")]
        private void Send()
        {
            Vector2 move = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector2 mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            PlayerInput p = new PlayerInput(move, mouse, Input.GetKey(KeyCode.Space), Input.GetKey(KeyCode.LeftControl), Input.GetKey(KeyCode.LeftShift));
            _ = client.SendMessage(p.payload);
        }
    }
}