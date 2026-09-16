using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;
using HMICore.Models;

namespace HMICore.ViewModels;

/// <summary>
/// StatusBar 전체의 데이터 컨텍스트. 이 바깥에서 쓰이지 않는 데이터 단위
/// (<see cref="SystemDataModel"/> · <see cref="AlarmDataModel"/> ·
/// <see cref="CustomStatusItemViewModel"/>)는 한 파일에 같이 둔다.
/// 여러 화면이 공유하는 <see cref="ObservableObject"/> 와 JSON 스키마
/// (Models/StatusBarCustomData.cs)만 따로 남겼다.
/// </summary>
public sealed class StatusBarViewModel : ObservableObject
{
    private readonly DispatcherTimer _clock;
    private readonly List<CustomFieldLayout> _customLayout = new();
    private readonly Dictionary<string, List<CustomFieldOption>> _customValues = new();
    private readonly Dictionary<string, string> _currentValueCodes = new();
    private readonly Dictionary<string, CustomStatusItemViewModel> _customItemsByKey = new();

    private bool _isEnglish;

    public SystemDataModel SystemData { get; } = new();

    public AlarmDataModel AlarmData { get; } = new();

    public ObservableCollection<CustomStatusItemViewModel> CustomItems { get; } = new();

    public IReadOnlyDictionary<string, CustomStatusItemViewModel> CustomItemsByKey => _customItemsByKey;

    public StatusBarViewModel()
    {
        UpdateClock();

        _clock = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
    }

    /// <summary>
    /// 커스텀 전시정보 JSON 을 읽어 <see cref="CustomItems"/> 를 다시 채운다.
    /// 두 가지 형태를 모두 받는다 :
    /// <code>
    /// { "status_bar": { "itemWithLayout": [...], "itemWithValue": {...} } }  설정 파일을 통째로 넘길 때
    /// { "itemWithLayout": [...], "itemWithValue": {...} }                    JsonNode 로 그 부분만
    ///                                                                        떼서 ToString() 해 넘길 때
    /// </code>
    /// </summary>
    public void LoadCustomDisplayData(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        StatusBarCustomSection? section;
        using (var doc = JsonDocument.Parse(json))
        {
            var wrapped = doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("status_bar", out _);

            section = wrapped
                ? JsonSerializer.Deserialize<StatusBarCustomRoot>(json, options)?.StatusBar
                : JsonSerializer.Deserialize<StatusBarCustomSection>(json, options);
        }

        if (section is null)
        {
            return;
        }

        _customLayout.Clear();
        _customLayout.AddRange(section.ItemWithLayout);

        _customValues.Clear();
        _currentValueCodes.Clear();
        foreach (var (key, value) in section.ItemWithValue)
        {
            _customValues[key] = value;
            if (value.Count > 0)
            {
                _currentValueCodes[key] = value[0].Value;
            }
        }

        RebuildCustomItems();
    }

    public void UpdateCustomValue(string key, string value)
    {
        if (!_customValues.TryGetValue(key, out var candidates))
        {
            return;
        }

        var option = candidates.FirstOrDefault(o => o.Value == value);
        if (option is null)
        {
            return;
        }

        _currentValueCodes[key] = value;

        if (!_customItemsByKey.TryGetValue(key, out var item))
        {
            return;
        }

        item.Value = _isEnglish ? option.EngValue : option.KorValue;
        item.ValueBrush = ParseBrush(option.Color);
    }

    public void SetLanguage(bool isEnglish)
    {
        if (_isEnglish == isEnglish)
        {
            return;
        }

        _isEnglish = isEnglish;
        RebuildCustomItems();
    }

    private void RebuildCustomItems()
    {
        CustomItems.Clear();
        _customItemsByKey.Clear();

        foreach (var field in _customLayout)
        {
            _customValues.TryGetValue(field.Key, out var candidates);

            CustomFieldOption? current = null;
            if (candidates is not null && _currentValueCodes.TryGetValue(field.Key, out var code))
            {
                current = candidates.FirstOrDefault(o => o.Value == code);
            }
            current ??= candidates?.FirstOrDefault();

            var item = new CustomStatusItemViewModel
            {
                Key = field.Key,
                Ratio = field.Ratio > 0 ? field.Ratio : 1,
                Name = _isEnglish ? field.EngName : field.KorName,
                Value = (_isEnglish ? current?.EngValue : current?.KorValue) ?? string.Empty,
                ValueBrush = ParseBrush(current?.Color),
            };

            CustomItems.Add(item);
            _customItemsByKey[field.Key] = item;
        }
    }

    private static Brush ParseBrush(string? color)
    {
        if (!string.IsNullOrWhiteSpace(color))
        {
            try
            {
                if (ColorConverter.ConvertFromString(color) is Color parsed)
                {
                    return new SolidColorBrush(parsed);
                }
            }
            catch (FormatException)
            {
            }
        }

        return System.Windows.Application.Current.TryFindResource("ForegroundBrush") as Brush ?? Brushes.White;
    }

    private void UpdateClock() => SystemData.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}

/// <summary>StatusBar 윗줄의 기준 정보 - 시각 · 방위 · 좌표.</summary>
public sealed class SystemDataModel : ObservableObject
{
    private string _currentTime = string.Empty;
    private string _headingMil = string.Empty;
    private string _coordinate = string.Empty;

    public string CurrentTime
    {
        get => _currentTime;
        set => SetProperty(ref _currentTime, value);
    }

    public string HeadingMil
    {
        get => _headingMil;
        set => SetProperty(ref _headingMil, value);
    }

    public string Coordinate
    {
        get => _coordinate;
        set => SetProperty(ref _coordinate, value);
    }
}

/// <summary>StatusBar 아랫줄의 현재 알람과 등급별 집계.</summary>
public sealed class AlarmDataModel : ObservableObject
{
    private string _alarmTime = string.Empty;
    private string _alarmMessage = string.Empty;
    private int _alarmCount;
    private string _alarmLevel = string.Empty;
    private int _warningCount;
    private int _cautionCount;
    private int _ignoreCount;

    public string AlarmTime
    {
        get => _alarmTime;
        set => SetProperty(ref _alarmTime, value);
    }

    public string AlarmMessage
    {
        get => _alarmMessage;
        set => SetProperty(ref _alarmMessage, value);
    }

    public int AlarmCount
    {
        get => _alarmCount;
        set => SetProperty(ref _alarmCount, value);
    }

    public string AlarmLevel
    {
        get => _alarmLevel;
        set => SetProperty(ref _alarmLevel, value);
    }

    public int WarningCount
    {
        get => _warningCount;
        set => SetProperty(ref _warningCount, value);
    }

    public int CautionCount
    {
        get => _cautionCount;
        set => SetProperty(ref _cautionCount, value);
    }

    public int IgnoreCount
    {
        get => _ignoreCount;
        set => SetProperty(ref _ignoreCount, value);
    }
}

/// <summary>커스텀 전시정보 한 칸. 비율(Ratio)과 색은 JSON 에서 온다.</summary>
public sealed class CustomStatusItemViewModel : ObservableObject
{
    private string _name = string.Empty;
    private string _value = string.Empty;
    private Brush _valueBrush = Brushes.White;

    public string Key { get; init; } = string.Empty;

    public double Ratio { get; init; } = 1;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public Brush ValueBrush
    {
        get => _valueBrush;
        set => SetProperty(ref _valueBrush, value);
    }
}

/// <summary>Core 창의 루트 데이터 컨텍스트.</summary>
public sealed class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        StatusBar = new StatusBarViewModel();
    }

    public StatusBarViewModel StatusBar { get; }
}
