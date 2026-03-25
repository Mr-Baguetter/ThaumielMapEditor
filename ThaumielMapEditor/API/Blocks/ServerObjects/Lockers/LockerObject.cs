using System;
using System.Collections.Generic;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects.Lockers
{
    public class LockerObject : ServerObject
    {
        [YamlIgnore]
        public Locker Base { get; private set; }

        public List<LockerChamber> Chambers { get; private set; }

        public override ObjectType ObjectType { get; set; } = ObjectType.Locker;

        public LockerType Type { get; private set; }

        public Locker? GetPrefabFromType(LockerType type)
        {
            return type switch
            {
                LockerType.Adrenaline => PrefabHelper.LockerAdrenalineMedkit,
                LockerType.ExperimentalWeapon => PrefabHelper.LockerExperimentalWeapon,
                LockerType.LargeGun => PrefabHelper.LockerLargeGun,
                LockerType.Medkit => PrefabHelper.LockerRegularMedkit,
                LockerType.Misc => PrefabHelper.LockerMisc,
                LockerType.Pedestal => PrefabHelper.Pedestal,
                LockerType.RifleRack => PrefabHelper.LockerRifleRack,
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown LockerType: {type}")
            };
        }

        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            ParseValues(serializable);
            Locker? prefab = GetPrefabFromType(Type);
            if (prefab is null)
            {
                LogManager.Warn($"Failed to get locker prefab.");
                return;
            }

            GameObject lockerobj = UnityEngine.Object.Instantiate(prefab.gameObject);
            if (!lockerobj.TryGetComponent<Locker>(out var locker))
            {
                LogManager.Warn($"Failed to get Locker component from prefab.");
                return;
            }

            Base = locker;
            Object = locker.gameObject;
            NetworkServer.UnSpawn(locker.gameObject);
            SetWorldTransform(schematic);
            locker.gameObject.name += " [TME Locker]";
            if (locker.TryGetComponent<StructurePositionSync>(out var posSync))
            {
                posSync.Network_position = Position;
                posSync.Network_rotationY = (sbyte)Mathf.RoundToInt(Rotation.eulerAngles.y / 5.625f);
            }

            locker.ParentRoom = RoomExtensions.GetClosestRoomToPosition(Position);
            locker.gameObject.SetActive(true);
            NetworkServer.Spawn(locker.gameObject);
            ClearChambers();
            PopulateChambers();
            base.SpawnObject(schematic, serializable);
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Locker)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Locker.");                
                return;
            }

            if (!serializable.Values.TryConvertValue<LockerType>("LockerType", out var lockerType))
            {
                LogManager.Warn("Failed to parse LockerType");
                return;
            }

            if (!serializable.Values.TryConvertValue<List<LockerChamber>>("Chambers", out var chambers))
            {
                LogManager.Warn("Failed to parse LockerChambers");
                return;
            }

            Type = lockerType;
            Chambers = chambers;
        }

        public void ClearChambers()
        {
            Base.Loot = [];

            foreach (MapGeneration.Distributors.LockerChamber chamber in Base.Chambers)
            {
                foreach (ItemPickupBase item in chamber.Content)
                {
                    item.DestroySelf();
                }

                chamber.Content.Clear();
                chamber.ToBeSpawned.Clear();
            }

        }

        public void PopulateChambers()
        {
            List<LockerChamber> Spawned = [];

            foreach (MapGeneration.Distributors.LockerChamber chamber in Base.Chambers)
            {
                foreach (LockerChamber lockerChamber in Chambers)
                {
                    if (lockerChamber.Index != Base.Chambers.IndexOf(chamber))
                        continue;

                    chamber.RequiredPermissions = lockerChamber.Permissions;
                    foreach (ChamberData chamberData in lockerChamber.Data)
                    {
                        if (UnityEngine.Random.Range(0, 101) > chamberData.SpawnPercent)
                            continue;
                        if (Spawned.Contains(lockerChamber))
                            continue;

                        chamber.SpawnItem(chamberData.ItemType, chamberData.AmountToSpawn);
                        LogManager.Debug($"Spawned item in Chamber {Base.Chambers.IndexOf(chamber)}");
                        Spawned.Add(lockerChamber);
                    }
                }
            }
        }
    }
}