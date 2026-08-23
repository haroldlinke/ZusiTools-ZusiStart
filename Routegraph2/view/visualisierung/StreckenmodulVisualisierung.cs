using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiCLIProject.Routegraph2;
using ZusiStart.Data;
using ZusiStart.KlLib2;
using Color = System.Windows.Media.Color;

namespace ZusiCLIProject.Routegraph2
{
  public class StreckenmodulSegmentierer : Segmentierer
  {
    protected override bool IstSegmentGrenze(Strecke.ElementInfo vorgaenger, Strecke.ElementInfo nachfolger)
    {
      return false;
    }
  }
  public class StreckenmodulVisualisierung : Visualisierung
  {

    public override void SetzeDarstellung(StreckensegmentItem item)
    {
      int farbnummer = 0;


      int Optstatus = (int)item.Start.ParentBuffer.GetOptimisationStatus();
      if (Optstatus < 0)
        Optstatus = 0;
      else if (Optstatus > 6)
        Optstatus = 6;

      if (farben_.TryGetValue(Optstatus, out var value))
        item.Stroke = new SolidColorBrush(value.Item2);
      else
        item.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
      item.StrokeDashArray = null;
      if ((item.Start == OhneAlterPseudoelement))
        item.StrokeDashArray = new DoubleCollection(new double[] { 1, 2 }); //new DoubleCollection(new double[] { 5, 5 });
      else
        item.StrokeDashArray = null;

    }

    public override Segmentierer Segmentierer { get { return new AlterSegmentierer(); } }
    public override System.Windows.Controls.Canvas Legende
    {
      get
      {
        System.Windows.Controls.Canvas result = new System.Windows.Controls.Canvas();
        var segmentierer = Segmentierer;
        Strecke.ElementInfo pseudoelement = new();
        pseudoelement.ParentBuffer = new Strecke.Element();
        pseudoelement.ParentBuffer.GreenDirectionInfoIfSetOnZusi = pseudoelement;
        foreach (var it in farben_)
        {
          pseudoelement.ParentBuffer.SetOptimisationStatus(it.Key);
          NeuesLegendeElement(result, segmentierer, pseudoelement, it.Value.Item1);
        }
        //NeuesLegendeElement(result, segmentierer, OhneAlterPseudoelement, "gestrichelt = Alter unbekannt");
        return result;
      }
    }

    private static readonly Dictionary<int, Tuple<string, Color>> farben_;
    public static Strecke.ElementInfo OhneAlterPseudoelement { get; private set; } = new();
    static StreckenmodulVisualisierung()
    {
      OhneAlterPseudoelement.ParentBuffer = new Strecke.Element();
      OhneAlterPseudoelement.ParentBuffer.GreenDirectionInfoIfSetOnZusi = OhneAlterPseudoelement;
      OhneAlterPseudoelement.ParentBuffer.SetOptimisationStatus(0);
      //OhneFahrleitungPseudoelement.ParentBuffer.Drahthoehe = 0;

      farben_ = new Dictionary<int, Tuple<string, Color>>
            {
                { 0, new Tuple<string, Color>("Streckenmodul in optimiertem Fahrplan enthalten", Color.FromRgb(255, 0, 0)) },
                { 1, new Tuple<string, Color>("andere Streckenmodule", Colors.Black) }
            };
    }

  }
}
