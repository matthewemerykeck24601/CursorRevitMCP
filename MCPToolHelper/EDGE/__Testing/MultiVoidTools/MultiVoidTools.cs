// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.MultiVoidTools.MultiVoidTools
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing.MultiVoidTools;

public class MultiVoidTools
{
  private static HashSet<ElementId> _previouslySelected;
  private static UIDocument _uidoc;
  private static List<ElementId> _voidIds;

  public static void CutElements(List<Element> selectedVoids, List<ElementId> allVoids)
  {
    EDGE.__Testing.MultiVoidTools.MultiVoidTools._voidIds = allVoids;
    EDGE.__Testing.MultiVoidTools.MultiVoidTools._previouslySelected = new HashSet<ElementId>();
    EDGE.__Testing.MultiVoidTools.MultiVoidTools._uidoc = ActiveModel.UIDoc;
    selectedVoids = EDGE.__Testing.MultiVoidTools.MultiVoidTools.VerifyVoids((IEnumerable<Element>) selectedVoids).ToList<Element>();
    List<Element> solidVoids;
    List<Element> voidVoids;
    EDGE.__Testing.MultiVoidTools.MultiVoidTools.SplitVoidList((IEnumerable<Element>) selectedVoids, out solidVoids, out voidVoids);
    EDGE.__Testing.MultiVoidTools.MultiVoidTools.ExecuteCut(solidVoids, voidVoids);
  }

  private static void ExecuteCut(List<Element> allSolidVoids, List<Element> allVoidVoids)
  {
    List<Element> list1 = allSolidVoids.ToList<Element>();
    List<Element> list2 = allVoidVoids.ToList<Element>();
    using (Transaction transaction = new Transaction(ActiveModel.Document, "Multi Void Cutting"))
    {
      while (true)
      {
        Element elem1 = EDGE.__Testing.MultiVoidTools.MultiVoidTools.SelectElement(true);
        int num1 = (int) transaction.Start();
        List<Element> list3 = new List<Element>()
        {
          StructuralFraming.RefineNestedFamily(elem1)
        }.Concat<Element>(elem1.GetSubComponentIds().Select<ElementId, Element>(new Func<ElementId, Element>(ActiveModel.Document.GetElement)).Where<Element>((Func<Element, bool>) (elem => elem.Name.Contains("INSULATION") || elem.Name.Contains("GROUT")))).ToList<Element>();
        BoundingBoxXYZ boundingBoxXyz = elem1.get_BoundingBox((View) null);
        Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
        LogicalOrFilter filter = new LogicalOrFilter((ElementFilter) new BoundingBoxIsInsideFilter(outline), (ElementFilter) new BoundingBoxIntersectsFilter(outline));
        if (allSolidVoids.Any<Element>())
          list1 = new FilteredElementCollector(ActiveModel.Document, (ICollection<ElementId>) allSolidVoids.Select<Element, ElementId>((Func<Element, ElementId>) (elem => elem.Id)).ToList<ElementId>()).WherePasses((ElementFilter) filter).ToElements().ToList<Element>();
        if (allVoidVoids.Any<Element>())
          list2 = new FilteredElementCollector(ActiveModel.Document, (ICollection<ElementId>) allVoidVoids.Select<Element, ElementId>((Func<Element, ElementId>) (elem => elem.Id)).ToList<ElementId>()).WherePasses((ElementFilter) filter).ToElements().ToList<Element>();
        List<string> source = new List<string>();
        foreach (Element element in list3)
        {
          ElementIntersectsElementFilter intersectsElementFilter = new ElementIntersectsElementFilter(element);
          foreach (Element cuttingSolid in list1)
          {
            try
            {
              SolidSolidCutUtils.AddCutBetweenSolids(ActiveModel.Document, element, cuttingSolid);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
            {
              source.Add(cuttingSolid.Id.IntegerValue.ToString());
            }
          }
          foreach (Element cuttingInstance in list2)
          {
            try
            {
              InstanceVoidCutUtils.AddInstanceVoidCut(ActiveModel.Document, element, cuttingInstance);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
            {
              source.Add(cuttingInstance.Id.IntegerValue.ToString());
            }
          }
        }
        if (source.Any<string>())
          source.Aggregate<string, string>("", (Func<string, string, string>) ((current, i) => $"{current}{i};"));
        int num2 = (int) transaction.Commit();
      }
    }
  }

  public static void UncutElements(List<Element> selectedVoids, List<ElementId> allVoids)
  {
    EDGE.__Testing.MultiVoidTools.MultiVoidTools._voidIds = allVoids;
    EDGE.__Testing.MultiVoidTools.MultiVoidTools._previouslySelected = new HashSet<ElementId>();
    EDGE.__Testing.MultiVoidTools.MultiVoidTools._uidoc = ActiveModel.UIDoc;
    selectedVoids = EDGE.__Testing.MultiVoidTools.MultiVoidTools.VerifyVoids((IEnumerable<Element>) selectedVoids).ToList<Element>();
    List<Element> solidVoids;
    List<Element> voidVoids;
    EDGE.__Testing.MultiVoidTools.MultiVoidTools.SplitVoidList((IEnumerable<Element>) selectedVoids, out solidVoids, out voidVoids);
    EDGE.__Testing.MultiVoidTools.MultiVoidTools.ExecuteUncut(solidVoids, voidVoids);
  }

  private static void ExecuteUncut(List<Element> allSolidVoids, List<Element> allVoidVoids)
  {
    List<Element> list1 = allSolidVoids.ToList<Element>();
    List<Element> list2 = allVoidVoids.ToList<Element>();
    using (Transaction transaction = new Transaction(ActiveModel.Document, "Multi Void Uncutting"))
    {
      while (true)
      {
        Element elem1 = EDGE.__Testing.MultiVoidTools.MultiVoidTools.SelectElement(true);
        int num1 = (int) transaction.Start();
        List<Element> list3 = new List<Element>()
        {
          StructuralFraming.RefineNestedFamily(elem1)
        }.Concat<Element>(elem1.GetSubComponentIds().Select<ElementId, Element>(new Func<ElementId, Element>(ActiveModel.Document.GetElement)).Where<Element>((Func<Element, bool>) (elem => elem.Name.Contains("INSULATION") || elem.Name.Contains("GROUT")))).ToList<Element>();
        BoundingBoxXYZ boundingBoxXyz = elem1.get_BoundingBox((View) null);
        Outline outline = new Outline(boundingBoxXyz.Min, boundingBoxXyz.Max);
        LogicalOrFilter filter = new LogicalOrFilter((ElementFilter) new BoundingBoxIsInsideFilter(outline), (ElementFilter) new BoundingBoxIntersectsFilter(outline));
        if (allSolidVoids.Any<Element>())
          list1 = new FilteredElementCollector(ActiveModel.Document, (ICollection<ElementId>) allSolidVoids.Select<Element, ElementId>((Func<Element, ElementId>) (elem => elem.Id)).ToList<ElementId>()).WherePasses((ElementFilter) filter).ToElements().ToList<Element>();
        if (allVoidVoids.Any<Element>())
          list2 = new FilteredElementCollector(ActiveModel.Document, (ICollection<ElementId>) allVoidVoids.Select<Element, ElementId>((Func<Element, ElementId>) (elem => elem.Id)).ToList<ElementId>()).WherePasses((ElementFilter) filter).ToElements().ToList<Element>();
        foreach (Element second in list1)
        {
          foreach (Element first in list3)
          {
            if (SolidSolidCutUtils.CutExistsBetweenElements(first, second, out bool _))
              SolidSolidCutUtils.RemoveCutBetweenSolids(ActiveModel.Document, first, second);
          }
        }
        foreach (Element cuttingInstance in list2)
        {
          ICollection<ElementId> elementsBeingCut = InstanceVoidCutUtils.GetElementsBeingCut(cuttingInstance);
          foreach (Element element in list3)
          {
            if (elementsBeingCut.Contains(element.Id))
              InstanceVoidCutUtils.RemoveInstanceVoidCut(ActiveModel.Document, element, cuttingInstance);
          }
        }
        int num2 = (int) transaction.Commit();
      }
    }
  }

  private static void SplitVoidList(
    IEnumerable<Element> voids,
    out List<Element> solidVoids,
    out List<Element> voidVoids)
  {
    solidVoids = new List<Element>();
    voidVoids = new List<Element>();
    foreach (Element elem in voids)
    {
      Solid solid = Geometry.GetSolid(elem);
      if (ActiveModel.Document.GetElement(solid.GraphicsStyleId) == null)
      {
        if (elem.HasSuperComponent())
          solidVoids.Add(elem.GetSuperComponent());
        else
          solidVoids.Add(elem);
      }
      else if (elem.HasSuperComponent())
        voidVoids.Add(elem.GetSuperComponent());
      else
        voidVoids.Add(elem);
    }
  }

  private static IEnumerable<Element> VerifyVoids(IEnumerable<Element> voids)
  {
    return voids.SelectMany<Element, ElementId>((Func<Element, IEnumerable<ElementId>>) (elem => new List<ElementId>()
    {
      elem.Id
    }.Concat<ElementId>(elem.GetSubComponentIds()))).Select<ElementId, Element>(new Func<ElementId, Element>(ActiveModel.Document.GetElement)).Where<Element>((Func<Element, bool>) (voidElem => EDGE.__Testing.MultiVoidTools.MultiVoidTools._voidIds.Contains(voidElem.Id)));
  }

  private static Element SelectElement(bool isCut)
  {
    Element element;
    while (true)
    {
      ElementId elementId;
      do
      {
        elementId = EDGE.__Testing.MultiVoidTools.MultiVoidTools._uidoc.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new NoVoids(EDGE.__Testing.MultiVoidTools.MultiVoidTools._voidIds), "Select the next Element to " + (isCut ? "Cut." : "Uncut.")).ElementId;
        element = elementId.GetElement();
      }
      while (EDGE.__Testing.MultiVoidTools.MultiVoidTools._previouslySelected.Contains(elementId));
      if (element is AssemblyInstance)
        TaskDialog.Show("Error", "Error: The selected Element is an Assembly. Assemblies cannot be modified using this utility. Please select a different Element.");
      else
        break;
    }
    return element;
  }
}
