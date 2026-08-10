namespace HMICore.ViewModels;

/// <summary>
/// Core 화면(StatusBar + 소프트키 레일)의 데이터 컨텍스트.
/// Application(중앙 대시보드) 데이터는 더 이상 여기 없다 - 별도 프로세스
/// (GVA.HMI.Example.Application) 가 자기 자신의 ViewModel 을 따로 갖는다.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        StatusBar = new StatusBarViewModel();
    }

    public StatusBarViewModel StatusBar { get; }
}
