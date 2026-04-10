// -----------------------------------------------------------------------
// <copyright file="WorkstationInteractPatch.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using HarmonyLib;
using InventorySystem.Items.Firearms.Attachments;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;

namespace ThaumielMapEditor.HarmonyPatches
{
    [HarmonyPatch]
    public static class WorkstationInteractPatch
    {
        [HarmonyPatch(typeof(WorkstationController), nameof(WorkstationController.ServerInteract))]
        public static bool Prefix(WorkstationController __instance, ReferenceHub ply, byte colliderId)
        {
            if (ply == null)
            {
                return true;
            }

            foreach (SchematicData schematic in SchematicLoader.SpawnedSchematics.Where(s => !s.Workstations.IsEmpty()))
            {
                foreach (WorkstationObject workstation in schematic.Workstations)
                {
                    if (workstation.Base == __instance)
                    {
                        if (!workstation.AllowInteractions)
                        {
                            return false;
                        }

                        if (workstation.AllowedRoles != null && workstation.AllowedRoles.Count > 0)
                        {
                            if (!workstation.AllowedRoles.Contains(ply.roleManager.CurrentRole.RoleTypeId))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}