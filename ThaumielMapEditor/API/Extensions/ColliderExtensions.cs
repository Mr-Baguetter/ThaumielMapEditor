using LabApi.Features.Wrappers;
using UnityEngine;

namespace ThaumielMapEditor.API.Extensions
{
    public static class ColliderExtensions
    {
        public static bool TryGetPlayer(this Collider collider, out Player player)
        {
            player = collider.GetPlayer()!;
            return player != null;
        }

        public static Player? GetPlayer(this Collider collider)
        {
            ReferenceHub? hub = collider.GetComponentInParent<ReferenceHub>();
            if (hub == null || !Player.TryGet(hub.gameObject, out var player))
                return null;

            return player;
        }
    }
}