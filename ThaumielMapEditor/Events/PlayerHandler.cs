// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;

namespace ThaumielMapEditor.Events
{
    internal class PlayerHandler
    {
        public enum TagType
        {
            LeadDeveloper = 1,
            Developer = 2,
            Contributor = 3,
        }

        public static void Register()
        {
            PlayerEvents.Joined += OnPlayerJoined;
            PlayerEvents.ChangedSpectator += OnPlayerChangedSpectator;
            PlayerEvents.Spawned += PlayerSpawnPoint.OnPlayerSpawned;
            Scp079Events.ChangedCamera += OnScp079ChangedCamera;
            ReferenceHub.OnBeforePlayerDestroyed += OnPlayerLeft;
        }

        public static void Unregister()
        {
            PlayerEvents.Joined -= OnPlayerJoined;
            PlayerEvents.ChangedSpectator -= OnPlayerChangedSpectator;
            PlayerEvents.Spawned -= PlayerSpawnPoint.OnPlayerSpawned;
            Scp079Events.ChangedCamera += OnScp079ChangedCamera;
            ReferenceHub.OnBeforePlayerDestroyed -= OnPlayerLeft;
        }

        // If you contribute and want a CreditTag add your own steamid to this.
        internal static readonly Dictionary<string, TagType> Credits = new()
        {
            // MrBaguetter
            ["76561199150506472@steam"] = TagType.LeadDeveloper,

            // Example developer badge
            ["EXAMPLE99@steam"] = TagType.Developer,

            // Example contributor badge
            ["EXAMPLE99@steam"] = TagType.Contributor
        };

        internal static void SetTag(Player player)
        {
            if (!Main.Instance.Config!.EnableCreditTags)
                return;

            if (!Credits.TryGetValue(player.UserId, out var type))
            {
                LogManager.Debug($"Player {player.DisplayName} is not in the Credits dictionary.");
                return;
            }

            switch (type)
            {
                case TagType.LeadDeveloper:
                    player.GroupName = "TME Lead Developer";
                    player.GroupColor = "pumpkin";
                    break;

                case TagType.Developer:
                    player.GroupName = "TME Developer";
                    player.GroupColor = "purple";
                    break;

                case TagType.Contributor:
                    player.GroupName = "TME Contributor";
                    player.GroupColor = "red";
                    break;
            };
        }

        private static void OnScp079ChangedCamera(Scp079ChangedCameraEventArgs ev)
        {
            foreach (CullingObject cullingZone in CullingObject.AllInstances)
            {
                if (cullingZone.IsInsideCollider(ev.Camera.Position))
                {
                    cullingZone.ToggleVisibility(ev.Player, true);
                }
                else
                    cullingZone.ToggleVisibility(ev.Player, false);
            }
        }
        
        private static void OnPlayerChangedSpectator(PlayerChangedSpectatorEventArgs ev)
        {
            if (ev.OldTarget == ev.NewTarget)
                return;

            UpdateSpectatorLOD(ev.OldTarget, ev.Player, isNowVisible: false);
            UpdateSpectatorLOD(ev.NewTarget, ev.Player, isNowVisible: true);

            if (ev.OldTarget != null)
            {
                foreach (CullingObject cullingZone in CullingObject.AllInstances)
                {
                    if (cullingZone.PlayersInside.Contains(ev.OldTarget))
                        cullingZone.ToggleVisibility(ev.Player, false);
                }
            }

            if (ev.NewTarget != null)
            {
                foreach (CullingObject cullingZone in CullingObject.AllInstances)
                {
                    if (cullingZone.PlayersInside.Contains(ev.NewTarget))
                        cullingZone.ToggleVisibility(ev.Player, true);
                }
            }
        }

        internal static void UpdateSpectatorLOD(Player target, Player spectator, bool isNowVisible)
        {
            if (target == null || !LODHelper.PlayersInLODZones.TryGetValue(target, out var zones))
                return;

            foreach (LODZone zone in zones)
            {
                if (!SchematicLoader.SchematicLODZones.TryGetValue(zone, out var schematic))
                    continue;

                foreach (PrimitiveObject primitive in schematic.GetClientObject<PrimitiveObject>())
                {
                    if (zone.PrimitivestoUnload.Contains(primitive.PrimitiveType))
                    {
                        if (isNowVisible)
                        {
                            primitive.ShowForPlayer(spectator);
                        }
                        else
                            primitive.DespawnForPlayer(spectator);
                    }
                }
            }
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

                SetTag(ev.Player);
            });
        }
    }
}