// -----------------------------------------------------------------------
// <copyright file="LightObjectServer.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using AdminToys;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class LightObjectServer : ServerObject
    {
        public LightSourceToy Base { get; private set; }

        public float Intensity
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightIntensity = value;
            }
        } = 1f;

        public float Range
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightRange = value;
            }
        } = 10f;

        public Color Color
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightColor = value;
            }
        } = Color.white;

        public LightShadows Shadows
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.ShadowType = value;
            }
        } = LightShadows.None;

        public float ShadowStrength
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.ShadowStrength = value;
            }
        } = 1f;

        public LightType Type
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightType = value;
            }
        } = LightType.Point;

        public float SpotAngle
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.SpotAngle = value;
            }
        } = 30f;

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Light;

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            ParseValues(serializable);
            LightSourceToy? light = UnityEngine.Object.Instantiate(PrefabHelper.LightSource);
            if (light == null)
                return;

            NetworkServer.UnSpawn(light.gameObject);
            Base = light;
            Object = light.gameObject;
            NetId = light.netId;
            Base.LightIntensity = Intensity;
            Base.LightRange = Range;
            Base.LightColor = Color;
            Base.ShadowType = Shadows;
            Base.ShadowStrength = ShadowStrength;
            Base.LightType = Type;
            Base.SpotAngle = SpotAngle;
            light.gameObject.transform.position = Position;
            light.gameObject.transform.rotation = Rotation;
            light.gameObject.transform.localScale = Scale;
            NetworkServer.Spawn(light.gameObject);
            base.SpawnObject(schematic, serializable);
        }

        /// <summary>
        /// Reads serialized schematic data into this object.
        /// </summary>
        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Light)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Light");
                return;
            }

            if (serializable.Values.TryConvertValue<float>("LightIntensity", out var intensity))
            {
                Intensity = intensity;
            }

            if (serializable.Values.TryConvertValue<float>("LightRange", out var range))
            {
                Range = range;
            }

            if (serializable.Values.TryConvertValue<Color>("LightColor", out var color))
            {
                Color = color;
            }

            if (serializable.Values.TryConvertValue<LightShadows>("ShadowType", out var shadows))
            {
                Shadows = shadows;
            }

            if (serializable.Values.TryConvertValue<float>("ShadowStrength", out var shadowStrength))
            {
                ShadowStrength = shadowStrength;
            }

            if (serializable.Values.TryConvertValue<LightType>("LightType", out var type))
            {
                Type = type;
            }

            if (serializable.Values.TryConvertValue<float>("SpotAngle", out var spot))
            {
                SpotAngle = spot;
            }
        }
    }
}