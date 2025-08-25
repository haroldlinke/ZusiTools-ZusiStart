using Sovoma.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ZusiStart.Controls
{
  public class TrainsView : TreeView
  {
    private static readonly Mutex _animateMutex = new();

    private int _column;
    private Grid _parentGrid;
    private ScrollViewer _scrollViewer;

    public TrainsView()
    {
      MouseEnter += TrainsView_MouseEnter;
      Loaded += TrainsView_Loaded;
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      _scrollViewer = Template.FindName("_tv_scrollviewer_", this) as ScrollViewer;
    }

    private void TrainsView_MouseEnter(object sender, MouseEventArgs e)
    {
      try
      {

        if (_parentGrid != null && _scrollViewer != null && _scrollViewer.ScrollableWidth != 0)
        {
          if (_animateMutex.WaitOne(300))
          {
            ColumnDefinition cdA = _parentGrid.ColumnDefinitions[_column];
            ColumnDefinition cdB = _parentGrid.ColumnDefinitions[1 - _column];
            GridLength length = cdA.Width;
            if (length.IsAuto)
            {
              throw new NotSupportedException("Don't how to convert GridLength.Auto to GridLength.Pixel");
            }

            double totalWidth = ActualWidth / length.Value;
            double desiredWidth = ActualWidth + _scrollViewer.ScrollableWidth;
            double starWidth = Math.Min(desiredWidth / totalWidth, 1.0);

            GridLengthAnimation gla1 = new(length.Value, starWidth, TimeSpan.FromMilliseconds(200));

            length = cdB.Width;
            if (length.IsAuto)
            {
              throw new NotSupportedException("Don't how to convert GridLength.Auto to GridLength.Pixel");
            }

            GridLengthAnimation gla2 = new(length.Value, 1 - starWidth, TimeSpan.FromMilliseconds(200));
            gla2.Completed += (s, a) => _animateMutex.ReleaseMutex();

            cdA.BeginAnimation(ColumnDefinition.WidthProperty, gla1);
            cdB.BeginAnimation(ColumnDefinition.WidthProperty, gla2);
          }
        }
      }
      catch (Exception ex)
      {

      }
    }

    private void TrainsView_Loaded(object sender, RoutedEventArgs e)
    {
      _parentGrid = this.GetVisualAncestor<Grid>();
      if (_parentGrid != null)
      {
        _column = Grid.GetColumn(this);
      }
    }
  }
}
