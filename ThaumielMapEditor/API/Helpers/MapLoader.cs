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
        /// <item><description><c>LoadIf::MapName::IsLoaded::ConditionMap</c> - Loads a map if the condition map is currently loaded.</description></item>
        /// <item><description><c>LoadIf::MapName::IsNotLoaded::ConditionMap</c> - Loads a map if the condition map is not currently loaded.</description></item>
        /// <item><description><c>UnloadIf::MapName::IsLoaded::ConditionMap</c> - Unloads a map if the condition map is currently loaded.</description></item>
        /// <item><description><c>UnloadIf::MapName::IsNotLoaded::ConditionMap</c> - Unloads a map if the condition map is not currently loaded.</description></item>
        /// </list>
        /// </remarks>
        public static void ParseInput(string input)
        {
            if (input.Contains("LoadIf::"))
            {
                string[] parts = input.Replace("LoadIf::", "").Split(["::"], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 3)
                {
                    LogManager.Warn($"Invalid LoadIf syntax: '{input}'. Expected: LoadIf::MapName::IsLoaded/IsNotLoaded::ConditionMap");
                    return;
                }

                string mapName = parts[0].Trim();
                string condition = parts[1].Trim();
                string conditionMap = parts[2].Trim();

                if (EvaluateCondition(condition, conditionMap))
                    LoadMap(mapName);
            }

            if (input.Contains("UnloadIf::"))
            {
                string[] parts = input.Replace("UnloadIf::", "").Split(["::"], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 3)
                {
                    LogManager.Warn($"Invalid UnloadIf syntax: '{input}'. Expected: UnloadIf::MapName::IsLoaded/IsNotLoaded::ConditionMap");
                    return;
                }

                string mapName = parts[0].Trim();
                string condition = parts[1].Trim();
                string conditionMap = parts[2].Trim();

                if (EvaluateCondition(condition, conditionMap))
                    UnloadMap(mapName);
            }

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
            {
                LogManager.Warn($"Map name {name} is invalid!");
                return;
            }

            SchematicLoader.SpawnMap(map);
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
            {
                LogManager.Warn($"Map name {name} is invalid!");
                return;
            }

            SchematicLoader.DestroyMap(map);
        }

        /// <summary>
        /// Determines if the specified map by name is loaded.
        /// </summary>
        /// <param name="name">The map name to check.</param>
        /// <returns><see langword="true"/> if the specified map is loaded. Otherwise <see langword="false"/>.</returns>
        private static bool IsMapLoaded(string name)
            => SchematicLoader.SpawnedMaps.Any(s => string.Equals(s.FileName, name, StringComparison.CurrentCultureIgnoreCase));

        private static bool EvaluateCondition(string condition, string mapName)
        {
            return condition.ToLowerInvariant() switch
            {
                "isloaded" => IsMapLoaded(mapName),
                "isnotloaded" => !IsMapLoaded(mapName),
                _ => LogUnknownCondition(condition)
            };
        }

        private static bool LogUnknownCondition(string condition)
        {
            LogManager.Warn($"Unknown condition '{condition}'. Supported conditions: IsLoaded, IsNotLoaded.");
            return false;
        }
    }
}