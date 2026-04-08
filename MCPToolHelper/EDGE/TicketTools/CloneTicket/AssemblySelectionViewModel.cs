// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.AssemblySelectionViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.ObjectModel;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

internal class AssemblySelectionViewModel : INotifyPropertyChanged
{
  private ObservableCollection<AssemblyListObject> _assemblyList = new ObservableCollection<AssemblyListObject>();

  public ObservableCollection<AssemblyListObject> AssemblyList
  {
    get => this._assemblyList;
    set
    {
      if (value.Equals((object) this._assemblyList))
        return;
      this.AssemblyList = this._assemblyList;
      this.NotifyPropertyChanged(nameof (AssemblyList));
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
