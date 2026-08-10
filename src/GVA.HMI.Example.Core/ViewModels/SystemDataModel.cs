namespace HMICore.ViewModels;

/// <summary>바인딩 경로 <c>SystemData.*</c> 그대로 - 기존 프로젝트가 이미 이
/// 경로로 바인딩하고 있어서 이름/구조를 맞췄다.</summary>
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
