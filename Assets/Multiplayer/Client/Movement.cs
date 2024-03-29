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
    }
}
