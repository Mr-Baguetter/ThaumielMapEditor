using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class WorkstationObject : ServerObject
    {
        /// <summary>
        /// The instantiated <c>WorkstationController</c> that backs this server object.
        /// It will be null until <see cref="SpawnObject"/> successfully instantiates the prefab.
        /// </summary>
        [YamlIgnore]
        public WorkstationController? Base { get; private set; }

        /// <inheritdoc/>
        public override ObjectType ObjectType { get; set; } = ObjectType.Workstation;

        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            if (PrefabHelper.Workstation == null)
            {
                LogManager.Warn($"Workstation prefab is null!");
                return;
            }

            WorkstationController workstationPrefab = UnityEngine.Object.Instantiate(PrefabHelper.Workstation);
            NetworkServer.UnSpawn(workstationPrefab.gameObject);
            Base = workstationPrefab;
            Object = Base.gameObject;
            NetId = Base.netId;
            SetWorldTransform(schematic);
            NetworkServer.Spawn(workstationPrefab.gameObject);
            base.SpawnObject(schematic, serializable);
        }
    }
}