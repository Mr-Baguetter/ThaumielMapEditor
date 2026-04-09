// -----------------------------------------------------------------------
// <copyright file="PrimitiveHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using AdminToys;
using MEC;
using Mirror;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;

namespace ThaumielMapEditor.Events
{
    internal class PrimitiveHandler
    {

        public static void Register()
        {
            PrimitiveObject.ScaleUpdated += OnScaleUpdated;
            PrimitiveObject.PositionUpdated += OnPositionUpdated;
            PrimitiveObject.RotationUpdated += OnRotationUpdated;
            SchematicData.SchematicPositionUpdated += OnSchematicPositionUpdated;
            SchematicData.SchematicRotationUpdated += OnSchematicRotationUpdated;
        }

        public static void Unregister()
        {
            PrimitiveObject.ScaleUpdated -= OnScaleUpdated;
            PrimitiveObject.PositionUpdated -= OnPositionUpdated;
            PrimitiveObject.RotationUpdated -= OnRotationUpdated;
            SchematicData.SchematicPositionUpdated -= OnSchematicPositionUpdated;
            SchematicData.SchematicRotationUpdated -= OnSchematicRotationUpdated;
        }

        private static void OnScaleUpdated(Vector3 scale, PrimitiveObject primitive)
        {
            if (!primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable) || primitive.ServerCollider == null || primitive.Schematic == null || primitive.Schematic.Primitive == null)
                return;

            primitive.ServerCollider.transform.localScale = new(Math.Abs(scale.x), Math.Abs(scale.y), Math.Abs(scale.z));
        }

        private static void OnPositionUpdated(Vector3 position, PrimitiveObject primitive)
        {
            if (!primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable) || primitive.ServerCollider == null || primitive.Schematic == null || primitive.Schematic.Primitive == null)
                return;

            primitive.ServerCollider.transform.position = primitive.Schematic.Primitive.Transform.TransformPoint(position);
        }

        private static void OnRotationUpdated(Quaternion rotation, PrimitiveObject primitive)
        {
            if (!primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable) || primitive.ServerCollider == null || primitive.Schematic == null || primitive.Schematic.Primitive == null)
                return;

            primitive.ServerCollider.transform.rotation = rotation;
        }

        private static void OnSchematicPositionUpdated(SchematicData schematic)
        {
            foreach (PrimitiveObject primitive in schematic.Primitives)
            {
                if (!primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable) || primitive.ServerCollider == null || schematic.Primitive == null)
                    continue;

                LogManager.Debug($"Updated Position");
                primitive.ServerCollider.transform.position = schematic.Primitive.Transform.TransformPoint(primitive.Position);
            }
        }

        private static void OnSchematicRotationUpdated(SchematicData schematic)
        {
            foreach (PrimitiveObject primitive in schematic.Primitives)
            {
                if (!primitive.PrimitiveFlags.HasFlag(PrimitiveFlags.Collidable) || primitive.ServerCollider == null)
                    continue;

                LogManager.Debug($"Updated Rotation");
                primitive.ServerCollider.transform.rotation = schematic.Rotation * primitive.Rotation;
            }
        }
    }
}