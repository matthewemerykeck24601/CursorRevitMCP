// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.SlopeIntersection
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

public class SlopeIntersection
{
  private XYZ _slopeDirection;

  public XYZ IntersectionPoint { get; }

  public WarpingEdge IntersectedWarpingEdge { get; }

  public bool StartEdge { get; set; }

  public SlopeIntersection(WarpingEdge edge, XYZ intersectionPoint)
  {
    this.IntersectedWarpingEdge = edge;
    this.IntersectionPoint = intersectionPoint;
    this._slopeDirection = (this.IntersectedWarpingEdge.EndPoint.Point - this.IntersectedWarpingEdge.StartPoint.Point).Normalize();
  }

  public bool SetStartEdge(XYZ familyDirection, FamilyInstance famInst)
  {
    XYZ point = (famInst.Location as LocationPoint).Point;
    XYZ source = point + familyDirection.Normalize() * Parameters.GetParameterAsZeroBasedDouble((Element) famInst, "DIM_LENGTH");
    this.StartEdge = this.IntersectionPoint.DistanceTo(point) < this.IntersectionPoint.DistanceTo(source);
    return this.StartEdge;
  }

  public double GetSlopeAngle(XYZ familyDirection)
  {
    double num = familyDirection.Normalize().CrossProduct(XYZ.BasisZ).AngleOnPlaneTo(this._slopeDirection, familyDirection.Normalize().Negate());
    if (num > Math.PI / 2.0 && num < 3.0 * Math.PI / 2.0)
      num -= Math.PI;
    else if (num > 3.0 * Math.PI / 2.0)
      num -= 2.0 * Math.PI;
    return -1.0 * num;
  }
}
