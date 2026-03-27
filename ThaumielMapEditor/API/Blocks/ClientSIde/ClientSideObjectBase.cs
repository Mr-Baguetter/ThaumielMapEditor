using System.Collections.Generic;
using LabApi.Features.Wrappers;
using Mirror;
using ThaumielMapEditor.API.Enums;
using UnityEngine;

namespace ThaumielMapEditor.API.Blocks.ClientSide
{
    public abstract class ClientSideObjectBase
    {

        /// <summary>
        /// All the <see cref="Player"/>s that this <see cref="ClientSideObjectBase"/> instance has been spawned for.
        /// </summary>
        public HashSet<Player> SpawnedPlayers { get; internal set; } = [];
        
        /// <summary>
        /// Gets or sets the position of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        /// <remarks>
        /// This will be automatically synced to players.
        /// </remarks>
        public virtual Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the scale of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        /// <remarks>
        /// This will be automatically synced to players.
        /// </remarks>
        public virtual Vector3 Scale { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        /// <remarks>
        /// This will be automatically synced to players.
        /// </remarks>
        public virtual Quaternion Rotation { get; set; }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        public virtual bool IsStatic { get; set; }

        /// <summary>
        /// Gets or sets the sync interval of the <see cref="ClientSideObjectBase"/>.
        /// </summary>
        public virtual byte MovementSmoothing { get; set; }

        /// <summary>
        /// Gets or sets the colliders of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        public virtual Collider[] ServerColliders { get; set; } = [];

        /// <summary>
        /// Gets or sets the netid of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        public abstract uint NetId { get; set; }

        /// <summary>
        /// Gets or sets the object type of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        public abstract ObjectType ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the asset id of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        public abstract uint AssetId { get; set; }

        /// <summary>
        /// Spawns the <see cref="ClientSideObjectBase"/> instance for the specified player.
        /// </summary>
        /// <param name="player">The player to spawn the <see cref="ClientSideObjectBase"/> instance to.</param>
        public virtual void SpawnForPlayer(Player player) { }

        /// <summary>
        /// Destroys this <see cref="ClientSideObjectBase"/> instance for the specified <see cref="Player"/>
        /// </summary>
        /// <param name="player">The <see cref="Player"/> to destroy this object on.</param>
        public void DestroyForPlayer(Player player)
        {
            player.Connection.Send(new ObjectDestroyMessage { netId = NetId });
            SpawnedPlayers.Remove(player);
        }

        /// <summary>
        /// Destroys this <see cref="ClientSideObjectBase"/> instance for all <see cref="Player"/>s
        /// </summary>
        public void DestroyForAllPlayers()
        {
            foreach (Player player in Player.ReadyList)
            {
                if (player.IsHost)
                    continue;

                DestroyForPlayer(player);
            }
        }
    }
}