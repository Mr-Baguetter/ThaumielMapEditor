using System;
using System.Collections.Generic;
using System.IO;
using LabApi.Loader.Features.Paths;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    }
}