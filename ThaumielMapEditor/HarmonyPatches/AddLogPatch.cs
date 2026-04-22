// -----------------------------------------------------------------------
// <copyright file="AddLogPatch.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using HarmonyLib;
using System;
using System.Text.RegularExpressions;
using ThaumielMapEditor.API.Helpers.Networking;

namespace ThaumielMapEditor.HarmonyPatches
{
    [HarmonyPatch]
    public class AddLogPatch
    {
        [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
        public static void Postfix(string q, ConsoleColor color, bool hideFromOutputs)
        {
            if (!q.Contains("ThaumielMapEditor") || !q.Contains("[ERROR]") || !Main.Instance.Config.AutomaticErrorUpload)
                return;

            q = Regex.Replace(q, @"_Patch\d+", "");
            LogsUploader.SendAutoRequest($"{q.Replace("MonoMod.Utils.DynamicMethodDefinition.", "")}");
        }
    }
}
