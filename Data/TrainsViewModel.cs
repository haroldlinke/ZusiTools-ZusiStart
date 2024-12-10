using Sovoma;
using Sovoma.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;

namespace ZusiStart.Data
{
    //=========================================================================
    public class TrainsViewModel : BaseTreeViewViewModel<TrainsViewModel, Zug>
    {
        private readonly bool _hasSortButton;
        private readonly bool _isDecoTrain;
        private readonly DateTime? _startTime;
        private readonly TimeSpan? _journeyTime;

        public bool HasSortButton { get => _hasSortButton; }
        public bool IsDecoTrain { get => _isDecoTrain; }
        public TimeSpan? JourneyTime { get => _journeyTime; }
        public DateTime? StartTime { get => _startTime; }

        //---------------------------------------------------------------------
        public static TrainsViewModel BuildViewModel(TimeTable timeTable)
        {
            TrainsViewModel root = new TrainsViewModel("root");

            DataManager dataManager = DataManager.Instance;

            foreach (var g in dataManager.AllTrains
                .Where(z => z.BelongsToTimeTable == timeTable.ID)
                .GroupBy(z => z.Type)
                .Select(grp => new { TypeID = grp.Key, Members = grp.ToList() }))
            {
                TrainsViewModel tvm = new TrainsViewModel(g.TypeID);

                foreach (var mg in g.Members.GroupBy(zz => zz.FahrplanGruppe)
                    .Select(grp => new
                    {
                        GroupID = grp.Key,
                        Members = grp.OrderBy(o =>
                        {
                            FahrplanEintrag? fpe = null;
                            try
                            {
                                fpe = o.FahrplanEintraege.First(fe => fe.Departure != null);
                            }
                            catch { }
                            return fpe != null ? fpe.Departure.Value : default(DateTime);
                        }).ToList()
                    })
                    .OrderBy(o => o.GroupID))
                {
                    TrainsViewModel gvm = new TrainsViewModel(mg.GroupID);

                    mg.Members.ForEach(m =>
                    {
                        gvm.Children.Add(new TrainsViewModel(m));
                    });

                    if (gvm.Children.Count > 0)
                    {
                        tvm.Children.Add(gvm);
                    }
                }

                if (tvm.Children.Count > 0)
                {
                    root.Children.Add(tvm);
                }
            }

            root.Initialize();
            return root;
        }

        //---------------------------------------------------------------------
        public static TrainsViewModel BuildViewModel(List<Zug> trains)
        {
            TrainsViewModel root = new TrainsViewModel("root");

            DataManager dataManager = DataManager.Instance;

            foreach (var g in trains.GroupBy(z => z.Type)
                .Select(grp => new { TypeID = grp.Key, Members = grp.ToList() }))
            {
                TrainsViewModel tvm = new TrainsViewModel(g.TypeID);

                foreach (var mg in g.Members.GroupBy(zz => zz.FahrplanGruppe)
                    .Select(grp => new
                    {
                        GroupID = grp.Key,
                        Members = grp.OrderBy(o =>
                        {
                            FahrplanEintrag? fpe = null;
                            try
                            {
                                fpe = o.FahrplanEintraege.First(fe => fe.Departure != null);
                            }
                            catch { }
                            return fpe != null ? fpe.Departure.Value : default(DateTime);
                        }).ToList()
                    })
                    .OrderBy(o => o.GroupID))
                {
                    TrainsViewModel gvm = new TrainsViewModel(mg.GroupID);

                    mg.Members.ForEach(m =>
                    {
                        gvm.Children.Add(new TrainsViewModel(m));
                    });

                    if (gvm.Children.Count > 0)
                    {
                        tvm.Children.Add(gvm);
                    }
                }

                if (tvm.Children.Count > 0)
                {
                    root.Children.Add(tvm);
                }
            }

            root.Initialize();
            return root;
        }

        //---------------------------------------------------------------------
        public TrainsViewModel(TrainsViewModel parent, bool expanded, bool selected)
            : base(parent, expanded, selected)
        { }

        //---------------------------------------------------------------------
        private TrainsViewModel(string caption)
            : this(null, true, false)
        {
            _displayName = caption?.Trim();
            _hasSortButton = true;
            IsBold = true;
        }

        //---------------------------------------------------------------------
        private TrainsViewModel(TrainType type)
            : base(null, true, false)
        {
            _displayName = type == TrainType.Freight ? "Güterzüge" : "Personenzüge";
            IsBold = true;
        }

        //---------------------------------------------------------------------
        private TrainsViewModel(Zug zug)
            : this(null, true, false)
        {
            // Abfahrtszeit
            _startTime = zug.StartTime;
            // Gattung/Nummer
            _displayName = string.IsNullOrEmpty(zug.Gattung) ? zug.Nummer : string.Format("{0} {1}", zug.Gattung, zug.Nummer);
            // Fahrzeit
            _journeyTime = zug.JourneyTime;
            // object
            _object = zug;
            _isDecoTrain = zug.IsDecoTrain;
        }
    }

    [ValueConversion(typeof(TimeSpan?), typeof(string))]
    public class JourneyTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                return string.Format("{0} Min.", Math.Round(ts.TotalMinutes));
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime?), typeof(string))]
    public class StartTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                return dt.ToString("HH:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ViewSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CollectionViewSource cvs = new CollectionViewSource() { Source = value };
            cvs.Filter += Cvs_Filter;
            return cvs.View;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private void Cvs_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is TrainsViewModel tvm)
            {
                e.Accepted = tvm.Object is Zug z ? !z.IsDecoTrain || DataManager.Instance.IsDecoTrainsAllowed : true;
            }
            else
            {
                e.Accepted = false;
            }
        }
    }
}
