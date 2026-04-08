// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.HiddenGeomReferenceCalculator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class HiddenGeomReferenceCalculator
{
  public static List<ReferencedPoint> GetDimLineReference(
    FamilyInstance inst,
    DimensionEdge dimFromSide,
    View view)
  {
    if (view == null)
      return (List<ReferencedPoint>) null;
    XYZ dimFromDirection = dimFromSide == DimensionEdge.Top || dimFromSide == DimensionEdge.Bottom ? view.UpDirection : view.RightDirection;
    if (inst != null)
    {
      List<ReferencedPoint> dimLineReference = new List<ReferencedPoint>();
      List<ReferencedPoint> collection = new List<ReferencedPoint>();
      Document document = inst.Document;
      Options options = document.Application.Create.NewGeometryOptions();
      if (options != null)
      {
        options.ComputeReferences = true;
        options.DetailLevel = document.ActiveView.DetailLevel;
        options.IncludeNonVisibleObjects = true;
      }
      GeometryElement source = inst.get_Geometry(options);
      GeometryInstance geometryInstance = source.First<GeometryObject>() as GeometryInstance;
      if ((GeometryObject) geometryInstance == (GeometryObject) null)
      {
        foreach (GeometryObject geometryObject in source)
        {
          if (geometryObject is GeometryInstance)
          {
            geometryInstance = geometryObject as GeometryInstance;
            break;
          }
        }
      }
      if ((GeometryObject) geometryInstance != (GeometryObject) null)
      {
        GeometryElement symbolGeometry = geometryInstance.GetSymbolGeometry();
        if ((GeometryObject) symbolGeometry != (GeometryObject) null)
        {
          bool flag = false;
          foreach (GeometryObject geometryObject in symbolGeometry)
          {
            if (geometryObject is Line)
            {
              Line line = geometryObject as Line;
              ElementId graphicsStyleId = line.GraphicsStyleId;
              if (inst.Document.GetElement(graphicsStyleId).Name == "EdgeDimLines")
              {
                Line transformed = line.CreateTransformed(inst.GetTransform()) as Line;
                if (HiddenGeomReferenceCalculator.AreParallel(transformed, dimFromDirection.Normalize()))
                {
                  dimLineReference.Add(new ReferencedPoint()
                  {
                    Reference = line.Reference,
                    Point = transformed.GetEndPoint(0)
                  });
                  flag = true;
                }
                else if (HiddenGeomReferenceCalculator.LineIsParallelToDimPlane(transformed, dimFromDirection, view.ViewDirection))
                {
                  collection.Add(new ReferencedPoint()
                  {
                    Reference = line.GetEndPointReference(0),
                    Point = transformed.GetEndPoint(0)
                  });
                  flag = true;
                }
              }
            }
          }
          if (dimLineReference.Count == 0)
          {
            foreach (GeometryObject geometryObject in symbolGeometry)
            {
              if (geometryObject is Line)
              {
                Line line = geometryObject as Line;
                ElementId graphicsStyleId = line.GraphicsStyleId;
                if (inst.Document.GetElement(graphicsStyleId).Name == "EdgeDimLines")
                {
                  Line transformed = line.CreateTransformed(inst.GetTransform()) as Line;
                  if (HiddenGeomReferenceCalculator.AreParallel(transformed, view.ViewDirection.Normalize()) && !flag)
                    dimLineReference.Add(new ReferencedPoint()
                    {
                      Reference = line.GetEndPointReference(1),
                      Point = transformed.GetEndPoint(1)
                    });
                }
              }
            }
          }
          if (dimLineReference.Count == 0)
            dimLineReference.AddRange((IEnumerable<ReferencedPoint>) collection);
          return dimLineReference;
        }
      }
    }
    return (List<ReferencedPoint>) null;
  }

  public static bool LineIsParallelToDimPlane(
    Line transformedLine,
    XYZ dimFromDirection,
    XYZ viewDirection)
  {
    XYZ xyz1 = transformedLine.ComputeDerivatives(0.0, true).BasisX.Normalize();
    if (xyz1.CrossProduct(viewDirection).GetLength() < 1E-06)
      return false;
    XYZ xyz2 = dimFromDirection.CrossProduct(viewDirection);
    XYZ xyz3 = xyz1.CrossProduct(xyz2.Normalize());
    xyz3.GetLength();
    return xyz3.GetLength() > 0.99999;
  }

  public static bool FamilyInstanceContainsEdgeDimLines(FamilyInstance inst)
  {
    if (inst != null)
    {
      Document document = inst.Document;
      GeometryElement geoElement = inst.get_Geometry(new Options()
      {
        ComputeReferences = true,
        DetailLevel = document.ActiveView.DetailLevel,
        IncludeNonVisibleObjects = true
      });
      List<GeometryObject> geometryObjectList = new List<GeometryObject>();
      geometryObjectList.AddRange((IEnumerable<GeometryObject>) HiddenGeomReferenceCalculator.GetLines(geoElement));
      foreach (GeometryObject geometryObject in geometryObjectList)
      {
        ElementId graphicsStyleId = (geometryObject as Line).GraphicsStyleId;
        Element element = inst.Document.GetElement(graphicsStyleId);
        if (element != null && element.Name == "EdgeDimLines")
          return true;
      }
    }
    return false;
  }

  private static List<GeometryObject> GetLines(GeometryElement geoElement)
  {
    List<GeometryObject> lines = new List<GeometryObject>();
    if ((GeometryObject) geoElement == (GeometryObject) null)
      return lines;
    foreach (GeometryObject geometryObject in geoElement)
    {
      if (geometryObject is Line)
        lines.Add((GeometryObject) (geometryObject as Line));
      else if (geometryObject is GeometryInstance)
      {
        GeometryInstance geometryInstance = geometryObject as GeometryInstance;
        lines.AddRange((IEnumerable<GeometryObject>) HiddenGeomReferenceCalculator.GetLines(geometryInstance.GetSymbolGeometry()));
      }
    }
    return lines;
  }

  public static bool AreParallel(Line line, XYZ directionVector)
  {
    return line.Direction.IsAlmostEqualTo(directionVector, 1E-05) | line.Direction.IsAlmostEqualTo(directionVector.Negate(), 1E-05);
  }
}
