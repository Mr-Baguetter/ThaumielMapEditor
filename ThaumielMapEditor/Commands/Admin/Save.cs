// -----------------------------------------------------------------------
// <copyright file="Save.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
using CommandSystem;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Attributes;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;

namespace ThaumielMapEditor.Commands.Admin
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Save : ISubCommand
    {
        public string Name => "save";

        public string VisibleArgs => "<Map Name>";

        public int RequiredArgsCount => 1;

        public string Description => "Saves the current spawned schematics into a map file";

        public string[] Aliases => [""];

        public string RequiredPermission => "tme.save";

        public bool Execute(List<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            MapData map = new();
            if (!Player.TryGet(sender, out var player))
            {
                response = "You must be a player to run this command!";
                return false;
            }
            
            if (player.Room == null)
            {
                response = $"You must be in a room to run this!";
                return false;
            }

            map.Room = player.Room;
            map.FileName = arguments[0];
            foreach (SchematicData schematic in SchematicLoader.SpawnedSchematics)
            {
                Vector3 pos = player.Room.LocalPosition(schematic.Position);
                map.Schematics.Add(new() { LocalPosition = pos, SchematicName = schematic.FileName});
            }

            SchematicLoader.SaveMap(map);
            response = $"Saved map {arguments[0]}.";
            return true;
        }
    }
}