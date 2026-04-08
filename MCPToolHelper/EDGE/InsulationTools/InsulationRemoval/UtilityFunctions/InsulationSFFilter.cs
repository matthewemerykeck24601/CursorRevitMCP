// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulationSFFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationRemoval.UtilityFunctions;

public class InsulationSFFilter : ISelectionFilter
{
  public bool AllowElement(Element element)
  {
    return element.Category.Id.Equals((object) new ElementId(BuiltInCategory.OST_StructuralFraming)) && Utils.SelectionUtils.SelectionUtils.CheckConstructionProduct(element) && Parameters.LookupParameter(element, "INSULATION_INCLUDED") != null;
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
