// -----------------------------------------------------------------------
// <copyright file="TMETag.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.Events;

namespace ThaumielMapEditor.Commands.Client
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class TMETag : ICommand
    {
        public string Command => nameof(TMETag).ToLower();

        public string[] Aliases => ["tmet"];

        public string Description => "Applies your ThaumielMapEditor credit tag if you have one.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out var player) || player.IsHost)
            {
                response = "You have to be a player to run this command!";
                return false;
            }

            if (!PlayerHandler.Credits.TryGetValue(player.UserId, out var tag))
            {
                response = "You do not have a credit tag!";
                return false;
            }

            player.GroupName = tag.Name;
            player.GroupColor = tag.Color;

            response = $"Applied tag <color={tag.Color}>{tag.Name}</color>";
            return true;
        }
    }
}