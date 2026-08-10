using System.Windows.Media;

namespace HMICore.ViewModels;

public sealed class CustomStatusItemViewModel : ObservableObject
{
    private string _name = string.Empty;
    private string _value = string.Empty;
    private Brush _valueBrush = Brushes.White;

    public string Key { get; init; } = string.Empty;

    public double Ratio { get; init; } = 1;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public Brush ValueBrush
    {
        get => _valueBrush;
        set => SetProperty(ref _valueBrush, value);
    }
}
