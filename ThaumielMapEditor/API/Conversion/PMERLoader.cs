// -----------------------------------------------------------------------
// <copyright file="PMERLoader.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Text.Json;

namespace ThaumielMapEditor.API.Conversion
{
    public static class PMERLoader
    {
        /// <summary>
        /// Loads a PMER schematic from the <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The file path to load the schematic from.</param>
        /// <returns></returns>
        public static PMERRoot Load(string path)
        {
            string json = File.ReadAllText(path);
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new ObjectDictionaryConverter());
            options.Converters.Add(new Vector3Converter());

            return JsonSerializer.Deserialize<PMERRoot>(json, options) ?? new();
        }
    }
}