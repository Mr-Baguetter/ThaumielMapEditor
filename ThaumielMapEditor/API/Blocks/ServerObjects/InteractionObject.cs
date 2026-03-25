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

        [YamlIgnore]
        public InvisibleInteractableToy Base { get; private set; }

        public override ObjectType ObjectType { get; set; } = ObjectType.Interactable;

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

        public bool CanSearch => Base.CanSearch;

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

        public void ParseValues(SerializableObject serializable)
        {
            if (serializable.ObjectType is not ObjectType.Interactable)
            {
                LogManager.Warn($"Tried to parse {serializable.ObjectType} as Interactable.");                
                return;
            }

            if (!serializable.Values.TryConvertValue<ColliderShape>("Shape", out var shape))
            {
                LogManager.Warn("Failed to parse Shape");
                return;
            }
            if (!serializable.Values.TryConvertValue<float>("Duration", out var duration))
            {
                LogManager.Warn("Failed to parse Duration");
                return;
            }
            if (!serializable.Values.TryConvertValue<bool>("Locked", out var locked))
            {
                LogManager.Warn("Failed to parse Locked");
                return;
            }

            Shape = shape;
            InteractionDuration = duration;
            IsLocked = locked;
        }

        private void HandleInteracted(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnInteracted?.Invoke(this, player);
        }

        private void HandleSearching(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnSearching?.Invoke(this, player);
        }

        private void HandleSearched(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnSearched?.Invoke(this, player);
        }

        private void HandleSearchAborted(ReferenceHub hub)
        {
            if (!hub.TryGet(out var player))
                return;

            OnSearchAborted?.Invoke(this, player);
        }
    }
}