// -----------------------------------------------------------------------
// <copyright file="TeleporterObject.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PlayerRoles;
using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class TeleporterObject : ServerObject
    {
        /// <summary>
        /// Gets the Id for this <see cref="TeleporterObject"/> instance
        /// </summary>
        public Guid Id { get; internal set; }

        /// <summary>
        /// Gets or sets the Id to teleport to for this <see cref="TeleporterObject"/> instance
        /// </summary>
        public List<Guid> Targets { get; set; } = [];

        /// <summary>
        /// Gets or sets the cooldown time for this <see cref="TeleporterObject"/> instance
        /// </summary>
        public float CoolDown { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RoleTypeId"/>s allowed for this <see cref="TeleporterObject"/> instance
        /// </summary>
        public List<RoleTypeId> AllowedRoles { get; set; } = [];

        /// <summary>
        /// Gets or sets whether this <see cref="TeleporterObject"/> instance uses perplayer cooldowns or global cooldowns.
        /// </summary>
        public bool PerPlayerCooldown { get; set; }

        /// <summary>
        /// Gets the <see cref="TeleporterFlags"/> of this <see cref="TeleporterObject"/> instance
        /// </summary>
        public TeleporterFlags Flags { get; set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Teleporter;

        [YamlIgnore]
        public TeleporterHandler TeleporterHandler { get; private set; }

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            ParseValues(serializable);
            SetWorldTransform(schematic);

            GameObject triggerObj = new($"Teleporter_{Id}");
            triggerObj.transform.position = Position;
            triggerObj.transform.rotation = Rotation;
            triggerObj.transform.localScale = Scale;

            BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            Rigidbody body = triggerObj.AddComponent<Rigidbody>();
            body.isKinematic = true;

            TeleporterHandler = triggerObj.AddComponent<TeleporterHandler>();
            TeleporterHandler.Init(this);

            base.SpawnObject(schematic, serializable);
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Teleporter)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Teleporter");
                return;
            }

            if (!serializable.Values.TryConvertValue<string>("Id", out var id))
            {
                LogManager.Warn("Failed to parse Id");
            }
            
            if (!Guid.TryParse(id, out var guid))
            {
                LogManager.Warn("Failed to parse Id as Guid");
            }
            
            if (!serializable.Values.TryConvertValue<List<string>>("Target", out var target))
            {
                LogManager.Warn("Failed to parse Target");
            }

            List<Guid> ids = [];
            foreach (string rawid in target)
            {
                if (!Guid.TryParse(rawid, out var result))
                    continue;
                
                ids.Add(result);
            }

            if (!serializable.Values.TryConvertValue<float>("CoolDown", out var coolDown))
            {
                LogManager.Warn("Failed to parse CoolDown");
            }
            
            if (!serializable.Values.TryConvertValue<List<RoleTypeId>>("AllowedRoles", out var allowedRoles))
            {
                LogManager.Warn("Failed to parse AllowedRoles");
            }
            
            if (!serializable.Values.TryConvertValue<bool>("PerPlayerCooldown", out var perPlayerCooldown))
            {
                LogManager.Warn("Failed to parse PerPlayerCooldown");
            }
            
            if (!serializable.Values.TryConvertValue<TeleporterFlags>("Flags", out var flags))
            {
                LogManager.Warn("Failed to parse Flags");
            }

            Id = guid;
            Targets = ids;
            CoolDown = coolDown;
            AllowedRoles = allowedRoles;
            PerPlayerCooldown = perPlayerCooldown;
            Flags = flags;
        }
    }
}