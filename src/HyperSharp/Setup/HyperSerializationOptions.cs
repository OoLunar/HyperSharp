using System.Text.Json;

namespace HyperSharp.Setup
{
    /// <summary>
    /// Represents the JSON serialization options for HyperSharp.
    /// </summary>
    public static class HyperJsonSerializationOptions
    {
        /// <summary>
        /// The default JSON serialization options for HyperSharp.
        /// </summary>
        public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
    }
}
