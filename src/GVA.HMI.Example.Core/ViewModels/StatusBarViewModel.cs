using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;
using HMICore.Models;

namespace HMICore.ViewModels;

/// <summary>
/// StatusBar 전체 데이터 컨텍스트. <see cref="SystemData"/>/<see cref="AlarmData"/>
/// 는 기존 프로젝트가 이미 쓰는 바인딩 경로(<c>SystemData.*</c>, <c>AlarmData.*</c>)
/// 를 그대로 맞췄다 - 이 클래스 자체가 바뀌어도 XAML 의 바인딩 경로는 안 바뀐다.
/// </summary>
public sealed class StatusBarViewModel : ObservableObject
{
    private readonly DispatcherTimer _clock;
    private readonly List<CustomFieldLayout> _customLayout = new();
    private readonly Dictionary<string, List<CustomFieldOption>> _customValues = new();

    /// <summary>Key → 현재 반영된 실측값 코드(itemWithValue[key][n].Value). 언어 전환/
    /// 재조립을 해도 이 값은 유지된다 - <see cref="UpdateCustomValue"/> 가 갱신한다.</summary>
    private readonly Dictionary<string, string> _currentValueCodes = new();

    private bool _isEnglish;

    public SystemDataModel SystemData { get; } = new();

    public AlarmDataModel AlarmData { get; } = new();

    /// <summary>커스텀 전시정보 - <see cref="LoadCustomDisplayData"/> 로 채운다.</summary>
    public ObservableCollection<CustomStatusItemViewModel> CustomItems { get; } = new();

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
    /// 커스텀 전시정보 JSON 을 읽어 <see cref="CustomItems"/> 를 다시 채운다. 실제
    /// 연동 시 이 문자열이 파일에서 왔는지 소켓에서 왔는지는 이 메서드가 몰라도
    /// 된다 - 그냥 JSON 텍스트만 넘기면 된다.
    /// </summary>
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
                // 초기값 = 목록 첫 항목. 실측값이 들어오기 시작하면 UpdateCustomValue() 가 갈아치운다.
                _currentValueCodes[key] = value[0].Value;
            }
        }

        RebuildCustomItems();
    }

    /// <summary>
    /// Key 하나의 실측값을 갱신한다 - <c>itemWithValue[key]</c> 에서 <c>Value</c> 가
    /// <paramref name="value"/> 와 일치하는 항목을 찾아 그 표시(KorValue/EngValue/
    /// Color)로 바꾼다. 외부에서 계속 호출할 진입점은 이거다 - 레이아웃/값 테이블을
    /// 통째로 다시 보낼 필요 없이 Key 와 새 코드값만 넘기면 된다. 일치하는 Key/Value
    /// 가 없으면 조용히 무시한다(레이아웃에 없는 Key 를 잘못 부른 경우 등).
    /// </summary>
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

        var item = CustomItems.FirstOrDefault(i => i.Key == key);
        if (item is null)
        {
            return;
        }

        item.Value = _isEnglish ? option.EngValue : option.KorValue;
        item.ValueBrush = ParseBrush(option.Color);
    }

    /// <summary>한/영 전환 - 커스텀 전시정보의 Name/Value 를 다시 계산해서 밀어 넣는다.</summary>
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

        foreach (var field in _customLayout)
        {
            _customValues.TryGetValue(field.Key, out var candidates);

            CustomFieldOption? current = null;
            if (candidates is not null && _currentValueCodes.TryGetValue(field.Key, out var code))
            {
                current = candidates.FirstOrDefault(o => o.Value == code);
            }
            current ??= candidates?.FirstOrDefault();

            CustomItems.Add(new CustomStatusItemViewModel
            {
                Key = field.Key,
                Ratio = field.Ratio > 0 ? field.Ratio : 1,
                Name = _isEnglish ? field.EngName : field.KorName,
                Value = (_isEnglish ? current?.EngValue : current?.KorValue) ?? string.Empty,
                ValueBrush = ParseBrush(current?.Color),
            });
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
                // 잘못된 색상 문자열 - 기본색으로 넘어간다.
            }
        }

        return System.Windows.Application.Current.TryFindResource("ForegroundBrush") as Brush ?? Brushes.White;
    }

    // 년월일까지 표시(HH:mm:ss 만으론 부족하다는 피드백).
    private void UpdateClock() => SystemData.CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
}
