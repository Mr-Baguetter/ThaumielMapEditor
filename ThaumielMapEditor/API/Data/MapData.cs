using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;

namespace ThaumielMapEditor.API.Data
{
    public class MapData
    {
        public string FileName = string.Empty;
        public Guid Id;

        public Vector3 Position { get; set; }
        public Room? Room { get; set; }
        public List<(Vector3, string)> Schematics { get; set; } = [];
    }
}