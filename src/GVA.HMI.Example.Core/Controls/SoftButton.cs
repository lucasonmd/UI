using System.Windows;
using System.Windows.Controls.Primitives;

namespace HMICore.Controls;

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

    public string KeyId
    {
        get => (string)GetValue(KeyIdProperty);
        set => SetValue(KeyIdProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public SoftButtonState State
    {
        get => (SoftButtonState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    public void SetLabel(string label) => Label = label;

    public void SetState(SoftButtonState state) => State = state;

    protected override void OnClick()
    {
        if (State is SoftButtonState.SoftButtonDisabled or SoftButtonState.SoftButtonHidden)
        {
            return;
        }

        base.OnClick();
    }
}
