using System.Collections.ObjectModel;

namespace GVA.HMI.Example.Application.ViewModels;

/// <summary>
/// Application(중앙 대시보드) 화면 전체의 데이터 컨텍스트.
/// 여기 들어있는 값은 레이아웃 검증용 예시 데이터다 - 실제 연동 시 이 클래스만 교체하면 된다.
/// (Core.MainViewModel 에 있던 Groups 를 그대로 옮겨왔다 - 두 프로세스로 분리되면서.)
///
/// 이 화면이 쓰는 데이터 단위(<see cref="DisplayGroup"/> · <see cref="MetricRow"/> ·
/// <see cref="Severity"/>)는 이 화면 밖에서 쓰이지 않으므로 한 파일에 같이 둔다.
/// </summary>
public sealed class ApplicationViewModel : ObservableObject
{
    public ApplicationViewModel()
    {
        Groups = new ObservableCollection<DisplayGroup>
        {
            CreateGroup("POWERTRAIN", "PWR", "2140", "rpm", Severity.None, new[]
            {
                new MetricRow("COOLANT TEMP", "104 °C", Severity.Caution),
                new MetricRow("FUEL LEVEL", "68 %"),
                new MetricRow("OIL PRESSURE", "4.2 bar", Severity.Normal),
            }),
            CreateGroup("DRIVETRAIN", "DRV", "42", "km/h", Severity.None, new[]
            {
                new MetricRow("GEAR", "D3"),
                new MetricRow("STEERING ANGLE", "−7.5 °"),
                new MetricRow("BRAKE PRESSURE", "NORMAL", Severity.Normal),
            }),
            CreateGroup("ELECTRICAL", "ELEC", "27.8", "V", Severity.None, new[]
            {
                new MetricRow("ALTERNATOR LOAD", "63 %"),
                new MetricRow("AUX POWER", "TRIPPED", Severity.Warning),
                new MetricRow("CURRENT DRAW", "118 A"),
            }),
            CreateGroup("DIAGNOSTICS", "BIT", "1", "CAUTION", Severity.Caution, new[]
            {
                new MetricRow("NODES ONLINE", "14 / 15"),
                new MetricRow("LAST BIT", "14:02:31"),
                new MetricRow("LOG CAPACITY", "41 %", Severity.Normal),
            }),
        };
    }

    public ObservableCollection<DisplayGroup> Groups { get; }

    private static DisplayGroup CreateGroup(
        string title,
        string code,
        string heroValue,
        string heroUnit,
        Severity heroSeverity,
        IEnumerable<MetricRow> rows)
    {
        var group = new DisplayGroup
        {
            Title = title,
            Code = code,
            HeroValue = heroValue,
            HeroUnit = heroUnit,
            HeroSeverity = heroSeverity,
        };

        foreach (var row in rows)
        {
            group.Rows.Add(row);
        }

        return group;
    }
}

/// <summary>정보전시영역의 카드 하나. 타이틀바 + 대표값 + 상세 3줄.</summary>
public sealed class DisplayGroup : ObservableObject
{
    private string _title = string.Empty;
    private string _code = string.Empty;
    private string _heroValue = string.Empty;
    private string _heroUnit = string.Empty;
    private Severity _heroSeverity = Severity.None;

    /// <summary>전시영역 타이틀 (예: 동력계통).</summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>타이틀바 우측 약호 (예: PWR).</summary>
    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    public string HeroValue
    {
        get => _heroValue;
        set => SetProperty(ref _heroValue, value);
    }

    public string HeroUnit
    {
        get => _heroUnit;
        set => SetProperty(ref _heroUnit, value);
    }

    public Severity HeroSeverity
    {
        get => _heroSeverity;
        set => SetProperty(ref _heroSeverity, value);
    }

    public ObservableCollection<MetricRow> Rows { get; } = new();
}

/// <summary>전시 카드 하단의 키-값 한 줄.</summary>
public sealed class MetricRow : ObservableObject
{
    private string _key = string.Empty;
    private string _value = string.Empty;
    private Severity _severity = Severity.None;

    public MetricRow()
    {
    }

    public MetricRow(string key, string value, Severity severity = Severity.None)
    {
        _key = key;
        _value = value;
        _severity = severity;
    }

    public string Key
    {
        get => _key;
        set => SetProperty(ref _key, value);
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public Severity Severity
    {
        get => _severity;
        set => SetProperty(ref _severity, value);
    }
}

/// <summary>
/// 규격서 §4 의 상태색. 적/황/녹은 Warning · Caution · Advisory 예약색이므로
/// 일반 값 표기에는 <see cref="None"/> 을 쓴다.
/// </summary>
public enum Severity
{
    None,
    Warning,
    Caution,
    Normal,
}
