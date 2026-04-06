// -----------------------------------------------------------------------
// <copyright file="PickupObject.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

﻿using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class PickupObject : ServerObject
    {
        /// <summary>
        /// The type of item that this pickup will spawn.
        /// </summary>
        public ItemType ItemToSpawn { get; private set; }

        /// <summary>
        /// Chance (0-100) that this pickup will actually spawn when processed.
        /// </summary>
        public float SpawnPercentage { get; private set; }

        /// <summary>
        /// Maximum stack/amount that the spawned pickup can contain.
        /// </summary>
        public uint MaxAmount { get; private set; }

        /// <summary>
        /// Whether this pickup should be treated as infinite (no depletion).
        /// </summary>
        public bool IsInfinite { get; private set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Pickup;

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            SetWorldTransform(schematic);

            if (!ParseValues(serializable))
            {
                LogManager.Warn($"Failed to parse pickup values, aborting spawn.");
                return;
            }

            if (SpawnPercentage < 100f && UnityEngine.Random.Range(0f, 100f) > SpawnPercentage)
                return;

            Pickup? pickup = Pickup.Create(ItemToSpawn, Position, Rotation);
            if (pickup == null)
            {
                LogManager.Warn($"Failed to create pickup of type {ItemToSpawn}.");
                return;
            }

            Object = pickup.GameObject;
            NetId = pickup.Base.netId;

            pickup.Spawn();

            LogManager.Debug($"Spawned pickup {ItemToSpawn} at {Position}");
            base.SpawnObject(schematic, serializable);
        }

        /// <summary>
        /// Parse pickup-specific values from a <see cref="SerializableObject"/>.
        /// Validates that the serializable is a pickup and extracts
        /// </summary>
        /// <param name="serializable">The serialized object to read values from.</param>
        /// <returns>True if all required values were parsed successfully; otherwise false.</returns>
        public bool ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Pickup)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Pickup");
                return false;
            }

            if (!serializable.Values.TryConvertValue<ItemType>("ItemToSpawn", out var item))
            {
                LogManager.Warn("Failed to parse ItemToSpawn");
                return false;
            }
            if (!serializable.Values.TryConvertValue<float>("SpawnPercentage", out var percentage))
            {
                LogManager.Warn("Failed to parse SpawnPercentage");
                return false;
            }
            if (!serializable.Values.TryConvertValue<uint>("MaxAmount", out var max))
            {
                LogManager.Warn("Failed to parse MaxAmount");
                return false;
            }
            if (!serializable.Values.TryConvertValue<bool>("IsInfinite", out var infinite))
            {
                LogManager.Warn("Failed to parse IsInfinite");
                return false;
            }

            ItemToSpawn = item;
            SpawnPercentage = percentage;
            MaxAmount = max;
            IsInfinite = infinite;
            return true;
        }
    }
}
