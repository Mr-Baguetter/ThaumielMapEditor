using ThaumielMapEditor.API.Data;

namespace ThaumielMapEditor.Events.EventArgs.Schematic
{
    public class SchematicDestroyedEventArgs : System.EventArgs
    {
        public SchematicData Schematic { get; }

        public SchematicDestroyedEventArgs(SchematicData schematic)
        {
            Schematic = schematic;
        }
    }
}