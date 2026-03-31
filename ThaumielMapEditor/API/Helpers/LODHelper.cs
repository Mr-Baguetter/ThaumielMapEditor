using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Data;
using UnityEngine;

namespace ThaumielMapEditor.API.Helpers
{
    public class LODHelper
    {
        public static void GenerateLODZones(SchematicData schematic)
        {
            if (Main.Instance.Config.SchematiclodBlacklist.Contains(schematic.FileName))
                return;

            BoxCollider lod0collider = schematic.Primitive.GameObject.AddComponent<BoxCollider>();
            lod0collider.size = schematic.Primitive.Scale*2;
            lod0collider.name = $"{schematic.FileName}-LOD0-Collider";
            lod0collider.isTrigger = true;
            LODZone lod0 = lod0collider.gameObject.AddComponent<LODZone>();
            lod0.Init(schematic, [PrimitiveType.Capsule, PrimitiveType.Sphere]);

            BoxCollider lod1collider = schematic.Primitive.GameObject.AddComponent<BoxCollider>();
            lod1collider.size = schematic.Primitive.Scale*2.5f;
            lod1collider.name = $"{schematic.FileName}-LOD1-Collider";
            lod1collider.isTrigger = true;
            LODZone lod1 = lod1collider.gameObject.AddComponent<LODZone>();
            lod1.Init(schematic, [PrimitiveType.Cylinder]);            
        }
    }
}