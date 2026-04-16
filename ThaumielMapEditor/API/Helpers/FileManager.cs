// -----------------------------------------------------------------------
// <copyright file="FileManager.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using LabApi.Loader.Features.Paths;

namespace ThaumielMapEditor.API.Helpers
{
    public class FileManager
    {
        /// <summary>
        /// Gets the Thaumiel directory
        /// </summary>
        /// <returns>Directory path to the Thaumiel directory</returns>
        public static string Dir() => Path.Combine(PathManager.Configs.ToString(), "Thaumiel");

        /// <summary>
        /// Gets the Thaumiel directory plus the inputed directories
        /// </summary>
        /// <param name="path">The path of directories</param>
        /// <returns>Directory path to the Thaumiel directory plus the inputed directories</returns>
        public static string Dir(string[] path) => Path.Combine([Dir(), .. path]);

        /// <summary>
        /// Tries to create a directory with the inputted name
        /// </summary>
        /// <param name="name">The name of the directory</param>
        public static void TryCreateDirectory(string name) => Directory.CreateDirectory(Dir([name]));

        /// <summary>
        /// Tries to create a directory at the path
        /// </summary>
        /// <param name="path">The directory path to make</param>
        public static void TryCreateDirectory(string[] path) => Directory.CreateDirectory(Dir(path));

        /// <summary>
        /// Gets all the file paths in the Thaumiel directory combined with the specified directory path.
        /// </summary>
        /// <param name="name">The directory path relative to the Thaumiel directory.</param>
        /// <param name="filter">The search pattern to filter files by. Defaults to <c>*</c> which returns all files.</param>
        /// <returns>An array of file paths matching the <paramref name="filter"/> in the resolved directory.</returns>
        public static string[] GetFilesInDirectory(string name, string filter = "*") =>
            Directory.GetFiles(Dir([name]), filter);

        /// <summary>
        /// Reads the specified file at the file path in the background.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onComplete">Fired when the reading is complete with the read text</param>
        public static void ReadFileInBackground(string path, Action<string> onComplete)
        {
            if (!File.Exists(path))
                return;

            Task.Run(() =>
            {
                try
                {
                    string content = File.ReadAllText(path);
                    onComplete?.Invoke(content);
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Exception reading file: {ex}");
                }
            });
        }
    }
}