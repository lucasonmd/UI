namespace HMICore.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        StatusBar = new StatusBarViewModel();
    }

    public StatusBarViewModel StatusBar { get; }
}
