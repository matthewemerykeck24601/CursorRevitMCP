// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.BrowserViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.Resources;
using System.ComponentModel;
using System.Diagnostics;

#nullable disable
namespace EDGE.EDGEBrowser;

internal class BrowserViewModel : INotifyPropertyChanged
{
  public string FilePath;
  public string FileName;
  public string txtFileName;
  public string txtFilePath;

  public ExternalEvent FamilyLoadEvent { get; set; }

  public ExternalEvent FamilyDownloadEvent { get; set; }

  public Command FamilyLoadCommand { get; set; }

  public Command FamilyDownloadCommand { get; set; }

  public bool CanExecuteFamilyLoad(object parameter) => true;

  public void ExecuteFamilyLoad(object parameter) => this.FamilyLoadEvent.Raise();

  public event PropertyChangedEventHandler PropertyChanged;

  [DebuggerHidden]
  protected void OnPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }

  public BrowserViewModel(string fPath, string fName, string txtfPath, string txtfName)
  {
    this.FamilyLoadCommand = new Command(new Command.ICommandOnExecute(this.ExecuteFamilyLoad), new Command.ICommandOnCanExecute(this.CanExecuteFamilyLoad));
    this.FileName = fName;
    this.FilePath = fPath;
    this.txtFilePath = txtfPath;
    this.txtFileName = txtfName;
  }
}
