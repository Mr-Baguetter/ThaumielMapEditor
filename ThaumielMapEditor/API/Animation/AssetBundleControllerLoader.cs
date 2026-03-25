using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Animation
{
    public static class AssetBundleControllerLoader
    {
        /// <summary>
        /// Loads a RuntimeAnimatorController from a bundle file sitting next to the schematic.
        /// The bundle file should be named after the controller e.g. "DoorController".
        /// </summary>
        public static RuntimeAnimatorController? LoadController(string schematicFileName, string controllerName)
        {
            string path = ThaumFileManager.Dir(["Schematics", schematicFileName, controllerName]);

            AssetBundle? bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                LogManager.Error($"Failed to load controller bundle at '{path}'.");
                return null;
            }

            RuntimeAnimatorController? controller = bundle.LoadAsset<RuntimeAnimatorController>(controllerName);
            if (controller == null)
            {
                LogManager.Error($"Controller '{controllerName}' not found inside bundle at '{path}'.");
            }
            else
                LogManager.Debug($"Loaded controller '{controllerName}' from '{path}'.");

            bundle.Unload(false);
            return controller;
        }
    }
}