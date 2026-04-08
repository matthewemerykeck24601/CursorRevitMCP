// Decompiled with JetBrains decompiler
// Type: SameControlMarkFilterEDrawing
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using EDGE.TicketTools.AutoDimensioning;
using Utils.ElementUtils;

#nullable disable
public class SameControlMarkFilterEDrawing : ISelectionFilter
{
  private string modelControlMark = "";
  private string modelElemInstName = "";

  public SameControlMarkFilterEDrawing(Autodesk.Revit.DB.Element modelElem)
  {
    this.modelControlMark = modelElem.GetControlMark();
    if (!(modelElem is Autodesk.Revit.DB.FamilyInstance))
      return;
    this.modelElemInstName = (modelElem as Autodesk.Revit.DB.FamilyInstance).Name;
  }

  public bool AllowElement(Autodesk.Revit.DB.Element element)
  {
    if (!(element is Autodesk.Revit.DB.FamilyInstance familyInstance) || !element.Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming)) && !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(familyInstance))
      return false;
    return this.modelControlMark.Equals("") || this.modelControlMark == null ? familyInstance.Name.Equals(this.modelElemInstName) : familyInstance.GetControlMark().Equals(this.modelControlMark);
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
