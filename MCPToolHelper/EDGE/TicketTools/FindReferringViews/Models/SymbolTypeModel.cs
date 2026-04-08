// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.FindReferringViews.Models.SymbolTypeModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.FindReferringViews.Models;

internal class SymbolTypeModel : INotifyPropertyChanged
{
  public SymbolFamilyModel Parent;
  private bool? isChecked;

  public string Name { get; set; }

  public string Workset { get; set; }

  public List<ViewSheetModel> AssociatedViewSheets { get; set; }

  public bool? IsChecked
  {
    get => this.isChecked;
    set => this.SetIsChecked(value);
  }

  public SymbolTypeModel()
  {
  }

  public SymbolTypeModel(FamilySymbol symbol, SymbolFamilyModel parent)
  {
    this.Name = symbol.Name;
    this.AssociatedViewSheets = new List<ViewSheetModel>();
    this.isChecked = new bool?(false);
    this.Parent = parent;
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
      if (this.Parent.CheckedSymbols.Contains(this))
        this.Parent.CheckedSymbols.Remove(this);
    }
    else if (value.GetValueOrDefault())
    {
      this.isChecked = new bool?(true);
      if (!this.Parent.CheckedSymbols.Contains(this))
        this.Parent.CheckedSymbols.Add(this);
    }
    this.Parent.VerifyCheckState();
    this.OnPropertyChanged("IsChecked");
  }

  public event PropertyChangedEventHandler PropertyChanged;

  protected void OnPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
