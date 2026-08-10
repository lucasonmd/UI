namespace HMICore.ViewModels;

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
