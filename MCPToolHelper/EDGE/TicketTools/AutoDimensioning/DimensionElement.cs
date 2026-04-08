// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.DimensionElement
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class DimensionElement
{
  public double horizPos;
  public double verticalPos;
  public ElementId id;
  public ReferencedPoint refP;
  public DimensionEdge dimensionEdge;

  public DimensionElement(double xPos, double yPos, ElementId eid)
  {
    this.horizPos = xPos;
    this.verticalPos = yPos;
    this.id = eid;
  }

  public DimensionElement(double xPos, double yPos, ReferencedPoint refP)
  {
    this.horizPos = xPos;
    this.verticalPos = yPos;
    this.refP = refP;
  }

  public bool Matches(DimensionElement other, DimensionEdge direction)
  {
    return direction == DimensionEdge.Top || direction == DimensionEdge.Bottom ? this.horizPos.ApproximatelyEquals(other.horizPos) : this.verticalPos.ApproximatelyEquals(other.verticalPos);
  }
}
