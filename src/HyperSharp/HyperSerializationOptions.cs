using System.Text.Json;

namespace OoLunar.HyperSharp
{
    public static class HyperJsonSerializationOptions
    {
        public static JsonSerializerOptions Default { get; }

        static HyperJsonSerializationOptions() => Default = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }
}
