// -----------------------------------------------------------------------
// <copyright file="Main.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

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
using System.IO;
using LabApi.Loader.Features.Paths;

namespace ThaumielMapEditor
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Main : Plugin<Config>
    {
        public override string Name => "Thaumiel Map Editor";
        public override string Description => ":3";
        public override string Author => "Mr. Baguetter";
        public override Version Version => new(0, 4, 0);
        public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;
        public override LoadPriority Priority => LoadPriority.Medium;
        public string HarmonyId => $"MrBaguetter_TME_{Guid.NewGuid()}";

        public Harmony harmony;
        public static Main Instance;

        public override void Enable()
        {
            Instance = this;
            
            try
            {
                harmony = new(HarmonyId);
                harmony.PatchAll();
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to patch {ex}");
            }

            SchematicLoader.Init();

            PlayerHandler.Register();
            ServerHandler.Register();
            PrimitiveHandler.Register();

            ThaumFileManager.TryCreateDirectory("Audio");
            Config?.AudioPath = Path.Combine(PathManager.Configs.ToString(), "Thaumiel", "Audio");
            SaveConfig();
        }

        public override void Disable()
        {
            PlayerHandler.Unregister();
            ServerHandler.Unregister();
            PrimitiveHandler.Unregister();
            
            Instance = null!;
            harmony.UnpatchAll();
        }
    }
}
