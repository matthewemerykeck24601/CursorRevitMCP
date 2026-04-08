// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.AssemblyInstancesAndStructuralFraming
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools;

public class AssemblyInstancesAndStructuralFraming : ISelectionFilter
{
  public bool AllowElement(Element element)
  {
    return element is AssemblyInstance && !Utils.ElementUtils.Parameters.GetParameterAsBool(element, "HARDWARE_DETAIL") && (element as AssemblyInstance).GetStructuralFramingElement().GetTopLevelElement() != null;
  }

  public bool AllowReference(Reference refer, XYZ point) => false;
}
