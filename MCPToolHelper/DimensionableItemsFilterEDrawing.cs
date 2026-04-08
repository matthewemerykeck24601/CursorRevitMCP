// Decompiled with JetBrains decompiler
// Type: DimensionableItemsFilterEDrawing
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using EDGE.TicketTools.AutoDimensioning;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
public class DimensionableItemsFilterEDrawing : ISelectionFilter
{
  public bool AllowElement(Autodesk.Revit.DB.Element element)
  {
    if (element.Category == null)
      return false;
    Autodesk.Revit.DB.ElementId id1 = element.Category.Id;
    List<Autodesk.Revit.DB.ElementId> list = new List<Autodesk.Revit.DB.BuiltInCategory>()
    {
      Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel,
      Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment,
      Autodesk.Revit.DB.BuiltInCategory.OST_StructConnections
    }.Select<Autodesk.Revit.DB.BuiltInCategory, Autodesk.Revit.DB.ElementId>((Func<Autodesk.Revit.DB.BuiltInCategory, Autodesk.Revit.DB.ElementId>) (id => new Autodesk.Revit.DB.ElementId(id))).ToList<Autodesk.Revit.DB.ElementId>();
    return id1.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel)) && Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID") && HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(element as Autodesk.Revit.DB.FamilyInstance) || id1.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming)) && !Parameters.GetParameterAsString(element, "CONSTRUCTION_PRODUCT").Equals("") || list.Contains(id1) && !Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID") && !Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().Contains("RAW") && !Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().Contains("CONSUMABLE") && !(element as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName.Contains("CONNECTOR") && !(element as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName.Contains("COMPONENT") && HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(element as Autodesk.Revit.DB.FamilyInstance);
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
