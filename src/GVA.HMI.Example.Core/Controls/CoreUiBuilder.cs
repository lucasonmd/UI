using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HMICore.ViewModels;
using HMICore.Views;

// ImplicitUsings 가 System.IO 를 끌어와서 Path 가 모호해진다 - 도형 쪽으로 못박는다.
using Path = System.Windows.Shapes.Path;

namespace HMICore.Controls;

public sealed class CoreUiBuilder
{
    private const double CanvasWidth = 1920;
    private const double CanvasHeight = 1080;

    private const double AppX = 165, AppY = 105, AppWidth = 1590, AppHeight = 915;
    private const double FrameMargin = 4;
    private const double FrameCornerRadius = 14;

    // 배경판을 캔버스 밖으로 얼마나 넘치게 그릴지. 아래 BuildVoidBackdrop 참고.
    private const double BackdropOverscan = 2000;

    private const double StatusBarX = 0, StatusBarY = 0;

    private const double RailKeyHeight = 100;
    private const double RailPitch = 167.2;
    private const double LeftRailX = 0, LeftRailY = 84, LeftRailWidth = 153;
    private const int LeftRailCount = 6;
    private const double RightRailX = 1767, RightRailY = 84, RightRailWidth = 153;
    private const int RightRailCount = 6;

    private const double BottomRowX = 33, BottomRowY = 1028;
    private const double BottomKeyWidth = 214, BottomRowHeight = 48, BottomPitch = 234.29;
    private const int BottomRowCount = 8;

    private const double LabelX = 165, LabelY = 79, LabelWidth = 183, LabelHeight = 20, LabelPitch = 201;
    private const int LabelCount = 8;

    private readonly Window _window;
    private readonly Grid _gdMain;
    private readonly Canvas _coreCanvas = new();
    private readonly ContentControl _applicationHost = new();

    private readonly Dictionary<string, SoftButton> _sideButtons = new();
    private readonly Dictionary<string, SoftButton> _bottomButtons = new();

    private readonly List<TopLabel> _topLabels = new();

    public IReadOnlyDictionary<string, SoftButton> SideButtons => _sideButtons;

    public IReadOnlyDictionary<string, SoftButton> BottomButtons => _bottomButtons;

    public IReadOnlyList<TopLabel> TopLabels => _topLabels;

    /// <summary>
    /// Application 영역(<see cref="AppX"/>,<see cref="AppY"/> 에 1590x915)에 얹을 자리.
    /// 비워두면 그 자리는 완전히 투명해서 뒤가 그대로 비치고 클릭도 통과한다.
    /// <c>Content</c> 를 넣으면 그 엘리먼트만 그 위에 그려진다(넣은 부분만 불투명해진다).
    /// </summary>
    public ContentControl ApplicationHost => _applicationHost;

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
        // Background 를 칠하지 않는다 - 사각형 하나라 가운데를 비울 수가 없다.
        // 대신 아래 BuildVoidBackdrop() 이 "캔버스 - Application 영역" 모양으로 칠한다.
        var root = new Grid
        {
            Width = CanvasWidth,
            Height = CanvasHeight,
            UseLayoutRounding = false,
            // 배경판이 캔버스 밖으로 넘쳐서 레터박스 여백을 덮는다 - 켜면 그 여백이 뚫린다.
            ClipToBounds = false,
        };

        root.Children.Add(BuildVoidBackdrop());

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

        // 배경을 주지 않는다 - 배경판이 이미 비워둔 자리이므로 그대로 두면 투명하고,
        // Content 를 넣었을 때만 그 엘리먼트가 그 위에 그려진다.
        _applicationHost.HorizontalAlignment = HorizontalAlignment.Left;
        _applicationHost.VerticalAlignment = VerticalAlignment.Top;
        _applicationHost.Margin = new Thickness(AppX, AppY, 0, 0);
        _applicationHost.Width = AppWidth;
        _applicationHost.Height = AppHeight;
        root.Children.Add(_applicationHost);

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
            // root 와 같은 이유 - 넘쳐 나온 배경판을 살려둬야 레터박스가 안 뚫린다.
            ClipToBounds = false,
        };
        RenderOptions.SetBitmapScalingMode(viewbox, BitmapScalingMode.HighQuality);

        _gdMain.Children.Add(viewbox);
    }

    /// <summary>
    /// 화면 바탕을 칠하되 Application 영역(1590x915)만 도려낸 배경판. 창이
    /// <c>AllowsTransparency</c> + <c>Background=Transparent</c> 라서, 이 판이 칠하지
    /// 않은 자리가 곧 뚫린 자리다. Grid 의 <c>Background</c> 로는 사각형 하나밖에
    /// 못 칠하니 도형으로 뺀다 - <c>GeometryCombineMode.Exclude</c> 가 하는 일이
    /// Win32 의 <c>CombineRgn(RGN_DIFF)</c> 와 같다.
    ///
    /// 캔버스보다 <see cref="BackdropOverscan"/> 만큼 크게 그리는 이유 : 창 비율이
    /// 16:9 가 아니면 Viewbox 가 위아래(또는 좌우)에 레터박스 여백을 남기는데, 창
    /// 배경이 투명이라 그 여백까지 뚫려 보인다. Grid/Viewbox 는 자식을 잘라내지
    /// 않으므로(ClipToBounds 기본값 false) 넘치게 그려 그 여백을 덮는다.
    ///
    /// 좌표를 음수로 두지 않으려고 도형은 전부 양수로 그리고 판 자체를 음수 Margin
    /// 으로 밀어낸다 - 도형 좌표계와 레이아웃 좌표계를 섞지 않기 위해서다.
    ///
    /// <c>IsHitTestVisible</c> 을 끄지 않는다 : 이 판이 창 드래그
    /// (<c>OnWindowMouseLeftButtonDown</c>)의 히트 타겟이다. 칠하지 않은 Application
    /// 영역은 자연히 히트 대상이 아니라서 클릭이 그대로 통과한다.
    /// </summary>
    private Path BuildVoidBackdrop()
    {
        var outer = new RectangleGeometry(new Rect(
            0,
            0,
            CanvasWidth + BackdropOverscan * 2,
            CanvasHeight + BackdropOverscan * 2));

        var hole = new RectangleGeometry(new Rect(
            BackdropOverscan + AppX,
            BackdropOverscan + AppY,
            AppWidth,
            AppHeight));

        return new Path
        {
            Fill = (Brush)_window.FindResource("VoidBrush"),
            Data = new CombinedGeometry(GeometryCombineMode.Exclude, outer, hole),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(-BackdropOverscan, -BackdropOverscan, 0, 0),
        };
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
