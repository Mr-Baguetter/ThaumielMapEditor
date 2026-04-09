// -----------------------------------------------------------------------
// <copyright file="ServerHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.Handlers;
using MEC;
using ThaumielMapEditor.API.Components.Tools;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.Events
{
    internal class ServerHandler
    {
        public static void Register()
        {
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.LczDecontaminationStarted += OnDecom;
            ServerEvents.ExplosionSpawned += OnExploded;
            WarheadEvents.Started += OnWarheadStarting;
            WarheadEvents.Detonated += OnWarheadDetonated;
        }

        public static void Unregister()
        {
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStart;
            ServerEvents.LczDecontaminationStarted -= OnDecom; 
            ServerEvents.ExplosionSpawned -= OnExploded;
            WarheadEvents.Started -= OnWarheadStarting;
            WarheadEvents.Detonated -= OnWarheadDetonated;
        }

        private static void OnExploded(ExplosionSpawnedEventArgs ev)
        {
            Collider[] colliders = Physics.OverlapSphere(ev.Position, ev.Settings.MaxRadius, ev.Settings.DetectionMask);

            foreach (Collider collider in colliders)
            {
                if (!collider.TryGetComponent<ObjectHealth>(out var healthobj))
                    continue;

                Vector3 direction = collider.transform.position - ev.Position;
                Vector3 force = (1f - direction.magnitude / ev.Settings.MaxRadius) * (direction / direction.magnitude) * ev.Settings._rigidbodyBaseForce + Vector3.up * ev.Settings._rigidbodyLiftForce;

                healthobj.Force = force;
                healthobj.Damage(ev.Settings._playerDamageOverDistance.Evaluate(direction.magnitude), DamageType.Explosion);
                ev.Player?.SendHitMarker();
            }
        }

        private static void OnWaitingForPlayers()
        {
            PrefabHelper.RegisterPrefabs();
            Timing.RunCoroutine(UpdateHelper.CheckForUpdatesCoroutine(false));

            foreach (string name in Main.Instance.Config.WaitingForPlayers)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnRoundStart()
        {
            foreach (string name in Main.Instance.Config.RoundStarted)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnDecom()
        {
            foreach (string name in Main.Instance.Config.DecontaminationStarted)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnWarheadStarting(WarheadStartedEventArgs ev)
        {
            foreach (string name in Main.Instance.Config.WarheadStarted)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnWarheadDetonated(WarheadDetonatedEventArgs ev)
        {
            foreach (string name in Main.Instance.Config.WarheadDetonated)
            {
                MapLoader.ParseInput(name);
            }
        }
    }
}