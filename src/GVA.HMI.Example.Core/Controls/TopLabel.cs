using System.Windows;
using System.Windows.Controls.Primitives;

namespace HMICore.Controls;

/// <summary>
/// 상단 기능영역 라벨 8개 전용 컨트롤(규격서 §8, 텍스트 없음). F1~F20 은
/// <see cref="SoftButton"/>/<see cref="SoftButtonState"/> 를 쓰지만, 이 라벨들은 기존에
/// 쓰던 별개의 호출 규약(<see cref="T_Button_State"/>, <see cref="SetLabelState"/>)을
/// 그대로 따른다 - 두 컨트롤을 하나로 합치지 않았다(상태 모델 자체가 다르다,
/// Left/Right 반쪽 칠하기는 F1~F20 쪽엔 없는 개념이다).
///
/// 시각 표현은 Themes/GvaStyles.xaml 의 TopLabelStyle 이 담당한다.
/// </summary>
public class TopLabel : ButtonBase
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(T_Button_State), typeof(TopLabel),
            new FrameworkPropertyMetadata(T_Button_State.LabelStateEnabled));

    public static readonly DependencyProperty MultiStateProperty =
        DependencyProperty.Register(nameof(MultiState), typeof(string), typeof(TopLabel),
            new PropertyMetadata(string.Empty));

    /// <summary>LabelStateEnabled / LabelStateDisabled / LabelStateSelected /
    /// LabelStateSelectedLeft(왼쪽 반쪽만 색칠) / LabelStateSeletedRight(오른쪽 반쪽만 색칠).</summary>
    public T_Button_State State
    {
        get => (T_Button_State)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <summary><see cref="SetLabelState"/> 의 두 번째 인자를 그대로 보관한다. 이 라벨
    /// 자체엔 텍스트가 없어서(규격서 §8, 상단 하드웨어 버튼 좌표 미확정) 지금은
    /// 화면에 쓰이지 않지만, 기존 호출부 시그니처를 그대로 받기 위해 값은 보관해
    /// 둔다 - 나중에 실제 용도가 정해지면 TopLabelStyle 에서 꺼내 쓰면 된다.</summary>
    public string MultiState
    {
        get => (string)GetValue(MultiStateProperty);
        set => SetValue(MultiStateProperty, value);
    }

    /// <summary>기존에 쓰던 호출 방식 - state 와 multiState 를 한 번에 설정하고, 즉시
    /// UI(트랙 바 색·해치 표시)에 반영한다. DependencyProperty 를 그대로 바꾸는 것과
    /// 같아서 별도의 새로고침 호출이 필요 없다.</summary>
    public void SetLabelState(T_Button_State state, string multiState)
    {
        State = state;
        MultiState = multiState;
    }

    /// <summary>Disabled 상태에서는 눌러도 Click 이 발생하지 않는다.</summary>
    protected override void OnClick()
    {
        if (State == T_Button_State.LabelStateDisabled)
        {
            return;
        }

        base.OnClick();
    }
}
