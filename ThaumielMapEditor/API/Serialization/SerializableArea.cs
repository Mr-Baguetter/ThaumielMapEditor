using System.Collections.Generic;
using ThaumielMapEditor.API.Enums;

namespace ThaumielMapEditor.API.Serialization
{
    public class SerializableArea
    {
        public string SchematicName { get; set; } = string.Empty;
        public AreaType AreaType { get; set; }
        public Dictionary<string, object> Values { get; set; } = new();
    }
}