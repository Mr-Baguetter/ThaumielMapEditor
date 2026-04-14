// -----------------------------------------------------------------------
// <copyright file="Destroy.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using CommandSystem;
using ThaumielMapEditor.API.Attributes;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;

namespace ThaumielMapEditor.Commands.Admin
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Destroy : ISubCommand
    {
        public string Name => "destroy";

        public string VisibleArgs => "<Schematic Id>";

        public int RequiredArgsCount => 1;

        public string Description => "Destroys the specified schematic";

        public string[] Aliases => ["de", "delete", "remove", "del"];

        public string RequiredPermission => "tme.destroy";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            uint count = 0;
            if (!SchematicLoader.SchematicsById.TryGetValue(uint.Parse(arguments[0]), out var data))
            {
                StringBuilder sb = new();
                sb.AppendLine();
                sb.AppendLine($"No schematic with id {arguments[0]} was found.");
                sb.AppendLine($"Available schematics:");
                foreach (KeyValuePair<uint, SchematicData> kvp in SchematicLoader.SchematicsById)
                    sb.AppendLine($"- [{kvp.Key}]: {kvp.Value.FileName}");

                response = sb.ToString();
                return false;
            }

            SchematicLoader.DestroySchematic(data);
            response = $"Destroyed schematic {arguments[0]} for {count} players";
            return true;
        }
    }
}