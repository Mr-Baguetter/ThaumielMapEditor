using System;
using System.Collections.Generic;
using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using ThaumielMapEditor.API.Interfaces;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.ClientSide
{
    public class PrimitiveObject : ClientSideObjectBase, ICullableObject
    {
        public string Name { get; set; } = string.Empty;
        public static event Action<Vector3, PrimitiveObject>? PositionUpdated;
        public static event Action<Vector3, PrimitiveObject>? ScaleUpdated;
        public static event Action<Quaternion, PrimitiveObject>? RotationUpdated;

        public bool Spawned = false;

        /// <inheritdoc/>
        public override Quaternion Rotation
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                RotationUpdated?.Invoke(value, this);
                SyncToPlayers(2uL, w => w.WriteQuaternion(field));
            }
        }

        /// <inheritdoc/>
        public override Vector3 Scale
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                ScaleUpdated?.Invoke(value, this);
                SyncToPlayers(4uL, w => w.WriteVector3(field));
            }
        }

        /// <inheritdoc/>
        public override Vector3 Position
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                PositionUpdated?.Invoke(value, this);
                SyncToPlayers(1uL, w => w.WriteVector3(field));
            }
        }

        public Color Color
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(64uL, w => w.WriteColor(field));
            }
        }

        public PrimitiveType PrimitiveType
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(32uL, w => w.WriteInt((int)field));
            }
        }

        public PrimitiveFlags PrimitiveFlags
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(128uL, w => w.WriteByte((byte)field));
            }
        }

        /// <inheritdoc/>
        public override uint NetId { get; set; }

        /// <inheritdoc/>
        public override bool IsStatic
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(16uL, w => w.WriteBool(field));
            }
        }

        /// <inheritdoc/>
        public override byte MovementSmoothing
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(8uL, w => w.WriteByte(field));
            }
        }

        public MeshCollider? ServerCollider { get; set; }
        public SchematicData? Schematic { get; set; }

        /// <inheritdoc/>
        public override uint AssetId { get; set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Primitive; 

        /// <inheritdoc/>
        public override void SpawnForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            using NetworkWriterPooled payloadWriter = NetworkWriterPool.Get();

            payloadWriter.WriteByte(1);

            int sizePos = payloadWriter.Position;
            payloadWriter.WriteByte(0);
            int dataStart = payloadWriter.Position;

            payloadWriter.WriteVector3(Position);
            payloadWriter.WriteQuaternion(Rotation);
            payloadWriter.WriteVector3(Scale);
            payloadWriter.WriteByte(MovementSmoothing);
            payloadWriter.WriteBool(IsStatic);
            payloadWriter.WriteInt((int)PrimitiveType);
            payloadWriter.WriteColor(Color);
            payloadWriter.WriteByte((byte)PrimitiveFlags);
            payloadWriter.WriteUInt(ParentId);

            int dataEnd = payloadWriter.Position;
            payloadWriter.Position = sizePos;
            payloadWriter.WriteByte((byte)(dataEnd - dataStart));
            payloadWriter.Position = dataEnd;

            ArraySegment<byte> payload = payloadWriter.ToArraySegment();

            player.Connection.Send(new SpawnMessage
            {
                netId = NetId,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = 0,
                assetId = AssetId,
                position = Position,
                rotation = Rotation,
                scale = Scale,
                payload = payload
            });

            SpawnedPlayers.Add(player);
            Spawned = true;
        }

        private ulong _pendingDirtyBits = 0;
        private readonly SortedDictionary<ulong, Action<NetworkWriter>> _pendingWrites = [];

        private void SyncToPlayers(ulong dirtyBits, Action<NetworkWriter> writeValues)
        {
            if (SpawnedPlayers.Count == 0 || !Spawned)
                return;

            _pendingDirtyBits |= dirtyBits;
            _pendingWrites[dirtyBits] = writeValues;
            FlushSync();
        }

        public void FlushSync()
        {
            if (_pendingDirtyBits == 0 || _pendingWrites.Count == 0)
                return;

            if (SpawnedPlayers.Count == 0 || !Spawned)
            {
                _pendingDirtyBits = 0;
                _pendingWrites.Clear();
                return;
            }

            using NetworkWriterPooled payloadWriter = NetworkWriterPool.Get();

            int safetyPos = payloadWriter.Position;
            payloadWriter.WriteByte(0);
            int dataStart = payloadWriter.Position;

            payloadWriter.WriteULong(_pendingDirtyBits);

            foreach (Action<NetworkWriter> write in _pendingWrites.Values)
                write(payloadWriter);

            int dataEnd = payloadWriter.Position;
            payloadWriter.Position = safetyPos;
            payloadWriter.WriteByte((byte)((dataEnd - dataStart) & 0xFF));
            payloadWriter.Position = dataEnd;

            EntityStateMessage msg = new()
            {
                netId = NetId,
                payload = payloadWriter.ToArraySegment()
            };

            foreach (Player player in SpawnedPlayers)
            {
                if (player.IsHost)
                    continue;

                player.Connection.Send(msg);
            }

            _pendingDirtyBits = 0;
            _pendingWrites.Clear();
        }

        public void DeserializeValues(SerializableObject serializable)
        {
            switch (serializable.ObjectType)
            {
                case ObjectType.Primitive:
                    if (!serializable.Values.TryConvertValue<Color>("Color", out var color))
                    {
                        LogManager.Warn($"Failed to parse Color");
                        return;
                    }
                    if (!serializable.Values.TryConvertValue<PrimitiveType>("PrimitiveType", out var primitiveType))
                    {
                        LogManager.Warn($"Failed to parse PrimitiveType");
                        return;
                    }
                    if (!serializable.Values.TryConvertValue<PrimitiveFlags>("PrimitiveFlags", out var flags))
                    {
                        LogManager.Warn($"Failed to parse PrimitiveFlags");
                        return;
                    }

                    Color = color;
                    PrimitiveType = primitiveType;
                    PrimitiveFlags = flags;
                    break;
            }
        }
    }
}