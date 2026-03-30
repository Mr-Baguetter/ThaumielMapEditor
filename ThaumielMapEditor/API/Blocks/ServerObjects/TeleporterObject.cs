using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using PlayerRoles;
using ThaumielMapEditor.API.Components;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;

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
        public Guid Target { get; set; }

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

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Teleporter;

        public TeleporterHandler TeleporterHandler { get; private set; }

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            PrimitiveObjectToy primitive = PrimitiveObjectToy.Create();
            ParseValues(serializable);
            SetWorldTransform(schematic);

            primitive.Position = Position;
            primitive.Rotation = Rotation;
            primitive.Scale = Scale;
            primitive.Flags = AdminToys.PrimitiveFlags.None;
            BoxCollider collider = primitive.Base.gameObject.AddComponent<BoxCollider>();
            collider.size = Scale;
            collider.isTrigger = true;
            TeleporterHandler = primitive.Base.gameObject.AddComponent<TeleporterHandler>();
            TeleporterHandler.Init(primitive, this);

            base.SpawnObject(schematic, serializable);
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Teleporter)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Teleporter");
                return;                
            }

            if (!serializable.Values.TryConvertValue<Guid>("Id", out var id))
            {
                LogManager.Warn("Failed to parse Id");
                return;
            }
            if (!serializable.Values.TryConvertValue<Guid>("Target", out var target))
            {
                LogManager.Warn("Failed to parse Target");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("CoolDown", out var coolDown))
            {
                LogManager.Warn("Failed to parse CoolDown");
                return;
            }
            if (!serializable.Values.TryConvertValue<List<RoleTypeId>>("AllowedRoles", out var allowedRoles))
            {
                LogManager.Warn("Failed to parse AllowedRoles");
                return;
            }
            if (!serializable.Values.TryConvertValue<bool>("PerPlayerCooldown", out var perPlayerCooldown))
            {
                LogManager.Warn("Failed to parse PerPlayerCooldown");
                return;
            }

            Id = id;
            Target = target;
            CoolDown = coolDown;
            AllowedRoles = allowedRoles;
            PerPlayerCooldown = perPlayerCooldown;
        }
    }
}