// -----------------------------------------------------------------------
// <copyright file="ServerNamePatch.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using HarmonyLib;

namespace ThaumielMapEditor.HarmonyPatches
{
    [HarmonyPatch]
    public static class ServerNamePatch
    {
        [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
        public static void Postfix()
        {
            if (!Main.Instance.Config!.EnableServerTracking)
                return;
            
            ServerConsole.ServerName += $"<color=#00000000><size=1>TME {Main.Instance.Version}</size></color>";
        }
    }
}