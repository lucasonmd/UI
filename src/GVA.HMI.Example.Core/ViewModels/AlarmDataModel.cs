namespace HMICore.ViewModels;

/// <summary>바인딩 경로 <c>AlarmData.*</c> 그대로. <see cref="AlarmLevel"/> 은
/// "경고"/"주의" 문자열이고, StatusBarView.xaml 의 DataTrigger 가 이 값을 직접
/// 비교해서 알람 시간/메세지/카운트 뱃지 색을 바꾼다.</summary>
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

    /// <summary>"경고" / "주의" (그 외 값이면 중립색).</summary>
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
