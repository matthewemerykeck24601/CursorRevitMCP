// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.PinPlacement.InsulationFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

#nullable disable
namespace EDGE.InsulationTools.PinPlacement;

public class InsulationFilter : ISelectionFilter
{
  public bool AllowElement(Element e) => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(e);

  public bool AllowReference(Reference refer, XYZ point) => false;
}
