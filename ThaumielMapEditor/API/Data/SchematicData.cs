// -----------------------------------------------------------------------
// <copyright file="SchematicData.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Blocks.ClientSide;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;
using System;
using ThaumielMapEditor.API.Animation;
using ThaumielMapEditor.API.Components.Tools;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.Events.EventArgs.Handlers;
using UnityEngine;
using ThaumielMapEditor.API.Enums;

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
        /// Gets the room this <see cref="SchematicData"/> instance was spawned in.
        /// </summary>
        public Room? Room { get; internal set; }

        public AnimationController AnimationController => AnimationController.Get(this);

        /// <summary>
        /// Gets or sets the scale of this <see cref="SchematicData"/> instance.
        /// </summary>
        public Vector3 Scale { get; set; }

        public Dictionary<int, Transform> ServerSideTransforms = [];

        /// <summary>
        /// Gets the base <see cref="LabPrimitive"/> that all client primtives will be parented to.
        /// </summary>
        public LabPrimitive? Primitive { get; internal set; }

        /// <summary>
        /// A list of the spawned <see cref="LODData"/> instances
        /// </summary>
        public List<LODData> LODZones { get; internal set; } = [];

        /// <summary>
        /// A list of all spawned <see cref="ClientSideObjectBase"/> instances.
        /// </summary>
        public List<ClientSideObjectBase> SpawnedClientObjects = [];

        /// <summary>
        /// A list of all spawned <see cref="ServerObject"/> instances.
        /// </summary>
        public List<ServerObject> SpawnedServerObjects = [];

        /// <summary>
        /// The <see cref="BlockExecutor"/> of this schematic.
        /// </summary>
        public BlockExecutor? Executor { get; internal set; }

        /// <summary>
        /// Retrieves all spawned <see cref="ClientSideObjectBase"/>s of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ClientSideObjectBase"/> to filter.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all <see cref="ClientSideObjectBase"/>s that match type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> GetClientObject<T>() where T : ClientSideObjectBase => SpawnedClientObjects.OfType<T>();

        /// <summary>
        /// Retrieves all spawned <see cref="ServerObject"/>s of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="ServerObject"/> to filter.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all <see cref="ServerObject"/>s that match type <typeparamref name="T"/>.</returns>
        public IEnumerable<T> GetServerObject<T>() where T : ServerObject => SpawnedServerObjects.OfType<T>();

#region ClientObjects
        /// <summary>
        /// Gets all spawned <see cref="PrimitiveObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetClientObject)}<PrimitiveObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<PrimitiveObject> Primitives =>
            SpawnedClientObjects.OfType<PrimitiveObject>();

        /// <summary>
        /// Gets all spawned <see cref="CapybaraObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetClientObject)}<CapybaraObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<CapybaraObject> Capybaras =>
            SpawnedClientObjects.OfType<CapybaraObject>();

        /// <summary>
        /// Gets all spawned <see cref="LightObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetClientObject)}<LightObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<LightObject> Lights =>
            SpawnedClientObjects.OfType<LightObject>();
            
#endregion

#region ServerObjects
        /// <summary>
        /// Gets all spawned <see cref="CameraObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<CameraObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<CameraObject> Cameras =>
            SpawnedServerObjects.OfType<CameraObject>();

        /// <summary>
        /// Gets all spawned <see cref="DoorObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<DoorObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<DoorObject> Doors =>
            SpawnedServerObjects.OfType<DoorObject>();

        /// <summary>
        /// Gets all spawned <see cref="ClutterObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<ClutterObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<ClutterObject> Clutter =>
            SpawnedServerObjects.OfType<ClutterObject>();

        /// <summary>
        /// Gets all spawned <see cref="InteractionObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<InteractionObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<InteractionObject> Interactables =>
            SpawnedServerObjects.OfType<InteractionObject>();
            
        /// <summary>
        /// Gets all spawned <see cref="PickupObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<PickupObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<PickupObject> Pickups =>
            SpawnedServerObjects.OfType<PickupObject>();

        /// <summary>
        /// Gets all spawned <see cref="TargetDummyObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<TargetDummyObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<TargetDummyObject> TargetDummies =>
            SpawnedServerObjects.OfType<TargetDummyObject>();

        /// <summary>
        /// Gets all spawned <see cref="TextToyObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<TextToyObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<TextToyObject> TextToys =>
            SpawnedServerObjects.OfType<TextToyObject>();

        /// <summary>
        /// Gets all spawned <see cref="WaypointObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<WaypointObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<WaypointObject> Waypoints =>
            SpawnedServerObjects.OfType<WaypointObject>();

        /// <summary>
        /// Gets all spawned <see cref="WorkstationObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<WorkstationObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<WorkstationObject> Workstations =>
            SpawnedServerObjects.OfType<WorkstationObject>();

        /// <summary>
        /// Gets all spawned <see cref="TeleporterObject"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<TeleporterObject>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<TeleporterObject> Teleporters =>
            SpawnedServerObjects.OfType<TeleporterObject>();

        /// <summary>
        /// Gets all spawned <see cref="CapybaraObjectServer"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<CapybaraObjectServer>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<CapybaraObjectServer> ServerCapybaras =>
            SpawnedServerObjects.OfType<CapybaraObjectServer>();

        /// <summary>
        /// Gets all spawned <see cref="LightObjectServer"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<LightObjectServer>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<LightObjectServer> ServerLights =>
            SpawnedServerObjects.OfType<LightObjectServer>();

        /// <summary>
        /// Gets all spawned <see cref="PrimitiveObjectServer"/>s belonging to this schematic.
        /// </summary>
        [Obsolete($"Use {nameof(GetServerObject)}<PrimitiveObjectServer>() instead of this. This property will be removed in version 0.6.0")]
        public IEnumerable<PrimitiveObjectServer> ServerPrimitives =>
            SpawnedServerObjects.OfType<PrimitiveObjectServer>();
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

        /// <summary>
        /// Destroys this <see cref="SchematicData"/> instance.
        /// </summary>
        public void Destroy()
        {
            SchematicHandler.OnSchematicDestroyed(new(this));

            foreach (KeyValuePair<LODZone, SchematicData> kvp in SchematicLoader.SchematicLODZones.Where(s => s.Value == this).ToArray())
            {
                SchematicLoader.SchematicLODZones.Remove(kvp.Key);
            }

            foreach (ClientSideObjectBase clientobj in SpawnedClientObjects.ToArray())
            {
                clientobj.DestroyForAllPlayers();
                SpawnedClientObjects.Remove(clientobj);
            }

            foreach (ServerObject serverobj in SpawnedServerObjects.ToArray())
            {
                if (serverobj is DoorObject door && door.Object!.TryGetComponent<DoorLink>(out var link))
                    link.Unregister();

                if (serverobj.Object!.TryGetComponent<BlockyRuntime>(out var blocky))
                    Executor?.Execute(ArgumentsParser.Load(blocky.Blocky!), null!, EventType.OnDestroyed);

                serverobj.DestroyObject(this);
            }

            Executor = null;
            AnimationController.Remove(this);
            ColliderHelper.SchematicColliders.Remove(this);
        }
    }
}