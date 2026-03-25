using System.Collections.Generic;
using ThaumielMapEditor.API.Enums;

namespace ThaumielMapEditor.API.Serialization
{
    public class SerializableObject
    {
        public int ObjectId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }
        public bool IsStatic { get; set; }
        public byte MovementSmoothing { get; set; }
        public ObjectType ObjectType { get; set; }

        public Dictionary<string, object> Values { get; set; } = new();
    }
}