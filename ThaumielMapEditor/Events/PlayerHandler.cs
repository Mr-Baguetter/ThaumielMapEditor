// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;

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

        // If you contribute and want a CreditTag add your own steamid to this.
        internal static readonly Dictionary<string, CreditTag> Credits = new()
        {
            // MrBaguetter
            ["76561199150506472@steam"] = new CreditTag()
            {
                Name = "TME Lead Developer",
                Color = "pumpkin"
            },

            // Example contributor badge
            ["EXAMPLE99@steam"] = new CreditTag()
            {
                Name = "TME Contributor",
                Color = "red"
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

                if (Main.Instance.Config!.EnableCreditTags && Credits.TryGetValue(ev.Player.UserId, out var credittag) && ev.Player.UserGroup == null)
                {
                    ev.Player.GroupColor = credittag.Color;
                    ev.Player.GroupName = credittag.Name;
                }
            });
        }
    }
}