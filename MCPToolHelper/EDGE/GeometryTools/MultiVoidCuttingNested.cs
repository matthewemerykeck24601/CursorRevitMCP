// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.MultiVoidCuttingNested
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.GeometryTools;

public class MultiVoidCuttingNested
{
  public static void MultiCutter(
    Document doc,
    ICollection<Element> voidVoidList,
    ICollection<Element> solidVoidList,
    FamilyInstance familyInstanceToBeCut)
  {
    ICollection<Element> elements1 = (ICollection<Element>) new List<Element>();
    ICollection<Element> elements2 = (ICollection<Element>) new List<Element>();
    foreach (Element solidVoid in (IEnumerable<Element>) solidVoidList)
      elements1.Add(solidVoid);
    foreach (Element voidVoid in (IEnumerable<Element>) voidVoidList)
      elements2.Add(voidVoid);
    foreach (Element cuttingInstance in (IEnumerable<Element>) elements2)
    {
      BoundingBoxXYZ boundingBoxXyz = cuttingInstance.get_BoundingBox(doc.ActiveView);
      Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
      LogicalOrFilter filter = new LogicalOrFilter((ElementFilter) new BoundingBoxIntersectsFilter(outline), (ElementFilter) new BoundingBoxIsInsideFilter(outline));
      FilteredElementCollector elementCollector = new FilteredElementCollector(doc);
      elementCollector.WherePasses((ElementFilter) filter);
      foreach (Element element in elementCollector)
      {
        if (element.Id == familyInstanceToBeCut.Id && !InstanceVoidCutUtils.GetElementsBeingCut(cuttingInstance).Contains(familyInstanceToBeCut.Id))
          InstanceVoidCutUtils.AddInstanceVoidCut(doc, (Element) familyInstanceToBeCut, cuttingInstance);
      }
    }
    foreach (Element element1 in (IEnumerable<Element>) elements1)
    {
      BoundingBoxXYZ boundingBoxXyz = element1.get_BoundingBox(doc.ActiveView);
      Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
      LogicalOrFilter filter = new LogicalOrFilter((ElementFilter) new BoundingBoxIntersectsFilter(outline), (ElementFilter) new BoundingBoxIsInsideFilter(outline));
      FilteredElementCollector elementCollector = new FilteredElementCollector(doc);
      elementCollector.WherePasses((ElementFilter) filter);
      elementCollector.WherePasses((ElementFilter) new ElementIntersectsElementFilter(element1));
      foreach (Element element2 in elementCollector)
      {
        if (element2.Id == familyInstanceToBeCut.Id)
          SolidSolidCutUtils.AddCutBetweenSolids(doc, (Element) familyInstanceToBeCut, element1);
      }
    }
  }
}
