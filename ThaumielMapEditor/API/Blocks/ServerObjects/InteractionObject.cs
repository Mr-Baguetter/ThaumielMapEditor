// -----------------------------------------------------------------------
// <copyright file="InteractionObject.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using AdminToys;
using Mirror;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Enums;
using ThaumielMapEditor.API.Extensions;
using ThaumielMapEditor.API.Helpers;
using ThaumielMapEditor.API.Serialization;
using static AdminToys.InvisibleInteractableToy;
using System;
using LabApi.Features.Wrappers;
using static ThaumielMapEditor.API.Extensions.PlayerExtensions;
using YamlDotNet.Serialization;
using Interactables.Interobjects.DoorUtils;
using System.Linq;
using ThaumielMapEditor.Events.EventArgs.Handlers;

namespace ThaumielMapEditor.API.Blocks.ServerObjects
{
    public class InteractionObject : ServerObject
    {
        /// <summary>
        /// Fired when a player interacts with an <see cref="InteractionObject"/> that has no interaction duration.
        /// </summary>
        /// <remarks>
        /// This event is only triggered when <see cref="InteractionDuration"/> is 0.
        /// For held interactions, see <see cref="OnSearched"/>.
        /// </remarks>
        public static event Action<InteractionObject, Player>? OnInteracted;

        /// <summary>
        /// Fired when a player begins an interaction on an <see cref="InteractionObject"/>.
        /// </summary>
        /// <remarks>
        /// This event fires at the start of the search/pickup interaction. If the interaction is cancelled,
        /// <see cref="OnSearchAborted"/> will be fired instead of <see cref="OnSearched"/>.
        /// </remarks>
        public static event Action<InteractionObject, Player>? OnSearching;

        /// <summary>
        /// Fired when a player successfully completes a interaction on an <see cref="InteractionObject"/>.
        /// </summary>
        /// <remarks>
        /// This event only fires if the player holds the interaction for the full <see cref="InteractionDuration"/>
        /// without moving out of range or cancelling.
        /// </remarks>
        public static event Action<InteractionObject, Player>? OnSearched;

        /// <summary>
        /// Fired when a player cancels or fails a interaction on an <see cref="InteractionObject"/>.
        /// </summary>
        /// <remarks>
        /// This event fires if the interaction is interrupted after <see cref="OnSearching"/> has already been triggered,
        /// such as when the player moves out of range or manually cancels.
        /// </remarks>
        public static event Action<InteractionObject, Player>? OnSearchAborted;

        /// <summary>
        /// The runtime instance of the underlying <see cref="InvisibleInteractableToy"/>.
        /// </summary>
        [YamlIgnore]
#pragma warning disable CS8618
        public InvisibleInteractableToy Base { get; private set; }
#pragma warning restore CS8618

        /// <summary>
        /// The type of this server object. Always <see cref="ObjectType.Interactable"/>.
        /// </summary>
        public override ObjectType ObjectType { get; set; } = ObjectType.Interactable;

        /// <summary>
        /// The collider shape used by the interactable toy.
        /// </summary>
        /// <remarks>
        /// Setting this property updates the underlying <see cref="InvisibleInteractableToy.Shape"/>.
        /// </remarks>
        public ColliderShape Shape
        {
            get;

            set
            {
                if (field == value)
                    return;

                field = value;
                Base.Shape = value;
            }
        }

        /// <summary>
        /// How long (in seconds) a player must hold the interaction for held interactions.
        /// </summary>
        /// <remarks>
        /// Setting this property updates the underlying <see cref="InvisibleInteractableToy.InteractionDuration"/>.
        /// A value of 0 represents an instant interaction and will trigger <see cref="OnInteracted"/>.
        /// </remarks>
        public float InteractionDuration
        {
            get;

            set
            {
                if (field == value)
                    return;

                field = value;
                Base.InteractionDuration = value;
            }
        }

        /// <summary>
        /// Whether the interactable is locked and cannot be searched by players.
        /// </summary>
        /// <remarks>
        /// Setting this property updates the underlying <see cref="InvisibleInteractableToy.IsLocked"/>.
        /// </remarks>
        public bool IsLocked
        {
            get;

            set
            {
                if (field == value)
                    return;

                field = value;
                Base.IsLocked = value;
            }
        }

        /// <summary>
        /// The permissions required to interact with this <see cref="InteractionObject"/> instance.
        /// </summary>
        public DoorPermissionFlags Permissions
        {
            get;

            set
            {
                if (field == value)
                    return;

                field = value;
            }
        }

        /// <summary>
        /// Returns true if the underlying interactable toy can currently be searched by players.
        /// </summary>
        public bool CanSearch => Base.CanSearch;
    
        /// <inheritdoc/>
        public override void SpawnObject(SchematicData schematic, SerializableObject serializable)
        {
            if (PrefabHelper.Interactable == null)
            {
                LogManager.Warn($"Failed to spawn Interact Object. Prefab is null.");
                return;
            }

            InvisibleInteractableToy toy = UnityEngine.Object.Instantiate(PrefabHelper.Interactable);
            NetworkServer.UnSpawn(toy.gameObject);
            Base = toy;
            Object = toy.gameObject;
            NetId = toy.netId;
            ParseValues(serializable);
            SetWorldTransform(schematic);
            Base.Shape = Shape;
            Base.InteractionDuration = InteractionDuration;
            Base.IsLocked = IsLocked;
            NetworkServer.Spawn(toy.gameObject);

            toy.OnInteracted += HandleInteracted;
            toy.OnSearching += HandleSearching;
            toy.OnSearched += HandleSearched;
            toy.OnSearchAborted += HandleSearchAborted;
            base.SpawnObject(schematic, serializable);
        }

        public void SpawnObject(SchematicData schematic)
        {
            if (PrefabHelper.Interactable == null)
            {
                LogManager.Warn($"Failed to spawn Interact Object. Prefab is null.");
                return;
            }

            InvisibleInteractableToy toy = UnityEngine.Object.Instantiate(PrefabHelper.Interactable);
            NetworkServer.UnSpawn(toy.gameObject);
            Base = toy;
            Object = toy.gameObject;
            NetId = toy.netId;
            SetWorldTransform(schematic);
            Base.Shape = Shape;
            Base.InteractionDuration = InteractionDuration;
            Base.IsLocked = IsLocked;
            NetworkServer.Spawn(toy.gameObject);

            toy.OnInteracted += HandleInteracted;
            toy.OnSearching += HandleSearching;
            toy.OnSearched += HandleSearched;
            toy.OnSearchAborted += HandleSearchAborted;
            schematic.SpawnedServerObjects.Add(this);
            ObjectHandler.OnServerObjectSpawned(new(this));
            SpawnedObjects.Add(this);
        }


        /// <summary>
        /// Parses configuration values from a <see cref="SerializableObject"/> into this instance.
        /// </summary>
        /// <param name="serializable">Serializable object expected to have ObjectType = <see cref="ObjectType.Interactable"/> and values for "Shape", "Duration", and "Locked".</param>
        /// <remarks>
        /// If the <see cref="SerializableObject.ObjectType"/> is not <see cref="ObjectType.Interactable"/>, the method logs a warning.
        /// Each individual value is parsed via <see cref="DictionaryExtensions.TryConvertValue{T}"/> and will log a warning and abort
        /// on failure to parse any expected value.
        /// </remarks>
        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType != ObjectType.Interactable)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Interactable.");                
                return;
            }

            if (!serializable.Values.TryConvertValue<ColliderShape>("Shape", out var shape))
            {
                LogManager.Warn("Failed to parse Shape");
            }

            if (!serializable.Values.TryConvertValue<float>("Duration", out var duration))
            {
                LogManager.Warn("Failed to parse Duration");
            }

            if (!serializable.Values.TryConvertValue<bool>("Locked", out var locked))
            {
                LogManager.Warn("Failed to parse Locked");
            }

            if (!serializable.Values.TryConvertValue<DoorPermissionFlags>("Permissions", out var perms))
            {
                LogManager.Warn("Failed to parse Permissions");
            }

            Shape = shape;
            InteractionDuration = duration;
            IsLocked = locked;
            Permissions = perms;
        }

        /// <summary>
        /// Internal handler that translates the toy's <see cref="InvisibleInteractableToy.OnInteracted"/> callback
        /// into the static <see cref="OnInteracted"/> event after resolving the <see cref="ReferenceHub"/> to a <see cref="Player"/>.
        /// </summary>
        /// <param name="hub">ReferenceHub provided by the toy callback.</param>
        private void HandleInteracted(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnInteracted?.Invoke(this, player);
        }

        /// <summary>
        /// Internal handler that translates the toy's <see cref="InvisibleInteractableToy.OnSearching"/> callback
        /// into the static <see cref="OnSearching"/> event after resolving the <see cref="ReferenceHub"/> to a <see cref="Player"/>.
        /// </summary>
        /// <param name="hub">ReferenceHub provided by the toy callback.</param>
        private void HandleSearching(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnSearching?.Invoke(this, player);
        }

        /// <summary>
        /// Internal handler that translates the toy's <see cref="InvisibleInteractableToy.OnSearched"/> callback
        /// into the static <see cref="OnSearched"/> event after resolving the <see cref="ReferenceHub"/> to a <see cref="Player"/>.
        /// </summary>
        /// <param name="hub">ReferenceHub provided by the toy callback.</param>
        private void HandleSearched(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnSearched?.Invoke(this, player);
        }

        /// <summary>
        /// Internal handler that translates the toy's <see cref="InvisibleInteractableToy.OnSearchAborted"/> callback
        /// into the static <see cref="OnSearchAborted"/> event after resolving the <see cref="ReferenceHub"/> to a <see cref="Player"/>.
        /// </summary>
        /// <param name="hub">ReferenceHub provided by the toy callback.</param>
        private void HandleSearchAborted(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnSearchAborted?.Invoke(this, player);
        }

        /// <summary>
        /// Tries to get a <see cref="InteractionObject"/> from a <see cref="InvisibleInteractableToy"/>
        /// </summary>
        /// <param name="toy">The <see cref="InvisibleInteractableToy"/> to check for <see cref="InteractionObject"/></param>
        /// <param name="interactionobj">The <see cref="InteractionObject"/> if found.</param>
        /// <returns><see langword="true"/> if found else returns <see langword="false"/> if not found</returns>
        public static bool TryGetInteractionObject(InvisibleInteractableToy toy, out InteractionObject interactionobj)
        {
            foreach (InteractionObject interaction in SpawnedObjects.Where(obj => obj is InteractionObject).Cast<InteractionObject>())
            {
                if (interaction.NetId != toy.netId)
                    continue;

                interactionobj = interaction;
                return true;
            }

            interactionobj = null!;
            return false;
        }
    }
}