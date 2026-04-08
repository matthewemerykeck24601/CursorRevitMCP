// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.FindReferringViews.Models.LegendModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.FindReferringViews.ViewModels;
using System.Collections.Generic;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.FindReferringViews.Models;

internal class LegendModel
{
  private bool? isChecked;

  public List<ViewSheetModel> AssociatedViewSheets { get; set; }

  public View ViewSheet { get; set; }

  public static List<LegendModel> LegendList { get; set; }

  public View Legend { get; set; }

  public string Name { get; set; }

  public bool? IsChecked
  {
    get => this.isChecked;
    set => this.SetIsChecked(value);
  }

  public LegendModel()
  {
    this.AssociatedViewSheets = new List<ViewSheetModel>();
    LegendModel.LegendList = new List<LegendModel>();
    this.Name = "Legends";
  }

  public LegendModel(View legend)
  {
    Document document = ActiveModel.UIDoc.Document;
    this.Name = legend.Name;
    this.Legend = legend;
    this.AssociatedViewSheets = new List<ViewSheetModel>();
    this.isChecked = new bool?(false);
  }

  public LegendModel(Autodesk.Revit.DB.ViewSheet sheet)
  {
    Document document = ActiveModel.UIDoc.Document;
    this.Name = $"{sheet.SheetNumber} - {sheet.Name}";
    this.ViewSheet = (View) sheet;
  }

  private void SetIsChecked(bool? value)
  {
    bool? nullable1 = value;
    bool? isChecked = this.isChecked;
    if (nullable1.GetValueOrDefault() == isChecked.GetValueOrDefault() & nullable1.HasValue == isChecked.HasValue)
      return;
    bool? nullable2 = value;
    bool flag = false;
    if (nullable2.GetValueOrDefault() == flag & nullable2.HasValue)
    {
      this.isChecked = new bool?(false);
      if (!MainViewModel.CheckedLegends.Contains(this))
        return;
      MainViewModel.CheckedLegends.Remove(this);
    }
    else
    {
      if (!value.GetValueOrDefault())
        return;
      this.isChecked = new bool?(true);
      if (MainViewModel.CheckedLegends.Contains(this))
        return;
      MainViewModel.CheckedLegends.Add(this);
    }
  }
}
