// -----------------------------------------------------------------------
// <copyright file="Convert.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using CommandSystem;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Paths;
using ThaumielMapEditor.API.Attributes;
using ThaumielMapEditor.API.Conversion;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;
using ThaumielMapEditor.API.Serialization;

namespace ThaumielMapEditor.Commands.Admin
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Convert : ISubCommand
    {
        public string Name => "convert";
        public string VisibleArgs => "<Schematic Name>";
        public int RequiredArgsCount => 1;
        public string Description => "Converts the PMER schematic with the specified name";
        public string[] Aliases => ["cv"];
        public string RequiredPermission => "tme.convert";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "No schematic name provided.";
                return false;
            }

            string merDir = Path.Combine(PathManager.Configs.ToString(), "ProjectMER", "Schematics");

            foreach (string filePath in Directory.GetFiles(merDir, "*.json", SearchOption.AllDirectories))
            {
                string filename = Path.GetFileNameWithoutExtension(filePath);

                if (filename.Contains("-Rigidbodies"))
                {
                    LogManager.ExtraDebug($"Found RigidBodies while attempting to parse {filePath}.");
                    continue;
                }
                if (!filename.Equals(arguments.At(0), StringComparison.OrdinalIgnoreCase))
                    continue;

                string content = File.ReadAllText(filePath);
                if (!content.TrimStart().StartsWith("{"))
                    continue;

                string schematicName = arguments.At(0);

                Task.Run(async () =>
                {
                    try
                    {
                        LogManager.ExtraDebug($"Converting '{schematicName}'. Requested by \"{Player.Get(sender)?.Nickname ?? "Console/Null"}\" - Loading file through PMER.");
                        PMERRoot root = PMERLoader.Load(filePath);
                        LogManager.ExtraDebug($"'{schematicName}' + \"{Player.Get(sender)?.Nickname ?? "Console/Null"}\" - Converting schematic");
                        SerializableSchematic schematic = await PMERConverter.ConvertSchematicAsync(root);
                        string yaml = SchematicLoader.Serializer.Serialize(schematic);
                        string outputPath = ThaumFileManager.Dir(["Schematics", $"{schematicName}.yml"]);
                        LogManager.ExtraDebug($"'{schematicName}' + \"{Player.Get(sender)?.Nickname ?? "Console/Null"}\" - Creating directory at {outputPath}");

                        var directoryPath = Path.GetDirectoryName(outputPath);
                        if (directoryPath == null)
                        {
                            LogManager.Error($"NRE caught while attempting to create directory at {outputPath}.");
                            return;
                        }
                        
                        Directory.CreateDirectory(directoryPath);
                        LogManager.ExtraDebug($"'{schematicName}' + \"{Player.Get(sender)?.Nickname ?? "Console/Null"}\" - Writing text to {outputPath}");
                        File.WriteAllText(outputPath, yaml);

                        LogManager.Info($"Conversion of '{schematicName}' completed successfully.");
                        sender.Respond($"Conversion of '{schematicName}' completed successfully.");
                        SchematicLoader.ReloadSchematics();
                    }
                    catch (Exception e)
                    {
                        LogManager.Error($"Conversion of '{schematicName}' failed: {e}");
                        sender.Respond($"Conversion of {schematicName} failed. Please check your server console.", false);
                    }
                });

                response = $"<color=yellow>Conversion of '{schematicName}' started. Check logs for completion.</color>";
                return true;
            }

            response = $"<color=red>Failed to find file with name</color> \"{arguments.At(0)}\"";
            return true;
        }
    }
}