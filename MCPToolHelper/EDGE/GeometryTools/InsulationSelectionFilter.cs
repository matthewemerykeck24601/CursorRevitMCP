// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.InsulationSelectionFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.GeometryTools;

public class InsulationSelectionFilter : ISelectionFilter
{
  public bool AllowElement(Element element)
  {
    if (element.Category == null)
      return false;
    int integerValue = element.Category.Id.IntegerValue;
    if (!integerValue.Equals(-2001350))
    {
      integerValue = element.Category.Id.IntegerValue;
      if (!integerValue.Equals(-2000151))
        return false;
    }
    return Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().ToUpper().Contains("INSULATION") && !Parameters.GetParameterAsBool(element, "HARDWARE_DETAIL");
  }

  public bool AllowReference(Reference refer, XYZ point) => false;
}
