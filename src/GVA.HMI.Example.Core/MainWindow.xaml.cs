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

        // 실제 HMI 화면(StatusBar/레일/하단 열/기능영역 라벨/Application 프레임)은
        // 전부 CoreUiBuilder 가 gd_main 안에 만든다 - 이 창은 껍데기다.
        _ui = new CoreUiBuilder(this);
        ApplyPreviewContent();

        SourceInitialized += OnSourceInitialized;
        Closing += OnClosing;
    }

    /// <summary>
    /// 보기용 테스트 초기화 - 실제 서비스에선 외부(기존 프로젝트)가
    /// SetLabel/SetState/SetLabelState 로 채우므로 없어도 되는 코드다.
    /// 이 프로젝트를 단독으로 실행했을 때 빈 "ngva.f1"~"ngva.f20"/전부 Disabled
    /// 상태만 보이면 확인하기 불편해서, 예전에 쓰던 데모 문구·상태로 한 번
    /// 덮어써 둔 것뿐이다 - 지워도 레이아웃/기능엔 전혀 영향이 없다.
    /// </summary>
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

    /// <summary>
    /// StatusBar 도 같은 이유로 보기용 데이터를 한 번 채운다. SystemData.CurrentTime
    /// 은 StatusBarViewModel 이 알아서 매초 갱신하므로 여기서 건드리지 않는다.
    /// LoadCustomDisplayData 호출은 실제 연동부에서도 그대로 쓰는 진입점이다 -
    /// 문자열이 어디서 왔는지(여기서는 그냥 인라인 리터럴)만 다르다.
    /// </summary>
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

    /// <summary>
    /// Application(중앙 대시보드)은 별도 프로세스다 - 여기서 HWND 가 막 생긴 시점에
    /// 그 exe 를 자동으로 띄운다. Application 쪽이 이 HWND 를 받아서 자기 창을
    /// Win32 소유(owner) 관계로 묶고, Application 영역 자리를 스스로 추적해서
    /// 따라온다 - Core 는 자리만 비워두면 된다(CoreUiBuilder 의 LAUNCHING 자리표시자 참고).
    /// </summary>
    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var ownerHwnd = new WindowInteropHelper(this).Handle;

        var corePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(corePath))
        {
            return;
        }

        // Core.exe 와 Application.exe 는 같은 빌드 구성(Debug/Release, TFM)으로
        // 나란히 빌드된다는 전제 - 폴더/파일명의 "Core" 를 "Application" 로만
        // 바꾸면 정확히 그 자리를 가리킨다.
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

    /// <summary>
    /// Win32 owner 관계상 Core 를 닫으면(DestroyWindow) 소유된 Application 창도
    /// 시스템이 같이 닫아주지만, 혹시 몰라 프로세스 자체도 명시적으로 정리한다.
    /// </summary>
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
            // 이미 종료됐다 - 무시해도 안전하다.
        }
    }

    /// <summary>
    /// WindowStyle="None" 이라 타이틀바가 없다 - 빈 배경(소프트키가 아닌 곳)을
    /// 누른 채 끌면 창을 옮길 수 있게 대신 처리한다. 버튼 위에서는 Button 이
    /// MouseLeftButtonDown 을 먼저 처리(Handled=true)하므로 여기까지 올라오지 않는다.
    /// </summary>
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
            // 버튼을 뗀 상태로 DragMove 가 불릴 수 있는 타이밍 - 무시해도 안전하다.
        }
    }

    /// <summary>
    /// F1~F20/기능영역 라벨 입력 이벤트는 여기서 다루지 않는다 - 호출부가
    /// 나중에 직접 등록한다(CoreUiBuilder.SideButtons/BottomButtons/TopLabels 참고).
    /// 여기서는 창 자체에 관한 단축키(Alt+Enter/Esc)만 다룬다.
    /// </summary>
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

    /// <summary>
    /// 창이 항상 WindowStyle=None 이므로 "전체화면 토글"은 이제 Maximized 여부만 바꾸면 된다.
    /// </summary>
    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}
