// -----------------------------------------------------------------------
// <copyright file="LODHelper.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;

namespace ThaumielMapEditor.API.Helpers
{
    public class LODHelper
    {
        public static List<LODData>? GenerateLODZones(SchematicData schematic, SerializableSchematic serializable)
        {
            if (Main.Instance.Config!.SchematiclodBlacklist.Contains(schematic.FileName))
                return null;

            List<LODData> lodData = [];

            uint index = 0;
            foreach (SerializableLOD lod in serializable.LOD)
            {
                LODData data = new()
                {
                    Index = ++index,
                    Bounds = lod.Bounds,
                    Primitives = lod.Primitives
                };

                GameObject colliderobj = new($"{schematic.FileName}-LOD{data.Index}-ColliderObj");
                colliderobj.transform.SetParent(schematic.Primitive.GameObject.transform);
                BoxCollider collider = colliderobj.AddComponent<BoxCollider>();
                collider.size = data.Bounds;
                collider.name = $"{schematic.FileName}-LOD{data.Index}-Collider";
                collider.isTrigger = true;

                Rigidbody body = colliderobj.AddComponent<Rigidbody>();
                body.isKinematic = true;

                LODZone lodZone = colliderobj.AddComponent<LODZone>();
                lodZone.Init(schematic, data.Primitives, data.Index);

                lodData.Add(data);
            }

            schematic.LODZones = lodData;
            return lodData;
        }

        /// <summary>
        /// Gets the players that are inside of the specified <see cref="LODZone"/> index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="schematic"></param>
        /// <returns></returns>
        public static IEnumerable<Player> PlayersInsideZone(uint index, SchematicData schematic)
        {
            if (Main.Instance.Config!.SchematiclodBlacklist.Contains(schematic.FileName))
                return [];

            List<Player> players = [];
            LODZone lod = null!;
            foreach (LODZone lodzone in schematic.Primitive.GameObject.GetComponents<LODZone>())
            {
                if (lodzone.Index != index)
                    continue;

                lod = lodzone;
                break;
            }

            foreach (Player player in Player.ReadyList)
            {
                if (player.IsHost)
                    continue;
                
                if (lod == null)
                    return [];
                
                if (lod.Collider.bounds.Contains(player.Position))
                    players.Add(player);
            }

            return players;
        }

        /// <summary>
        /// Gets the <see cref="Player"/>s in the specified <see cref="LODZone"/>
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static Player[] GetPlayersInZone(LODZone zone)
        {
            List<Player> players = [];

            foreach (Player player in Player.ReadyList)
            {
                if (player.IsHost)
                    continue;
                                
                if (zone.Collider.bounds.Contains(player.Position))
                    players.Add(player);
            }

            return players.ToArray();
        }
    }
}