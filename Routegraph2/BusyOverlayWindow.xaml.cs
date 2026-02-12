using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ZusiCLIProject.FileLibrary.Zusi3;
//using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiCLIProject.Routegraph2;
using ZusiStart.Data;


namespace Z8Routegraph2n
{
  public partial class BusyOverlayWindow : Window, INotifyPropertyChanged
  {
    private string _message = LocalizationManager.Translate("Der Streckenplan wird erstellt...");

    public string Message
    {
      get => _message;
      set
      {
        if (_message != value)
        {
          _message = value;
          OnPropertyChanged(nameof(Message));
        }
      }
    }

    public BusyOverlayWindow()
    {
      InitializeComponent();
      DataContext = this; // Bind to self
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

}
