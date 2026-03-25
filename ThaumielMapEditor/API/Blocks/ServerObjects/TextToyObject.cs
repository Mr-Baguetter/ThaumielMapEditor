using System;
using System.Collections.Generic;
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
    public class TextToyObject : ServerObject
    {
        [YamlIgnore]
        public TextToy Base { get; private set; }

        public string TextFormat
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.TextFormat = value;
            }
        }
        public Vector2 DisplaySize
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.DisplaySize = value;
            }
        }
        public override ObjectType ObjectType { get; set; } = ObjectType.TextToy;

        public void SpawnObject(SerializableObject serializable, SchematicData schematic)
        {
            if (PrefabHelper.TextToy == null)
            {
                LogManager.Warn($"Failed to spawn TextToy. Prefab is null.");
                return;
            }

            TextToy textToy = UnityEngine.Object.Instantiate(PrefabHelper.TextToy);
            Base = textToy;
            Object = textToy.gameObject;
            NetId = textToy.netId;
            NetworkServer.UnSpawn(Base.gameObject);
            ParseValues(serializable);
            SetWorldTransform(schematic);

            Base.TextFormat = TextFormat;
            Base.DisplaySize = DisplaySize;
            NetworkServer.Spawn(Base.gameObject);
            base.SpawnObject(schematic, serializable);
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.TextToy)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as TextToy");
                return;                
            }

            if (!serializable.Values.TryConvertValue<string>("TextFormat", out var textFormat))
            {
                LogManager.Warn("Failed to parse TextFormat");
                return;
            }
            if (serializable.Values.TryGetValue("DisplaySize", out var raw) && raw is IDictionary<object, object> dict)
            {
                float x = Convert.ToSingle(dict["x"]);
                float y = Convert.ToSingle(dict["y"]);

                DisplaySize = new(x, y);
            }

            TextFormat = textFormat;
        }
    }
}