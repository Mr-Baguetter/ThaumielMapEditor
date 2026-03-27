using System;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Extensions;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class TriggerHandler : MonoBehaviour
    {
        /// <summary>
        /// Fired when a <see cref="Player"/> enters the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Player>? OnPlayerEntered;

        /// <summary>
        /// Fired when a <see cref="Player"/> leaves the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Player>? OnPlayerExited;

        private void OnTriggerEnter(Collider other)
        {
            if (other is not BoxCollider || !other.TryGetPlayer(out var player))
                return;

            OnPlayerEntered?.Invoke(player);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other is not BoxCollider || !other.TryGetPlayer(out var player))
                return;

            OnPlayerExited?.Invoke(player);
        }
    }
}