// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.PinPlacement.PinPlacementChoice
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.InsulationTools.PinPlacement;

public class PinPlacementChoice
{
  public static List<Element> WholeModel(Document revitDoc)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> elementList = new List<Element>();
    return new FilteredElementCollector(revitDoc).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(e))).ToList<Element>();
  }

  public static List<Element> ActiveView(Document revitDoc, UIDocument uidoc)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> elementList = new List<Element>();
    return new FilteredElementCollector(revitDoc, uidoc.ActiveView.Id).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(e))).ToList<Element>();
  }

  public static List<Element> SelectedElements(
    Document revitDoc,
    ICollection<ElementId> preSelected,
    Autodesk.Revit.UI.Selection.Selection sel,
    bool dirMarkTool)
  {
    List<Element> elementList = new List<Element>();
    if (preSelected.Count == 0)
    {
      try
      {
        InsulationFilter insulationFilter = new InsulationFilter();
        preSelected = (ICollection<ElementId>) sel.PickObjects((ObjectType) 1, (ISelectionFilter) insulationFilter, "Please select insulation elements for processing").ToList<Reference>().Select<Reference, ElementId>((Func<Reference, ElementId>) (r => r.ElementId)).ToList<ElementId>();
      }
      catch
      {
        return (List<Element>) null;
      }
    }
    if (preSelected.Count == 0)
    {
      TaskDialog taskDialog = new TaskDialog("Warning");
      if (dirMarkTool)
      {
        taskDialog.Title = "Directional Marker Placement - Warning";
        taskDialog.MainInstruction = "There are no elements in the active view to process for Directional Marker Placement.";
      }
      else
      {
        taskDialog.Title = "Automatic Pin Placement - Warning";
        taskDialog.MainInstruction = "There are no elements in the active view to process for Automatic Pin Placement.";
      }
      taskDialog.Show();
      return (List<Element>) null;
    }
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    return new FilteredElementCollector(revitDoc, preSelected).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(e))).ToList<Element>();
  }
}
