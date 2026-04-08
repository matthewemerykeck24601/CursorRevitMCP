// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.AssemblyFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

#nullable disable
namespace EDGE.AdminTools.CAM;

public class AssemblyFilter : ISelectionFilter
{
  public bool AllowElement(Element element) => element.Category.Name == "Mass";

  public bool AllowReference(Reference refer, XYZ point) => false;
}
