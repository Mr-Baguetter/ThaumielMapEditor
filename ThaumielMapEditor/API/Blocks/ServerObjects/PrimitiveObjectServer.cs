// -----------------------------------------------------------------------
// <copyright file="PrimitiveObjectServer.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using AdminToys;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class PrimitiveObjectServer : ServerObject
    {
        public PrimitiveObjectToy Base { get; private set; }

        public Color Color
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.MaterialColor = value;
            }
        } = Color.white;

        public PrimitiveType PrimitiveType
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.PrimitiveType = value;
            }
        } = PrimitiveType.Cube;

        public PrimitiveFlags PrimitiveFlags
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.PrimitiveFlags = value;
            }
        } = PrimitiveFlags.None;


        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Primitive;

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            PrimitiveObjectToy? primitive = UnityEngine.Object.Instantiate(PrefabHelper.PrimitiveObject);
            if (primitive == null)
                return;

            NetworkServer.UnSpawn(primitive.gameObject);
            Base = primitive;
            Object = primitive.gameObject;
            NetId = primitive.netId;
            primitive.PrimitiveFlags = PrimitiveFlags;
            primitive.PrimitiveType = PrimitiveType;
            primitive.MaterialColor = Color;
            primitive.gameObject.transform.position = Position;
            primitive.gameObject.transform.rotation = Rotation;
            primitive.gameObject.transform.localScale = Scale;
            NetworkServer.Spawn(primitive.gameObject);
            base.SpawnObject(schematic, serializable);
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Primitive)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Primitive");
                return;
            }

            if (!serializable.Values.TryConvertValue<Color>("Color", out var color))
            {
                LogManager.Warn($"Failed to parse Color");
            }

            if (!serializable.Values.TryConvertValue<PrimitiveType>("PrimitiveType", out var primitiveType))
            {
                LogManager.Warn($"Failed to parse PrimitiveType");
            }

            if (!serializable.Values.TryConvertValue<PrimitiveFlags>("PrimitiveFlags", out var flags))
            {
                LogManager.Warn($"Failed to parse PrimitiveFlags");
            }

            Color = color;
            PrimitiveType = primitiveType;
            PrimitiveFlags = flags;
        }
    }
}