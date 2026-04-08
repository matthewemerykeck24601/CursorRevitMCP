// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.WarpingEdgeLoop
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AdminUtils;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

public class WarpingEdgeLoop
{
  private Transform _transform;
  private List<FamilyInstance> _intersectedSFInstances;
  private List<FamilyInstance> _embedInstances;
  private List<WarpedBeamData> _warpBeamData;
  private List<DirectShape> _directShapes;
  private const double slopeEps = 0.001;
  private const double posEps = 0.01;
  private Line[] _extents = new Line[4];

  public List<WarpingEdge> Edges { get; }

  private List<SpotElevationPoint> SpotElevationPoints { get; }

  public WarpingEdgeLoop(List<WarpingEdge> edges, Transform transform)
  {
    this.Edges = edges.ToList<WarpingEdge>();
    this._transform = transform;
    this.SpotElevationPoints = this.CollectSpots();
    this._warpBeamData = new List<WarpedBeamData>();
  }

  public void PrimeSFInstances(Dictionary<FamilyInstance, XYZ[]> sfDict)
  {
    this._intersectedSFInstances = new List<FamilyInstance>();
    foreach (FamilyInstance key in sfDict.Keys)
    {
      if (this.CheckPieceInsideLoop(sfDict[key]))
        this._intersectedSFInstances.Add(key);
    }
  }

  private Tuple<XYZ, XYZ> GetMinMaxTransformedToPlan(Transform planTransform)
  {
    double x1 = double.MaxValue;
    double y1 = double.MaxValue;
    double x2 = double.MinValue;
    double y2 = double.MinValue;
    foreach (XYZ point in this.SpotElevationPoints.Select<SpotElevationPoint, XYZ>((Func<SpotElevationPoint, XYZ>) (elevation => elevation.Point)))
    {
      XYZ xyz = planTransform.Inverse.OfPoint(point);
      x1 = xyz.X < x1 ? xyz.X : x1;
      y1 = xyz.Y < y1 ? xyz.Y : y1;
      x2 = xyz.X > x2 ? xyz.X : x2;
      y2 = xyz.Y > y2 ? xyz.Y : y2;
    }
    return new Tuple<XYZ, XYZ>(new XYZ(x1, y1, 0.0), new XYZ(x2, y2, 0.0));
  }

  public Tuple<UV, UV> GetMinMax_DotProduct(XYZ planDir, XYZ orthoDir)
  {
    double u1 = double.MaxValue;
    double v1 = double.MaxValue;
    double u2 = double.MinValue;
    double v2 = double.MinValue;
    foreach (XYZ xyz in this.SpotElevationPoints.Select<SpotElevationPoint, XYZ>((Func<SpotElevationPoint, XYZ>) (elevation => elevation.Point)))
    {
      double num1 = xyz.DotProduct(orthoDir);
      double num2 = xyz.DotProduct(planDir);
      u1 = num1 < u1 ? num1 : u1;
      v1 = num2 < v1 ? num2 : v1;
      u2 = num1 > u2 ? num1 : u2;
      v2 = num2 > v2 ? num2 : v2;
    }
    return new Tuple<UV, UV>(new UV(u1, v1), new UV(u2, v2));
  }

  private List<SpotElevationPoint> CollectSpots()
  {
    List<SpotElevationPoint> spotElevationPointList = new List<SpotElevationPoint>();
    foreach (WarpingEdge edge in this.Edges)
    {
      if (!spotElevationPointList.Contains(edge.StartPoint))
        spotElevationPointList.Add(edge.StartPoint);
      if (!spotElevationPointList.Contains(edge.EndPoint))
        spotElevationPointList.Add(edge.EndPoint);
    }
    return spotElevationPointList;
  }

  public XYZ GetPlanDirectionForWarpingLoop()
  {
    List<XYZ> source1 = new List<XYZ>();
    List<XYZ> xyzList = new List<XYZ>();
    foreach (FamilyInstance intersectedSfInstance in this._intersectedSFInstances)
    {
      XYZ xyz = this._transform.Inverse.OfVector(WarpingUtils.GetPlanDirectionForSFElement(AssemblyInstances.GetFlatElement(intersectedSfInstance.Document, intersectedSfInstance), this._transform));
      if (xyz != null)
      {
        if (Utils.ElementUtils.Parameters.GetParameterAsString((Element) intersectedSfInstance, "CONSTRUCTION_PRODUCT").ToUpper().Contains("DOUBLE TEE"))
          xyzList.Add(xyz);
        source1.Add(xyz);
      }
    }
    if (xyzList.Count > 0)
      source1 = xyzList;
    XYZ source2 = new XYZ();
    int num = int.MinValue;
    foreach (XYZ xyz in source1)
    {
      XYZ direction = xyz;
      if (!direction.IsAlmostEqualTo(source2, 0.001))
      {
        List<XYZ> list = source1.Where<XYZ>((Func<XYZ, bool>) (p => p.IsAlmostEqualTo(direction, 0.001))).ToList<XYZ>();
        if (list.Count > num)
        {
          source2 = direction;
          num = list.Count;
        }
      }
    }
    return source2;
  }

  private Tuple<List<Line>, List<Line>> GetOrthoSpotToSpots(
    List<XYZ> spots,
    XYZ planDir,
    out bool linear,
    out bool allConsumedParallel,
    out bool allConsumedOrtho)
  {
    allConsumedParallel = false;
    allConsumedOrtho = false;
    XYZ source1 = planDir.CrossProduct(XYZ.BasisZ);
    linear = true;
    Dictionary<Line, List<XYZ>> dictionary = new Dictionary<Line, List<XYZ>>();
    List<XYZ> source2 = new List<XYZ>();
    List<XYZ> source3 = new List<XYZ>();
    foreach (XYZ spot1 in spots)
    {
      foreach (XYZ spot2 in spots)
      {
        if (!spot1.IsAlmostEqualTo(spot2) && spot1.DistanceTo(spot2) >= 1.0 / 192.0)
        {
          spot1.DotProduct(planDir);
          spot2.DotProduct(planDir);
          Line bound = Line.CreateBound(spot1, spot2);
          dictionary.Add(bound, new List<XYZ>()
          {
            spot1,
            spot2
          });
        }
      }
    }
    List<Line> lineList1 = new List<Line>();
    foreach (Line key in dictionary.Keys)
    {
      Line flatLine = WarpingUtils.CreateFlatLine(key);
      Line unbound = Line.CreateUnbound(flatLine.Origin, flatLine.Direction);
      XYZ direction = flatLine.Direction;
      if (direction.IsAlmostEqualTo(planDir, 0.001) || direction.IsAlmostEqualTo(planDir.Negate(), 0.001))
      {
        Line line1 = direction.IsAlmostEqualTo(planDir, 0.001) ? key : Line.CreateBound(key.GetEndPoint(1), key.GetEndPoint(0));
        bool flag = true;
        List<Line> lineList2 = new List<Line>();
        source2.AddRange((IEnumerable<XYZ>) dictionary[key]);
        foreach (Line line2 in lineList1)
        {
          if (unbound.Project(WarpingUtils.CreateFlatLine(line2).GetEndPoint(0)).Distance < 0.01 || unbound.Project(WarpingUtils.CreateFlatLine(line2).GetEndPoint(1)).Distance < 0.01)
          {
            if (!line1.Direction.IsAlmostEqualTo(line2.Direction, 0.001))
            {
              linear = false;
              return (Tuple<List<Line>, List<Line>>) null;
            }
            if (flatLine.Length > WarpingUtils.CreateFlatLine(line2).Length)
              lineList2.Add(line2);
            else
              flag = false;
          }
        }
        foreach (Line line3 in lineList2)
          lineList1.Remove(line3);
        if (flag)
          lineList1.Add(line1);
      }
    }
    if (source2.Distinct<XYZ>().ToList<XYZ>().Count == spots.Count)
      allConsumedParallel = true;
    List<Line> lineList3 = new List<Line>();
    foreach (Line key in dictionary.Keys)
    {
      Line flatLine = WarpingUtils.CreateFlatLine(key);
      Line unbound = Line.CreateUnbound(flatLine.Origin, flatLine.Direction);
      XYZ direction = flatLine.Direction;
      if (direction.IsAlmostEqualTo(source1, 0.001) || direction.IsAlmostEqualTo(source1.Negate(), 0.001))
      {
        Line line4 = direction.IsAlmostEqualTo(source1, 0.001) ? key : Line.CreateBound(key.GetEndPoint(1), key.GetEndPoint(0));
        bool flag = true;
        List<Line> lineList4 = new List<Line>();
        source3.AddRange((IEnumerable<XYZ>) dictionary[key]);
        foreach (Line line5 in lineList3)
        {
          if (unbound.Project(WarpingUtils.CreateFlatLine(line5).GetEndPoint(0)).Distance < 0.01 || unbound.Project(WarpingUtils.CreateFlatLine(line5).GetEndPoint(1)).Distance < 0.01)
          {
            if (!line4.Direction.IsAlmostEqualTo(line5.Direction, 0.001))
            {
              linear = false;
              return (Tuple<List<Line>, List<Line>>) null;
            }
            if (flatLine.Length > WarpingUtils.CreateFlatLine(line5).Length)
              lineList4.Add(line5);
            else
              flag = false;
          }
        }
        foreach (Line line6 in lineList4)
          lineList3.Remove(line6);
        if (flag)
          lineList3.Add(line4);
      }
    }
    if (source3.Distinct<XYZ>().ToList<XYZ>().Count == spots.Count)
      allConsumedOrtho = true;
    return new Tuple<List<Line>, List<Line>>(lineList1, lineList3);
  }

  private List<Line> ProjectSpots(Line referenceLine, XYZ planDir, out bool linear)
  {
    linear = true;
    Line unbound1 = Line.CreateUnbound(referenceLine.Origin, referenceLine.Direction);
    Line flatLine1 = WarpingUtils.CreateFlatLine(referenceLine);
    Line unbound2 = Line.CreateUnbound(new XYZ(referenceLine.Origin.X, referenceLine.Origin.Y, 0.0), new XYZ(referenceLine.Direction.X, referenceLine.Direction.Y, 0.0).Normalize());
    List<Line> lineList1 = new List<Line>();
    foreach (SpotElevationPoint spotElevationPoint in this.SpotElevationPoints)
    {
      XYZ point = spotElevationPoint.Point;
      XYZ planePoint = spotElevationPoint.planePoint;
      double parameter = unbound2.Project(planePoint).Parameter;
      double normalizedParameter = flatLine1.ComputeNormalizedParameter(parameter);
      double rawParameter = referenceLine.ComputeRawParameter(normalizedParameter);
      XYZ xyz = unbound1.Evaluate(rawParameter, false);
      if (point.DistanceTo(xyz) > 0.0026)
      {
        Line line1 = xyz.DotProduct(planDir) >= point.DotProduct(planDir) ? Line.CreateBound(point, xyz) : Line.CreateBound(xyz, point);
        Line flatLine2 = WarpingUtils.CreateFlatLine(line1);
        List<Line> lineList2 = new List<Line>();
        bool flag = true;
        foreach (Line line2 in lineList1)
        {
          Line flatLine3 = WarpingUtils.CreateFlatLine(line2);
          if (flatLine3.GetEndPoint(0).IsAlmostEqualTo(flatLine2.GetEndPoint(0), 0.01) || flatLine3.GetEndPoint(0).IsAlmostEqualTo(flatLine2.GetEndPoint(1), 0.01) || flatLine3.GetEndPoint(1).IsAlmostEqualTo(flatLine2.GetEndPoint(0), 0.01) || flatLine3.GetEndPoint(1).IsAlmostEqualTo(flatLine2.GetEndPoint(1), 0.01))
          {
            line1.Direction.DistanceTo(line2.Direction);
            if (!line1.Direction.IsAlmostEqualTo(line2.Direction, 0.001))
            {
              linear = false;
              return (List<Line>) null;
            }
            if (flatLine2.Length > flatLine3.Length)
              lineList2.Add(line2);
            else
              flag = false;
          }
        }
        foreach (Line line3 in lineList2)
          lineList1.Remove(line3);
        if (flag)
          lineList1.Add(line1);
      }
    }
    return lineList1;
  }

  public bool IsConvex()
  {
    if (this.Edges.Count < 3)
      return false;
    List<XYZ> list = this.Edges.SelectMany<WarpingEdge, XYZ>((Func<WarpingEdge, IEnumerable<XYZ>>) (s => (IEnumerable<XYZ>) new List<XYZ>()
    {
      s.StartPoint.planePoint,
      s.EndPoint.planePoint
    })).Distinct<XYZ>().ToList<XYZ>();
    List<XYZ> convexHull = ConvexHull2D.ComputeConvexHull(list);
    return convexHull != null && convexHull.Count - 1 == list.Count;
  }

  public override int GetHashCode()
  {
    return this.Edges.Select<WarpingEdge, int>((Func<WarpingEdge, int>) (s => s.EdgeNumber)).Sum();
  }

  public override bool Equals(object obj)
  {
    return obj is WarpingEdgeLoop warpingEdgeLoop && string.Join("", this.Edges.Select<WarpingEdge, int>((Func<WarpingEdge, int>) (s => s.EdgeNumber)).OrderBy<int, int>((Func<int, int>) (s => s)).Select<int, string>((Func<int, string>) (s => s.ToString()))) == string.Join("", warpingEdgeLoop.Edges.Select<WarpingEdge, int>((Func<WarpingEdge, int>) (s => s.EdgeNumber)).OrderBy<int, int>((Func<int, int>) (s => s)).Select<int, string>((Func<int, string>) (s => s.ToString())));
  }

  private bool RemoveDuplicateSpots(List<XYZ> spots, List<XYZ> newSpots)
  {
    List<XYZ> xyzList = new List<XYZ>();
    foreach (XYZ spot in spots)
    {
      foreach (XYZ newSpot in newSpots)
      {
        if (Math.Abs(spot.X - newSpot.X) < 0.01 && Math.Abs(spot.Y - newSpot.Y) < 0.01)
        {
          if (Math.Abs(spot.Z - newSpot.Z) > 0.01)
            return false;
          xyzList.Add(newSpot);
        }
      }
    }
    foreach (XYZ xyz in xyzList)
      newSpots.Remove(xyz);
    return true;
  }

  private Line[] GetExtentLines(double min, double max, List<Line> lines, XYZ orthoDir)
  {
    Line[] extentLines = new Line[2];
    foreach (Line line in lines)
    {
      double num1 = line.Origin.DotProduct(orthoDir);
      double num2 = line.GetEndPoint(1).DotProduct(orthoDir);
      if (Math.Abs(num1 - min) < 0.01 || Math.Abs(num2 - min) < 0.01)
        extentLines[0] = line;
      if (Math.Abs(num1 - max) < 0.01 || Math.Abs(num2 - max) < 0.01)
        extentLines[1] = line;
    }
    return extentLines;
  }

  private List<XYZ> GetSpotsOnEdgesAssumingLinear(XYZ planDir)
  {
    List<XYZ> edgesAssumingLinear = new List<XYZ>();
    foreach (SpotElevationPoint spotElevationPoint in this.SpotElevationPoints)
    {
      Line unbound = Line.CreateUnbound(spotElevationPoint.planePoint, planDir);
      foreach (WarpingEdge edge in this.Edges)
      {
        IntersectionResultArray resultArray;
        if (unbound.Intersect(edge.GetFlatCurve(), out resultArray) == SetComparisonResult.Overlap)
        {
          double v = resultArray.get_Item(0).UVPoint.V;
          double normalizedParameter = edge.GetFlatCurve().ComputeNormalizedParameter(v);
          double rawParameter = edge.GetSlopeLine().ComputeRawParameter(normalizedParameter);
          XYZ source = edge.GetSlopeLine().Evaluate(rawParameter, false);
          if (spotElevationPoint.Point.DistanceTo(source) >= 0.0026 && normalizedParameter >= 0.0 && normalizedParameter <= 1.0)
            edgesAssumingLinear.Add(source);
        }
      }
    }
    return edgesAssumingLinear;
  }

  private List<XYZ> ProjectExtents(
    List<Line> shortLines,
    Transform transform,
    Tuple<XYZ, XYZ> minMax)
  {
    List<XYZ> xyzList = new List<XYZ>();
    XYZ xyz1 = minMax.Item1;
    XYZ xyz2 = minMax.Item2;
    foreach (Curve shortLine in shortLines)
    {
      Line transformed = shortLine.CreateTransformed(transform.Inverse) as Line;
      Line flatLine = WarpingUtils.CreateFlatLine(transformed);
      double normalizedParameter1 = 0.0;
      double normalizedParameter2 = 1.0;
      if (flatLine.Direction.IsAlmostEqualTo(XYZ.BasisY, 0.001))
      {
        normalizedParameter1 = flatLine.ComputeNormalizedParameter(xyz1.Y - transformed.Origin.Y);
        normalizedParameter2 = flatLine.ComputeNormalizedParameter(xyz2.Y - transformed.Origin.Y);
      }
      else if (flatLine.Direction.IsAlmostEqualTo(XYZ.BasisX, 0.001))
      {
        normalizedParameter1 = flatLine.ComputeNormalizedParameter(xyz1.X - transformed.Origin.X);
        normalizedParameter2 = flatLine.ComputeNormalizedParameter(xyz2.X - transformed.Origin.X);
      }
      double rawParameter1 = transformed.ComputeRawParameter(normalizedParameter1);
      double rawParameter2 = transformed.ComputeRawParameter(normalizedParameter2);
      if (normalizedParameter2 > 0.99)
        xyzList.Add(transform.OfPoint(transformed.Evaluate(rawParameter2, false)));
      if (normalizedParameter1 < 0.001)
        xyzList.Add(transform.OfPoint(transformed.Evaluate(rawParameter1, false)));
    }
    return xyzList;
  }

  private List<XYZ> ProjectSpotsToExtents(List<XYZ> spots, Line[] extents)
  {
    List<XYZ> extents1 = new List<XYZ>();
    foreach (XYZ spot in spots)
    {
      for (int index = 0; index < 2; ++index)
      {
        if ((GeometryObject) extents[index] != (GeometryObject) null)
        {
          Line flatLine = WarpingUtils.CreateFlatLine(extents[index]);
          double normalizedParameter = flatLine.ComputeNormalizedParameter(flatLine.Project(spot).Parameter);
          if (normalizedParameter < 1.0 && normalizedParameter > 0.0)
          {
            XYZ xyz = extents[index].Evaluate(normalizedParameter, true);
            if (xyz.DistanceTo(spot) > 0.01)
              extents1.Add(xyz);
          }
        }
      }
    }
    return extents1;
  }

  private bool CheckOrthoIntersect(Line l1, Line l2)
  {
    Line flatLine1 = WarpingUtils.CreateFlatLine(l1);
    Line flatLine2 = WarpingUtils.CreateFlatLine(l2);
    IntersectionResultArray resultArray;
    if (flatLine1.Intersect((Curve) flatLine2, out resultArray) == SetComparisonResult.Overlap)
    {
      UV uvPoint = resultArray.get_Item(0).UVPoint;
      double normalizedParameter1 = flatLine1.ComputeNormalizedParameter(uvPoint.U);
      double rawParameter1 = l1.ComputeRawParameter(normalizedParameter1);
      double z = l1.Evaluate(rawParameter1, false).Z;
      double normalizedParameter2 = flatLine2.ComputeNormalizedParameter(uvPoint.V);
      double rawParameter2 = l2.ComputeRawParameter(normalizedParameter2);
      if (Math.Abs(l2.Evaluate(rawParameter2, false).Z - z) > 0.01)
        return false;
    }
    return true;
  }

  public bool CheckLinearity()
  {
    if (this._intersectedSFInstances.Count == 0)
      return true;
    XYZ directionForWarpingLoop = this.GetPlanDirectionForWarpingLoop();
    XYZ xyz = directionForWarpingLoop.CrossProduct(XYZ.BasisZ);
    Tuple<UV, UV> minMaxDotProduct = this.GetMinMax_DotProduct(directionForWarpingLoop, xyz);
    Transform identity = Transform.Identity;
    identity.BasisX = directionForWarpingLoop;
    identity.BasisY = xyz;
    Tuple<XYZ, XYZ> transformedToPlan = this.GetMinMaxTransformedToPlan(identity);
    double u1 = minMaxDotProduct.Item1.U;
    double v1 = minMaxDotProduct.Item1.V;
    double u2 = minMaxDotProduct.Item2.U;
    double v2 = minMaxDotProduct.Item2.V;
    double extentHeight = Math.Abs(v2 - v1);
    double extentWidth = Math.Abs(u2 - u1);
    List<XYZ> list = this.SpotElevationPoints.Select<SpotElevationPoint, XYZ>((Func<SpotElevationPoint, XYZ>) (spot => spot.Point)).ToList<XYZ>();
    bool allConsumedParallel = false;
    bool allConsumedOrtho = false;
    List<Line> lineList1 = new List<Line>();
    List<Line> lineList2 = new List<Line>();
    int num1 = 0;
    Line[] lineArray1 = new Line[2];
    Line[] lineArray2 = new Line[2];
    bool linear;
    List<Line> lineList3;
    List<Line> lineList4;
    Line[] extentLines1;
    Line[] extentLines2;
    do
    {
      ++num1;
      Tuple<List<Line>, List<Line>> orthoSpotToSpots = this.GetOrthoSpotToSpots(list, directionForWarpingLoop, out linear, out allConsumedParallel, out allConsumedOrtho);
      if (!linear)
        return false;
      lineList3 = orthoSpotToSpots.Item1;
      lineList4 = orthoSpotToSpots.Item2;
      extentLines1 = this.GetExtentLines(u1, u2, lineList3, xyz);
      extentLines2 = this.GetExtentLines(v1, v2, lineList4, directionForWarpingLoop);
      linear = this.CheckSlopeLinearity(lineList3, xyz) & this.CheckSlopeLinearity(lineList4, directionForWarpingLoop);
      if (!linear)
        return false;
      if (lineList3.Count == 0 && lineList4.Count == 0)
      {
        List<XYZ> edgesAssumingLinear = this.GetSpotsOnEdgesAssumingLinear(directionForWarpingLoop);
        edgesAssumingLinear.AddRange((IEnumerable<XYZ>) this.GetSpotsOnEdgesAssumingLinear(xyz));
        if (!this.RemoveDuplicateSpots(list, edgesAssumingLinear))
          return false;
        list.AddRange((IEnumerable<XYZ>) edgesAssumingLinear);
      }
      else if (lineList3.Where<Line>((Func<Line, bool>) (line => line.Length < extentHeight - 0.01)).ToList<Line>().Count > 0)
      {
        List<XYZ> xyzList = this.ProjectExtents(lineList3, identity, transformedToPlan);
        if (!this.RemoveDuplicateSpots(list, xyzList))
          return false;
        list.AddRange((IEnumerable<XYZ>) xyzList);
      }
      else if (lineList4.Where<Line>((Func<Line, bool>) (line => line.Length < extentWidth - 0.01)).ToList<Line>().Count > 0)
      {
        List<XYZ> xyzList = this.ProjectExtents(lineList4, identity, transformedToPlan);
        if (!this.RemoveDuplicateSpots(list, xyzList))
          return false;
        list.AddRange((IEnumerable<XYZ>) xyzList);
      }
      else
      {
        if ((GeometryObject) extentLines1[0] != (GeometryObject) null || (GeometryObject) extentLines1[1] != (GeometryObject) null)
        {
          List<XYZ> extents = this.ProjectSpotsToExtents(list, extentLines1);
          if (!this.RemoveDuplicateSpots(list, extents))
            return false;
          if (extents.Count != 0)
          {
            list.AddRange((IEnumerable<XYZ>) extents);
            goto label_32;
          }
        }
        if ((GeometryObject) extentLines2[0] != (GeometryObject) null || (GeometryObject) extentLines2[1] != (GeometryObject) null)
        {
          List<XYZ> extents = this.ProjectSpotsToExtents(list, extentLines2);
          if (!this.RemoveDuplicateSpots(list, extents))
            return false;
          if (extents.Count != 0)
          {
            list.AddRange((IEnumerable<XYZ>) extents);
            goto label_32;
          }
        }
        if (lineList3.Count == 1 && lineList4.Count == 1)
        {
          linear = this.CheckOrthoIntersect(lineList3.First<Line>(), lineList4.First<Line>());
          if (!linear)
            return false;
          break;
        }
      }
label_32:;
    }
    while (!allConsumedOrtho || !allConsumedParallel);
    if ((GeometryObject) extentLines1[0] != (GeometryObject) null && (GeometryObject) extentLines1[1] != (GeometryObject) null && (GeometryObject) extentLines2[0] != (GeometryObject) null && (GeometryObject) extentLines2[1] != (GeometryObject) null)
    {
      this._extents[0] = extentLines1[0];
      this._extents[1] = extentLines1[1];
      this._extents[2] = extentLines2[0];
      this._extents[3] = extentLines2[1];
    }
    else if (lineList3.Count == 1 && lineList4.Count == 1)
    {
      Line line1 = lineList4.First<Line>();
      Line line2 = lineList3.First<Line>();
      double num2 = line1.Origin.DotProduct(line2.Direction);
      double num3 = line2.GetEndPoint(1).DotProduct(line2.Direction) - num2;
      double num4 = line2.GetEndPoint(0).DotProduct(line2.Direction) - num2;
      Line transformed1 = line1.CreateTransformed(Transform.CreateTranslation(line2.Direction * num4)) as Line;
      Line transformed2 = line1.CreateTransformed(Transform.CreateTranslation(line2.Direction * num3)) as Line;
      Line bound1 = Line.CreateBound(transformed1.GetEndPoint(0), transformed2.GetEndPoint(0));
      Line bound2 = Line.CreateBound(transformed1.GetEndPoint(1), transformed2.GetEndPoint(1));
      this._extents[0] = bound1;
      this._extents[1] = bound2;
      this._extents[2] = transformed1;
      this._extents[3] = transformed2;
    }
    return linear;
  }

  public bool SetMemberElevations(
    Document revitDoc,
    UIDocument uiDoc,
    string levelName,
    Transform transform,
    out XYZ loopPlanDirection,
    out List<FamilyInstance> errorMembers)
  {
    errorMembers = new List<FamilyInstance>();
    loopPlanDirection = this.GetPlanDirectionForWarpingLoop();
    if (this._intersectedSFInstances.Count == 0)
      return true;
    if (this._intersectedSFInstances == null)
      return false;
    foreach (ElementId id in this._intersectedSFInstances.Select<FamilyInstance, ElementId>((Func<FamilyInstance, ElementId>) (i => i.Id)).ToList<ElementId>().Distinct<ElementId>().ToList<ElementId>())
    {
      if (!(revitDoc.GetElement(id) is FamilyInstance element))
        errorMembers.Add(element);
      else if (!this.SetElevationsForMember(AssemblyInstances.GetFlatElement(revitDoc, element), loopPlanDirection))
        errorMembers.Add(element);
    }
    return errorMembers.Count == 0;
  }

  private bool SetElevationsForMember(FamilyInstance instance, XYZ loopDir)
  {
    XYZ xyz = this._transform.Inverse.OfVector(WarpingUtils.GetPlanDirectionForSFElement(instance, this._transform));
    if (xyz == null)
      return false;
    XYZ[] xyzArray = this.CheckEndpoint(instance, xyz);
    XYZ orthoDir = xyz.CrossProduct(XYZ.BasisZ);
    Tuple<double, double> markWarp = new Tuple<double, double>(double.NaN, double.NaN);
    Tuple<double, double> oppWarp = new Tuple<double, double>(double.NaN, double.NaN);
    if (xyzArray[0] != null)
      markWarp = this.CalculateWarpAtPoint(xyzArray[2], xyz, orthoDir);
    if (xyzArray[1] != null)
      oppWarp = this.CalculateWarpAtPoint(xyzArray[3], xyz, orthoDir);
    if (xyz.IsAlmostEqualTo(loopDir.Negate()))
    {
      double num1 = markWarp.Item2 - Math.PI;
      double num2 = oppWarp.Item2 - Math.PI;
      if (num1 < -1.0 * Math.PI)
        num1 += 2.0 * Math.PI;
      if (num2 < -1.0 * Math.PI)
        num2 += 2.0 * Math.PI;
      if (markWarp != null)
        markWarp = new Tuple<double, double>(markWarp.Item1, num1);
      if (oppWarp != null)
        oppWarp = new Tuple<double, double>(oppWarp.Item1, num2);
    }
    this.ApplyWarp(instance, xyzArray[2], xyzArray[3], markWarp, oppWarp);
    return true;
  }

  private bool CheckPieceInsideLoop(XYZ[] sfPoints)
  {
    int num1 = 0;
    int num2 = 0;
    XYZ direction = WarpingUtils.CreateFlatLine(Line.CreateBound(sfPoints[0], sfPoints[1])).Direction;
    XYZ xyz1 = new XYZ(sfPoints[0].X, sfPoints[0].Y, 0.0);
    XYZ xyz2 = new XYZ(sfPoints[1].X, sfPoints[1].Y, 0.0);
    Line unbound1 = Line.CreateUnbound(xyz1, direction);
    Line unbound2 = Line.CreateUnbound(xyz2, direction);
    foreach (WarpingEdge edge in this.Edges)
    {
      Line flatCurve = edge.GetFlatCurve() as Line;
      if (flatCurve.Project(xyz1).Distance < 0.01 || flatCurve.Project(xyz2).Distance < 0.01)
        return true;
      IntersectionResultArray resultArray;
      if (unbound1.Intersect((Curve) flatCurve, out resultArray) == SetComparisonResult.Overlap && resultArray.get_Item(0).UVPoint.U > 0.0)
        ++num1;
      if (unbound2.Intersect((Curve) flatCurve, out resultArray) == SetComparisonResult.Overlap && resultArray.get_Item(0).UVPoint.U > 0.0)
        ++num2;
    }
    return num1 % 2 != 0 || num2 % 2 != 0;
  }

  private XYZ[] CheckEndpoint(FamilyInstance instance, XYZ senseOfPlanDirection)
  {
    List<XYZ> xyzList = new List<XYZ>();
    XYZ[] xyzArray1 = new XYZ[4];
    if (instance.Location is LocationPoint)
    {
      XYZ endpoint1 = this._transform.Inverse.OfPoint((instance.Location as LocationPoint).Point);
      XYZ endpoint2 = endpoint1 + senseOfPlanDirection.Normalize() * Utils.ElementUtils.Parameters.GetParameterAsZeroBasedDouble((Element) instance, "DIM_LENGTH");
      Line transformed = Line.CreateBound(endpoint1, endpoint2).CreateTransformed(this._transform) as Line;
      xyzArray1[2] = endpoint1;
      xyzArray1[3] = endpoint2;
      FamilyInstance instance1 = instance;
      Transform transform = this._transform;
      XYZ[] lineWithVoid = WarpingUtils.GetLineWithVoid(transformed, instance1, transform);
      XYZ[] xyzArray2 = new XYZ[2];
      XYZ[] endpointsWithinLoop = this.GetEndpointsWithinLoop(lineWithVoid, senseOfPlanDirection, instance.Document);
      xyzArray1[0] = endpointsWithinLoop[0];
      xyzArray1[1] = endpointsWithinLoop[1];
    }
    return xyzArray1;
  }

  private XYZ[] GetEndpointsWithinLoop(XYZ[] endPoints, XYZ senseOfPlanDir, Document revitDoc)
  {
    XYZ endPoint1 = endPoints[0];
    XYZ endPoint2 = endPoints[1];
    if (endPoint1 == null && endPoint2 == null)
      return new XYZ[2];
    XYZ origin1 = new XYZ(endPoint1.X, endPoint1.Y, 0.0);
    XYZ origin2 = new XYZ(endPoint2.X, endPoint2.Y, 0.0);
    XYZ xyz1 = senseOfPlanDir;
    XYZ xyz2 = -senseOfPlanDir;
    XYZ xyz3 = senseOfPlanDir.CrossProduct(XYZ.BasisZ.Negate());
    XYZ xyz4 = senseOfPlanDir.CrossProduct(XYZ.BasisZ);
    Line unbound1 = Line.CreateUnbound(origin1, xyz1);
    Line.CreateUnbound(origin1, xyz2);
    Line unbound2 = Line.CreateUnbound(origin1, xyz3);
    Line.CreateUnbound(origin1, xyz4);
    Line unbound3 = Line.CreateUnbound(origin2, xyz1);
    Line.CreateUnbound(origin2, xyz2);
    Line unbound4 = Line.CreateUnbound(origin2, xyz3);
    Line.CreateUnbound(origin2, xyz4);
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    bool flag5 = false;
    bool flag6 = false;
    bool flag7 = false;
    bool flag8 = false;
    bool flag9 = false;
    foreach (WarpingEdge edge in this.Edges)
    {
      edge.GetSlopeLine();
      Curve curve = edge.GetFlatCurve();
      if (edge.isCurved)
      {
        flag9 = true;
        curve = (Curve) Line.CreateBound(curve.GetEndPoint(0), curve.GetEndPoint(1));
      }
      IntersectionResultArray resultArray1 = new IntersectionResultArray();
      SetComparisonResult comparisonResult1 = curve.Intersect((Curve) unbound1, out resultArray1);
      if (comparisonResult1 != SetComparisonResult.Disjoint)
      {
        IntersectionResult intersectionResult = resultArray1.get_Item(0);
        UV uvPoint = intersectionResult.UVPoint;
        XYZ xyzPoint = intersectionResult.XYZPoint;
        if (comparisonResult1 == SetComparisonResult.Overlap && Math.Abs(uvPoint.V) < 0.001)
        {
          flag1 = true;
          flag2 = true;
          flag3 = true;
          flag4 = true;
        }
        else if (comparisonResult1 == SetComparisonResult.Subset)
        {
          bool flag10 = false;
          XYZ xyz5 = (curve.GetEndPoint(0) - origin1).Normalize();
          XYZ xyz6 = (curve.GetEndPoint(1) - origin1).Normalize();
          if (xyz5.IsAlmostEqualTo(XYZ.Zero, 0.001) || xyz6.IsAlmostEqualTo(XYZ.Zero, 0.001))
            flag10 = true;
          else if (xyz5.IsAlmostEqualTo(xyz1, 0.001) && xyz6.IsAlmostEqualTo(xyz2, 0.001))
            flag10 = true;
          else if (xyz5.IsAlmostEqualTo(xyz2, 0.001) && xyz6.IsAlmostEqualTo(xyz1, 0.001))
            flag10 = true;
          if (flag10)
          {
            flag1 = true;
            flag2 = true;
            flag3 = true;
            flag4 = true;
          }
        }
        else if (uvPoint.V > 0.0 && !flag1)
          flag1 = true;
        else if (uvPoint.V < 0.0 && !flag2)
          flag2 = true;
      }
      IntersectionResultArray resultArray2 = new IntersectionResultArray();
      SetComparisonResult comparisonResult2 = curve.Intersect((Curve) unbound2, out resultArray2);
      if (comparisonResult2 != SetComparisonResult.Disjoint)
      {
        IntersectionResult intersectionResult = resultArray2.get_Item(0);
        UV uvPoint = intersectionResult.UVPoint;
        XYZ xyzPoint = intersectionResult.XYZPoint;
        if (comparisonResult2 == SetComparisonResult.Overlap && Math.Abs(uvPoint.V) < 0.001)
        {
          flag1 = true;
          flag2 = true;
          flag3 = true;
          flag4 = true;
        }
        else if (comparisonResult2 == SetComparisonResult.Subset)
        {
          bool flag11 = false;
          XYZ xyz7 = (curve.GetEndPoint(0) - origin1).Normalize();
          XYZ xyz8 = (curve.GetEndPoint(1) - origin1).Normalize();
          if (xyz7.IsAlmostEqualTo(XYZ.Zero, 0.001) || xyz8.IsAlmostEqualTo(XYZ.Zero, 0.001))
            flag11 = true;
          else if (xyz7.IsAlmostEqualTo(xyz3, 0.001) && xyz8.IsAlmostEqualTo(xyz4, 0.001))
            flag11 = true;
          else if (xyz7.IsAlmostEqualTo(xyz4, 0.001) && xyz8.IsAlmostEqualTo(xyz3, 0.001))
            flag11 = true;
          if (flag11)
          {
            flag1 = true;
            flag2 = true;
            flag3 = true;
            flag4 = true;
          }
        }
        else if (uvPoint.V > 0.0 && !flag3)
          flag3 = true;
        else if (uvPoint.V < 0.0 && !flag4)
          flag4 = true;
      }
      if (!(flag1 & flag2 & flag3 & flag4))
      {
        if (flag1 & flag2 & flag3 & flag4)
          break;
      }
      else
        break;
    }
    foreach (WarpingEdge edge in this.Edges)
    {
      edge.GetSlopeLine();
      Curve curve = edge.GetFlatCurve();
      if (edge.isCurved)
      {
        flag9 = true;
        curve = (Curve) Line.CreateBound(curve.GetEndPoint(0), curve.GetEndPoint(1));
      }
      IntersectionResultArray resultArray3 = new IntersectionResultArray();
      SetComparisonResult comparisonResult3 = curve.Intersect((Curve) unbound3, out resultArray3);
      if (comparisonResult3 != SetComparisonResult.Disjoint)
      {
        IntersectionResult intersectionResult = resultArray3.get_Item(0);
        UV uvPoint = intersectionResult.UVPoint;
        XYZ xyzPoint = intersectionResult.XYZPoint;
        if (comparisonResult3 == SetComparisonResult.Overlap && Math.Abs(uvPoint.V) < 0.001)
        {
          flag5 = true;
          flag6 = true;
          flag7 = true;
          flag8 = true;
        }
        else if (comparisonResult3 == SetComparisonResult.Subset)
        {
          bool flag12 = false;
          XYZ xyz9 = (curve.GetEndPoint(0) - origin2).Normalize();
          XYZ xyz10 = (curve.GetEndPoint(1) - origin2).Normalize();
          if (xyz9.IsAlmostEqualTo(XYZ.Zero, 0.001) || xyz10.IsAlmostEqualTo(XYZ.Zero, 0.001))
            flag12 = true;
          else if (xyz9.IsAlmostEqualTo(xyz1, 0.001) && xyz10.IsAlmostEqualTo(xyz2, 0.001))
            flag12 = true;
          else if (xyz9.IsAlmostEqualTo(xyz2, 0.001) && xyz10.IsAlmostEqualTo(xyz1, 0.001))
            flag12 = true;
          if (flag12)
          {
            flag5 = true;
            flag6 = true;
            flag7 = true;
            flag8 = true;
          }
        }
        else if (uvPoint.V > 0.0 && !flag5)
          flag5 = true;
        else if (uvPoint.V < 0.0 && !flag6)
          flag6 = true;
      }
      IntersectionResultArray resultArray4 = new IntersectionResultArray();
      SetComparisonResult comparisonResult4 = curve.Intersect((Curve) unbound4, out resultArray4);
      if (comparisonResult4 != SetComparisonResult.Disjoint)
      {
        IntersectionResult intersectionResult = resultArray4.get_Item(0);
        UV uvPoint = intersectionResult.UVPoint;
        XYZ xyzPoint = intersectionResult.XYZPoint;
        if (comparisonResult4 == SetComparisonResult.Overlap && Math.Abs(uvPoint.V) < 0.001)
        {
          flag5 = true;
          flag6 = true;
          flag7 = true;
          flag8 = true;
        }
        else if (comparisonResult4 == SetComparisonResult.Subset)
        {
          bool flag13 = false;
          XYZ xyz11 = (curve.GetEndPoint(0) - origin2).Normalize();
          XYZ xyz12 = (curve.GetEndPoint(1) - origin2).Normalize();
          if (xyz11.IsAlmostEqualTo(XYZ.Zero, 0.001) || xyz12.IsAlmostEqualTo(XYZ.Zero, 0.001))
            flag13 = true;
          else if (xyz11.IsAlmostEqualTo(xyz3, 0.001) && xyz12.IsAlmostEqualTo(xyz4, 0.001))
            flag13 = true;
          else if (xyz11.IsAlmostEqualTo(xyz4, 0.001) && xyz12.IsAlmostEqualTo(xyz3, 0.001))
            flag13 = true;
          if (flag13)
          {
            flag5 = true;
            flag6 = true;
            flag7 = true;
            flag8 = true;
          }
        }
        else if (uvPoint.V > 0.0 && !flag7)
          flag7 = true;
        else if (uvPoint.V < 0.0 && !flag8)
          flag8 = true;
      }
      if (!(flag5 & flag6 & flag7 & flag8))
      {
        if (flag5 & flag6 & flag7 & flag8)
          break;
      }
      else
        break;
    }
    XYZ[] endpointsWithinLoop = new XYZ[2];
    if (flag1 & flag2 & flag3 & flag4)
      endpointsWithinLoop[0] = endPoint1;
    if (flag5 & flag6 & flag7 & flag8)
      endpointsWithinLoop[1] = endPoint2;
    if (endpointsWithinLoop[0] != null && endpointsWithinLoop[1] != null || !flag9)
      return endpointsWithinLoop;
    if (endpointsWithinLoop[0] != null && endpointsWithinLoop[1] == null)
    {
      Line bound1 = Line.CreateBound(endPoint1, endPoint2);
      Line bound2 = Line.CreateBound(new XYZ(endPoint1.X, endPoint1.Y, 0.0), new XYZ(endPoint2.X, endPoint2.Y, 0.0));
      foreach (WarpingEdge edge in this.Edges)
      {
        if (edge.isCurved)
        {
          Curve flatCurve = edge.GetFlatCurve();
          IntersectionResultArray intersectionResultArray = new IntersectionResultArray();
          Line line = bound2;
          ref IntersectionResultArray local = ref intersectionResultArray;
          if (flatCurve.Intersect((Curve) line, out local) == SetComparisonResult.Overlap)
          {
            UV uvPoint = intersectionResultArray.get_Item(0).UVPoint;
            if (uvPoint.V > 0.0)
              endpointsWithinLoop[1] = bound1.Evaluate(bound2.ComputeNormalizedParameter(uvPoint.V), true);
          }
        }
      }
    }
    else if (endpointsWithinLoop[0] == null && endpointsWithinLoop[1] != null)
    {
      Line bound3 = Line.CreateBound(endPoint2, endPoint1);
      Line bound4 = Line.CreateBound(new XYZ(endPoint2.X, endPoint2.Y, 0.0), new XYZ(endPoint1.X, endPoint1.Y, 0.0));
      foreach (WarpingEdge edge in this.Edges)
      {
        if (edge.isCurved)
        {
          Curve flatCurve = edge.GetFlatCurve();
          IntersectionResultArray intersectionResultArray = new IntersectionResultArray();
          Line line = bound4;
          ref IntersectionResultArray local = ref intersectionResultArray;
          if (flatCurve.Intersect((Curve) line, out local) == SetComparisonResult.Overlap)
          {
            UV uvPoint = intersectionResultArray.get_Item(0).UVPoint;
            if (uvPoint.V > 0.0)
              endpointsWithinLoop[0] = bound3.Evaluate(bound4.ComputeNormalizedParameter(uvPoint.V), true);
          }
        }
      }
    }
    else if (endpointsWithinLoop[0] == null && endpointsWithinLoop[1] == null)
    {
      Line bound5 = Line.CreateBound(endPoint1, endPoint2);
      Line bound6 = Line.CreateBound(new XYZ(endPoint1.X, endPoint1.Y, 0.0), new XYZ(endPoint2.X, endPoint2.Y, 0.0));
      foreach (WarpingEdge edge in this.Edges)
      {
        if (edge.isCurved)
        {
          Curve flatCurve = edge.GetFlatCurve();
          IntersectionResultArray intersectionResultArray = new IntersectionResultArray();
          Line line = bound6;
          ref IntersectionResultArray local = ref intersectionResultArray;
          if (flatCurve.Intersect((Curve) line, out local) == SetComparisonResult.Overlap)
          {
            IntersectionResult intersectionResult1 = intersectionResultArray.get_Item(0);
            IntersectionResult intersectionResult2 = (IntersectionResult) null;
            if (intersectionResultArray.Size >= 2)
              intersectionResult2 = intersectionResultArray.get_Item(1);
            UV uvPoint = intersectionResult1.UVPoint;
            UV uv = (UV) null;
            if (intersectionResult2 != null)
              uv = intersectionResult2.UVPoint;
            if (uvPoint.V > 0.0)
            {
              endpointsWithinLoop[0] = bound5.Evaluate(bound6.ComputeNormalizedParameter(uvPoint.V), true);
              endpointsWithinLoop[1] = uv == null ? endPoint2 : bound5.Evaluate(bound6.ComputeNormalizedParameter(uv.V), true);
            }
          }
        }
      }
    }
    return endpointsWithinLoop;
  }

  private bool GetEndpointsWithinLoopDS(XYZ endPoint, XYZ direction, Document revitDoc)
  {
    XYZ xyz1 = endPoint;
    XYZ origin = new XYZ(xyz1.X, xyz1.Y, 0.0);
    XYZ direction1 = direction;
    XYZ direction2 = -direction;
    XYZ direction3 = direction.CrossProduct(XYZ.BasisZ.Negate());
    XYZ direction4 = direction.CrossProduct(XYZ.BasisZ);
    Line unbound1 = Line.CreateUnbound(origin, direction1);
    Line.CreateUnbound(origin, direction2);
    Line unbound2 = Line.CreateUnbound(origin, direction3);
    Line.CreateUnbound(origin, direction4);
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    foreach (WarpingEdge edge in this.Edges)
    {
      edge.GetSlopeLine();
      Curve flatCurve1 = edge.GetFlatCurve();
      IntersectionResultArray resultArray1 = new IntersectionResultArray();
      if (flatCurve1.Intersect((Curve) unbound1, out resultArray1) == SetComparisonResult.Overlap)
      {
        IntersectionResult intersectionResult = resultArray1.get_Item(0);
        UV uvPoint = intersectionResult.UVPoint;
        XYZ xyzPoint = intersectionResult.XYZPoint;
        if (Math.Abs(uvPoint.V) < 0.5 && edge.ContainingLoops.Count == 1)
        {
          flag1 = true;
          flag2 = true;
          flag3 = true;
          flag4 = true;
        }
        if (uvPoint.V > 0.0 && !flag1)
          flag1 = true;
        else if (uvPoint.V < 0.0 && !flag2)
          flag2 = true;
      }
      resultArray1 = new IntersectionResultArray();
      if (flatCurve1.Intersect((Curve) unbound2, out resultArray1) == SetComparisonResult.Overlap)
      {
        IntersectionResult intersectionResult = resultArray1.get_Item(0);
        UV uvPoint = intersectionResult.UVPoint;
        XYZ xyzPoint = intersectionResult.XYZPoint;
        if (Math.Abs(uvPoint.V) < 0.5 && edge.ContainingLoops.Count == 1)
        {
          flag3 = true;
          flag4 = true;
          flag1 = true;
          flag2 = true;
        }
        if (uvPoint.V > 0.0 && !flag3)
          flag3 = true;
        else if (uvPoint.V < 0.0 && !flag4)
          flag4 = true;
      }
      if (!(flag1 & flag2 & flag3 & flag4))
      {
        if (edge.isCurved)
        {
          Curve flatCurve2 = edge.GetFlatCurve();
          XYZ xyz2 = edge.actualCV.Evaluate(0.5, true);
          XYZ source = flatCurve2.Evaluate(0.5, true);
          double num1 = xyz2.DistanceTo(source);
          XYZ xyz3 = (xyz2 - source).Normalize();
          double x1 = origin.DistanceTo(flatCurve2.GetEndPoint(0));
          double x2 = (origin - flatCurve2.GetEndPoint(0)).DotProduct((flatCurve2 as Line).Direction);
          if (Math.Sqrt(Math.Pow(x1, 2.0) - Math.Pow(x2, 2.0)) <= num1)
          {
            CurveLoop curveLoop = new CurveLoop();
            Line bound1 = Line.CreateBound(flatCurve1.GetEndPoint(0), flatCurve1.GetEndPoint(1));
            Line bound2 = Line.CreateBound(flatCurve1.GetEndPoint(1), flatCurve1.GetEndPoint(1) + xyz3 * num1);
            Line bound3 = Line.CreateBound(bound2.GetEndPoint(1), bound2.GetEndPoint(1) + bound1.Direction.Negate() * bound1.Length);
            Line bound4 = Line.CreateBound(bound3.GetEndPoint(1), bound1.GetEndPoint(0));
            using (SubTransaction subTransaction = new SubTransaction(revitDoc))
            {
              int num2 = (int) subTransaction.Start();
              int num3 = (int) subTransaction.Commit();
            }
            curveLoop.Append((Curve) bound1);
            curveLoop.Append((Curve) bound2);
            curveLoop.Append((Curve) bound3);
            curveLoop.Append((Curve) bound4);
            foreach (Curve curve in curveLoop)
            {
              IntersectionResultArray resultArray2 = new IntersectionResultArray();
              if (curve.Intersect((Curve) unbound1, out resultArray2) == SetComparisonResult.Overlap)
              {
                IntersectionResult intersectionResult = resultArray2.get_Item(0);
                UV uvPoint = intersectionResult.UVPoint;
                XYZ xyzPoint = intersectionResult.XYZPoint;
                if (uvPoint.V > 0.0 && !flag1)
                  flag1 = true;
                else if (uvPoint.V < 0.0 && !flag2)
                  flag2 = true;
              }
              IntersectionResultArray resultArray3 = new IntersectionResultArray();
              if (curve.Intersect((Curve) unbound2, out resultArray3) == SetComparisonResult.Overlap)
              {
                IntersectionResult intersectionResult = resultArray3.get_Item(0);
                UV uvPoint = intersectionResult.UVPoint;
                XYZ xyzPoint = intersectionResult.XYZPoint;
                if (uvPoint.V > 0.0 && !flag3)
                  flag3 = true;
                else if (uvPoint.V < 0.0 && !flag4)
                  flag4 = true;
              }
            }
          }
        }
      }
      else
        break;
    }
    XYZ[] xyzArray = new XYZ[2];
    return flag1 & flag2 & flag3 & flag4;
  }

  private Solid CreateSolidFromPoint(XYZ point)
  {
    double num = 0.2;
    XYZ xyz1 = new XYZ(point.X - num, point.Y - num, point.Z - num);
    XYZ xyz2 = new XYZ(point.X + num, point.Y - num, point.Z - num);
    XYZ xyz3 = new XYZ(point.X + num, point.Y + num, point.Z - num);
    XYZ xyz4 = new XYZ(point.X - num, point.Y + num, point.Z - num);
    Line bound1 = Line.CreateBound(xyz1, xyz2);
    Line bound2 = Line.CreateBound(xyz2, xyz3);
    Line bound3 = Line.CreateBound(xyz3, xyz4);
    Line bound4 = Line.CreateBound(xyz4, xyz1);
    CurveLoop curveLoop = new CurveLoop();
    curveLoop.Append((Curve) bound1);
    curveLoop.Append((Curve) bound2);
    curveLoop.Append((Curve) bound3);
    curveLoop.Append((Curve) bound4);
    return GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
    {
      curveLoop
    }, XYZ.BasisZ, num * 2.0);
  }

  private Solid GetWarpLoopSolid()
  {
    if (this.Edges == null)
      return (Solid) null;
    IList<CurveLoop> profileLoops = (IList<CurveLoop>) new List<CurveLoop>();
    CurveLoop loop = this.GetLoop(this.Edges.SelectMany<WarpingEdge, XYZ>((Func<WarpingEdge, IEnumerable<XYZ>>) (edge => (IEnumerable<XYZ>) new List<XYZ>()
    {
      edge.StartPoint.Point,
      edge.EndPoint.Point
    })).Select<XYZ, double>((Func<XYZ, double>) (point => point.Z)).Max());
    if (loop == null)
      return (Solid) null;
    if (loop.IsOpen())
      return (Solid) null;
    loop.Transform(Transform.CreateTranslation(new XYZ(0.0, 0.0, 20.0)));
    profileLoops.Add(loop);
    return GeometryCreationUtilities.CreateExtrusionGeometry(profileLoops, XYZ.BasisZ.Negate(), 40.0);
  }

  private double GetSlope(Line line)
  {
    return line.Direction.Z / new XYZ(line.Direction.X, line.Direction.Y, 0.0).GetLength();
  }

  private bool CheckSlopeLinearity(List<Line> lines, XYZ basisVec)
  {
    if (lines.Count < 3)
      return true;
    List<XYZ> source = new List<XYZ>();
    foreach (Line line in lines)
    {
      double slope = this.GetSlope(line);
      double x = line.GetEndPoint(0).DotProduct(basisVec);
      source.Add(new XYZ(x, slope, 0.0));
    }
    List<XYZ> list = source.OrderBy<XYZ, double>((Func<XYZ, double>) (pvs => pvs.X)).ToList<XYZ>();
    if (list.Count > 1)
    {
      Line unbound = Line.CreateUnbound(list[0], (list[1] - list[0]).Normalize());
      foreach (XYZ xyz in list)
      {
        double parameter = xyz.X - unbound.Origin.X;
        if (Math.Abs(unbound.Evaluate(parameter, false).Y - xyz.Y) > 0.01)
          return false;
      }
    }
    return true;
  }

  private Tuple<double, double> CalculateWarpAtPoint(XYZ warpPoint, XYZ planDir, XYZ orthoDir)
  {
    Line flatLine1 = WarpingUtils.CreateFlatLine(Line.CreateUnbound(this._extents[0].Origin, this._extents[0].Direction));
    Line flatLine2 = WarpingUtils.CreateFlatLine(this._extents[0]);
    Line flatLine3 = WarpingUtils.CreateFlatLine(Line.CreateUnbound(this._extents[1].Origin, this._extents[1].Direction));
    Line flatLine4 = WarpingUtils.CreateFlatLine(this._extents[1]);
    XYZ point = new XYZ(warpPoint.X, warpPoint.Y, 0.0);
    double normalizedParameter1 = flatLine2.ComputeNormalizedParameter(flatLine1.Project(point).Parameter);
    double parameter = flatLine3.Project(point).Parameter;
    double normalizedParameter2 = flatLine4.ComputeNormalizedParameter(parameter);
    double rawParameter1 = this._extents[0].ComputeRawParameter(normalizedParameter1);
    double rawParameter2 = this._extents[1].ComputeRawParameter(normalizedParameter2);
    XYZ endpoint1 = this._extents[0].Evaluate(rawParameter1, false);
    XYZ xyz = this._extents[1].Evaluate(rawParameter2, false);
    double num1 = 0.0;
    if (endpoint1.IsAlmostEqualTo(xyz, 0.001))
      return new Tuple<double, double>(endpoint1.Z, num1);
    Line bound = Line.CreateBound(endpoint1, xyz);
    Line flatLine5 = WarpingUtils.CreateFlatLine(bound);
    double z = bound.Evaluate(bound.ComputeRawParameter(flatLine5.ComputeNormalizedParameter(point.DistanceTo(flatLine5.Origin))), false).Z;
    double num2 = bound.Direction.AngleOnPlaneTo(orthoDir, planDir);
    if (num2 > Math.PI)
      num2 -= 2.0 * Math.PI;
    return new Tuple<double, double>(z, num2);
  }

  private bool ApplyWarp(
    FamilyInstance instance,
    XYZ markPoint,
    XYZ oppPoint,
    Tuple<double, double> markWarp,
    Tuple<double, double> oppWarp)
  {
    double num = 0.0;
    if (instance.LookupParameter("Offset from Host") != null)
      num = instance.GetTopLevelElement().LookupParameter("Offset from Host").AsDouble();
    if (instance.LookupParameter("IsMirrored") != null)
      instance.GetTopLevelElement().LookupParameter("IsMirrored").AsInteger().ToString();
    XYZ[] xyzArray1 = new XYZ[4];
    XYZ[] xyzArray2 = new XYZ[4];
    XYZ[] xyzArray3 = new XYZ[4];
    FamilyInstance topLevelElement = instance.GetTopLevelElement() as FamilyInstance;
    Parameter parameter1 = topLevelElement.LookupParameter("Manual_Mark_End_Offset");
    Parameter parameter2 = topLevelElement.LookupParameter("Manual_Mark_End_Warp_Angle");
    Parameter parameter3 = topLevelElement.LookupParameter("Vertical_Offset_MarkEnd");
    Parameter parameter4 = topLevelElement.LookupParameter("Manual_Opp_End_Offset");
    Parameter parameter5 = topLevelElement.LookupParameter("Manual_Opp_End_Warp_Angle");
    Parameter parameter6 = topLevelElement.LookupParameter("Vertical_Offset_OppEnd");
    if (markPoint != null)
    {
      if (parameter1 != null && !parameter1.IsReadOnly && !double.IsNaN(markWarp.Item1))
        parameter1.Set(markWarp.Item1 - markPoint.Z + num);
      if (parameter2 != null && !parameter2.IsReadOnly && !double.IsNaN(markWarp.Item2))
      {
        if (topLevelElement.Mirrored)
          parameter2.Set(markWarp.Item2);
        else
          parameter2.Set(-markWarp.Item2);
      }
      if (parameter3 != null && !parameter3.IsReadOnly && !double.IsNaN(markWarp.Item1))
        parameter3.Set(markWarp.Item1 - markPoint.Z + num);
    }
    if (oppPoint != null)
    {
      if (parameter4 != null && !parameter4.IsReadOnly && !double.IsNaN(oppWarp.Item1))
        parameter4.Set(oppWarp.Item1 - oppPoint.Z + num);
      if (parameter5 != null && !parameter5.IsReadOnly && !double.IsNaN(oppWarp.Item2))
      {
        if (topLevelElement.Mirrored)
          parameter5.Set(oppWarp.Item2);
        else
          parameter5.Set(-oppWarp.Item2);
      }
      if (parameter6 != null && !parameter6.IsReadOnly && !double.IsNaN(oppWarp.Item1))
        parameter6.Set(oppWarp.Item1 - oppPoint.Z + num);
    }
    return true;
  }

  private XYZ[] CalculateOutlinePointDS(
    XYZ warpPoint,
    XYZ senseOfPlanDirection,
    WarpingEdgeLoop wLoop)
  {
    XYZ xyz1 = new XYZ(warpPoint.X, warpPoint.Y, 0.0);
    List<WarpingEdge> edges = wLoop.Edges;
    XYZ direction1 = senseOfPlanDirection;
    XYZ direction2 = -senseOfPlanDirection;
    XYZ direction3 = senseOfPlanDirection.CrossProduct(XYZ.BasisZ.Negate());
    XYZ[] outlinePointDs = new XYZ[6];
    Line unbound1 = Line.CreateUnbound(xyz1, direction1);
    Line.CreateUnbound(xyz1, direction2);
    Line unbound2 = Line.CreateUnbound(xyz1, direction3);
    Line.CreateUnbound(xyz1, direction3.Negate());
    Line line1 = unbound1;
    XYZ xyz2 = (XYZ) null;
    XYZ xyz3 = (XYZ) null;
    while (xyz2 == null && xyz3 == null)
    {
      foreach (WarpingEdge warpingEdge in edges)
      {
        Curve slopeLine = (Curve) warpingEdge.GetSlopeLine();
        Curve flatCurve = warpingEdge.GetFlatCurve();
        if (this.IsEdgeParalleltoLine(line1.Direction, (flatCurve as Line).Direction))
        {
          Line unbound3 = Line.CreateUnbound(flatCurve.Evaluate(0.5, true), direction3);
          IntersectionResultArray intersectionResultArray = new IntersectionResultArray();
          Line line2 = line1;
          ref IntersectionResultArray local = ref intersectionResultArray;
          if (unbound3.Intersect((Curve) line2, out local) == SetComparisonResult.Overlap)
          {
            IntersectionResult intersectionResult = intersectionResultArray.get_Item(0);
            UV uvPoint = intersectionResult.UVPoint;
            XYZ xyzPoint = intersectionResult.XYZPoint;
            if (Math.Abs(uvPoint.U) < 0.001)
            {
              xyz2 = warpingEdge.StartPoint.Point;
              xyz3 = warpingEdge.EndPoint.Point;
              break;
            }
          }
        }
        IntersectionResultArray resultArray = new IntersectionResultArray();
        if (flatCurve.Intersect((Curve) line1, out resultArray) == SetComparisonResult.Overlap)
        {
          IntersectionResult intersectionResult = resultArray.get_Item(0);
          UV uvPoint = intersectionResult.UVPoint;
          XYZ xyzPoint = intersectionResult.XYZPoint;
          if (Math.Abs(uvPoint.V) < 0.01)
          {
            xyz2 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
            xyz3 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
          }
          else if (uvPoint.V > 0.0 && xyz2 == null)
            xyz2 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
          else if (uvPoint.V > 0.0 && xyz2 != null)
          {
            XYZ xyz4 = flatCurve.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
            if (new XYZ(xyz2.X, xyz2.Y, 0.0).DistanceTo(xyz1) > xyz4.DistanceTo(xyz1))
            {
              xyz3 = xyz2;
              xyz2 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
            }
            else
              xyz3 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
          }
          else if (uvPoint.V < 0.0 && xyz3 == null)
            xyz3 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
          else if (uvPoint.V < 0.0 && xyz3 != null)
          {
            XYZ xyz5 = flatCurve.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
            if (new XYZ(xyz3.X, xyz3.Y, 0.0).DistanceTo(xyz1) > xyz5.DistanceTo(xyz1))
            {
              xyz2 = xyz3;
              xyz3 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
            }
            else
              xyz2 = slopeLine.Evaluate(flatCurve.ComputeNormalizedParameter(uvPoint.U), true);
          }
        }
      }
      line1 = unbound2;
    }
    outlinePointDs[0] = xyz2;
    outlinePointDs[1] = xyz3;
    return outlinePointDs;
  }

  private double CalculateVerticalOffset_LEGACY(XYZ endPoint1, XYZ endPoint2, XYZ warpPoint)
  {
    double z1 = endPoint1.Z;
    double z2 = endPoint2.Z;
    XYZ xyz = new XYZ(endPoint1.X, endPoint1.Y, 0.0);
    XYZ source1 = new XYZ(endPoint2.X, endPoint2.Y, 0.0);
    XYZ source2 = new XYZ(warpPoint.X, warpPoint.Y, 0.0);
    double d;
    if (xyz.IsAlmostEqualTo(source1) && xyz.IsAlmostEqualTo(source2) && source1.IsAlmostEqualTo(source2))
    {
      d = z2;
    }
    else
    {
      double num1 = xyz.DistanceTo(source1);
      double num2 = source2.DistanceTo(source1);
      XYZ source3 = (source1 - xyz).Normalize();
      double a = (source1 - source2).Normalize().DotProduct(source3);
      if (!Util.IsZero(a) && a < 0.0)
        num2 = -num2;
      d = z2 - (z2 - z1) / num1 * num2;
      if (double.IsNaN(d))
        d = z2;
    }
    return d;
  }

  private double CalculateSlopeAngle(XYZ endPoint1, XYZ endPoint2)
  {
    double z1 = endPoint1.Z;
    double z2 = endPoint2.Z;
    double num1 = new XYZ(endPoint1.X, endPoint1.Y, 0.0).DistanceTo(new XYZ(endPoint2.X, endPoint2.Y, 0.0));
    double num2 = z1;
    double num3 = z2 - num2;
    return num1 == 0.0 ? 0.0 : Math.Atan(num3 / num1);
  }

  public CurveLoop GetLoop(double topZ = 0.0)
  {
    CurveLoop loop = new CurveLoop();
    WarpingEdge warpingEdge = this.Edges.First<WarpingEdge>();
    if (warpingEdge == null)
      return (CurveLoop) null;
    SpotElevationPoint startingPoint0 = warpingEdge.StartPoint;
    SpotElevationPoint startingPoint1 = warpingEdge.StartPoint;
    if (this.Edges.Count > 1 && (warpingEdge.StartPoint.planePoint.IsAlmostEqualTo(this.Edges[1].StartPoint.planePoint) || warpingEdge.StartPoint.planePoint.IsAlmostEqualTo(this.Edges[1].EndPoint.planePoint)))
    {
      startingPoint0 = warpingEdge.EndPoint;
      startingPoint1 = warpingEdge.EndPoint;
    }
    foreach (WarpingEdge edge in this.Edges)
    {
      Curve flatCurve = (Curve) (edge.GetFlatCurve(topZ, startingPoint0, startingPoint1) as Line);
      try
      {
        loop.Append(flatCurve);
      }
      catch (Autodesk.Revit.Exceptions.ArgumentException ex)
      {
        TaskDialog.Show("Auto-Warping", "One or more warping loops in the project could not be constructed. Please ensure that all Spot Elevations form convex polygons around warpable elements.");
        return (CurveLoop) null;
      }
      startingPoint0 = edge.StartPoint;
      startingPoint1 = edge.EndPoint;
    }
    return loop;
  }

  public static bool PassesWarpableCriteria(FamilyInstance e, bool dsOnly = true)
  {
    List<string> source1 = new List<string>();
    source1.Add("EMBED");
    source1.Add("CIP");
    source1.Add("ERECTION");
    List<string> source2 = new List<string>()
    {
      "CORBEL",
      "LEDGE",
      "LDGE",
      "ADDON",
      "CONNECTOR_COMPONENT"
    };
    string mfgComponent = Utils.ElementUtils.Parameters.GetParameterAsString((Element) e, "MANUFACTURE_COMPONENT");
    if (source1.Where<string>((Func<string, bool>) (term => mfgComponent.ToUpper().Contains(term))).Count<string>() > 0)
      return true;
    if (!dsOnly)
    {
      if (source2.Where<string>((Func<string, bool>) (term => mfgComponent.ToUpper().Contains(term))).Count<string>() > 0)
        return true;
      string familyName = e.Symbol.FamilyName;
      if (source2.Where<string>((Func<string, bool>) (term => familyName.ToUpper().Contains(term))).Count<string>() > 0)
        return true;
    }
    return false;
  }

  public void CreateDirectShapesOfEmbeds(
    string levelName,
    Dictionary<string, List<string>> entourages,
    XYZ loopPlan,
    List<ElementId> warpedDS,
    List<FamilyInstance> levelConns)
  {
    if (this._intersectedSFInstances == null || this._intersectedSFInstances.Count<FamilyInstance>() == 0)
      return;
    List<FamilyInstance> source1 = new List<FamilyInstance>();
    List<FamilyInstance> familyInstanceList1 = new List<FamilyInstance>();
    List<Solid> sList = new List<Solid>();
    Document document = (Document) null;
    if (this._intersectedSFInstances.Count == 0)
      return;
    if (this._intersectedSFInstances.Count > 0)
      document = this._intersectedSFInstances.First<FamilyInstance>().Document;
    foreach (FamilyInstance intersectedSfInstance in this._intersectedSFInstances)
    {
      FamilyInstance flatElement = AssemblyInstances.GetFlatElement(document, intersectedSfInstance);
      familyInstanceList1.Add(flatElement);
      sList.AddRange((IEnumerable<Solid>) Solids.GetInstanceSolids((Element) flatElement));
    }
    Solid masterSolid = this.GetMasterSolid(sList);
    BoundingBoxXYZ boundingBox = masterSolid.GetBoundingBox();
    boundingBox.Min = boundingBox.Transform.OfPoint(boundingBox.Min);
    boundingBox.Max = boundingBox.Transform.OfPoint(boundingBox.Max);
    BoundingBoxIntersectsFilter filter1 = new BoundingBoxIntersectsFilter(new Outline(boundingBox.Min, boundingBox.Max));
    ElementMulticategoryFilter filter2 = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_SpecialityEquipment,
      BuiltInCategory.OST_GenericModel
    });
    List<FamilyInstance> list1 = new FilteredElementCollector(document).WherePasses((ElementFilter) filter2).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => !warpedDS.Contains(famInst.Id))).Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => Utils.ElementUtils.Parameters.GetParameterAsString((Element) famInst, "Work Plane").Equals(levelName) || Utils.ElementUtils.Parameters.GetParameterAsString((Element) famInst, "Host").Equals(levelName))).Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => WarpingEdgeLoop.PassesWarpableCriteria(famInst, false))).ToList<FamilyInstance>();
    source1.AddRange((IEnumerable<FamilyInstance>) list1);
    List<ElementId> source2 = new List<ElementId>();
    source2.AddRange((IEnumerable<ElementId>) warpedDS);
    List<ElementId> list2 = source2.Distinct<ElementId>().ToList<ElementId>();
    FilteredElementCollector elementCollector = list2.Count == 0 ? new FilteredElementCollector(document).WherePasses((ElementFilter) filter2) : new FilteredElementCollector(document).WherePasses((ElementFilter) filter2).WherePasses((ElementFilter) new ExclusionFilter((ICollection<ElementId>) list2));
    elementCollector.WherePasses((ElementFilter) filter1).WherePasses((ElementFilter) new ElementIntersectsSolidFilter(masterSolid)).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>();
    List<FamilyInstance> familyInstanceList2 = new List<FamilyInstance>();
    foreach (ElementId elementId in (IEnumerable<ElementId>) elementCollector.ToElementIds())
    {
      Element element = (Element) (document.GetElement(elementId) as FamilyInstance);
      if (element is FamilyInstance)
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        if (WarpingEdgeLoop.PassesWarpableCriteria(familyInstance, false))
        {
          warpedDS.Add(familyInstance.Id);
          warpedDS.Add(familyInstance.GetTopLevelElement().Id);
          source1.Add(familyInstance);
          familyInstanceList2.Add(familyInstance);
        }
      }
    }
    warpedDS = warpedDS.Distinct<ElementId>().ToList<ElementId>();
    HashSet<ElementId> elementIdSet = new HashSet<ElementId>();
    Parameter parameter1 = document.ProjectInformation.LookupParameter("AUTO_WARPING_CREATE_DIRECT_SHAPES");
    source1.AddRange((IEnumerable<FamilyInstance>) levelConns);
    List<FamilyInstance> list3 = source1.Distinct<FamilyInstance>().ToList<FamilyInstance>();
    bool isDSGlobal = true;
    if (parameter1 != null && parameter1.AsValueString() == "No")
      isDSGlobal = false;
    if (list3.Count <= 0)
      return;
    List<FamilyInstance> source3 = new List<FamilyInstance>();
    for (int index = 0; index < list3.Count; ++index)
    {
      FamilyInstance elem1 = list3[index];
      FamilyInstance topLevelElement = elem1.GetTopLevelElement() as FamilyInstance;
      if (!elementIdSet.Contains(topLevelElement.Id))
      {
        XYZ warpPoint = this._transform.Inverse.OfPoint(topLevelElement.GetTransform().Origin);
        XYZ endPoint = this._transform.Inverse.OfPoint(elem1.GetTransform().Origin);
        Element host = topLevelElement.Host;
        if (familyInstanceList2.Contains(elem1) || this.GetEndpointsWithinLoopDS(endPoint, loopPlan, elem1.Document))
        {
          elementIdSet.Add(topLevelElement.Id);
          this.ClearOldDS(entourages, (Element) topLevelElement);
          double z = this.CalculateWarpAtPoint(warpPoint, loopPlan, loopPlan.CrossProduct(XYZ.BasisZ)).Item1;
          XYZ xyz = new XYZ(0.0, 0.0, z);
          List<DirectShape> source4 = new List<DirectShape>();
          string uniqueId = topLevelElement.UniqueId;
          if (elem1 != null)
          {
            if (elem1.SuperComponent != null)
            {
              string name1 = elem1.SuperComponent.Name;
            }
            else
            {
              string name2 = elem1.Name;
            }
            elem1.Symbol.GetControlMark();
          }
          string manufactureComponent = elem1.GetManufactureComponent();
          XYZ moveDist = this._transform.BasisZ * xyz.Z;
          double num1 = z;
          Utils.ElementUtils.Parameters.GetParameterAsBool((Element) topLevelElement, "IS_WARPABLE_SHAPE");
          Parameter parameter2 = topLevelElement.LookupParameter("AUTO_WARP_DISPLACEMENT");
          int num2 = isDSGlobal ? 1 : 0;
          if (parameter2 != null && parameter2.IsReadOnly)
          {
            source3.Add(elem1);
          }
          else
          {
            bool isRawCons = this.CheckRawCons(topLevelElement);
            if (manufactureComponent.ToUpper().Contains("RAW CONSUMABLE") | isRawCons)
            {
              if (source4.Count<DirectShape>() != 0)
              {
                foreach (DirectShape directShapeForRawCon in this.CreateDirectShapeForRawCons(topLevelElement, moveDist, isRawCons, uniqueId, isDSGlobal))
                {
                  if (!source4.Contains(directShapeForRawCon))
                    source4.Add(directShapeForRawCon);
                }
              }
              else
                source4.AddRange((IEnumerable<DirectShape>) this.CreateDirectShapeForRawCons(topLevelElement, moveDist, isRawCons, uniqueId, isDSGlobal));
            }
            else
            {
              if (source4.Count<DirectShape>() != 0)
              {
                foreach (DirectShape directShape in this.CreateDirectShapeForConnection(topLevelElement, uniqueId, moveDist, isDSGlobal))
                {
                  if (!source4.Contains(directShape))
                    source4.Add(directShape);
                }
              }
              else
                source4.AddRange((IEnumerable<DirectShape>) this.CreateDirectShapeForConnection(topLevelElement, uniqueId, moveDist, isDSGlobal));
              parameter2?.Set(num1);
            }
            List<DirectShape> list4 = source4.Distinct<DirectShape>().ToList<DirectShape>();
            parameter2?.Set(num1);
            if (list4.Count > 0)
            {
              List<ElementId> elementIdList = new List<ElementId>();
              foreach (DirectShape elem2 in list4)
              {
                string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) elem2, "HOST_GUID");
                if (parameterAsString != null)
                {
                  if (entourages.Keys.Contains<string>(parameterAsString))
                    entourages[parameterAsString].Add(elem2.UniqueId);
                  else
                    entourages.Add(parameterAsString, new List<string>()
                    {
                      elem2.UniqueId
                    });
                }
                elementIdList.Add(elem2.Id);
              }
            }
          }
        }
      }
    }
    if (source3.Count<FamilyInstance>() > 0)
    {
      List<string> values = new List<string>();
      foreach (FamilyInstance familyInstance in source3)
        values.Add(familyInstance.Id.ToString() + "\n");
      TaskDialog.Show("Auto Warping", "The following instances have the parameter AUTO_WARP_DISPLACEMENT set as read only. Make the parameter for those instances writable or delete the offending instances and try running Auto Warping again. Canceling. \n" + string.Join(string.Empty, (IEnumerable<string>) values));
      throw new Exception("READ ONLY");
    }
  }

  private bool SetWarpableFlag(FamilyInstance element)
  {
    Parameter parameter1 = element.LookupParameter("IS_WARPABLE_SHAPE");
    Parameter parameter2 = element.LookupParameter("WARPABLE_FILTER_FLAG");
    if (parameter2 == null)
      return false;
    if (parameter1 != null && parameter1.AsInteger() == 1 && WarpingEdgeLoop.PassesWarpableCriteria(element))
    {
      parameter2.Set(1);
      return true;
    }
    parameter2.Set(0);
    return false;
  }

  private bool EvaluateAndSetWarpableForChildren(bool topDS, FamilyInstance child)
  {
    if (child == null)
      return false;
    int num = topDS ? 1 : 0;
    Parameter parameter = child.LookupParameter("IS_WARPABLE_SHAPE");
    if (parameter != null && !parameter.IsReadOnly && WarpingEdgeLoop.PassesWarpableCriteria(child) && num == 1)
      parameter.Set(1);
    this.SetWarpableFlag(child);
    return Utils.ElementUtils.Parameters.GetParameterAsString((Element) child, "MANUFACTURE_COMPONENT").ToUpper().Contains("RAW CONSUMABLE") ? num == 1 : Utils.ElementUtils.Parameters.GetParameterAsBool((Element) child, "IS_WARPABLE_SHAPE");
  }

  private List<DirectShape> CreateDirectShapeForConnection(
    FamilyInstance topLevel,
    string guid,
    XYZ moveDist,
    bool isDSGlobal)
  {
    Document document = topLevel.Document;
    if (!isDSGlobal)
      return new List<DirectShape>();
    List<DirectShape> shapeForConnection = new List<DirectShape>();
    List<DirectShape> directShapeList = new List<DirectShape>();
    bool parameterAsBool = Utils.ElementUtils.Parameters.GetParameterAsBool((Element) topLevel, "IS_WARPABLE_SHAPE");
    this.SetWarpableFlag(topLevel);
    string constructionProduct = topLevel.GetConstructionProduct();
    if (parameterAsBool && WarpingEdgeLoop.PassesWarpableCriteria(topLevel))
    {
      DirectShape directShapeOfElement = this.CreateDirectShapeOfElement(topLevel, moveDist);
      directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(this.GetVizClassification(topLevel));
      (directShapeOfElement.LookupParameter("HOST_GUID") ?? throw new Exception("HOST_GUID")).Set(guid);
      string name = topLevel.Name;
      string manufactureComponent = topLevel.GetManufactureComponent();
      string str = topLevel.GetControlMark();
      topLevel.Id.ToString();
      if (str.Equals(""))
        str = name;
      directShapeOfElement.SetName(str + " WARPED");
      directShapeOfElement.LookupParameter("IDENTITY_DESCRIPTION")?.Set(str + " WARPED");
      directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(manufactureComponent);
      directShapeOfElement.LookupParameter("CONSTRUCTION_PRODUCT")?.Set(constructionProduct);
      shapeForConnection.Add(directShapeOfElement);
    }
    foreach (ElementId subComponentId in (IEnumerable<ElementId>) topLevel.GetSubComponentIds())
    {
      if (document.GetElement(subComponentId) is FamilyInstance element)
      {
        bool warpableForChildren1 = this.EvaluateAndSetWarpableForChildren(parameterAsBool, element);
        if (warpableForChildren1 && WarpingEdgeLoop.PassesWarpableCriteria(element))
        {
          element.GetManufactureComponent();
          DirectShape directShapeOfElement = this.CreateDirectShapeOfElement(element, moveDist);
          directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(this.GetVizClassification(element));
          (directShapeOfElement.LookupParameter("HOST_GUID") ?? throw new Exception("HOST_GUID")).Set(guid);
          string name = element.Name;
          string manufactureComponent = element.GetManufactureComponent();
          string str = element.GetControlMark();
          element.Id.ToString();
          if (str.Equals(""))
            str = name;
          directShapeOfElement.SetName(str + " WARPED");
          directShapeOfElement.LookupParameter("IDENTITY_DESCRIPTION")?.Set(str + " WARPED");
          directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(manufactureComponent);
          directShapeOfElement.LookupParameter("CONSTRUCTION_PRODUCT")?.Set(constructionProduct);
          shapeForConnection.Add(directShapeOfElement);
        }
        foreach (ElementId id1 in element.GetSubComponentIds().ToList<ElementId>())
        {
          FamilyInstance element1 = document.GetElement(id1) as FamilyInstance;
          bool warpableForChildren2 = this.EvaluateAndSetWarpableForChildren(warpableForChildren1, element1);
          if (element1 != null)
          {
            if (warpableForChildren2 && WarpingEdgeLoop.PassesWarpableCriteria(element1))
            {
              element1.GetManufactureComponent();
              DirectShape directShapeOfElement = this.CreateDirectShapeOfElement(element1, moveDist);
              directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(this.GetVizClassification(element1));
              (directShapeOfElement.LookupParameter("HOST_GUID") ?? throw new Exception("HOST_GUID")).Set(guid);
              string name = element1.Name;
              string manufactureComponent = element1.GetManufactureComponent();
              string str = element1.GetControlMark();
              element1.Id.ToString();
              if (str.Equals(""))
                str = name;
              directShapeOfElement.SetName(str + " WARPED");
              directShapeOfElement.LookupParameter("IDENTITY_DESCRIPTION")?.Set(str + " WARPED");
              directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(manufactureComponent);
              directShapeOfElement.LookupParameter("CONSTRUCTION_PRODUCT")?.Set(constructionProduct);
              shapeForConnection.Add(directShapeOfElement);
            }
            foreach (ElementId id2 in element1.GetSubComponentIds().ToList<ElementId>())
            {
              FamilyInstance element2 = document.GetElement(id2) as FamilyInstance;
              bool warpableForChildren3 = this.EvaluateAndSetWarpableForChildren(warpableForChildren2, element2);
              if (element2 != null && warpableForChildren3 && WarpingEdgeLoop.PassesWarpableCriteria(element2))
              {
                element2.GetManufactureComponent();
                DirectShape directShapeOfElement = this.CreateDirectShapeOfElement(element2, moveDist);
                directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(this.GetVizClassification(element2));
                (directShapeOfElement.LookupParameter("HOST_GUID") ?? throw new Exception("HOST_GUID")).Set(guid);
                string name = element2.Name;
                string manufactureComponent = element2.GetManufactureComponent();
                string str = element2.GetControlMark();
                element2.Id.ToString();
                if (str.Equals(""))
                  str = name;
                directShapeOfElement.SetName(str + " WARPED");
                directShapeOfElement.LookupParameter("IDENTITY_DESCRIPTION")?.Set(str + " WARPED");
                directShapeOfElement.LookupParameter("MANUFACTURE_COMPONENT")?.Set(manufactureComponent);
                directShapeOfElement.LookupParameter("CONSTRUCTION_PRODUCT")?.Set(constructionProduct);
                shapeForConnection.Add(directShapeOfElement);
              }
            }
          }
        }
      }
    }
    return shapeForConnection;
  }

  private bool ValidRawConsForDirectShape(FamilyInstance raw)
  {
    List<string> source = new List<string>();
    source.Add("LIFTING");
    source.Add("LIFT");
    source.Add("REBAR");
    source.Add("MESH");
    source.Add("WWF");
    source.Add("SHEARGRID");
    source.Add("SHEAR GRID");
    source.Add("CGRID");
    source.Add("STRAND");
    string mfgComponent = Utils.ElementUtils.Parameters.GetParameterAsString((Element) raw, "MANUFACTURE_COMPONENT");
    return source.Where<string>((Func<string, bool>) (term => mfgComponent.ToUpper().Contains(term))).Count<string>() <= 0;
  }

  private List<DirectShape> CreateDirectShapeForRawCons(
    FamilyInstance topLevel,
    XYZ moveDist,
    bool isRawCons,
    string guid,
    bool isDSGlobal)
  {
    if (!isDSGlobal)
      return new List<DirectShape>();
    List<DirectShape> directShapeForRawCons = new List<DirectShape>();
    List<ElementId> elementIdList = new List<ElementId>();
    bool parameterAsBool = Utils.ElementUtils.Parameters.GetParameterAsBool((Element) topLevel, "IS_WARPABLE_SHAPE");
    topLevel.LookupParameter("HOST_GUID")?.AsString();
    DirectShape directShapeOfElement1 = this.CreateDirectShapeOfElement(topLevel, moveDist);
    directShapeOfElement1.LookupParameter("MANUFACTURE_COMPONENT")?.Set(this.GetVizClassification(topLevel));
    (directShapeOfElement1.LookupParameter("HOST_GUID") ?? throw new Exception("HOST_GUID")).Set(guid);
    string name1 = topLevel.Name;
    string manufactureComponent1 = topLevel.GetManufactureComponent();
    string str1 = topLevel.GetControlMark();
    topLevel.Id.ToString();
    string constructionProduct = topLevel.GetConstructionProduct();
    if (str1.Equals(""))
      str1 = name1;
    directShapeOfElement1.SetName(str1 + " WARPED");
    directShapeOfElement1.LookupParameter("IDENTITY_DESCRIPTION")?.Set(str1 + " WARPED");
    directShapeOfElement1.LookupParameter("MANUFACTURE_COMPONENT")?.Set(manufactureComponent1);
    directShapeOfElement1.LookupParameter("CONSTRUCTION_PRODUCT")?.Set(constructionProduct);
    manufactureComponent1.Contains("RAW CONSUMABLE");
    foreach (ElementId subComponentId1 in (IEnumerable<ElementId>) topLevel.GetSubComponentIds())
    {
      if (topLevel.Document.GetElement(subComponentId1) is FamilyInstance element1)
      {
        bool warpableForChildren1 = this.EvaluateAndSetWarpableForChildren(parameterAsBool, element1);
        string name2 = element1.Name;
        string manufactureComponent2 = element1.GetManufactureComponent();
        string str2 = element1.GetControlMark();
        element1.Id.ToString();
        DirectShape directShapeOfElement2 = this.CreateDirectShapeOfElement(element1, moveDist);
        directShapeOfElement2.LookupParameter("MANUFACTURE_COMPONENT")?.Set(this.GetVizClassification(element1));
        (directShapeOfElement2.LookupParameter("HOST_GUID") ?? throw new Exception("HOST_GUID")).Set(guid);
        if (str2.Equals(""))
          str2 = name2;
        directShapeOfElement2.SetName(str2 + " WARPED");
        directShapeOfElement2.LookupParameter("IDENTITY_DESCRIPTION")?.Set(str2 + " WARPED");
        directShapeOfElement2.LookupParameter("MANUFACTURE_COMPONENT")?.Set(manufactureComponent2);
        directShapeOfElement2.LookupParameter("CONSTRUCTION_PRODUCT")?.Set(constructionProduct);
        if (this.ValidRawConsForDirectShape(element1))
        {
          if (manufactureComponent2.Contains("RAW CONSUMABLE") & parameterAsBool)
          {
            directShapeOfElement1.AppendShape((IList<GeometryObject>) Solids.GetInstanceSolids((Element) element1).Select<Solid, Solid>((Func<Solid, Solid>) (s => SolidUtils.CreateTransformed(s, Transform.CreateTranslation(moveDist)))).Cast<GeometryObject>().ToList<GeometryObject>());
            elementIdList.Add(directShapeOfElement2.Id);
          }
          else
          {
            foreach (ElementId subComponentId2 in (IEnumerable<ElementId>) element1.GetSubComponentIds())
            {
              if (topLevel.Document.GetElement(subComponentId2) is FamilyInstance element)
              {
                bool warpableForChildren2 = this.EvaluateAndSetWarpableForChildren(warpableForChildren1, element);
                string name3 = element.Name;
                string manufactureComponent3 = element.GetManufactureComponent();
                string str3 = element.GetControlMark();
                element.Id.ToString();
                DirectShape directShapeOfElement3 = this.CreateDirectShapeOfElement(element, moveDist);
                directShapeOfElement3.LookupParameter("MANUFACTURE_COMPONENT")?.Set(this.GetVizClassification(element));
                (directShapeOfElement3.LookupParameter("HOST_GUID") ?? throw new Exception("HOST_GUID")).Set(guid);
                if (str3.Equals(""))
                  str3 = name3;
                directShapeOfElement3.SetName(str3 + " WARPED");
                directShapeOfElement3.LookupParameter("IDENTITY_DESCRIPTION")?.Set(str3 + " WARPED");
                directShapeOfElement3.LookupParameter("MANUFACTURE_COMPONENT")?.Set(manufactureComponent3);
                directShapeOfElement3.LookupParameter("CONSTRUCTION_PRODUCT")?.Set(constructionProduct);
                if (this.ValidRawConsForDirectShape(element1))
                {
                  if (manufactureComponent3.Contains("RAW CONSUMABLE"))
                  {
                    directShapeOfElement2.AppendShape((IList<GeometryObject>) Solids.GetInstanceSolids((Element) element).Select<Solid, Solid>((Func<Solid, Solid>) (s => SolidUtils.CreateTransformed(s, Transform.CreateTranslation(moveDist)))).Cast<GeometryObject>().ToList<GeometryObject>());
                    elementIdList.Add(directShapeOfElement3.Id);
                  }
                  else if (warpableForChildren2 && WarpingEdgeLoop.PassesWarpableCriteria(element))
                    directShapeForRawCons.Add(directShapeOfElement3);
                  else
                    elementIdList.Add(directShapeOfElement3.Id);
                }
                else
                  elementIdList.Add(directShapeOfElement3.Id);
              }
            }
            if (warpableForChildren1 && WarpingEdgeLoop.PassesWarpableCriteria(element1))
              directShapeForRawCons.Add(directShapeOfElement2);
            else
              elementIdList.Add(directShapeOfElement2.Id);
          }
        }
        else
          elementIdList.Add(directShapeOfElement2.Id);
      }
    }
    this.SetWarpableFlag(topLevel);
    if (parameterAsBool && WarpingEdgeLoop.PassesWarpableCriteria(topLevel))
      directShapeForRawCons.Add(directShapeOfElement1);
    else
      elementIdList.Add(directShapeOfElement1.Id);
    foreach (ElementId elementId in elementIdList)
      topLevel.Document.Delete(elementId);
    return directShapeForRawCons;
  }

  public static void ClearOldDirectShape(Document revitDoc, string levelName)
  {
    FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Entourage);
    FilteredElementCollector source1 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof (FamilyInstance));
    FilteredElementCollector source2 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_GenericModel).OfClass(typeof (FamilyInstance));
    List<FamilyInstance> list = source1.Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => Utils.ElementUtils.Parameters.GetParameterAsString((Element) famInst, "Work Plane").Equals(levelName))).ToList<FamilyInstance>();
    source2.Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => Utils.ElementUtils.Parameters.GetParameterAsString((Element) famInst, "Work Plane").Equals(levelName))).ToList<FamilyInstance>();
    List<FamilyInstance> source3 = new List<FamilyInstance>();
    foreach (FamilyInstance familyInstance in list)
      source3.Add(familyInstance);
    foreach (FamilyInstance familyInstance in source2)
      source3.Add(familyInstance);
    source3.Distinct<FamilyInstance>();
    List<ElementId> source4 = new List<ElementId>();
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    revitDoc.Application.ActiveAddInId.GetGUID().ToString();
    foreach (FamilyInstance familyInstance in list)
      stringList2.Add(familyInstance.UniqueId);
    foreach (Element element in elementCollector)
    {
      DirectShape directShape = element as DirectShape;
      string str1 = "";
      if (directShape != null)
      {
        Parameter parameter = element.LookupParameter("HOST_GUID");
        if (parameter != null)
          str1 = parameter.AsString();
        foreach (string str2 in stringList2)
        {
          if (str1 != null && str1.ToUpper().Contains(str2.ToUpper()))
            source4.Add(element.Id);
        }
      }
    }
    foreach (ElementId elementId in source4.Distinct<ElementId>().ToArray<ElementId>())
      revitDoc.Delete(elementId);
  }

  private void ClearOldDS(Dictionary<string, List<string>> entourages, Element embed)
  {
    Document document = embed.Document;
    List<string> stringList = new List<string>();
    if (!entourages.TryGetValue(embed.UniqueId, out stringList))
      return;
    foreach (string uniqueId in stringList)
    {
      Element element = document.GetElement(uniqueId);
      if (element != null)
        document.Delete(element.Id);
    }
  }

  private bool CheckRawCons(FamilyInstance topLevel)
  {
    bool flag = false;
    foreach (ElementId subComponentId in (IEnumerable<ElementId>) topLevel.GetSubComponentIds())
    {
      string str = "";
      if (topLevel.Document.GetElement(subComponentId) is FamilyInstance element)
        str = element.GetManufactureComponent();
      if (str.Contains("RAW CONSUMABLE"))
        flag = true;
      else if (element.GetSubComponentIds() != null)
        flag = this.CheckRawCons(element);
      if (flag)
        break;
    }
    return flag;
  }

  private string GetVizClassification(FamilyInstance famInst) => famInst.GetManufactureComponent();

  private DirectShape CreateDirectShapeOfElement(FamilyInstance famInst, XYZ moveDist)
  {
    Transform translation = Transform.CreateTranslation(moveDist);
    DirectShape element = DirectShape.CreateElement(famInst.Document, new ElementId(BuiltInCategory.OST_Entourage));
    element.ApplicationId = famInst.Document.Application.ActiveAddInId.GetGUID().ToString();
    foreach (Solid Geom in Solids.GetInstanceSolids((Element) famInst, translation).Where<Solid>((Func<Solid, bool>) (s => s.Volume > 0.0)).ToList<Solid>())
    {
      if (!element.IsValidGeometry(Geom))
      {
        TessellatedShapeBuilder tessellatedShapeBuilder = new TessellatedShapeBuilder();
        tessellatedShapeBuilder.OpenConnectedFaceSet(true);
        foreach (Face face1 in Geom.Faces)
        {
          Mesh mesh = face1.Triangulate();
          for (int idx = 0; idx < mesh.NumTriangles; ++idx)
          {
            MeshTriangle meshTriangle = mesh.get_Triangle(idx);
            TessellatedFace face2 = new TessellatedFace((IList<XYZ>) new List<XYZ>()
            {
              meshTriangle.get_Vertex(0),
              meshTriangle.get_Vertex(1),
              meshTriangle.get_Vertex(2)
            }, ElementId.InvalidElementId);
            if (tessellatedShapeBuilder.DoesFaceHaveEnoughLoopsAndVertices(face2))
              tessellatedShapeBuilder.AddFace(face2);
          }
        }
        tessellatedShapeBuilder.CloseConnectedFaceSet();
        tessellatedShapeBuilder.Target = TessellatedShapeBuilderTarget.AnyGeometry;
        tessellatedShapeBuilder.Fallback = TessellatedShapeBuilderFallback.Mesh;
        tessellatedShapeBuilder.Build();
        element.AppendShape(tessellatedShapeBuilder.GetBuildResult().GetGeometricalObjects());
      }
      else
        element.AppendShape((IList<GeometryObject>) new List<GeometryObject>()
        {
          (GeometryObject) Geom
        });
    }
    return element;
  }

  private bool IsEdgeParalleltoLine(XYZ dir1, XYZ dir2, double tolerance = 0.0001)
  {
    return Math.Abs(dir1.CrossProduct(dir2).GetLength()) < tolerance;
  }

  private Solid GetMasterSolid(List<Solid> sList)
  {
    Solid solid0 = (Solid) null;
    foreach (Solid s in sList)
    {
      if ((GeometryObject) s != (GeometryObject) null && s.Faces.Size > 0)
        solid0 = !((GeometryObject) solid0 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid0, s, BooleanOperationsType.Union) : s;
    }
    return solid0;
  }
}
