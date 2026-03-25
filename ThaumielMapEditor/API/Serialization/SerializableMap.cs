using System;
using System.Collections.Generic;
using MapGeneration;

namespace ThaumielMapEditor.API.Serialization
{
    public class MapSchematic
    {
        public Vector3 Position { get; set; }
        public string SchematicName { get; set; } = string.Empty;
    }
    
    public class SerializableMap
    {
        public string FileName { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public RoomName Room { get; set; }
        public Vector3 LocalPosition { get; set; }
        public List<MapSchematic> Schematics = new();
    }
}