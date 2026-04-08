// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.SpotElevationPoint
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

public class SpotElevationPoint
{
  public XYZ planePoint { get; set; }

  public XYZ Point { get; set; }

  public XYZ gPoint { get; set; }

  public Dictionary<XYZ, Curve> SearchDirections { get; set; }

  public List<WarpingEdge> ConnectedEdges { get; set; }

  public ElementId elemId { get; set; }

  public Transform transform { get; set; }

  public bool HasErrorCondition { get; private set; }

  public string ErrorCondition { get; private set; }

  public SpotElevationPoint(FamilyInstance spotElevationInstance)
  {
    this.SearchDirections = new Dictionary<XYZ, Curve>();
    this.ConnectedEdges = new List<WarpingEdge>();
    this.transform = this.CalcTransform(spotElevationInstance);
    if (this.transform == null)
      this.transform = Transform.Identity;
    if (spotElevationInstance.Location is LocationPoint location)
    {
      this.gPoint = location.Point;
      this.Point = this.transform.Inverse.OfPoint(location.Point);
      this.planePoint = new XYZ(this.Point.X, this.Point.Y, 0.0);
      this.CalcDirections(spotElevationInstance.Document);
      this.HasErrorCondition = false;
      this.elemId = spotElevationInstance.Id;
    }
    else
    {
      this.HasErrorCondition = true;
      this.ErrorCondition = "Spot Elevation Family does not have a Location Point.  This Could be a two pick family";
    }
  }

  private Transform CalcTransform(FamilyInstance spotElevationInstance)
  {
    Element host = spotElevationInstance.Host;
    switch (host)
    {
      case ReferencePlane _:
        Plane plane1 = (host as ReferencePlane).GetPlane();
        Transform translation1 = Transform.CreateTranslation(plane1.Origin);
        translation1.BasisX = plane1.XVec;
        translation1.BasisY = plane1.YVec;
        translation1.BasisZ = plane1.Normal;
        return translation1;
      case Level _:
        Level level = host as Level;
        Plane plane2 = SketchPlane.Create(spotElevationInstance.Document, level.GetPlaneReference()).GetPlane();
        Transform translation2 = Transform.CreateTranslation(plane2.Origin);
        translation2.BasisX = plane2.XVec;
        translation2.BasisY = plane2.YVec;
        translation2.BasisZ = plane2.Normal;
        return translation2;
      default:
        return (Transform) null;
    }
  }

  private void CalcDirections(Document revitDoc)
  {
    Outline searchBox = this.GetSearchBox();
    List<FamilyInstance> list = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) new BoundingBoxIntersectsFilter(searchBox)).Cast<FamilyInstance>().ToList<FamilyInstance>();
    List<FamilyInstance> source = new List<FamilyInstance>();
    foreach (FamilyInstance structFramingElem in list)
    {
      FamilyInstance flatElement = AssemblyInstances.GetFlatElement(revitDoc, structFramingElem);
      foreach (Solid instanceSolid in Solids.GetInstanceSolids((Element) flatElement))
      {
        if ((GeometryObject) instanceSolid != (GeometryObject) null && instanceSolid.Volume > 0.0 && this.AdjustOutline(SpotElevationPoint.GetTransformedBoundingBox((Element) flatElement, Transform.Identity)).Intersects(searchBox, 0.0))
          source.Add(flatElement);
      }
    }
    foreach (FamilyInstance elem in source.Distinct<FamilyInstance>().ToList<FamilyInstance>())
    {
      Curve curve = (Curve) null;
      Transform transform = this.transform;
      ref Curve local = ref curve;
      XYZ directionOfElementInPlan = Solids.GetDirectionOfElementInPlan((Element) elem, transform, out local);
      if (directionOfElementInPlan != null)
      {
        XYZ xyz = this.transform.Inverse.OfVector(directionOfElementInPlan);
        XYZ key = new XYZ(xyz.X, xyz.Y, 0.0).Normalize();
        XYZ endPoint1 = curve.GetEndPoint(0);
        XYZ endPoint2 = curve.GetEndPoint(1);
        double rawParameter = (this.planePoint - endPoint1).DotProduct((endPoint2 - endPoint1).Normalize());
        double normalizedParameter = curve.ComputeNormalizedParameter(rawParameter);
        SearchDirectionOptions directionOptions = normalizedParameter <= 0.901 ? (normalizedParameter >= 0.099 ? SearchDirectionOptions.both : SearchDirectionOptions.forward) : SearchDirectionOptions.backward;
        if (directionOptions != SearchDirectionOptions.backward)
          this.SearchDirections.Add(key, curve);
        if (directionOptions != SearchDirectionOptions.forward)
          this.SearchDirections.Add(key.Negate(), curve);
      }
    }
    if (this.SearchDirections.Count != 0)
      return;
    this.HasErrorCondition = true;
    this.ErrorCondition = $"{this.ErrorCondition}{Environment.NewLine}No Structural Framing Elements Found within 6'-0\" of this spot elevation";
  }

  public XYZ GetTransformedPoint(Transform trans) => trans.OfPoint(this.planePoint);

  public static XYZ GetDirectionFromFamilyOrigin(
    FamilyInstance famInst,
    XYZ planDirection,
    XYZ FromPoint)
  {
    Transform transform = famInst.GetTransform();
    XYZ source = planDirection.CrossProduct(transform.BasisX).GetLength() <= planDirection.CrossProduct(transform.BasisY).GetLength() ? transform.BasisX : transform.BasisY;
    BoundingBoxXYZ boundingBoxXyz = famInst.get_BoundingBox((View) null);
    XYZ xyz = (boundingBoxXyz.Max + boundingBoxXyz.Min) / 2.0;
    return (new XYZ(xyz.X, xyz.Y, 0.0) - FromPoint).DotProduct(source) <= 0.0 ? source.Negate() : source;
  }

  public static XYZ GetDirectionFromFamilyOrigin(
    FamilyInstance famInst,
    XYZ planDirection,
    XYZ FromPoint,
    Transform transform)
  {
    Transform transform1 = famInst.GetTransform();
    XYZ source = planDirection.CrossProduct(transform.Inverse.OfPoint(transform1.BasisX)).GetLength() <= planDirection.CrossProduct(transform.Inverse.OfPoint(transform1.BasisY)).GetLength() ? transform.Inverse.OfPoint(transform1.BasisX) : transform.Inverse.OfPoint(transform1.BasisY);
    BoundingBoxXYZ boundingBoxXyz = famInst.get_BoundingBox((View) null);
    XYZ point = (boundingBoxXyz.Max + boundingBoxXyz.Min) / 2.0;
    XYZ xyz = transform.Inverse.OfPoint(point);
    return (new XYZ(xyz.X, xyz.Y, 0.0) - FromPoint).DotProduct(source) <= 0.0 ? source.Negate() : source;
  }

  public static XYZ GetDirectionFromFamilyOriginSF(
    FamilyInstance famInst,
    XYZ planDirection,
    XYZ FromPoint)
  {
    Transform transform = famInst.GetTransform();
    planDirection.CrossProduct(transform.BasisX).GetLength();
    planDirection.CrossProduct(transform.BasisY).GetLength();
    XYZ basisY = transform.BasisY;
    BoundingBoxXYZ boundingBoxXyz = famInst.get_BoundingBox((View) null);
    XYZ xyz = (boundingBoxXyz.Max + boundingBoxXyz.Min) / 2.0;
    return (new XYZ(xyz.X, xyz.Y, 0.0) - FromPoint).DotProduct(basisY) <= 0.0 ? basisY.Negate() : basisY;
  }

  private Outline GetSearchBox()
  {
    XYZ xyz1 = new XYZ();
    XYZ xyz2 = new XYZ();
    return this.AdjustOutline(new Outline(this.gPoint + this.transform.OfVector(new XYZ(-6.0, -6.0, -6.0)), this.gPoint + this.transform.OfVector(new XYZ(6.0, 6.0, 6.0))));
  }

  private Outline AdjustOutline(Outline outline)
  {
    XYZ xyz1 = new XYZ();
    XYZ xyz2 = new XYZ();
    Outline outline1 = new Outline(outline);
    XYZ xyz3 = new XYZ(outline.MaximumPoint.X > outline.MinimumPoint.X ? outline.MaximumPoint.X : outline.MinimumPoint.X, outline.MaximumPoint.Y > outline.MinimumPoint.Y ? outline.MaximumPoint.Y : outline.MinimumPoint.Y, outline.MaximumPoint.Z > outline.MinimumPoint.Z ? outline.MaximumPoint.Z : outline.MinimumPoint.Z);
    XYZ xyz4 = new XYZ(outline.MaximumPoint.X < outline.MinimumPoint.X ? outline.MaximumPoint.X : outline.MinimumPoint.X, outline.MaximumPoint.Y < outline.MinimumPoint.Y ? outline.MaximumPoint.Y : outline.MinimumPoint.Y, outline.MaximumPoint.Z < outline.MinimumPoint.Z ? outline.MaximumPoint.Z : outline.MinimumPoint.Z);
    outline1.MaximumPoint = xyz3;
    outline1.MinimumPoint = xyz4;
    return outline1;
  }

  private static Outline GetTransformedBoundingBox(Element elem, Transform transform)
  {
    List<XYZ> xyzList = new List<XYZ>();
    foreach (Solid instanceSolid in Solids.GetInstanceSolids(elem))
    {
      if (instanceSolid.Faces.Size > 0)
      {
        foreach (Face face in instanceSolid.Faces)
        {
          foreach (EdgeArray edgeLoop in face.EdgeLoops)
          {
            foreach (Edge edge in edgeLoop)
            {
              foreach (XYZ point in (IEnumerable<XYZ>) edge.Tessellate())
              {
                bool flag = false;
                XYZ xyz1 = transform.Inverse.OfPoint(point);
                foreach (XYZ xyz2 in xyzList)
                {
                  if (xyz1.Equals((object) xyz2))
                  {
                    flag = true;
                    break;
                  }
                }
                if (!flag)
                  xyzList.Add(xyz1);
              }
            }
          }
        }
      }
    }
    bool flag1 = true;
    double x1 = 0.0;
    double y1 = 0.0;
    double z1 = 0.0;
    double x2 = 0.0;
    double y2 = 0.0;
    double z2 = 0.0;
    foreach (XYZ xyz in xyzList)
    {
      if (flag1)
      {
        x1 = xyz.X;
        y1 = xyz.Y;
        z1 = xyz.Z;
        x2 = xyz.X;
        y2 = xyz.Y;
        z2 = xyz.Z;
        flag1 = false;
      }
      else
      {
        x1 = xyz.X > x1 ? xyz.X : x1;
        y1 = xyz.Y > y1 ? xyz.Y : y1;
        z1 = xyz.Z > z1 ? xyz.Z : z1;
        x2 = xyz.X < x2 ? xyz.X : x2;
        y2 = xyz.Y < y2 ? xyz.Y : y2;
        z2 = xyz.Z < z2 ? xyz.Z : z2;
      }
    }
    return new Outline(new XYZ(x1, y1, z1), new XYZ(x2, y2, z2));
  }
}
