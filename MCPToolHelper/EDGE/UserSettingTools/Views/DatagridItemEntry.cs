// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.DatagridItemEntry
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class DatagridItemEntry : INotifyPropertyChanged
{
  public string mappingFromParam { set; get; }

  public string mappingToParam { set; get; }

  public bool canDeleteParam { set; get; }

  public DatagridItemEntry(string from, string to, bool canDelete)
  {
    this.mappingFromParam = from;
    this.mappingToParam = to;
    this.canDeleteParam = canDelete;
  }

  public DatagridItemEntry()
  {
    this.mappingFromParam = "";
    this.mappingToParam = "";
    this.canDeleteParam = true;
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
