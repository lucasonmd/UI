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

            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var name = new TextBlock { Style = (Style)FindResource("StatusLabelStyle"), Margin = new Thickness(0, 0, 12, 0) };
            name.SetBinding(TextBlock.TextProperty, new Binding(nameof(CustomStatusItemViewModel.Name)) { Source = item });

            var value = new TextBlock { Style = (Style)FindResource("CustomValueStyle") };
            value.SetBinding(TextBlock.TextProperty, new Binding(nameof(CustomStatusItemViewModel.Value)) { Source = item });
            value.SetBinding(TextBlock.ForegroundProperty, new Binding(nameof(CustomStatusItemViewModel.ValueBrush)) { Source = item });

            content.Children.Add(name);
            content.Children.Add(value);

            var panel = new Border
            {
                Style = (Style)FindResource("CustomStatusItemStyle"),
                HorizontalAlignment = HorizontalAlignment.Right,
                Child = content,
            };

            Grid.SetColumn(panel, i);
            CustomInfoHost.Children.Add(panel);
        }
    }
}
