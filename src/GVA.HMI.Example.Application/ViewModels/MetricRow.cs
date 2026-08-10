namespace GVA.HMI.Example.Application.ViewModels;

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
