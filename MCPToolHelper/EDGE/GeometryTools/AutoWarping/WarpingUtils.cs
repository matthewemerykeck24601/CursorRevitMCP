// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.WarpingUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Linq;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

public class WarpingUtils
{
  public static XYZ GetPlanDirectionForSFElement(FamilyInstance sfInstance, Transform transform)
  {
    XYZ directionOfElementInPlan = Solids.GetDirectionOfElementInPlan((Element) sfInstance, transform, out Curve _);
    if (directionOfElementInPlan == null)
      return (XYZ) null;
    XYZ point = (sfInstance.Location as LocationPoint).Point;
    return SpotElevationPoint.GetDirectionFromFamilyOriginSF(sfInstance, directionOfElementInPlan, point);
  }

  public static XYZ[] GetLineWithVoid(Line l, FamilyInstance instance, Transform transform)
  {
    Curve curve1 = (Curve) l;
    Solid solid = Solids.GetInstanceSolids((Element) instance).First<Solid>();
    SolidCurveIntersectionOptions intersectionOptions = new SolidCurveIntersectionOptions();
    intersectionOptions.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;
    Curve curve2 = curve1;
    SolidCurveIntersectionOptions options = intersectionOptions;
    SolidCurveIntersection source = solid.IntersectWithCurve(curve2, options);
    int index = source.Count<Curve>() - 1;
    try
    {
      CurveExtents curveSegmentExtents = source.GetCurveSegmentExtents(index);
      double endParameter = curveSegmentExtents.EndParameter;
      double startParameter = curveSegmentExtents.StartParameter;
      XYZ point1 = curve1.Evaluate(endParameter, false);
      XYZ point2 = curve1.Evaluate(startParameter, false);
      XYZ[] lineWithVoid = new XYZ[2]
      {
        null,
        transform.Inverse.OfPoint(point1)
      };
      lineWithVoid[0] = transform.Inverse.OfPoint(point2);
      return lineWithVoid;
    }
    catch (Exception ex)
    {
      return (XYZ[]) null;
    }
  }

  public static Line CreateFlatLine(Line line)
  {
    if (!line.IsBound)
      return Line.CreateUnbound(new XYZ(line.Origin.X, line.Origin.Y, 0.0), new XYZ(line.Direction.X, line.Direction.Y, 0.0).Normalize());
    XYZ endPoint1 = line.GetEndPoint(0);
    XYZ endPoint2 = line.GetEndPoint(1);
    return Line.CreateBound(new XYZ(endPoint1.X, endPoint1.Y, 0.0), new XYZ(endPoint2.X, endPoint2.Y, 0.0));
  }
}
