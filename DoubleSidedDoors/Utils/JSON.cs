using System.Text.Json;
using System.Text.Json.Serialization;

namespace DoubleSidedDoors.Utils;

internal static class JSON
{
    private static readonly JsonSerializerOptions _setting = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        IncludeFields = false,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        IgnoreReadOnlyProperties = true
    };

    static JSON()
    {
        _setting.Converters.Add(new LocaleTextConverter());
        _setting.Converters.Add(new JsonStringEnumConverter());

        if (PartialDataUtil.HasPData)
        {
            _setting.Converters.Add(PartialDataUtil.PersistentIDConverter!);
            DSDLogger.Log("PartialData support found! You are based.");
        }
    }

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _setting)!;
    }

    public static object Deserialize(Type type, string json)
    {
        return JsonSerializer.Deserialize(json, type, _setting)!;
    }

    public static string Serialize(object value, Type type)
    {
        return JsonSerializer.Serialize(value, type, _setting);
    }
}
