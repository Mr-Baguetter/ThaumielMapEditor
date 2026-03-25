using AdminToys;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Interfaces;
using ThaumielMapEditor.API.Serialization;
using YamlDotNet.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class CapybaraObject : ServerObject, ICullableObject
    {
        [YamlIgnore]
        public CapybaraToy? Base { get; private set; }

        public override ObjectType ObjectType { get; set; } = ObjectType.Capybara;

        public bool Collisions
        {
            get;

            set
            {
                if (field == value || Base == null)
                    return;

                Base.CollisionsEnabled = value;
                field = value;
            }
        }

        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            if (PrefabHelper.Capybara == null)
            {
                LogManager.Warn($"Failed to get Capybara prefab. Is null");
                return;
            }

            CapybaraToy capy = UnityEngine.Object.Instantiate(PrefabHelper.Capybara);
            if (capy == null)
            {
                LogManager.Warn($"Failed to Instantiate Capybara toy");
                return;
            }

            NetworkServer.UnSpawn(capy.gameObject);
            Base = capy;
            Object = capy.gameObject;
            NetId = capy.netId;
            SetWorldTransform(schematic);
            Base.CollisionsEnabled = Collisions;
            NetworkServer.Spawn(capy.gameObject);
            
            base.SpawnObject(schematic, serializable);
        }
    }
}