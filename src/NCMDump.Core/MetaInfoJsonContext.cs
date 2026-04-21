using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCMDump.Core
{
    [JsonSerializable(typeof(MetaInfo))]
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    internal partial class MetaInfoJsonContext : JsonSerializerContext
    {
    }
}
