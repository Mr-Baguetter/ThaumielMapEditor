using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Represents a server-side waypoint object used by the map editor.
    /// Wraps an underlying <see cref="WaypointToy"/> instance and exposes
    /// serializable properties such as bounds visualization, priority and bounds size.
    /// </summary>
    public class WaypointObject : ServerObject
    {
        /// <summary>
        /// Reference to the instantiated <see cref="WaypointToy"/> on the server.
        /// </summary>
        [YamlIgnore]
        public WaypointToy Base { get; private set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Waypoint;

        /// <summary>
        /// Whether the waypoint's bounds are visualized in the editor/runtime.
        /// Setting this property updates the underlying <see cref="WaypointToy.VisualizeBounds"/>
        /// when the toy instance is available.
        /// </summary>
        public bool VisualizeBounds
        {
            get;

            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.VisualizeBounds = value;
            }
        }

        /// <summary>
        /// Priority value for the waypoint. Higher values can be used to influence
        /// ordering or selection logic that consumes waypoint priorities.
        /// Setting this property updates the underlying <see cref="WaypointToy.Priority"/>
        /// when the toy instance is available.
        /// </summary>
        public float Priority
        {
            get;
            
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.Priority = value;
            }
        }

        /// <summary>
        /// Size of the waypoint bounds as a <see cref="Vector3"/>
        /// Setting this property updates the underlying <see cref="WaypointToy.BoundsSize"/>
        /// when the toy instance is available.
        /// </summary>
        public Vector3 BoundsSize
        {
            get;

            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.BoundsSize = value;
            }
        }

        /// <summary>
        /// Identifier assigned to this waypoint instance.
        /// </summary>
        public byte WaypointId { get; private set; }

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            if (PrefabHelper.WaypointToy == null)
            {
                LogManager.Warn($"Failed to spawn Waypoint. Prefab is null");
                return;
            }

            WaypointToy toy = UnityEngine.Object.Instantiate(PrefabHelper.WaypointToy);
            NetworkServer.UnSpawn(toy.gameObject);
            toy.VisualizeBounds = VisualizeBounds;
            toy.Priority = Priority;
            toy.BoundsSize = BoundsSize;
            ParseValues(serializable);
            Object = toy.gameObject;
            NetId = toy.netId;
            SetWorldTransform(schematic);
            NetworkServer.Spawn(toy.gameObject);

            base.SpawnObject(schematic, serializable);
        }

        /// <summary>
        /// Parses values from a <see cref="SerializableObject"/> and applies them to this <see cref="WaypointObject"/> instance.
        /// </summary>
        /// <param name="serializable">The serialized object containing values to parse.</param>
        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Waypoint)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Waypoint");
                return;                
            }
            
            if (!serializable.Values.TryConvertValue<bool>("VisualizeBounds", out var visualizeBounds))
            {
                LogManager.Warn("Failed to parse VisualizeBounds");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("Priority", out var priority))
            {
                LogManager.Warn("Failed to parse Priority");
                return;
            }
            if (serializable.Values.TryGetValue("BoundsSize", out var raw) && raw is IDictionary<object, object> dict)
            {
                float x = Convert.ToSingle(dict["x"]);
                float y = Convert.ToSingle(dict["y"]);
                float z = Convert.ToSingle(dict["z"]);

                BoundsSize = new(x, y, z);
            }

            VisualizeBounds = visualizeBounds;
            Priority = priority;
        }
    }
}