using System;
using System.Collections.Generic;
using MapGeneration;

namespace ThaumielMapEditor.API.Serialization
{
    public class MapSchematic
    {
        /// <summary>
        /// Gets or sets the position for the <see cref="MapSchematic"/> instance.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the schematic name of the parent schematic for the <see cref="MapSchematic"/> instance.
        /// </summary>
        public string SchematicName { get; set; } = string.Empty;
    }

    public class SerializableMap
    {
        /// <summary>
        /// Gets or sets the file name for the <see cref="SerializableMap"/> instance.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Guid id for the <see cref="SerializableMap"/> instance.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the room by its name for the <see cref="SerializableMap"/> instance.
        /// </summary>
        public RoomName Room { get; set; }

        /// <summary>
        /// Gets or sets the local position of the room for the <see cref="SerializableMap"/> instance.
        /// </summary>
        public Vector3 LocalPosition { get; set; }

        /// <summary>
        /// Gets or sets the schematics for the <see cref="SerializableMap"/> instance.
        /// </summary>
        public List<MapSchematic> Schematics = new();
    }
}