using System.Text.Json;
using System.Text.Json.Serialization;

namespace Metriflow.Correlation.Worker;

public static class JsonSetting
{
    public static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null,
        WriteIndented = true,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
