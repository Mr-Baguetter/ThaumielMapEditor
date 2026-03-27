using LabApi.Features.Wrappers;
using UnityEngine;

namespace ThaumielMapEditor.API.Extensions
{
    public static class ColliderExtensions
    {
        /// <summary>
        /// Tries to get a <see cref="Player"/> from a <see cref="Collider"/> using <see cref="GetPlayer(Collider)"/>
        /// </summary>
        /// <param name="collider">The <see cref="Collider"/> to check for a <see cref="Player"/></param>
        /// <param name="player">The <see cref="Player"/> if found.</param>
        /// <returns><see langword="true"/> if a <see cref="Player"/> was found else returns <see langword="false"/> when no <see cref="Player"/> is found.</returns>
        public static bool TryGetPlayer(this Collider collider, out Player player)
        {
            player = collider.GetPlayer()!;
            return player != null;
        }

        /// <summary>
        /// Gets a <see cref="Player"/> from a <see cref="Collider"/>
        /// </summary>
        /// <param name="collider">The <see cref="Collider"/> to check for a <see cref="Player"/></param>
        /// <returns><see cref="Player"/> if not <see langword="null"/> else returns <see langword="null"/></returns>
        public static Player? GetPlayer(this Collider collider)
        {
            ReferenceHub? hub = collider.GetComponentInParent<ReferenceHub>();
            if (hub == null || !Player.TryGet(hub.gameObject, out var player))
                return null;

            return player;
        }
    }
}