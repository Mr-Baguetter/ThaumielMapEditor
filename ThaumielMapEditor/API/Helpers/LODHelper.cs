using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Data;
using UnityEngine;

namespace ThaumielMapEditor.API.Helpers
{
    public class LODHelper
    {
        public static void GenerateLODZones(SchematicData schematic)
        {
            BoxCollider lod0collider = schematic.Primitive.GameObject.AddComponent<BoxCollider>();
            lod0collider.size = schematic.Primitive.Scale*2;
            lod0collider.name = $"{schematic.FileName}-LOD0-Collider";
            lod0collider.isTrigger = true;
            LODZone lod0 = lod0collider.gameObject.AddComponent<LODZone>();
            lod0.Init(schematic, [PrimitiveType.Capsule, PrimitiveType.Sphere]);
        }
    }
}