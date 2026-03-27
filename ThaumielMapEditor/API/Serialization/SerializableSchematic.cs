using System.Collections.Generic;

namespace ThaumielMapEditor.API.Serialization
{
    public class SerializableSchematic
    {
        /// <summary>
        /// Gets or sets the root object id for the <see cref="SerializableSchematic"/> instance.
        /// </summary>
        public int RootObjectId { get; set; }

        /// <summary>
        /// Gets or sets file name for the <see cref="SerializableSchematic"/> instance.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the rotation for the <see cref="SerializableSchematic"/> instance.
        /// </summary>
        public Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets or sets the scale for the <see cref="SerializableSchematic"/> instance.
        /// </summary>
        public Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets the objects for the <see cref="SerializableSchematic"/> instance.
        /// </summary>
        public List<SerializableObject> Objects {get; set; } = [];

        /// <summary>
        /// Gets or sets the areas for the <see cref="SerializableSchematic"/> instance.
        /// </summary>
        public List<SerializableArea> Areas { get; set; } = [];

        /// <summary>
        /// Gets or sets whether the <see cref="SerializableSchematic"/> instance had a animator when built.
        /// </summary>
        public bool ContainsAnimator { get; set; }
    }
}