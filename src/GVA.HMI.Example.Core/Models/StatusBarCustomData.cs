using System.Text.Json;
using System.Text.Json.Serialization;

namespace HMICore.Models;

public sealed class StatusBarCustomRoot
{
    [JsonPropertyName("status_bar")]
    public StatusBarCustomSection? StatusBar { get; set; }
}

public sealed class StatusBarCustomSection
{
    [JsonConverter(typeof(CustomFieldLayoutListConverter))]
    public List<CustomFieldLayout> ItemWithLayout { get; set; } = new();

    [JsonConverter(typeof(CustomFieldOptionMapConverter))]
    public Dictionary<string, List<CustomFieldOption>> ItemWithValue { get; set; } = new();
}

public sealed class CustomFieldLayout
{
    public string Key { get; init; } = string.Empty;
    public string KorName { get; init; } = string.Empty;
    public string EngName { get; init; } = string.Empty;
    public double Ratio { get; init; }
}

public sealed class CustomFieldOption
{
    public string Value { get; init; } = string.Empty;
    public string KorValue { get; init; } = string.Empty;
    public string EngValue { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}

internal sealed class CustomFieldLayoutListConverter : JsonConverter<List<CustomFieldLayout>>
{
    public override List<CustomFieldLayout> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new List<CustomFieldLayout>();
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("itemWithLayout 은 배열이어야 한다.");
        }

        while (reader.Read() && reader.TokenType == JsonTokenType.StartArray)
        {
            var row = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            result.Add(new CustomFieldLayout
            {
                Key = row[0].GetString() ?? string.Empty,
                KorName = row[1].GetString() ?? string.Empty,
                EngName = row[2].GetString() ?? string.Empty,
                Ratio = row[3].GetDouble(),
            });
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<CustomFieldLayout> value, JsonSerializerOptions options)
        => throw new NotSupportedException("읽기 전용 - 이 앱은 이 JSON 을 쓰지 않는다.");
}

internal sealed class CustomFieldOptionMapConverter : JsonConverter<Dictionary<string, List<CustomFieldOption>>>
{
    public override Dictionary<string, List<CustomFieldOption>> Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new Dictionary<string, List<CustomFieldOption>>();
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("itemWithValue 는 객체여야 한다.");
        }

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var key = reader.GetString() ?? string.Empty;
            reader.Read();

            var list = new List<CustomFieldOption>();
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"itemWithValue.{key} 는 배열이어야 한다.");
            }

            while (reader.Read() && reader.TokenType == JsonTokenType.StartArray)
            {
                var row = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                list.Add(new CustomFieldOption
                {
                    Value = row[0].GetString() ?? string.Empty,
                    KorValue = row[1].GetString() ?? string.Empty,
                    EngValue = row[2].GetString() ?? string.Empty,
                    Color = row[3].GetString() ?? string.Empty,
                });
            }

            result[key] = list;
        }

        return result;
    }

    public override void Write(
        Utf8JsonWriter writer, Dictionary<string, List<CustomFieldOption>> value, JsonSerializerOptions options)
        => throw new NotSupportedException("읽기 전용 - 이 앱은 이 JSON 을 쓰지 않는다.");
}
