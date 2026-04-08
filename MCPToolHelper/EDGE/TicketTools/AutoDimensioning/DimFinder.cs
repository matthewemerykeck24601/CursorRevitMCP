// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.DimFinder
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class DimFinder
{
  public double _dimWidth;
  public XYZ _dimMin;
  public XYZ _dimMax;
  public Dimension _currDim;
  public XYZ _principalAxis;
  public XYZ _perpendicularAxis;
  public double _vertProj;
  public double _minWProj;
  public double _maxWProj;

  public DimFinder(Dimension dim, XYZ principalAxis, XYZ perpendicularAxis)
  {
    this._currDim = dim;
    this._principalAxis = principalAxis;
    this._perpendicularAxis = perpendicularAxis;
    double num1 = 0.0;
    if (this._currDim.NumberOfSegments == 0)
    {
      if (this._currDim.Curve is Line)
      {
        XYZ perpendicularAxis1 = this._perpendicularAxis;
        num1 = this._currDim.Value.Value;
        this._dimMin = this._currDim.Origin - perpendicularAxis1 * (num1 / 2.0);
        this._dimMax = this._currDim.Origin + perpendicularAxis1 * (num1 / 2.0);
        this._minWProj = ProjectionUtils.ProjectPointOnDirection(this._dimMin, this._perpendicularAxis);
        this._maxWProj = ProjectionUtils.ProjectPointOnDirection(this._dimMax, this._perpendicularAxis);
      }
    }
    else if (this._currDim.NumberOfSegments > 1 && this._currDim.Curve is Line)
    {
      XYZ perpendicularAxis2 = this._perpendicularAxis;
      double num2 = double.MaxValue;
      double num3 = double.MinValue;
      foreach (DimensionSegment segment in this._currDim.Segments)
      {
        double num4 = segment.Value.Value;
        num1 += num4;
        XYZ point1 = segment.Origin - perpendicularAxis2 * (num4 / 2.0);
        XYZ point2 = segment.Origin + perpendicularAxis2 * (num4 / 2.0);
        double num5 = ProjectionUtils.ProjectPointOnDirection(point1, this._perpendicularAxis);
        double num6 = ProjectionUtils.ProjectPointOnDirection(point2, this._perpendicularAxis);
        if (num5 < num2)
        {
          num2 = num5;
          this._dimMin = point1;
        }
        if (num6 > num3)
        {
          num3 = num6;
          this._dimMax = point2;
        }
      }
      this._minWProj = num2;
      this._maxWProj = num3;
    }
    if (this._currDim.Curve is Line)
      this._vertProj = ProjectionUtils.ProjectPointOnDirection((this._currDim.Curve as Line).Origin, this._principalAxis);
    this._dimWidth = num1;
  }

  public double GetVertical(double projection, bool signed = false)
  {
    return !signed ? Math.Abs(projection - this._vertProj) : projection - this._vertProj;
  }

  public bool DetectHorizontalOverlap(double minW, double maxW)
  {
    return minW <= this._minWProj && maxW >= this._maxWProj || minW >= this._minWProj && minW <= this._maxWProj || maxW >= this._minWProj && maxW <= this._maxWProj;
  }
}
