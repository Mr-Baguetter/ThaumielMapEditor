using System.Text.Json;

namespace ThaumielMapEditor.API.Conversion
{
    public static class JsonElementExtensions
    {
        public static object ToObject(this JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out int i) ? i : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Object => element.ToString(),
                JsonValueKind.Array => element.ToString(),
                _ => null
            };
        }
    }
}