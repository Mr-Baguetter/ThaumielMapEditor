// -----------------------------------------------------------------------
// <copyright file="ColliderTrigger.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using LabApi.Features.Wrappers;
using Mirror;
using SecretLabNAudio.Core;
using SecretLabNAudio.Core.Extensions;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Components.Tools.Helpers;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.Commands;
using UnityEngine;
using static ThaumielMapEditor.API.Components.Tools.Helpers.RunCommand;

namespace ThaumielMapEditor.API.Components.Tools
{
    public class ColliderTrigger : ToolBase
    {
        public Vector3 Bounds;

#pragma warning disable CS8618
        public ColliderClasses OnEntered;

        public ColliderClasses OnExited;

        public GameObject ColliderObject;

        public Collider Collider;
#pragma warning restore CS8618

        public override ToolType Type => ToolType.ColliderTrigger;

        public override void Init(ServerObject obj, SchematicData schem, Dictionary<string, object> properties)
        {
            base.Init(obj, schem, properties);
            ParseValues(properties);
            ColliderObject = new($"{Object!.Object!.name} - ColliderObject");
            ColliderObject.transform.SetParent(Object.Object.transform);
            Collider = ColliderObject.AddComponent<Collider>();
            Collider.isTrigger = true;
        }

        public void ParseValues(Dictionary<string, object> properties)
        {
            if (properties.TryGetValue("OnEntered", out var entered))
                OnEntered = MapToObject<ColliderClasses>(entered) ?? new();

            if (properties.TryGetValue("OnExited", out var exited))
                OnExited = MapToObject<ColliderClasses>(exited) ?? new();

            if (properties.TryGetValue("Bounds", out var raw) && raw is IDictionary<object, object> dict)
            {
                float x = Convert.ToSingle(dict["x"]);
                float y = Convert.ToSingle(dict["y"]);
                float z = Convert.ToSingle(dict["z"]);

                Bounds = new(x, y, z);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            if (!Player.TryGet(root, out var player))
                return;

            HandleEffect(OnEntered, player);
            HandleCommand(OnEntered, player);
            HandleAudio(OnEntered, player);
            HandleAnimation(OnEntered, player);
        }

        private void OnTriggerExit(Collider other)
        {
            GameObject? root = other.GetComponentInParent<NetworkIdentity>()?.gameObject;
            if (root == null)
                return;

            if (!Player.TryGet(root, out var player))
                return;

            HandleEffect(OnExited, player);
            HandleCommand(OnExited, player);
            HandleAudio(OnExited, player);
            HandleAnimation(OnExited, player);
        }

        private void HandleAnimation(ColliderClasses classes, Player player)
        {
            foreach (PlayAnimation play in classes.PlayAnimation)
            {
                Schematic?.AnimationController.Play(play.ResolvedAnimationName);
            }
        }

        private void HandleEffect(ColliderClasses classes, Player player)
        {
            foreach (GiveEffect give in classes.GiveEffect)
            {
                if (!Enum.TryParse<EffectType>(give.EffectName, true, out var effect))
                    continue;

                if (!player.ReferenceHub.playerEffectsController.TryGetEffect(effect.ToString(), out var effectBase))
                {
                    LogManager.Warn($"Invalid EffectType {effect} for player {player.DisplayName} - {player.PlayerId}");
                    continue;
                }

                player.EnableEffect(effectBase, (byte)give.Intensity, give.Duration, true);
            }

            foreach (RemoveEffect remove in classes.RemoveEffect)
            {
                if (!Enum.TryParse<EffectType>(remove.EffectName, true, out var effect))
                    continue;

                if (!player.ReferenceHub.playerEffectsController.TryGetEffect(effect.ToString(), out var effectBase))
                {
                    LogManager.Warn($"Invalid EffectType {effect} for player {player.DisplayName} - {player.PlayerId}");
                    continue;
                }

                player.DisableEffect(effectBase);
            }
        }

        private void HandleCommand(ColliderClasses classes, Player player)
        {
            foreach (RunCommand command in classes.RunCommand)
            {
                command.Command
                .Replace("%id%", player.PlayerId.ToString())
                .Replace("%name%", player.DisplayName)
                .Replace("%userid%", player.UserId)
                .Replace("%role%", player.Role.ToString())
                .Replace("%health%", player.Health.ToString())
                .Replace("%maxhealth%", player.MaxHealth.ToString())
                .Replace("%room%", player.Room?.Name.ToString())
                .Replace("%position%", player.Position.ToString().Trim('(', ')'));

                switch (command.Type)
                {
                    case CommandType.Client:
                        Server.RunCommand($".{command.Command}", new SilentCommandSender());
                        break;
                    
                    case CommandType.Console:
                        Server.RunCommand($"{command.Command}", new SilentCommandSender());
                        break;
                    
                    case CommandType.RemoteAdmin:
                        Server.RunCommand($"/{command.Command}", new SilentCommandSender());
                        break;
                }
            }
        }

        private void HandleAudio(ColliderClasses classes, Player player)
        {
            foreach (PlayAudio play in classes.PlayAudio)
            {
                AudioPlayer audioPlayer = AudioPlayer.CreateDefault(parent: transform);
                audioPlayer.WithMinDistance(play.MinDistance);
                audioPlayer.WithMaxDistance(play.MaxDistance);
                audioPlayer.WithSpatial(play.IsSpatial);
                if (IsLocalFile(play.Path))
                {
                    audioPlayer.UseFile(Path.Combine(Main.Instance.Config?.AudioPath, play.Path), volume: play.Volume);
                }
                else
                    audioPlayer.UseFile(play.Path, volume: play.Volume);   
            }
        }
    }
}