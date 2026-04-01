using System.Collections.Generic;
using LabApi.Features.Wrappers;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using UnityEngine;

namespace ThaumielMapEditor.API.Components
{
    public class LODZone : TriggerHandler
    {
        public List<PrimitiveType> PrimitivestoUnload = [];
        public SchematicData Schematic;
        public uint Index;
        public BoxCollider Collider;

        public void Init(SchematicData schematic, List<PrimitiveType> unload, uint index, BoxCollider collider)
        {
            OnPlayerEntered += OnTriggerEnter;
            OnPlayerExited += OnTriggerExit;

            PrimitivestoUnload = unload;
            Schematic = schematic;
            Index = index;
            Collider = collider;
        }

        private void OnDestroy()
        {
            OnPlayerEntered -= OnTriggerEnter;
            OnPlayerExited -= OnTriggerExit;
        }

        private void OnTriggerEnter(Player player, Collider other)
        {
            foreach (PrimitiveObject prim in Schematic.Primitives)
            {
                if (!PrimitivestoUnload.Contains(prim.PrimitiveType))
                    continue;

                prim.SpawnForPlayer(player);
            }
        }

        private void OnTriggerExit(Player player, Collider other)
        {
            foreach (PrimitiveObject prim in Schematic.Primitives)
            {
                if (!PrimitivestoUnload.Contains(prim.PrimitiveType))
                    continue;

                prim.DespawnForPlayer(player);
            }
        }
    }
}