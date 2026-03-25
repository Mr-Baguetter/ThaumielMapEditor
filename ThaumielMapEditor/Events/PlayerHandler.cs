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
    public class PlayerHandler
    {
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
            foreach (SchematicData data in SchematicLoader.SpawnedSchematics)
            {
                LogManager.Debug($"Spawning {data.FileName} for player {ev.Player.DisplayName}");
                data.SyncWithPlayer(ev.Player);
            }

            AddPlayerTrigger(ev.Player);
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