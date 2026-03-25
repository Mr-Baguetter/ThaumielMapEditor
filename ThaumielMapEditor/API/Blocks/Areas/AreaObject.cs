using System.Collections.Generic;
using ThaumielMapEditor.API.Data;

namespace ThaumielMapEditor.API.Blocks.Areas
{
    public class AreaObject
    {
        public SchematicData? ParentSchematic { get; set; }

        public Dictionary<string, object> Values { get; set; } = [];

        public virtual void ParseValues() { }
    }
}