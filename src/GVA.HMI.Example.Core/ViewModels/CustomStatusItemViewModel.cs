using System.Windows.Media;

namespace HMICore.ViewModels;

/// <summary>
/// 커스텀 전시정보 한 칸 - JSON 의 Key 하나에 대응한다. 현재 언어 모드로 이미
/// 해석된 <see cref="Name"/>/<see cref="Value"/>/<see cref="ValueBrush"/> 를 들고
/// 있어서, XAML 은 언어 전환 로직을 몰라도 된다(<see cref="StatusBarViewModel.SetLanguage"/>
/// 가 재계산해서 다시 밀어 넣는다).
/// </summary>
public sealed class CustomStatusItemViewModel : ObservableObject
{
    private string _name = string.Empty;
    private string _value = string.Empty;
    private Brush _valueBrush = Brushes.White;

    /// <summary>JSON 의 Key. 표시용이 아니라 식별용.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>커스텀 영역 안에서 이 항목이 차지하는 가로 비율(Star 크기로 그대로 쓴다).</summary>
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
