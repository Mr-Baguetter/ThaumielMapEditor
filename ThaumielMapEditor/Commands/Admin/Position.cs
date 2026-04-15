// -----------------------------------------------------------------------
// <copyright file="Position.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using CommandSystem;
using LabApi.Features.Wrappers;
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

        public string VisibleArgs => "[Schematic Id], <Get|Set>, [X], [Y], [Z]";

        public int RequiredArgsCount => 1;

        public string Description => "Changes the position of a schematic";

        public string[] Aliases => ["pos"];

        public string RequiredPermission => "tme.position";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            sb.AppendLine();
            bool hasId = uint.TryParse(arguments.At(0), out uint id);
            SchematicData? data;

            if (hasId)
            {
                if (!SchematicLoader.SchematicsById.TryGetValue(id, out data) || data.Primitive == null)
                {
                    sb.AppendLine($"No schematic with id {id} was found.");
                    sb.AppendLine($"Available schematics:");

                    foreach (KeyValuePair<uint, SchematicData> kvp in SchematicLoader.SchematicsById)
                    {
                        sb.AppendLine($"- [{kvp.Key}]: {kvp.Value.FileName}");
                    }

                    response = sb.ToString();
                    return false;
                }
            }
            else
            {
                if (!Player.TryGet(sender, out var player))
                {
                    response = "Failed to parse player. Use the version with a Schematic ID instead.";
                    return false;
                }

                data = CommandHelper.GetSchematic(player);
                if (data == null)
                {
                    response = "Failed to find schematic via raycast. Make sure you are looking at one.";
                    return false;
                }
            }

            string subCommand = hasId ? arguments.At(1).ToLower() : arguments.At(0).ToLower();
            switch (subCommand)
            {
                case "get":
                    sb.AppendLine($"Got Schematic Position:");
                    sb.AppendLine($"- X: {data.Primitive!.Position.x}");
                    sb.AppendLine($"- Y: {data.Primitive.Position.y}");
                    sb.AppendLine($"- Z: {data.Primitive.Position.z}");
                    break;

                case "set":
                    int offset = hasId ? 2 : 1;

                    if (arguments.Count < offset + 3)
                    {
                        response = hasId ? "You require 5 arguments to set the position: <Schematic Id>, Set, <X>, <Y>, <Z>" : "You require 4 arguments to set the position: Set, <X>, <Y>, <Z>";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(offset), out float x))
                    {
                        response = "Failed to parse X coordinate. Make sure its a number.";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(offset + 1), out float y))
                    {
                        response = "Failed to parse Y coordinate. Make sure its a number.";
                        return false;
                    }

                    if (!float.TryParse(arguments.At(offset + 2), out float z))
                    {
                        response = "Failed to parse Z coordinate. Make sure its a number.";
                        return false;
                    }

                    data.Position = new(x, y, z);
                    data.Primitive!.Position = data.Position;

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