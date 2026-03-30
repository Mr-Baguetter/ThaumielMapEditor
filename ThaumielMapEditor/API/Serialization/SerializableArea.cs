using System.Collections.Generic;
using ThaumielMapEditor.API.Enums;

namespace ThaumielMapEditor.API.Serialization
{
    /// <summary>
    /// This class is used to read areas data from yaml
    /// </summary>
    public class SerializableArea
    {
        /// <summary>
        /// Gets or sets the object id for the <see cref="SerializableArea"/> instance.
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the parent id for the <see cref="SerializableArea"/> instance.
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// Gets or sets the schematic name of the parent schematic for the <see cref="SerializableArea"/> instance.
        /// </summary>        
        public string SchematicName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the area type for the <see cref="SerializableArea"/> instance.
        /// </summary>
        public AreaType AreaType { get; set; }

        /// <summary>
        /// Gets or sets the values of the <see cref="SerializableArea"/> instance.
        /// </summary>
        public Dictionary<string, object> Values { get; set; } = new();
    }
}