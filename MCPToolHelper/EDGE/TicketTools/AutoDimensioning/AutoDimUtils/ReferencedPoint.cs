// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoDimUtils.ReferencedPoint
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning.AutoDimUtils;

public class ReferencedPoint
{
  public Reference Reference { get; set; }

  public XYZ Point { get; set; }

  public XYZ LocalPoint { get; set; }

  public ElementId elementId { get; set; }

  public ReferencedPoint()
  {
  }

  public ReferencedPoint(Reference reffy, XYZ point)
  {
    this.Reference = reffy;
    this.Point = point;
  }

  public ReferencedPoint(Reference reffy, XYZ point, XYZ localPoint)
  {
    this.Reference = reffy;
    this.Point = point;
    this.LocalPoint = localPoint;
  }

  public ReferencedPoint(Reference reffy, XYZ point, XYZ localPoint, ElementId elemId)
  {
    this.Reference = reffy;
    this.Point = point;
    this.LocalPoint = localPoint;
    this.elementId = elemId;
  }
}
