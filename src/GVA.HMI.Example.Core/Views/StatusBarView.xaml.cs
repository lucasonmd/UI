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

    /// <summary>커스텀 전시정보는 Key 별 차지 비율(Ratio)이 JSON 에서 오기 때문에, XAML 의
    /// 정적 ColumnDefinition 으로는 표현할 수 없다 - 항목이 바뀔 때마다 여기서
    /// CustomInfoHost 의 열을 통째로 다시 만든다.</summary>
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

            // 제목은 왼쪽 끝, 값은 오른쪽 끝. 필드가 자기 비율 열을 꽉 채우므로
            // 둘 사이가 열 너비만큼 벌어져 이름/값 쌍이 붙어 보이지 않는다.
            var isLast = i == vm.CustomItems.Count - 1;
            var cell = new Grid
            {
                VerticalAlignment = VerticalAlignment.Center,
                // 구분선은 열 경계(왼쪽 끝)에 놓이므로, 앞 칸의 값과 뒤 칸의 제목이 그
                // 선에서 좌우 18px 로 똑같이 떨어지게 잡는다. 마지막 칸 오른쪽만 바 끝 여백(14).
                Margin = new Thickness(19, 0, isLast ? 14 : 18, 0),
            };
            cell.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            cell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var name = new TextBlock
            {
                Style = (Style)FindResource("StatusLabelStyle"),
                Margin = new Thickness(0, 0, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            name.SetBinding(TextBlock.TextProperty, new Binding(nameof(CustomStatusItemViewModel.Name)) { Source = item });

            // 임의의 한글/영문 값이라 mono 가 아닌 CustomValueStyle 을 쓴다(mono 엔
            // 한글 글리프가 없어 자동 대체 폰트로 튀어 폰트가 섞여 보인다).
            var value = new TextBlock
            {
                Style = (Style)FindResource("CustomValueStyle"),
                HorizontalAlignment = HorizontalAlignment.Right,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            value.SetBinding(TextBlock.TextProperty, new Binding(nameof(CustomStatusItemViewModel.Value)) { Source = item });
            value.SetBinding(TextBlock.ForegroundProperty, new Binding(nameof(CustomStatusItemViewModel.ValueBrush)) { Source = item });

            Grid.SetColumn(name, 0);
            Grid.SetColumn(value, 1);
            cell.Children.Add(name);
            cell.Children.Add(value);

            Grid.SetColumn(cell, i);
            CustomInfoHost.Children.Add(cell);

            // 필드는 테두리·배경 없이 헤어라인 하나로만 나눈다 - 칸마다 상자를 두르면
            // 하나로 이어진 면이 아니라 카드가 떠 있는 것처럼 보인다. 첫 칸도 그리는 건
            // 그 선이 앞의 HDG 와의 경계까지 겸해, 구분선 간격이 전부 같아지기 때문이다.
            var divider = new Border
            {
                Style = (Style)FindResource("FieldDividerStyle"),
                HorizontalAlignment = HorizontalAlignment.Left,
                // 스타일의 좌우 여백(18)은 StackPanel 에 줄 세울 때 쓰는 값이다.
                // 여기서는 열 경계에 정확히 앉혀야 하므로 0 으로 덮는다.
                Margin = new Thickness(0),
            };
            Grid.SetColumn(divider, i);
            CustomInfoHost.Children.Add(divider);
        }
    }
}
