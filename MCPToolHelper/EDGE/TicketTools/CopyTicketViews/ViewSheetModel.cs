// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketViews.ViewSheetModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.TicketTools.CopyTicketViews;

internal class ViewSheetModel
{
  private bool _isChecked;

  public ViewSheet ViewSheet { get; set; }

  public string Name { get; set; }

  public bool IsChecked
  {
    get => this._isChecked;
    set
    {
      if (value == this._isChecked)
        return;
      this._isChecked = value;
      if (value && !ViewModel.SelectedViewSheets.Contains(this))
        ViewModel.SelectedViewSheets.Add(this);
      if (value || !ViewModel.SelectedViewSheets.Contains(this))
        return;
      ViewModel.SelectedViewSheets.Remove(this);
    }
  }

  public ViewSheetModel()
  {
  }

  public ViewSheetModel(ViewSheet viewSheet)
  {
    this.ViewSheet = viewSheet;
    this.Name = $"{viewSheet.SheetNumber}-{viewSheet.Name}";
    this.IsChecked = false;
  }
}
