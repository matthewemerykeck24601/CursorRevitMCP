// Decompiled with JetBrains decompiler
// Type: ValidVoids
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using EDGE.TicketTools.AutoDimensioning;
using Utils.ElementUtils;

#nullable disable
public class ValidVoids : ISelectionFilter
{
  public bool AllowElement(Autodesk.Revit.DB.Element element)
  {
    return element.Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel)) && Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID") && HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(element as Autodesk.Revit.DB.FamilyInstance);
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
