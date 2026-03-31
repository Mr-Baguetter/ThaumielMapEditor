using System.Collections.Generic;
using UnityEngine;

namespace ThaumielMapEditor.API.Serialization
{
    public class SerializableLOD
    {
        public Vector3 Bounds { get; set; }
        public List<PrimitiveType> Primitives { get; set; } = [];
    }
}