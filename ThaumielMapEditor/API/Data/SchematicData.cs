using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Blocks.ClientSide;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;
using System;
using ThaumielMapEditor.API.Blocks.Areas;

namespace ThaumielMapEditor.API.Data
{
    public class SchematicData
    {
        public static event Action<SchematicData>? SchematicPositionUpdated;
        public static event Action<SchematicData>? SchematicRotationUpdated;

        public bool ContainsAnimator { get; set; }
        public string FileName = string.Empty;
        public int RootObjectId { get; internal set; }
        public uint Id;

        public Vector3 Position
        {
            get;
            set
            {
                field = value;
                SchematicPositionUpdated?.Invoke(this);
            }
        }

        public Quaternion Rotation
        {
            get;
            set
            {
                field = value;
                SchematicRotationUpdated?.Invoke(this);
            }
        }

        public Vector3 Scale { get; set; }

        public LabPrimitive? Primitive;

        public List<ClientSideObjectBase> SpawnedClientObjects = [];

        public List<ServerObject> SpawnedServerObjects = [];
        
        public List<AreaObject> SpawnedAreas = [];

#region ClientObjects
        /// <summary>
        /// Gets all spawned <see cref="PrimitiveObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<PrimitiveObject> Primitives =>
            SpawnedClientObjects.OfType<PrimitiveObject>();

        /// <summary>
        /// Gets all spawned <see cref="CapybaraObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<CapybaraObject> Capybaras =>
            SpawnedClientObjects.OfType<CapybaraObject>();

        /// <summary>
        /// Gets all spawned <see cref="LightObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<LightObject> Lights =>
            SpawnedClientObjects.OfType<LightObject>();
            
#endregion

#region ServerObjects
        /// <summary>
        /// Gets all spawned <see cref="CameraObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<CameraObject> Cameras =>
            SpawnedServerObjects.OfType<CameraObject>();

        /// <summary>
        /// Gets all spawned <see cref="DoorObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<DoorObject> Doors =>
            SpawnedServerObjects.OfType<DoorObject>();

        /// <summary>
        /// Gets all spawned <see cref="ClutterObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<ClutterObject> Clutter =>
            SpawnedServerObjects.OfType<ClutterObject>();

        /// <summary>
        /// Gets all spawned <see cref="InteractionObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<InteractionObject> Interactables =>
            SpawnedServerObjects.OfType<InteractionObject>();
            
        /// <summary>
        /// Gets all spawned <see cref="PickupObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<PickupObject> Pickups =>
            SpawnedServerObjects.OfType<PickupObject>();

        /// <summary>
        /// Gets all spawned <see cref="TargetDummyObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<TargetDummyObject> TargetDummies =>
            SpawnedServerObjects.OfType<TargetDummyObject>();

        /// <summary>
        /// Gets all spawned <see cref="TextToyObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<TextToyObject> TextToys =>
            SpawnedServerObjects.OfType<TextToyObject>();

        /// <summary>
        /// Gets all spawned <see cref="WaypointObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<WaypointObject> Waypoints =>
            SpawnedServerObjects.OfType<WaypointObject>();

        /// <summary>
        /// Gets all spawned <see cref="WorkstationObject"/>s belonging to this schematic.
        /// </summary>
        public IEnumerable<WorkstationObject> Workstations =>
            SpawnedServerObjects.OfType<WorkstationObject>();
#endregion
        public void SyncWithPlayer(Player player)
        {
            foreach (ClientSideObjectBase objects in SpawnedClientObjects)
            {
                objects.SpawnForPlayer(player);
            }
        }
    }
}