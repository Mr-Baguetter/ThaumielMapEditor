// -----------------------------------------------------------------------
// <copyright file="PlayerShotRaycast.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace ThaumielMapEditor.HarmonyPatches
{
    [HarmonyPatch]
    public static class PlayerShotRaycast
    {
        public static event Action<Player, FirearmItem, RaycastHit>? PlayerShot;

        [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerApplyDestructibleDamage))]
        public static void Postfix(HitscanHitregModuleBase __instance, DestructibleHitPair target, HitscanResult result)
        {
            PlayerShot?.Invoke(Player.Get(__instance.Firearm.Owner), FirearmItem.Get(__instance.Firearm), target.Hit);
        }
    }
}