using System.Collections.Generic;
using AdminToys;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace ThaumielMapEditor.Events
{
    internal class PlayerHandler
    {
        public class CreditTag
        {
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
        }

        public static void Register()
        {
            PlayerEvents.Joined += OnPlayerJoined;
            ReferenceHub.OnBeforePlayerDestroyed += OnPlayerLeft;
        }

        public static void Unregister()
        {
            PlayerEvents.Joined -= OnPlayerJoined;
            ReferenceHub.OnBeforePlayerDestroyed -= OnPlayerLeft;
        }

        // If you contribute and want a CreditTag add yourself to this.
        private static readonly Dictionary<string, CreditTag> Credits = new()
        {
            // MrBaguetter
            ["76561199150506472@steam"] = new CreditTag()
            {
                Name = "TME Lead Developer",
                Color = "pumpkin"
            }
        };

        private static void OnPlayerLeft(ReferenceHub hub)
        {
            if (!Player.TryGet(hub.gameObject, out var player))
            {
                LogManager.Warn($"Failed to get leaving player.");
                return;
            }

            foreach (SchematicData data in SchematicLoader.SpawnedSchematics)
            {
                foreach (ClientSideObjectBase clientobj in data.SpawnedClientObjects)
                {
                    if (!clientobj.SpawnedPlayers.Contains(player))
                        continue;

                    clientobj.SpawnedPlayers.Remove(player);
                }
            }
        }

        private static void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            if (ev.Player == null)
                return;

            Timing.CallDelayed(0.5f, () =>
            {
                if (ev.Player == null || ev.Player.IsDestroyed)
                {
                    LogManager.Warn($"Player was null or destroyed before sync could run.");
                    return;
                }

                foreach (SchematicData data in SchematicLoader.SchematicsById.Values)
                {
                    LogManager.Debug($"Spawning {data.FileName} for player {ev.Player.DisplayName}");
                    data.SyncWithPlayer(ev.Player);
                }

                AddPlayerTrigger(ev.Player);

                if (Main.Instance.Config.EnableCreditTags && Credits.TryGetValue(ev.Player.UserId, out var credittag))
                {
                    ev.Player.GroupColor = credittag.Color;
                    ev.Player.GroupName = credittag.Name;
                }
            });
        }

        public static void AddPlayerTrigger(Player player)
        {
            LabPrimitive trigger = LabPrimitive.Create(player?.GameObject?.transform, false);
            trigger.GameObject.name = $"[Thaumiel Map Editor] PlayerTrigger {player?.PlayerId}";
            trigger.Flags = PrimitiveFlags.None;
            trigger.MovementSmoothing = 0;
            trigger.Spawn();

            if (!trigger.GameObject.TryGetComponent<BoxCollider>(out var collider))
                collider = trigger.GameObject.AddComponent<BoxCollider>();

            collider.isTrigger = true;
            collider.size = new(1, 2, 1);
        }
    }
}