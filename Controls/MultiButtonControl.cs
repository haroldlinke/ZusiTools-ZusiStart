using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZusiStart.Controls
{
    public class MultiButtonControl : ItemsControl
    {
        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
            "ButtonStyle",
            typeof(Style),
            typeof(MultiButtonControl),
            new PropertyMetadata(null));
        public Style ButtonStyle
        {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(MultiButtonControl),
            new PropertyMetadata(new CornerRadius(6)));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static MultiButtonControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiButtonControl), new FrameworkPropertyMetadata(typeof(MultiButtonControl)));
        }
    }

    public class MultiButtonItem : DependencyObject, ICommandSource
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(MultiButtonItem),
            new PropertyMetadata(false));
        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(MultiButtonItem),
            new PropertyMetadata(false, OnIsSelectedChanged));
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(MultiButtonItem),
            new PropertyMetadata(null, OnCommandChanged));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(MultiButtonItem),
            new PropertyMetadata(null));
        public object CommandParameter
        {
            get => (object)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(MultiButtonItem),
            new PropertyMetadata(null));
        public IInputElement CommandTarget
        {
            get => (IInputElement)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(string),
            typeof(MultiButtonItem),
            new PropertyMetadata(null));
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MultiButtonItem)?.OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        private void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null)
            {
                oldCommand.CanExecuteChanged -= CanExecuteChanged;
            }
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += CanExecuteChanged;
            }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MultiButtonItem)?.OnIsSelectedChanged((bool)e.NewValue);
        }

        private void OnIsSelectedChanged(bool value)
        {
            if (value)
            {
                if (Command is RoutedCommand c)
                {
                    c.Execute(CommandParameter, CommandTarget);
                }
            }
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (Command is RoutedCommand c)
            {
                IsEnabled = c.CanExecute(CommandParameter, CommandTarget);
            }
            else
            {
                IsEnabled = Command.CanExecute(CommandParameter);
            }
        }
    }
}
