using System;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Extensions;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class TriggerHandler : MonoBehaviour
    {
        public event Action<Player>? OnPlayerEntered;
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