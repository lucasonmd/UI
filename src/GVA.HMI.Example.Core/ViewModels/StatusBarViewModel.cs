using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;
using HMICore.Models;

namespace HMICore.ViewModels;

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

    public void LoadCustomDisplayData(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var root = JsonSerializer.Deserialize<StatusBarCustomRoot>(json, options);
        var section = root?.StatusBar;
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
