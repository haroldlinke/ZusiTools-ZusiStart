using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZusiStart.Controls
{
    public enum SortMode
    {
        None,
        Ascending,
        Descending
    }

    public class SortButton : Button
    {
        public static readonly DependencyProperty SortModeProperty = DependencyProperty.Register(
            "SortMode",
            typeof(SortMode),
            typeof(SortButton),
            new PropertyMetadata(SortMode.None, OnSortModeChanged));
        public SortMode SortMode
        {
            get => (SortMode)GetValue(SortModeProperty);
            set => SetValue(SortModeProperty, value);
        }

        public static readonly RoutedEvent SortModeChangedEvent = EventManager.RegisterRoutedEvent(
                "SortModeChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(SortButton));

        static SortButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SortButton), new FrameworkPropertyMetadata(typeof(SortButton)));
        }

        public SortButton()
        {
            Click += SortButton_Click;
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            int sm = (int)SortMode;
            SortMode = (++sm) > (int)SortMode.Descending ? SortMode.None : (SortMode)sm;
        }

        private static void OnSortModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SortButton)?.OnSortModeChanged(/*(SortMode)e.NewValue*/);
        }

        private void OnSortModeChanged(/*SortMode value*/)
        {
            RaiseEvent(new RoutedEventArgs(SortModeChangedEvent, this));
        }
    }
}
