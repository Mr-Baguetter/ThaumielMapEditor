// -----------------------------------------------------------------------
// <copyright file="Position.cs" company="Thaumiel Team">
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
    public class Position : ISubCommand
    {
        public string Name => "position";

        public string VisibleArgs => "<Schematic Id>, <Get|Set>, <X>, <Y>, <Z>";

        public int RequiredArgsCount => 2;

        public string Description => "Changes the position of a schematic";

        public string[] Aliases => ["pos"];

        public string RequiredPermission => "tme.position";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            if (!SchematicLoader.SchematicsById.TryGetValue(uint.Parse(arguments[0]), out var data) || data.Primitive == null)
            {
                sb.AppendLine();
                sb.AppendLine($"No schematic with id {arguments[0]} was found.");
                sb.AppendLine($"Available schematics:");
                foreach (KeyValuePair<uint, SchematicData> kvp in SchematicLoader.SchematicsById)
                    sb.AppendLine($"- [{kvp.Key}]: {kvp.Value.FileName}");

                response = sb.ToString();
                return false;
            }

            sb.AppendLine();
            switch (arguments[1].ToLower())
            {
                case "get":
                    sb.AppendLine($"Got Schematic Position:");
                    sb.AppendLine($"- X: {data.Primitive.Position.x}");
                    sb.AppendLine($"- Y: {data.Primitive.Position.y}");
                    sb.AppendLine($"- Z: {data.Primitive.Position.z}");
                    break;

                case "set":
                    if (arguments.Count != 5)
                    {
                        response = "You require 5 arguments to set the position <Schematic Id>, Set, <X>, <Y>, <Z>";
                        return false;
                    }

                    data.Position = new(float.Parse(arguments[2]), float.Parse(arguments[3]), float.Parse(arguments[4]));
                    data.Primitive.Position = data.Position;
                    
                    sb.AppendLine($"Set Schematic Position:");
                    sb.AppendLine($"- X: {data.Primitive.Position.x}");
                    sb.AppendLine($"- Y: {data.Primitive.Position.y}");
                    sb.AppendLine($"- Z: {data.Primitive.Position.z}");
                    break;

                default:
                    response = "You are required to specify 'Get' or 'Set'";
                    return false;
            }

            response = sb.ToString();
            return true;
        }
    }
}