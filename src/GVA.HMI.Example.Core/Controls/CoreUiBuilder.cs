using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HMICore.ViewModels;
using HMICore.Views;

namespace HMICore.Controls;

public sealed class CoreUiBuilder
{
    private const double CanvasWidth = 1920;
    private const double CanvasHeight = 1080;

    private const double AppX = 165, AppY = 145, AppWidth = 1590, AppHeight = 875;
    private const double FrameMargin = 4;
    private const double FrameCornerRadius = 14;

    private const double StatusBarX = 0, StatusBarY = 0;

    private const double RailKeyHeight = 100;
    private const double RailPitch = 159.2;
    private const double LeftRailX = 0, LeftRailY = 124, LeftRailWidth = 153;
    private const int LeftRailCount = 6;
    private const double RightRailX = 1767, RightRailY = 124, RightRailWidth = 153;
    private const int RightRailCount = 6;

    private const double BottomRowX = 33, BottomRowY = 1028;
    private const double BottomKeyWidth = 214, BottomRowHeight = 48, BottomPitch = 234.29;
    private const int BottomRowCount = 8;

    private const double LabelX = 165, LabelY = 119, LabelWidth = 183, LabelHeight = 20, LabelPitch = 201;
    private const int LabelCount = 8;

    private readonly Window _window;
    private readonly Grid _gdMain;
    private readonly Canvas _coreCanvas = new();

    private readonly Dictionary<string, SoftButton> _sideButtons = new();
    private readonly Dictionary<string, SoftButton> _bottomButtons = new();

    private readonly List<TopLabel> _topLabels = new();

    public IReadOnlyDictionary<string, SoftButton> SideButtons => _sideButtons;

    public IReadOnlyDictionary<string, SoftButton> BottomButtons => _bottomButtons;

    public IReadOnlyList<TopLabel> TopLabels => _topLabels;

    public StatusBarViewModel StatusBar { get; } = new();

    public CoreUiBuilder(Window window)
    {
        _window = window;

        _gdMain = (window.FindName("gd_main") as Grid)
            ?? throw new InvalidOperationException(
                "gd_main 을 찾을 수 없다 - 호스트 창에 <Grid x:Name=\"gd_main\"/> 이 있어야 한다.");

        Build();
    }

    private void Build()
    {
        var root = new Grid
        {
            Width = CanvasWidth,
            Height = CanvasHeight,
            Background = (Brush)_window.FindResource("VoidBrush"),
            UseLayoutRounding = false,
        };

        var statusBar = new StatusBarView
        {
            DataContext = StatusBar,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(StatusBarX, StatusBarY, 0, 0),
        };
        root.Children.Add(statusBar);

        var applicationFrame = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(AppX - FrameMargin, AppY - FrameMargin, 0, 0),
            Width = AppWidth + FrameMargin * 2,
            Height = AppHeight + FrameMargin * 2,
            CornerRadius = new CornerRadius(FrameCornerRadius),
            BorderBrush = (Brush)_window.FindResource("FrameSeamBrush"),
            BorderThickness = new Thickness(1),
            IsHitTestVisible = false,
        };
        root.Children.Add(applicationFrame);

        var applicationPlaceholder = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(AppX, AppY, 0, 0),
            Width = AppWidth,
            Height = AppHeight,
            Background = (Brush)_window.FindResource("VoidBrush"),
            IsHitTestVisible = false,
            Child = new TextBlock
            {
                Text = "LAUNCHING APPLICATION…",
                FontFamily = (FontFamily)_window.FindResource("GvaMonoFont"),
                FontSize = 18,
                Foreground = (Brush)_window.FindResource("ForegroundOffBrush"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        root.Children.Add(applicationPlaceholder);

        root.Children.Add(_coreCanvas);

        var outerBezel = new Border
        {
            Width = CanvasWidth,
            Height = CanvasHeight,
            BorderBrush = (Brush)_window.FindResource("FrameSeamBrush"),
            BorderThickness = new Thickness(1),
            IsHitTestVisible = false,
        };
        root.Children.Add(outerBezel);

        BuildChassisPlates();
        BuildFunctionLabels();
        BuildRail(LeftRailX, LeftRailY, LeftRailCount, "Main", 1, (Style)_window.FindResource("SideSoftButtonStyle"));
        BuildRail(RightRailX, RightRailY, RightRailCount, "Main", LeftRailCount + 1, (Style)_window.FindResource("SideSoftButtonRightStyle"));
        BuildBottomRow();

        var viewbox = new Viewbox
        {
            Stretch = Stretch.Uniform,
            Child = root,
        };
        RenderOptions.SetBitmapScalingMode(viewbox, BitmapScalingMode.HighQuality);

        _gdMain.Children.Add(viewbox);
    }

    private void BuildChassisPlates()
    {
        var voidBrush = (Brush)_window.FindResource("VoidBrush");
        var railHeight = RailPitch * (LeftRailCount - 1) + RailKeyHeight;

        AddPlate(LeftRailX, LeftRailY, LeftRailWidth, railHeight, new CornerRadius(0, 6, 6, 0), voidBrush);
        AddPlate(RightRailX, RightRailY, RightRailWidth, railHeight, new CornerRadius(6, 0, 0, 6), voidBrush);
        AddPlate(0, BottomRowY, CanvasWidth, BottomRowHeight, new CornerRadius(0), voidBrush);
    }

    private void AddPlate(double x, double y, double width, double height, CornerRadius radius, Brush background)
    {
        var plate = new Border
        {
            Width = width,
            Height = height,
            CornerRadius = radius,
            Background = background,
            IsHitTestVisible = false,
        };
        Canvas.SetLeft(plate, x);
        Canvas.SetTop(plate, y);
        _coreCanvas.Children.Add(plate);
    }

    private void BuildFunctionLabels()
    {
        var style = (Style)_window.FindResource("TopLabelStyle");
        for (var i = 0; i < LabelCount; i++)
        {
            var label = new TopLabel
            {
                Style = style,
                State = T_Button_State.LabelStateDisabled,
            };
            Canvas.SetLeft(label, LabelX + i * LabelPitch);
            Canvas.SetTop(label, LabelY);

            _coreCanvas.Children.Add(label);
            _topLabels.Add(label);
        }
    }

    private void BuildRail(double x, double y, int count, string groupName, int startIndex, Style style)
    {
        for (var i = 0; i < count; i++)
        {
            var index = startIndex + i;
            AddSoftButton(_sideButtons, style, x, y + i * RailPitch, groupName, $"F{index}", $"ngva.f{index}");
        }
    }

    private void BuildBottomRow()
    {
        var style = (Style)_window.FindResource("BottomSoftButtonStyle");
        for (var i = 0; i < BottomRowCount; i++)
        {
            var index = LeftRailCount + RightRailCount + i + 1;
            AddSoftButton(_bottomButtons, style, BottomRowX + i * BottomPitch, BottomRowY, groupName: string.Empty, $"F{index}", $"ngva.f{index}");
        }
    }

    private void AddSoftButton(Dictionary<string, SoftButton> target, Style style, double x, double y, string groupName, string keyId, string label)
    {
        var button = new SoftButton
        {
            Style = style,
            GroupName = groupName,
            KeyId = keyId,
            Label = label,
            State = SoftButtonState.SoftButtonEnabled,
        };
        Canvas.SetLeft(button, x);
        Canvas.SetTop(button, y);

        _coreCanvas.Children.Add(button);
        target.Add(keyId, button);
    }
}
