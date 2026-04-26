// -----------------------------------------------------------------------
// <copyright file="Rotation.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Attributes;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;

namespace ThaumielMapEditor.Commands.Admin
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Rotation : ISubCommand
    {
        public string Name => "rotate";
        public string VisibleArgs => "<Schematic ID>, <X>, <Y>, <Z>";
        public int RequiredArgsCount => 3;
        public string Description => "Grabs the specified schematic";
        public string[] Aliases => ["rot"];
        public string RequiredPermission => "tme.rotate";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 3)
            {
                if (!Player.TryGet(sender, out var player))
                {
                    response = "Failed to parse player. Use the 4 argument version instead.";
                    return false;
                }

                if (!float.TryParse(arguments.At(0), out var x))
                {
                    response = "Failed to parse X coordinate. Make sure its a number";
                    return false;
                }

                if (!float.TryParse(arguments.At(1), out var y))
                {
                    response = "Failed to parse Y coordinate. Make sure its a number";
                    return false;
                }

                if (!float.TryParse(arguments.At(2), out var z))
                {
                    response = "Failed to parse Z coordinate. Make sure its a number";
                    return false;
                }

                SchematicData? schematic = CommandHelper.GetSchematic(player);
                if (schematic == null)
                {
                    response = "Failed to find schematic.";
                    return false;
                }

                Quaternion rotation = Quaternion.Euler(new(x, y, z));
                Quaternion prevrot = schematic.Primitive!.Rotation;
                schematic.Rotation = rotation;
                schematic.Primitive.Rotation = rotation;

                foreach (ServerObject serverObject in schematic.SpawnedServerObjects)
                {
                    serverObject.UpdateObject(schematic);
                }

                response = $"Rotated schematic {schematic.FileName} to {rotation} from {prevrot}";
                return true;
            }
            else
            {
                if (!uint.TryParse(arguments.At(0), out var id))
                {
                    response = $"Failed to parse ID. Make sure its a non negative number. You can get all schematics by running this command: 'tme list'";
                    return false;
                }

                if (!SchematicLoader.TryGetSchematicById(id, out var schematic))
                {
                    response = $"Failed to find schematic with the ID {id}. Run 'tme spawned' to get all spawned schematics";
                    return false;
                }

                if (schematic == null)
                {
                    response = "The schematic is null!";
                    return false;
                }

                if (schematic.Primitive == null)
                {
                    response = $"Main schematic primitive is null!";
                    return false;
                }

                if (!float.TryParse(arguments.At(1), out var x))
                {
                    response = "Failed to parse X coordinate. Make sure its a number";
                    return false;
                }

                if (!float.TryParse(arguments.At(2), out var y))
                {
                    response = "Failed to parse Y coordinate. Make sure its a number";
                    return false;
                }

                if (!float.TryParse(arguments.At(3), out var z))
                {
                    response = "Failed to parse Z coordinate. Make sure its a number";
                    return false;
                }

                Quaternion rotation = Quaternion.Euler(new(x, y, z));
                Quaternion prevrot = schematic.Primitive.Rotation;
                schematic.Rotation = rotation;
                schematic.Primitive.Rotation = rotation;

                foreach (ServerObject serverObject in schematic.SpawnedServerObjects)
                {
                    serverObject.UpdateObject(schematic);
                }

                response = $"Rotated schematic {schematic.FileName} to {rotation} from {prevrot}";
                return true;
            }
        }
    }
}