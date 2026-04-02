using LabApi.Features.Wrappers;
using Mirror;
using System;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class TriggerHandler : MonoBehaviour
    {
        /// <summary>
        /// Fired when a <see cref="Player"/> enters the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Player, Collider>? OnPlayerEntered;

        /// <summary>
        /// Fired when a <see cref="Player"/> leaves the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Player, Collider>? OnPlayerExited;

        private void OnTriggerEnter(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            if (!Player.TryGet(root, out var player))
                return;

            OnPlayerEntered?.Invoke(player, other);
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            if (!Player.TryGet(root, out var player))
                return;

            OnPlayerExited?.Invoke(player, other);
        }
    }
}