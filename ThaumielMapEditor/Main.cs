global using Vector3 = UnityEngine.Vector3;
global using Quaternion = UnityEngine.Quaternion;
global using Logger = LabApi.Features.Console.Logger;
global using ThaumFileManager = ThaumielMapEditor.API.Helpers.FileManager;

using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using System;
using ThaumielMapEditor.API.Helpers;
using LabApi.Loader.Features.Plugins.Enums;
using ThaumielMapEditor.Events;
using HarmonyLib;
using ThaumielMapEditor.API.Attributes;

namespace ThaumielMapEditor
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Main : Plugin<Config>
    {
        public override string Name => "Thaumiel Map Editor";
        public override string Description => ":3";
        public override string Author => "Mr. Baguetter";
        public override Version Version => new(0, 1, 0);
        public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;
        public override LoadPriority Priority => LoadPriority.Medium;
        public string HarmonyId => $"MrBaguetter_{Guid.NewGuid()}";
        public Harmony harmony;

        public static Main Instance { get; set; } = null!;

        public override void Enable()
        {
            Instance = this;
            harmony = new(HarmonyId);
            harmony.PatchAll();

            SchematicLoader.Init();

            PlayerHandler.Register();
            ServerHandler.Register();
            PrimitiveHandler.Register();
            //SchematicHandler.Register();
        }

        public override void Disable()
        {
            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            PrimitiveHandler.Unregister();
            //SchematicHandler.Unregister();
            
            Instance = null!;
            harmony.UnpatchAll();
        }
    }
}
