using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class CullingTrigger : TriggerHandler
    {
        private SchematicData? Schematic;

        /// <summary>
        /// Initializes the <see cref="CullingTrigger"/> component
        /// </summary>
        /// <param name="schematic">The schematic tied to this <see cref="CullingTrigger"/> instance.</param>
        public void Init(SchematicData schematic)
        {
            OnPlayerEntered += OnTriggerEnter;
            OnPlayerExited += OnTriggerExit;

            Schematic = schematic;
        }

        private void OnDestroy()
        {
            OnPlayerEntered -= OnTriggerEnter;
            OnPlayerExited -= OnTriggerExit;
        }

        private void OnTriggerEnter(Player player, Collider other)
        {
            if (Schematic == null || player.IsDestroyed || !player.IsAlive)
                return;

            foreach (PrimitiveObject primitive in Schematic.Primitives)
                primitive.HideForPlayer(player);
        }

        private void OnTriggerExit(Player player, Collider other)
        {
            if (Schematic == null || player.IsDestroyed || !player.IsAlive)
                return;

            foreach (PrimitiveObject primitive in Schematic.Primitives)
                primitive.ShowForPlayer(player);
        }
    }
}