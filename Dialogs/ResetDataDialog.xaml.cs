using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace ZusiStart.Dialogs
{
    /// <summary>
    /// Interaktionslogik für ResetDataDialog.xaml
    /// </summary>
    public partial class ResetDataDialog : Window
    {
        //---------------------------------------------------------------------
        public static readonly DependencyProperty DeleteCacheProperty = DependencyProperty.Register(
            "DeleteCache",
            typeof(bool),
            typeof(ResetDataDialog),
            new PropertyMetadata(false));
        public bool DeleteCache
        {
            get => (bool)GetValue(DeleteCacheProperty);
            set => SetValue(DeleteCacheProperty, value);
        }

        //---------------------------------------------------------------------
        public static readonly DependencyProperty DeletePicturesProperty = DependencyProperty.Register(
            "DeletePictures",
            typeof(bool),
            typeof(ResetDataDialog),
            new PropertyMetadata(false));
        public bool DeletePictures
        {
            get => (bool)GetValue(DeletePicturesProperty);
            set => SetValue(DeletePicturesProperty, value);
        }

        //---------------------------------------------------------------------
        public static readonly DependencyProperty DeleteRecentTrainsProperty = DependencyProperty.Register(
            "DeleteRecentTrains",
            typeof(bool),
            typeof(ResetDataDialog),
            new PropertyMetadata(false));
        public bool DeleteRecentTrains
        {
            get => (bool)GetValue(DeleteRecentTrainsProperty);
            set => SetValue(DeleteRecentTrainsProperty, value);
        }

        //---------------------------------------------------------------------
        public static readonly RoutedUICommand OkCommand = new RoutedUICommand("Ok", "OkCommand", typeof(ResetDataDialog));

        //---------------------------------------------------------------------
        public ResetDataDialog()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(OkCommand, (s, e) => DialogResult = true, OnCanOk));

            Loaded += ResetDataDialog_Loaded;
        }

        //---------------------------------------------------------------------
        private void ResetDataDialog_Loaded(object sender, RoutedEventArgs e)
        {
            DeleteCache = true;
        }

        //---------------------------------------------------------------------
        private void OnCanOk(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DeleteCache || DeletePictures || DeleteRecentTrains;
        }
    }
}
