// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationMarking.UtilityFunction.Marking
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationMarking.UtilityFunction;

public class Marking
{
  public static List<Element> filterforInsulatMarking(
    List<Element> selectedElements,
    out List<Element> zerovols)
  {
    List<Element> elementList1 = new List<Element>();
    List<Element> elementList2 = new List<Element>();
    List<Element> list = selectedElements.Where<Element>((Func<Element, bool>) (s => Parameters.GetParameterAsString(s, "MANUFACTURE_COMPONENT").ToUpper().Contains("INSULATION"))).ToList<Element>();
    foreach (Element elem in list)
    {
      if (elem.GetElementVolume() <= 0.0)
        elementList2.Add(elem);
    }
    foreach (Element element in elementList2)
    {
      if (list.Contains(element))
        list.Remove(element);
    }
    zerovols = elementList2;
    return list;
  }

  public static List<Element> filterforInsulatMarkingActive(
    List<Element> selectedElements,
    out List<Element> zerovols)
  {
    List<Element> elementList1 = new List<Element>();
    List<Element> elementList2 = new List<Element>();
    List<Element> list = selectedElements.Where<Element>((Func<Element, bool>) (s => Parameters.GetParameterAsString(s, "MANUFACTURE_COMPONENT").ToUpper().Contains("INSULATION"))).ToList<Element>();
    foreach (Element elem in list)
    {
      if (elem.GetElementVolume() <= 0.0)
        elementList2.Add(elem);
    }
    foreach (Element element in elementList2)
    {
      if (list.Contains(element))
        list.Remove(element);
    }
    zerovols = elementList2;
    return list;
  }

  public static List<ElementId> SelectedValidInsulations(Document revitdoc, UIDocument uiDoc)
  {
    try
    {
      List<Reference> list = uiDoc.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new InsulationFilter(), "Pick Insulations to be marked").ToList<Reference>();
      List<ElementId> elementIdList = new List<ElementId>();
      foreach (Reference reference in list)
      {
        if (revitdoc.GetElement(reference) != null)
          elementIdList.Add(revitdoc.GetElement(reference).Id);
      }
      return elementIdList;
    }
    catch
    {
      return (List<ElementId>) null;
    }
  }
}
