using System.Linq;
using AdminToys;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms.Attachments;
using MapGeneration.Distributors;
using MapGeneration.RoomConnectors;
using Mirror;
using UnityEngine;

namespace ThaumielMapEditor.API.Helpers
{
    public class PrefabHelper
    {
        public static bool RanRegister = false;

        #region AssetIds
        public static uint PrimitiveAssetId { get; private set; }
        public static uint LightAssetId { get; private set; }
        public static uint CullingParentAssetId { get; private set; }
        public static uint TextToyAssetId { get; private set; }
        #endregion

        public static PrimitiveObjectToy? PrimitiveObject { get; private set; }
        public static LightSourceToy? LightSource { get; private set; }
        public static DoorVariant? DoorLcz { get; private set; }
        public static DoorVariant? DoorHcz { get; private set; }
        public static DoorVariant? DoorEz { get; private set; }
        public static DoorVariant? DoorHeavyBulk { get; private set; }
        public static DoorVariant? DoorGate { get; private set; }
        public static WorkstationController? Workstation { get; private set; }
        public static InvisibleInteractableToy? Interactable { get; private set; }
        public static SpawnableCullingParent? CullingParent { get; private set; }
        public static CapybaraToy? Capybara { get; private set; }
        public static TextToy? TextToy { get; private set; }
        public static WaypointToy? WaypointToy { get; private set; }

#region Targets
        public static ShootingTarget? ShootingTargetSport { get; private set; }
        public static ShootingTarget? ShootingTargetDBoy { get; private set; }
        public static ShootingTarget? ShootingTargetBinary { get; private set; }
#endregion

        #region Lockers
        public static Locker? LockerLargeGun { get; private set; }
        public static Locker? LockerRifleRack { get; private set; }
        public static Locker? LockerMisc { get; private set; }
        public static Locker? LockerRegularMedkit { get; private set; }
        public static Locker? LockerAdrenalineMedkit { get; private set; }
        public static Locker? LockerExperimentalWeapon { get; private set; }
        public static Locker? Pedestal { get; private set; }
        #endregion

        #region Cameras
        public static Scp079CameraToy? CameraLcz { get; private set; }
        public static Scp079CameraToy? CameraHcz { get; private set; }
        public static Scp079CameraToy? CameraSz { get; private set; }
        public static Scp079CameraToy? CameraEzArm { get; private set; }
        public static Scp079CameraToy? CameraEz { get; private set; }
        #endregion

        #region  Clutter
        public static GameObject? SimpleBoxes { get; private set; }
        public static GameObject? PipesShort { get; private set; }
        public static GameObject? BoxesLadder { get; private set; }
        public static GameObject? TankSupportedShelf { get; private set; }
        public static GameObject? AngledFences { get; private set; }
        public static GameObject? HugeOrangePipes { get; private set; }
        public static GameObject? PipesLong { get; private set; }
        public static GameObject? BrokenElectricalBox { get; private set; }
        public static GameObject? PrismaticCloud { get; private set; }
        public static GameObject? BrownCandyTantrum { get; private set; }
        #endregion

        public static void RegisterPrefabs()
        {
            foreach (GameObject prefab in NetworkClient.prefabs.Values.ToArray())
            {
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
                    LightAssetId = prefab.GetComponent<NetworkIdentity>().assetId;
                    if (LightAssetId == 0)
                    {
                        LogManager.Warn($"Failed to get AssetId for Lights.");
                    }
                    else
                        LogManager.Debug($"Got AssetId {LightAssetId} for Lights");

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
                    CullingParentAssetId = prefab.GetComponent<NetworkIdentity>().assetId;
                    if (CullingParentAssetId == 0)
                    {
                        LogManager.Warn($"Failed to get AssetId for CullingParent.");
                    }
                    else
                        LogManager.Debug($"Got AssetId {CullingParentAssetId} for CullingParent");

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
                    TextToyAssetId = prefab.GetComponent<NetworkIdentity>().assetId;
                    if (TextToyAssetId == 0)
                    {
                        LogManager.Warn($"Failed to get AssetId for TextToy.");
                    }
                    else
                        LogManager.Debug($"Got AssetId {TextToyAssetId} for TextToy");

                    continue;
                }
                if (prefab.GetComponent<SpawnableClutterConnector>())
                {
                    switch (prefab.name)
                    {
                        case "Simple Boxes Open Connector":
                            SimpleBoxes = prefab;
                            break;

                        case "Pipes Short Open Connector":
                            PipesShort = prefab;
                            break;

                        case "Boxes Ladder Open Connector":
                            BoxesLadder = prefab;
                            break;

                        case "Tank-Supported Shelf Open Connector":
                            TankSupportedShelf = prefab;
                            break;

                        case "Angled Fences Open Connector":
                            AngledFences = prefab;
                            break;

                        case "Huge Orange Pipes Open Connector":
                            HugeOrangePipes = prefab;
                            break;

                        case "Pipes Long Open Connector":
                            PipesLong = prefab;
                            break;
                    }

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