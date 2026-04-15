using LabApi.Events;
using ThaumielMapEditor.Events.EventArgs.Schematic;

namespace ThaumielMapEditor.Events.EventArgs.Handlers
{
    public class SchematicHandler
    {
        public static event LabEventHandler<SchematicSpawnedEventArgs>? SchematicSpawned;

        public static event LabEventHandler<SchematicDestroyedEventArgs>? SchematicDestroyed;

        internal static void OnSchematicSpawned(SchematicSpawnedEventArgs ev) => SchematicSpawned.InvokeEvent(ev);

        internal static void OnSchematicDestroyed(SchematicDestroyedEventArgs ev) => SchematicDestroyed.InvokeEvent(ev);
    }
}