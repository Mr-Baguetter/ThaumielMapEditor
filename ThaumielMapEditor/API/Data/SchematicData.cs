using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Blocks.ClientSide;
using LabPrimitive = LabApi.Features.Wrappers.PrimitiveObjectToy;
using System;

namespace ThaumielMapEditor.API.Data
{
    public class SchematicData
    {
        public static event Action<SchematicData>? SchematicPositionUpdated;
        public static event Action<SchematicData>? SchematicRotationUpdated;

        public bool ContainsAnimator { get; set; }
        public string FileName = string.Empty;
        public uint Id;

        public Vector3 Position
        {
            get;
            set
            {
                field = value;
                SchematicPositionUpdated?.Invoke(this);
            }
        }

        public Quaternion Rotation
        {
            get;
            set
            {
                field = value;
                SchematicRotationUpdated?.Invoke(this);
            }
        }
        
        public Vector3 Scale { get; set; }

        public LabPrimitive? Primitive;
        public List<ClientSideObjectBase> SpawnedClientObjects = [];
        public List<ServerObject> SpawnedServerObjects = [];

        public IEnumerable<PrimitiveObject> Primitives
            => SpawnedClientObjects.OfType<PrimitiveObject>();
            
        public void SyncWithPlayer(Player player)
        {
            foreach (ClientSideObjectBase objects in SpawnedClientObjects)
            {
                objects.SpawnForPlayer(player);
            }
        }
    }
}