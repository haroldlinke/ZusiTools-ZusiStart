using ZusiCLIProject.Routegraph2;
using ZusiCLIProject.FileLibrary.Zusi3;
using Color = System.Windows.Media.Color;
using System;
using System.Windows.Media;
using System.Collections.Generic;
using ZusiStart.KlLib2;

namespace ZusiCLIProject.Routegraph2
{
  public class AlterSegmentierer : Segmentierer
  {
    protected override bool IstSegmentGrenze(Strecke.ElementInfo vorgaenger, Strecke.ElementInfo nachfolger)
    {
      return false; // vorgaenger.ParentBuffer.GetAlter() != nachfolger.ParentBuffer.GetAlter();
    }
  }
  public class AlterVisualisierung : Visualisierung
  {

    public override void SetzeDarstellung(StreckensegmentItem item)
    {
      int alter = (int)item.Start.ParentBuffer.GetAlter();
      if (alter < 0)
        alter = 0;
      else if (alter > 6)
        alter = 6;

      if (farben_.TryGetValue(alter, out var value))
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
          pseudoelement.ParentBuffer.SetAlter(it.Key);
          NeuesLegendeElement(result, segmentierer, pseudoelement, it.Value.Item1);
        }
        //NeuesLegendeElement(result, segmentierer, OhneAlterPseudoelement, "gestrichelt = Alter unbekannt");
        return result;
      }
    }

    private static readonly Dictionary<int, Tuple<string, Color>> farben_;
    public static Strecke.ElementInfo OhneAlterPseudoelement { get; private set; } = new();
    static AlterVisualisierung()
    {
      OhneAlterPseudoelement.ParentBuffer = new Strecke.Element();
      OhneAlterPseudoelement.ParentBuffer.GreenDirectionInfoIfSetOnZusi = OhneAlterPseudoelement;
      OhneAlterPseudoelement.ParentBuffer.SetAlter(0);
      //OhneFahrleitungPseudoelement.ParentBuffer.Drahthoehe = 0;
      
      farben_ = new Dictionary<int, Tuple<string, Color>>
            {
                { 0, new Tuple<string, Color>("0-2 Monate", Color.FromRgb(255, 0, 0)) },
                { 1, new Tuple<string, Color>("3-5 Monate", Color.FromRgb(203, 102, 0)) },
                { 2, new Tuple<string, Color>("6-8 Monate", Color.FromRgb(0, 255, 0)) },
                { 3, new Tuple<string, Color>("9-11 Monate", Color.FromRgb(0, 203, 102)) },
                { 4, new Tuple<string, Color>("12-14 Monate", Color.FromRgb(0, 102, 203)) },
                { 5, new Tuple<string, Color>("15-17 Monate", Color.FromRgb(0, 0, 255)) },
                { 6, new Tuple<string, Color>(">18 Monate", Colors.Black) }
            };
      //farben_ = new Dictionary<int, Tuple<string, Color>>
      //      {
      //          { 0, new Tuple<string, Color>("1-3 Monate", Colors.Black) },
      //          { 1, new Tuple<string, Color>("4-6 Monate", Color.FromRgb(128, 128, 128)) },
      //          { 2, new Tuple<string, Color>("5-9 Monate", Color.FromRgb(0, 203, 102)) },
      //          { 3, new Tuple<string, Color>("10-12 Monate", Color.FromRgb(255, 0, 0)) },
      //          { 4, new Tuple<string, Color>("13-15 Monate", Color.FromRgb(0, 152, 203)) },
      //          { 5, new Tuple<string, Color>("16-18 Monate", Color.FromRgb(0, 122, 203)) },
      //          { 6, new Tuple<string, Color>(">18 Monate", Color.FromRgb(0, 0, 255)) }
      //      };
    }

  }
}
