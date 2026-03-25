using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Mirror;
using System;
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

        public bool IsStatic { get; set; }
        public byte MovementSmoothing { get; set; }
        public uint NetId { get; set; }
        public virtual GameObject? Object { get; set; }
        public virtual ObjectType ObjectType { get; set; }

        private StructurePositionSync? PositionSync { get; set; }

        public void SetWorldTransform(SchematicData schematic)
        {
            Position = schematic.Position + (schematic.Rotation * Vector3.Scale(Position, schematic.Scale));
            Rotation = schematic.Rotation * Rotation;
            Scale = Vector3.Scale(Scale, schematic.Scale);
        }

        public virtual void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            OnObjectCreated?.Invoke(this);
            schematic.SpawnedServerObjects.Add(this);
        }

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

        public void DestroyObject(SchematicData schematic)
        {
            if (Object == null)
            {
                LogManager.Warn($"Failed to destroy Object. Object is null.");
                return;
            }

            OnObjectDestroying?.Invoke(this);
            schematic.SpawnedServerObjects.Remove(this);
            NetworkServer.Destroy(Object);
        }

        public T GetValue<T>(SerializableObject serializable, string key) =>
            serializable.Values.GetConvertValue<T>(key);
    }
}