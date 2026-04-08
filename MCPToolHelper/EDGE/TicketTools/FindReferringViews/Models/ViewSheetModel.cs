// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.FindReferringViews.Models.ViewSheetModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.TicketTools.FindReferringViews.Models;

internal class ViewSheetModel
{
  public View ViewSheet { get; set; }

  public LegendModel ParentLegend { get; set; }

  public AnnotationSymbol ParentSymbol { get; set; }

  public string Name { get; set; }

  public ViewSheetModel()
  {
  }

  public ViewSheetModel(Autodesk.Revit.DB.ViewSheet viewSheet, LegendModel legend)
  {
    this.ViewSheet = (View) viewSheet;
    this.Name = $"{viewSheet.SheetNumber}-{viewSheet.Name}";
    this.ParentLegend = legend;
  }

  public ViewSheetModel(Autodesk.Revit.DB.ViewSheet viewSheet, AnnotationSymbol symbol)
  {
    this.ViewSheet = (View) viewSheet;
    this.Name = $"{viewSheet.SheetNumber}-{viewSheet.Name}";
    this.ParentSymbol = symbol;
  }
}
