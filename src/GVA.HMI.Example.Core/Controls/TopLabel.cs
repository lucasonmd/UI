using System.Windows;
using System.Windows.Controls.Primitives;

namespace HMICore.Controls;

public class TopLabel : ButtonBase
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(T_Button_State), typeof(TopLabel),
            new FrameworkPropertyMetadata(T_Button_State.LabelStateEnabled));

    public static readonly DependencyProperty MultiStateProperty =
        DependencyProperty.Register(nameof(MultiState), typeof(string), typeof(TopLabel),
            new PropertyMetadata(string.Empty));

    public T_Button_State State
    {
        get => (T_Button_State)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string MultiState
    {
        get => (string)GetValue(MultiStateProperty);
        set => SetValue(MultiStateProperty, value);
    }

    public void SetLabelState(T_Button_State state, string multiState)
    {
        State = state;
        MultiState = multiState;
    }

    protected override void OnClick()
    {
        if (State == T_Button_State.LabelStateDisabled)
        {
            return;
        }

        base.OnClick();
    }
}
