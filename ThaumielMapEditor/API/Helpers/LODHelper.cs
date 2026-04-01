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
            if (Main.Instance.Config.SchematiclodBlacklist.Contains(schematic.FileName))
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

                BoxCollider collider = schematic.Primitive.GameObject.AddComponent<BoxCollider>();
                collider.size = data.Bounds;
                collider.name = $"{schematic.FileName}-LOD{data.Index}-Collider";
                collider.isTrigger = true;

                LODZone lodZone = collider.gameObject.AddComponent<LODZone>();
                lodZone.Init(schematic, data.Primitives, data.Index, collider);

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
            if (Main.Instance.Config.SchematiclodBlacklist.Contains(schematic.FileName))
                return [];

            List<Player> players = [];
            LODZone lod = null!;
            foreach (BoxCollider collider in schematic.Primitive.GameObject.GetComponents<BoxCollider>())
            {
                if (!collider.TryGetComponent<LODZone>(out var lodzone))
                    continue;

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
    }
}