using System.Collections.ObjectModel;

namespace GVA.HMI.Example.Application.ViewModels;

/// <summary>
/// Application(중앙 대시보드) 화면 전체의 데이터 컨텍스트.
/// 여기 들어있는 값은 레이아웃 검증용 예시 데이터다 - 실제 연동 시 이 클래스만 교체하면 된다.
/// (Core.MainViewModel 에 있던 Groups 를 그대로 옮겨왔다 - 두 프로세스로 분리되면서.)
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
