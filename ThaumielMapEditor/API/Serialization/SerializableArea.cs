using System.Collections.Generic;
using ThaumielMapEditor.API.Enums;

namespace ThaumielMapEditor.API.Serialization
{
    public class SerializableArea
    {
        public int ObjectId { get; set; }
        public int ParentId { get; set; }
        public string SchematicName { get; set; } = string.Empty;
        public AreaType AreaType { get; set; }
        public Dictionary<string, object> Values { get; set; } = new();
    }
}