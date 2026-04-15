// -----------------------------------------------------------------------
// <copyright file="InteractableTrigger.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using SecretLabNAudio.Core;
using SecretLabNAudio.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Blocks.ServerObjects;
using ThaumielMapEditor.API.Components.Tools.Helpers;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.Commands;
using ThaumielMapEditor.HarmonyPatches;
using static AdminToys.InvisibleInteractableToy;
using static ThaumielMapEditor.API.Components.Tools.Helpers.RunCommand;

namespace ThaumielMapEditor.API.Components.Tools
{
    public class InteractableTrigger : ToolBase
    {
        public Vector3 Bounds;

        public float InteractionTime;

        public DoorPermissionFlags Permission;

        public ColliderShape Shape;
#pragma warning disable CS8618
        public InteractableClasses OnInteracted;

        public InteractableClasses OnInteractionDenied;

        public InteractionObject Interactable;
#pragma warning restore CS8618

        public override ToolType Type => ToolType.InteractableTrigger;

        public override void Init(ServerObject obj, SchematicData schem, Dictionary<string, object> properties)
        {
            base.Init(obj, schem, properties);
            ParseValues(properties);
            Interactable = new()
            {
                Position = obj.Position,
                Rotation = obj.Rotation,
                Shape = Shape,
                InteractionDuration = InteractionTime,
                Permissions = Permission
            };

            Interactable.SpawnObject(schem);
            Interactable.Object?.transform.SetParent(obj.Object?.transform, true);
            Interactable.Object?.transform.localScale = Bounds;
            InteractionObject.OnInteracted += Interacted;
            InteractionObject.OnSearched += Interacted;
            InteractToyValidatePatch.OnDenied += Denied;
        }

        private void OnDestroy()
        {
            InteractionObject.OnInteracted -= Interacted;
            InteractionObject.OnSearched -= Interacted;
            InteractToyValidatePatch.OnDenied -= Denied;
        }

        private void Denied(InteractionObject obj, Player player)
        {
            if (obj != Interactable)
                return;

            HandleAnimation(OnInteractionDenied, player);
            HandleEffect(OnInteractionDenied, player);
            HandleCommand(OnInteractionDenied, player);
            HandleAudio(OnInteractionDenied, player);
        }

        public void Interacted(InteractionObject obj, Player player)
        {
            if (obj != Interactable)
                return;

            HandleAnimation(OnInteracted, player);
            HandleEffect(OnInteracted, player);
            HandleCommand(OnInteracted, player);
            HandleAudio(OnInteracted, player);
        }

        private void HandleAnimation(InteractableClasses classes, Player player)
        {
            foreach (PlayAnimation play in classes.PlayAnimation)
            {
                Schematic?.AnimationController.Play(play.ResolvedAnimationName);
            }
        }

        private void HandleEffect(InteractableClasses classes, Player player)
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

        private void HandleCommand(InteractableClasses classes, Player player)
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

        private void HandleAudio(InteractableClasses classes, Player player)
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

        public void ParseValues(Dictionary<string, object> properties)
        {
            if (properties.TryGetValue("OnInteracted", out var interacted))
                OnInteracted = MapToObject<InteractableClasses>(interacted) ?? new();

            if (properties.TryGetValue("OnInteractionDenied", out var denied))
                OnInteractionDenied = MapToObject<InteractableClasses>(denied) ?? new();

            if (properties.TryConvertValue<ColliderShape>("Shape", out var shape))
                Shape = shape;

            if (properties.TryConvertValue<DoorPermissionFlags>("Permission", out var perms))
                Permission = perms;

            if (properties.TryGetValue("InteractionTime", out var time))
                InteractionTime = Convert.ToSingle(time);

            if (properties.TryGetValue("Bounds", out var raw) && raw is IDictionary<object, object> dict)
            {
                float x = Convert.ToSingle(dict["x"]);
                float y = Convert.ToSingle(dict["y"]);
                float z = Convert.ToSingle(dict["z"]);

                Bounds = new(x, y, z);
            }
        }
    }
}