// -----------------------------------------------------------------------
// <copyright file="ISubCommand.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using CommandSystem;

namespace ThaumielMapEditor.API.Interfaces
{
    internal interface ISubCommand
    {
        public string Name { get; }

        public string VisibleArgs { get; }

        public int RequiredArgsCount { get; }

        public string Description { get; }

        public string[] Aliases { get; }

        public string RequiredPermission { get; }

        public bool Execute(List<string> arguments, ICommandSender sender, out string response);
    }
}