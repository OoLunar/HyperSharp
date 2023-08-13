using System.Text.Json;

namespace HyperSharp.Setup
{
    public static class HyperJsonSerializationOptions
    {
        public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
    }
}
