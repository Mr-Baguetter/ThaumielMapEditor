using System.Collections.Generic;
using ThaumielMapEditor.API.Blocks.Areas;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Serialization
{
    public class SerializableSchematic
    {
        public string FileName { get; set; } = string.Empty;
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public List<SerializableObject> Objects {get; set; } = [];
        public bool ContainsAnimator { get; set; }

        [YamlIgnore] // Ignore for now since its not implemented in unity.
        public List<SerializableArea> Areas { get; set; } = [];
    }
}