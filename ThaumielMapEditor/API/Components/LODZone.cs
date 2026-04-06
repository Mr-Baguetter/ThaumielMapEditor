// -----------------------------------------------------------------------
// <copyright file="LODZone.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class LODZone : TriggerHandler
    {
        /// <summary>
        /// Gets the list of <see cref="PrimitiveType"/>s that should be spawned and despawned when a <see cref="Player"/> enters or exits this zone.
        /// </summary>
        public List<PrimitiveType> PrimitivestoUnload { get; private set; } = [];

        /// <summary>
        /// Gets the <see cref="SchematicData"/> this zone belongs to.
        /// </summary>
        public SchematicData Schematic { get; private set; }

        /// <summary>
        /// Gets the index of this <see cref="LODZone"/> within its <see cref="SchematicData"/>.
        /// </summary>
        public uint Index { get; private set; }
        
        /// <summary>
        /// Initializes this <see cref="LODZone"/> instance and subscribes to the trigger enter and exit events.
        /// </summary>
        /// <param name="schematic">The <see cref="SchematicData"/> this zone belongs to.</param>
        /// <param name="unload">The list of <see cref="PrimitiveType"/>s to spawn and despawn as players enter and exit.</param>
        /// <param name="index">The index of this zone within the schematic.</param>
        public void Init(SchematicData schematic, List<PrimitiveType> unload, uint index)
        {
            OnPlayerEntered += OnTriggerEnter;
            OnPlayerExited += OnTriggerExit;

            PrimitivestoUnload = unload;
            Schematic = schematic;
            Index = index;
        }

        private void OnDestroy()
        {
            OnPlayerEntered -= OnTriggerEnter;
            OnPlayerExited -= OnTriggerExit;
        }

        private void OnTriggerEnter(Player player, Collider other)
        {
            foreach (PrimitiveObject prim in Schematic.Primitives)
            {
                if (!PrimitivestoUnload.Contains(prim.PrimitiveType))
                    continue;

                prim.SpawnForPlayer(player);
            }
        }

        private void OnTriggerExit(Player player, Collider other)
        {
            foreach (PrimitiveObject prim in Schematic.Primitives)
            {
                if (!PrimitivestoUnload.Contains(prim.PrimitiveType))
                    continue;

                prim.DespawnForPlayer(player);
            }
        }
    }
}