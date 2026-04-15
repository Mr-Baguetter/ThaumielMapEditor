// -----------------------------------------------------------------------
// <copyright file="InteractToyValidatePatch.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using AdminToys;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Attributes;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using static AdminToys.InvisibleInteractableToy;

namespace ThaumielMapEditor.HarmonyPatches
{
    [HarmonyPatch]
    [DoNotParse]
    public static class InteractToyValidatePatch
    {
        public static event Action<InteractionObject, Player>? OnDenied;

        [HarmonyPatch(typeof(InteractableToySearchCompletor), nameof(InteractableToySearchCompletor.ValidateStart))]
        [HarmonyPrefix]
        public static bool SearchingPrefix(InteractableToySearchCompletor __instance, ref bool __result)
        {
            if (!InteractionObject.TryGetInteractionObject(__instance._target, out var interactionobj))
                return true;
            
            Player player = Player.Get(__instance.Hub);
            if (interactionobj.Permissions == DoorPermissionFlags.None || player.IsBypassEnabled)
            {
                return true;
            }
            
            if (player.CurrentItem == null || player.CurrentItem is not KeycardItem keycard)
            {
                OnDenied?.Invoke(interactionobj, player);
                __result = false;
                return false;
            }


            if (!keycard.Permissions.HasFlagAll(interactionobj.Permissions))
            {
                OnDenied?.Invoke(interactionobj, player);
                __result = false;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(InvisibleInteractableToy), nameof(InvisibleInteractableToy.ServerInteract))]
        [HarmonyPrefix]
        public static bool InteractingPrefix(InvisibleInteractableToy __instance, ReferenceHub ply, byte colliderId)
        {
            if (!InteractionObject.TryGetInteractionObject(__instance, out var interactionobj))
                return true;
            
            Player player = Player.Get(ply);
            if (interactionobj.Permissions == DoorPermissionFlags.None || player.IsBypassEnabled)
            {
                return true;
            }
            
            if (player.CurrentItem == null || player.CurrentItem is not KeycardItem keycard)
            {
                OnDenied?.Invoke(interactionobj, player);
                return false;
            }

            if (!keycard.Permissions.HasFlagAll(interactionobj.Permissions))
            {
                OnDenied?.Invoke(interactionobj, player);
                return false;
            }

            return true;
        }
    }
}