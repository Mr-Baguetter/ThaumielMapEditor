// -----------------------------------------------------------------------
// <copyright file="ClientSideObjectBase.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
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
        internal ulong _pendingDirtyBits = 0;
        internal readonly SortedDictionary<ulong, Action<NetworkWriter>> _pendingWrites = [];

        /// <summary>
        /// All the <see cref="Player"/>s that this <see cref="ClientSideObjectBase"/> instance has been spawned for.
        /// </summary>
        public HashSet<Player> SpawnedPlayers { get; internal set; } = [];

        /// <summary>
        /// Gets a value indicating whether this <see cref="ClientSideObjectBase"/> has been spawned for any players.
        /// </summary>
        public bool Spawned { get; internal set; } = false;

        /// <summary>
        /// Gets the object id from the <see cref="SerializableObject"/> this <see cref="ClientSideObjectBase"/> instance was generated from.
        /// </summary>
        public int ObjectId { get; internal set; }

        /// <summary>
        /// Gets the parent id from the <see cref="SerializableObject"/> this <see cref="ClientSideObjectBase"/> instance was generated from.
        /// </summary>
        public int ParentId { get; internal set; }

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
        /// Gets or sets the world position of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        public Vector3 WorldPosition { get; set; }

        /// <summary>
        /// Gets or sets the world rotation of the <see cref="ClientSideObjectBase"/> instance.
        /// </summary>
        public Quaternion WorldRotation { get; set; }

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
        public uint ParentNetId { get; internal set; }

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

            GameObject? go = NetworkServer.spawned.TryGetValue(ParentNetId, out NetworkIdentity identity) ? identity.gameObject : null;
            if (go != null)
            {
                Parent = go;
            }
            else
                LogManager.Warn($"Failed to find GameObject with NetId {ParentNetId}!");
        }

        /// <summary>
        /// Gets a value from a <see cref="SerializableObject"/> by key, converting it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="serializable">The serializable object to retrieve the value from.</param>
        /// <param name="key">The key of the value to retrieve.</param>
        /// <returns>The value associated with the key, converted to <typeparamref name="T"/>.</returns>
        public T GetValue<T>(SerializableObject serializable, string key) =>
            serializable.Values.GetConvertValue<T>(key);

        /// <summary>
        /// Hides this object for the specified player.
        /// </summary>
        /// <param name="player">The player to hide this object for.</param>
        public void HideForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            player.Connection.Send(new ObjectHideMessage { netId = NetId });
        }

        /// <summary>
        /// Shows this object for the specified player.
        /// </summary>
        /// <param name="player">The player to show this object for.</param>
        public void ShowForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            SpawnForPlayer(player);
        }

        /// <summary>
        /// Destroys this object for the specified player.
        /// </summary>
        /// <param name="player">The player to despawn this object for.</param>
        public void DespawnForPlayer(Player player)
        {
            if (player.IsHost)
                return;

            player.Connection.Send(new ObjectDestroyMessage { netId = NetId });
            SpawnedPlayers.Remove(player);
        }

        /// <summary>
        /// Destroys this object for all ready players.
        /// </summary>
        /// <returns>The number of players this object was despawned for.</returns>
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

        /// <summary>
        /// Syncs the specified bits to all <see cref="Player"/>s this <see cref="ClientSideObjectBase"/> is spawned for
        /// </summary>
        /// <param name="dirtyBits">The bits to sync.</param>
        /// <param name="writeValues">The writer to use.</param>
        public void SyncToPlayers(ulong dirtyBits, Action<NetworkWriter> writeValues)
        {
            if (SpawnedPlayers.Count == 0 || !Spawned)
                return;

            _pendingDirtyBits |= dirtyBits;
            _pendingWrites[dirtyBits] = writeValues;
            FlushSync();
        }

        /// <summary>
        /// Syncs all the pending bits in <see cref="_pendingWrites"/> and <see cref="_pendingDirtyBits"/>.
        /// </summary>
        public void FlushSync()
        {
            if (_pendingDirtyBits == 0 || _pendingWrites.Count == 0)
                return;

            if (SpawnedPlayers.Count == 0 || !Spawned)
            {
                _pendingDirtyBits = 0;
                _pendingWrites.Clear();
                return;
            }

            using NetworkWriterPooled payloadWriter = NetworkWriterPool.Get();

            int safetyPos = payloadWriter.Position;
            payloadWriter.WriteByte(0);
            int dataStart = payloadWriter.Position;

            payloadWriter.WriteULong(_pendingDirtyBits);

            foreach (Action<NetworkWriter> write in _pendingWrites.Values)
            {
                write(payloadWriter);
            }

            int dataEnd = payloadWriter.Position;
            payloadWriter.Position = safetyPos;
            payloadWriter.WriteByte((byte)((dataEnd - dataStart) & 0xFF));
            payloadWriter.Position = dataEnd;

            EntityStateMessage msg = new()
            {
                netId = NetId,
                payload = payloadWriter.ToArraySegment()
            };

            foreach (Player player in SpawnedPlayers)
            {
                if (player.IsHost)
                    continue;

                player.Connection.Send(msg);
            }

            _pendingDirtyBits = 0;
            _pendingWrites.Clear();
        }

        /// <summary>
        /// Spawns a <see cref="GameObject"/> to the specified <see cref="Player"/>.
        /// </summary>
        /// <param name="obj">The <see cref="GameObject"/> to be spawned.</param>
        /// <param name="player">The <see cref="Player"/> to be spawned for.</param>
        /// <returns><see langword="true"/> if it is successfully spawned otherwise returns <see langword="false"/></returns>
        public static bool SpawnForPlayer(GameObject obj, Player player)
        {
            if (obj == null)
            {
                LogManager.Error("object is null.");
                return false;
            }

            if (!obj.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
            {
                LogManager.Error($"{obj.name} has no NetworkIdentity component.");
                return false;
            }

            if (NetworkServer.spawned.ContainsKey(identity.netId))
            {
                LogManager.Warn($"netId {identity.netId} is already in the spawned dictionary.");
                return false;
            }
            
            identity.isLocalPlayer = false;
            identity.isClient = true;
            identity.isServer = false;
            identity.netId = NetworkIdentity.GetNextNetworkId();
            NetworkServer.spawned[identity.netId] = identity;
            LogManager.Info($"SpawnForConnection: Registered {obj.name} with netId={identity.netId}.");
            identity.OnStartServer();
            SendCustomSpawnMessage(identity, player);

            return true;
        }

        private static void SendCustomSpawnMessage(NetworkIdentity identity, Player player)
        {
            if (!player.Connection.isReady)
            {
                LogManager.Warn($"Player is not ready. Message not sent.");
                return;
            }

            using NetworkWriterPooled ownerWriter = NetworkWriterPool.Get();
            using NetworkWriterPooled observersWriter = NetworkWriterPool.Get();
            ArraySegment<byte> payload = BuildSpawnPayload(identity, ownerWriter, observersWriter);
            SpawnMessage message = new()
            {
                netId = identity.netId,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = 0,
                assetId = identity.assetId,
                position = identity.transform.localPosition,
                rotation = identity.transform.localRotation,
                scale = identity.transform.localScale,
                payload = payload
            };

            player.Connection.Send(message);
            LogManager.Debug($"Sent SpawnMessage for {identity.name} (netId={identity.netId}) to player {player.DisplayName}.");
        }

        private static ArraySegment<byte> BuildSpawnPayload(NetworkIdentity identity, NetworkWriterPooled ownerWriter, NetworkWriterPooled observersWriter)
        {
            if (identity.NetworkBehaviours.Length == 0)
                return default;

            identity.SerializeServer(initialState: true, ownerWriter, observersWriter);
            return observersWriter.ToArraySegment();
        }
    }
}