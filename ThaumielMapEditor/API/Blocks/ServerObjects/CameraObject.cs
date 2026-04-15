// -----------------------------------------------------------------------
// <copyright file="CameraObject.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

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
        /// <summary>
        /// The underlying in game camera toy instance.
        /// </summary>
#pragma warning disable CS8618
        [YamlIgnore]
        public Scp079CameraToy Base { get; internal set; }
#pragma warning restore CS8618

        /// <summary>
        /// The camera prefab type (mapped to a specific prefab via <see cref="GetCameraPrefab"/>).
        /// </summary>
        public CameraType Type { get; internal set; }

        /// <summary>
        /// Display label for the camera. Setting this property updates the networked label on
        /// the underlying <see cref="Base"/> when available.
        /// </summary>
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
        } = string.Empty;

        /// <summary>
        /// The <see cref="Room"/> that the camera belongs to.
        /// Setting this property updates <see cref="Scp079CameraToy.NetworkRoom"/> on <see cref="Base"/>.
        /// </summary>
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

        /// <summary>
        /// Vertical rotation constraint applied to the camera.
        /// When set, the value is copied to <see cref="Scp079CameraToy.NetworkVerticalConstraint"/>.
        /// </summary>
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

        /// <summary>
        /// Horizontal rotation constraint applied to the camera.
        /// When set, the value is copied to <see cref="Scp079CameraToy.NetworkHorizontalConstraint"/>.
        /// </summary>
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
        
        /// <summary>
        /// Zoom constraint for the camera (min/max).
        /// When set, the value is copied to <see cref="Scp079CameraToy.NetworkZoomConstraint"/>.
        /// </summary>
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

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Camera;

        /// <summary>
        /// Returns the corresponding camera prefab instance for the provided <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The camera type to resolve to a prefab.</param>
        /// <returns>The matching <see cref="Scp079CameraToy"/> prefab from <see cref="PrefabHelper"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an unknown <paramref name="type"/> is provided.</exception>
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

        /// <inheritdoc/>
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

        /// <summary>
        /// Parses values from a <see cref="SerializableObject"/> and applies them to this instance.
        /// </summary>
        /// <param name="serializable">The serialized camera object to parse.</param>
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
            }

            if (!serializable.Values.TryConvertValue<RoomName>("Room", out var room))
            {
                LogManager.Warn("Failed to parse Room");
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