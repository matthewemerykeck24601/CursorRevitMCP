// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.WallPanelSelectionFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Utils.AssemblyUtils;

#nullable disable
namespace EDGE.GeometryTools;

public class WallPanelSelectionFilter : ISelectionFilter
{
  public bool AllowElement(Element element)
  {
    if (element.Category == null)
      return false;
    int integerValue = element.Category.Id.IntegerValue;
    if (integerValue.Equals(-2001320))
      return Utils.ElementUtils.Parameters.GetParameterAsString(element, "CONSTRUCTION_PRODUCT").ToUpper().ToUpper().Contains("WALL");
    integerValue = element.Category.Id.IntegerValue;
    return integerValue.Equals(-2000267) && Utils.ElementUtils.Parameters.GetParameterAsString((element as AssemblyInstance).GetStructuralFramingElement(), "CONSTRUCTION_PRODUCT").ToUpper().ToUpper().Contains("WALL");
  }

  public bool AllowReference(Reference refer, XYZ point) => false;
}
