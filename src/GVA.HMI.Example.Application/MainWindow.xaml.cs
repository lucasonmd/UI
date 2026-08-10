using System.Windows;
using GVA.HMI.Example.Application.Interop;
using GVA.HMI.Example.Application.ViewModels;

namespace GVA.HMI.Example.Application;

public partial class MainWindow : Window
{
    // Core/MainWindow.xaml.cs(CoreUiBuilder.cs) 의 AppWidth/AppHeight 와 반드시
    // 같아야 한다 - 단독 실행(디버그) 모드의 기본 창 크기로만 쓰인다(Core 가
    // 실행한 정상 경로에서는 OwnerWindowSync 가 실시간으로 크기를 다시 잡는다).
    private const double DefaultWidth = 1590;
    private const double DefaultHeight = 875;

    private OwnerWindowSync? _sync;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new ApplicationViewModel();

        if (App.OwnerHandle != IntPtr.Zero)
        {
            // Core 가 실행한 정상 경로 - SourceInitialized(HWND 생성 직후, 아직 화면에
            // 보이기 전)에서 자리를 잡아야 처음부터 정확한 위치에 뜬다(점프 없음).
            SourceInitialized += OnSourceInitialized;
        }
        else
        {
            // 단독 실행(디버그용) - 그냥 보통 창처럼 가운데에, 크기 조절 가능하게.
            Width = DefaultWidth;
            Height = DefaultHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.CanResize;
        }
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _sync = new OwnerWindowSync(this, App.OwnerHandle);
        _sync.Start();
    }
}
