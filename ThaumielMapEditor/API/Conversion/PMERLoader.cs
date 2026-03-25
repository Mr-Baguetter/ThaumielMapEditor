using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThaumielMapEditor.API.Conversion
{
    public static class PMERLoader
    {
        public static PMERRoot Load(string path)
        {
            string json = File.ReadAllText(path);
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new ObjectDictionaryConverter());
            options.Converters.Add(new Vector3Converter());

            return JsonSerializer.Deserialize<PMERRoot>(json, options);
        }
    }
}