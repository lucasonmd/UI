using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using HMICore.Controls;

namespace HMICore;

public partial class MainWindow : Window
{
    // 도려낼 자리. Controls/CoreUiBuilder.cs 의 같은 상수와 값이 반드시 같아야 한다.
    private const double CanvasWidth = 1920, CanvasHeight = 1080;
    private const double AppX = 165, AppY = 105, AppWidth = 1590, AppHeight = 915;

    private readonly CoreUiBuilder _ui;

    public MainWindow()
    {
        InitializeComponent();

        _ui = new CoreUiBuilder(this);
        ApplyPreviewContent();

        SourceInitialized += OnSourceInitialized;
        // 창 크기가 바뀌면 Viewbox 배율이 바뀌므로 구멍도 다시 뚫어야 한다.
        SizeChanged += (_, _) => UpdateApplicationCutout();
    }

    private void ApplyPreviewContent()
    {
        SetPreview("F1", "DRIVE", SoftButtonState.SoftButtonSelected);
        SetPreview("F2", "ENGINE", SoftButtonState.SoftButtonEnabled);
        SetPreview("F3", "BATTERY", SoftButtonState.SoftButtonEnabled);
        SetPreview("F4", "COMMS LINK STATUS 02", SoftButtonState.SoftButtonEnabled);
        SetPreview("F5", "WEAPONS", SoftButtonState.SoftButtonDisabled);
        SetPreview("F6", "MAINTENANCE", SoftButtonState.SoftButtonEnabled);

        SetPreview("F7", "ALERTS", SoftButtonState.SoftButtonEnabled);
        SetPreview("F8", "MAP", SoftButtonState.SoftButtonEnabled);
        SetPreview("F9", "VIDEO", SoftButtonState.SoftButtonDisabled);
        SetPreview("F10", "MISSION DATA LOG", SoftButtonState.SoftButtonEnabled);
        SetPreview("F11", "SETTINGS", SoftButtonState.SoftButtonEnabled);
        SetPreview("F12", "TEST", SoftButtonState.SoftButtonDisabled);

        SetPreview("F13", "UP", SoftButtonState.SoftButtonEnabled);
        SetPreview("F14", "ALARMS", SoftButtonState.SoftButtonEnabled);
        SetPreview("F15", "THREAT", SoftButtonState.SoftButtonEnabled);
        SetPreview("F16", "ACK", SoftButtonState.SoftButtonEnabled);
        SetPreview("F17", "▲", SoftButtonState.SoftButtonEnabled);
        SetPreview("F18", "▼", SoftButtonState.SoftButtonEnabled);
        SetPreview("F19", "LABELS", SoftButtonState.SoftButtonEnabled);
        SetPreview("F20", "ENTER", SoftButtonState.SoftButtonEnabled);

        T_Button_State[] labelStates =
        {
            T_Button_State.LabelStateSelected,
            T_Button_State.LabelStateEnabled,
            T_Button_State.LabelStateEnabled,
            T_Button_State.LabelStateEnabled,
            T_Button_State.LabelStateDisabled,
            T_Button_State.LabelStateEnabled,
            T_Button_State.LabelStateEnabled,
            T_Button_State.LabelStateDisabled,
        };
        for (var i = 0; i < _ui.TopLabels.Count && i < labelStates.Length; i++)
        {
            _ui.TopLabels[i].SetLabelState(labelStates[i], string.Empty);
        }

        ApplyStatusBarPreview();
    }

    private void ApplyStatusBarPreview()
    {
        var status = _ui.StatusBar;

        status.SystemData.HeadingMil = "4392 mil";
        status.SystemData.Coordinate = "37.5665, 126.9780";

        status.AlarmData.AlarmTime = "14:06:58";
        status.AlarmData.AlarmMessage = "COOLANT TEMP OVER LIMIT — 104 °C";
        status.AlarmData.AlarmCount = 3;
        status.AlarmData.AlarmLevel = "주의";
        status.AlarmData.WarningCount = 0;
        status.AlarmData.CautionCount = 1;
        status.AlarmData.IgnoreCount = 23;

        status.LoadCustomDisplayData("""
        {
          "status_bar": {
            "itemWithLayout": [
              ["MSN", "임무", "MSN", 1],
              ["OPMODE", "모드", "MODE", 1],
              ["GPS", "항법", "NAV", 1],
              ["NET", "통신", "NET", 1],
              ["PWR", "전원", "PWR", 1],
              ["NBC", "방호", "NBC", 1],
              ["FUEL", "연료", "FUEL", 1],
              ["CREW", "승무", "CREW", 1]
            ],
            "itemWithValue": {
              "MSN": [["RECON", "정찰", "RECON", "#FF6FBF80"]],
              "OPMODE": [["ACTIVE", "활성", "ACTIVE", "#FF5FAECC"]],
              "GPS": [["FIX3D", "3D고정", "3D FIX", "#FF6FBF80"]],
              "NET": [["LINKOK", "정상", "LINK OK", "#FF6FBF80"]],
              "PWR": [["EXT", "외부", "EXT", "#FF5FAECC"]],
              "NBC": [["CLEAR", "청정", "CLEAR", "#FF6FBF80"]],
              "FUEL": [["L68", "68%", "68%", "#FFE0A63C"]],
              "CREW": [["FULL", "4/4", "4/4", "#FF6FBF80"]]
            }
          }
        }
        """);
    }

    private void SetPreview(string keyId, string label, SoftButtonState state)
    {
        if (!_ui.SideButtons.TryGetValue(keyId, out var button) &&
            !_ui.BottomButtons.TryGetValue(keyId, out button))
        {
            return;
        }

        button.SetLabel(label);
        button.SetState(state);
    }

    private void OnSourceInitialized(object? sender, EventArgs e) => UpdateApplicationCutout();

    /// <summary>
    /// Application 영역(캔버스 기준 <see cref="AppX"/>,<see cref="AppY"/> 에 1590x915)을
    /// 창에서 아예 도려낸다 - 그 자리로 바탕화면이 그대로 비치고 클릭도 통과한다.
    ///
    /// AllowsTransparency 대신 Win32 윈도우 리전을 쓰는 이유 : AllowsTransparency 는
    /// 창 전체를 소프트웨어 렌더링으로 떨어뜨린다. 1920x1080 전면 UI 에서는 비싸고,
    /// 여기서 필요한 건 사각형 하나를 뚫는 것뿐이라 리전이 더 싸고 정확하다.
    ///
    /// 좌표 계산은 Viewbox(Stretch=Uniform)와 같은 산수를 반복한다 - 균일 축소 +
    /// 레터박스 중앙정렬. 리전은 물리 픽셀 단위라 DPI 배율을 곱해야 한다.
    /// </summary>
    private void UpdateApplicationCutout()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero || !GetClientRect(hwnd, out var client))
        {
            return;
        }

        double clientW = client.Right - client.Left;
        double clientH = client.Bottom - client.Top;
        if (clientW <= 0 || clientH <= 0)
        {
            return;
        }

        var scale = Math.Min(clientW / CanvasWidth, clientH / CanvasHeight);
        var offsetX = (clientW - CanvasWidth * scale) / 2.0;
        var offsetY = (clientH - CanvasHeight * scale) / 2.0;

        var left = (int)Math.Round(offsetX + AppX * scale);
        var top = (int)Math.Round(offsetY + AppY * scale);
        var right = left + (int)Math.Round(AppWidth * scale);
        var bottom = top + (int)Math.Round(AppHeight * scale);

        // SetWindowRgn 은 리전 소유권을 가져가므로 full 은 여기서 해제하지 않는다.
        var full = CreateRectRgn(0, 0, (int)clientW, (int)clientH);
        var hole = CreateRectRgn(left, top, right, bottom);
        CombineRgn(full, full, hole, RgnDiff);
        DeleteObject(hole);

        SetWindowRgn(hwnd, full, true);
    }

    private void OnWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
        }
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            ToggleMaximize();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            e.Handled = true;
        }
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private const int RgnDiff = 4;

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateRectRgn(int left, int top, int right, int bottom);

    [DllImport("gdi32.dll")]
    private static extern int CombineRgn(IntPtr dest, IntPtr src1, IntPtr src2, int mode);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr obj);

    [DllImport("user32.dll")]
    private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out Rect32 lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect32
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
