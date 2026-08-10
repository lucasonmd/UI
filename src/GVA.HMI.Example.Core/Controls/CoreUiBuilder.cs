using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HMICore.ViewModels;
using HMICore.Views;

namespace HMICore.Controls;

/// <summary>
/// GVA HMI Core UI(상단 바 + 좌우 레일 + 하단 열 + 기능영역 라벨 + 프레임)를
/// 통째로 만들어서 호스트 창에 꽂아 넣는다. 이 프로젝트 전용 Window 를 직접
/// 상속하지 않고 <see cref="Window"/> 하나만 매개변수로 받는 이유는, 기존
/// 프로젝트에 UI 만 옮겨 붙이는 상황을 전제로 하기 때문이다 - 종속적인 설계
/// (특정 Window 서브클래스·특정 App.xaml 리소스 병합 순서 등)를 최대한 피한다.
///
/// 호스트 창에는 <c>&lt;Grid x:Name="gd_main"/&gt;</c> 가 있어야 한다 - 그 안에
/// 모든 걸 만들어 넣는다(기존에 쓰던 그 이름 그대로).
///
/// 위치/크기는 "기본 형태"로 이 클래스 상단 상수에 고정돼 있다. 라벨/상태는
/// 전부 기본값(F1~F20 은 "ngva.f1".."ngva.f20" / SoftButtonEnabled, TopLabel
/// 8개는 전부 LabelStateDisabled)으로 생성되고, 실제 내용은 <see cref="SideButtons"/>/
/// <see cref="BottomButtons"/>/<see cref="TopLabels"/> 로 개별 컨트롤을 찾아
/// SetLabel/SetState/SetLabelState 를 호출해서 채운다 - 이 클래스는 그 방법을
/// 강요하지 않는다(바인딩 없이도 된다). 좌/우 레일(F1~F12)과 하단 열(F13~F20)은
/// KeyId 로 바로 찾을 수 있게 Dictionary 로 노출한다.
///
/// 클릭/키보드 같은 입력 이벤트는 여기서 등록하지 않는다 - 호출부가 직접
/// <see cref="SideButtons"/>/<see cref="BottomButtons"/>/<see cref="TopLabels"/>
/// 에서 필요한 컨트롤을 찾아 원하는 이벤트를 스스로 붙인다(ButtonBase 라 Click
/// 은 기본으로 갖고 있다).
/// </summary>
public sealed class CoreUiBuilder
{
    private const double CanvasWidth = 1920;
    private const double CanvasHeight = 1080;

    private const double AppX = 165, AppY = 105, AppWidth = 1590, AppHeight = 915;
    private const double FrameMargin = 4;
    private const double FrameCornerRadius = 14;

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

    /// <summary>좌/우 레일 = F1~F12. KeyId("F1".."F12")로 바로 찾는다.</summary>
    private readonly Dictionary<string, SoftButton> _sideButtons = new();

    /// <summary>하단 열 = F13~F20. KeyId("F13".."F20")로 바로 찾는다.</summary>
    private readonly Dictionary<string, SoftButton> _bottomButtons = new();

    private readonly List<TopLabel> _topLabels = new();

    /// <summary>좌/우 레일 소프트버튼(F1~F12), KeyId 로 조회.</summary>
    public IReadOnlyDictionary<string, SoftButton> SideButtons => _sideButtons;

    /// <summary>하단 열 소프트버튼(F13~F20), KeyId 로 조회.</summary>
    public IReadOnlyDictionary<string, SoftButton> BottomButtons => _bottomButtons;

    /// <summary>상단 기능영역 라벨 8개, 왼쪽부터 순서대로.</summary>
    public IReadOnlyList<TopLabel> TopLabels => _topLabels;

    /// <summary>SystemData/AlarmData/커스텀 전시정보(JSON)를 채우는 진입점.</summary>
    public StatusBarViewModel StatusBar { get; } = new();

    /// <summary>
    /// <paramref name="window"/> 안에서 <c>x:Name="gd_main"</c> 인 Grid 를 찾아 그
    /// 안에 Core UI 를 전부 만들어 넣는다. 호스트가 <see cref="FrameworkElement.InitializeComponent"/>
    /// (또는 그에 준하는 XAML 로딩)을 이미 마친 뒤에 호출해야 한다 - 그래야 이름이
    /// 등록돼 있어 <see cref="FrameworkElement.FindName"/> 으로 찾을 수 있다.
    /// </summary>
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

        // Application(중앙 대시보드) 자리 - 이 프로젝트에선 별도 프로세스 창이 이
        // 위에 겹쳐 앉는다(MainWindow.xaml.cs 참고). 기존 프로젝트에 옮겨 붙일 땐
        // 이 자리에 그 프로젝트의 실제 콘텐츠를 넣으면 된다 - 지금은 자리표시자뿐이다.
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

    /// <summary>
    /// 좌/우 레일과 하단 열 버튼 뒤에 까는 순검정 뒤판 - 버튼보다 먼저 넣어야
    /// Canvas 의 선언 순서(=Z-순서)상 뒤에 깔린다. 레일 뒤판 높이는 키 개수 ×
    /// 피치 + 키 높이로 자동 계산한다(레일 전체 세로 길이와 정확히 일치).
    /// </summary>
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

    /// <summary>
    /// 상단 기능영역 라벨 8개 - F1~F20 과 다른 컨트롤(TopLabel)/상태 모델
    /// (T_Button_State) 을 쓴다. 기본 상태는 전부 LabelStateDisabled - 내용은
    /// 전적으로 외부의 TopLabel.SetLabelState() 호출로 채워진다.
    /// </summary>
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

    /// <summary>F1~F12(좌/우 레일) 은 기본 라벨 "ngva.f&lt;n&gt;", 기본 상태 SoftButtonEnabled 로 생성된다.</summary>
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
