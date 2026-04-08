// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.DimPlacementCalculatorEDrawing
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AdminUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class DimPlacementCalculatorEDrawing
{
  private XYZ _principalAxis;
  private XYZ _perpendicularAxis;
  public XYZ clickPoint = new XYZ(0.0, 0.0, 0.0);
  private View _view;
  private double _textOffset = 0.5;
  private double _pointDistance;
  private Document _revitDoc;
  private Element _closestDim;
  private List<Dimension> _dimsInDirection;
  private List<TextNote> _notesInDirection;
  private DimensionEdge previousOrientation;
  private DimensionEdge currSide;

  public DimPlacementCalculatorEDrawing(XYZ principalAxis, View view, Document revitDoc)
  {
    this._principalAxis = principalAxis;
    this._view = view;
    this._revitDoc = revitDoc;
    this._perpendicularAxis = principalAxis.CrossProduct(revitDoc.ActiveView.ViewDirection);
    this._dimsInDirection = new List<Dimension>();
    this._notesInDirection = new List<TextNote>();
    this.previousOrientation = DimensionEdge.None;
  }

  internal DimPlacementInfo GetDimPlacementInfo(DimensionEdge dimPlacementSide)
  {
    DimPlacementInfo dimPlacementInfo = new DimPlacementInfo();
    XYZ perpendicularAxis = this._perpendicularAxis;
    dimPlacementInfo.DimensionFromEdge = DimensionEdge.Left;
    dimPlacementInfo.DimAlongDirection = perpendicularAxis;
    dimPlacementInfo.PlacementLine = this.GetDimLine(perpendicularAxis, dimPlacementSide);
    dimPlacementInfo.ExtremePoint = new Dictionary<ExtremePointType, XYZ>()
    {
      {
        ExtremePointType.Line,
        this.GetExtremePoint()
      }
    };
    dimPlacementInfo.TextAngle = -perpendicularAxis.AngleOnPlaneTo(this._view.RightDirection, this._view.ViewDirection);
    return dimPlacementInfo;
  }

  public XYZ BumpNote(TextNote note, DimensionEdge dimEdge)
  {
    XYZ coord = note.Coord;
    return dimEdge != DimensionEdge.Bottom ? coord : coord + note.Width * (double) this._view.Scale * -this._perpendicularAxis.Normalize();
  }

  public void CalculateSnapItems(XYZ clickedPoint)
  {
    List<Element> list = new FilteredElementCollector(this._revitDoc, this._revitDoc.ActiveView.Id).WherePasses((ElementFilter) new ElementMulticlassFilter((IList<Type>) new List<Type>()
    {
      typeof (Dimension),
      typeof (TextNote)
    })).ToList<Element>();
    double num1 = clickedPoint.DotProduct(this._principalAxis);
    Dictionary<ElementId, double> dictionary = new Dictionary<ElementId, double>();
    foreach (Element element in list)
    {
      if (element is Dimension)
      {
        Dimension dimension = element as Dimension;
        Line curve = dimension.Curve as Line;
        if ((GeometryObject) curve != (GeometryObject) null && Util.IsParallel(this._perpendicularAxis, curve.Direction))
        {
          double num2 = curve.Origin.DotProduct(this._principalAxis);
          dictionary.Add(element.Id, num2);
          this._dimsInDirection.Add(dimension);
        }
      }
      else if (element is TextNote && Util.IsParallel(this._perpendicularAxis, (element as TextNote).BaseDirection))
        this._notesInDirection.Add(element as TextNote);
    }
    double num3 = double.MaxValue;
    ElementId id = (ElementId) null;
    foreach (ElementId key in dictionary.Keys)
    {
      double num4 = Math.Abs(dictionary[key] - num1);
      if (num4 < num3)
      {
        num3 = num4;
        id = key;
      }
    }
    double num5 = this.GetDimSnapForDefaultDimStyle(this._revitDoc) * (double) this._view.Scale;
    if (num3 > num5 * 1.25)
      id = (ElementId) null;
    if (!(id != (ElementId) null))
      return;
    this._closestDim = this._revitDoc.GetElement(id);
  }

  public XYZ GetTextPlacementPoint(
    Line dimLine,
    DimensionEdge dimEdge,
    List<ReferencedPoint> referencePoints)
  {
    XYZ directionVector = this._perpendicularAxis;
    if (dimEdge == DimensionEdge.Bottom)
      directionVector = directionVector.Negate();
    IEnumerable<double> source = referencePoints.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, directionVector)));
    double val2 = source.Min();
    double num1 = Math.Max(source.Max(), val2);
    XYZ origin = dimLine.Origin;
    double num2 = origin.DotProduct(directionVector);
    double num3 = num1 - num2;
    this._textOffset = (double) this._view.Scale * (1.0 / 64.0);
    return origin + directionVector * (num3 + this._textOffset);
  }

  private XYZ GetExtremePoint()
  {
    if (this._closestDim == null)
      return this.clickPoint;
    Line curve = (this._closestDim as Dimension).Curve as Line;
    XYZ extremePoint = new XYZ();
    if ((GeometryObject) curve != (GeometryObject) null)
    {
      XYZ direction = curve.Direction;
      extremePoint = curve.Origin;
    }
    return extremePoint;
  }

  public Line GetDimLine(XYZ dimLineDirectionVector, DimensionEdge dimPlacementSide)
  {
    XYZ extremePoint = this.GetExtremePoint();
    return this.GetNewDimLine(dimPlacementSide, dimLineDirectionVector, extremePoint);
  }

  private Line GetNewDimLine(
    DimensionEdge dimPlacementSide,
    XYZ dimLineDirectionVector,
    XYZ extremeDimensionLocationPoint)
  {
    XYZ vectorForDimPlacement = this.GetDirectionVectorForDimPlacement(dimPlacementSide);
    double num = this.GetDimSnapForDefaultDimStyle(this._revitDoc) * (double) this._view.Scale;
    XYZ xyz1 = extremeDimensionLocationPoint;
    XYZ xyz2 = this._closestDim != null ? xyz1 + vectorForDimPlacement.Normalize() * num : xyz1;
    return Line.CreateBound(xyz2 - dimLineDirectionVector * 2.0, xyz2 + dimLineDirectionVector * 2.0);
  }

  public DimPlacementInfo DetermineDimLocation(
    List<ReferencedPoint> referencedPoints,
    List<ReferencedPoint> referencedPointsGL,
    DimPlacementInfo oldPlacementInfo,
    DimensionEdge dimPlacementSide,
    TextNote newNote,
    List<ReferencedPoint> overallPointCloud = null)
  {
    DimPlacementInfo dimLocation1 = new DimPlacementInfo();
    dimLocation1.DimensionFromEdge = DimensionEdge.Left;
    dimLocation1.DimAlongDirection = oldPlacementInfo.DimAlongDirection;
    dimLocation1.ExtremePoint = oldPlacementInfo.ExtremePoint;
    IEnumerable<double> source1 = referencedPoints.Union<ReferencedPoint>((IEnumerable<ReferencedPoint>) referencedPointsGL).Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, this._perpendicularAxis)));
    double minW = source1.Min();
    double maxW = source1.Max();
    IEnumerable<double> source2;
    if (overallPointCloud != null)
    {
      IEnumerable<double> doubles;
      if (overallPointCloud.Count <= 0)
        doubles = (IEnumerable<double>) new List<double>()
        {
          double.MinValue
        };
      else
        doubles = overallPointCloud.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, this._principalAxis)));
      source2 = doubles;
    }
    else
    {
      IEnumerable<double> doubles;
      if (referencedPoints.Count <= 0)
        doubles = (IEnumerable<double>) new List<double>()
        {
          double.MinValue
        };
      else
        doubles = referencedPoints.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, this._principalAxis)));
      source2 = doubles;
    }
    double minProj = source2.Min();
    double maxProj = source2.Max();
    if (newNote != null)
    {
      double num = this._textOffset + newNote.Width * (double) this._view.Scale;
      if (dimPlacementSide == DimensionEdge.Top)
        maxW += num;
      else
        minW -= num;
    }
    this._dimsInDirection = new List<Dimension>();
    this._notesInDirection = new List<TextNote>();
    this.CalculateSnapItems(this.clickPoint);
    this.currSide = dimPlacementSide;
    XYZ dimLocation2 = this.FindDimLocation(minW, maxW, minProj, maxProj, this.clickPoint, this.previousOrientation);
    XYZ perpendicularAxis = this._perpendicularAxis;
    dimLocation1.PlacementLine = Line.CreateBound(dimLocation2 - perpendicularAxis * 2.0, dimLocation2 + perpendicularAxis * 2.0);
    dimLocation1.TextAngle = -perpendicularAxis.AngleOnPlaneTo(this._view.RightDirection, this._view.ViewDirection);
    return dimLocation1;
  }

  private XYZ FindDimLocation(
    double minW,
    double maxW,
    double minProj,
    double maxProj,
    XYZ point,
    DimensionEdge dimPlacementSide)
  {
    XYZ point1 = point;
    this.GetDirectionVectorForDimPlacement(dimPlacementSide);
    double num1 = this.GetDimSnapForDefaultDimStyle(this._revitDoc);
    if (num1 == 0.0)
      num1 = 0.001;
    double scaledSnapDist = num1 * (double) this._view.Scale;
    double other1 = maxProj + scaledSnapDist;
    double other2 = minProj - scaledSnapDist;
    double pointProj = ProjectionUtils.ProjectPointOnDirection(point1, this._principalAxis);
    if (pointProj < other1 && pointProj > other2 && !pointProj.ApproximatelyEquals(other1) && !pointProj.ApproximatelyEquals(other2))
    {
      DimensionEdge dimPlacementSide1 = Math.Abs(pointProj - other1) <= Math.Abs(pointProj - other2) ? DimensionEdge.Top : DimensionEdge.Bottom;
      XYZ vectorForDimPlacement = this.GetDirectionVectorForDimPlacement(dimPlacementSide1);
      double num2 = dimPlacementSide1 == DimensionEdge.Top ? Math.Abs(other1 - pointProj) : Math.Abs(other2 - pointProj);
      XYZ point2 = point + vectorForDimPlacement.Normalize() * num2;
      return this.FindDimLocation(minW, maxW, minProj, maxProj, point2, dimPlacementSide1);
    }
    if (this._dimsInDirection.Count == 0)
      return point;
    List<DimFinder> list1 = this._dimsInDirection.Select<Dimension, DimFinder>((Func<Dimension, DimFinder>) (d => new DimFinder(d, this._principalAxis, this._perpendicularAxis))).OrderBy<DimFinder, double>((Func<DimFinder, double>) (di => di.GetVertical(pointProj))).ToList<DimFinder>();
    List<NoteFinder> list2 = this._notesInDirection.Select<TextNote, NoteFinder>((Func<TextNote, NoteFinder>) (d => new NoteFinder(d, this._principalAxis, this._perpendicularAxis, this._view.Scale))).OrderBy<NoteFinder, double>((Func<NoteFinder, double>) (di => di.GetVertical(pointProj))).ToList<NoteFinder>();
    List<DimFinder> list3 = list1.Where<DimFinder>((Func<DimFinder, bool>) (di => di.DetectHorizontalOverlap(minW, maxW))).ToList<DimFinder>();
    List<DimFinder> list4 = list1.Where<DimFinder>((Func<DimFinder, bool>) (di => di.GetVertical(pointProj) < scaledSnapDist)).ToList<DimFinder>();
    List<NoteFinder> list5 = list2.Where<NoteFinder>((Func<NoteFinder, bool>) (di => di.DetectHorizontalOverlap(minW, maxW))).ToList<NoteFinder>();
    List<NoteFinder> list6 = list2.Where<NoteFinder>((Func<NoteFinder, bool>) (di => di.GetVertical(pointProj) < scaledSnapDist)).ToList<NoteFinder>();
    foreach (DimFinder dimFinder in list4)
    {
      dimFinder.GetVertical(pointProj);
      if (!scaledSnapDist.ApproximatelyEquals(dimFinder.GetVertical(pointProj)) && list3.Contains(dimFinder))
      {
        DimensionEdge dimPlacementSide2 = dimPlacementSide;
        if (dimPlacementSide2 == DimensionEdge.None)
        {
          DimensionEdge dimensionEdge = dimFinder.GetVertical(pointProj, true) >= 0.0 ? DimensionEdge.Top : DimensionEdge.Bottom;
          dimPlacementSide2 = 0.0.ApproximatelyEquals(dimFinder.GetVertical(pointProj, true)) ? this.currSide : dimensionEdge;
        }
        XYZ vectorForDimPlacement = this.GetDirectionVectorForDimPlacement(dimPlacementSide2);
        point1 = dimFinder._dimMin + vectorForDimPlacement.Normalize() * scaledSnapDist;
        return this.FindDimLocation(minW, maxW, minProj, maxProj, point1, dimPlacementSide2);
      }
    }
    if (list4.Count > 0)
    {
      DimFinder dimFinder = list4.First<DimFinder>();
      foreach (NoteFinder noteFinder in list6)
      {
        if (!scaledSnapDist.ApproximatelyEquals(dimFinder.GetVertical(pointProj)) && list5.Contains(noteFinder))
        {
          DimensionEdge dimPlacementSide3 = dimPlacementSide;
          if (dimPlacementSide3 == DimensionEdge.None)
          {
            DimensionEdge dimensionEdge = noteFinder.GetVertical(pointProj, true) >= 0.0 ? DimensionEdge.Top : DimensionEdge.Bottom;
            dimPlacementSide3 = 0.0.ApproximatelyEquals(noteFinder.GetVertical(pointProj, true)) ? this.currSide : dimensionEdge;
          }
          XYZ vectorForDimPlacement = this.GetDirectionVectorForDimPlacement(dimPlacementSide3);
          point1 = noteFinder._midPoint + vectorForDimPlacement.Normalize() * scaledSnapDist;
          return this.FindDimLocation(minW, maxW, minProj, maxProj, point1, dimPlacementSide3);
        }
      }
      if (point1.Equals((object) point))
      {
        XYZ xyz = point;
        point1 = dimFinder._dimMin;
        double newPointProj = ProjectionUtils.ProjectPointOnDirection(point1, this._principalAxis);
        if (list3.Count<DimFinder>((Func<DimFinder, bool>) (di => di.GetVertical(newPointProj) < scaledSnapDist || di.GetVertical(newPointProj).ApproximatelyEquals(scaledSnapDist))) > 0)
          point1 = xyz;
        if (list5.Count<NoteFinder>((Func<NoteFinder, bool>) (di => di.GetVertical(newPointProj) < scaledSnapDist || di.GetVertical(newPointProj).ApproximatelyEquals(scaledSnapDist))) > 0)
          point1 = xyz;
      }
    }
    this.previousOrientation = dimPlacementSide;
    return point1;
  }

  private double GetDimDistance(double projection, Dimension dim, double scaledSnapDist)
  {
    return Math.Abs(ProjectionUtils.ProjectPointOnDirection(dim.Origin, this._principalAxis) - projection);
  }

  private double GetDimSnapForDefaultDimStyle(Document revitDoc)
  {
    ElementId defaultElementTypeId = revitDoc.GetDefaultElementTypeId(ElementTypeGroup.LinearDimensionType);
    return Parameters.GetParameterAsDouble((Element) (revitDoc.GetElement(defaultElementTypeId) as DimensionType), BuiltInParameter.DIM_STYLE_DIM_LINE_SNAP_DIST);
  }

  public static double GetTextBoundForDim(Dimension dimension, View view)
  {
    double num1 = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
    double num2 = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE).AsDouble();
    int num3 = dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ).AsInteger() == 2 ? 1 : 0;
    double textBoundForDim = (num1 + num2) * (double) view.Scale;
    if (num3 == 0)
    {
      foreach (DimensionSegment segment in dimension.Segments)
      {
        double num4 = dimension.Curve.Distance(segment.TextPosition) + (num2 + num1) * (double) view.Scale * 1.1;
        textBoundForDim = num4 > textBoundForDim ? num4 : textBoundForDim;
      }
    }
    return textBoundForDim;
  }

  public static double GetFlatTextBound(Dimension dimension, View view)
  {
    return (dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble() + dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE).AsDouble()) * (double) view.Scale;
  }

  public XYZ GetDirectionVectorForDimPlacement(DimensionEdge dimPlacementSide)
  {
    return dimPlacementSide != DimensionEdge.Top ? this._principalAxis.Negate() : this._principalAxis;
  }

  public DimensionEdge DetermineDimEdge(List<ReferencedPoint> referencePoints)
  {
    double num1 = ProjectionUtils.ProjectPointOnDirection(this.clickPoint, this._principalAxis);
    IEnumerable<double> source = referencePoints.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, this._principalAxis)));
    double num2 = source.Min();
    double num3 = (source.Max() + num2) / 2.0;
    return num1 < num3 ? DimensionEdge.Bottom : DimensionEdge.Top;
  }

  public DimensionEdge GetDimPlacementSide(XYZ pickedPoint)
  {
    double num1 = pickedPoint.DotProduct(this._principalAxis);
    double num2 = 0.0;
    if (this._closestDim == null)
      return DimensionEdge.Top;
    Line curve = (this._closestDim as Dimension).Curve as Line;
    if ((GeometryObject) curve != (GeometryObject) null && Util.IsParallel(this._perpendicularAxis, curve.Direction))
      num2 = curve.Origin.DotProduct(this._principalAxis);
    return num2 - num1 < 0.0 ? DimensionEdge.Top : DimensionEdge.Bottom;
  }
}
