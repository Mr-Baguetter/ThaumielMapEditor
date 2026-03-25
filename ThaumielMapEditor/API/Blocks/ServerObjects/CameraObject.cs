using AdminToys;
using LabApi.Features.Wrappers;
using MapGeneration;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;
using CameraType = ThaumielMapEditor.API.Enums.CameraType;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class CameraObject : ServerObject
    {
        [YamlIgnore]
        public Scp079CameraToy Base { get; internal set; }
        public CameraType Type { get; internal set; }

        public string Label
        {
            get;
            set
            {
                if (field == value || Base == null)
                    return;

                Base.NetworkLabel = value;
                field = value;
            }
        }

        public Room Room
        {
            get;
            set
            {
                if (field == value || Base == null)
                    return;

                Base.NetworkRoom = value.Base;
                field = value;
            }
        } = Room.Get(RoomName.Outside).First();

        public Vector2 VerticalConstraint
        {
            get;
            set
            {
                if (field == value || Base == null)
                    return;

                Base.NetworkVerticalConstraint = value;
                field = value;
            }
        }

        public Vector2 HorizontalConstraint
        {
            get;
            set
            {
                if (field == value || Base == null)
                    return;

                Base.NetworkHorizontalConstraint = value;
                field = value;
            }
        }
        
        public Vector2 ZoomConstraint
        {
            get;
            set
            {
                if (field == value || Base == null)
                    return;
                
                Base.NetworkZoomConstraint = value;
                field = value;
            }
        }

        public override ObjectType ObjectType { get; set; } = ObjectType.Camera;

        public static Scp079CameraToy GetCameraPrefab(CameraType type)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return type switch
            {
                CameraType.Ez => PrefabHelper.CameraEz,
                CameraType.EzArm => PrefabHelper.CameraEzArm,
                CameraType.Hcz => PrefabHelper.CameraHcz,
                CameraType.Lcz => PrefabHelper.CameraLcz,
                CameraType.Sz => PrefabHelper.CameraSz,
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown CameraType: {type}")
            };
#pragma warning restore CS8603 // Possible null reference return.
        }

        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            Scp079CameraToy camera = UnityEngine.Object.Instantiate(GetCameraPrefab(GetValue<CameraType>(serializable, "CameraType")));
            NetworkServer.UnSpawn(camera.gameObject);
            Base = camera;
            Object = camera.gameObject;
            ParseValues(serializable);
            SetWorldTransform(schematic);
            NetworkServer.Spawn(camera.gameObject);
        }

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType is not ObjectType.Camera)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Camera");
                return;                
            }

            if (!serializable.Values.TryConvertValue<string>("Label", out var label))
            {
                LogManager.Warn("Failed to parse Label");
                return;
            }
            if (!serializable.Values.TryConvertValue<RoomName>("Room", out var room))
            {
                LogManager.Warn("Failed to parse Room");
                return;
            }
            if (serializable.Values.TryGetValue("VerticalConstraint", out var raw) && raw is IDictionary<object, object> dict)
            {
                float x = Convert.ToSingle(dict["x"]);
                float y = Convert.ToSingle(dict["y"]);

                VerticalConstraint = new(x, y);
            }
            if (serializable.Values.TryGetValue("HorizontalConstraint", out var raw1) && raw1 is IDictionary<object, object> dict1)
            {
                float x = Convert.ToSingle(dict1["x"]);
                float y = Convert.ToSingle(dict1["y"]);

                HorizontalConstraint = new(x, y);
            }
            if (serializable.Values.TryGetValue("ZoomConstraint", out var raw2) && raw2 is IDictionary<object, object> dict2)
            {
                float x = Convert.ToSingle(dict2["x"]);
                float y = Convert.ToSingle(dict2["y"]);

                ZoomConstraint = new(x, y);
            }

            Label = label;
            Room = Room.Get(room).First();
        }
    }
}