using System.Collections.Generic;
using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
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
        /// Gets or sets the <see cref="GameObject"/> the <see cref="ClientSideObjectBase"/> is parented to on the client.
        /// </summary>
        public GameObject? Parent { get; internal set; }

        /// <summary>
        /// Gets or sets the netId of the <see cref="GameObject"/> this <see cref="ClientSideObjectBase"/> is parented to.
        /// </summary>
        public uint ParentId { get; internal set; }

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

        /// <summary>
        /// Sets the parent of the 
        /// </summary>
        /// <param name="player">The <see cref="Player"/> that will recive the parent message=</param>
        /// <param name="parentId">The <see cref="NetworkBehaviour.netId"/> of the parent</param>
        public void SetParent(Player player, uint parentId)
        {
            player.SendFakeRPC(NetId, typeof(AdminToyBase), nameof(AdminToyBase.RpcChangeParent), 0, parentId);

            GameObject? go = NetworkServer.spawned.TryGetValue(ParentId, out NetworkIdentity identity) ? identity.gameObject : null;
            if (go != null)
            {
                Parent = go;
            }
            else
                LogManager.Warn($"Failed to find GameObject with NetId {ParentId}!");
        }

        public T GetValue<T>(SerializableObject serializable, string key) =>
            serializable.Values.GetConvertValue<T>(key);

        public void HideForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            player.Connection.Send(new ObjectHideMessage { netId = NetId });
        }

        public void ShowForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            SpawnForPlayer(player);
        }

        public void DespawnForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            player.Connection.Send(new ObjectDestroyMessage { netId = NetId });
            SpawnedPlayers.Remove(player);
        }

        public uint DespawnForAllPlayers()
        {
            uint count = 0;
            foreach (Player player in Player.ReadyList)
            {
                if (player.IsHost)
                    continue;

                count++;
                DespawnForPlayer(player);
            }
            
            return count;
        }
    }
}