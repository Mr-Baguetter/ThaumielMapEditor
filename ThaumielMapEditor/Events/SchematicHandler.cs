using System.Collections.Generic;
using ThaumielMapEditor.API.Animation;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.Events
{
    /// <summary>
    /// This is a example of how to setup animations to run after spawning a schematic.
    /// </summary>
    internal class SchematicHandler
    {
        public static void Register()
        {
            SchematicLoader.SchematicSpawned += OnSchematicSpawned;
            SchematicLoader.SchematicDestroyed += OnSchematicDestroyed;
        }

        public static void Unregister()
        {
            SchematicLoader.SchematicSpawned -= OnSchematicSpawned;
            SchematicLoader.SchematicDestroyed -= OnSchematicDestroyed;
        }

        private static void OnSchematicSpawned(SchematicData schematic)
        {
            schematic.AnimationController.Play("Idle");
        }

        private static void OnSchematicDestroyed(SchematicData schematic)
        {
            if (AnimationController.AnimationSchematics.TryGetValue(schematic, out AnimationController? controller))
                controller.Destroy();
        }
    }
}