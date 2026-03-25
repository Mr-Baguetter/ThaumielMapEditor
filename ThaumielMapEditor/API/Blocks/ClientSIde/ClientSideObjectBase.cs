using System.Collections.Generic;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Enums;

namespace ThaumielMapEditor.API.Blocks.ClientSide
{
    public abstract class ClientSideObjectBase
    {
        public HashSet<Player> SpawnedPlayers { get; set; } = [];
        
        public virtual Vector3 Position { get; set; }
        public virtual Vector3 Scale { get; set; }
        public virtual Quaternion Rotation { get; set; }
        public virtual bool IsStatic { get; set; }
        public virtual byte MovementSmoothing { get; set; }
        public abstract uint NetId { get; set; }
        public abstract ObjectType ObjectType { get; set; }
        public abstract uint AssetId { get; set; }

        public virtual void SpawnForPlayer(Player player) { }
    }
}