using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace GVA.HMI.Example.Application.Interop;

/// <summary>
/// 이 창을 Core 창의 "Application 영역" 위에 정확히 겹쳐 앉힌다. Core 는
/// Viewbox(Stretch=Uniform)로 그 캔버스를 자기 창 크기에 맞춰 균일 축소(레터박스
/// 포함)하므로, 여기서도 같은 산수를 그대로 반복해서 Core 의 창 사각형
/// (GetWindowRect)만 보고 스스로 자리를 계산한다 - Core 쪽에서 좌표를 밀어줄
/// 필요가 없다.
///
/// 기준 좌표(<see cref="CanvasWidth"/> 등)는 "기본 형태" 상수라 Core/MainWindow.xaml.cs
/// 상단의 같은 상수와 값이 반드시 같아야 한다 - 둘 다 각자 파일을 갖고 있으니
/// (설정 파일 공유는 하지 않는다, 종속을 피하려고 일부러 그렇다) Application 영역
/// 좌표를 바꿀 땐 두 파일을 같이 고쳐야 한다.
///
/// Win32 owner(부모가 아니라 "소유") 관계로 Core 에 묶는다 - 항상 Core 위에 뜨고,
/// Core 가 최소화되면 같이 숨고, Core 가 닫히면(DestroyWindow) 시스템이 이 창도
/// 같이 닫아준다. Sync() 에서 IsWindow 로 한 번 더 확인하는 건 그 보험이다.
/// </summary>
internal sealed class OwnerWindowSync
{
    // Core 의 논리 캔버스 크기와 그 안에서 Application 이 차지하는 자리.
    // Core/MainWindow.xaml.cs 의 CanvasWidth/CanvasHeight/AppX/AppY/AppWidth/AppHeight 와 반드시 같아야 한다.
    private const double CanvasWidth = 1920;
    private const double CanvasHeight = 1080;
    private const double AppX = 165, AppY = 145, AppWidth = 1590, AppHeight = 875;

    private const int GwlHwndParent = -8;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpNoZOrder = 0x0004;

    private readonly Window _window;
    private readonly IntPtr _ownerHwnd;
    private readonly IntPtr _selfHwnd;
    private readonly DispatcherTimer _timer;

    private Rect32 _lastOwnerRect;
    private bool _hasLastRect;

    public OwnerWindowSync(Window window, IntPtr ownerHwnd)
    {
        _window = window;
        _ownerHwnd = ownerHwnd;
        _selfHwnd = new WindowInteropHelper(window).Handle;

        SetWindowLongPtr(_selfHwnd, GwlHwndParent, _ownerHwnd);

        // ~60Hz 폴링 - GetWindowRect 하나뿐이라 부담 없다. 프로세스 경계를 넘는
        // 실시간 이벤트 훅(SetWinEventHook) 대신 폴링을 쓴 이유는 훨씬 단순하고,
        // 데모 앱 하나에서 16ms 마다 Win32 호출 한 번은 비용이랄 것도 없기 때문이다.
        _timer = new DispatcherTimer(DispatcherPriority.Send)
        {
            Interval = TimeSpan.FromMilliseconds(16),
        };
        _timer.Tick += (_, _) => Sync();
    }

    /// <summary>첫 배치를 창이 화면에 보이기 전에 한 번 해두고(점프 방지), 폴링을 시작한다.</summary>
    public void Start()
    {
        Sync();
        _timer.Start();
    }

    private void Sync()
    {
        if (!IsWindow(_ownerHwnd))
        {
            // Core 가 사라졌다(닫힘/크래시) - 혼자 남아있을 이유가 없다.
            _timer.Stop();
            System.Windows.Application.Current.Shutdown();
            return;
        }

        if (!GetWindowRect(_ownerHwnd, out var ownerRect))
        {
            return;
        }

        if (_hasLastRect && ownerRect.Equals(_lastOwnerRect))
        {
            return;
        }

        _lastOwnerRect = ownerRect;
        _hasLastRect = true;

        var ownerW = ownerRect.Right - ownerRect.Left;
        var ownerH = ownerRect.Bottom - ownerRect.Top;
        if (ownerW <= 0 || ownerH <= 0)
        {
            return;
        }

        // Core 의 Viewbox(Stretch=Uniform) 와 동일한 산수 : 균일 축소 + 레터박스 중앙정렬.
        var scale = Math.Min(ownerW / CanvasWidth, ownerH / CanvasHeight);
        var contentW = CanvasWidth * scale;
        var contentH = CanvasHeight * scale;
        var offsetX = ownerRect.Left + (ownerW - contentW) / 2.0;
        var offsetY = ownerRect.Top + (ownerH - contentH) / 2.0;

        var x = (int)Math.Round(offsetX + AppX * scale);
        var y = (int)Math.Round(offsetY + AppY * scale);
        var w = (int)Math.Round(AppWidth * scale);
        var h = (int)Math.Round(AppHeight * scale);

        SetWindowPos(_selfHwnd, IntPtr.Zero, x, y, w, h, SwpNoActivate | SwpNoZOrder);
    }

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        => IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
            : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect32 lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect32 : IEquatable<Rect32>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public bool Equals(Rect32 other)
            => Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

        public override bool Equals(object? obj) => obj is Rect32 other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);
    }
}
