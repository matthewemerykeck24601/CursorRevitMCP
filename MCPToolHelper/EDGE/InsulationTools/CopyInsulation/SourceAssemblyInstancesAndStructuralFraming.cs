// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.CopyInsulation.SourceAssemblyInstancesAndStructuralFraming
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.InsulationTools.CopyInsulation;

public class SourceAssemblyInstancesAndStructuralFraming : ISelectionFilter
{
  public bool AllowElement(Element element)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    try
    {
      if (element is AssemblyInstance assembly)
      {
        if (!TargetAssemblyInstancesAndStructuralFraming.IsValidStructuralFramingAssembly(assembly))
          return false;
        List<ElementId> list = assembly.GetMemberIds().ToList<ElementId>();
        return new FilteredElementCollector(assembly.Document, (ICollection<ElementId>) list).WherePasses((ElementFilter) filter).OfType<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (x => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent((Element) x))).Count<FamilyInstance>() != 0;
      }
      if (element.Category == null || element.Category.Id.IntegerValue != -2001320 || !TargetAssemblyInstancesAndStructuralFraming.IsValidStructuralFramingAssembly(element.AssemblyInstanceId.GetElement() as AssemblyInstance) || element.AssemblyInstanceId == ElementId.InvalidElementId)
        return false;
      AssemblyInstance element1 = element.AssemblyInstanceId.GetElement() as AssemblyInstance;
      List<ElementId> list1 = element1.GetMemberIds().ToList<ElementId>();
      return new FilteredElementCollector(element1.Document, (ICollection<ElementId>) list1).WherePasses((ElementFilter) filter).OfType<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (x => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent((Element) x))).Count<FamilyInstance>() != 0;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
