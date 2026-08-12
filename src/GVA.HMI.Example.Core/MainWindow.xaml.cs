using System.Windows;
using System.Windows.Input;
using HMICore.Controls;

namespace HMICore;

public partial class MainWindow : Window
{
    private readonly CoreUiBuilder _ui;

    public MainWindow()
    {
        InitializeComponent();

        _ui = new CoreUiBuilder(this);
        ApplyPreviewContent();
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
}
