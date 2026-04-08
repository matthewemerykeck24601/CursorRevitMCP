// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM_Reinforcement_Utils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using CNCExport.Utils.ElementUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AdminUtils;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.AdminTools;

public class CAM_Reinforcement_Utils
{
  public static List<Tuple<double, List<Curve>>> getBarsGeometry(
    Document doc,
    List<Element> rebarElements,
    List<Element> problematicBars)
  {
    List<Tuple<double, List<Curve>>> barsGeometry = new List<Tuple<double, List<Curve>>>();
    foreach (Element rebarElement in rebarElements)
    {
      bool problematic = false;
      Tuple<double, List<Curve>> barGeometry = CAM_Reinforcement_Utils.getBarGeometry(doc, rebarElement, out problematic);
      if (!problematic && barGeometry != null)
        barsGeometry.Add(barGeometry);
      else
        problematicBars.Add(rebarElement);
    }
    return barsGeometry;
  }

  public static Tuple<double, List<Curve>> getBarGeometry(
    Document doc,
    Element bar,
    out bool problematic)
  {
    problematic = false;
    List<Curve> curveList1 = new List<Curve>();
    XYZ q = (XYZ) null;
    Face face1 = (Face) null;
    XYZ source1 = XYZ.Zero;
    XYZ source2 = XYZ.Zero;
    foreach (Solid instanceSolid in Solids.GetInstanceSolids(bar))
    {
      if (instanceSolid.Faces.Size == 4)
      {
        int num1 = 0;
        int num2 = 0;
        List<PlanarFace> planarFaceList = new List<PlanarFace>();
        foreach (Face face2 in instanceSolid.Faces)
        {
          if (face2 is CylindricalFace)
            ++num1;
          if (face2 is PlanarFace)
          {
            ++num2;
            planarFaceList.Add(face2 as PlanarFace);
          }
        }
        if (num1 == 2 && num2 == 2)
        {
          Arc arc1 = planarFaceList[0].GetEdgesAsCurveLoops().First<CurveLoop>().First<Curve>() as Arc;
          Arc arc2 = planarFaceList[1].GetEdgesAsCurveLoops().First<CurveLoop>().First<Curve>() as Arc;
          XYZ center1 = arc1.Center;
          XYZ center2 = arc2.Center;
          curveList1.Add((Curve) Line.CreateBound(center1, center2));
          continue;
        }
      }
      foreach (Face face3 in instanceSolid.Faces)
      {
        if (face3 is RevolvedFace)
        {
          if (!(face1 is RevolvedFace) || !(face1 as RevolvedFace).Origin.IsAlmostEqualTo((face3 as RevolvedFace).Origin))
          {
            RevolvedFace revolvedFace = face3 as RevolvedFace;
            Plane byNormalAndOrigin = Plane.CreateByNormalAndOrigin(revolvedFace.Axis, revolvedFace.Origin);
            revolvedFace.get_Radius(0).AngleOnPlaneTo(byNormalAndOrigin.XVec, byNormalAndOrigin.Normal);
            revolvedFace.get_Radius(1).AngleOnPlaneTo(byNormalAndOrigin.XVec, byNormalAndOrigin.Normal);
            double num = (revolvedFace.Curve as Arc).Center.DistanceTo(XYZ.Zero);
            XYZ end0 = new XYZ();
            XYZ end1 = new XYZ();
            XYZ xyz = new XYZ();
            List<CurveLoop> list1 = revolvedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
            bool flag = false;
            foreach (Curve curve1 in list1.FirstOrDefault<CurveLoop>())
            {
              Curve curve2 = curve1;
              if (curve1 is HermiteSpline)
              {
                List<XYZ> list2 = curve1.Tessellate().ToList<XYZ>();
                curve2 = (Curve) Arc.Create(list2[0], list2[1], curve1.Evaluate(0.5, true));
              }
              if (curve2 is Arc)
              {
                Arc arc = curve2 as Arc;
                if (arc.Normal.CrossProduct(revolvedFace.Axis).IsAlmostEqualTo(XYZ.Zero))
                  xyz = (arc.Evaluate(0.5, true) - arc.Center).Normalize();
                else if (!flag)
                {
                  end0 = arc.Center;
                  flag = true;
                }
                else if (!end0.IsAlmostEqualTo(arc.Center))
                {
                  end1 = arc.Center;
                  break;
                }
              }
            }
            (new XYZ(end0.X, end0.Y, 0.0) - revolvedFace.Origin).Normalize();
            if ((new XYZ(end1.X, end1.Y, 0.0) - revolvedFace.Origin).Normalize().IsAlmostEqualTo(revolvedFace.get_Radius(0)))
              ;
            XYZ pointOnArc = revolvedFace.Origin + xyz * num;
            Curve curve = (Curve) Arc.Create(end0, end1, pointOnArc);
            if ((GeometryObject) curve != (GeometryObject) null)
            {
              q = (XYZ) null;
              curveList1.Add(curve);
            }
          }
          else
            continue;
        }
        if (face3 is CylindricalFace)
        {
          CylindricalFace cylindricalFace = face3 as CylindricalFace;
          double num = 0.0;
          foreach (Curve curve in cylindricalFace.GetEdgesAsCurveLoops().First<CurveLoop>())
          {
            if (curve is Line && Math.Abs(curve.ApproximateLength) > num)
              num = Math.Abs(curve.ApproximateLength);
          }
          XYZ origin = cylindricalFace.Origin;
          XYZ endpoint2 = cylindricalFace.Origin + cylindricalFace.Axis.Negate() * num;
          if (q == null || !Util.IsParallel(cylindricalFace.Axis, q) || (!endpoint2.IsAlmostEqualTo(source2) || !origin.IsAlmostEqualTo(source1)) && (!origin.IsAlmostEqualTo(source2) || !endpoint2.IsAlmostEqualTo(source1)))
          {
            curveList1.Add((Curve) Line.CreateBound(origin, endpoint2));
            source1 = origin;
            source2 = endpoint2;
            q = cylindricalFace.Axis;
          }
          else
            continue;
        }
        face1 = face3;
      }
    }
    List<Curve> curveList2 = new List<Curve>();
    Curve curve3 = (Curve) null;
    List<int> intList = new List<int>();
    foreach (Curve cv in curveList1)
    {
      int num = curveList1.IndexOf(cv);
      bool flag1 = false;
      XYZ endPoint1 = cv.GetEndPoint(0);
      XYZ endPoint2 = cv.GetEndPoint(1);
      foreach (Curve curve4 in curveList1)
      {
        if (!curve4.Equals((object) cv) && (curve4.GetEndPoint(0).IsAlmostEqualTo(endPoint1) && !curve4.GetEndPoint(1).IsAlmostEqualTo(endPoint2) || curve4.GetEndPoint(1).IsAlmostEqualTo(endPoint1) && !curve4.GetEndPoint(0).IsAlmostEqualTo(endPoint2)))
        {
          flag1 = true;
          break;
        }
      }
      if (!flag1)
      {
        curve3 = cv;
        curveList2.Add(curve3);
        intList.Add(num);
        break;
      }
      bool flag2 = false;
      foreach (Curve curve5 in curveList1)
      {
        if (!curve5.Equals((object) cv) && (curve5.GetEndPoint(0).IsAlmostEqualTo(endPoint2) && !curve5.GetEndPoint(1).IsAlmostEqualTo(endPoint1) || curve5.GetEndPoint(1).IsAlmostEqualTo(endPoint2) && !curve5.GetEndPoint(0).IsAlmostEqualTo(endPoint1)))
        {
          flag2 = true;
          break;
        }
      }
      if (!flag2)
      {
        curve3 = CAM_Utils.trueReverse(cv);
        curveList2.Add(curve3);
        intList.Add(num);
        break;
      }
    }
    int num3 = 0;
    int count = curveList1.Count;
    do
    {
      foreach (Curve cv in curveList1)
      {
        int num4 = curveList1.IndexOf(cv);
        if (cv.GetEndPoint(0).IsAlmostEqualTo(curve3.GetEndPoint(1)) && !cv.GetEndPoint(1).IsAlmostEqualTo(curve3.GetEndPoint(0)))
        {
          if (!intList.Contains(num4))
          {
            if (curve3 is Line && cv is Line && (curve3 as Line).Direction.IsAlmostEqualTo((cv as Line).Direction))
            {
              --count;
              curveList2.Remove(curve3);
              Line bound = Line.CreateBound(curve3.GetEndPoint(0), cv.GetEndPoint(1));
              curveList2.Add((Curve) bound);
              intList.Add(num4);
              curve3 = (Curve) bound;
              break;
            }
            curve3 = cv;
            curveList2.Add(curve3);
            intList.Add(num4);
            break;
          }
        }
        else if (cv.GetEndPoint(1).IsAlmostEqualTo(curve3.GetEndPoint(1)) && !cv.GetEndPoint(0).IsAlmostEqualTo(curve3.GetEndPoint(0)) && !intList.Contains(num4))
        {
          if (curve3 is Line && cv is Line)
          {
            Curve curve6 = CAM_Utils.trueReverse(cv);
            if ((curve3 as Line).Direction.IsAlmostEqualTo((curve6 as Line).Direction))
            {
              --count;
              curveList2.Remove(curve3);
              Line bound = Line.CreateBound(curve3.GetEndPoint(0), curve6.GetEndPoint(1));
              curveList2.Add((Curve) bound);
              intList.Add(num4);
              curve3 = (Curve) bound;
              break;
            }
          }
          curve3 = CAM_Utils.trueReverse(cv);
          curveList2.Add(curve3);
          intList.Add(num4);
          break;
        }
      }
      ++num3;
    }
    while (curveList2.Count < count && num3 < 100);
    if (num3 >= 100)
    {
      problematic = true;
      return (Tuple<double, List<Curve>>) null;
    }
    Element element = doc.GetElement(bar.GetTypeId());
    Tuple<double, List<Curve>> barGeometry = new Tuple<double, List<Curve>>(element.LookupParameter("BAR_DIAMETER") == null ? 0.03 : element.LookupParameter("BAR_DIAMETER").AsDouble(), curveList2);
    if (curveList2.Count != 0)
      return barGeometry;
    problematic = true;
    return (Tuple<double, List<Curve>>) null;
  }

  public static List<BoundingBoxXYZ> GetMeshBoundingBox(
    List<Element> meshList,
    Document revitDoc,
    Transform transform)
  {
    List<BoundingBoxXYZ> meshBoundingBox = new List<BoundingBoxXYZ>();
    foreach (Element mesh in meshList)
    {
      List<Line> source = new List<Line>();
      List<Arc> arcList = new List<Arc>();
      if (!(mesh is FamilyInstance))
        throw new Exception("Invalid Mesh");
      Options options = new Options()
      {
        DetailLevel = ViewDetailLevel.Fine
      };
      mesh.GetParameters("DIM_WWF_XX");
      foreach (GeometryObject geometryObject in (mesh.get_Geometry(options).First<GeometryObject>() as GeometryInstance).SymbolGeometry.ToList<GeometryObject>())
      {
        if (geometryObject is Line)
        {
          if (revitDoc.GetElement(geometryObject.GraphicsStyleId).Name != "EdgeDimLines")
            source.Add((geometryObject as Line).CreateTransformed((mesh as FamilyInstance).GetTransform()) as Line);
        }
        else if (geometryObject is Arc)
          arcList.Add(geometryObject as Arc);
      }
      XYZ xyz1 = new XYZ(double.MinValue, double.MinValue, double.MinValue);
      XYZ xyz2 = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue);
      double maxX_ = double.MinValue;
      double maxY_ = double.MinValue;
      double maxZ_ = double.MinValue;
      double minX_ = double.MaxValue;
      double minY_ = double.MaxValue;
      double minZ_ = double.MaxValue;
      List<XYZ> vertexes = new List<XYZ>();
      foreach (Line line in source)
      {
        for (int index = 0; index < 2; ++index)
        {
          XYZ endPoint = line.GetEndPoint(index);
          XYZ xyz3 = transform.Inverse.OfPoint(endPoint);
          vertexes.Add(xyz3);
        }
      }
      XYZ xyz4 = new XYZ();
      if (source.Count > 0)
        xyz4 = source.ElementAt<Line>(0).Direction.CrossProduct(source.ElementAt<Line>(1).Direction);
      foreach (Curve curve in arcList)
      {
        foreach (XYZ point in (IEnumerable<XYZ>) curve.Tessellate())
          vertexes.Add(transform.Inverse.OfPoint(point));
      }
      CAM_Utils.GetMinMax(vertexes, out maxX_, out maxY_, out maxZ_, out minX_, out minY_, out minZ_);
      XYZ xyz5 = new XYZ(maxX_, maxY_, maxZ_);
      XYZ xyz6 = new XYZ(minX_, minY_, minZ_);
      XYZ xyz7 = transform.Inverse.OfVector(xyz4.Normalize());
      XYZ xyz8 = new XYZ(Math.Abs(xyz7.X), Math.Abs(xyz7.Y), Math.Abs(xyz7.Z));
      meshBoundingBox.Add(new BoundingBoxXYZ()
      {
        Max = xyz5 + xyz8 * 0.0094,
        Min = xyz6 - xyz8 * 0.0094
      });
    }
    return meshBoundingBox;
  }

  public static List<Element> GetMeshFromElements(
    List<ElementId> assemblyElements,
    Document revitDoc)
  {
    return assemblyElements.Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)).Where<Element>((Func<Element, bool>) (elem => Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("MESH") || Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("WWF") || Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("SHEARGRID") || Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("SHEAR GRID") || Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("CGRID"))).Where<Element>((Func<Element, bool>) (elem => Parameters.GetParameterAsString(elem, "MESH_SHEET") == "")).ToList<Element>();
  }
}
