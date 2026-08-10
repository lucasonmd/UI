using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HMICore.ViewModels;

namespace HMICore.Views;

public partial class StatusBarView : UserControl
{
    public StatusBarView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is StatusBarViewModel oldVm)
        {
            oldVm.CustomItems.CollectionChanged -= OnCustomItemsChanged;
        }

        if (e.NewValue is StatusBarViewModel newVm)
        {
            newVm.CustomItems.CollectionChanged += OnCustomItemsChanged;
            RebuildCustomInfo(newVm);
        }
    }

    private void OnCustomItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is StatusBarViewModel vm)
        {
            RebuildCustomInfo(vm);
        }
    }

    /// <summary>
    /// 커스텀 전시정보는 Key 별 차지 비율(Ratio)이 JSON 에서 오기 때문에, XAML 의
    /// 정적 ColumnDefinition 으로는 표현할 수 없다 - 항목이 바뀔 때마다(언어 전환
    /// 포함) 여기서 CustomInfoHost 의 열을 통째로 다시 만든다(CoreUiBuilder 와
    /// 같은 이유로 코드에서 조립한다).
    /// </summary>
    private void RebuildCustomInfo(StatusBarViewModel vm)
    {
        CustomInfoHost.ColumnDefinitions.Clear();
        CustomInfoHost.Children.Clear();

        for (var i = 0; i < vm.CustomItems.Count; i++)
        {
            var item = vm.CustomItems[i];
            CustomInfoHost.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(item.Ratio, GridUnitType.Star),
            });

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
            };

            if (i > 0)
            {
                panel.Children.Add(new Border
                {
                    Style = (Style)FindResource("GroupDividerStyle"),
                });
            }

            var name = new TextBlock { Style = (Style)FindResource("StatusLabelStyle") };
            name.SetBinding(TextBlock.TextProperty, new Binding(nameof(CustomStatusItemViewModel.Name)) { Source = item });

            // 임의의 한글/영문 값이라 mono 가 아닌 CustomValueStyle 을 쓴다(mono 는
            // 한글 글리프가 없어서 자동 대체 폰트로 튀어 폰트가 섞여 보였다).
            var value = new TextBlock { Style = (Style)FindResource("CustomValueStyle") };
            value.SetBinding(TextBlock.TextProperty, new Binding(nameof(CustomStatusItemViewModel.Value)) { Source = item });
            value.SetBinding(TextBlock.ForegroundProperty, new Binding(nameof(CustomStatusItemViewModel.ValueBrush)) { Source = item });

            panel.Children.Add(name);
            panel.Children.Add(value);

            Grid.SetColumn(panel, i);
            CustomInfoHost.Children.Add(panel);
        }
    }
}
