// -----------------------------------------------------------------------
// <copyright file="Reload.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using CommandSystem;
using System;
using ThaumielMapEditor.API.Attributes;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;

namespace ThaumielMapEditor.Commands.Admin
{
#pragma warning disable CS1591
    [DoNotParse]
    public class Reload : ISubCommand
    {
        public string Name => "reload";

        public string VisibleArgs => "";

        public int RequiredArgsCount => 0;

        public string Description => "Reloads all schematics";

        public string[] Aliases => ["re"];

        public string RequiredPermission => "tme.reload";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            SchematicLoader.ReloadSchematics();
            response = "Reloaded.";
            return true;
        }
    }
}