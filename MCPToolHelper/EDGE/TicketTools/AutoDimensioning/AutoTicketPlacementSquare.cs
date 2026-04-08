// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoTicketPlacementSquare
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class AutoTicketPlacementSquare
{
  public double projectedX { get; set; }

  public double projectedY { get; set; }

  public XYZ Left { get; set; }

  public XYZ Right { get; set; }

  public XYZ Center { get; set; }

  public SquarePlacement side { get; set; }

  public int layer { get; set; }

  public bool bOccupied { get; set; }

  public double width { get; set; }

  public List<AutoTicketTag> TagList { get; set; }

  public AutoTicketPlacementSquare(
    View v,
    XYZ Center_PT,
    double width,
    int layer,
    SquarePlacement side)
  {
    this.TagList = new List<AutoTicketTag>();
    this.width = width;
    this.Center = Center_PT;
    this.Left = this.Center - v.RightDirection * width / 2.0;
    this.Right = this.Center + v.RightDirection * width / 2.0;
    this.projectedX = v.RightDirection.DotProduct(this.Center);
    this.projectedY = v.UpDirection.DotProduct(this.Center);
    this.layer = layer;
    this.side = side;
    this.bOccupied = false;
  }
}
