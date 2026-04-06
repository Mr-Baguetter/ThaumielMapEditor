// -----------------------------------------------------------------------
// <copyright file="PrefabHelper.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using AdminToys;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using MapGeneration.RoomConnectors;
using Mirror;
using ThaumielMapEditor.API.Data;
using UnityEngine;

namespace ThaumielMapEditor.API.Helpers
{
    public class PrefabHelper
    {
        /// <summary>
        /// A list of all registered prefabs and their associated collider data.
        /// </summary>
        public static List<PrefabCollidersData> PrefabColliders = [];

        /// <summary>
        /// Indicates whether <see cref="RegisterPrefabs"/> has been called.
        /// </summary>
        public static bool RanRegister = false;

        /// <summary>
        /// The network asset ID of the <see cref="PrimitiveObject"/> prefab.
        /// </summary>
        public static uint PrimitiveAssetId { get; private set; }

        /// <summary>
        /// The registered <see cref="PrimitiveObjectToy"/> prefab.
        /// </summary>
        public static PrimitiveObjectToy? PrimitiveObject { get; private set; }

        /// <summary>
        /// The registered <see cref="LightSourceToy"/> prefab.
        /// </summary>
        public static LightSourceToy? LightSource { get; private set; }

        /// <summary>
        /// The registered Light Containment Zone <see cref="DoorVariant"/> prefab.
        /// </summary>
        public static DoorVariant? DoorLcz { get; private set; }

        /// <summary>
        /// The registered Heavy Containment Zone <see cref="DoorVariant"/> prefab.
        /// </summary>
        public static DoorVariant? DoorHcz { get; private set; }

        /// <summary>
        /// The registered Entrance Zone <see cref="DoorVariant"/> prefab.
        /// </summary>
        public static DoorVariant? DoorEz { get; private set; }

        /// <summary>
        /// The registered Heavy Bulk <see cref="DoorVariant"/> prefab.
        /// </summary>
        public static DoorVariant? DoorHeavyBulk { get; private set; }

        /// <summary>
        /// The registered Gate <see cref="DoorVariant"/> prefab.
        /// </summary>
        public static DoorVariant? DoorGate { get; private set; }

        /// <summary>
        /// The registered <see cref="WorkstationController"/> prefab.
        /// </summary>
        public static WorkstationController? Workstation { get; private set; }

        /// <summary>
        /// The registered <see cref="InvisibleInteractableToy"/> prefab.
        /// </summary>
        public static InvisibleInteractableToy? Interactable { get; private set; }

        /// <summary>
        /// The registered <see cref="SpawnableCullingParent"/> prefab.
        /// </summary>
        public static SpawnableCullingParent? CullingParent { get; private set; }

        /// <summary>
        /// The registered <see cref="CapybaraToy"/> prefab.
        /// </summary>
        public static CapybaraToy? Capybara { get; private set; }

        /// <summary>
        /// The registered <see cref="TextToy"/> prefab.
        /// </summary>
        public static TextToy? TextToy { get; private set; }

        /// <summary>
        /// The registered <see cref="WaypointToy"/> prefab.
        /// </summary>
        public static WaypointToy? WaypointToy { get; private set; }

        /// <summary>
        /// The registered sport <see cref="ShootingTarget"/> prefab.
        /// </summary>
        public static ShootingTarget? ShootingTargetSport { get; private set; }

        /// <summary>
        /// The registered D-Boy <see cref="ShootingTarget"/> prefab.
        /// </summary>
        public static ShootingTarget? ShootingTargetDBoy { get; private set; }

        /// <summary>
        /// The registered binary <see cref="ShootingTarget"/> prefab.
        /// </summary>
        public static ShootingTarget? ShootingTargetBinary { get; private set; }

        /// <summary>
        /// The registered large gun <see cref="Locker"/> prefab.
        /// </summary>
        public static Locker? LockerLargeGun { get; private set; }

        /// <summary>
        /// The registered rifle rack <see cref="Locker"/> prefab.
        /// </summary>
        public static Locker? LockerRifleRack { get; private set; }

        /// <summary>
        /// The registered miscellaneous <see cref="Locker"/> prefab.
        /// </summary>
        public static Locker? LockerMisc { get; private set; }

        /// <summary>
        /// The registered regular medkit <see cref="Locker"/> prefab.
        /// </summary>
        public static Locker? LockerRegularMedkit { get; private set; }

        /// <summary>
        /// The registered adrenaline medkit <see cref="Locker"/> prefab.
        /// </summary>
        public static Locker? LockerAdrenalineMedkit { get; private set; }

        /// <summary>
        /// The registered experimental weapon <see cref="Locker"/> prefab.
        /// </summary>
        public static Locker? LockerExperimentalWeapon { get; private set; }

        /// <summary>
        /// The registered pedestal <see cref="Locker"/> prefab.
        /// </summary>
        public static Locker? Pedestal { get; private set; }

        /// <summary>
        /// The registered Light Containment Zone <see cref="Scp079CameraToy"/> prefab.
        /// </summary>
        public static Scp079CameraToy? CameraLcz { get; private set; }

        /// <summary>
        /// The registered Heavy Containment Zone <see cref="Scp079CameraToy"/> prefab.
        /// </summary>
        public static Scp079CameraToy? CameraHcz { get; private set; }

        /// <summary>
        /// The registered Surface Zone <see cref="Scp079CameraToy"/> prefab.
        /// </summary>
        public static Scp079CameraToy? CameraSz { get; private set; }

        /// <summary>
        /// The registered Entrance Zone arm <see cref="Scp079CameraToy"/> prefab.
        /// </summary>
        public static Scp079CameraToy? CameraEzArm { get; private set; }

        /// <summary>
        /// The registered Entrance Zone <see cref="Scp079CameraToy"/> prefab.
        /// </summary>
        public static Scp079CameraToy? CameraEz { get; private set; }

        /// <summary>
        /// The registered simple boxes clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? SimpleBoxes { get; private set; }

        /// <summary>
        /// The registered short pipes clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? PipesShort { get; private set; }

        /// <summary>
        /// The registered boxes ladder clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? BoxesLadder { get; private set; }

        /// <summary>
        /// The registered tank-supported shelf clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? TankSupportedShelf { get; private set; }

        /// <summary>
        /// The registered angled fences clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? AngledFences { get; private set; }

        /// <summary>
        /// The registered huge orange pipes clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? HugeOrangePipes { get; private set; }

        /// <summary>
        /// The registered long pipes clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? PipesLong { get; private set; }

        /// <summary>
        /// The registered broken electrical box clutter <see cref="GameObject"/> prefab.
        /// </summary>
        public static GameObject? BrokenElectricalBox { get; private set; }

        /// <summary>
        /// The registered generator <see cref="Scp079Generator"/> prefab.
        /// </summary>
        public static Scp079Generator? Generator { get; private set; }

        /// <summary>
        /// Iterates over all registered network prefabs and caches them for use by the map editor.
        /// </summary>
        public static void RegisterPrefabs()
        {
            foreach (GameObject prefab in NetworkClient.prefabs.Values.ToArray())
            {
                PrefabColliders.Add(new()
                {
                    Prefab = prefab,
                    Colliders = ColliderData.ParseObjectColliders(prefab)
                });
                
                if (prefab.TryGetComponent<PrimitiveObjectToy>(out var primitiveObject))
                {
                    PrimitiveObject = primitiveObject;
                    PrimitiveAssetId = prefab.GetComponent<NetworkIdentity>().assetId;
                    if (PrimitiveAssetId == 0)
                    {
                        LogManager.Warn($"Failed to get AssetId for Primitives.");
                    }
                    else
                        LogManager.Debug($"Got AssetId {PrimitiveAssetId} for Primitives");

                    continue;
                }
                if (prefab.TryGetComponent<LightSourceToy>(out var lightSource))
                {
                    LightSource = lightSource;
                    continue;
                }
                if (prefab.TryGetComponent<DoorVariant>(out var doorVariant))
                {
                    switch (prefab.name)
                    {
                        case "LCZ BreakableDoor":
                            DoorLcz = doorVariant;
                            continue;

                        case "HCZ BreakableDoor":
                            DoorHcz = doorVariant;
                            continue;

                        case "EZ BreakableDoor":
                            DoorEz = doorVariant;
                            continue;

                        case "HCZ BulkDoor":
                            DoorHeavyBulk = doorVariant;
                            continue;

                        case "Spawnable Unsecured Pryable GateDoor":
                            DoorGate = doorVariant;
                            continue;
                    }
                }
                if (prefab.TryGetComponent<WorkstationController>(out var workstation))
                {
                    Workstation = workstation;
                    continue;
                }
                if (prefab.TryGetComponent<InvisibleInteractableToy>(out var interactable))
                {
                    Interactable = interactable;
                    continue;
                }
                if (prefab.TryGetComponent<SpawnableCullingParent>(out var cullingParent))
                {
                    CullingParent = cullingParent;
                    continue;
                }
                if (prefab.TryGetComponent<CapybaraToy>(out var capybara))
                {
                    Capybara = capybara;
                    continue;
                }
                if (prefab.TryGetComponent<TextToy>(out var texttoy))
                {
                    TextToy = texttoy;
                    continue;
                }
                if (prefab.GetComponent<SpawnableClutterConnector>())
                {
                    switch (prefab.name)
                    {
                        case "Simple Boxes Open Connector":
                            SimpleBoxes = prefab;
                            continue;

                        case "Pipes Short Open Connector":
                            PipesShort = prefab;
                            continue;

                        case "Boxes Ladder Open Connector":
                            BoxesLadder = prefab;
                            continue;

                        case "Tank-Supported Shelf Open Connector":
                            TankSupportedShelf = prefab;
                            continue;

                        case "Angled Fences Open Connector":
                            AngledFences = prefab;
                            continue;

                        case "Huge Orange Pipes Open Connector":
                            HugeOrangePipes = prefab;
                            continue;

                        case "Pipes Long Open Connector":
                            PipesLong = prefab;
                            continue;
                    }
                }
                if (prefab.TryGetComponent<Scp079Generator>(out var generator))
                {
                    Generator = generator;
                    continue;
                }
                if (prefab.name == "Broken Electrical Box Open Connector")
                {
                    BrokenElectricalBox = prefab;
                    continue;
                }
                if (prefab.TryGetComponent(out Scp079CameraToy cameraToy))
                {
                    switch (prefab.name)
                    {
                        case "LczCameraToy":
                            CameraLcz = cameraToy;
                            continue;

                        case "HczCameraToy":
                            CameraHcz = cameraToy;
                            continue;

                        case "SzCameraToy":
                            CameraSz = cameraToy;
                            continue;

                        case "EzArmCameraToy":
                            CameraEzArm = cameraToy;
                            continue;

                        case "EzCameraToy":
                            CameraEz = cameraToy;
                            continue;
                    }
                }
                if (prefab.TryGetComponent(out WaypointToy waypointToy))
                {
                    WaypointToy = waypointToy;
                    continue;
                }
                if (prefab.TryGetComponent(out Locker locker))
                {
                    switch (prefab.name)
                    {
                        case "LargeGunLockerStructure":
                            LockerLargeGun = locker;
                            continue;

                        case "RifleRackStructure":
                            LockerRifleRack = locker;
                            continue;

                        case "MiscLocker":
                            LockerMisc = locker;
                            continue;

                        case "RegularMedkitStructure":
                            LockerRegularMedkit = locker;
                            continue;

                        case "AdrenalineMedkitStructure":
                            LockerAdrenalineMedkit = locker;
                            continue;

                        case "Scp500PedestalStructure Variant":
                            Pedestal = locker;
                            continue;

                        case "Experimental Weapon Locker":
                            LockerExperimentalWeapon = locker;
                            continue;
                    }
                }

                if (prefab.TryGetComponent(out ShootingTarget shootingTarget))
                {
                    switch (prefab.name)
                    {
                        case "sportTargetPrefab":
                            ShootingTargetSport = shootingTarget;
                            continue;

                        case "dboyTargetPrefab":
                            ShootingTargetDBoy = shootingTarget;
                            continue;

                        case "binaryTargetPrefab":
                            ShootingTargetBinary = shootingTarget;
                            continue;
                    }
                }
            }

            RanRegister = true;
        }
    }
}