// -----------------------------------------------------------------------
// <copyright file="ObjectHealth.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using MEC;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Extensions;
using UnityEngine;
using ThaumielMapEditor.API.Enums;
using PlayerStatsSystem;
using PlayerRoles.PlayableScps.Scp939;
using InventorySystem.Items.Scp1509;
using PlayerRoles.PlayableScps.Scp1507;

namespace ThaumielMapEditor.API.Components.Tools
{
    public class ObjectHealth : ToolBase, IDestructible
    {
        /// <summary>
        /// Defines the types that <see cref="ObjectHealth"/> will use.
        /// </summary>
        public enum DestroyState
        {
            Animate,
            ApplyPhysics,
            Destroy
        }

        /// <inheritdoc/>
        public override ToolType Type => ToolType.Health;

        /// <summary>
        /// Gets the <see cref="DestroyState"/> that the <see cref="ObjectHealth"/> instance will use when destroyed.
        /// </summary>
        public DestroyState State { get; private set; } = DestroyState.Destroy;

        /// <summary>
        /// Gets or sets the max health that the <see cref="ObjectHealth"/> instance has.
        /// </summary>
        public float HealthMax { get; set; } = 100f;

        /// <summary>
        /// Gets or sets the current health that the <see cref="ObjectHealth"/> instance has.
        /// </summary>
        public float Health
        {
            get;
            set
            {
                field = Mathf.Max(0f, value);
                if (field <= 0)
                    Destroy();
            }
        } = 100f;

        /// <summary>
        /// Gets or sets the allowed <see cref="DamageType"/>s that the <see cref="ObjectHealth"/> instance can be damaged by.
        /// </summary>
        public List<DamageType> AllowedDamage = [];

        /// <summary>
        /// Gets or sets the amount of time in seconds that the <see cref="ObjectHealth"/> instance will have untill it despawns after being destroyed.
        /// </summary>
        public float DespawnTime { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the animation name that the <see cref="ObjectHealth"/> instance will play if the <see cref="State"/> is <see cref="DestroyState.Animate"/>.
        /// </summary>
        public string StateName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the launch force of the <see cref="ObjectHealth"/> instance that will be applied to the <see cref="Rigidbody"/> if the <see cref="State"/> is <see cref="DestroyState.ApplyPhysics"/>.
        /// </summary>
        public Vector3 Force { get; set; } = Vector3.zero;

        public uint NetworkId => Object.NetId;

        public Vector3 CenterOfMass => Vector3.zero;

        /// <inheritdoc/>
        public override void Init(ServerObject obj, SchematicData schem, Dictionary<string, object> properties)
        {
            Object = obj;
            Schematic = schem;

            if (!properties.TryConvertValue<DestroyState>("State", out var state))
            {
                LogManager.Warn("Failed to parse State");
            }

            if (!properties.TryConvertValue<float>("MaxHealth", out var max))
            {
                LogManager.Warn("Failed to parse MaxHealth");
            }

            if (!properties.TryConvertValue<List<DamageType>>("AllowedDamage", out var allowed))
            {
                LogManager.Warn("Failed to parse AllowedDamage");
            }

            if (!properties.TryConvertValue<float>("Despawn", out var despawn))
            {
                LogManager.Warn("Failed to parse Despawn");
            }

            switch (state)
            {
                case DestroyState.Animate:
                    if (properties.TryConvertValue<string>("StateName", out var name))
                    {
                        StateName = name;
                    }

                    break;
            }

            State = state;
            HealthMax = max;
            AllowedDamage = allowed;
            DespawnTime = despawn;
        }

        /// <summary>
        /// Destroys the <see cref="ObjectHealth"/> instance and applies the destroy state from <see cref="State"/>
        /// </summary>
        public void Destroy()
        {
            switch (State)
            {
                case DestroyState.Animate:
                    Schematic.AnimationController.Play(StateName, Object.Name);
                    Timing.CallDelayed(DespawnTime, () => Object.DestroyObject(Schematic));
                    break;

                case DestroyState.ApplyPhysics:
                    Object.MovementSmoothing = 0;
                    if (!gameObject.TryGetComponent<Rigidbody>(out var rigidbody))
                        rigidbody = gameObject.AddComponent<Rigidbody>();
                    
                    rigidbody.isKinematic = false;
                    rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    if (Force != Vector3.zero)
                    {
                        rigidbody.AddForce(Force, ForceMode.Impulse);
                    }
                    
                    Timing.CallDelayed(DespawnTime, () => Object.DestroyObject(Schematic));
                    break;

                case DestroyState.Destroy:
                    Object.DestroyObject(Schematic);
                    break;
            }
        }

        public bool Damage(float damage, DamageHandlerBase handler, Vector3 exactHitPos)
        {
            switch (handler)
            {
                case FirearmDamageHandler firearm when AllowedDamage.Contains(DamageType.Shot):
                    Health -= damage;
                    return true;

                case ExplosionDamageHandler explosion when AllowedDamage.Contains(DamageType.Explosion):
                    Health -= damage;
                    return true;

                case Scp939DamageHandler scp939 when AllowedDamage.Contains(DamageType.Scp939Lunge) || AllowedDamage.Contains(DamageType.Scp939Swipe):
                    Health -= damage;
                    return true;

                case Scp096DamageHandler scp096 when AllowedDamage.Contains(DamageType.Scp096Charge) || AllowedDamage.Contains(DamageType.Scp096Swipe):
                    Health -= damage;
                    return true;
                    
                case JailbirdDamageHandler jailbird when AllowedDamage.Contains(DamageType.JailbirdCharge) || AllowedDamage.Contains(DamageType.JailbirdHit):
                    Health -= damage;
                    return true;

                case DisruptorDamageHandler disruptor when AllowedDamage.Contains(DamageType.DisruptorBurst) || AllowedDamage.Contains(DamageType.DisruptorCharge):
                    Health -= damage;
                    return true;

                case MicroHidDamageHandler micro when AllowedDamage.Contains(DamageType.MicroHidQuick) || AllowedDamage.Contains(DamageType.MircoHidFullCharge) || AllowedDamage.Contains(DamageType.MircoHidBroken):
                    Health -= damage;
                    return true;

                case Scp1509DamageHandler scp1509 when AllowedDamage.Contains(DamageType.Scp1509):
                    Health -= damage;
                    return true;

                case MarshmallowDamageHandler marshmallow when AllowedDamage.Contains(DamageType.Marshmallow):
                    Health -= damage;
                    return true;

                case Scp1507DamageHandler scp1507 when AllowedDamage.Contains(DamageType.Scp1507):
                    Health -= damage;
                    return true;
            }

            return false;
        }
    }
}