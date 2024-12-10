using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ZusiStart.Controls
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register(
            "From",
            typeof(double),
            typeof(GridLengthAnimation),
            new PropertyMetadata(0.0));
        public double From
        {
            get => (double)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register(
            "To",
            typeof(double),
            typeof(GridLengthAnimation),
            new PropertyMetadata(0.0));
        public double To
        {
            get => (double)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public GridLengthAnimation()
        { }

        public GridLengthAnimation(double to, Duration duration)
        {
            To = to;
            Duration = duration;
        }

        public GridLengthAnimation(double from, double to, Duration duration)
        {
            From = from;
            To = to;
            Duration = duration;
        }

        public override Type TargetPropertyType => typeof(GridLength);

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (From > To)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (From - To) + To, GridUnitType.Star);
            }
            else
            {
                return new GridLength(animationClock.CurrentProgress.Value * (To - From) + From, GridUnitType.Star);
            }
        }

        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();
    }
}
