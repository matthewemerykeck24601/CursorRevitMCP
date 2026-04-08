// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.HighlightEvent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

internal class HighlightEvent : INotifyPropertyChanged
{
  private List<ElementId> _listOfIds;
  private ElementId _directShape;
  public bool isolate = true;

  public ExternalEvent HighlightEvents { get; set; }

  public Command HighlightCommand { get; set; }

  public List<ElementId> ListOfIds
  {
    get => this._listOfIds;
    set => this._listOfIds = value;
  }

  public ElementId directShape
  {
    get => this._directShape;
    set => this._directShape = value;
  }

  public MKExistingDetails detailWindow { get; set; }

  public MKVerificationResults_Existing overallResults { get; set; }

  public bool CanExecuteHighlightEvent(object parameter) => true;

  public void ExecuteHighlightEvent(object parameter) => this.HighlightEvents.Raise();

  public event PropertyChangedEventHandler PropertyChanged;

  [DebuggerHidden]
  protected void OnPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }

  public HighlightEvent(UIDocument uiDoc)
  {
    this.HighlightCommand = new Command(new Command.ICommandOnExecute(this.ExecuteHighlightEvent), new Command.ICommandOnCanExecute(this.CanExecuteHighlightEvent));
  }
}
