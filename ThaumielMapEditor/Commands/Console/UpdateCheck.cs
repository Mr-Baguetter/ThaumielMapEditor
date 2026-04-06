// -----------------------------------------------------------------------
// <copyright file="UpdateCheck.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CommandSystem;
using MEC;
using ThaumielMapEditor.API.Helpers;

namespace ThaumielMapEditor.Commands.Console
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class UpdateCheck : ParentCommand
    {
        public override string Command => "tmeupdatecheck";

        public override string[] Aliases => [];

        public override string Description => "Checks for updates to the Thaumiel Map Editor plugin.";

        public override void LoadGeneratedCommands() { }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Timing.WaitUntilDone(Timing.RunCoroutine(UpdateHelper.CheckForUpdatesCoroutine(true)));
            response = "Request ran";
            return true;
        }
    }
}