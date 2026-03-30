using MapGeneration.Distributors;
using Mirror;
using System;
using System.Collections.Generic;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks
{
    public class ServerObject
    {
        /// <summary>
        /// 
        /// </summary>
        public static List<ServerObject> SpawnedObjects { get; set; } = [];

        /// <summary>
        /// Fired when a <see cref="ServerObject"/> is spawned into the server.
        /// </summary>
        /// <remarks>
        /// This event is invoked at the end of <see cref="SpawnObject"/>.
        /// The object is fully spawned and networked by the time this event fires.
        /// </remarks>
        public static event Action<ServerObject>? OnObjectCreated;

        /// <summary>
        /// Fired when a <see cref="ServerObject"/> is being destroyed and removed from the server.
        /// </summary>
        /// <remarks>
        /// This event is invoked before the underlying <see cref="Object"/> is destroyed via <see cref="NetworkServer"/>.
        /// </remarks>
        public static event Action<ServerObject>? OnObjectDestroying;

        /// <summary>
        /// Fired when a <see cref="ServerObject"/> has its world transform updated.
        /// </summary>
        /// <remarks>
        /// This event is invoked at the end of <see cref="UpdateObject"/> after the new transform has been applied via <see cref="SetWorldTransform"/>.
        /// </remarks>
        public static event Action<ServerObject, bool>? OnObjectUpdated;

        /// <summary>
        /// Gets or sets the Position of the ServerObject
        /// </summary>
        public Vector3 Position
        {
            get;
            set
            {
                if (field == value)
                    return;

                Object?.transform.position = value;
                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the Scale of the ServerObject
        /// </summary>
        public Vector3 Scale
        {
            get;
            set
            {
                if (field == value)
                    return;

                Object?.transform.localScale = value;
                field = value;
            }
        }

        /// <summary>
        /// Gets or sets the Rotation of the ServerObject
        /// </summary>
        public Quaternion Rotation
        {
            get;
            set
            {
                if (field == value)
                    return;

                Object?.transform.rotation = value;
                field = value;
            }
        }

        /// <summary>
        /// Gets or Sets whether or not the ServerObject is static
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// The ticks between the server sending a update to the client about this object's position. 0 means no delay, 1 means the server will send an update every tick, 2 means every other tick, and so on.
        /// </summary>
        public byte MovementSmoothing { get; set; }

        /// <summary>
        /// The NetId of the spawned ServerObject.
        /// </summary>
        public uint NetId { get; set; }

        /// <summary>
        /// Gets or sets the associated game object instance.
        /// </summary>
        public virtual GameObject? Object { get; set; }

        /// <summary>
        /// Gets or sets the type of the object represented by this instance.
        /// </summary>
        public virtual ObjectType ObjectType { get; set; }

        private StructurePositionSync? PositionSync { get; set; }

        /// <summary>
        /// Applies the specified schematic's transformation to the current object's world position, rotation, and scale.
        /// </summary>
        /// <param name="schematic">The schematic data containing the position, rotation, and scale to apply to the current object.</param>
        public void SetWorldTransform(SchematicData schematic)
        {
            Position = schematic.Position + (schematic.Rotation * Vector3.Scale(Position, schematic.Scale));
            Rotation = schematic.Rotation * Rotation;
            Scale = Vector3.Scale(Scale, schematic.Scale);
        }

        /// <summary>
        /// Spawns the specified <see cref="ServerObject"/> into the server and adds it to the provided <see cref="SchematicData"/>'s <see cref="SchematicData.SpawnedServerObjects"/> list.
        /// </summary>
        /// <param name="schematic"></param>
        /// <param name="serializable"></param>
        public virtual void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            OnObjectCreated?.Invoke(this);
            SpawnedObjects.Add(this);
            schematic.SpawnedServerObjects.Add(this);
        }

        /// <summary>
        /// Updates the object's world transform and network state based on the specified schematic data, optionally respawning the object on the network.
        /// </summary>
        /// <remarks>
        /// If the object is not currently spawned, this method will not perform any update. The method also updates the object's network synchronization component if present.
        /// </remarks>
        /// <param name="schematic">The schematic data used to update the object's position and rotation.</param>
        /// <param name="respawn">If <see langword="true"/>, the object is respawned on the network after updating. If <see langword="false"/>, the object is not respawned.</param>
        public void UpdateObject(SchematicData schematic, bool respawn = true)
        {
            if (Object == null)
            {
                LogManager.Warn($"Failed to update Object. Object is null.");
                return;
            }

            NetworkServer.UnSpawn(Object);
            SetWorldTransform(schematic);
            if (PositionSync == null && Object.TryGetComponent<StructurePositionSync>(out var posSync))
                PositionSync = posSync;

            PositionSync?.Network_position = Position;
            PositionSync?.Network_rotationY = (sbyte)Mathf.RoundToInt(Rotation.eulerAngles.y / 5.625f);

            OnObjectUpdated?.Invoke(this, respawn);
            if (respawn)
                NetworkServer.Spawn(Object);
        }

        /// <summary>
        /// Destroys the associated networked object and removes it from the specified schematic's collection of spawned
        /// server objects.
        /// </summary>
        /// <param name="schematic">The schematic data instance from which the object will be removed.</param>
        public void DestroyObject(SchematicData schematic)
        {
            if (Object == null)
            {
                LogManager.Warn($"Failed to destroy Object. Object is null.");
                return;
            }

            OnObjectDestroying?.Invoke(this);
            schematic.SpawnedServerObjects.Remove(this);
            SpawnedObjects.Remove(this);
            NetworkServer.Destroy(Object);
        }

        public T GetValue<T>(SerializableObject serializable, string key) =>
            serializable.Values.GetConvertValue<T>(key);
    }
}