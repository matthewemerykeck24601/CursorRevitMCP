// Decompiled with JetBrains decompiler
// Type: GridAndLevelFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

#nullable disable
internal class GridAndLevelFilter : ISelectionFilter
{
  public bool AllowElement(Autodesk.Revit.DB.Element element)
  {
    Autodesk.Revit.DB.ElementId elementId1 = new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_Levels);
    Autodesk.Revit.DB.ElementId elementId2 = new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_Grids);
    return element.Category != null && (element.Category.Id.Equals((object) elementId1) || element.Category.Id.Equals((object) elementId2));
  }

  public bool AllowReference(Reference refer, XYZ point) => false;
}
