// -----------------------------------------------------------------------
// <copyright file="PlayerBlocks.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace ThaumielMapEditor.API.Helpers.BlockParser
{
    public class PlayerGetPropertyBlock : BlockBase
    {
        public string Property { get; set; } = string.Empty;

        public override object ReturnExecute(object obj)
        {
            if (obj is not Player player)
            {
                LogManager.Warn("obj is not a Player.");
                return null!;
            }

            if (string.IsNullOrEmpty(Property))
                return null!;

#pragma warning disable CS8603
            return Property switch
            {
                "Position" => player.Position,
                "Rotation" => player.Rotation,
                "Scale" => player.Scale,
                "Role" => player.Role,
                "PlayerId" => player.PlayerId,
                "Name" => player.Nickname,
                "DisplayName" => player.DisplayName,
                "UserId" => player.UserId,
                "IsAlive" => player.IsAlive,
                "IsNpc" => player.IsNpc,
                "IsHost" => player.IsHost,
                "Health" => player.Health,
                "MaxHealth" => player.MaxHealth,
                "ArtificialHealth" => player.ArtificialHealth,
                "MaxArtificialHealth" => player.MaxArtificialHealth,
                "HumeShield" => player.HumeShield,
                "MaxHumeShield" => player.MaxHumeShield,
                "Team" => player.Team,
                "Faction" => player.Faction,
                "IsSCP" => player.IsSCP,
                "IsHuman" => player.IsHuman,
                "IsMuted" => player.IsMuted,
                "IsGodModeEnabled" => player.IsGodModeEnabled,
                "IsNoclipEnabled" => player.IsNoclipEnabled,
                "IsBypassEnabled" => player.IsBypassEnabled,
                "IsOverwatchEnabled" => player.IsOverwatchEnabled,
                "IsDisarmed" => player.IsDisarmed,
                "GroupName" => player.GroupName,
                "Velocity" => player.Velocity,
                "Room" => player.Room,
                "Zone" => player.Zone,
                _ => LogUnknownProperty(Property)
            };
#pragma warning restore CS8603
        }

        private static object LogUnknownProperty(string property)
        {
            LogManager.Warn($"PlayerGetPropertyBlock: Unknown property '{property}'.");
            return null!;
        }
    }

    public class PlayerSetGravityBlock : BlockBase
    {
        public float X { get; set; }
        public float Y { get; set; } = -19.86f;
        public float Z { get; set; }

        public override void Execute(Player player)
        {
            player.Gravity = new Vector3(X, Y, Z);
            LogManager.Debug($"Set gravity for player '{player.DisplayName}' to ({X}, {Y}, {Z}).");
        }
    }

    public class PlayerGetByIdBlock : BlockBase
    {
        public int PlayerId { get; set; }

        public override object ReturnExecute(object obj)
        {
            Player? player = Player.Get(PlayerId);
            if (player == null)
            {
                LogManager.Warn($"PlayerGetByIdBlock: No player found with id '{PlayerId}'.");
                return null!;
            }

            return player;
        }
    }

    public class PlayerGetByUserIdBlock : BlockBase
    {
        public string UserId { get; set; } = string.Empty;

        public override object ReturnExecute(object obj)
        {
            Player? player = Player.Get(UserId);
            if (player == null)
            {
                LogManager.Warn($"PlayerGetByUserIdBlock: No player found with user id '{UserId}'.");
                return null!;
            }

            return player;
        }
    }

    public class PlayerSetRoleBlock : BlockBase
    {
        public RoleTypeId Role { get; set; }
        public bool KeepPosition { get; set; } = true;

        public override void Execute(Player player)
        {
            LogManager.Debug($"Setting role for player '{player.DisplayName}' to '{Role}'. KeepPosition={KeepPosition}.");
            player.SetRole(Role, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
        }
    }

    public class PlayerGiveItemBlock : BlockBase
    {
        public ItemType Item { get; set; }
        public int Amount { get; set; } = 1;
        public bool DropIfFull { get; set; } = true;

        public override void Execute(Player player)
        {
            for (int i = 0; i < Amount; i++)
            {
                if (player.IsInventoryFull)
                {
                    if (!DropIfFull)
                    {
                        LogManager.Debug($"Inventory full for '{player.DisplayName}', skipping remaining {Amount - i} item(s).");
                        break;
                    }

                    Pickup.Create(Item, player.Position)?.Spawn();
                    LogManager.Debug($"Inventory full for '{player.DisplayName}', dropped '{Item}' instead.");
                    continue;
                }

                player.AddItem(Item);
            }
        }
    }

    public class PlayerRemoveItemBlock : BlockBase
    {
        public ItemType Item { get; set; }

        public override void Execute(Player player)
        {
            Item? item = player.Items.Where(i => i.Type == Item).First();
            if (item != null)
            {
                player.RemoveItem(item);
            }
            else
                LogManager.Warn($"Player '{player.DisplayName}' does not have item '{Item}'.");
        }
    }

    public class PlayerSetHealthBlock : BlockBase
    {
        public string HealthType { get; set; } = string.Empty;
        public float Value { get; set; }

        public override void Execute(Player player)
        {
            switch (HealthType)
            {
                case "health":
                    player.Health = Value;
                    break;
                case "max_health":
                    player.MaxHealth = Value;
                    break;
                case "artificial_health":
                    player.ArtificialHealth = Value;
                    break;
                case "max_artificial_health":
                    player.MaxArtificialHealth = Value;
                    break;
                case "hume_shield":
                    player.HumeShield = Value;
                    break;
                case "max_hume_shield":
                    player.MaxHumeShield = Value;
                    break;
                case "hume_shield_regen_rate":
                    player.HumeShieldRegenRate = Value;
                    break;
                case "hume_shield_regen_cooldown":
                    player.HumeShieldRegenCooldown = Value;
                    break;
                default:
                    LogManager.Warn($"Unknown health type '{HealthType}'.");
                    break;
            }

            LogManager.Debug($"Set '{HealthType}' to '{Value}' for player '{player.DisplayName}'.");
        }
    }

    public class PlayerSetGroupBlock : BlockBase
    {
        public string GroupType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public override void Execute(Player player)
        {
            switch (GroupType)
            {
                case "name":
                    player.GroupName = Value;
                    break;
                case "color":
                    player.GroupColor = Value;
                    break;
                default:
                    LogManager.Warn($"Unknown group type '{GroupType}'.");
                    break;
            }

            LogManager.Debug($"Set group '{GroupType}' to '{Value}' for player '{player.DisplayName}'.");
        }
    }

    public class PlayerSendHintBlock : BlockBase
    {
        public string Message { get; set; } = string.Empty;
        public float Duration { get; set; } = 5f;
        public override void Execute(Player player)
        {
            player.SendHint(Message, Duration);
            LogManager.Debug($"Sent hint to player '{player.DisplayName}': '{Message}' (Duration: {Duration}s).");
        }
    }

    public class PlayerSendBroadcastBlock : BlockBase
    {
        public string Message { get; set; } = string.Empty;
        public ushort Duration { get; set; } = 5;
        public override void Execute(Player player)
        {
            player.SendBroadcast(Message, Duration);
            LogManager.Debug($"Sent broadcast to player '{player.DisplayName}': '{Message}' (Duration: {Duration}s).");
        }
    }
}