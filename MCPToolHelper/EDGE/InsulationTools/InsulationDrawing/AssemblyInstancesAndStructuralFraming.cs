// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.AssemblyInstancesAndStructuralFraming
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public class AssemblyInstancesAndStructuralFraming : ISelectionFilter
{
  private Dictionary<ElementId, List<ElementId>> elementIdsDictionary = new Dictionary<ElementId, List<ElementId>>();
  private List<ViewSheet> viewSheets = new List<ViewSheet>();
  private List<Element> TitleBlocks = new List<Element>();
  private Dictionary<ElementId, bool> passDictionary = new Dictionary<ElementId, bool>();

  public bool AllowElement(Element element)
  {
    AssemblyInstance assemblyInstance = element as AssemblyInstance;
    if (assemblyInstance == null)
      return false;
    if (this.passDictionary.ContainsKey(assemblyInstance.Id))
      return this.passDictionary[assemblyInstance.Id];
    this.passDictionary.Add(assemblyInstance.Id, false);
    if (Parameters.GetParameterAsBool((Element) assemblyInstance, "HARDWARE_DETAIL"))
      return false;
    List<ElementId> elementIdList1 = new List<ElementId>();
    List<ElementId> elementIdList2;
    if (this.elementIdsDictionary.ContainsKey(assemblyInstance.Id))
    {
      elementIdList2 = this.elementIdsDictionary[assemblyInstance.Id];
    }
    else
    {
      elementIdList2 = assemblyInstance.GetMemberIds().ToList<ElementId>();
      this.elementIdsDictionary.Add(assemblyInstance.Id, elementIdList2);
    }
    if (!new FilteredElementCollector(assemblyInstance.Document, (ICollection<ElementId>) elementIdList2).OfCategory(BuiltInCategory.OST_StructuralFraming).Any<Element>())
      return false;
    if (this.viewSheets.Count == 0)
      this.viewSheets = new FilteredElementCollector(assemblyInstance.Document).OfCategory(BuiltInCategory.OST_Sheets).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().ToList<ViewSheet>();
    List<ViewSheet> list = this.viewSheets.Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId.Equals((object) assemblyInstance.Id))).ToList<ViewSheet>();
    if (list.Count == 0)
      return false;
    if (this.TitleBlocks.Count == 0)
      this.TitleBlocks = new FilteredElementCollector(assemblyInstance.Document).OfCategory(BuiltInCategory.OST_TitleBlocks).ToList<Element>();
    bool flag = false;
    foreach (ViewSheet viewSheet in list)
    {
      foreach (Element titleBlock in this.TitleBlocks)
      {
        if (titleBlock.OwnerViewId == viewSheet.Id)
        {
          flag = true;
          break;
        }
      }
      if (flag)
        break;
    }
    if (!flag)
      return false;
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    FilteredElementCollector elementCollector = new FilteredElementCollector(assemblyInstance.Document, (ICollection<ElementId>) elementIdList2);
    if (!new FilteredElementCollector(assemblyInstance.Document, (ICollection<ElementId>) elementIdList2).WherePasses((ElementFilter) filter).OfType<FamilyInstance>().Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (x => InsulationDrawingUtils.ValidForInsulationDrawing((Element) x) && InsulationDrawingUtils.InsulationLocked((Element) x))).Any<FamilyInstance>())
      return false;
    this.passDictionary[assemblyInstance.Id] = true;
    return true;
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
