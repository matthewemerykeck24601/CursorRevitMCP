// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.FindReferringViews.Models.SymbolFamilyModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.FindReferringViews.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.FindReferringViews.Models;

internal class SymbolFamilyModel : INotifyPropertyChanged
{
  private bool? isChecked;

  public static List<SymbolFamilyModel> SymbolFamilyList { get; set; }

  public List<SymbolTypeModel> SymbolTypeList { get; set; }

  public List<SymbolTypeModel> CheckedSymbols { get; set; }

  public string Name { get; set; }

  public bool? IsChecked
  {
    get => this.isChecked;
    set => this.SetIsChecked(value, true);
  }

  public SymbolFamilyModel()
  {
  }

  public SymbolFamilyModel(string familyName)
  {
    this.Name = familyName;
    this.SymbolTypeList = new List<SymbolTypeModel>();
    this.isChecked = new bool?(false);
    this.CheckedSymbols = new List<SymbolTypeModel>();
  }

  private void SetIsChecked(bool? value, bool updatechildren)
  {
    bool? nullable = value;
    bool? isChecked = this.isChecked;
    if (nullable.GetValueOrDefault() == isChecked.GetValueOrDefault() & nullable.HasValue == isChecked.HasValue)
      return;
    this.isChecked = value;
    if (!(value.HasValue & updatechildren))
      return;
    foreach (SymbolTypeModel symbolType in this.SymbolTypeList)
      symbolType.IsChecked = value;
  }

  public void VerifyCheckState()
  {
    bool? nullable1 = new bool?();
    for (int index = 0; index < this.SymbolTypeList.Count; ++index)
    {
      bool? isChecked = this.SymbolTypeList.ElementAt<SymbolTypeModel>(index).IsChecked;
      if (index == 0)
        nullable1 = isChecked;
      bool? nullable2 = nullable1;
      bool? nullable3 = isChecked;
      if (!(nullable2.GetValueOrDefault() == nullable3.GetValueOrDefault() & nullable2.HasValue == nullable3.HasValue))
      {
        nullable1 = new bool?();
        break;
      }
    }
    if (this.CheckedSymbols.Count == 0)
    {
      nullable1 = new bool?(false);
      if (MainViewModel.CheckedSymbols.Contains(this))
        MainViewModel.CheckedSymbols.Remove(this);
    }
    else if (!MainViewModel.CheckedSymbols.Contains(this))
      MainViewModel.CheckedSymbols.Add(this);
    this.SetIsChecked(nullable1, false);
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
