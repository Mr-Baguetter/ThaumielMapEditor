using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ThaumielMapEditor.API.Conversion
{
    public class PMERRoot
    {
        /// <summary>
        /// Gets or sets the root id of the PMER schematic root
        /// </summary>
        [JsonPropertyName("RootObjectId")]
        public int RootObjectId { get; set; }
        
        /// <summary>
        /// Gets or sets the blocks of the PMER schematic root
        /// </summary>
        [JsonPropertyName("Blocks")]
        public List<PMERBlock> Blocks { get; set; } = [];
    }

}