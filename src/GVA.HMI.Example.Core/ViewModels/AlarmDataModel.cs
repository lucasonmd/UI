namespace HMICore.ViewModels;

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
