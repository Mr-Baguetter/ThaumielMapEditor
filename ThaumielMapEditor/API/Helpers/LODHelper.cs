using System.Collections.Generic;
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
                lodZone.Init(schematic, data.Primitives);

                lodData.Add(data);
            }

            schematic.LODZones = lodData;
            return lodData;
        }
    }
}