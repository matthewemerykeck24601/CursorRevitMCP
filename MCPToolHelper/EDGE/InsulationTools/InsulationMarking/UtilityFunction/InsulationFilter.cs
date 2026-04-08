// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationMarking.UtilityFunction.InsulationFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationMarking.UtilityFunction;

public class InsulationFilter : ISelectionFilter
{
  public bool AllowElement(Element element)
  {
    ElementId id = element.Category.Id;
    return (id.Equals((object) new ElementId(BuiltInCategory.OST_SpecialityEquipment)) || id.Equals((object) new ElementId(BuiltInCategory.OST_GenericModel))) && Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").Contains("INSULATION");
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
