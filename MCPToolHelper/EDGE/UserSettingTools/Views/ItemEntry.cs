// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.ItemEntry
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class ItemEntry : INotifyPropertyChanged
{
  public string templateSchedule { set; get; }

  public string scheduleSuffix { set; get; }

  public ItemEntry(string template, string suffix)
  {
    this.templateSchedule = template;
    this.scheduleSuffix = suffix;
  }

  public ItemEntry()
  {
    this.templateSchedule = "";
    this.scheduleSuffix = "";
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
