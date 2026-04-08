// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.DimPlacementCalculator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class DimPlacementCalculator
{
  private FamilyInstance _sfInstance;
  private ViewSection _view;
  private double _textOffset;

  public DimPlacementCalculator(FamilyInstance sfInstance, ViewSection view)
  {
    this._sfInstance = sfInstance;
    this._view = view;
  }

  internal DimPlacementInfo GetDimPlacementInfo(
    DimensionEdge dimPlacementSide,
    DimensionType dimType)
  {
    DimPlacementInfo dimPlacementInfo = new DimPlacementInfo();
    if (dimPlacementSide == DimensionEdge.None)
      return (DimPlacementInfo) null;
    XYZ lineDirectionVector = this.GetDimLineDirectionVector(dimPlacementSide);
    dimPlacementInfo.DimensionFromEdge = dimPlacementSide == DimensionEdge.Top || dimPlacementSide == DimensionEdge.Bottom ? DimensionEdge.Left : DimensionEdge.Bottom;
    dimPlacementInfo.DimAlongDirection = lineDirectionVector;
    dimPlacementInfo.PlacementLine = this.GetDimLine(lineDirectionVector, dimPlacementSide, dimType);
    dimPlacementInfo.ExtremePoint = this.GetExtremeDimLocationPoint(dimPlacementSide);
    dimPlacementInfo.TextPlacementPoint = this.GetTextPlacementPoint(dimPlacementInfo.PlacementLine, dimPlacementSide);
    dimPlacementInfo.TextAngle = lineDirectionVector.AngleTo(this._view.RightDirection);
    if (dimPlacementSide != DimensionEdge.Right)
      ;
    return dimPlacementInfo;
  }

  public XYZ BumpNote(TextNote note, DimensionEdge dimEdge)
  {
    XYZ coord = note.Coord;
    XYZ xyz;
    switch (dimEdge)
    {
      case DimensionEdge.Right:
        xyz = coord + note.Width * (double) this._view.Scale * -this._view.UpDirection.Normalize();
        break;
      case DimensionEdge.Bottom:
        xyz = coord + note.Width * (double) this._view.Scale * -this._view.RightDirection.Normalize();
        break;
      default:
        xyz = coord;
        break;
    }
    return xyz;
  }

  private XYZ GetTextPlacementPoint(Line dimLine, DimensionEdge dimEdge)
  {
    XYZ source = dimLine.Direction.Normalize();
    if (dimEdge == DimensionEdge.Bottom || dimEdge == DimensionEdge.Right)
      source = source.Negate();
    BoundingBoxXYZ boundingBoxXyz = this._sfInstance.get_BoundingBox((View) null);
    double num1 = Math.Max(boundingBoxXyz.Max.DotProduct(source), boundingBoxXyz.Min.DotProduct(source));
    XYZ endPoint = dimLine.GetEndPoint(0);
    dimLine.GetEndPoint(1);
    double num2 = endPoint.DotProduct(source);
    double num3 = num1 - num2;
    this._textOffset = 0.5;
    return endPoint + source * (num3 + this._textOffset);
  }

  private XYZ GetVectorToDimPlacementEdgeFromSFCenter(XYZ pickedPoint)
  {
    XYZ sfCenter = this.GetSFCenter();
    XYZ xyz = pickedPoint - sfCenter;
    return xyz - xyz.DotProduct(this._view.ViewDirection.Normalize()) * this._view.ViewDirection.Normalize();
  }

  public Line GetDimLine(
    XYZ dimLineDirectionVector,
    DimensionEdge dimPlacementSide,
    DimensionType dimType)
  {
    Dictionary<ExtremePointType, XYZ> dimLocationPoint = this.GetExtremeDimLocationPoint(dimPlacementSide);
    return this.GetNewDimLine(dimPlacementSide, dimLineDirectionVector, dimLocationPoint, dimType);
  }

  private Line GetNewDimLine(
    DimensionEdge dimPlacementSide,
    XYZ dimLineDirectionVector,
    Dictionary<ExtremePointType, XYZ> extremeDimensionLocationPoint,
    DimensionType dimType)
  {
    XYZ vectorForDimPlacement = this.GetDirectionVectorForDimPlacement(dimPlacementSide);
    double num1 = this.GetDimSnapForDefaultDimStyle(this._view.Document, dimType) * (double) this._view.Scale;
    XYZ source = !extremeDimensionLocationPoint.ContainsKey(ExtremePointType.Line) ? extremeDimensionLocationPoint[ExtremePointType.SFBound] + vectorForDimPlacement.Normalize() * (num1 * 1.5) : extremeDimensionLocationPoint[ExtremePointType.Line] + vectorForDimPlacement.Normalize() * num1;
    if (extremeDimensionLocationPoint.ContainsKey(ExtremePointType.Bound) && vectorForDimPlacement.DotProduct(source) < vectorForDimPlacement.DotProduct(extremeDimensionLocationPoint[ExtremePointType.Bound]))
    {
      double num2 = vectorForDimPlacement.DotProduct(source);
      double num3 = Math.Abs(vectorForDimPlacement.DotProduct(extremeDimensionLocationPoint[ExtremePointType.Bound]) - num2);
      source += vectorForDimPlacement.Normalize() * (num3 + num1 * 0.5);
    }
    return Line.CreateBound(source - dimLineDirectionVector * 2.0, source + dimLineDirectionVector * 2.0);
  }

  private XYZ GetDimLineDirectionVector(DimensionEdge dimPlacementSide)
  {
    return dimPlacementSide == DimensionEdge.Top || dimPlacementSide == DimensionEdge.Bottom ? this._view.RightDirection : this._view.UpDirection;
  }

  internal DimensionEdge GetDimensionFromEdge(XYZ pickedPoint)
  {
    this.GetVectorToDimPlacementEdgeFromSFCenter(pickedPoint);
    switch (this.GetDimPlacementEdgeBasedOnPickedPoint(pickedPoint))
    {
      case DimensionEdge.Top:
      case DimensionEdge.Bottom:
        return DimensionEdge.Left;
      default:
        return DimensionEdge.Bottom;
    }
  }

  private double GetDimSnapForDefaultDimStyle(Document revitDoc, DimensionType dimType)
  {
    if (dimType == null)
    {
      ElementId defaultElementTypeId = revitDoc.GetDefaultElementTypeId(ElementTypeGroup.LinearDimensionType);
      dimType = revitDoc.GetElement(defaultElementTypeId) as DimensionType;
    }
    return Parameters.GetParameterAsDouble((Element) dimType, BuiltInParameter.DIM_STYLE_DIM_LINE_SNAP_DIST);
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
        if (segment.IsTextPositionAdjustable())
        {
          double num4 = dimension.Curve.Distance(segment.TextPosition) + (num2 + num1 + num1) * (double) view.Scale;
          textBoundForDim = num4 > textBoundForDim ? num4 : textBoundForDim;
        }
      }
    }
    return textBoundForDim;
  }

  public static double GetFlatTextBound(Dimension dimension, View view)
  {
    return (dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble() + dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE).AsDouble()) * (double) view.Scale;
  }

  private Dictionary<ExtremePointType, XYZ> GetExtremeDimLocationPoint(
    DimensionEdge dimPlacementSide)
  {
    Dictionary<ExtremePointType, XYZ> extremePoints = new Dictionary<ExtremePointType, XYZ>();
    XYZ orientationVector = this.GetDirectionVectorForDimPlacement(dimPlacementSide);
    FilteredElementCollector source1 = new FilteredElementCollector(this._sfInstance.Document, this._view.Id).OfClass(typeof (Dimension));
    FilteredElementCollector source2 = new FilteredElementCollector(this._sfInstance.Document, this._view.Id).OfClass(typeof (IndependentTag));
    List<XYZ> list1 = source1.Cast<Dimension>().Select<Dimension, XYZ>((Func<Dimension, XYZ>) (s => s.NumberOfSegments <= 1 ? s.Origin : s.Segments.get_Item(0).Origin)).ToList<XYZ>();
    List<XYZ> source3 = new List<XYZ>();
    List<XYZ> source4 = new List<XYZ>();
    List<Dimension> list2 = source1.Select<Element, Dimension>((Func<Element, Dimension>) (elem => elem as Dimension)).ToList<Dimension>();
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform((View) this._view);
    List<XYZ> outlinePoints = new List<XYZ>();
    BoundingBoxXYZ viewBbox = DimReferencesManager.GetViewBBox(this._sfInstance, (View) this._view, out outlinePoints);
    source3.AddRange(outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => viewTransform.OfPoint(pt))));
    int index = 0;
    List<Dimension> dimensionList = new List<Dimension>();
    XYZ max = viewBbox.Max;
    XYZ min = viewBbox.Min;
    foreach (Dimension dimension in list2)
    {
      bool flag = false;
      foreach (DimensionSegment segment in dimension.Segments)
      {
        XYZ xyz = viewTransform.Inverse.OfPoint(segment.Origin);
        switch (dimPlacementSide)
        {
          case DimensionEdge.Left:
            if (xyz.X >= min.X)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_17;
            }
            continue;
          case DimensionEdge.Right:
            if (xyz.X <= max.X)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_17;
            }
            continue;
          case DimensionEdge.Top:
            if (xyz.Y <= max.Y)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_17;
            }
            continue;
          case DimensionEdge.Bottom:
            if (xyz.Y >= min.Y)
            {
              dimensionList.Add(list2[index]);
              flag = true;
              goto label_17;
            }
            continue;
          default:
            continue;
        }
      }
label_17:
      ++index;
      int num = flag ? 1 : 0;
    }
    foreach (Dimension dimension in dimensionList)
      list2.Remove(dimension);
    XYZ xyz1 = orientationVector;
    if (dimPlacementSide == DimensionEdge.Bottom || dimPlacementSide == DimensionEdge.Right)
      xyz1 = orientationVector.Negate();
    foreach (Dimension dimension in list2)
    {
      double textBoundForDim = DimPlacementCalculator.GetTextBoundForDim(dimension, (View) this._view);
      foreach (DimensionSegment segment in dimension.Segments)
      {
        source4.Add(segment.Origin + xyz1 * textBoundForDim);
        source4.Add(segment.Origin);
      }
    }
    List<XYZ> collection = new List<XYZ>();
    foreach (IndependentTag independentTag in source2.Cast<IndependentTag>())
    {
      collection.Add(independentTag.get_BoundingBox((View) this._view).Max);
      collection.Add(independentTag.get_BoundingBox((View) this._view).Min);
    }
    source4.AddRange((IEnumerable<XYZ>) collection);
    extremePoints.Add(ExtremePointType.SFBound, source3.OrderByDescending<XYZ, double>((Func<XYZ, double>) (p => p.DotProduct(orientationVector))).First<XYZ>());
    list1.RemoveAll((Predicate<XYZ>) (pt => pt.DotProduct(orientationVector) < extremePoints[ExtremePointType.SFBound].DotProduct(orientationVector)));
    source4.RemoveAll((Predicate<XYZ>) (pt => pt.DotProduct(orientationVector) < extremePoints[ExtremePointType.SFBound].DotProduct(orientationVector)));
    if (source4.Count > 0)
    {
      XYZ xyz2 = source4.OrderByDescending<XYZ, double>((Func<XYZ, double>) (p => p.DotProduct(orientationVector))).First<XYZ>();
      extremePoints.Add(ExtremePointType.Bound, xyz2);
    }
    if (list1.Count > 0)
    {
      XYZ xyz3 = list1.OrderByDescending<XYZ, double>((Func<XYZ, double>) (p => p.DotProduct(orientationVector))).First<XYZ>();
      extremePoints.Add(ExtremePointType.Line, xyz3);
    }
    return extremePoints;
  }

  public XYZ GetDirectionVectorForDimPlacement(DimensionEdge dimPlacementSide)
  {
    XYZ vectorForDimPlacement;
    XYZ xyz;
    switch (dimPlacementSide)
    {
      case DimensionEdge.Right:
        xyz = this._view.RightDirection;
        break;
      case DimensionEdge.Top:
      case DimensionEdge.Bottom:
        vectorForDimPlacement = dimPlacementSide == DimensionEdge.Top ? this._view.UpDirection : this._view.UpDirection.Negate();
        goto label_5;
      default:
        xyz = this._view.RightDirection.Negate();
        break;
    }
    vectorForDimPlacement = xyz;
label_5:
    return vectorForDimPlacement;
  }

  private DimensionEdge GetDimPlacementEdge(XYZ diffProjToView)
  {
    DimensionEdge dimPlacementEdge = DimensionEdge.None;
    double num1 = diffProjToView.Normalize().DotProduct(this._view.UpDirection.Normalize());
    if (num1 > Math.Cos(Math.PI / 4.0) || num1 < -1.0 * Math.Cos(Math.PI / 4.0))
    {
      dimPlacementEdge = num1 <= 0.0 ? DimensionEdge.Bottom : DimensionEdge.Top;
    }
    else
    {
      double num2 = diffProjToView.Normalize().DotProduct(this._view.RightDirection.Normalize());
      if (num2 > Math.Cos(Math.PI / 4.0) || num2 < Math.Cos(-1.0 * Math.PI / 4.0))
        dimPlacementEdge = num2 <= 0.0 ? DimensionEdge.Left : DimensionEdge.Right;
    }
    return dimPlacementEdge;
  }

  public DimensionEdge GetDimPlacementEdgeBasedOnPickedPoint(XYZ pickedPoint)
  {
    BoundingBoxXYZ boundingBoxXyz = this._sfInstance.get_BoundingBox((View) this._view);
    double num1 = boundingBoxXyz.Min.DotProduct(this._view.RightDirection.Normalize());
    double num2 = boundingBoxXyz.Max.DotProduct(this._view.RightDirection.Normalize());
    double num3 = boundingBoxXyz.Min.DotProduct(this._view.UpDirection.Normalize());
    double num4 = boundingBoxXyz.Max.DotProduct(this._view.UpDirection.Normalize());
    double num5 = pickedPoint.DotProduct(this._view.RightDirection.Normalize());
    double num6 = pickedPoint.DotProduct(this._view.UpDirection.Normalize());
    if (num1 > num2)
    {
      double num7 = num2;
      num2 = num1;
      num1 = num7;
    }
    if (num3 > num4)
    {
      double num8 = num4;
      num4 = num3;
      num3 = num8;
    }
    if (num5 < num2 & num5 > num1 && num6 > num3 && num6 < num4)
    {
      TaskDialog.Show("EDGE Error", "Picked point for dimension is within the member itself.  Please select again.");
      return DimensionEdge.None;
    }
    return num5 > num1 && num5 < num2 ? (num6 <= num4 ? DimensionEdge.Bottom : DimensionEdge.Top) : (num6 > num3 && num6 < num4 ? (num5 <= num2 ? DimensionEdge.Left : DimensionEdge.Right) : (num5 < num1 ? (num6 > num4 ? (num1 - num5 <= num6 - num4 ? DimensionEdge.Top : DimensionEdge.Left) : (num1 - num5 <= num3 - num6 ? DimensionEdge.Bottom : DimensionEdge.Left)) : (num6 > num4 ? (num5 - num2 <= num6 - num4 ? DimensionEdge.Top : DimensionEdge.Right) : (num5 - num2 <= num3 - num6 ? DimensionEdge.Bottom : DimensionEdge.Right))));
  }

  private XYZ GetSFCenter()
  {
    BoundingBoxXYZ boundingBoxXyz = this._sfInstance.get_BoundingBox((View) this._view);
    return (boundingBoxXyz.Min + boundingBoxXyz.Max) / 2.0;
  }
}
