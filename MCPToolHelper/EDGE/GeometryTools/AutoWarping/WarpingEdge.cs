// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.WarpingEdge
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

public class WarpingEdge
{
  private static int _s_count;
  public ElementId p1;
  public ElementId p2;
  public bool isCurved;
  public Curve actualCV;
  public List<WarpingEdgeLoop> ContainingLoops = new List<WarpingEdgeLoop>();

  public SpotElevationPoint StartPoint { get; set; }

  public SpotElevationPoint EndPoint { get; set; }

  public int EdgeNumber { get; set; }

  public WarpingEdge() => this.EdgeNumber = WarpingEdge._s_count++;

  public WarpingEdge(SpotElevationPoint spotElevation, SpotElevationPoint nearestPoint, Curve cv = null)
  {
    this.EdgeNumber = WarpingEdge._s_count++;
    this.StartPoint = spotElevation;
    this.EndPoint = nearestPoint;
    this.p1 = this.StartPoint.elemId;
    this.p2 = this.EndPoint.elemId;
    this.actualCV = cv;
    this.isCurved = cv is Arc;
  }

  public override int GetHashCode() => this.EdgeNumber;

  public override bool Equals(object obj)
  {
    return obj is WarpingEdge warpingEdge && this.EdgeContainsPoint(warpingEdge.StartPoint) && this.GetEndPoint(warpingEdge.StartPoint).planePoint.IsAlmostEqualTo(warpingEdge.EndPoint.planePoint);
  }

  public Line GetSlopeLine() => Line.CreateBound(this.StartPoint.Point, this.EndPoint.Point);

  public Curve GetFlatCurve(
    double ZElevation,
    SpotElevationPoint startingPoint0,
    SpotElevationPoint startingPoint1)
  {
    int num = startingPoint0.planePoint.IsAlmostEqualTo(this.StartPoint.planePoint) ? 0 : (!startingPoint1.planePoint.IsAlmostEqualTo(this.StartPoint.planePoint) ? 1 : 0);
    return (Curve) Line.CreateBound((num != 0 ? this.EndPoint.planePoint : this.StartPoint.planePoint) + XYZ.BasisZ * ZElevation, (num != 0 ? this.StartPoint.planePoint : this.EndPoint.planePoint) + XYZ.BasisZ * ZElevation);
  }

  public Curve GetFlatCurve(double ZElevation = 0.0)
  {
    return (Curve) Line.CreateBound(this.StartPoint.planePoint, this.EndPoint.planePoint);
  }

  public XYZ GetDirection(SpotElevationPoint startingPoint)
  {
    return !startingPoint.planePoint.IsAlmostEqualTo(this.StartPoint.planePoint) ? (this.StartPoint.planePoint - this.EndPoint.planePoint).Normalize() : (this.EndPoint.planePoint - this.StartPoint.planePoint).Normalize();
  }

  public List<WarpingEdge> GetEdgesConnectedToEnd(SpotElevationPoint startingPoint)
  {
    return !startingPoint.planePoint.IsAlmostEqualTo(this.StartPoint.planePoint) ? this.StartPoint.ConnectedEdges : this.EndPoint.ConnectedEdges;
  }

  public SpotElevationPoint GetEndPoint(SpotElevationPoint startPoint)
  {
    return !this.StartPoint.planePoint.IsAlmostEqualTo(startPoint.planePoint) ? this.StartPoint : this.EndPoint;
  }

  public bool EdgeContainsPoint(SpotElevationPoint spot)
  {
    return this.StartPoint.planePoint.IsAlmostEqualTo(spot.planePoint) || this.EndPoint.planePoint.IsAlmostEqualTo(spot.planePoint);
  }

  public double GetLength() => this.StartPoint.Point.DistanceTo(this.EndPoint.Point);
}
