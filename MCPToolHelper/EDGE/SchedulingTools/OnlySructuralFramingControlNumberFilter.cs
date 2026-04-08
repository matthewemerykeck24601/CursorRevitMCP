// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.OnlySructuralFramingControlNumberFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.SchedulingTools;

public class OnlySructuralFramingControlNumberFilter : ISelectionFilter
{
  private List<ElementId> selectedIds = new List<ElementId>();

  public OnlySructuralFramingControlNumberFilter()
  {
  }

  public OnlySructuralFramingControlNumberFilter(ICollection<ElementId> alreadySelectedIds)
  {
    this.selectedIds = alreadySelectedIds.ToList<ElementId>();
  }

  public bool AllowElement(Element element)
  {
    return !this.selectedIds.Contains(element.Id) && element.Category != null && element.Category.Id.IntegerValue == -2001320;
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
