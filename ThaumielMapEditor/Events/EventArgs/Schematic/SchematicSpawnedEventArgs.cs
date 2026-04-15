using ThaumielMapEditor.API.Data;

namespace ThaumielMapEditor.Events.EventArgs.Schematic
{
    public class SchematicSpawnedEventArgs : System.EventArgs
    {
        public SchematicData Schematic { get; }

        public SchematicSpawnedEventArgs(SchematicData schematic)
        {
            Schematic = schematic;
        }
    }
}