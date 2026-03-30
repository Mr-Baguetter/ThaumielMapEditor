using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Blocks.ClientSide;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;
using System;
using ThaumielMapEditor.API.Blocks.Areas;
using Mirror;
using ThaumielMapEditor.API.Helpers;

namespace ThaumielMapEditor.API.Data
{
    public class SchematicData
    {
        /// <summary>
        /// Fired when the <see cref="Position"/> is set;
        /// </summary>
        public static event Action<SchematicData>? SchematicPositionUpdated;

        /// <summary>
        /// Fired when the <see cref="Rotation"/> is set;
        /// </summary>
        public static event Action<SchematicData>? SchematicRotationUpdated;

        /// <summary>
        /// Gets or sets whether or not this <see cref="SchematicData"/> has a animator in it when built from unity.
        /// </summary>
        public bool ContainsAnimator { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; internal set; }= string.Empty;

        /// <summary>
        /// Gets the root object id.
        /// </summary>
        public int RootObjectId { get; internal set; }

        /// <summary>
        /// Gets or sets the id
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Gets or sets the position of this <see cref="SchematicData"/> instance.
        /// </summary>
        public Vector3 Position
        {
            get;
            set
            {
                field = value;
                SchematicPositionUpdated?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the rotation of this <see cref="SchematicData"/> instance.
        /// </summary>
        public Quaternion Rotation
        {
            get;
            set
            {
                field = value;
                SchematicRotationUpdated?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the scale of this <see cref="SchematicData"/> instance.
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Gets the base <see cref="LabPrimitive"/> that all client primtives will be parented to.
        /// </summary>
        public LabPrimitive? Primitive { get; internal set; }

        /// <summary>
        /// A list of all spawned <see cref="ClientSideObjectBase"/> instances.
        /// </summary>
        public List<ClientSideObjectBase> SpawnedClientObjects = [];

        /// <summary>
        /// A list of all spawned <see cref="ServerObject"/> instances.
        /// </summary>
        public List<ServerObject> SpawnedServerObjects = [];
        
        /// <summary>
        /// A list of all spawned <see cref="AreaObject"/> instances.
        /// </summary>
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

        /// <summary>
        /// Syncs the <see cref="ClientSideObjectBase"/> of this <see cref="SchematicData"/> with the specified <see cref="Player"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to sync with.</param>
        public void SyncWithPlayer(Player player)
        {
            foreach (ClientSideObjectBase objects in SpawnedClientObjects)
            {
                objects.SpawnForPlayer(player);
            }
        }

        internal void Destroy()
        {
            foreach (ClientSideObjectBase clientobj in SpawnedClientObjects.ToArray())
            {
                clientobj.DestroyForAllPlayers();
                SpawnedClientObjects.Remove(clientobj);
            }

            foreach (ServerObject serverobj in SpawnedServerObjects.ToArray())
            {
                serverobj.DestroyObject(this);
            }
        }
    }
}