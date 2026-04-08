// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.DimensioningGeometryUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class DimensioningGeometryUtils
{
  public static Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> GetDimensioningPointsReferences(
    FamilyInstance structuralFraming,
    View view,
    Transform axisTransform = null,
    bool invert = false)
  {
    Transform transform = axisTransform != null ? axisTransform : Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
    List<ReferencedPoint> source1 = new List<ReferencedPoint>();
    FamilyInstance elem = structuralFraming;
    bool flag;
    ref bool local = ref flag;
    foreach (Solid symbolSolid in Solids.GetSymbolSolids((Element) elem, out local, options: new Options()
    {
      ComputeReferences = true
    }))
    {
      foreach (Edge edge in symbolSolid.Edges)
      {
        XYZ ep1 = edge.AsCurve().GetEndPoint(0);
        XYZ ep2 = edge.AsCurve().GetEndPoint(1);
        if (flag)
        {
          ep1 = structuralFraming.GetTransform().OfPoint(ep1);
          ep2 = structuralFraming.GetTransform().OfPoint(ep2);
        }
        if (!source1.Any<ReferencedPoint>((Func<ReferencedPoint, bool>) (refP => refP.Point.IsAlmostEqualTo(ep1))))
        {
          XYZ localPoint = transform.Inverse.OfPoint(ep1);
          source1.Add(new ReferencedPoint(edge.GetEndPointReference(0), ep1, localPoint));
        }
        if (!source1.Any<ReferencedPoint>((Func<ReferencedPoint, bool>) (refP => refP.Point.IsAlmostEqualTo(ep2))))
        {
          XYZ localPoint = transform.Inverse.OfPoint(ep2);
          source1.Add(new ReferencedPoint(edge.GetEndPointReference(1), ep2, localPoint));
        }
      }
    }
    double newZ = invert ? source1.Min<ReferencedPoint>((Func<ReferencedPoint, double>) (rp => rp.LocalPoint.Z)) : source1.Max<ReferencedPoint>((Func<ReferencedPoint, double>) (rp => rp.LocalPoint.Z));
    List<IGrouping<double, ReferencedPoint>> list1 = source1.GroupBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (refP => Math.Round(refP.LocalPoint.Y, 5))).ToList<IGrouping<double, ReferencedPoint>>();
    Dictionary<double, Dictionary<double, ReferencedPoint>> dictionary1 = new Dictionary<double, Dictionary<double, ReferencedPoint>>();
    foreach (IGrouping<double, ReferencedPoint> source2 in list1)
    {
      dictionary1.Add(source2.Key, new Dictionary<double, ReferencedPoint>());
      foreach (IGrouping<double, ReferencedPoint> source3 in source2.GroupBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (refP => Math.Round(refP.LocalPoint.X, 5))).ToList<IGrouping<double, ReferencedPoint>>())
        dictionary1[source2.Key].Add(source3.Key, DimensioningGeometryUtils.flattenPoint(source3.OrderByDescending<ReferencedPoint, double>((Func<ReferencedPoint, double>) (refP => refP.LocalPoint.Z)).First<ReferencedPoint>(), newZ));
    }
    List<IGrouping<double, ReferencedPoint>> list2 = source1.GroupBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (refP => Math.Round(refP.LocalPoint.X, 5))).ToList<IGrouping<double, ReferencedPoint>>();
    Dictionary<double, Dictionary<double, ReferencedPoint>> dictionary2 = new Dictionary<double, Dictionary<double, ReferencedPoint>>();
    foreach (IGrouping<double, ReferencedPoint> source4 in list2)
    {
      dictionary2.Add(source4.Key, new Dictionary<double, ReferencedPoint>());
      foreach (IGrouping<double, ReferencedPoint> source5 in source4.GroupBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (refP => Math.Round(refP.LocalPoint.Y, 5))).ToList<IGrouping<double, ReferencedPoint>>())
        dictionary2[source4.Key].Add(source5.Key, DimensioningGeometryUtils.flattenPoint(source5.OrderByDescending<ReferencedPoint, double>((Func<ReferencedPoint, double>) (refP => refP.LocalPoint.Z)).First<ReferencedPoint>(), newZ));
    }
    double key1 = dictionary2.Keys.Min();
    double key2 = dictionary2.Keys.Max();
    double key3 = dictionary2[key1].Keys.Min();
    double key4 = dictionary2[key1].Keys.Max();
    double key5 = dictionary2[key2].Keys.Min();
    double key6 = dictionary2[key2].Keys.Max();
    double key7 = dictionary1.Keys.Min();
    double key8 = dictionary1.Keys.Max();
    double key9 = dictionary1[key7].Keys.Min();
    double key10 = dictionary1[key7].Keys.Max();
    double key11 = dictionary1[key8].Keys.Min();
    double key12 = dictionary1[key8].Keys.Max();
    pointsReferences.Add(DimensionEdge.Top, new Dictionary<DimensionEdge, ReferencedPoint>());
    pointsReferences.Add(DimensionEdge.Bottom, new Dictionary<DimensionEdge, ReferencedPoint>());
    pointsReferences.Add(DimensionEdge.Left, new Dictionary<DimensionEdge, ReferencedPoint>());
    pointsReferences.Add(DimensionEdge.Right, new Dictionary<DimensionEdge, ReferencedPoint>());
    pointsReferences[DimensionEdge.Top].Add(DimensionEdge.Left, dictionary2[key1][key4]);
    pointsReferences[DimensionEdge.Top].Add(DimensionEdge.Right, dictionary2[key2][key6]);
    pointsReferences[DimensionEdge.Bottom].Add(DimensionEdge.Left, dictionary2[key1][key3]);
    pointsReferences[DimensionEdge.Bottom].Add(DimensionEdge.Right, dictionary2[key2][key5]);
    pointsReferences[DimensionEdge.Left].Add(DimensionEdge.Top, dictionary1[key8][key11]);
    pointsReferences[DimensionEdge.Left].Add(DimensionEdge.Bottom, dictionary1[key7][key9]);
    pointsReferences[DimensionEdge.Right].Add(DimensionEdge.Top, dictionary1[key8][key12]);
    pointsReferences[DimensionEdge.Right].Add(DimensionEdge.Bottom, dictionary1[key7][key10]);
    return pointsReferences;
  }

  private static ReferencedPoint flattenPoint(ReferencedPoint pt, double newZ)
  {
    XYZ localPoint = new XYZ(pt.LocalPoint.X, pt.LocalPoint.Y, newZ);
    return new ReferencedPoint(pt.Reference, pt.Point, localPoint);
  }

  private static ReferencedPoint GetEndpointReference(PairedEdgeCurve pairedEdge, XYZ direction)
  {
    double num1 = DimensioningGeometryUtils.PointProjectedOnDirection(pairedEdge.GetTransformedCurve().GetEndPoint(0), direction);
    double num2 = DimensioningGeometryUtils.PointProjectedOnDirection(pairedEdge.GetTransformedCurve().GetEndPoint(1), direction);
    return new ReferencedPoint(pairedEdge.Edge.GetEndPointReference(num1 <= num2 ? 1 : 0), pairedEdge.Curve.GetEndPoint(num1 <= num2 ? 1 : 0));
  }

  private static double GetLeftMostEndPoint(Curve curve, XYZ direction)
  {
    return DimensioningGeometryUtils.PointProjectedOnDirection(curve.Evaluate(0.5, true), direction);
  }

  private static double PointProjectedOnDirection(XYZ point, XYZ direction)
  {
    return point.DotProduct(direction.Normalize());
  }

  private static double GetProjectedMidPoint(Edge curve, XYZ direction)
  {
    return curve.Evaluate(0.5).DotProduct(direction.Normalize());
  }

  private static TransformedFace GetDimensioningFaceForView(
    FamilyInstance structuralFraming,
    View view)
  {
    if (structuralFraming == null || view == null)
      return (TransformedFace) null;
    List<TransformedFace> list = DimensioningGeometryUtils.GetFaceArray(structuralFraming, true, view.DetailLevel).Where<TransformedFace>((Func<TransformedFace, bool>) (tFace => tFace.GetTransformedNormal(new UV(0.0, 0.0)).IsAlmostEqualTo(view.ViewDirection))).OrderByDescending<TransformedFace, double>((Func<TransformedFace, double>) (face => face.Face.Area)).ToList<TransformedFace>();
    return list.Any<TransformedFace>() ? list.First<TransformedFace>() : (TransformedFace) null;
  }

  private static List<TransformedFace> GetFaceArray(
    FamilyInstance elem,
    bool bComputeReferences,
    ViewDetailLevel detailLevel)
  {
    List<TransformedFace> faceArray = new List<TransformedFace>();
    Options options = new Options()
    {
      ComputeReferences = bComputeReferences,
      DetailLevel = detailLevel
    };
    GeometryElement source1 = elem.get_Geometry(options);
    bool flag = false;
    foreach (GeometryObject geometryObject in source1)
    {
      Solid solid = geometryObject as Solid;
      if ((GeometryObject) solid != (GeometryObject) null && solid.Faces.Size > 0)
      {
        flag = true;
        foreach (Face face in solid.Faces)
          faceArray.Add(new TransformedFace(face, Transform.Identity));
      }
    }
    if (!flag)
    {
      IEnumerable<GeometryInstance> source2 = source1.OfType<GeometryInstance>();
      if (source2.Any<GeometryInstance>())
      {
        Transform transform = elem.GetTransform();
        foreach (GeometryObject geometryObject in source2.First<GeometryInstance>().GetSymbolGeometry())
        {
          Solid solid = geometryObject as Solid;
          if ((GeometryObject) solid != (GeometryObject) null && solid.Faces.Size > 0)
          {
            foreach (Face face in solid.Faces)
              faceArray.Add(new TransformedFace(face, transform));
          }
        }
      }
    }
    return faceArray;
  }
}
