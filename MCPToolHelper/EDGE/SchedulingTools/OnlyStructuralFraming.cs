// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.OnlyStructuralFraming
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.SchedulingTools;

internal class OnlyStructuralFraming : ISelectionFilter
{
  private static UIDocument uidoc = ActiveModel.UIDoc;
  private static Document doc = OnlyStructuralFraming.uidoc.Document;

  public bool AllowElement(Element element)
  {
    List<ElementId> elementIdList = new List<ElementId>();
    ElementId elementId = new ElementId(BuiltInCategory.OST_StructuralFraming);
    return element.Category != null && element.Category.Id.Equals((object) elementId);
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
