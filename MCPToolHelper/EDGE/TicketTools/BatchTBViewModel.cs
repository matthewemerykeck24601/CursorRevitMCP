// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.BatchTBViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools;

public class BatchTBViewModel : INotifyPropertyChanged
{
  private List<ParentClass> _ItemsList = new List<ParentClass>();

  public event PropertyChangedEventHandler PropertyChanged;

  public List<ParentClass> ItemsList
  {
    get => this._ItemsList;
    set
    {
      if (this._ItemsList == value)
        return;
      this._ItemsList = value;
      this.NotifyPropertyChanged(nameof (ItemsList));
      this.NotifyPropertyChanged("Sheets");
      this.NotifyPropertyChanged("SheetName");
      this.NotifyPropertyChanged("CheckedSheetBool");
      this.NotifyPropertyChanged("AssemblyName");
      this.NotifyPropertyChanged("CheckedItemBool");
      this.NotifyPropertyChanged("Members");
      this.NotifyPropertyChanged("HierarchyCheckBoxItems");
    }
  }

  public BatchTBViewModel()
  {
  }

  public BatchTBViewModel(List<ParentClass> items) => this.ItemsList = items;

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
