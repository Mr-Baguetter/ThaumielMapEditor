// -----------------------------------------------------------------------
// <copyright file="TextToyObject.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

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
        /// <summary>
        /// The instantiated runtime <see cref="TextToy"/> associated with this server object.
        /// </summary>
        /// <remarks>
        /// It will be null until <see cref="SpawnObject(SchematicData, SerializableObject)"/> successfully instantiates the prefab.
        /// </remarks>
        [YamlIgnore]
#pragma warning disable CS8618
        public TextToy Base { get; private set; }
#pragma warning restore CS8618

        /// <summary>
        /// The text format string used by the <see cref="TextToy"/> for rendering text.
        /// </summary>
        /// <remarks>
        /// Setting this property updates the underlying <see cref="Base"/> instance's <c>TextFormat</c> if the
        /// runtime object has already been created.
        /// </remarks>
        public string Text
        {
            get;
            set
            {
                if (field == value)
                    return;

                field = value;
                Base?.TextFormat = value;
            }
        } = string.Empty;

        /// <summary>
        /// The display size (width, height) used by the <see cref="TextToy"/> when rendering text.
        /// </summary>
        /// <remarks>
        /// Setting this property updates the underlying <see cref="Base"/> instance's <c>DisplaySize</c> if the
        /// runtime object has already been created.
        /// </remarks>
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

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.TextToy;

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
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

            Base.TextFormat = Text;
            Base.DisplaySize = DisplaySize;
            NetworkServer.Spawn(Base.gameObject);
            base.SpawnObject(schematic, serializable);
        }

        /// <summary>
        /// Parses values from the provided <see cref="SerializableObject"/> and applies them to this instance.
        /// </summary>
        /// <param name="serializable">The serialized object containing keys such as "TextFormat" and "DisplaySize".</param>
        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.TextToy)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as TextToy");
                return;
            }

            if (!serializable.Values.TryConvertValue<string>("Text", out var text))
            {
                LogManager.Warn("Failed to parse Text");
            }

            if (serializable.Values.TryGetValue("DisplaySize", out var raw) && raw is IDictionary<object, object> dict)
            {
                float x = Convert.ToSingle(dict["x"]);
                float y = Convert.ToSingle(dict["y"]);

                DisplaySize = new(x, y);
            }

            Text = text;
        }
    }
}