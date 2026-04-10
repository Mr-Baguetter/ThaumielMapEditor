// -----------------------------------------------------------------------
// <copyright file="TeleporterHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using LabApiExtensions.Extensions;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class TeleporterHandler : MonoBehaviour
    {
        /// <summary>
        /// Gets the <see cref="TeleporterObject"/> this handler is managing.
        /// </summary>
        public TeleporterObject Teleporter { get; private set; }

        /// <summary>
        /// Tracks per-player cooldown expiry times.
        /// Each entry maps a <see cref="Player"/> to the <see cref="Time.time"/> value at which their cooldown expires.
        /// </summary>
        /// <remarks>
        /// Expired entries are automatically removed each frame.
        /// </remarks>
        public Dictionary<Player, float> PlayerCooldowns = [];

        private float _globalCooldownEnd;

        private readonly HashSet<Player> _playersInside = [];

        /// <summary>
        /// Initializes this <see cref="TeleporterHandler"/> instance with the given teleporter.
        /// </summary>
        /// <param name="teleporter">The <see cref="TeleporterObject"/> that defines this teleporter's configuration.</param>
        public void Init(TeleporterObject teleporter)
        {
            Teleporter = teleporter;
        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            TeleporterObject? target = FindTargetTeleporter();
            if (target == null)
            {
                LogManager.Warn($"Teleporter {Teleporter.Id} could not find target teleporter.");
                return;
            }

            if (Player.TryGet(root, out var player))
            {
                if (!HasFlagFast(TeleporterFlags.AllowPlayers))
                    return;

                if (!_playersInside.Add(player))
                    return;

                if (!IsRoleAllowed(player))
                    return;

                if (IsOnCooldown(player))
                    return;

                target.TeleporterHandler.ForcePlayerCooldown(player);
                player.Position = target.Position;
                LogManager.Debug($"Player {player.Nickname} teleported from {Teleporter.Id} to {target.Id}");
                ApplyCooldown(player);
                return;
            }

            if (other.TryGetComponent(out ItemPickupBase pickupbase) && Pickup.TryGet(pickupbase.Info.Serial, out var pickup))
            {
                if (HasFlagFast(TeleporterFlags.AllowPickups))
                {
                    pickup.Position = target.Position;
                    LogManager.Debug($"Pickup {pickup.Type} teleported from {Teleporter.Id} to {target.Id}");
                }

                if (pickup is Projectile projectile && HasFlagFast(TeleporterFlags.AllowProjectiles))
                {
                    projectile.Position = target.Position;
                    LogManager.Debug($"Projectile {projectile.Type} teleported from {Teleporter.Id} to {target.Id}");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            if (!Player.TryGet(root, out var player))
                return;

            _playersInside.Remove(player);
        }

        /// <summary>
        /// Forces a minimum cooldown on a player regardless of teleporter settings, used to prevent immediate return teleportation.
        /// </summary>
        public void ForcePlayerCooldown(Player player, float duration = 1f)
        {
            float expiry = Time.time + duration;
            if (!PlayerCooldowns.TryGetValue(player, out float existing) || existing < expiry)
                PlayerCooldowns[player] = expiry;
        }

        private bool IsRoleAllowed(Player player) =>
            Teleporter.AllowedRoles.Count == 0 || Teleporter.AllowedRoles.Contains(player.Role);

        private bool IsOnCooldown(Player player)
        {
            if (Teleporter.CoolDown <= 0f)
                return false;

            if (Teleporter.PerPlayerCooldown)
            {
                if (PlayerCooldowns.TryGetValue(player, out float expiry))
                {
                    if (Time.time >= expiry)
                    {
                        PlayerCooldowns.Remove(player);
                        return false;
                    }

                    return true;
                }

                return false;
            }

            return Time.time < _globalCooldownEnd;
        }

        private void ApplyCooldown(Player player)
        {
            if (Teleporter.CoolDown <= 0f)
                return;

            if (Teleporter.PerPlayerCooldown)
            {
                PlayerCooldowns[player] = Time.time + Teleporter.CoolDown;
            }
            else
                _globalCooldownEnd = Time.time + Teleporter.CoolDown;
        }

        public bool HasFlagFast(TeleporterFlags flag) => (Teleporter.Flags & flag) != 0;

        private TeleporterObject? FindTargetTeleporter()
        {
            TeleporterObject target = ServerObject.SpawnedObjects.OfType<TeleporterObject>().FirstOrDefault(t => t.Id == Teleporter.Targets.GetRandom());
            if (target == null)
                return null;

            return target;
        }
    }
}