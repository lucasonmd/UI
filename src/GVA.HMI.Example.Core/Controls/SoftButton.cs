using System.Windows;
using System.Windows.Controls.Primitives;

namespace HMICore.Controls;

/// <summary>
/// F1~F20 소프트키 라벨. 이름은 기존에 쓰던 외부 규약을 따른다(원래 "SoftKey" 였는데
/// <see cref="SoftButtonState"/> 와 짝이 맞도록, 그리고 호출부 쪽 <c>Dictionary&lt;string,
/// SoftButton&gt;</c> 선언과 타입이 맞도록 "SoftButton" 으로 바꿨다).
///
/// 화면 요소가 아니라 베젤의 <b>물리 버튼</b>에 대응하는 라벨이다(규격서 §3).
/// 맞춰야 하는 것은 중심선이지 크기가 아니므로, 화면 라벨은 물리 버튼보다 크게 그린다.
///
/// 시각 표현은 Themes/GvaStyles.xaml 의 스타일이 담당한다.
///   SideSoftButtonStyle       - F1~F6  (153 x 90)
///   SideSoftButtonRightStyle  - F7~F12 (153 x 90, 라벨 우측 정렬)
///   BottomSoftButtonStyle     - F13~F20 (214 x 48)
///
/// <see cref="Label"/>/<see cref="State"/> 는 DependencyProperty 라서 XAML/바인딩으로도
/// 쓸 수 있지만, 기존 프로젝트 쪽 호출부와 맞추기 위해 <see cref="SetLabel"/>/
/// <see cref="SetState"/> 라는 명령형 메서드도 같이 둔다 - 둘 다 결국 같은
/// DependencyProperty 를 바꾸므로 즉시 화면에 반영된다(속성 접근이든 메서드 호출이든
/// 결과는 같다, 편한 쪽을 쓰면 된다).
/// </summary>
public class SoftButton : ButtonBase
{
    public static readonly DependencyProperty KeyIdProperty =
        DependencyProperty.Register(nameof(KeyId), typeof(string), typeof(SoftButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(SoftButton),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(SoftButtonState), typeof(SoftButton),
            new FrameworkPropertyMetadata(SoftButtonState.SoftButtonEnabled));

    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(SoftButton),
            new PropertyMetadata(string.Empty));

    /// <summary>버튼 각인 문자열이자 식별 키. "F1" ~ "F20". 우측 상단에 mono 로 표시되고,
    /// <see cref="Controls.CoreUiBuilder.SideButtons"/>/<see cref="Controls.CoreUiBuilder.BottomButtons"/>
    /// 의 Dictionary 키로도 그대로 쓰인다.</summary>
    public string KeyId
    {
        get => (string)GetValue(KeyIdProperty);
        set => SetValue(KeyIdProperty, value);
    }

    /// <summary>기능 라벨. F1~F12 는 좌/우 정렬, F13~F20 은 가운데 정렬, 둘 다 최대 2줄.</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>SoftButtonHidden / SoftButtonEnabled / SoftButtonDisabled / SoftButtonSelected.</summary>
    public SoftButtonState State
    {
        get => (SoftButtonState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <summary>같은 이름을 가진 키끼리 하나만 Selected 로 유지된다. 비우면 배타 동작 없음.</summary>
    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    /// <summary>버튼 텍스트를 갱신한다. <see cref="Label"/> 속성을 그대로 설정하는 것과 같다.</summary>
    public void SetLabel(string label) => Label = label;

    /// <summary>버튼 상태를 갱신한다. <see cref="State"/> 속성을 그대로 설정하는 것과 같다 -
    /// DependencyProperty 라 호출 즉시 UI(배경·글자색·해치·표시 여부)에 반영된다.</summary>
    public void SetState(SoftButtonState state) => State = state;

    /// <summary>Hidden/Disabled 상태에서는 눌러도 Click 이 발생하지 않는다.</summary>
    protected override void OnClick()
    {
        if (State is SoftButtonState.SoftButtonDisabled or SoftButtonState.SoftButtonHidden)
        {
            return;
        }

        base.OnClick();
    }
}
