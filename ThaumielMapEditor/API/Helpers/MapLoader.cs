// -----------------------------------------------------------------------
// <copyright file="MapLoader.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Serialization;
using Random = UnityEngine.Random;

namespace ThaumielMapEditor.API.Helpers
{
    public class MapLoader
    {
        /// <summary>
        /// Parses an input string and executes map load or unload operations based on the provided syntax.
        /// </summary>
        /// <param name="input">The input command string (e.g., "Load::MapA", "Unload::MapA||MapB").</param>
        /// <remarks>
        /// Supported syntax:
        /// <list type="bullet">
        /// <item><description><c>Load::MapName</c> - Loads a single map.</description></item>
        /// <item><description><c>Load::MapA||MapB</c> - Loads one random map from the list.</description></item>
        /// <item><description><c>Load::MapA&amp;&amp;MapB</c> - Loads all specified maps.</description></item>
        /// <item><description><c>Unload::MapName</c> - Unloads a single map.</description></item>
        /// <item><description><c>Unload::MapA||MapB</c> - Unloads one random map from the list.</description></item>
        /// <item><description><c>Unload::MapA&amp;&amp;MapB</c> - Unloads all specified maps.</description></item>
        /// </list>
        /// </remarks>
        public static void ParseInput(string input)
        {
            if (input.Contains("Load::"))
            {
                string mapPart = input.Replace("Load::", "").Trim();

                if (mapPart.Contains("||"))
                {
                    string[] orMaps = mapPart.Split(["||"], StringSplitOptions.RemoveEmptyEntries);
                    string selectedMap = orMaps[Random.Range(0, orMaps.Length)].Trim();
                    LoadMap(selectedMap);
                }
                else if (mapPart.Contains("&&"))
                {
                    foreach (string map in mapPart.Split(["&&"], StringSplitOptions.RemoveEmptyEntries))
                    {
                        LoadMap(map.Trim());
                    }
                }
                else
                    LoadMap(mapPart);
            }

            if (input.Contains("Unload::"))
            {
                string mapPart = input.Replace("Unload::", "").Trim();

                if (mapPart.Contains("||"))
                {
                    string[] orMaps = mapPart.Split(["||"], StringSplitOptions.RemoveEmptyEntries);
                    string selectedMap = orMaps[Random.Range(0, orMaps.Length)].Trim();
                    UnloadMap(selectedMap);
                }
                else if (mapPart.Contains("&&"))
                {
                    foreach (string map in mapPart.Split(["&&"], StringSplitOptions.RemoveEmptyEntries))
                    {
                        UnloadMap(map.Trim());
                    }
                }
                else
                    UnloadMap(mapPart);
            }
        }

        /// <summary>
        /// Loads a map by its file name.
        /// </summary>
        /// <param name="name">The name of the map file to load.</param>
        /// <remarks>
        /// The method performs a case-insensitive search in the loaded maps collection.
        /// If the map is not found, a warning is logged.
        /// </remarks>
        public static void LoadMap(string name)
        {
            SerializableMap? map = SchematicLoader.LoadedMaps.FirstOrDefault(s => string.Equals(s.FileName, name, StringComparison.CurrentCultureIgnoreCase));
            if (map == null)
                LogManager.Warn($"Map name {name} is invalid!");

            SchematicLoader.SpawnMap(map!);
        }

        /// <summary>
        /// Unloads a currently spawned map by its file name.
        /// </summary>
        /// <param name="name">The name of the map file to unload.</param>
        /// <remarks>
        /// The method performs a case-insensitive search in the spawned maps collection.
        /// If the map is not found, a warning is logged.
        /// </remarks>
        public static void UnloadMap(string name)
        {
            MapData? map = SchematicLoader.SpawnedMaps.FirstOrDefault(s => string.Equals(s.FileName, name, StringComparison.CurrentCultureIgnoreCase));
            if (map == null)
                LogManager.Warn($"Map name {name} is invalid!");

            SchematicLoader.DestroyMap(map!);
        }
    }
}