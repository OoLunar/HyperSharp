using System.Text.Json;

namespace OoLunar.HyperSharp.Setup
{
    public static class HyperJsonSerializationOptions
    {
        public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
    }
}
