using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;

namespace ThaumielMapEditor.API.Blocks.ServerObjects.Lockers
{
    public class LockerChamber
    {
        public uint Index { get; set; }
        public DoorPermissionFlags Permissions { get; set; }
        public List<ChamberData> Data { get; set; } = [];
    }

    public class ChamberData
    {
        public ItemType ItemType { get; set; }
        public float SpawnPercent { get; set; }
        public int AmountToSpawn { get; set; }
    }
}