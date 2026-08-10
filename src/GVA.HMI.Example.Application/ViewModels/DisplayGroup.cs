using System.Collections.ObjectModel;

namespace GVA.HMI.Example.Application.ViewModels;

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
