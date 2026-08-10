using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GVA.HMI.Example.Application.ViewModels;

namespace GVA.HMI.Example.Application.Converters;

/// <summary>
/// <see cref="Severity"/> -> Brush. 색상값 자체는 GvaColors.xaml 에서만 정의한다.
/// </summary>
public sealed class SeverityToBrushConverter : IValueConverter
{
    /// <summary>Severity.None 일 때 쓸 브러시. 사용처에서 지정한다.</summary>
    public Brush? DefaultBrush { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = (value as Severity?) switch
        {
            Severity.Warning => "WarningBrush",
            Severity.Caution => "CautionBrush",
            Severity.Normal => "NormalBrush",
            _ => null,
        };

        if (key is null)
        {
            return DefaultBrush ?? System.Windows.Application.Current.TryFindResource("ForegroundBrush") ?? Brushes.White;
        }

        return System.Windows.Application.Current.TryFindResource(key) ?? Brushes.White;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
