// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoTicketTag
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class AutoTicketTag
{
  public XYZ placementPoint { get; set; }

  public XYZ elbowPoint { get; set; }

  public XYZ endPoint { get; set; }

  public IndependentTag tag { get; set; }

  public BoundingBoxXYZ tagBBox { get; set; }

  public double width { get; set; }

  public double height { get; set; }

  public CalloutQuadrant quadrant { get; set; }

  public FamilySymbol leftFamily { get; set; }

  public FamilySymbol rightFamily { get; set; }

  public View view { get; set; }

  public AutoTicketPlacementSquare square { get; set; }

  public DimensionEdge side { get; set; }

  public Reference referenceToTaggedElement { get; set; }

  public double GetProjected(bool bVertical = false)
  {
    return (bVertical ? this.view.UpDirection : this.view.RightDirection).DotProduct(this.endPoint);
  }

  public AutoTicketTag()
  {
  }

  public AutoTicketTag(AutoTicketTag tag)
  {
    this.elbowPoint = tag.elbowPoint;
    this.square = tag.square;
    this.side = tag.side;
  }
}
