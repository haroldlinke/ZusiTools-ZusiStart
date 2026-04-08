using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZusiKlassenLib2.Cab;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Vehicle;

namespace ZusiStart.Data
{
  public class VehicleVariant : DependencyObject
  {
    private readonly FahrzeugVariante _fahrzeugVariante;
    private readonly ObservableCollection<Indusi> _indusis = new();

    public string Class => GetClass();

    public string Description => _fahrzeugVariante?.Beschreibung;

    public string Drivetrain => _fahrzeugVariante.Drivetrain?.Name;

    public int IDHaupt => _fahrzeugVariante.IDHaupt;

    public int IDNeben => _fahrzeugVariante.IDNeben;

    public string IndusiInfo => GetIndusiInfo();

    public bool IsLocoElectrical => GetLocoElectrical();

    public double Mass => _fahrzeugVariante.Masse;

    public string Name => GetName();

    public string ServiceTime => GetServiceTime();

    public string VariantID => GetVariantID();

    public ObservableCollection<Indusi> Indusis => _indusis;

    public VehicleVariant(FahrzeugVariante fahrzeugVariante)
    {
      _fahrzeugVariante = fahrzeugVariante ?? throw new ArgumentNullException("fahrzeugVariante");

      if (_fahrzeugVariante.DateiFuehrerstand != null)
      {
        try
        {
          DriversCabFile dcf = new(_fahrzeugVariante.DateiFuehrerstandVorn.FullPath);
          dcf.Parse();
          DriversCab? cab = dcf.Root;
          if (cab != null)
          {
            Funktionalitaeten ff = cab.Items;
            ff.Indusis.ForEach(i => _indusis.Add(i));
          }
        }
        catch
        {

        }
      }
    }

    public double GetBrakeWeight(Bremsstellung bs) => _fahrzeugVariante.Bremsgewicht(bs);

    private string GetClass()
    {
      string br = _fahrzeugVariante.BR;
      if (br.ToLower().StartsWith("br"))
      {
        br = br.Substring(2).Trim();
      }

      return br;
    }

    private string GetIndusiInfo()
    {
      StringBuilder sb = new();
      foreach (var v in _indusis)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        sb.Append(v.FktName);
      }
      return sb.ToString();
    }

    private bool GetLocoElectrical()
    {
      return _fahrzeugVariante.Drivetrain.DrivetrainType switch
      {
        DrivetrainType.ElectricSeriesMotor or DrivetrainType.ElectricThreePhaseMotor => true,
        _ => false,
      };
    }

    private string GetName()
    {
      return IDHaupt > 0 && IDNeben > 0 ? string.Format("{0}.{1}", IDHaupt, IDNeben) : null;
    }

    private string GetServiceTime()
    {
      DateTime? from = _fahrzeugVariante.EinsatzAb;
      DateTime? to = _fahrzeugVariante.EinsatzBis;

      string res;
      if (from == null)
      {
        if (to == null)
        {
          res = "nicht angegeben";
        }
        else
        {
          res = to.Value.ToString(@"bi\s MMM yyyy");
        }
      }
      else
      {
        if (to == null)
        {
          res = from.Value.ToString(@"\sei\t MMM yyyy");
        }
        else
        {
          res = from.Value.ToString(@"von MMM yyyy") + to.Value.ToString(@" bi\s MMM yyyy");
        }
      }

      return res;
    }

    private string GetVariantID()
    {
      if (_fahrzeugVariante.IDHaupt > 0 && _fahrzeugVariante.IDNeben > 0)
      {
        return string.Format("{0}.{1}", _fahrzeugVariante.IDHaupt, _fahrzeugVariante.IDNeben);
      }
      else
      {
        return _fahrzeugVariante.Beschreibung;
      }
    }
  }
}
