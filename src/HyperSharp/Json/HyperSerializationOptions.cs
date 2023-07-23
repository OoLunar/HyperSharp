using System.Text.Json;

namespace OoLunar.HyperSharp.Json
{
    public static class HyperSerializationOptions
    {
        public static JsonSerializerOptions Default { get; }

        static HyperSerializationOptions()
        {
            Default = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            Default.Converters.Add(new IErrorJsonConverterFactory());
        }
    }
}
