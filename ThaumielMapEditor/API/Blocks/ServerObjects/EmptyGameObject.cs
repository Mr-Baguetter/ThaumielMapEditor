using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using UnityEngine;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class EmptyGameObject : ServerObject
    {
        [YamlIgnore]
        public GameObject? Base { get; private set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.GameObject;

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            GameObject gameObject = new()
            {
                name = serializable.Name
            };

            Base = gameObject;
            SetWorldTransform(schematic);
            base.SpawnObject(schematic, serializable);
        }
    }
}