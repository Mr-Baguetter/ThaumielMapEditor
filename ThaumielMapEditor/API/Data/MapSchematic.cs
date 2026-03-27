namespace ThaumielMapEditor.API.Data
{
    public class MapSchematicData
    {
        /// <summary>
        /// Gets or sets the local position relative to the <see cref="MapData"/> instance this <see cref="MapSchematicData"/> instance is tied to.
        /// </summary>
        public Vector3 LocalPosition { get; set; }
        
        /// <summary>
        /// Gets or sets the schematic name to load of this <see cref="MapSchematicData"/> instance.
        /// </summary>
        public string SchematicName { get; set; } = string.Empty;
    }
}