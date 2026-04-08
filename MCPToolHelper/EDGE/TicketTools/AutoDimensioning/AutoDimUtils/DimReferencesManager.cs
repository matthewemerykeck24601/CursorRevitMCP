// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoDimUtils.DimReferencesManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AdminUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning.AutoDimUtils;

public class DimReferencesManager
{
  private DimensionEdge _dimAlongSide;
  private Document _revitDoc;
  public Dictionary<ReferencedPoint, Reference> _existingDimensionDictionary;
  private XYZ _dimAlongDirection;
  private XYZ _dimFromDirection;
  public Dictionary<ReferencedPoint, Reference> referencePairs;
  public List<ReferencedPoint> _refPoints;

  public ReferenceArray DimReferences { get; set; }

  public int Count { get; private set; }

  public DimReferencesManager(
    Document revitDoc,
    DimensionEdge dimensionEdge,
    XYZ dimAlongDirection,
    XYZ dimTendency)
  {
    this.DimReferences = new ReferenceArray();
    this.referencePairs = new Dictionary<ReferencedPoint, Reference>();
    this.Count = 0;
    this._revitDoc = revitDoc;
    this._dimAlongSide = dimensionEdge;
    this._existingDimensionDictionary = new Dictionary<ReferencedPoint, Reference>();
    this._dimAlongDirection = dimAlongDirection;
    this._dimFromDirection = dimTendency;
  }

  public static Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>> InstrumentSFContours(
    Document revitDoc,
    Autodesk.Revit.DB.ElementId sfElemId,
    View view)
  {
    try
    {
      Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>> dictionary = new Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>>();
      Autodesk.Revit.DB.Element element = revitDoc.GetElement(sfElemId);
      Transform tr = (Transform) null;
      if (element == null)
        return (Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>>) null;
      if (element is Autodesk.Revit.DB.FamilyInstance)
        tr = (element as Autodesk.Revit.DB.FamilyInstance).GetTransform();
      bool bSymbol;
      List<Solid> symbolSolids = Solids.GetSymbolSolids(element, out bSymbol, options: new Autodesk.Revit.DB.Options()
      {
        ComputeReferences = true
      });
      Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences = DimensioningGeometryUtils.GetDimensioningPointsReferences(element as Autodesk.Revit.DB.FamilyInstance, (View) (view as ViewSection));
      double refDist;
      Solid diffSolid = DimReferencesManager.GetDiffSolid(bSymbol ? symbolSolids.Select<Solid, Solid>((Func<Solid, Solid>) (s => SolidUtils.CreateTransformed(s, tr))).ToList<Solid>() : symbolSolids.ToList<Solid>(), view, out refDist);
      if ((GeometryObject) diffSolid == (GeometryObject) null || diffSolid.Volume == 0.0)
        return (Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>>) null;
      Solid prismaticSolid = DimReferencesManager.GetPrismaticSolid(diffSolid, view, refDist);
      return !((GeometryObject) prismaticSolid == (GeometryObject) null) ? DimReferencesManager.RetrieveSolidVertices(symbolSolids, diffSolid, prismaticSolid, pointsReferences, view, refDist, bSymbol ? tr : (Transform) null) : (Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>>) null;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("ShortCurve"))
      {
        TaskDialog.Show("Auto-Ticket Generation", "Element for auto ticketing contained invalid geometry. Please check for edges that fall below Revit's 1/32\" minimum");
        return (Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>>) null;
      }
    }
    return (Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>>) null;
  }

  public static BoundingBoxXYZ GetViewBBox(Autodesk.Revit.DB.IndependentTag tag, View view)
  {
    return DimReferencesManager.GetViewBBox(tag, view, out List<XYZ> _);
  }

  public static BoundingBoxXYZ GetViewBBox(
    Autodesk.Revit.DB.IndependentTag tag,
    View view,
    out List<XYZ> outlinePoints)
  {
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    BoundingBoxXYZ boundingBoxXyz = tag.get_BoundingBox(view);
    if (boundingBoxXyz == null)
    {
      outlinePoints = new List<XYZ>();
      return (BoundingBoxXYZ) null;
    }
    double num1 = boundingBoxXyz.Max.X - boundingBoxXyz.Min.X;
    double num2 = boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y;
    double height = boundingBoxXyz.Max.Z - boundingBoxXyz.Min.Z;
    List<XYZ> source = new List<XYZ>()
    {
      boundingBoxXyz.Min,
      boundingBoxXyz.Min + XYZ.BasisX * num1,
      boundingBoxXyz.Min + XYZ.BasisY * num2,
      boundingBoxXyz.Min + XYZ.BasisX * num1 + XYZ.BasisY * num2
    };
    source.AddRange((IEnumerable<XYZ>) source.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => pt + XYZ.BasisZ * height)).ToList<XYZ>());
    List<XYZ> list = source.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => viewTransform.Inverse.OfPoint(pt))).ToList<XYZ>();
    XYZ xyz1 = new XYZ(list.Min<XYZ>((Func<XYZ, double>) (pt => pt.X)), list.Min<XYZ>((Func<XYZ, double>) (pt => pt.Y)), list.Min<XYZ>((Func<XYZ, double>) (pt => pt.Z)));
    XYZ xyz2 = new XYZ(list.Max<XYZ>((Func<XYZ, double>) (pt => pt.X)), list.Max<XYZ>((Func<XYZ, double>) (pt => pt.Y)), list.Min<XYZ>((Func<XYZ, double>) (pt => pt.Z)));
    BoundingBoxXYZ viewBbox = new BoundingBoxXYZ();
    viewBbox.Min = xyz1;
    viewBbox.Max = xyz2;
    double num3 = viewBbox.Max.X - viewBbox.Min.X;
    double num4 = viewBbox.Max.Y - viewBbox.Min.Y;
    height = viewBbox.Max.Z - viewBbox.Min.Z;
    outlinePoints = new List<XYZ>()
    {
      viewBbox.Min,
      viewBbox.Min + XYZ.BasisX * num3,
      viewBbox.Min + XYZ.BasisY * num4,
      viewBbox.Min + XYZ.BasisX * num3 + XYZ.BasisY * num4
    };
    outlinePoints.AddRange((IEnumerable<XYZ>) outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => pt + XYZ.BasisZ * height)).ToList<XYZ>());
    outlinePoints = outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (pt => viewTransform.OfPoint(pt))).ToList<XYZ>();
    viewBbox.Min = new XYZ(outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => pt.X)), outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => pt.Y)), outlinePoints.Min<XYZ>((Func<XYZ, double>) (pt => pt.Z)));
    viewBbox.Max = new XYZ(outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => pt.X)), outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => pt.Y)), outlinePoints.Max<XYZ>((Func<XYZ, double>) (pt => pt.Z)));
    return viewBbox;
  }

  public static BoundingBoxXYZ GetViewBBox(
    Solid solid,
    View view,
    out List<XYZ> outlinePoints,
    bool bSymbol = false)
  {
    outlinePoints = new List<XYZ>();
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    BoundingBoxXYZ bbox = SolidUtils.CreateTransformed(solid, viewTransform.Inverse).GetBoundingBox();
    double num1 = bbox.Max.X - bbox.Min.X;
    double num2 = bbox.Max.Y - bbox.Min.Y;
    double height = bbox.Max.Z - bbox.Min.Z;
    outlinePoints.Add(bbox.Min);
    outlinePoints.Add(bbox.Min + XYZ.BasisX * num1);
    outlinePoints.Add(bbox.Min + XYZ.BasisY * num2);
    outlinePoints.Add(bbox.Min + XYZ.BasisX * num1 + XYZ.BasisY * num2);
    List<XYZ> list = outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (oP => oP + XYZ.BasisZ * height)).ToList<XYZ>();
    outlinePoints.AddRange((IEnumerable<XYZ>) list);
    bbox.Min = bbox.Transform.OfPoint(bbox.Min);
    bbox.Max = bbox.Transform.OfPoint(bbox.Max);
    outlinePoints = outlinePoints.Select<XYZ, XYZ>((Func<XYZ, XYZ>) (p => bbox.Transform.OfPoint(p))).ToList<XYZ>();
    return bbox;
  }

  public static BoundingBoxXYZ GetViewBBox(
    Autodesk.Revit.DB.FamilyInstance instance,
    View view,
    out List<XYZ> outlinePoints)
  {
    outlinePoints = new List<XYZ>();
    BoundingBoxXYZ bb1 = new BoundingBoxXYZ()
    {
      Min = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue),
      Max = new XYZ(double.MinValue, double.MinValue, double.MinValue)
    };
    bool bSymbol;
    foreach (Solid symbolSolid in Solids.GetSymbolSolids((Autodesk.Revit.DB.Element) instance, out bSymbol))
    {
      Solid solid = symbolSolid;
      if (bSymbol)
        solid = SolidUtils.CreateTransformed(symbolSolid, instance.GetTransform());
      List<XYZ> outlinePoints1;
      BoundingBoxXYZ viewBbox = DimReferencesManager.GetViewBBox(solid, view, out outlinePoints1, bSymbol);
      bb1 = Utils.MiscUtils.MiscUtils.AdjustBBox(bb1, viewBbox);
      outlinePoints.AddRange((IEnumerable<XYZ>) outlinePoints1);
    }
    return bb1;
  }

  private static Solid GetDiffSolid(List<Solid> solids, View view, out double refDist)
  {
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    Solid solid1 = (Solid) null;
    foreach (Solid solid2 in solids)
    {
      if (solid2.Volume != 0.0)
      {
        Solid transformed = SolidUtils.CreateTransformed(solid2, viewTransform.Inverse);
        solid1 = (GeometryObject) solid1 == (GeometryObject) null ? transformed : BooleanOperationsUtils.ExecuteBooleanOperation(solid1, transformed, BooleanOperationsType.Union);
      }
    }
    BoundingBoxXYZ boundingBox1 = solid1.GetBoundingBox();
    double num1 = boundingBox1.Max.X - boundingBox1.Min.X;
    double num2 = boundingBox1.Max.Y - boundingBox1.Min.Y;
    double extrusionDist = boundingBox1.Max.Z - boundingBox1.Min.Z;
    XYZ xyz1 = boundingBox1.Min + num1 * XYZ.BasisX;
    XYZ xyz2 = xyz1 + num2 * XYZ.BasisY;
    XYZ xyz3 = boundingBox1.Min + num2 * XYZ.BasisY;
    Solid transformed1 = SolidUtils.CreateTransformed(GeometryCreationUtilities.CreateExtrusionGeometry((IList<Autodesk.Revit.DB.CurveLoop>) new List<Autodesk.Revit.DB.CurveLoop>()
    {
      Autodesk.Revit.DB.CurveLoop.Create((IList<Curve>) new List<Curve>()
      {
        (Curve) Line.CreateBound(boundingBox1.Min, xyz1),
        (Curve) Line.CreateBound(xyz1, xyz2),
        (Curve) Line.CreateBound(xyz2, xyz3),
        (Curve) Line.CreateBound(xyz3, boundingBox1.Min)
      })
    }, XYZ.BasisZ, extrusionDist), boundingBox1.Transform);
    BoundingBoxXYZ boundingBox2 = transformed1.GetBoundingBox();
    double num3 = boundingBox2.Max.Z - boundingBox2.Min.Z;
    refDist = num3;
    return (GeometryObject) transformed1 != (GeometryObject) null && transformed1.Volume != 0.0 && (GeometryObject) solid1 != (GeometryObject) null && solid1.Volume != 0.0 ? BooleanOperationsUtils.ExecuteBooleanOperation(transformed1, solid1, BooleanOperationsType.Difference) : (Solid) null;
  }

  private static Solid GetPrismaticSolid(Solid diffSolid, View view, double refDist)
  {
    Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    double num1 = double.MinValue;
    double num2 = double.MaxValue;
    Autodesk.Revit.DB.Face face1 = (Autodesk.Revit.DB.Face) null;
    Autodesk.Revit.DB.Face face2 = (Autodesk.Revit.DB.Face) null;
    foreach (Autodesk.Revit.DB.Face face3 in diffSolid.Faces)
    {
      if (face3 is PlanarFace)
      {
        PlanarFace planarFace = face3 as PlanarFace;
        double z = planarFace.Origin.Z;
        if (planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ) && z > num1)
        {
          face1 = (Autodesk.Revit.DB.Face) planarFace;
          num1 = z;
        }
        if (planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ.Negate()) && z < num2)
        {
          face2 = (Autodesk.Revit.DB.Face) planarFace;
          num2 = z;
        }
      }
    }
    if ((GeometryObject) face1 == (GeometryObject) null || (GeometryObject) face2 == (GeometryObject) null)
      return (Solid) null;
    Solid solid0 = (Solid) null;
    foreach (Autodesk.Revit.DB.CurveLoop edgesAsCurveLoop in (IEnumerable<Autodesk.Revit.DB.CurveLoop>) face1.GetEdgesAsCurveLoops())
    {
      Solid extrusionGeometry = GeometryCreationUtilities.CreateExtrusionGeometry((IList<Autodesk.Revit.DB.CurveLoop>) new List<Autodesk.Revit.DB.CurveLoop>()
      {
        edgesAsCurveLoop
      }, XYZ.BasisZ.Negate(), refDist);
      if ((GeometryObject) solid0 == (GeometryObject) null)
        solid0 = extrusionGeometry;
      else
        BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid0, extrusionGeometry, BooleanOperationsType.Union);
    }
    Solid solid = (Solid) null;
    foreach (Autodesk.Revit.DB.CurveLoop edgesAsCurveLoop in (IEnumerable<Autodesk.Revit.DB.CurveLoop>) face2.GetEdgesAsCurveLoops())
    {
      Solid extrusionGeometry = GeometryCreationUtilities.CreateExtrusionGeometry((IList<Autodesk.Revit.DB.CurveLoop>) new List<Autodesk.Revit.DB.CurveLoop>()
      {
        edgesAsCurveLoop
      }, XYZ.BasisZ, refDist);
      if ((GeometryObject) solid == (GeometryObject) null)
        solid = extrusionGeometry;
      else
        BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid, extrusionGeometry, BooleanOperationsType.Union);
    }
    return BooleanOperationsUtils.ExecuteBooleanOperation(solid0, solid, BooleanOperationsType.Intersect);
  }

  private static double GetRefDist(Solid solid, View view, Transform tr)
  {
    BoundingBoxXYZ boundingBox = solid.GetBoundingBox();
    double num1 = boundingBox.Max.X - boundingBox.Min.X;
    double num2 = boundingBox.Max.Y - boundingBox.Min.Y;
    double num3 = boundingBox.Max.Z - boundingBox.Min.Z;
    double refDist = -1.0;
    if (Util.IsParallel(view.ViewDirection, tr.OfVector(boundingBox.Transform.BasisX)))
      refDist = num1;
    else if (Util.IsParallel(view.ViewDirection, tr.OfVector(boundingBox.Transform.BasisY)))
      refDist = num2;
    else if (Util.IsParallel(view.ViewDirection, tr.OfVector(boundingBox.Transform.BasisZ)))
      refDist = num3;
    return refDist;
  }

  private static Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>> RetrieveSolidVertices(
    List<Solid> solids,
    Solid negSolid,
    Solid diffSolid,
    Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dimensionEdges,
    View view,
    double refDist,
    Transform tr = null)
  {
    List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
    List<ReferencedPoint> source1 = new List<ReferencedPoint>();
    List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    List<Autodesk.Revit.DB.Face> faceList = new List<Autodesk.Revit.DB.Face>();
    foreach (Autodesk.Revit.DB.Face face in diffSolid.Faces)
    {
      if (face is PlanarFace && (face as PlanarFace).FaceNormal.IsAlmostEqualTo(XYZ.BasisZ))
        faceList.Add(face);
    }
    Dictionary<int, List<ReferencedPoint>> pointsByBlockout1 = new Dictionary<int, List<ReferencedPoint>>();
    int key1 = 0;
    int key2 = -1;
    foreach (Autodesk.Revit.DB.Face face in faceList)
    {
      foreach (EdgeArray edgeLoop in face.EdgeLoops)
      {
        bool flag = false;
        ++key1;
        pointsByBlockout1.Add(key1, new List<ReferencedPoint>());
        if (!pointsByBlockout1.ContainsKey(key2))
          pointsByBlockout1.Add(key2, new List<ReferencedPoint>());
        XYZ q = (XYZ) null;
        XYZ endpoint1_1 = (XYZ) null;
        XYZ endpoint1_2 = (XYZ) null;
        List<Line> source2 = new List<Line>();
        foreach (Autodesk.Revit.DB.Edge edge in edgeLoop)
        {
          XYZ xyz1 = edge.AsCurve().GetEndPoint(0);
          XYZ xyz2 = edge.AsCurve().GetEndPoint(1);
          Line bound = Line.CreateBound(new XYZ(xyz1.X, xyz1.Y, 0.0), new XYZ(xyz2.X, xyz2.Y, 0.0));
          if (q != null && Util.IsParallel(bound.Direction, q))
          {
            if ((!endpoint1_1.IsAlmostEqualTo(xyz1) || !endpoint1_2.IsAlmostEqualTo(xyz2)) && (!endpoint1_2.IsAlmostEqualTo(xyz1) || !endpoint1_1.IsAlmostEqualTo(xyz2)))
            {
              source2.Remove(source2.Last<Line>());
              if (endpoint1_1.IsAlmostEqualTo(xyz1))
              {
                bound = Line.CreateBound(endpoint1_2, xyz2);
                xyz1 = endpoint1_2;
              }
              else if (endpoint1_1.IsAlmostEqualTo(xyz2))
              {
                bound = Line.CreateBound(endpoint1_2, xyz1);
                xyz2 = xyz1;
                xyz1 = endpoint1_2;
              }
              else if (endpoint1_2.IsAlmostEqualTo(xyz1))
              {
                bound = Line.CreateBound(endpoint1_1, xyz2);
                xyz1 = endpoint1_1;
              }
              else if (endpoint1_2.IsAlmostEqualTo(xyz2))
              {
                bound = Line.CreateBound(endpoint1_1, xyz1);
                xyz2 = xyz1;
                xyz1 = endpoint1_1;
              }
            }
            else
              continue;
          }
          q = bound.Direction;
          endpoint1_1 = xyz1;
          endpoint1_2 = xyz2;
          source2.Add(bound);
        }
        foreach (Line line in source2)
        {
          XYZ ep1 = line.GetEndPoint(0);
          XYZ ep2 = line.GetEndPoint(1);
          Line.CreateBound(new XYZ(ep1.X, ep1.Y, 0.0), new XYZ(ep2.X, ep2.Y, 0.0));
          ReferencedPoint referencedPoint1 = new ReferencedPoint((Reference) null, ep1, ep1);
          ReferencedPoint referencedPoint2 = new ReferencedPoint((Reference) null, ep2, ep2);
          XYZ xyz3 = new XYZ(dimensionEdges[DimensionEdge.Top][DimensionEdge.Left].LocalPoint.X, dimensionEdges[DimensionEdge.Left][DimensionEdge.Bottom].LocalPoint.Y, 0.0);
          XYZ xyz4 = new XYZ(dimensionEdges[DimensionEdge.Top][DimensionEdge.Right].LocalPoint.X, dimensionEdges[DimensionEdge.Left][DimensionEdge.Top].LocalPoint.Y, 0.0);
          if (referencedPointList2.Find((Predicate<ReferencedPoint>) (p => p.LocalPoint.X.ApproximatelyEquals(ep1.X) && p.LocalPoint.Y.ApproximatelyEquals(ep1.Y))) == null)
          {
            referencedPointList2.Add(referencedPoint1);
            if (((ep1.X.ApproximatelyEquals(xyz3.X) || ep1.X.ApproximatelyEquals(xyz4.X) || ep1.Y.ApproximatelyEquals(xyz3.Y) ? 1 : (ep1.Y.ApproximatelyEquals(xyz4.Y) ? 1 : 0)) | (flag ? 1 : 0)) != 0)
            {
              if (!flag)
              {
                flag = true;
                if (pointsByBlockout1[key1].Any<ReferencedPoint>())
                  pointsByBlockout1[key2].AddRange((IEnumerable<ReferencedPoint>) pointsByBlockout1[key1]);
                pointsByBlockout1.Remove(key1);
                --key1;
              }
              pointsByBlockout1[key2].Add(referencedPoint1);
            }
            else
              pointsByBlockout1[key1].Add(referencedPoint1);
            if (referencedPointList2.Find((Predicate<ReferencedPoint>) (p => p.LocalPoint.X.ApproximatelyEquals(ep2.X) && p.LocalPoint.Y.ApproximatelyEquals(ep2.Y))) == null)
            {
              referencedPointList2.Add(referencedPoint2);
              if (((ep2.X.ApproximatelyEquals(xyz3.X) || ep2.X.ApproximatelyEquals(xyz4.X) || ep2.Y.ApproximatelyEquals(xyz3.Y) ? 1 : (ep2.Y.ApproximatelyEquals(xyz4.Y) ? 1 : 0)) | (flag ? 1 : 0)) != 0)
              {
                if (!flag)
                {
                  flag = true;
                  if (pointsByBlockout1[key1].Any<ReferencedPoint>())
                    pointsByBlockout1[key2].AddRange((IEnumerable<ReferencedPoint>) pointsByBlockout1[key1]);
                  pointsByBlockout1.Remove(key1);
                  --key1;
                }
                pointsByBlockout1[key2].Add(referencedPoint2);
              }
              else
                pointsByBlockout1[key1].Add(referencedPoint2);
            }
          }
        }
        if (flag)
          --key2;
      }
    }
    foreach (Solid solid in solids)
    {
      foreach (Autodesk.Revit.DB.Edge edge in solid.Edges)
      {
        Reference endPointReference1 = edge.GetEndPointReference(0);
        Reference endPointReference2 = edge.GetEndPointReference(1);
        XYZ point1 = edge.AsCurve().GetEndPoint(0);
        XYZ point2 = edge.AsCurve().GetEndPoint(1);
        if (tr != null)
        {
          point1 = tr.OfPoint(point1);
          point2 = tr.OfPoint(point2);
        }
        XYZ ep1Local = viewTransform.Inverse.OfPoint(point1);
        XYZ ep2Local = viewTransform.Inverse.OfPoint(point2);
        ReferencedPoint referencedPoint3 = source1.Find((Predicate<ReferencedPoint>) (p => p.LocalPoint.X.ApproximatelyEquals(ep1Local.X) && p.LocalPoint.Y.ApproximatelyEquals(ep1Local.Y)));
        if (referencedPoint3 == null || referencedPoint3.LocalPoint.Z <= ep1Local.Z)
        {
          if (referencedPoint3 != null)
            source1.Remove(referencedPoint3);
          source1.Add(new ReferencedPoint(endPointReference1, point1, ep1Local));
          ReferencedPoint referencedPoint4 = source1.Find((Predicate<ReferencedPoint>) (p => p.LocalPoint.X.ApproximatelyEquals(ep2Local.X) && p.LocalPoint.Y.ApproximatelyEquals(ep2Local.Y)));
          if (referencedPoint4 == null || referencedPoint4.LocalPoint.Z <= ep2Local.Z)
          {
            if (referencedPoint4 != null)
              source1.Remove(referencedPoint4);
            source1.Add(new ReferencedPoint(endPointReference2, point2, ep2Local));
          }
        }
      }
    }
    Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>> dictionary = new Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>>();
    foreach (int key3 in pointsByBlockout1.Keys)
    {
      List<ReferencedPoint> collection = new List<ReferencedPoint>();
      foreach (ReferencedPoint referencedPoint5 in pointsByBlockout1[key3])
      {
        ReferencedPoint rp = referencedPoint5;
        ReferencedPoint referencedPoint6 = source1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (OGRP => OGRP.LocalPoint.X.ApproximatelyEquals(rp.LocalPoint.X) && OGRP.LocalPoint.Y.ApproximatelyEquals(rp.LocalPoint.Y))).OrderByDescending<ReferencedPoint, double>((Func<ReferencedPoint, double>) (OGRP => OGRP.LocalPoint.Z)).FirstOrDefault<ReferencedPoint>();
        if (referencedPoint6 != null && !collection.Contains(referencedPoint6))
          collection.Add(referencedPoint6);
      }
      pointsByBlockout1[key3].Clear();
      pointsByBlockout1[key3].AddRange((IEnumerable<ReferencedPoint>) collection);
      pointsByBlockout1[key3].RemoveAll((Predicate<ReferencedPoint>) (rp =>
      {
        Autodesk.Revit.DB.SolidCurveIntersection curveIntersection = negSolid.IntersectWithCurve((Curve) Line.CreateBound(rp.LocalPoint - XYZ.BasisZ * 100.0, rp.LocalPoint + XYZ.BasisZ * 100.0), new Autodesk.Revit.DB.SolidCurveIntersectionOptions());
        double me = 0.0;
        foreach (Curve curve in curveIntersection)
          me += curve.Length;
        return !me.ApproximatelyEquals(refDist);
      }));
    }
    Dictionary<int, List<ReferencedPoint>> pointsByBlockout2 = new Dictionary<int, List<ReferencedPoint>>();
    foreach (int key4 in pointsByBlockout1.Keys.ToList<int>())
    {
      if (key4 < 0)
      {
        pointsByBlockout2.Add(key4, pointsByBlockout1[key4]);
        pointsByBlockout1.Remove(key4);
      }
    }
    Dictionary<int, List<ReferencedPoint>> BlockoutDictionary = DimReferencesManager.getConcreteDictionary(pointsByBlockout1);
    DimReferencesManager.getConcreteDictionary(pointsByBlockout2, true).ToList<KeyValuePair<int, List<ReferencedPoint>>>().ForEach((Action<KeyValuePair<int, List<ReferencedPoint>>>) (kvp => BlockoutDictionary.Add(kvp.Key, kvp.Value)));
    foreach (int key5 in BlockoutDictionary.Keys)
    {
      dictionary.Add(key5, new Dictionary<DimensionEdge, List<ReferencedPoint>>());
      dictionary[key5].Add(DimensionEdge.Top, DimReferencesManager.RemoveOverlappedSF(BlockoutDictionary[key5], view, DimensionEdge.Top));
      dictionary[key5].Add(DimensionEdge.Bottom, DimReferencesManager.RemoveOverlappedSF(BlockoutDictionary[key5], view, DimensionEdge.Bottom));
      dictionary[key5].Add(DimensionEdge.Left, DimReferencesManager.RemoveOverlappedSF(BlockoutDictionary[key5], view, DimensionEdge.Left));
      dictionary[key5].Add(DimensionEdge.Right, DimReferencesManager.RemoveOverlappedSF(BlockoutDictionary[key5], view, DimensionEdge.Right));
    }
    return dictionary;
  }

  private static Dictionary<int, List<ReferencedPoint>> getConcreteDictionary(
    Dictionary<int, List<ReferencedPoint>> pointsByBlockout,
    bool bContour = false)
  {
    List<List<ReferencedPoint>> referencedPointListList = new List<List<ReferencedPoint>>();
    Dictionary<int, List<int>> BlockoutIndices = DimReferencesManager.ConsolidateBlockouts(pointsByBlockout);
    List<int> source1 = new List<int>();
    foreach (int key1 in BlockoutIndices.Keys)
    {
      int key = key1;
      if (!source1.Contains(key))
      {
        List<ReferencedPoint> source2 = new List<ReferencedPoint>();
        List<int> intList = new List<int>();
        List<int> list = BlockoutIndices.Keys.Where<int>((Func<int, bool>) (k => BlockoutIndices[k].Contains(key))).ToList<int>();
        source2.AddRange((IEnumerable<ReferencedPoint>) pointsByBlockout[key]);
        foreach (int key2 in list)
        {
          source1.Add(key2);
          source2.AddRange((IEnumerable<ReferencedPoint>) pointsByBlockout[key2]);
          foreach (int key3 in BlockoutIndices[key2])
          {
            source1.Add(key3);
            source2.AddRange((IEnumerable<ReferencedPoint>) pointsByBlockout[key3]);
          }
        }
        referencedPointListList.Add(source2.Distinct<ReferencedPoint>().ToList<ReferencedPoint>());
        source1 = source1.Distinct<int>().ToList<int>();
      }
    }
    int key4 = 0;
    Dictionary<int, List<ReferencedPoint>> concreteDictionary = new Dictionary<int, List<ReferencedPoint>>();
    foreach (List<ReferencedPoint> referencedPointList in referencedPointListList)
    {
      key4 += bContour ? -1 : 1;
      concreteDictionary.Add(key4, referencedPointList);
    }
    return concreteDictionary;
  }

  private static Dictionary<int, List<int>> ConsolidateBlockouts(
    Dictionary<int, List<ReferencedPoint>> blockouts)
  {
    Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
    foreach (int key1 in blockouts.Keys)
    {
      dictionary.Add(key1, new List<int>());
      List<double> LocalXs = blockouts[key1].Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => rP.LocalPoint.X)).Distinct<double>().ToList<double>();
      List<double> LocalYs = blockouts[key1].Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => rP.LocalPoint.Y)).Distinct<double>().ToList<double>();
      foreach (int key2 in blockouts.Keys)
      {
        List<double> otherLocalXs = blockouts[key2].Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => rP.LocalPoint.X)).Distinct<double>().ToList<double>();
        List<double> otherLocalYs = blockouts[key2].Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => rP.LocalPoint.Y)).Distinct<double>().ToList<double>();
        if (!LocalXs.Any<double>((Func<double, bool>) (X => !otherLocalXs.Any<double>((Func<double, bool>) (otherX => otherX.ApproximatelyEquals(X))))) && !otherLocalXs.Any<double>((Func<double, bool>) (X => !LocalXs.Any<double>((Func<double, bool>) (otherX => otherX.ApproximatelyEquals(X))))) || !LocalYs.Any<double>((Func<double, bool>) (Y => !otherLocalYs.Any<double>((Func<double, bool>) (otherY => otherY.ApproximatelyEquals(Y))))) && !otherLocalYs.Any<double>((Func<double, bool>) (Y => !LocalYs.Any<double>((Func<double, bool>) (otherY => otherY.ApproximatelyEquals(Y))))))
          dictionary[key1].Add(key2);
      }
    }
    return dictionary;
  }

  private static List<ReferencedPoint> RemoveOverlappedSF(
    List<ReferencedPoint> originalPoints,
    View view,
    DimensionEdge side)
  {
    List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
    Dictionary<ReferencedPoint, Reference> existingPoints = new Dictionary<ReferencedPoint, Reference>();
    referencedPointList1.AddRange((IEnumerable<ReferencedPoint>) originalPoints);
    foreach (ReferencedPoint originalPoint in originalPoints)
    {
      int num1 = side == DimensionEdge.Top ? 1 : (side == DimensionEdge.Bottom ? 1 : 0);
      XYZ alongDirection = num1 != 0 ? XYZ.BasisX : XYZ.BasisY;
      XYZ direction = num1 == 0 ? XYZ.BasisX : XYZ.BasisY;
      List<ReferencedPoint> referencedPointList2 = DimReferencesManager.PointIsOverlappedSF(originalPoint, existingPoints, alongDirection);
      if (referencedPointList2.Count > 0)
      {
        double num2 = ProjectionUtils.ProjectPointOnDirection(originalPoint.LocalPoint, direction);
        foreach (ReferencedPoint key in referencedPointList2)
        {
          double num3 = ProjectionUtils.ProjectPointOnDirection(key.LocalPoint, direction);
          if (side == DimensionEdge.Top || side == DimensionEdge.Right)
          {
            if (num3 < num2)
            {
              referencedPointList1.Remove(key);
              existingPoints.Remove(key);
            }
            else
              referencedPointList1.Remove(originalPoint);
          }
          else if (num3 > num2)
          {
            existingPoints.Remove(key);
            referencedPointList1.Remove(key);
          }
          else
            referencedPointList1.Remove(originalPoint);
        }
      }
    }
    return referencedPointList1;
  }

  public DimReferencesManager.addResult AddReference(ReferencedPoint rP, bool hardwareDetail = false)
  {
    List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
    List<ReferencedPoint> source = !hardwareDetail ? this.PointIsOverlapped(rP) : this.PointIsOverlappedHWD(rP);
    if (source.Count == 0)
    {
      this.DimReferences.Append(rP.Reference);
      this.referencePairs.Add(rP, rP.Reference);
      return DimReferencesManager.addResult.Add;
    }
    if (source.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (p => ProjectionUtils.ProjectPointOnDirection(p.Point, this._dimFromDirection) >= ProjectionUtils.ProjectPointOnDirection(rP.Point, this._dimFromDirection))).Count<ReferencedPoint>() != 0)
      return DimReferencesManager.addResult.DoNotAdd;
    List<Reference> list = this.DimReferences.Cast<Reference>().ToList<Reference>();
    Dictionary<ReferencedPoint, Reference> dictionary = new Dictionary<ReferencedPoint, Reference>();
    foreach (ReferencedPoint key in this.referencePairs.Keys)
      dictionary.Add(key, this.referencePairs[key]);
    this.DimReferences.Clear();
    this.referencePairs.Clear();
    foreach (Reference reference in list)
    {
      Reference r = reference;
      if (source.Count<ReferencedPoint>((Func<ReferencedPoint, bool>) (p => p.Reference.Equals((object) r))) == 0)
      {
        this.DimReferences.Append(r);
        foreach (ReferencedPoint key in dictionary.Keys)
        {
          if (dictionary[key].EqualTo(r))
          {
            this.referencePairs.Add(key, r);
            break;
          }
        }
      }
    }
    this.DimReferences.Append(rP.Reference);
    this.referencePairs.Add(rP, rP.Reference);
    return DimReferencesManager.addResult.Replace;
  }

  public DimReferencesManager.addResult AddReferenceED(ReferencedPoint rP)
  {
    List<ReferencedPoint> source = this.PointIsOverlappedED(rP);
    Autodesk.Revit.DB.Element element1 = this._revitDoc.GetElement(rP.Reference);
    List<ReferencedPoint> olTemp = new List<ReferencedPoint>();
    if (element1.Category.Id.IntegerValue == -2001320 && (element1.Name.Contains("FLAT") || element1.Name.Contains("WARPED")))
    {
      foreach (ReferencedPoint referencedPoint in source)
      {
        Autodesk.Revit.DB.Element element2 = this._revitDoc.GetElement(referencedPoint.Reference);
        if (element2.Category.Id.IntegerValue == -2001320 && element2.GetSuperComponent() != null && element1.GetSuperComponent() != null && !(element2.GetSuperComponent().Id != element1.GetSuperComponent().Id))
        {
          if (element2.Name.Contains("FLAT") && element1.Name.Contains("WARPED"))
            return DimReferencesManager.addResult.DoNotAdd;
          if (element1.Name.Contains("FLAT") && element2.Name.Contains("WARPED"))
            olTemp.Add(referencedPoint);
        }
      }
      List<Reference> list = this.DimReferences.Cast<Reference>().ToList<Reference>();
      this.DimReferences.Clear();
      foreach (Reference reference in list)
      {
        Reference r = reference;
        if (olTemp.Count<ReferencedPoint>((Func<ReferencedPoint, bool>) (p => p.Reference.Equals((object) r))) == 0)
          this.DimReferences.Append(r);
      }
      source.RemoveAll((Predicate<ReferencedPoint>) (e => olTemp.Contains(e)));
    }
    if (source.Count == 0)
    {
      this.DimReferences.Append(rP.Reference);
      return DimReferencesManager.addResult.Add;
    }
    if (source.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (p => ProjectionUtils.ProjectPointOnDirection(p.Point, this._dimFromDirection) >= ProjectionUtils.ProjectPointOnDirection(rP.Point, this._dimFromDirection))).Count<ReferencedPoint>() != 0)
      return DimReferencesManager.addResult.DoNotAdd;
    List<Reference> list1 = this.DimReferences.Cast<Reference>().ToList<Reference>();
    this.DimReferences.Clear();
    foreach (Reference reference in list1)
    {
      Reference r = reference;
      if (source.Count<ReferencedPoint>((Func<ReferencedPoint, bool>) (p => p.Reference.Equals((object) r))) == 0)
        this.DimReferences.Append(r);
    }
    this.DimReferences.Append(rP.Reference);
    return DimReferencesManager.addResult.Replace;
  }

  public void AddElementReference(Reference reference, View view)
  {
    if (this.DimReferences.Cast<Reference>().Contains<Reference>(reference) || !(this._revitDoc.GetElement(reference.ElementId) is Autodesk.Revit.DB.FamilyInstance element))
      return;
    DimensionEdge dimFromSide = this._dimAlongSide == DimensionEdge.Bottom || this._dimAlongSide == DimensionEdge.Top ? DimensionEdge.Left : DimensionEdge.Bottom;
    foreach (ReferencedPoint rP in HiddenGeomReferenceCalculator.GetDimLineReference(element, dimFromSide, view))
    {
      int num = (int) this.AddReference(rP);
    }
    ++this.Count;
  }

  public void AddElementReferenceEDrawing(Reference reference, View view, XYZ principalAxis)
  {
    if (this.DimReferences.Cast<Reference>().Contains<Reference>(reference) || !(this._revitDoc.GetElement(reference.ElementId) is Autodesk.Revit.DB.FamilyInstance element))
      return;
    List<Line> refLines = new List<Line>();
    foreach (ReferencedPoint rP in HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(element, principalAxis, view, out refLines))
    {
      int num = (int) this.AddReference(rP);
    }
    ++this.Count;
  }

  public void AddGridLevelReference(Autodesk.Revit.DB.Element gridLevel, Line gridLevelLine)
  {
    if (!((GeometryObject) gridLevelLine != (GeometryObject) null) || gridLevel == null)
      return;
    int num = (int) this.AddReference(new ReferencedPoint()
    {
      Reference = new Reference(gridLevel),
      Point = gridLevelLine.GetEndPoint(0)
    });
  }

  public void AddExtentReference(ReferencedPoint extentReference)
  {
    this.DimReferences.Append(extentReference.Reference);
    ProjectionUtils.ProjectPointOnDirection(extentReference.Point, this._dimAlongDirection);
    this._existingDimensionDictionary.Add(extentReference, extentReference.Reference);
    this.referencePairs.Add(extentReference, extentReference.Reference);
  }

  private static List<ReferencedPoint> PointIsOverlappedSF(
    ReferencedPoint refPoint,
    Dictionary<ReferencedPoint, Reference> existingPoints,
    XYZ alongDirection)
  {
    List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
    double other = ProjectionUtils.ProjectPointOnDirection(refPoint.LocalPoint, alongDirection);
    foreach (ReferencedPoint key in existingPoints.Keys)
    {
      if (ProjectionUtils.ProjectPointOnDirection(key.LocalPoint, alongDirection).ApproximatelyEquals(other, 1.0 / 96.0))
        referencedPointList.Add(key);
    }
    if (!existingPoints.ContainsKey(refPoint))
      existingPoints.Add(refPoint, refPoint.Reference);
    return referencedPointList;
  }

  private static List<ReferencedPoint> PointIsOverlapped(
    ReferencedPoint refPoint,
    Dictionary<ReferencedPoint, Reference> existingPoints,
    XYZ alongDirection,
    XYZ fromDirection)
  {
    List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
    double other = ProjectionUtils.ProjectPointOnDirection(refPoint.Point, alongDirection);
    foreach (ReferencedPoint key in existingPoints.Keys)
    {
      if (ProjectionUtils.ProjectPointOnDirection(key.Point, alongDirection).ApproximatelyEquals(other, 1.0 / 96.0))
        referencedPointList.Add(key);
    }
    existingPoints.Add(refPoint, refPoint.Reference);
    return referencedPointList;
  }

  private static List<ReferencedPoint> PointIsOverlappedED(
    ReferencedPoint refPoint,
    Dictionary<ReferencedPoint, Reference> existingPoints,
    XYZ alongDirection,
    XYZ fromDirection)
  {
    List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
    double other = ProjectionUtils.ProjectPointOnDirection(refPoint.Point, alongDirection);
    foreach (ReferencedPoint key in existingPoints.Keys)
    {
      if (ProjectionUtils.ProjectPointOnDirection(key.Point, alongDirection).ApproximatelyEquals(other, 0.00032552083333333332))
        referencedPointList.Add(key);
    }
    existingPoints.Add(refPoint, refPoint.Reference);
    return referencedPointList;
  }

  private static List<ReferencedPoint> PointIsOverlappedHWD(
    ReferencedPoint refPoint,
    Dictionary<ReferencedPoint, Reference> existingPoints,
    XYZ alongDirection,
    XYZ fromDirection)
  {
    List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
    double other = ProjectionUtils.ProjectPointOnDirection(refPoint.Point, alongDirection);
    foreach (ReferencedPoint key in existingPoints.Keys)
    {
      if (ProjectionUtils.ProjectPointOnDirection(key.Point, alongDirection).ApproximatelyEquals(other, 1.0 / 192.0))
        referencedPointList.Add(key);
    }
    existingPoints.Add(refPoint, refPoint.Reference);
    return referencedPointList;
  }

  public List<ReferencedPoint> PointIsOverlapped(ReferencedPoint refPoint)
  {
    return DimReferencesManager.PointIsOverlapped(refPoint, this._existingDimensionDictionary, this._dimAlongDirection, this._dimFromDirection);
  }

  public List<ReferencedPoint> PointIsOverlappedED(ReferencedPoint refPoint)
  {
    return DimReferencesManager.PointIsOverlappedED(refPoint, this._existingDimensionDictionary, this._dimAlongDirection, this._dimFromDirection);
  }

  public List<ReferencedPoint> PointIsOverlappedHWD(ReferencedPoint refPoint)
  {
    return DimReferencesManager.PointIsOverlappedHWD(refPoint, this._existingDimensionDictionary, this._dimAlongDirection, this._dimFromDirection);
  }

  public List<ReferencedPoint> PointIsOverlappedSF(ReferencedPoint refPoint)
  {
    XYZ alongDirection = this._dimAlongSide == DimensionEdge.Left || this._dimAlongSide == DimensionEdge.Right ? XYZ.BasisX : XYZ.BasisY;
    return DimReferencesManager.PointIsOverlappedSF(refPoint, this._existingDimensionDictionary, alongDirection);
  }

  public enum addResult
  {
    Add,
    Replace,
    DoNotAdd,
  }
}
