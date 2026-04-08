// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.CopyInsulation.TargetAssemblyInstancesAndStructuralFraming
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
namespace EDGE.InsulationTools.CopyInsulation;

public class TargetAssemblyInstancesAndStructuralFraming : ISelectionFilter
{
  private AssemblyInstance assemToExclude;

  public TargetAssemblyInstancesAndStructuralFraming(AssemblyInstance assemblyToExclude)
  {
    this.assemToExclude = assemblyToExclude;
  }

  public bool AllowElement(Element element)
  {
    try
    {
      return element is AssemblyInstance assembly ? !(assembly.Id == this.assemToExclude.Id) && TargetAssemblyInstancesAndStructuralFraming.IsValidStructuralFramingAssembly(assembly) : element.Category != null && element.Category.Id.IntegerValue == -2001320 && !(this.assemToExclude.Id == element.AssemblyInstanceId) && TargetAssemblyInstancesAndStructuralFraming.IsValidStructuralFramingAssembly(element.AssemblyInstanceId.GetElement() as AssemblyInstance) && !(element.AssemblyInstanceId == ElementId.InvalidElementId);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public bool AllowReference(Reference refer, XYZ point) => true;

  public static bool IsValidStructuralFramingAssembly(AssemblyInstance assembly)
  {
    bool flag = true;
    if (assembly == null)
      return false;
    if (Parameters.GetParameterAsBool((Element) assembly, "HARDWARE_DETAIL"))
      flag = false;
    List<ElementId> list = assembly.GetMemberIds().ToList<ElementId>();
    if (!new FilteredElementCollector(assembly.Document, (ICollection<ElementId>) list).OfCategory(BuiltInCategory.OST_StructuralFraming).Any<Element>())
      flag = false;
    return flag;
  }
}
