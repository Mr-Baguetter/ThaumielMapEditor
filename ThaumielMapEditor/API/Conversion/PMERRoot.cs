using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ThaumielMapEditor.API.Conversion
{
    public class PMERRoot
    {
        [JsonPropertyName("RootObjectId")]
        public int RootObjectId { get; set; }
        
        [JsonPropertyName("Blocks")]
        public List<PMERBlock> Blocks { get; set; } = [];
    }

}