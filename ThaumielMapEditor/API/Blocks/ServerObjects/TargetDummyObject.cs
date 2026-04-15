// -----------------------------------------------------------------------
// <copyright file="TargetDummyObject.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using AdminToys;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class TargetDummyObject : ServerObject
    {
        /// <summary>
        /// The instantiated <see cref="ShootingTarget"/> component for the spawned object.
        /// This value is null until <see cref="SpawnObject(SchematicData, SerializableObject)"/> is called.
        /// </summary>
        [YamlIgnore]
#pragma warning disable CS8618
        public ShootingTarget Base { get; private set; }
#pragma warning restore CS8618

        /// <summary>
        /// The configured <see cref="TargetType"/> for this target.
        /// Determined by parsing serialized data before spawning.
        /// </summary>
        public TargetType Type { get; private set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Target;

        /// <summary>
        /// Returns the prefab <see cref="ShootingTarget"/> that corresponds to the provided <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The target variant to resolve a prefab for.</param>
        /// <returns>The matching <see cref="ShootingTarget"/> prefab.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an unsupported <paramref name="type"/> is provided.</exception>
        public ShootingTarget? GetPrefab(TargetType type)
        {
			ShootingTarget? prefab = type switch
			{
				TargetType.Binary => PrefabHelper.ShootingTargetBinary,
				TargetType.ClassD => PrefabHelper.ShootingTargetDBoy,
				TargetType.Sport => PrefabHelper.ShootingTargetSport,
				_ => throw new InvalidOperationException(),
			};

            return prefab;
        }

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            ParseValues(serializable);
            ShootingTarget? prefab = GetPrefab(Type);
            if (prefab == null) 
                return;
                
            ShootingTarget? target = UnityEngine.Object.Instantiate(prefab);
            NetworkServer.UnSpawn(target.gameObject);
            Object = target.gameObject;
            NetId = target.netId;
            Base = target;
            SetWorldTransform(schematic);
            NetworkServer.Spawn(target.gameObject);

            base.SpawnObject(schematic, serializable);
        }

        /// <summary>
        /// Parses and applies values from a <see cref="SerializableObject"/> to this instance.
        /// Validates that the serializable represents a target and extracts the <see cref="TargetType"/>.
        /// Logs warnings when parsing fails or the object type does not match.
        /// </summary>
        /// <param name="serializable">The serialized object data to parse.</param>
        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Target)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Target Dummy");
                return;
            }

            if (!serializable.Values.TryConvertValue<TargetType>("TargetType", out var type))
            {
                LogManager.Warn("Failed to parse TargetType");
            }

            Type = type;
        }
    }
}