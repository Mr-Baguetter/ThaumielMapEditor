using System.Collections.Generic;

namespace ThaumielMapEditor.API.Serialization
{
    public class SerializableSchematic
    {
        public int RootObjectId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public List<SerializableObject> Objects {get; set; } = [];
        public List<SerializableArea> Areas { get; set; } = [];
        public bool ContainsAnimator { get; set; }
    }
}