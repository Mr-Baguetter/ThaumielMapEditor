using System.Collections.Generic;
using UnityEngine;

namespace ThaumielMapEditor.API.Data
{
    public class LODData
    {
        public uint Index { get; set; }
        public Vector3 Bounds { get; set; }
        public List<PrimitiveType> Primitives { get; set; } = [];
    }
}