using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.RoomConnectors;
using Mirror;
using System;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class DoorObject : ServerObject
    {
        public DoorVariant? GetDoorFromType(DoorType type)
        {
            return type switch
            {
                DoorType.Lcz => PrefabHelper.DoorLcz,
                DoorType.Hcz => PrefabHelper.DoorHcz,
                DoorType.Ez => PrefabHelper.DoorEz,
                DoorType.Gate => PrefabHelper.DoorGate,
                DoorType.BulkHead => PrefabHelper.DoorHeavyBulk,
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown door type: {type}")
            };
        }

        [YamlIgnore]
        public DoorVariant? Base { get; internal set; }

        public DoorType DoorType { get; set; }
        public DoorPermissionFlags Permissions
        {
            get;
            set
            {
                if (field == value)
                    return;

                Base?.RequiredPermissions = new(value, RequireAllPermissions, Bypass2176);
                field = value;
            }
        }

        public bool RequireAllPermissions
        {
            get;
            set
            {
                if (field == value)
                    return;

                Base?.RequiredPermissions = new(Permissions, value, Bypass2176);
                field = value;
            }
        }

        public bool Bypass2176
        {
            get;
            set
            {
                if (field == value)
                    return;

                Base?.RequiredPermissions = new(Permissions, RequireAllPermissions, value);
                field = value;
            }
        }

        public float MaxHealth
        {
            get;
            set
            {
                if (field == value || Base == null || Base is not BreakableDoor breakable)
                    return;

                breakable.MaxHealth = value;
                field = value;
            }
        }

        public float Health
        {
            get;
            set
            {
                if (field == value || Base == null || Base is not BreakableDoor breakable)
                    return;

                breakable.RemainingHealth = value;
                field = value;
            }
        }

        public bool IsOpen
        {
            get;
            set
            {
                if (field == value)
                    return;

                if (value)
                {
                    DoorEvents.TriggerAction(Base, DoorAction.Opened, null);
                }
                else
                {
                    DoorEvents.TriggerAction(Base, DoorAction.Closed, null);
                }

                field = value;
            }
        }
        
        public bool IsLocked 
        {
            get;
            set
            {
                if (field == value)
                    return;

                Base?.ServerChangeLock(DoorLockReason.AdminCommand, value);
                field = value;
            }
        }

        public override ObjectType ObjectType { get; set; } = ObjectType.Door;

        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            Base = GetDoorFromType(DoorType);
            if (Base == null)
            {
                LogManager.Warn($"Failed to get DoorVariant from {schematic.FileName}, DoorType: {DoorType}");
                return;
            }

            GameObject doorPrefab = UnityEngine.Object.Instantiate(Base).gameObject;
            NetworkServer.UnSpawn(doorPrefab);
            if (doorPrefab.TryGetComponent<WallableSmallNodeRoomConnector>(out var con) && DoorType == DoorType.Hcz)
                con.Network_syncBitmask = 3;

            Object = doorPrefab;
            NetId = Base.netId;
            SetWorldTransform(schematic);
            ApplyProperties(doorPrefab);
            NetworkServer.Spawn(doorPrefab);
            base.SpawnObject(schematic, serializable);
        }

        public void ApplyProperties(GameObject prefab)
        {
            if (!prefab.TryGetComponent<DoorVariant>(out var door))
                return;
            
            if (door is BreakableDoor breakable)
            {
                breakable.MaxHealth = MaxHealth;
                breakable.RemainingHealth = Health;
            }

            if (IsLocked)
                door.ServerChangeLock(DoorLockReason.AdminCommand, true);

            if (IsOpen)
                DoorEvents.TriggerAction(door, DoorAction.Opened, null);

            door.RequiredPermissions = new(Permissions, RequireAllPermissions, Bypass2176);
        }

        public override string ToString()
        {
            return $"DoorType: {DoorType}, Permissions: {Permissions}, RequireAllPermissions: {RequireAllPermissions}, Bypass2176: {Bypass2176}, MaxHealth: {MaxHealth}, Health: {Health}, IsOpen: {IsOpen}, IsLocked: {IsLocked}";
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType is not ObjectType.Door)
                return;

            if (!serializable.Values.TryConvertValue<DoorType>("DoorType", out var doorType))
            {
                LogManager.Warn("Failed to parse DoorType");
                return;
            }
            if (!serializable.Values.TryConvertValue<DoorPermissionFlags>("Permissions", out var permissions))
            {
                LogManager.Warn("Failed to parse Permissions");
                return;
            }
            if (!serializable.Values.TryConvertValue<bool>("RequireAllPermissions", out var requireAllPermissions))
            {
                LogManager.Warn("Failed to parse RequireAllPermissions");
                return;
            }
            if (!serializable.Values.TryConvertValue<bool>("Bypass2176", out var bypass2176))
            {
                LogManager.Warn("Failed to parse Bypass2176");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("MaxHealth", out var maxHealth))
            {
                LogManager.Warn("Failed to parse MaxHealth");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("Health", out var health))
            {
                LogManager.Warn("Failed to parse Health");
                return;
            }
            if (!serializable.Values.TryConvertValue<bool>("IsOpen", out var isOpen))
            {
                LogManager.Warn("Failed to parse IsOpen");
                return;
            }
            if (!serializable.Values.TryConvertValue<bool>("IsLocked", out var isLocked))
            {
                LogManager.Warn("Failed to parse IsLocked");
                return;
            }

            DoorType = doorType;
            Permissions = permissions;
            RequireAllPermissions = requireAllPermissions;
            Bypass2176 = bypass2176;
            MaxHealth = maxHealth;
            Health = health;
            IsOpen = isOpen;
            IsLocked = isLocked;
        }
    }
}