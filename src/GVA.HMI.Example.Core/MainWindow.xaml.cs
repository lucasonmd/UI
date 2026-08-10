using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using HMICore.Controls;

namespace HMICore;

public partial class MainWindow : Window
{
    private readonly CoreUiBuilder _ui;
    private Process? _applicationProcess;

    public MainWindow()
    {
        InitializeComponent();

        _ui = new CoreUiBuilder(this);
        ApplyPreviewContent();

        SourceInitialized += OnSourceInitialized;
        Closing += OnClosing;
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
              ["MSN", "임무", "MSN", 0.3],
              ["OPMODE", "모드", "MODE", 0.35],
              ["GPS", "위성항법", "GPS", 0.35]
            ],
            "itemWithValue": {
              "MSN": [["RECON", "정찰", "RECON", "#FF32D74B"]],
              "OPMODE": [["ACTIVE", "활성", "ACTIVE", "#FF00C8FF"]],
              "GPS": [["FIX3D", "3D 고정", "3D FIX", "#FF32D74B"]]
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

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var ownerHwnd = new WindowInteropHelper(this).Handle;

        HwndSource.FromHwnd(ownerHwnd)?.AddHook(WndProc);

        var corePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(corePath))
        {
            return;
        }

        var appPath = corePath.Replace(
            "GVA.HMI.Example.Core", "GVA.HMI.Example.Application", StringComparison.Ordinal);

        if (!File.Exists(appPath))
        {
            Trace.TraceWarning($"Application.exe 를 찾을 수 없다: {appPath}");
            return;
        }

        try
        {
            _applicationProcess = Process.Start(new ProcessStartInfo(appPath)
            {
                ArgumentList = { $"--owner={ownerHwnd}" },
                UseShellExecute = false,
            });
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Application 프로세스 실행 실패: {ex.Message}");
        }
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_applicationProcess is null)
        {
            return;
        }

        try
        {
            if (!_applicationProcess.HasExited)
            {
                _applicationProcess.CloseMainWindow();
                if (!_applicationProcess.WaitForExit(500))
                {
                    _applicationProcess.Kill();
                }
            }
        }
        catch (InvalidOperationException)
        {
        }
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

    private const int WM_NCCALCSIZE = 0x0083;

    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_NCCALCSIZE && wParam != IntPtr.Zero)
        {
            handled = true;
        }

        return IntPtr.Zero;
    }
}
