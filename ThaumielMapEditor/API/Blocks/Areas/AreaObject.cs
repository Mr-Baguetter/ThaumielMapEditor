using System.Collections.Generic;
using ThaumielMapEditor.API.Data;

namespace ThaumielMapEditor.API.Blocks.Areas
{
    public class AreaObject
    {
        /// <summary>
        /// Gets or sets the <see cref="SchematicData"/> this area is assigned to.
        /// </summary>
        public SchematicData? ParentSchematic { get; set; }

        /// <summary>
        /// Gets or sets the loadded values this area instance loaded
        /// </summary>
        public Dictionary<string, object> Values { get; set; } = [];

        /// <summary>
        /// Parses the values from <see cref="Values"/> into the <see langword="properies"/> of the parent class
        /// </summary>
        public virtual void ParseValues() { }
    }
}