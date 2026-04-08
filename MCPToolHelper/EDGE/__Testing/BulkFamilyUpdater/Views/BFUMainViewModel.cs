// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.Views.BFUMainViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater.Views;

internal class BFUMainViewModel : INotifyPropertyChanged
{
  private ObservableCollection<FamilyUpdaterTask> _tasks = new ObservableCollection<FamilyUpdaterTask>();
  private FamilyUpdaterTask _selectedTask;
  private bool? _EDGEDimLines = new bool?(false);

  public ObservableCollection<FamilyUpdaterTask> Tasks
  {
    get => this._tasks;
    set
    {
      if (value == this._tasks)
        return;
      this._tasks = value;
      this.OnPropertyChanged(nameof (Tasks));
    }
  }

  public FamilyUpdaterTask SelectedTask
  {
    get => this._selectedTask;
    set
    {
      if (value == this._selectedTask)
        return;
      this._selectedTask = value;
      this.OnPropertyChanged(nameof (SelectedTask));
    }
  }

  public bool? EDGEDimLines
  {
    get => this._EDGEDimLines;
    set
    {
      bool? nullable = value;
      bool? edgeDimLines = this._EDGEDimLines;
      if (nullable.GetValueOrDefault() == edgeDimLines.GetValueOrDefault() & nullable.HasValue == edgeDimLines.HasValue)
        return;
      this._EDGEDimLines = value;
      this.OnPropertyChanged(nameof (EDGEDimLines));
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  [DebuggerHidden]
  protected void OnPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }
}
