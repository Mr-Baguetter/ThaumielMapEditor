// -----------------------------------------------------------------------
// <copyright file="EmptyGameObject.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class EmptyGameObject : ServerObject
    {
        [YamlIgnore]
        public GameObject? Base { get; private set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.GameObject;

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            GameObject gameObject = new()
            {
                name = serializable.Name
            };

            gameObject.transform.localPosition = serializable.Position;
            gameObject.transform.localRotation = serializable.Rotation;
            gameObject.transform.localScale = serializable.Scale;
            gameObject.transform.SetParent(schematic.Primitive!.Transform, worldPositionStays: false);

            Base = gameObject;
            Object = gameObject;
            SetWorldTransform(schematic);
            base.SpawnObject(schematic, serializable);
        }
    }
}