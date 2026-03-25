using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class PickupObject : ServerObject
    {
        public ItemType ItemToSpawn { get; private set; }

        public float SpawnPercentage { get; private set; }

        public uint MaxAmount { get; private set; }

        public bool IsInfinite { get; private set; }

        public override ObjectType ObjectType { get; set; } = ObjectType.Pickup;

        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            SetWorldTransform(schematic);

            if (!ParseValues(serializable))
            {
                LogManager.Warn($"Failed to parse pickup values, aborting spawn.");
                return;
            }

            if (SpawnPercentage < 100f && UnityEngine.Random.Range(0f, 100f) > SpawnPercentage)
                return;

            Pickup? pickup = Pickup.Create(ItemToSpawn, Position, Rotation);
            if (pickup == null)
            {
                LogManager.Warn($"Failed to create pickup of type {ItemToSpawn}.");
                return;
            }

            Object = pickup.GameObject;
            NetId = pickup.Base.netId;

            pickup.Spawn();

            LogManager.Debug($"Spawned pickup {ItemToSpawn} at {Position}");
            base.SpawnObject(schematic, serializable);
        }

        public bool ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Pickup)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Pickup");
                return false;
            }

            if (!serializable.Values.TryConvertValue<ItemType>("ItemToSpawn", out var item))
            {
                LogManager.Warn("Failed to parse ItemToSpawn");
                return false;
            }
            if (!serializable.Values.TryConvertValue<float>("SpawnPercentage", out var percentage))
            {
                LogManager.Warn("Failed to parse SpawnPercentage");
                return false;
            }
            if (!serializable.Values.TryConvertValue<uint>("MaxAmount", out var max))
            {
                LogManager.Warn("Failed to parse MaxAmount");
                return false;
            }
            if (!serializable.Values.TryConvertValue<bool>("IsInfinite", out var infinite))
            {
                LogManager.Warn("Failed to parse IsInfinite");
                return false;
            }

            ItemToSpawn = item;
            SpawnPercentage = percentage;
            MaxAmount = max;
            IsInfinite = infinite;
            return true;
        }
    }
}
