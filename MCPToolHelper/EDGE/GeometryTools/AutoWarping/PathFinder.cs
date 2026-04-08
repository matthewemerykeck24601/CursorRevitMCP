// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.PathFinder
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

public class PathFinder
{
  private const double ALLOWABLE_RIGHT_TURN_ANGLE_IN_DEGREES = 1.0;
  private const double SEARCH_TOLERANCE_FOR_SPOTELEVATIONS = 2.0;
  public List<SpotElevationPoint> UnusedSpotElevations;
  private List<SpotElevationPoint> _spotElevations;
  private List<WarpingEdge> _Edges;
  private Dictionary<FamilyInstance, XYZ[]> _sfDict;
  private Document _revitDoc;
  private Transform _transform;
  private bool isCurved;

  public List<WarpingEdgeLoop> EdgeLoops { get; set; }

  public PathFinder(
    Dictionary<FamilyInstance, XYZ[]> sfDict,
    List<SpotElevationPoint> spotElevations,
    Document revitDoc,
    Transform transform,
    out List<WarpingEdgeLoop> nonLinearLoops,
    out List<List<WarpingEdge>> collisions)
  {
    this._Edges = new List<WarpingEdge>();
    this.EdgeLoops = new List<WarpingEdgeLoop>();
    this.UnusedSpotElevations = new List<SpotElevationPoint>();
    this._revitDoc = revitDoc;
    this._spotElevations = spotElevations;
    this._transform = transform;
    this._sfDict = sfDict;
    this.CalculatePaths(out nonLinearLoops, out collisions);
    this.FindUnusedSpotElevs();
  }

  private void CalculatePaths(
    out List<WarpingEdgeLoop> nonLinearLoops,
    out List<List<WarpingEdge>> collisions)
  {
    this.FindConnectedPoints();
    this.FindSmallestLoops(out nonLinearLoops, out collisions);
  }

  private void FindUnusedSpotElevs()
  {
    List<SpotElevationPoint> second = new List<SpotElevationPoint>();
    foreach (WarpingEdgeLoop edgeLoop in this.EdgeLoops)
    {
      foreach (WarpingEdge edge in edgeLoop.Edges)
      {
        second.Add(edge.StartPoint);
        second.Add(edge.EndPoint);
      }
    }
    this.UnusedSpotElevations.AddRange(this._spotElevations.Except<SpotElevationPoint>((IEnumerable<SpotElevationPoint>) second));
  }

  private void FindSmallestLoops(
    out List<WarpingEdgeLoop> nonLinearLoops,
    out List<List<WarpingEdge>> intersections)
  {
    intersections = new List<List<WarpingEdge>>();
    Dictionary<FamilyInstance, Line> dictionary1 = new Dictionary<FamilyInstance, Line>();
    foreach (FamilyInstance key in this._sfDict.Keys)
    {
      Line flatLine = WarpingUtils.CreateFlatLine(Line.CreateBound(this._sfDict[key][0], this._sfDict[key][1]));
      dictionary1.Add(key, flatLine);
    }
    this.EdgeLoops.Clear();
    nonLinearLoops = new List<WarpingEdgeLoop>();
    List<WarpingEdgeLoop> source1 = new List<WarpingEdgeLoop>();
    List<WarpingEdgeLoop> warpingEdgeLoopList = new List<WarpingEdgeLoop>();
    Dictionary<SpotElevationPoint, List<WarpingEdge>> unusedEdges = new Dictionary<SpotElevationPoint, List<WarpingEdge>>();
    bool flag1;
    do
    {
      flag1 = false;
      Dictionary<WarpingEdge, List<WarpingEdge>> dictionary2 = new Dictionary<WarpingEdge, List<WarpingEdge>>();
      List<WarpingEdge> warpingEdgeList = new List<WarpingEdge>();
      foreach (WarpingEdge edge in this._Edges)
      {
        if (!warpingEdgeList.Contains(edge))
        {
          List<WarpingEdge> collider = new List<WarpingEdge>();
          if (!this.ValidEdge(edge, this._Edges, out collider))
          {
            warpingEdgeList.AddRange((IEnumerable<WarpingEdge>) collider);
            dictionary2.Add(edge, collider);
            flag1 = true;
          }
        }
      }
      if (flag1)
      {
        bool flag2 = false;
        List<List<WarpingEdge>> collection = new List<List<WarpingEdge>>();
        foreach (WarpingEdge key1 in dictionary2.Keys)
        {
          int num1 = int.MinValue;
          WarpingEdge warpingEdge1 = (WarpingEdge) null;
          Dictionary<WarpingEdge, int> dictionary3 = new Dictionary<WarpingEdge, int>();
          foreach (WarpingEdge key2 in dictionary2[key1])
          {
            int num2 = 0;
            Line flatCurve = key2.GetFlatCurve() as Line;
            foreach (FamilyInstance key3 in dictionary1.Keys)
            {
              Line line = dictionary1[key3];
              IntersectionResultArray resultArray;
              if (flatCurve.Intersect((Curve) line, out resultArray) == SetComparisonResult.Overlap)
              {
                double normalizedParameter = line.ComputeNormalizedParameter(resultArray.get_Item(0).UVPoint.V);
                if (normalizedParameter > 0.01 && normalizedParameter < 0.99)
                  ++num2;
              }
            }
            dictionary3.Add(key2, num2);
            if (num2 > num1)
            {
              num1 = num2;
              warpingEdge1 = key2;
            }
          }
          bool flag3 = true;
          foreach (WarpingEdge key4 in dictionary3.Keys)
          {
            if (dictionary3[key4] != num1)
            {
              flag3 = false;
              break;
            }
          }
          if (!flag3)
          {
            flag2 = true;
            foreach (WarpingEdge warpingEdge2 in dictionary2[key1])
            {
              if (warpingEdge2.Equals((object) warpingEdge1))
              {
                warpingEdge2.StartPoint.ConnectedEdges.Remove(warpingEdge2);
                warpingEdge2.EndPoint.ConnectedEdges.Remove(warpingEdge2);
                this._Edges.Remove(warpingEdge2);
              }
            }
          }
          else
            collection.Add(dictionary2[key1].ToList<WarpingEdge>());
        }
        if (!flag2 && collection.Count > 0)
        {
          intersections.AddRange((IEnumerable<List<WarpingEdge>>) collection);
          return;
        }
      }
    }
    while (flag1);
    foreach (SpotElevationPoint spotElevation in this._spotElevations)
      unusedEdges.Add(spotElevation, new List<WarpingEdge>((IEnumerable<WarpingEdge>) spotElevation.ConnectedEdges));
    foreach (SpotElevationPoint spotElevation in this._spotElevations)
    {
      if (spotElevation.ConnectedEdges.Count >= 2)
      {
        List<WarpingEdgeLoop> loopsBySpotElevation = this.GetShortestLoopsBySpotElevation(spotElevation);
        source1.AddRange((IEnumerable<WarpingEdgeLoop>) loopsBySpotElevation);
        foreach (WarpingEdgeLoop warpingEdgeLoop in loopsBySpotElevation)
        {
          foreach (WarpingEdge edge in warpingEdgeLoop.Edges)
          {
            if (unusedEdges[spotElevation].Contains(edge))
              unusedEdges[spotElevation].Remove(edge);
            SpotElevationPoint otherPoint = this.GetOtherPoint(spotElevation, edge);
            if (unusedEdges[otherPoint].Contains(edge))
              unusedEdges[otherPoint].Remove(edge);
          }
        }
      }
    }
    foreach (SpotElevationPoint key in unusedEdges.Keys)
    {
      if (key.ConnectedEdges.Count >= 2 && unusedEdges[key].Count > 0)
      {
        foreach (WarpingEdge startingEdge in unusedEdges[key].ToList<WarpingEdge>())
        {
          List<WarpingEdgeLoop> loopsBySpotElevation = this.GetShortestLoopsBySpotElevation(key, startingEdge);
          if (loopsBySpotElevation.Count > 0)
          {
            foreach (WarpingEdgeLoop warpingEdgeLoop in loopsBySpotElevation)
            {
              if (warpingEdgeLoop.Edges.Contains(startingEdge) && unusedEdges[key].Contains(startingEdge))
                unusedEdges[key].Remove(startingEdge);
            }
          }
          source1.AddRange((IEnumerable<WarpingEdgeLoop>) loopsBySpotElevation);
        }
      }
    }
    List<SpotElevationPoint> list1 = unusedEdges.Keys.Where<SpotElevationPoint>((Func<SpotElevationPoint, bool>) (p => unusedEdges[p].Count > 0)).ToList<SpotElevationPoint>();
    if (list1.Count > 0)
    {
      foreach (SpotElevationPoint key in list1)
      {
        foreach (WarpingEdge warpingEdge in unusedEdges[key])
          this._Edges.Remove(warpingEdge);
      }
    }
    this.EdgeLoops.AddRange(source1.Distinct<WarpingEdgeLoop>());
    List<WarpingEdgeLoop> list2 = this.EdgeLoops.OrderBy<WarpingEdgeLoop, int>((Func<WarpingEdgeLoop, int>) (l => l.Edges.Count)).ToList<WarpingEdgeLoop>();
    List<WarpingEdge> warpingEdgeList1 = new List<WarpingEdge>();
    List<WarpingEdgeLoop> collection1 = new List<WarpingEdgeLoop>();
    foreach (WarpingEdgeLoop warpingEdgeLoop in list2)
    {
      int num = 0;
      foreach (WarpingEdge edge in warpingEdgeLoop.Edges)
      {
        if (!warpingEdgeList1.Contains(edge))
        {
          warpingEdgeList1.Add(edge);
          ++num;
        }
      }
      if (num != 0)
        collection1.Add(warpingEdgeLoop);
      if (warpingEdgeList1.Count == this._Edges.Count)
        break;
    }
    this.EdgeLoops.Clear();
    this.EdgeLoops.AddRange((IEnumerable<WarpingEdgeLoop>) collection1);
    foreach (WarpingEdgeLoop edgeLoop in this.EdgeLoops)
    {
      foreach (WarpingEdge edge in edgeLoop.Edges)
      {
        if (!edge.ContainingLoops.Contains(edgeLoop))
          edge.ContainingLoops.Add(edgeLoop);
      }
    }
    List<WarpingEdge> warpingEdgeList2 = new List<WarpingEdge>();
    List<WarpingEdge> list3 = this._Edges.Where<WarpingEdge>((Func<WarpingEdge, bool>) (e => e.ContainingLoops.Count == 1)).ToList<WarpingEdge>();
    Dictionary<SpotElevationPoint, List<WarpingEdge>> dictionary4 = new Dictionary<SpotElevationPoint, List<WarpingEdge>>();
    foreach (SpotElevationPoint spotElevation in this._spotElevations)
    {
      List<WarpingEdge> list4 = spotElevation.ConnectedEdges.Intersect<WarpingEdge>((IEnumerable<WarpingEdge>) list3).ToList<WarpingEdge>();
      if (list4.Count > 0)
        dictionary4.Add(spotElevation, list4);
    }
    List<SpotElevationPoint> spotElevationPointList = new List<SpotElevationPoint>();
    List<WarpingEdgeLoop> source2 = new List<WarpingEdgeLoop>();
    foreach (SpotElevationPoint key in dictionary4.Keys)
    {
      Queue<Tuple<List<WarpingEdge>, SpotElevationPoint>> tupleQueue = new Queue<Tuple<List<WarpingEdge>, SpotElevationPoint>>();
      SpotElevationPoint spotElevationPoint1 = key;
      List<WarpingEdge> warpingEdgeList3 = new List<WarpingEdge>();
      tupleQueue.Enqueue(new Tuple<List<WarpingEdge>, SpotElevationPoint>(warpingEdgeList3, spotElevationPoint1));
      bool flag4 = false;
      while (tupleQueue.Count > 0)
      {
        Tuple<List<WarpingEdge>, SpotElevationPoint> tuple = tupleQueue.Dequeue();
        SpotElevationPoint spotElevationPoint2 = tuple.Item2;
        foreach (WarpingEdge edge in dictionary4[spotElevationPoint2])
        {
          if (!tuple.Item1.Contains(edge))
          {
            List<WarpingEdge> list5 = tuple.Item1.ToList<WarpingEdge>();
            list5.Add(edge);
            if (this.GetOtherPoint(tuple.Item2, edge).Equals((object) key))
            {
              flag4 = true;
              source2.Add(new WarpingEdgeLoop(list5, this._transform));
            }
            if (!flag4)
              tupleQueue.Enqueue(new Tuple<List<WarpingEdge>, SpotElevationPoint>(list5, this.GetOtherPoint(spotElevationPoint2, edge)));
          }
        }
      }
    }
    List<CurveLoop> curveLoopList = new List<CurveLoop>();
    foreach (WarpingEdgeLoop warpingEdgeLoop1 in source2.Distinct<WarpingEdgeLoop>())
    {
      foreach (WarpingEdgeLoop warpingEdgeLoop2 in source2.Distinct<WarpingEdgeLoop>())
      {
        if (!warpingEdgeLoop1.Equals((object) warpingEdgeLoop2) && this.IsWithinOtherLoop(warpingEdgeLoop2.GetLoop(), warpingEdgeLoop1.GetLoop()))
        {
          if (!this.EdgeLoops.Contains(warpingEdgeLoop1))
          {
            this.EdgeLoops.Add(warpingEdgeLoop1);
            using (List<WarpingEdge>.Enumerator enumerator = warpingEdgeLoop1.Edges.GetEnumerator())
            {
              while (enumerator.MoveNext())
                enumerator.Current.ContainingLoops.Add(warpingEdgeLoop1);
              break;
            }
          }
          break;
        }
      }
    }
    List<WarpingEdge> source3 = new List<WarpingEdge>();
    foreach (WarpingEdgeLoop edgeLoop in this.EdgeLoops)
    {
      edgeLoop.PrimeSFInstances(this._sfDict);
      if (!edgeLoop.CheckLinearity())
      {
        nonLinearLoops.Add(edgeLoop);
        foreach (WarpingEdge edge in edgeLoop.Edges)
        {
          edge.ContainingLoops.Remove(edgeLoop);
          source3.Add(edge);
        }
      }
      else
      {
        foreach (WarpingEdge edge in edgeLoop.Edges)
          source3.Add(edge);
      }
    }
    source3.Distinct<WarpingEdge>().ToList<WarpingEdge>();
    foreach (WarpingEdgeLoop warpingEdgeLoop in nonLinearLoops)
      this.EdgeLoops.Remove(warpingEdgeLoop);
  }

  private void FindConnectedPoints()
  {
    foreach (SpotElevationPoint spotElevation in this._spotElevations)
    {
      foreach (XYZ key1 in spotElevation.SearchDirections.Keys)
      {
        Curve searchDirection = spotElevation.SearchDirections[key1];
        XYZ endPoint1 = spotElevation.SearchDirections[key1].GetEndPoint(0);
        XYZ endPoint2 = spotElevation.SearchDirections[key1].GetEndPoint(1);
        double rawParameter = (spotElevation.planePoint - endPoint1).DotProduct((endPoint2 - endPoint1).Normalize());
        if (searchDirection.ComputeNormalizedParameter(rawParameter) <= 0.901)
          ;
        SpotElevationPoint pointAlongDirection = this.FindNearestSpotElevationPointAlongDirection(spotElevation, key1);
        if (pointAlongDirection != null)
        {
          bool flag = false;
          foreach (XYZ key2 in pointAlongDirection.SearchDirections.Keys)
          {
            if (key2.CrossProduct(key1).IsAlmostEqualTo(XYZ.Zero))
              flag = true;
          }
          if (flag)
          {
            WarpingEdge warpingEdgeReference = this.GetWarpingEdgeReference(spotElevation, pointAlongDirection, spotElevation.SearchDirections[key1]);
            if (warpingEdgeReference != null && !spotElevation.ConnectedEdges.Contains(warpingEdgeReference))
            {
              spotElevation.ConnectedEdges.Add(warpingEdgeReference);
              pointAlongDirection.ConnectedEdges.Add(warpingEdgeReference);
              using (SubTransaction subTransaction = new SubTransaction(this._revitDoc))
              {
                int num1 = (int) subTransaction.Start();
                int num2 = (int) subTransaction.Commit();
              }
            }
          }
        }
      }
    }
  }

  private bool ValidEdge(
    WarpingEdge edge,
    List<WarpingEdge> edgesForComparison,
    out List<WarpingEdge> collider)
  {
    bool flag = true;
    collider = new List<WarpingEdge>();
    collider.Add(edge);
    foreach (WarpingEdge warpingEdge in edgesForComparison)
    {
      Curve flatCurve1 = warpingEdge.GetFlatCurve();
      Curve flatCurve2 = edge.GetFlatCurve();
      if (!((GeometryObject) flatCurve2 == (GeometryObject) flatCurve1))
      {
        XYZ endPoint1 = flatCurve1.GetEndPoint(0);
        XYZ endPoint2 = flatCurve1.GetEndPoint(1);
        XYZ endPoint3 = flatCurve2.GetEndPoint(0);
        XYZ endPoint4 = flatCurve2.GetEndPoint(1);
        IntersectionResultArray resultArray = new IntersectionResultArray();
        if (flatCurve2.Intersect(flatCurve1, out resultArray) == SetComparisonResult.Overlap)
        {
          for (int index = 0; index < resultArray.Size; ++index)
          {
            XYZ xyzPoint = resultArray.get_Item(index).XYZPoint;
            if (!xyzPoint.IsAlmostEqualTo(endPoint1) && !xyzPoint.IsAlmostEqualTo(endPoint2) && !xyzPoint.IsAlmostEqualTo(endPoint3) && !xyzPoint.IsAlmostEqualTo(endPoint4))
            {
              flag = false;
              collider.Add(warpingEdge);
            }
          }
        }
      }
    }
    return flag;
  }

  private WarpingEdge GetWarpingEdgeReference(
    SpotElevationPoint point1,
    SpotElevationPoint point2,
    Curve cv,
    bool bLookup = false)
  {
    WarpingEdge warpingEdge1 = this._Edges.Where<WarpingEdge>((Func<WarpingEdge, bool>) (edge => edge.EdgeContainsPoint(point1) && edge.GetEndPoint(point1).planePoint.IsAlmostEqualTo(point2.planePoint))).FirstOrDefault<WarpingEdge>();
    if (warpingEdge1 != null || bLookup)
      return warpingEdge1 ?? (WarpingEdge) null;
    WarpingEdge warpingEdgeReference = new WarpingEdge(point1, point2, cv);
    WarpingEdge warpingEdge2 = new WarpingEdge();
    this._Edges.Add(warpingEdgeReference);
    return warpingEdgeReference;
  }

  private SpotElevationPoint FindNearestSpotElevationPointAlongDirection(
    SpotElevationPoint spotElevation,
    XYZ direction)
  {
    List<SpotElevationPoint> source = new List<SpotElevationPoint>();
    double num = direction.AngleTo(XYZ.BasisX);
    Transform rotationAtPoint = Transform.CreateRotationAtPoint(XYZ.BasisZ, (XYZ.BasisX.CrossProduct(direction).Normalize().IsAlmostEqualTo(XYZ.BasisZ) ? -1.0 : 1.0) * num, spotElevation.planePoint);
    foreach (SpotElevationPoint spotElevation1 in this._spotElevations)
    {
      if (!spotElevation1.planePoint.IsAlmostEqualTo(spotElevation.planePoint))
      {
        XYZ xyz = rotationAtPoint.OfPoint(spotElevation1.planePoint);
        if (Math.Abs(xyz.Y - spotElevation.planePoint.Y) < 2.0 && xyz.X > spotElevation.planePoint.X)
          source.Add(spotElevation1);
      }
    }
    return source.OrderBy<SpotElevationPoint, double>((Func<SpotElevationPoint, double>) (spotElevPoint => spotElevPoint.planePoint.DistanceTo(spotElevation.planePoint))).FirstOrDefault<SpotElevationPoint>();
  }

  private List<WarpingEdgeLoop> GetShortestLoopsBySpotElevation(
    SpotElevationPoint startPoint,
    WarpingEdge startingEdge = null)
  {
    List<WarpingEdgeLoop> loopsBySpotElevation = new List<WarpingEdgeLoop>();
    Queue<Tuple<List<WarpingEdge>, SpotElevationPoint>> tupleQueue = new Queue<Tuple<List<WarpingEdge>, SpotElevationPoint>>();
    List<WarpingEdge> warpingEdgeList = new List<WarpingEdge>();
    SpotElevationPoint spotElevationPoint = startPoint;
    if (startingEdge != null)
    {
      spotElevationPoint = this.GetOtherPoint(startPoint, startingEdge);
      warpingEdgeList.Add(startingEdge);
    }
    bool flag = false;
    tupleQueue.Enqueue(new Tuple<List<WarpingEdge>, SpotElevationPoint>(warpingEdgeList, spotElevationPoint));
    while (tupleQueue.Count > 0)
    {
      Tuple<List<WarpingEdge>, SpotElevationPoint> tuple = tupleQueue.Dequeue();
      SpotElevationPoint point = tuple.Item2;
      foreach (WarpingEdge connectedEdge in point.ConnectedEdges)
      {
        if (!tuple.Item1.Contains(connectedEdge))
        {
          List<WarpingEdge> list = tuple.Item1.ToList<WarpingEdge>();
          list.Add(connectedEdge);
          if (this.GetOtherPoint(tuple.Item2, connectedEdge).Equals((object) startPoint))
          {
            flag = true;
            loopsBySpotElevation.Add(new WarpingEdgeLoop(list, this._transform));
          }
          if (!flag)
            tupleQueue.Enqueue(new Tuple<List<WarpingEdge>, SpotElevationPoint>(list, this.GetOtherPoint(point, connectedEdge)));
        }
      }
      if (tupleQueue.Count > 2000)
        return new List<WarpingEdgeLoop>();
    }
    return loopsBySpotElevation;
  }

  private SpotElevationPoint GetOtherPoint(SpotElevationPoint point, WarpingEdge edge)
  {
    return !point.Equals((object) edge.StartPoint) ? edge.StartPoint : edge.EndPoint;
  }

  public void CheckOverlapLoops()
  {
    List<WarpingEdge> warpingEdgeList = new List<WarpingEdge>();
    List<CurveLoop> curveLoopList = new List<CurveLoop>();
    foreach (WarpingEdgeLoop edgeLoop in this.EdgeLoops)
    {
      CurveLoop loop = this.GetLoop(edgeLoop);
      curveLoopList.Add(loop);
    }
    foreach (WarpingEdgeLoop edgeLoop1 in this.EdgeLoops)
    {
      CurveLoop loop = this.GetLoop(edgeLoop1);
      foreach (WarpingEdgeLoop edgeLoop2 in this.EdgeLoops)
      {
        if (!edgeLoop1.Equals((object) edgeLoop2) && loop != null && this.IsWithinOtherLoop(loop, this.GetLoop(edgeLoop2)))
          throw new Exception("LOOP");
      }
    }
  }

  private CurveLoop GetLoop(WarpingEdgeLoop loop)
  {
    CurveLoop loop1 = new CurveLoop();
    WarpingEdge warpingEdge = loop.Edges.First<WarpingEdge>();
    if (warpingEdge == null)
      return (CurveLoop) null;
    SpotElevationPoint startingPoint0 = warpingEdge.StartPoint;
    SpotElevationPoint startingPoint1 = warpingEdge.StartPoint;
    if (loop.Edges.Count > 1 && (warpingEdge.StartPoint.planePoint.IsAlmostEqualTo(loop.Edges[1].StartPoint.planePoint) || warpingEdge.StartPoint.planePoint.IsAlmostEqualTo(loop.Edges[1].EndPoint.planePoint)))
    {
      startingPoint0 = warpingEdge.EndPoint;
      startingPoint1 = warpingEdge.EndPoint;
    }
    foreach (WarpingEdge edge in loop.Edges)
    {
      Curve flatCurve = (Curve) (edge.GetFlatCurve(0.0, startingPoint0, startingPoint1) as Line);
      try
      {
        loop1.Append(flatCurve);
      }
      catch (Autodesk.Revit.Exceptions.ArgumentException ex)
      {
        TaskDialog.Show("Auto-Warping", "One or more warping loops in the project could not be constructed. Please ensure that all Spot Elevations form convex polygons around warpable elements.");
        return (CurveLoop) null;
      }
      startingPoint0 = edge.StartPoint;
      startingPoint1 = edge.EndPoint;
    }
    return loop1;
  }

  private bool IsWithinOtherLoop(CurveLoop curveLoop, CurveLoop curveLoop2)
  {
    curveLoop = this.ConsolidateCurveLoop(curveLoop);
    if (curveLoop2.Equals((object) curveLoop))
      return false;
    foreach (Curve curve1 in curveLoop2)
    {
      bool flag = false;
      int num1 = 0;
      int num2 = 0;
      if (curve1 is Line)
      {
        Line unbound1 = Line.CreateUnbound(curve1.GetEndPoint(0), (curve1 as Line).Direction);
        Line unbound2 = Line.CreateUnbound(curve1.GetEndPoint(0), -(curve1 as Line).Direction);
        foreach (Curve curve2 in curveLoop)
        {
          IntersectionResultArray resultArray1 = new IntersectionResultArray();
          IntersectionResultArray resultArray2 = new IntersectionResultArray();
          IntersectionResultArray resultArray3 = new IntersectionResultArray();
          Curve curve3 = curve2;
          SetComparisonResult comparisonResult1 = unbound1.Intersect(curve3, out resultArray1);
          SetComparisonResult comparisonResult2 = unbound2.Intersect(curve3, out resultArray2);
          switch (curve1.Intersect(curve2, out resultArray3))
          {
            case SetComparisonResult.Overlap:
              if (resultArray3.Size > 1)
              {
                flag = true;
                goto label_25;
              }
              break;
            case SetComparisonResult.Subset:
            case SetComparisonResult.Superset:
            case SetComparisonResult.Equal:
              flag = true;
              goto label_25;
          }
          if (comparisonResult1 == SetComparisonResult.Overlap && resultArray1.get_Item(0).UVPoint.U >= 0.0)
          {
            if (Math.Abs(resultArray1.get_Item(0).UVPoint.U) > 0.001)
              ++num1;
          }
          else if (comparisonResult1 == SetComparisonResult.Superset && (curve3.GetEndPoint(0) - curve1.GetEndPoint(0)).Normalize().IsAlmostEqualTo(unbound1.Direction))
            ++num1;
          if (comparisonResult2 == SetComparisonResult.Overlap && resultArray2.get_Item(0).UVPoint.U >= 0.0)
          {
            if (Math.Abs(resultArray2.get_Item(0).UVPoint.U) > 0.001)
              ++num2;
          }
          else if (comparisonResult2 == SetComparisonResult.Superset && (curve3.GetEndPoint(0) - curve1.GetEndPoint(0)).Normalize().IsAlmostEqualTo(unbound2.Direction))
            ++num2;
        }
label_25:
        if (num1 % 2 != 0 && num2 % 2 != 0 && !flag)
          return true;
      }
      else if (curve1 is Arc)
      {
        foreach (XYZ origin in (IEnumerable<XYZ>) curve1.Tessellate())
        {
          Line unbound = Line.CreateUnbound(origin, curveLoop.GetPlane().YVec);
          int num3 = 0;
          foreach (Curve curve4 in curveLoop)
          {
            IntersectionResultArray resultArray4 = new IntersectionResultArray();
            IntersectionResultArray resultArray5 = new IntersectionResultArray();
            Curve curve5 = curve4;
            SetComparisonResult comparisonResult = unbound.Intersect(curve5, out resultArray4);
            switch (curve1.Intersect(curve4, out resultArray5))
            {
              case SetComparisonResult.Overlap:
                if (resultArray5.Size > 1)
                {
                  flag = true;
                  goto label_42;
                }
                break;
              case SetComparisonResult.Subset:
              case SetComparisonResult.Superset:
                flag = true;
                goto label_42;
            }
            if (comparisonResult == SetComparisonResult.Overlap && resultArray4.get_Item(0).UVPoint.U >= 0.0 && Math.Abs(resultArray4.get_Item(0).UVPoint.U) > 1E-07)
              ++num3;
          }
label_42:
          if (num3 % 2 != 0 && !flag)
            return true;
        }
      }
    }
    return false;
  }

  private CurveLoop ConsolidateCurveLoop(CurveLoop curveLoop)
  {
    CurveLoop curveLoop1 = new CurveLoop();
    Curve curve1 = (Curve) null;
    Curve curve2 = (Curve) null;
    foreach (Curve curve3 in curveLoop)
    {
      if ((GeometryObject) curve2 == (GeometryObject) null)
      {
        curve1 = curve3;
        curve2 = curve3;
      }
      else if (!((GeometryObject) curve3 == (GeometryObject) curve1))
      {
        if (curve3 is Line)
        {
          if ((curve3 as Line).Direction.IsAlmostEqualTo((curve2 as Line).Direction))
          {
            curve2 = (Curve) Line.CreateBound(curve2.GetEndPoint(0), curve3.GetEndPoint(1));
          }
          else
          {
            curveLoop1.Append(curve2);
            curve2 = curve3;
          }
        }
      }
      else
        break;
    }
    if ((GeometryObject) curve2 != (GeometryObject) null)
      curveLoop1.Append(curve2);
    return curveLoop1 ?? curveLoop;
  }
}
