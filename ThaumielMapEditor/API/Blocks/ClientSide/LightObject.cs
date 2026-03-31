using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.ClientSide
{
    public class LightObject : ClientSideObjectBase
    {
        private ulong _pendingDirtyBits = 0;
        private readonly SortedDictionary<ulong, Action<NetworkWriter>> _pendingWrites = [];
        
        /// <summary>
        /// Gets a value indicating whether this light object has been spawned for any players.
        /// </summary>
        public bool Spawned { get; private set; }

        /// <summary>
        /// Gets or sets the intensity of the light.
        /// </summary>
        public float Intensity
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(32uL, w => w.WriteFloat(value));
            }
        } = 1f;

        /// <summary>
        /// Gets or sets the range of the light.
        /// </summary>
        public float Range
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(64uL, w => w.WriteFloat(value));
            }
        } = 10f;

        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        public Color Color
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(128uL, w => w.WriteColor(value));
            }
        } = Color.white;

        /// <summary>
        /// Gets or sets the shadow type used by the light.
        /// </summary>
        public LightShadows Shadows
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(256uL, w => w.WriteInt((int)value));
            }
        } = LightShadows.None;

        /// <summary>
        /// Gets or sets the strength of the shadows cast by the light.
        /// </summary>
        public float ShadowStrength
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(512uL, w => w.WriteFloat(value));
            }
        } = 1f;

        /// <summary>
        /// Gets or sets the type of the light.
        /// </summary>
        public LightType Type
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(1024uL, w => w.WriteInt((int)value));
            }
        } = LightType.Point;

        /// <summary>
        /// Gets or sets the shape of the light.
        /// </summary>
        public LightShape Shape
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(2048uL, w => w.WriteInt((int)value));
            }
        } = LightShape.Cone;

        /// <summary>
        /// Gets or sets the outer spot angle of the light.
        /// </summary>
        public float SpotAngle
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(4096uL, w => w.WriteFloat(value));
            }
        } = 30f;

        /// <summary>
        /// Gets or sets the inner spot angle of the light.
        /// </summary>
        public float InnerSpotAngle
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                SyncToPlayers(8192uL, w => w.WriteFloat(value));
            }
        } = 20f;

        /// <summary>
        /// Gets or sets the schematic data associated with this light object.
        /// </summary>
        public SchematicData? Schematic { get; set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Light;

        /// <inheritdoc/>
        public override uint NetId { get; set; }

        /// <inheritdoc/>
        public override uint AssetId { get; set; }

        /// <inheritdoc/>
        public override void SpawnForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            using NetworkWriterPooled writer = NetworkWriterPool.Get();

            writer.WriteByte(1);

            int sizePos = writer.Position;
            writer.WriteByte(0);
            int start = writer.Position;

            writer.WriteVector3(Position);
            writer.WriteQuaternion(Rotation);
            writer.WriteVector3(Scale);
            writer.WriteByte(MovementSmoothing);
            writer.WriteBool(IsStatic);
            writer.WriteFloat(Intensity);
            writer.WriteFloat(Range);
            writer.WriteColor(Color);
            writer.WriteInt((int)Shadows);
            writer.WriteFloat(ShadowStrength);
            writer.WriteInt((int)Type);
            writer.WriteInt((int)Shape);
            writer.WriteFloat(SpotAngle);
            writer.WriteFloat(InnerSpotAngle);

            int end = writer.Position;

            writer.Position = sizePos;
            writer.WriteByte((byte)(end - start));
            writer.Position = end;

            player.Connection.Send(new SpawnMessage
            {
                netId = NetId,
                assetId = AssetId,
                position = Position,
                rotation = Rotation,
                scale = Scale,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = 0,
                payload = writer.ToArraySegment()
            });

            SpawnedPlayers.Add(player);
            Spawned = true;
        }

        /// <summary>
        /// Deserializes and applies light specific values from a <see cref="SerializableObject"/>.
        /// </summary>
        /// <param name="serializable">The serialized object containing light data.</param>
        public void DeserializeValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Light)
                return;

            if (!serializable.Values.TryConvertValue<float>("LightIntensity", out var intensity))
            {
                LogManager.Warn("Failed to parse LightIntensity");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("LightRange", out var range))
            {
                LogManager.Warn("Failed to parse LightRange");
                return;
            }
            if (!serializable.Values.TryConvertValue<Color>("LightColor", out var color))
            {
                LogManager.Warn("Failed to parse LightColor");
                return;
            }
            if (!serializable.Values.TryConvertValue<LightShadows>("ShadowType", out var shadowType))
            {
                LogManager.Warn("Failed to parse ShadowType");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("ShadowStrength", out var shadowStrength))
            {
                LogManager.Warn("Failed to parse ShadowStrength");
                return;
            }
            if (!serializable.Values.TryConvertValue<LightType>("LightType", out var lightType))
            {
                LogManager.Warn("Failed to parse LightType");
                return;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            if (!serializable.Values.TryConvertValue<LightShape>("LightShape", out var lightShape))
            {
                LogManager.Warn("Failed to parse LightShape");
                return;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            if (!serializable.Values.TryConvertValue<float>("SpotAngle", out var spotAngle))
            {
                LogManager.Warn("Failed to parse SpotAngle");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("InnerSpotAngle", out var innerSpotAngle))
            {
                LogManager.Warn("Failed to parse InnerSpotAngle");
                return;
            }

            Intensity = intensity;
            Range = range;
            Color = color;
            Shadows = shadowType;
            ShadowStrength = shadowStrength;
            Type = lightType;
            Shape = lightShape;
            SpotAngle = spotAngle;
            InnerSpotAngle = innerSpotAngle;
        }

        private void SyncToPlayers(ulong dirtyBits, Action<NetworkWriter> writeValues)
        {
            if (SpawnedPlayers.Count == 0 || !Spawned)
                return;

            _pendingDirtyBits |= dirtyBits;
            _pendingWrites[dirtyBits] = writeValues;
            FlushSync();
        }

        /// <summary>
        /// Flushes all pending synchronization updates to spawned players.
        /// </summary>
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
    }
}