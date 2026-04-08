// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.BulkUpdatorItemEntry
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.QATools;

public class BulkUpdatorItemEntry : INotifyPropertyChanged
{
  public string filter { set; get; }

  public string subCatName { set; get; }

  public string layerName { set; get; }

  public int layerColor { set; get; }

  public BulkUpdatorItemEntry(string filterCategory, string subCat, string layer, int color)
  {
    this.filter = filterCategory;
    this.subCatName = subCat;
    this.layerName = layer;
    this.layerColor = color;
  }

  public BulkUpdatorItemEntry()
  {
    this.filter = "";
    this.subCatName = "";
    this.layerName = "";
    this.layerColor = 0;
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
