using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class TeleporterHandler : TriggerHandler
    {
        public PrimitiveObjectToy Primitive;
        public TeleporterObject Teleporter;

        /// <summary>
        /// Tracks perplayer cooldown expiry times.
        /// </summary>
        public Dictionary<Player, float> PlayerCooldowns = [];

        private float _globalCooldownEnd;

        private readonly HashSet<Player> _playersInside = [];

        public void Init(PrimitiveObjectToy prim, TeleporterObject teleporter)
        {
            Primitive = prim;
            Teleporter = teleporter;

            OnPlayerEntered += OnTriggerEnter;
            OnPlayerExited += OnTriggerExit;
        }

        private void OnDestroy()
        {
            OnPlayerEntered -= OnTriggerEnter;
            OnPlayerExited -= OnTriggerExit;
        }

        private void Update()
        {
            if (PlayerCooldowns.IsEmpty())
                return;

            List<Player> expired = [];

            foreach (KeyValuePair<Player, float> kvp in PlayerCooldowns)
            {
                if (Time.time >= kvp.Value)
                    expired.Add(kvp.Key);
            }

            foreach (Player player in expired)
                PlayerCooldowns.Remove(player);
        }

        private void OnTriggerEnter(Player player)
        {
            if (!_playersInside.Add(player))
                return;

            if (!IsRoleAllowed(player))
                return;

            if (IsOnCooldown(player))
                return;

            TeleporterObject? target = FindTargetTeleporter();

            if (target == null)
            {
                LogManager.Warn($"Teleporter {Teleporter.Id} could not find target teleporter with Id: {Teleporter.Target}");
                return;
            }

            player.Position = target.Position;
            LogManager.Debug($"Player {player.Nickname} teleported from {Teleporter.Id} to {Teleporter.Target}");

            ApplyCooldown(player);
        }

        private void OnTriggerExit(Player player)
        {
            _playersInside.Remove(player);
        }

        private bool IsRoleAllowed(Player player) =>
            Teleporter.AllowedRoles.Count == 0 || Teleporter.AllowedRoles.Contains(player.Role);

        private bool IsOnCooldown(Player player)
        {
            if (Teleporter.CoolDown <= 0f)
                return false;

            return Teleporter.PerPlayerCooldown ? PlayerCooldowns.TryGetValue(player, out float expiry) && Time.time < expiry : Time.time < _globalCooldownEnd;
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

        private TeleporterObject? FindTargetTeleporter() =>
            ServerObject.SpawnedObjects.OfType<TeleporterObject>().FirstOrDefault(t => t.Id == Teleporter.Target);
    }
}