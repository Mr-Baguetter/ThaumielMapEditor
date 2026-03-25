using System;
using AdminToys;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class TargetDummyObject : ServerObject
    {
        [YamlIgnore]
        public ShootingTarget Base { get; private set; }

        public TargetType Type { get; private set; }

        public ShootingTarget GetPrefab(TargetType type)
        {
			ShootingTarget prefab = type switch
			{
				TargetType.Binary => PrefabHelper.ShootingTargetBinary,
				TargetType.ClassD => PrefabHelper.ShootingTargetDBoy,
				TargetType.Sport => PrefabHelper.ShootingTargetSport,
				_ => throw new InvalidOperationException(),
			};

            return prefab;
        }

        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            ParseValues(serializable);
            ShootingTarget target = UnityEngine.Object.Instantiate(GetPrefab(Type));
            NetworkServer.UnSpawn(target.gameObject);
            Object = target.gameObject;
            NetId = target.netId;
            Base = target;
            SetWorldTransform(schematic);
            NetworkServer.Spawn(target.gameObject);

            base.SpawnObject(schematic, serializable);
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Target)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Target Dummy");
                return;                
            }

            if (!serializable.Values.TryConvertValue<TargetType>("TargetType", out var type))
            {
                LogManager.Warn("Failed to parse TargetType");
                return;
            }

            Type = type;
        }
    }
}