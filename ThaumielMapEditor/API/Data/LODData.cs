using System.Collections.Generic;
using UnityEngine;

namespace ThaumielMapEditor.API.Data
{
    public class LODData
    {
        /// <summary>
        /// Gets the index of this <see cref="LODData"/> within its schematic.
        /// </summary>
        public uint Index { get; internal set; }

        /// <summary>
        /// Gets the bounds of the <see cref="LODData"/> zone.
        /// </summary>
        public Vector3 Bounds { get; internal set; }

        /// <summary>
        /// Gets the list of <see cref="PrimitiveType"/>s that will be spawned and despawned as players enter and exit this zone.
        /// </summary>
        public List<PrimitiveType> Primitives { get; internal set; } = [];
    }
}