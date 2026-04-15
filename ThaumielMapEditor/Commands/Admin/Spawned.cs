// -----------------------------------------------------------------------
// <copyright file="Spawned.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
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
    public class Spawned : ISubCommand
    {
        public string Name => "spawned";

        public string VisibleArgs => "";

        public int RequiredArgsCount => 0;

        public string Description => "Gets all spawned Schematics";

        public string[] Aliases => ["spd"];

        public string RequiredPermission => "tme.spawned";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            StringBuilder sb = new();
            sb.AppendLine();
            sb.AppendLine($"Available schematics:");
            foreach (KeyValuePair<uint, SchematicData> kvp in SchematicLoader.SchematicsById)
                sb.AppendLine($"- [{kvp.Key}]: {kvp.Value.FileName}");

            response = sb.ToString();
            return true;
        }
    }
}