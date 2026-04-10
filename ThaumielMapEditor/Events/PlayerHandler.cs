// -----------------------------------------------------------------------
// <copyright file="PlayerHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp939Events;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp939;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Components.Tools;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.HarmonyPatches;
using UnityEngine;

namespace ThaumielMapEditor.Events
{
    internal class PlayerHandler
    {
        public class CreditTag
        {
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
        }

        public static void Register()
        {
            PlayerEvents.Joined += OnPlayerJoined;
            PlayerShotRaycast.PlayerShot += OnPlayerShot;
            Scp939Events.Lunging += OnLunging;
            Scp939Events.Attacked += On939Attacked;
            Scp096Events.Charging += OnCharging;
            ReferenceHub.OnBeforePlayerDestroyed += OnPlayerLeft;
        }

        public static void Unregister()
        {
            PlayerEvents.Joined -= OnPlayerJoined;
            PlayerShotRaycast.PlayerShot -= OnPlayerShot;
            Scp939Events.Lunging -= OnLunging;
            Scp939Events.Attacked -= On939Attacked;
            Scp096Events.Charging -= OnCharging;
            ReferenceHub.OnBeforePlayerDestroyed -= OnPlayerLeft;
        }

        // If you contribute and want a CreditTag add your own steamid to this.
        private static readonly Dictionary<string, CreditTag> Credits = new()
        {
            // MrBaguetter
            ["76561199150506472@steam"] = new CreditTag()
            {
                Name = "TME Lead Developer",
                Color = "pumpkin"
            },

            // Example contributor badge
            ["EXAMPLE99@steam"] = new CreditTag()
            {
                Name = "TME Contributor",
                Color = "red"
            }
        };

        private static void On939Attacked(Scp939AttackedEventArgs ev)
        {
            if (Physics.Raycast((ev.Player.Camera.position + ev.Player.Camera.forward), ev.Player.Camera.forward, out RaycastHit hit, 10f))
            {
                if (hit.collider.TryGetComponent<ObjectHealth>(out var healthobj))
                {
                    healthobj.Force = (ev.Player.Camera.forward + hit.normal * -1f).normalized * 10f;
                    healthobj.Damage(ev.Damage, DamageType.Scp939Swipe);
                }
            }
        }

        private static void OnCharging(Scp096ChargingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Player.RoleBase is not Scp096Role role)
                return;

            if (!role.SubroutineModule.TryGetSubroutine<Scp096ChargeAbility>(out var chargeAbility))
                return;

            Timing.RunCoroutine(ChargeCollisionCheck(chargeAbility, role));
        }

        private static IEnumerator<float> ChargeCollisionCheck(Scp096ChargeAbility chargeAbility, Scp096Role role)
        {
            while (!chargeAbility.Duration.IsReady && role.IsAbilityState(Scp096AbilityState.Charging))
            {
                Vector3 detectionCenter = chargeAbility._tr.TransformPoint(chargeAbility._detectionOffset);
                Collider[] colliders = Physics.OverlapBox(detectionCenter, chargeAbility._detectionExtents, chargeAbility._tr.rotation);

                foreach (Collider collider in colliders)
                {
                    if (!collider.TryGetComponent<ObjectHealth>(out var healthObj))
                        continue;

                    if (Physics.Linecast(detectionCenter, collider.transform.position, ThrownProjectile.HitBlockerMask))
                        continue;

                    if (!role.TryGetOwner(out var hub))
                        continue;

                    Player player = Player.Get(hub);
                    LogManager.Debug($"SCP096 charge hit ObjectHealth on {collider.gameObject.name}");
                    healthObj.Force = (player.Camera.forward + (collider.transform.position - detectionCenter).normalized).normalized * 20f;
                    healthObj.Damage(Scp096ChargeAbility.DamageObjects, DamageType.Scp096Charge);
                    yield break;
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private static void OnLunging(Scp939LungingEventArgs ev)
        {
            if (ev.LungeState != Scp939LungeState.Triggered)
                return;

            if (ev.Player.RoleBase is not Scp939Role role)
                return;

            if (!role.SubroutineModule.TryGetSubroutine<Scp939LungeAbility>(out var lungeAbility))
                return;

            Timing.RunCoroutine(LungeCollisionCheck(lungeAbility, role));
        }

        private static IEnumerator<float> LungeCollisionCheck(Scp939LungeAbility lungeAbility, Scp939Role role)
        {
            while (lungeAbility.State == Scp939LungeState.Triggered)
            {
                Vector3 currentPos = role.FpcModule.Position;
                Collider[] colliders = Physics.OverlapSphere(currentPos, lungeAbility._overallTolerance);

                foreach (Collider collider in colliders)
                {
                    if (!collider.TryGetComponent<ObjectHealth>(out var healthObj))
                        continue;

                    if (Physics.Linecast(currentPos, collider.transform.position, ThrownProjectile.HitBlockerMask))
                        continue;

                    if (!role.TryGetOwner(out var hub))
                        continue;

                    Player player = Player.Get(hub);
                    LogManager.Debug($"SCP939 mid lunge hit ObjectHealth on {collider.gameObject.name}");
                    healthObj.Force = (player.Camera.forward + (collider.transform.position - currentPos).normalized).normalized * 20f;
                    healthObj.Damage(Scp939LungeAbility.LungeDamage, DamageType.Scp939Lunge);
                    lungeAbility.State = Scp939LungeState.LandHit;
                    yield break;
                }

                yield return Timing.WaitForOneFrame;
            }
        }
        
        private static void OnPlayerShot(Player player, FirearmItem item, RaycastHit hit)
        {
            if (item == null)
                return;

            if (hit.collider.TryGetComponent<ObjectHealth>(out var healthobj) && item.GameObject.TryGetComponent<HitscanHitregModuleBase>(out var hitreg))
            {
                healthobj.Force = (player.Camera.forward + hit.normal * -1f).normalized * 10f;
                healthobj.Damage(hitreg.EffectiveDamage, DamageType.Shot);
                player.SendHitMarker();
            }
        }

        private static void OnPlayerLeft(ReferenceHub hub)
        {
            if (!Player.TryGet(hub.gameObject, out var player))
            {
                LogManager.Warn($"Failed to get leaving player.");
                return;
            }

            foreach (SchematicData data in SchematicLoader.SpawnedSchematics)
            {
                foreach (ClientSideObjectBase clientobj in data.SpawnedClientObjects)
                {
                    if (!clientobj.SpawnedPlayers.Contains(player))
                        continue;

                    clientobj.SpawnedPlayers.Remove(player);
                }
            }
        }

        private static void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            if (ev.Player == null)
                return;

            Timing.CallDelayed(0.5f, () =>
            {
                if (ev.Player == null || ev.Player.IsDestroyed)
                {
                    LogManager.Warn($"Player was null or destroyed before sync could run.");
                    return;
                }

                foreach (SchematicData data in SchematicLoader.SchematicsById.Values)
                {
                    LogManager.Debug($"Spawning {data.FileName} for player {ev.Player.DisplayName}");
                    data.SyncWithPlayer(ev.Player);
                }

                if (Main.Instance.Config!.EnableCreditTags && Credits.TryGetValue(ev.Player.UserId, out var credittag) && ev.Player.UserGroup == null)
                {
                    ev.Player.GroupColor = credittag.Color;
                    ev.Player.GroupName = credittag.Name;
                }
            });
        }
    }
}