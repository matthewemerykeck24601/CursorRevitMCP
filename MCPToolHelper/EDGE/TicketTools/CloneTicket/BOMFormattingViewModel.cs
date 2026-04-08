// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.BOMFormattingViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

internal class BOMFormattingViewModel : INotifyPropertyChanged
{
  private List<sheetListItem> _sheetList = new List<sheetListItem>();

  public List<sheetListItem> SheetList
  {
    get => this._sheetList;
    set
    {
      if (this._sheetList == value)
        return;
      this._sheetList = value;
      this.NotifyPropertyChanged(nameof (SheetList));
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
