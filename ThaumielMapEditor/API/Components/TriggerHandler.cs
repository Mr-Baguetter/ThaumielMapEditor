using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using Mirror;
using System;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class TriggerHandler : MonoBehaviour
    {
        /// <summary>
        /// Gets the 
        /// </summary>
        public BoxCollider Collider { get; private set; }

        /// <summary>
        /// Gets the
        /// </summary>
        public Rigidbody Rigidbody { get; private set; }

        /// <summary>
        /// Fired when a <see cref="Player"/> enters the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Player, Collider>? OnPlayerEntered;

        /// <summary>
        /// Fired when a <see cref="Player"/> leaves the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Player, Collider>? OnPlayerExited;

        /// <summary>
        /// Fired when a <see cref="Pickup"/> enters the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Pickup, Collider>? OnPickupEntered;

        /// <summary>
        /// Fired when a <see cref="Pickup"/> leaves the bounds of the <see cref="TriggerHandler"/>
        /// </summary>
        public event Action<Pickup, Collider>? OnPickupExited;

        private void Awake()
        {
            if (!TryGetComponent<BoxCollider>(out var collider))
                collider = gameObject.AddComponent<BoxCollider>();

            collider.isTrigger = true;

            if (!TryGetComponent<Rigidbody>(out var body))
                body = gameObject.AddComponent<Rigidbody>();

            body.isKinematic = true;

            Collider = collider;
            Rigidbody = body;
        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            if (Player.TryGet(root, out var player))
            {
                OnPlayerEntered?.Invoke(player, other);
            }

            if (root.TryGetComponent<ItemPickupBase>(out var pickupbase) && Pickup.TryGet(pickupbase.Info.Serial, out var pickup))
            {
                OnPickupEntered?.Invoke(pickup, other);   
            }
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            if (Player.TryGet(root, out var player))
            {
                OnPlayerExited?.Invoke(player, other);
            }

            if (root.TryGetComponent<ItemPickupBase>(out var pickupbase) && Pickup.TryGet(pickupbase.Info.Serial, out var pickup))
            {
                OnPickupExited?.Invoke(pickup, other);   
            }
        }
    }
}