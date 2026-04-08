// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.LaserUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.AdminUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.AdminTools.CAM;

internal class LaserUtils
{
  public static CurveArray OverkillCurveArray(CurveArray input)
  {
    Dictionary<int, List<Curve>> dictionary = new Dictionary<int, List<Curve>>();
    Line line1 = (Line) null;
    int key1 = -1;
    foreach (Curve curve in input)
    {
      if (curve is Line line2)
      {
        if ((GeometryObject) line1 == (GeometryObject) null || !Util.IsParallel(line1.Direction, line2.Direction))
        {
          ++key1;
          dictionary.Add(key1, new List<Curve>() { curve });
        }
        else
          dictionary[key1].Add((Curve) line2);
        line1 = line2;
      }
      else
      {
        ++key1;
        line1 = (Line) null;
        dictionary.Add(key1, new List<Curve>() { curve });
      }
    }
    CurveArray curveArray = new CurveArray();
    foreach (int key2 in dictionary.Keys)
    {
      if (dictionary[key2].Count == 1)
      {
        curveArray.Append(dictionary[key2].First<Curve>());
      }
      else
      {
        List<XYZ> unrepeatedPoints = new List<XYZ>();
        List<XYZ> ep1s = dictionary[key2].Select<Curve, XYZ>((Func<Curve, XYZ>) (c => c.GetEndPoint(0))).ToList<XYZ>();
        List<XYZ> ep2s = dictionary[key2].Select<Curve, XYZ>((Func<Curve, XYZ>) (c => c.GetEndPoint(1))).ToList<XYZ>();
        unrepeatedPoints = ep1s.Where<XYZ>((Func<XYZ, bool>) (p => !ep2s.Any<XYZ>((Func<XYZ, bool>) (p2 => p.IsAlmostEqualTo(p2))))).Union<XYZ>(ep2s.Where<XYZ>((Func<XYZ, bool>) (p => !ep1s.Any<XYZ>((Func<XYZ, bool>) (p2 => p.IsAlmostEqualTo(p2)))))).ToList<XYZ>();
        if (unrepeatedPoints.Count == 2)
        {
          List<XYZ> list = ep1s.Where<XYZ>((Func<XYZ, bool>) (p => !unrepeatedPoints.Any<XYZ>((Func<XYZ, bool>) (p2 => p.IsAlmostEqualTo(p2))))).Union<XYZ>(ep2s.Where<XYZ>((Func<XYZ, bool>) (p => !unrepeatedPoints.Any<XYZ>((Func<XYZ, bool>) (p2 => p.IsAlmostEqualTo(p2))) && !ep1s.Any<XYZ>((Func<XYZ, bool>) (p2 => p.IsAlmostEqualTo(p2)))))).ToList<XYZ>();
          XYZ endpoint1 = unrepeatedPoints[0];
          XYZ xyz = unrepeatedPoints[1];
          double num1 = endpoint1.DistanceTo(xyz);
          foreach (XYZ source in list)
          {
            double num2 = unrepeatedPoints[0].DistanceTo(source);
            if (num2 > num1)
            {
              endpoint1 = unrepeatedPoints[0];
              xyz = source;
              num1 = num2;
            }
            double num3 = unrepeatedPoints[1].DistanceTo(source);
            if (num3 > num1)
            {
              endpoint1 = source;
              xyz = unrepeatedPoints[1];
              num1 = num3;
            }
          }
          Line bound = Line.CreateBound(endpoint1, xyz);
          curveArray.Append((Curve) bound);
        }
      }
    }
    return curveArray;
  }

  public static List<CurveLoop> GetRecesses(
    Solid solid,
    Autodesk.Revit.DB.Transform transform,
    double maxZ,
    double minZ)
  {
    List<CurveLoop> recesses = new List<CurveLoop>();
    foreach (Face face in SolidUtils.CreateTransformed(solid, transform.Inverse).Faces)
    {
      if (face is PlanarFace planarFace && (planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ) || planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ.Negate())) && !planarFace.Origin.Z.ApproximatelyEquals(maxZ) && !planarFace.Origin.Z.ApproximatelyEquals(minZ))
      {
        foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) planarFace.GetEdgesAsCurveLoops())
          recesses.Add(CurveLoop.CreateViaTransform(edgesAsCurveLoop, transform));
      }
    }
    return recesses;
  }

  public static CurveArray GetCurveArrayFromLoop(CurveLoop loop, Autodesk.Revit.DB.Transform transform)
  {
    CurveArray curveArrayFromLoop = new CurveArray();
    foreach (Curve curve1 in CurveLoop.CreateViaTransform(loop, transform.Inverse))
    {
      Curve curve2 = curve1.Clone();
      if (curve2 is Arc arc && !arc.Normal.IsAlmostEqualTo(XYZ.BasisZ))
        curve2 = arc.CreateReversed();
      curveArrayFromLoop.Append(curve2);
    }
    return curveArrayFromLoop;
  }

  public static void CheckDirectory(string path)
  {
    if (Directory.Exists(path))
      return;
    Directory.CreateDirectory(path);
  }

  public static List<ElementId> CollectCurveIds(List<List<ModelCurveArray>> mcaLists)
  {
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (List<ModelCurveArray> mcaList in mcaLists)
    {
      foreach (ModelCurveArray modelCurveArray in mcaList)
      {
        foreach (ModelCurve modelCurve in modelCurveArray)
          elementIdList.Add(modelCurve.Id);
      }
    }
    return elementIdList;
  }

  public static void AssignLineStyle(List<ModelCurveArray> mca, Category style, Document revitDoc)
  {
    foreach (ModelCurveArray modelCurveArray in mca)
    {
      foreach (CurveElement curveElement in modelCurveArray)
        curveElement.LineStyle = (Element) style.GetGraphicsStyle(GraphicsStyleType.Projection);
    }
  }

  public static Autodesk.Revit.DB.Color WindowsToAutodeskColor(System.Windows.Media.Color color)
  {
    return new Autodesk.Revit.DB.Color(color.R, color.G, color.B);
  }
}
