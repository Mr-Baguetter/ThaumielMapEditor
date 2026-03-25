using AdminToys;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class LightObject : ServerObject
    {
        [YamlIgnore]
        public LightSourceToy Base { get; private set; }

        public float LightIntensity
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightIntensity = value;
            }
        }

        public float LightRange
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightRange = value;
            }
        }

        public Color LightColor
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightColor = value;
            }
        }

        public LightShadows ShadowType
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.ShadowType = value;
            }
        }

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
        }

        public LightType LightType
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightType = value;
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public LightShape LightShape
#pragma warning restore CS0618 // Type or member is obsolete
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.LightShape = value;
            }
        }

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
        }

        public float InnerSpotAngle
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.InnerSpotAngle = value;
            }
        }

        public override ObjectType ObjectType { get; set; } = ObjectType.Light;

        public void SpawnObject(SerializableObject serializable, SchematicData schematic)
        {
            LightSourceToy light = UnityEngine.Object.Instantiate(PrefabHelper.LightSource);
            Base = light;
            Object = light.gameObject;
            NetId = light.netId;
            NetworkServer.UnSpawn(light.gameObject);
            DeserializeValues(serializable);
            SetWorldTransform(schematic);

            light.LightIntensity = LightIntensity;
            light.LightRange = LightRange;
            light.LightColor = LightColor;
            light.ShadowType = ShadowType;
            light.ShadowStrength = ShadowStrength;
            light.LightType = LightType;
            light.LightShape = LightShape;
            light.SpotAngle = SpotAngle;
            light.InnerSpotAngle = InnerSpotAngle;

            NetworkServer.Spawn(light.gameObject);
            base.SpawnObject(schematic, serializable);
        }

        public void DeserializeValues(SerializableObject serializable)
        {
            if (serializable.ObjectType is not ObjectType.Light)
                return;

            if (!serializable.Values.TryConvertValue<float>("LightIntensity", out var intensity))
            {
                LogManager.Warn("Failed to parse LightIntensity");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("LightRange", out var range))
            {
                LogManager.Warn("Failed to parse LightRange");
                return;
            }
            if (!serializable.Values.TryConvertValue<Color>("LightColor", out var color))
            {
                LogManager.Warn("Failed to parse LightColor");
                return;
            }
            if (!serializable.Values.TryConvertValue<LightShadows>("ShadowType", out var shadowType))
            {
                LogManager.Warn("Failed to parse ShadowType");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("ShadowStrength", out var shadowStrength))
            {
                LogManager.Warn("Failed to parse ShadowStrength");
                return;
            }
            if (!serializable.Values.TryConvertValue<LightType>("LightType", out var lightType))
            {
                LogManager.Warn("Failed to parse LightType");
                return;
            }
#pragma warning disable CS0618 // Type or member is obsolete
            if (!serializable.Values.TryConvertValue<LightShape>("LightShape", out var lightShape))
            {
                LogManager.Warn("Failed to parse LightShape");
                return;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            if (!serializable.Values.TryConvertValue<float>("SpotAngle", out var spotAngle))
            {
                LogManager.Warn("Failed to parse SpotAngle");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("InnerSpotAngle", out var innerSpotAngle))
            {
                LogManager.Warn("Failed to parse InnerSpotAngle");
                return;
            }

            LightIntensity = intensity;
            LightRange = range;
            LightColor = color;
            ShadowType = shadowType;
            ShadowStrength = shadowStrength;
            LightType = lightType;
            LightShape = lightShape;
            SpotAngle = spotAngle;
            InnerSpotAngle = innerSpotAngle;
        }
    }
}
