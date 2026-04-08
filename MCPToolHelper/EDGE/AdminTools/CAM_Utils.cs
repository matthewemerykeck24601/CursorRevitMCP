// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM_Utils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utils.AdminUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.AdminTools;

public class CAM_Utils
{
  private const double shortCurveLength = 0.0035;

  public static List<CurveLoop> ExplodeCurveLoop(CurveLoop loop)
  {
    List<CurveLoop> curveLoopList = new List<CurveLoop>();
    List<CurveLoop> collection = new List<CurveLoop>();
    List<Curve> curveList = new List<Curve>();
    List<XYZ> source = new List<XYZ>();
    List<Curve> list1 = loop.ToList<Curve>();
    for (int index1 = 0; index1 < list1.Count; ++index1)
    {
      Curve cv = list1[index1];
      List<XYZ> list2 = source.Where<XYZ>((Func<XYZ, bool>) (p => p.IsAlmostEqualTo(cv.GetEndPoint(0)))).ToList<XYZ>();
      if (list2.Count > 0)
      {
        XYZ xyz = list2.FirstOrDefault<XYZ>();
        CurveLoop curveLoop = new CurveLoop();
        for (int index2 = source.IndexOf(xyz); index2 < index1; ++index2)
        {
          curveList.Remove(list1[index2]);
          curveLoop.Append(list1[index2]);
        }
        collection.Add(curveLoop);
      }
      curveList.Add(cv);
      source.Add(cv.GetEndPoint(0));
    }
    CurveLoop curveLoop1 = new CurveLoop();
    foreach (Curve curve in curveList)
      curveLoop1.Append(curve);
    curveLoopList.Add(curveLoop1);
    curveLoopList.AddRange((IEnumerable<CurveLoop>) collection);
    return curveLoopList;
  }

  public static Dictionary<CurveLoop, Tuple<CurveLoop, XYZ, bool>> GetFlattenedToZOuter(
    List<Tuple<CurveLoop, XYZ, bool>> originalCurveLoopList,
    Transform transform,
    double desiredZ,
    double shortCurveTol,
    out Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> upwardInner,
    out Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> downwardInner,
    out bool OverallSplines)
  {
    upwardInner = new Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>>();
    downwardInner = new Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>>();
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ, bool>> flattenedToZouter = new Dictionary<CurveLoop, Tuple<CurveLoop, XYZ, bool>>();
    OverallSplines = false;
    foreach (Tuple<CurveLoop, XYZ, bool> originalCurveLoop in originalCurveLoopList)
    {
      CurveLoop viaTransform = CurveLoop.CreateViaTransform(originalCurveLoop.Item1, transform.Inverse);
      CurveLoop curveLoop = new CurveLoop();
      XYZ xyz1 = new XYZ(Math.Round(viaTransform.First<Curve>().GetEndPoint(0).X, 6), Math.Round(viaTransform.First<Curve>().GetEndPoint(0).Y, 6), desiredZ);
      bool flag = false;
      foreach (Curve curve in viaTransform)
      {
        if (!(curve is Line) || !Util.IsParallel((curve as Line).Direction, XYZ.BasisZ))
        {
          XYZ xyz2 = xyz1;
          XYZ xyz3 = new XYZ(Math.Round(curve.GetEndPoint(1).X, 6), Math.Round(curve.GetEndPoint(1).Y, 6), desiredZ);
          if (xyz2.DistanceTo(xyz3) <= 0.0035)
          {
            if (curve is Line)
            {
              XYZ xyz4 = new XYZ((curve as Line).Direction.X, (curve as Line).Direction.Y, 0.0).Normalize();
              xyz3 = xyz2 + xyz4 * 0.0035;
            }
            else if (curve is Arc)
              xyz3 = xyz2 + (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize() * 0.0035;
          }
          Curve bound;
          if (curve is Line)
            bound = (Curve) Line.CreateBound(xyz2, xyz3);
          else if (curve is Arc)
          {
            XYZ pointOnArc = new XYZ((curve as Arc).Evaluate(0.5, true).X, (curve as Arc).Evaluate(0.5, true).Y, desiredZ);
            bound = (Curve) Arc.Create(xyz2, xyz3, pointOnArc);
          }
          else
          {
            OverallSplines = true;
            flag = true;
            break;
          }
          xyz1 = xyz3;
          curveLoop.Append(bound);
        }
      }
      if (!flag && curveLoop.HasPlane() && curveLoop.FirstOrDefault<Curve>().GetEndPoint(0).IsAlmostEqualTo(curveLoop.LastOrDefault<Curve>().GetEndPoint(1)))
      {
        curveLoop.Transform(transform);
        List<CurveLoop> source = CAM_Utils.ExplodeCurveLoop(curveLoop);
        if (source.Count > 0)
        {
          flattenedToZouter.Add(source.First<CurveLoop>(), originalCurveLoop);
          source.Remove(source.First<CurveLoop>());
          foreach (CurveLoop key in source)
          {
            if (originalCurveLoop.Item3)
              upwardInner.Add(key, new Tuple<CurveLoop, XYZ>(originalCurveLoop.Item1, originalCurveLoop.Item2));
            else
              downwardInner.Add(key, new Tuple<CurveLoop, XYZ>(originalCurveLoop.Item1, originalCurveLoop.Item2));
          }
        }
        else
          flattenedToZouter.Add(curveLoop, originalCurveLoop);
      }
    }
    return flattenedToZouter;
  }

  public static Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> GetFlattenedToZ(
    List<Tuple<CurveLoop, XYZ>> originalCurveLoopList,
    Transform transform,
    double desiredZ,
    double shortCurveTol,
    out bool OverallSplines)
  {
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> flattenedToZ = new Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>>();
    OverallSplines = false;
    foreach (Tuple<CurveLoop, XYZ> originalCurveLoop in originalCurveLoopList)
    {
      CurveLoop source = originalCurveLoop.Item1;
      source.Transform(transform.Inverse);
      CurveLoop curveLoop = new CurveLoop();
      XYZ xyz1 = new XYZ(Math.Round(source.First<Curve>().GetEndPoint(0).X, 6), Math.Round(source.First<Curve>().GetEndPoint(0).Y, 6), desiredZ);
      bool flag = false;
      foreach (Curve curve in source)
      {
        if (!(curve is Line) || !Util.IsParallel((curve as Line).Direction, XYZ.BasisZ))
        {
          XYZ xyz2 = xyz1;
          XYZ xyz3 = new XYZ(Math.Round(curve.GetEndPoint(1).X, 6), Math.Round(curve.GetEndPoint(1).Y, 6), desiredZ);
          if (xyz2.DistanceTo(xyz3) <= 0.0035)
          {
            if (curve is Line)
            {
              XYZ xyz4 = new XYZ((curve as Line).Direction.X, (curve as Line).Direction.Y, 0.0).Normalize();
              xyz3 = xyz2 + xyz4 * 0.0035;
            }
            else if (curve is Arc)
              xyz3 = xyz2 + (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize() * 0.0035;
          }
          Curve bound;
          if (curve is Line)
            bound = (Curve) Line.CreateBound(xyz2, xyz3);
          else if (curve is Arc)
          {
            XYZ pointOnArc = new XYZ((curve as Arc).Evaluate(0.5, true).X, (curve as Arc).Evaluate(0.5, true).Y, desiredZ);
            bound = (Curve) Arc.Create(xyz2, xyz3, pointOnArc);
          }
          else
          {
            OverallSplines = true;
            flag = true;
            break;
          }
          xyz1 = xyz3;
          curveLoop.Append(bound);
        }
      }
      if (!flag && curveLoop.HasPlane() && curveLoop.FirstOrDefault<Curve>().GetEndPoint(0).IsAlmostEqualTo(curveLoop.LastOrDefault<Curve>().GetEndPoint(1)))
      {
        curveLoop.Transform(transform);
        flattenedToZ.Add(curveLoop, originalCurveLoop);
      }
    }
    return flattenedToZ;
  }

  public static Tuple<CurveLoop, CurveLoop> GetFlattenedToZ(
    CurveLoop curveLoop,
    Transform transform,
    double desiredZ,
    double shortCurveTol,
    out bool OverallSplines)
  {
    OverallSplines = false;
    Tuple<CurveLoop, CurveLoop> flattenedToZ = (Tuple<CurveLoop, CurveLoop>) null;
    CurveLoop viaTransform = CurveLoop.CreateViaTransform(curveLoop, transform.Inverse);
    if (viaTransform.Count<Curve>() == 0)
      return (Tuple<CurveLoop, CurveLoop>) null;
    CurveLoop source = new CurveLoop();
    XYZ xyz1 = new XYZ(Math.Round(viaTransform.First<Curve>().GetEndPoint(0).X, 6), Math.Round(viaTransform.First<Curve>().GetEndPoint(0).Y, 6), desiredZ);
    bool flag = false;
    foreach (Curve curve in viaTransform)
    {
      if (!(curve is Line) || !Util.IsParallel((curve as Line).Direction, XYZ.BasisZ))
      {
        XYZ xyz2 = xyz1;
        XYZ xyz3 = new XYZ(Math.Round(curve.GetEndPoint(1).X, 6), Math.Round(curve.GetEndPoint(1).Y, 6), desiredZ);
        if (xyz2.DistanceTo(xyz3) <= 0.0035)
        {
          if (curve is Line)
          {
            XYZ xyz4 = new XYZ((curve as Line).Direction.X, (curve as Line).Direction.Y, 0.0).Normalize();
            xyz3 = xyz2 + xyz4 * 0.0035;
          }
          else if (curve is Arc)
            xyz3 = xyz2 + (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize() * 0.0035;
        }
        Curve bound;
        if (curve is Line)
          bound = (Curve) Line.CreateBound(xyz2, xyz3);
        else if (curve is Arc)
        {
          XYZ pointOnArc = new XYZ((curve as Arc).Evaluate(0.5, true).X, (curve as Arc).Evaluate(0.5, true).Y, desiredZ);
          bound = (Curve) Arc.Create(xyz2, xyz3, pointOnArc);
        }
        else
        {
          OverallSplines = true;
          flag = true;
          break;
        }
        xyz1 = xyz3;
        source.Append(bound);
      }
    }
    if (flag)
      return (Tuple<CurveLoop, CurveLoop>) null;
    if (source.Count<Curve>() == 1 || source.HasPlane() && source.FirstOrDefault<Curve>().GetEndPoint(0).IsAlmostEqualTo(source.LastOrDefault<Curve>().GetEndPoint(1)))
    {
      source.Transform(transform);
      flattenedToZ = new Tuple<CurveLoop, CurveLoop>(source, curveLoop);
    }
    return flattenedToZ;
  }

  public static Dictionary<CurveLoop, CurveLoop> GetFlattenedToZ(
    List<CurveLoop> originalCurveLoopList,
    Transform transform,
    double desiredZ,
    double shortCurveTol,
    out bool OverallSplines)
  {
    Dictionary<CurveLoop, CurveLoop> flattenedToZ1 = new Dictionary<CurveLoop, CurveLoop>();
    OverallSplines = false;
    foreach (CurveLoop originalCurveLoop in originalCurveLoopList)
    {
      bool OverallSplines1;
      Tuple<CurveLoop, CurveLoop> flattenedToZ2 = CAM_Utils.GetFlattenedToZ(originalCurveLoop, transform, desiredZ, shortCurveTol, out OverallSplines1);
      if (flattenedToZ2 == null)
      {
        if (OverallSplines1)
          OverallSplines = true;
      }
      else
        flattenedToZ1.Add(flattenedToZ2.Item1, flattenedToZ2.Item2);
    }
    return flattenedToZ1;
  }

  public static bool GetMaterialIdForSolid(
    Solid solid,
    Dictionary<Solid, ElementId> solidMaterialList,
    out ElementId materialId)
  {
    foreach (KeyValuePair<Solid, ElementId> solidMaterial in solidMaterialList)
    {
      PlanarFace planarFace1 = solidMaterial.Key.Faces.get_Item(0) as PlanarFace;
      PlanarFace planarFace2 = solidMaterial.Key.Faces.get_Item(1) as PlanarFace;
      PlanarFace planarFace3 = solid.Faces.get_Item(0) as PlanarFace;
      PlanarFace planarFace4 = solid.Faces.get_Item(1) as PlanarFace;
      if (planarFace1.Origin.IsAlmostEqualTo(planarFace3.Origin) && planarFace2.Origin.IsAlmostEqualTo(planarFace4.Origin))
      {
        materialId = solidMaterial.Value;
        return true;
      }
    }
    materialId = (ElementId) null;
    return false;
  }

  public static List<int> GetVoidCounterList(List<CurveLoop> curveLoops, Transform transform)
  {
    List<int> voidCounterList = new List<int>();
    for (int index = 0; index < curveLoops.Count; ++index)
    {
      CurveLoop curveLoop1 = curveLoops[index];
      int num = 0;
      foreach (CurveLoop curveLoop2 in curveLoops)
      {
        if (!curveLoop2.Equals((object) curveLoop1) && CAM_Utils.CurveLoopContainsCurveLoop(curveLoop2, curveLoop1, transform))
          ++num;
      }
      if (num % 2 != 0)
        voidCounterList.Add(index);
    }
    return voidCounterList;
  }

  public static bool CurveLoopContainsPoint(CurveLoop curveLoop, XYZ point, Transform transform = null)
  {
    int num = 0;
    Line line = transform != null ? Line.CreateUnbound(transform.Inverse.OfPoint(point), transform.Inverse.OfVector(curveLoop.GetPlane().YVec)) : Line.CreateUnbound(point, curveLoop.GetPlane().YVec);
    List<XYZ> source = new List<XYZ>();
    foreach (Curve curve1 in curveLoop)
    {
      IntersectionResultArray resultArray = new IntersectionResultArray();
      Curve curve2 = transform != null ? curve1.CreateTransformed(transform.Inverse) : curve1;
      if (line.Intersect(curve2, out resultArray) == SetComparisonResult.Overlap)
      {
        IntersectionResult intersectionResult = resultArray.get_Item(0);
        if (intersectionResult.UVPoint.U >= 0.0)
        {
          XYZ hitPoint = intersectionResult.XYZPoint;
          if (!source.Any<XYZ>((Func<XYZ, bool>) (p => p.IsAlmostEqualTo(hitPoint))))
          {
            source.Add(hitPoint);
            ++num;
          }
        }
      }
    }
    return num % 2 != 0;
  }

  public static bool CurveLoopContainsCurveLoop(
    CurveLoop curveLoop,
    CurveLoop curveLoop2,
    Transform transform = null)
  {
    if (curveLoop2.Equals((object) curveLoop))
      return false;
    foreach (Curve curve1 in curveLoop2)
    {
      int num1 = 0;
      if (curve1 is Line)
      {
        Line line = transform != null ? Line.CreateUnbound(transform.Inverse.OfPoint(curve1.GetEndPoint(0)), transform.Inverse.OfVector(curveLoop.GetPlane().YVec)) : Line.CreateUnbound(curve1.GetEndPoint(0), curveLoop.GetPlane().YVec);
        List<XYZ> source = new List<XYZ>();
        foreach (Curve curve2 in curveLoop)
        {
          IntersectionResultArray resultArray = new IntersectionResultArray();
          Curve curve3 = transform != null ? curve2.CreateTransformed(transform.Inverse) : curve2;
          if (line.Intersect(curve3, out resultArray) == SetComparisonResult.Overlap)
          {
            IntersectionResult intersectionResult = resultArray.get_Item(0);
            if (intersectionResult.UVPoint.U >= 0.0)
            {
              XYZ hitPoint = intersectionResult.XYZPoint;
              if (!source.Any<XYZ>((Func<XYZ, bool>) (p => p.IsAlmostEqualTo(hitPoint))))
              {
                source.Add(hitPoint);
                ++num1;
              }
            }
          }
        }
        if (num1 % 2 == 0)
          return false;
      }
      else if (curve1 is Arc)
      {
        foreach (XYZ xyz in (IEnumerable<XYZ>) curve1.Tessellate())
        {
          Line line = transform != null ? Line.CreateUnbound(transform.Inverse.OfPoint(xyz), transform.Inverse.OfVector(curveLoop.GetPlane().YVec)) : Line.CreateUnbound(xyz, curveLoop.GetPlane().YVec);
          int num2 = 0;
          foreach (Curve curve4 in curveLoop)
          {
            IntersectionResultArray resultArray = new IntersectionResultArray();
            Curve curve5 = transform != null ? curve4.CreateTransformed(transform.Inverse) : curve4;
            if (line.Intersect(curve5, out resultArray) == SetComparisonResult.Overlap && resultArray.get_Item(0).UVPoint.U >= 0.0)
              ++num2;
          }
          if (num2 % 2 == 0)
            return false;
        }
      }
    }
    return true;
  }

  public static bool GetLayerDictionary(
    Dictionary<Solid, Tuple<string, double, bool>> LayerDictionary,
    List<Tuple<CurveLoop, XYZ, bool>> outerCurves,
    List<Tuple<CurveLoop, XYZ>> upwardFacingCurves,
    List<Tuple<CurveLoop, XYZ>> downwardFacingCurves,
    Document revitDoc,
    string material,
    double slabWeight,
    double minZ,
    double maxZ,
    double height,
    out bool overallSplineError,
    Transform transform)
  {
    overallSplineError = false;
    bool OverallSplines = false;
    if (height <= 0.0)
      return true;
    List<CurveLoop> curveLoopList1 = new List<CurveLoop>();
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ, bool>> dictionary1 = new Dictionary<CurveLoop, Tuple<CurveLoop, XYZ, bool>>();
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> upwardInner;
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> downwardInner;
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ, bool>> flattenedToZouter = CAM_Utils.GetFlattenedToZOuter(outerCurves, transform, minZ, revitDoc.Application.ShortCurveTolerance, out upwardInner, out downwardInner, out OverallSplines);
    if (OverallSplines)
      overallSplineError = true;
    if (flattenedToZouter == null)
      return false;
    List<CurveLoop> curveLoopList2 = new List<CurveLoop>();
    List<CurveLoop> curveLoopList3 = new List<CurveLoop>();
    Dictionary<double, List<CurveLoop>> dictionary2 = new Dictionary<double, List<CurveLoop>>();
    Dictionary<double, List<CurveLoop>> dictionary3 = new Dictionary<double, List<CurveLoop>>();
    Dictionary<double, List<CurveLoop>> dictionary4 = new Dictionary<double, List<CurveLoop>>();
    Dictionary<double, List<CurveLoop>> dictionary5 = new Dictionary<double, List<CurveLoop>>();
    if (flattenedToZouter.Keys.Count < 1)
      return false;
    foreach (CurveLoop key in flattenedToZouter.Keys)
    {
      Tuple<CurveLoop, XYZ, bool> tuple = flattenedToZouter[key];
      double z = transform.Inverse.OfPoint(tuple.Item2).Z;
      if (tuple.Item3)
      {
        if (!Util.IsEqual(z, maxZ))
        {
          if (dictionary2.ContainsKey(z))
            dictionary2[z].Add(key);
          else
            dictionary2.Add(z, new List<CurveLoop>() { key });
        }
      }
      else if (!Util.IsEqual(z, minZ))
      {
        if (dictionary4.ContainsKey(z))
          dictionary4[z].Add(key);
        else
          dictionary4.Add(z, new List<CurveLoop>() { key });
      }
    }
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> flattenedToZ1 = CAM_Utils.GetFlattenedToZ(upwardFacingCurves, transform, minZ, revitDoc.Application.ShortCurveTolerance, out OverallSplines);
    if (OverallSplines)
      overallSplineError = true;
    if (flattenedToZ1 == null)
      return false;
    foreach (CurveLoop key in upwardInner.Keys)
      flattenedToZ1.Add(key, upwardInner[key]);
    foreach (CurveLoop key in flattenedToZ1.Keys)
    {
      Tuple<CurveLoop, XYZ> tuple = flattenedToZ1[key];
      double z = transform.Inverse.OfPoint(tuple.Item2).Z;
      if (!Util.IsEqual(z, maxZ))
      {
        if (dictionary3.ContainsKey(z))
          dictionary3[z].Add(key);
        else
          dictionary3.Add(z, new List<CurveLoop>() { key });
      }
    }
    Dictionary<CurveLoop, Tuple<CurveLoop, XYZ>> flattenedToZ2 = CAM_Utils.GetFlattenedToZ(downwardFacingCurves, transform, minZ, revitDoc.Application.ShortCurveTolerance, out OverallSplines);
    if (OverallSplines)
      overallSplineError = true;
    if (flattenedToZ2 == null)
      return false;
    foreach (CurveLoop key in downwardInner.Keys)
      flattenedToZ2.Add(key, downwardInner[key]);
    foreach (CurveLoop key in flattenedToZ2.Keys)
    {
      Tuple<CurveLoop, XYZ> tuple = flattenedToZ2[key];
      double z = transform.Inverse.OfPoint(tuple.Item2).Z;
      if (!Util.IsEqual(z, minZ))
      {
        if (dictionary5.ContainsKey(z))
          dictionary5[z].Add(key);
        else
          dictionary5.Add(z, new List<CurveLoop>() { key });
      }
    }
    Solid extrusionGeometry1 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
    {
      flattenedToZouter.Keys.FirstOrDefault<CurveLoop>()
    }, transform.BasisZ, height);
    foreach (CurveLoop key in flattenedToZouter.Keys)
    {
      try
      {
        Solid extrusionGeometry2 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
        {
          key
        }, transform.BasisZ, height);
        BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(extrusionGeometry1, extrusionGeometry2, BooleanOperationsType.Union);
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    Solid solid1 = (Solid) null;
    Solid solid0_1 = (Solid) null;
    foreach (CurveLoop key in flattenedToZ1.Keys)
    {
      try
      {
        Solid extrusionGeometry3 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
        {
          key
        }, transform.BasisZ, height);
        if ((GeometryObject) solid1 == (GeometryObject) null)
          solid1 = extrusionGeometry3;
        else
          BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid1, extrusionGeometry3, BooleanOperationsType.Union);
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    foreach (CurveLoop key in flattenedToZ2.Keys)
    {
      try
      {
        Solid extrusionGeometry4 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
        {
          key
        }, transform.BasisZ, height);
        if ((GeometryObject) solid0_1 == (GeometryObject) null)
          solid0_1 = extrusionGeometry4;
        else
          BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid0_1, extrusionGeometry4, BooleanOperationsType.Union);
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    List<Solid> solidList = new List<Solid>();
    foreach (double key in dictionary2.Keys)
    {
      try
      {
        List<Solid> collection = new List<Solid>();
        foreach (CurveLoop curveLoop in dictionary2[key])
        {
          Solid extrusionGeometry5 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
          {
            curveLoop
          }, transform.BasisZ, height);
          collection.Add(extrusionGeometry5);
        }
        if (dictionary3.ContainsKey(key))
        {
          foreach (CurveLoop curveLoop in dictionary3[key])
          {
            Solid extrusionGeometry6 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
            {
              curveLoop
            }, transform.BasisZ, height);
            foreach (Solid solid0_2 in collection)
              BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid0_2, extrusionGeometry6, BooleanOperationsType.Difference);
          }
        }
        solidList.AddRange((IEnumerable<Solid>) collection);
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    foreach (double key in dictionary4.Keys)
    {
      try
      {
        List<Solid> collection = new List<Solid>();
        foreach (CurveLoop curveLoop in dictionary4[key])
        {
          Solid extrusionGeometry7 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
          {
            curveLoop
          }, transform.BasisZ, height);
          collection.Add(extrusionGeometry7);
        }
        if (dictionary5.ContainsKey(key))
        {
          foreach (CurveLoop curveLoop in dictionary5[key])
          {
            Solid extrusionGeometry8 = GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
            {
              curveLoop
            }, transform.BasisZ, height);
            foreach (Solid solid0_3 in collection)
              BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid0_3, extrusionGeometry8, BooleanOperationsType.Difference);
          }
        }
        solidList.AddRange((IEnumerable<Solid>) collection);
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    if ((GeometryObject) solid0_1 != (GeometryObject) null || (GeometryObject) solid1 != (GeometryObject) null)
    {
      try
      {
        Solid solid2 = !((GeometryObject) solid0_1 == (GeometryObject) null) ? (!((GeometryObject) solid1 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid0_1, solid1, BooleanOperationsType.Union) : solid0_1) : solid1;
        foreach (Solid solid1_1 in solidList)
          BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid2, solid1_1, BooleanOperationsType.Difference);
        Solid key = BooleanOperationsUtils.ExecuteBooleanOperation(extrusionGeometry1, solid2, BooleanOperationsType.Difference);
        List<CurveLoop> curveLoopList4 = new List<CurveLoop>();
        foreach (Face face in key.Faces)
        {
          PlanarFace planarFace = face as PlanarFace;
          if ((GeometryObject) planarFace != (GeometryObject) null && planarFace.FaceNormal.IsAlmostEqualTo(transform.BasisZ.Negate()))
            curveLoopList4.AddRange((IEnumerable<CurveLoop>) planarFace.GetEdgesAsCurveLoops());
        }
        List<CurveLoop> curveLoopList5 = new List<CurveLoop>();
        if (curveLoopList4.Count < 2)
        {
          LayerDictionary.Add(key, new Tuple<string, double, bool>(material, slabWeight, false));
        }
        else
        {
          foreach (CurveLoop curveLoop2 in curveLoopList4)
          {
            bool flag = false;
            foreach (CurveLoop curveLoop in curveLoopList4)
            {
              if (!curveLoop2.Equals((object) curveLoop) && CAM_Utils.CurveLoopContainsCurveLoop(curveLoop, curveLoop2, transform))
              {
                flag = true;
                break;
              }
            }
            if (!flag)
              curveLoopList5.Add(curveLoop2);
          }
          if (curveLoopList5.Count > 1)
          {
            foreach (CurveLoop curveLoop in curveLoopList5)
            {
              List<CurveLoop> profileLoops = new List<CurveLoop>()
              {
                curveLoop
              };
              foreach (CurveLoop curveLoop2 in curveLoopList4)
              {
                if (CAM_Utils.CurveLoopContainsCurveLoop(curveLoop, curveLoop2, transform))
                  profileLoops.Add(curveLoop2);
              }
              LayerDictionary.Add(GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) profileLoops, transform.BasisZ, height), new Tuple<string, double, bool>(material, slabWeight, false));
            }
          }
          else
            LayerDictionary.Add(key, new Tuple<string, double, bool>(material, slabWeight, false));
        }
      }
      catch (Exception ex)
      {
        return false;
      }
    }
    else
    {
      List<CurveLoop> curveLoopList6 = new List<CurveLoop>();
      foreach (Face face in extrusionGeometry1.Faces)
      {
        PlanarFace planarFace = face as PlanarFace;
        if ((GeometryObject) planarFace != (GeometryObject) null && planarFace.FaceNormal.IsAlmostEqualTo(transform.BasisZ.Negate()))
          curveLoopList6.AddRange((IEnumerable<CurveLoop>) planarFace.GetEdgesAsCurveLoops());
      }
      List<CurveLoop> curveLoopList7 = new List<CurveLoop>();
      if (curveLoopList6.Count < 2)
      {
        LayerDictionary.Add(extrusionGeometry1, new Tuple<string, double, bool>(material, slabWeight, false));
      }
      else
      {
        foreach (CurveLoop curveLoop2 in curveLoopList6)
        {
          bool flag = false;
          foreach (CurveLoop curveLoop in curveLoopList6)
          {
            if (!curveLoop2.Equals((object) curveLoop) && CAM_Utils.CurveLoopContainsCurveLoop(curveLoop, curveLoop2, transform))
            {
              flag = true;
              break;
            }
          }
          if (!flag)
            curveLoopList7.Add(curveLoop2);
        }
        if (curveLoopList7.Count > 1)
        {
          foreach (CurveLoop curveLoop in curveLoopList7)
          {
            List<CurveLoop> profileLoops = new List<CurveLoop>()
            {
              curveLoop
            };
            foreach (CurveLoop curveLoop2 in curveLoopList6)
            {
              if (CAM_Utils.CurveLoopContainsCurveLoop(curveLoop, curveLoop2, transform))
                profileLoops.Add(curveLoop2);
            }
            LayerDictionary.Add(GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) profileLoops, transform.BasisZ, height), new Tuple<string, double, bool>(material, slabWeight, false));
          }
        }
        else
          LayerDictionary.Add(extrusionGeometry1, new Tuple<string, double, bool>(material, slabWeight, false));
      }
    }
    return true;
  }

  public static void GetCurveLoops(
    Solid solid,
    List<Tuple<CurveLoop, XYZ, bool>> outerCurves,
    List<Tuple<CurveLoop, XYZ>> upwardFacingCurves,
    List<Tuple<CurveLoop, XYZ>> downwardFacingCurves,
    out double maxZ,
    out double minZ,
    Transform transform,
    double slopeTolerance = 0.01)
  {
    maxZ = double.MinValue;
    minZ = double.MaxValue;
    foreach (Face face in solid.Faces)
    {
      PlanarFace planarFace = face as PlanarFace;
      if ((GeometryObject) planarFace != (GeometryObject) null && Math.Abs(transform.Inverse.OfVector(planarFace.FaceNormal).Z) >= slopeTolerance)
      {
        List<CurveLoop> list = planarFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
        List<int> voidCounterList = CAM_Utils.GetVoidCounterList(list, transform);
        if (transform.Inverse.OfVector(planarFace.FaceNormal).Z > slopeTolerance)
        {
          XYZ point = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue);
          for (int index = 0; index < list.Count; ++index)
          {
            foreach (Curve curve in list[index])
            {
              if (point.Z > transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                point = transform.Inverse.OfPoint(curve.GetEndPoint(0));
              if (point.Z > transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                point = transform.Inverse.OfPoint(curve.GetEndPoint(1));
              if (maxZ < transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                maxZ = transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z;
              if (maxZ < transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                maxZ = transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z;
            }
          }
          for (int index = 0; index < list.Count; ++index)
          {
            if (!voidCounterList.Contains(index) || voidCounterList == null)
              outerCurves.Add(new Tuple<CurveLoop, XYZ, bool>(list[index], transform.OfPoint(point), true));
            else
              upwardFacingCurves.Add(new Tuple<CurveLoop, XYZ>(list[index], transform.OfPoint(point)));
          }
        }
        else if (Math.Abs(transform.Inverse.OfVector(planarFace.FaceNormal).Z) > slopeTolerance)
        {
          XYZ point = new XYZ(double.MinValue, double.MinValue, double.MinValue);
          for (int index = 0; index < list.Count; ++index)
          {
            foreach (Curve curve in list[index])
            {
              if (point.Z < transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                point = transform.Inverse.OfPoint(curve.GetEndPoint(0));
              if (point.Z < transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                point = transform.Inverse.OfPoint(curve.GetEndPoint(1));
              if (minZ > transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                minZ = transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z;
              if (minZ > transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                minZ = transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z;
            }
          }
          for (int index = 0; index < list.Count; ++index)
          {
            if (!voidCounterList.Contains(index) || voidCounterList == null)
              outerCurves.Add(new Tuple<CurveLoop, XYZ, bool>(list[index], transform.OfPoint(point), false));
            else
              downwardFacingCurves.Add(new Tuple<CurveLoop, XYZ>(list[index], transform.OfPoint(point)));
          }
        }
      }
    }
  }

  public static double[] GetMinMaxZ(Solid solid, Transform transform, double slopeTolerance = 0.01)
  {
    double num1 = double.MinValue;
    double num2 = double.MaxValue;
    foreach (Face face in solid.Faces)
    {
      PlanarFace planarFace = face as PlanarFace;
      if ((GeometryObject) planarFace != (GeometryObject) null && Math.Abs(transform.Inverse.OfVector(planarFace.FaceNormal).Z) >= slopeTolerance)
      {
        List<CurveLoop> list = planarFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
        CAM_Utils.GetVoidCounterList(list, transform);
        if (transform.Inverse.OfVector(planarFace.FaceNormal).Z > slopeTolerance)
        {
          XYZ xyz = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue);
          for (int index = 0; index < list.Count; ++index)
          {
            foreach (Curve curve in list[index])
            {
              if (xyz.Z > transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                xyz = transform.Inverse.OfPoint(curve.GetEndPoint(0));
              if (xyz.Z > transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                xyz = transform.Inverse.OfPoint(curve.GetEndPoint(1));
              if (num1 < transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                num1 = transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z;
              if (num1 < transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                num1 = transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z;
            }
          }
        }
        else if (Math.Abs(transform.Inverse.OfVector(planarFace.FaceNormal).Z) > slopeTolerance)
        {
          XYZ xyz = new XYZ(double.MinValue, double.MinValue, double.MinValue);
          for (int index = 0; index < list.Count; ++index)
          {
            foreach (Curve curve in list[index])
            {
              if (xyz.Z < transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                xyz = transform.Inverse.OfPoint(curve.GetEndPoint(0));
              if (xyz.Z < transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                xyz = transform.Inverse.OfPoint(curve.GetEndPoint(1));
              if (num2 > transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z)
                num2 = transform.Inverse.OfPoint(curve.GetEndPoint(0)).Z;
              if (num2 > transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z)
                num2 = transform.Inverse.OfPoint(curve.GetEndPoint(1)).Z;
            }
          }
        }
      }
    }
    return new double[2]{ num2, num1 };
  }

  public static Curve trueReverse(Curve cv)
  {
    if (!(cv is Arc))
      return cv.CreateReversed();
    Arc arc = cv as Arc;
    return (Curve) Arc.Create(arc.GetEndPoint(1), arc.GetEndPoint(0), arc.Evaluate(0.5, true));
  }

  public static void GetMinMax(
    List<XYZ> vertexes,
    out double maxX_,
    out double maxY_,
    out double maxZ_,
    out double minX_,
    out double minY_,
    out double minZ_)
  {
    double num1 = double.MinValue;
    double num2 = double.MinValue;
    double num3 = double.MinValue;
    double num4 = double.MaxValue;
    double num5 = double.MaxValue;
    double num6 = double.MaxValue;
    foreach (XYZ vertex in vertexes)
    {
      if (vertex.X > num1)
        num1 = vertex.X;
      if (vertex.Y > num2)
        num2 = vertex.Y;
      if (vertex.X > num3)
        num3 = vertex.Z;
      if (vertex.X < num4)
        num4 = vertex.X;
      if (vertex.Y < num5)
        num5 = vertex.Y;
      if (vertex.X < num6)
        num6 = vertex.Z;
    }
    maxX_ = num1;
    maxY_ = num2;
    maxZ_ = num3;
    minX_ = num4;
    minY_ = num5;
    minZ_ = num6;
  }

  public static BoundingBoxXYZ CreateTransformedBBoxFromElement(Element elem, Transform transform = null)
  {
    if (elem is FamilyInstance)
    {
      if (Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))
        return (BoundingBoxXYZ) null;
      FamilyInstance familyInstance = elem as FamilyInstance;
      if (familyInstance.GetSubComponentIds().Count > 0)
      {
        List<Element> elementList = new List<Element>();
        BoundingBoxXYZ bb1 = (BoundingBoxXYZ) null;
        Solid solid = Solids.GetInstanceSolids(elem, useViewDetail: true).FirstOrDefault<Solid>();
        if ((GeometryObject) solid != (GeometryObject) null && solid.Volume > 0.0)
        {
          elementList.Add(elem);
          bb1 = bb1 != null ? Utils.MiscUtils.MiscUtils.AdjustBBox(bb1, Util.getTransformedBoundingBox(transform, elem, true)) : Util.getTransformedBoundingBox(transform, elem, true);
        }
        foreach (ElementId subComponentId in (IEnumerable<ElementId>) familyInstance.GetSubComponentIds())
        {
          Element element = elem.Document.GetElement(subComponentId);
          if (Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))
          {
            elementList.Add(element);
            BoundingBoxXYZ transformedBoundingBox = Util.getTransformedBoundingBox(transform, element, true);
            bb1 = bb1 != null ? Utils.MiscUtils.MiscUtils.AdjustBBox(bb1, transformedBoundingBox) : transformedBoundingBox;
          }
        }
        return bb1;
      }
    }
    return Util.getTransformedBoundingBox(transform, elem, true);
  }

  public static int CompareStrings(string stringA, string stringB)
  {
    int num1 = 0;
    if (stringA == null || stringB == null)
      return -1;
    Regex regex1 = new Regex("\\D+");
    Regex regex2 = new Regex("\\d+");
    MatchCollection matchCollection1 = regex1.Matches(stringA);
    MatchCollection matchCollection2 = regex1.Matches(stringB);
    MatchCollection matchCollection3 = regex2.Matches(stringA);
    MatchCollection matchCollection4 = regex2.Matches(stringB);
    List<Match> matchList1 = new List<Match>();
    List<Match> matchList2 = new List<Match>();
    foreach (Match match in matchCollection1)
      matchList1.Add(match);
    foreach (Match match in matchCollection3)
      matchList1.Add(match);
    foreach (Match match in matchCollection2)
      matchList2.Add(match);
    foreach (Match match in matchCollection4)
      matchList2.Add(match);
    matchList1.Sort((Comparison<Match>) ((p, q) => p.Index.CompareTo(q.Index)));
    matchList2.Sort((Comparison<Match>) ((p, q) => p.Index.CompareTo(q.Index)));
    int num2 = matchList1.Count > matchList2.Count ? matchList1.Count : matchList2.Count;
    for (int index = 0; index < num2; ++index)
    {
      if (index >= matchList1.Count)
        return -1;
      if (index >= matchList2.Count)
        return 1;
      int result1 = 0;
      int result2 = 0;
      if (int.TryParse(matchList1[index].ToString(), out result1) && int.TryParse(matchList2[index].ToString(), out result2))
      {
        if (result1 != result2)
          return result1 < result2 ? -1 : 1;
        num1 = 0;
      }
      else
      {
        num1 = matchList1[index].ToString().CompareTo(matchList2[index].ToString());
        if (num1 != 0)
          return num1;
      }
    }
    return num1;
  }
}
