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
        public static string Dir() => Path.Combine(PathManager.Configs.ToString(), "Thaumiel");
        public static string Dir(string[] path) => Path.Combine([Dir(), .. path]);

        public static void TryCreateDirectory(string name) => Directory.CreateDirectory(Dir([name]));
        public static void TryCreateDirectory(string[] path) => Directory.CreateDirectory(Dir(path));

        public static string[] GetFilesInDirectory(string name, string filter = "*") =>
            Directory.GetFiles(Dir([name]), filter);

        public static void ParseYamlFiles<T>(string name, Action<T> onParsed)
        {
            TryCreateDirectory(name);
            IDeserializer deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
            int count = 0;

            foreach (string file in GetFilesInDirectory(name, "*.yml"))
            {
                try
                {
                    string yaml = File.ReadAllText(file);
                    T result = deserializer.Deserialize<T>(yaml);
                    onParsed(result);
                    count++;
                    LogManager.Debug($"Loaded {typeof(T).Name} from {Path.GetFileName(file)}");
                }
                catch (Exception ex)
                {
                    LogManager.Error($"Failed to parse {Path.GetFileName(file)} as {typeof(T).Name}: {ex.Message}");
                }
            }

            LogManager.Info($"Loaded {count} {typeof(T).Name}(s) from {name}");
        }

        public static void CreateDefaultFile<T>(string name, string fileName) where T : new()
        {
            if (Directory.Exists(Dir([name])))
                return;

            TryCreateDirectory(name);
            ISerializer serializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
            File.WriteAllText(Path.Combine(PathManager.Configs.ToString(), "Thaumiel", name, fileName), serializer.Serialize(new T()));
        }

        public static void CreateDefaultFiles<T>(string name, List<(string fileName, T item)> items) where T : new()
        {
            if (Directory.Exists(Dir([name])))
                return;

            TryCreateDirectory(name);
            ISerializer serializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();

            foreach ((string fileName, T item) in items)
                File.WriteAllText(Path.Combine(PathManager.Configs.ToString(), "Thaumiel", name, fileName), serializer.Serialize(item));
        }
    }
}