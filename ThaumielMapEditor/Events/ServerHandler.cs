// -----------------------------------------------------------------------
// <copyright file="ServerHandler.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.Handlers;
using MEC;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Data;
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
            ServerEvents.RoomLightChanged += OnRoomLightChanged;
        }

        public static void Unregister()
        {
            ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
            ServerEvents.RoundStarted -= OnRoundStart;
            ServerEvents.LczDecontaminationStarted -= OnDecom; 
            WarheadEvents.Started -= OnWarheadStarting;
            WarheadEvents.Detonated -= OnWarheadDetonated;
            ServerEvents.RoomLightChanged -= OnRoomLightChanged;
        }

        // TODO Test.
        private static void OnRoomLightChanged(RoomLightChangedEventArgs ev)
        {
            foreach (SchematicData schematic in SchematicLoader.SpawnedSchematics.Where(s => s.Room != null && s.Room == ev.Room))
            {
                if (schematic.Lights.IsEmpty() && schematic.ServerLights.IsEmpty())
                    continue;

                foreach (LightObjectServer serverLight in schematic.ServerLights)
                {
                    float Intensity = 0;
                    Intensity = serverLight.Intensity;

                    if (!ev.NewState)
                    {
                        serverLight.Intensity = 0;
                    }
                    else
                        serverLight.Intensity = Intensity;
                }

                foreach (LightObject light in schematic.Lights)
                {
                    float Intensity = 0;
                    Intensity = light.Intensity;

                    if (!ev.NewState)
                    {
                        light.Intensity = 0;
                    }
                    else
                        light.Intensity = Intensity;
                }
            }
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