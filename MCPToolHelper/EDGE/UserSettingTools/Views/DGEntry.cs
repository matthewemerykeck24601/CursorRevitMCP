// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.DGEntry
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class DGEntry : INotifyPropertyChanged
{
  public string scheduleName { set; get; }

  public DGEntry(string template) => this.scheduleName = template;

  public DGEntry() => this.scheduleName = "";

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
