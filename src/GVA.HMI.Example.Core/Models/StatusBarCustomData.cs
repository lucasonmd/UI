using System.Text.Json;
using System.Text.Json.Serialization;

namespace HMICore.Models;

/// <summary>
/// StatusBar 커스텀 전시정보 JSON 스키마 - 기존 프로젝트가 이미 이 모양으로 주는
/// 데이터라 필드/배열 순서를 그대로 따랐다(우리가 새로 정한 스키마가 아니다).
///
/// <code>
/// {
///   "status_bar": {
///     "itemWithLayout": [ [Key, KorName, EngName, Ratio], ... ],
///     "itemWithValue": { "Key1": [ [Value, KorValue, EngValue, Color], ... ], ... }
///   }
/// }
/// </code>
///
/// itemWithLayout/itemWithValue 둘 다 "배열의 배열"(위치 기반 튜플)이라 일반적인
/// System.Text.Json 객체 매핑이 안 통한다 - 아래 JsonConverter 가 직접 읽는다.
/// </summary>
public sealed class StatusBarCustomRoot
{
    // JSON 키가 "status_bar"(스네이크 케이스)라 PropertyNameCaseInsensitive 만으로는
    // 안 맞는다(밑줄은 대소문자 무시로 안 없어진다) - 명시적으로 매핑해야 한다.
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

/// <summary>[Key, KorName, EngName, Ratio] 한 줄 - Ratio 는 커스텀 영역 안에서 이
/// 항목이 차지하는 비율(가로 Star 크기로 그대로 쓴다).</summary>
public sealed class CustomFieldLayout
{
    public string Key { get; init; } = string.Empty;
    public string KorName { get; init; } = string.Empty;
    public string EngName { get; init; } = string.Empty;
    public double Ratio { get; init; }
}

/// <summary>[Value, KorValue, EngValue, Color] 한 줄 - Value 는 실측값과 매칭하는
/// 코드, Color 는 "#AARRGGBB"/"#RRGGBB" 문자열.</summary>
public sealed class CustomFieldOption
{
    public string Value { get; init; } = string.Empty;
    public string KorValue { get; init; } = string.Empty;
    public string EngValue { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}

/// <summary>[[Key, KorName, EngName, Ratio], ...] 를 읽는다.</summary>
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

/// <summary>{ "Key1": [[Value, KorValue, EngValue, Color], ...], ... } 를 읽는다.</summary>
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
            reader.Read(); // 값(배열) 시작 위치로

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
