// Decompiled with JetBrains decompiler
// Type: HiddenGeomReferenceCalculatorEDrawing
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System.Collections.Generic;

#nullable disable
public class HiddenGeomReferenceCalculatorEDrawing
{
  public static List<ReferencedPoint> GetDimLineReference(
    Autodesk.Revit.DB.FamilyInstance inst,
    XYZ principalAxis,
    View view,
    out List<Line> refLines)
  {
    if (view == null)
    {
      refLines = new List<Line>();
      return (List<ReferencedPoint>) null;
    }
    XYZ dimFromDirection = principalAxis;
    if (inst != null)
    {
      List<ReferencedPoint> dimLineReference = new List<ReferencedPoint>();
      List<Line> lineList = new List<Line>();
      List<ReferencedPoint> collection1 = new List<ReferencedPoint>();
      List<Line> collection2 = new List<Line>();
      Document document = inst.Document;
      Autodesk.Revit.DB.Options options = document.Application.Create.NewGeometryOptions();
      if (options != null)
      {
        options.ComputeReferences = true;
        options.DetailLevel = document.ActiveView.DetailLevel;
        options.IncludeNonVisibleObjects = true;
      }
      GeometryElement geometryElement = inst.get_Geometry(options);
      GeometryInstance geometryInstance = (GeometryInstance) null;
      foreach (GeometryObject geometryObject in geometryElement)
      {
        if (geometryObject is GeometryInstance)
        {
          geometryInstance = geometryObject as GeometryInstance;
          break;
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
              Autodesk.Revit.DB.ElementId graphicsStyleId = line.GraphicsStyleId;
              Autodesk.Revit.DB.Element element = inst.Document.GetElement(graphicsStyleId);
              if (element != null && element.Name == "EdgeDimLines")
              {
                Line transformed = line.CreateTransformed(inst.GetTransform()) as Line;
                if (HiddenGeomReferenceCalculatorEDrawing.AreParallel(transformed, dimFromDirection.Normalize()))
                {
                  dimLineReference.Add(new ReferencedPoint()
                  {
                    Reference = line.Reference,
                    Point = transformed.GetEndPoint(0)
                  });
                  dimLineReference.Add(new ReferencedPoint()
                  {
                    Reference = line.Reference,
                    Point = transformed.GetEndPoint(1)
                  });
                  lineList.Add(transformed);
                  flag = true;
                }
                else if (HiddenGeomReferenceCalculatorEDrawing.LineIsParallelToDimPlane(transformed, dimFromDirection, view.ViewDirection))
                {
                  collection1.Add(new ReferencedPoint()
                  {
                    Reference = line.GetEndPointReference(0),
                    Point = transformed.GetEndPoint(0)
                  });
                  collection1.Add(new ReferencedPoint()
                  {
                    Reference = line.GetEndPointReference(1),
                    Point = transformed.GetEndPoint(1)
                  });
                  collection2.Add(transformed);
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
                Autodesk.Revit.DB.ElementId graphicsStyleId = line.GraphicsStyleId;
                Autodesk.Revit.DB.Element element = inst.Document.GetElement(graphicsStyleId);
                if (element != null && element.Name == "EdgeDimLines")
                {
                  Line transformed = line.CreateTransformed(inst.GetTransform()) as Line;
                  if (HiddenGeomReferenceCalculatorEDrawing.AreParallel(transformed, view.ViewDirection.Normalize()) && !flag)
                  {
                    dimLineReference.Add(new ReferencedPoint()
                    {
                      Reference = line.GetEndPointReference(1),
                      Point = transformed.GetEndPoint(1)
                    });
                    dimLineReference.Add(new ReferencedPoint()
                    {
                      Reference = line.GetEndPointReference(0),
                      Point = transformed.GetEndPoint(0)
                    });
                    lineList.Add(transformed);
                  }
                }
              }
            }
          }
          if (dimLineReference.Count == 0)
            dimLineReference.AddRange((IEnumerable<ReferencedPoint>) collection1);
          if (lineList.Count == 0)
            lineList.AddRange((IEnumerable<Line>) collection2);
          refLines = lineList;
          return dimLineReference;
        }
      }
    }
    refLines = new List<Line>();
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

  public static bool FamilyInstanceContainsEdgeDimLines(Autodesk.Revit.DB.FamilyInstance inst)
  {
    if (inst != null)
    {
      Document document = inst.Document;
      GeometryElement geoElement = inst.get_Geometry(new Autodesk.Revit.DB.Options()
      {
        ComputeReferences = true,
        DetailLevel = document.ActiveView.DetailLevel,
        IncludeNonVisibleObjects = true
      });
      List<GeometryObject> geometryObjectList = new List<GeometryObject>();
      geometryObjectList.AddRange((IEnumerable<GeometryObject>) HiddenGeomReferenceCalculatorEDrawing.GetLines(geoElement));
      foreach (GeometryObject geometryObject in geometryObjectList)
      {
        Autodesk.Revit.DB.ElementId graphicsStyleId = (geometryObject as Line).GraphicsStyleId;
        Autodesk.Revit.DB.Element element = inst.Document.GetElement(graphicsStyleId);
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
        lines.AddRange((IEnumerable<GeometryObject>) HiddenGeomReferenceCalculatorEDrawing.GetLines(geometryInstance.GetSymbolGeometry()));
      }
    }
    return lines;
  }

  private static bool AreParallel(Line line, XYZ directionVector)
  {
    return line.Direction.IsAlmostEqualTo(directionVector, 1E-05) | line.Direction.IsAlmostEqualTo(directionVector.Negate(), 1E-05);
  }
}
