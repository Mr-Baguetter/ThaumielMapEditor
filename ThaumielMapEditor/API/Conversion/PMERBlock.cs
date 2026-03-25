using System.Collections.Generic;

namespace ThaumielMapEditor.API.Conversion
{
    public class PMERBlock
    {
        public string Name { get; set; }

        public int ObjectId { get; set; }
        public int ParentId { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public int BlockType { get; set; }

        public Dictionary<string, object> Properties { get; set; } = [];
    }
}