// -----------------------------------------------------------------------
// <copyright file="ServerHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.Handlers;
using MEC;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Helpers.Networking;

namespace ThaumielMapEditor.Events
{
    internal class ServerHandler
    {
        public static void Register()
        {
            ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
            ServerEvents.RoundStarted += OnRoundStart;
            ServerEvents.LczDecontaminationStarted += OnDecom;
            WarheadEvents.Started += OnWarheadStarting;
            WarheadEvents.Detonated += OnWarheadDetonated;
        }

        public static void Unregister()
        {
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStart;
            ServerEvents.LczDecontaminationStarted -= OnDecom; 
            WarheadEvents.Started -= OnWarheadStarting;
            WarheadEvents.Detonated -= OnWarheadDetonated;
        }

        private static void OnWaitingForPlayers()
        {
            PrefabHelper.RegisterPrefabs();
            Timing.RunCoroutine(Updater.CheckForUpdatesCoroutine(false));

            foreach (string name in Main.Instance.Config!.WaitingForPlayers)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnRoundStart()
        {
            foreach (string name in Main.Instance.Config!.RoundStarted)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnDecom()
        {
            foreach (string name in Main.Instance.Config!.DecontaminationStarted)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnWarheadStarting(WarheadStartedEventArgs ev)
        {
            foreach (string name in Main.Instance.Config!.WarheadStarted)
            {
                MapLoader.ParseInput(name);
            }
        }

        private static void OnWarheadDetonated(WarheadDetonatedEventArgs ev)
        {
            foreach (string name in Main.Instance.Config!.WarheadDetonated)
            {
                MapLoader.ParseInput(name);
            }
        }
    }
}