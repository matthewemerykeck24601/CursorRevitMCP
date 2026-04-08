// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.ViewSheetItemEntry
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.CopyTicketAnnotation;

public class ViewSheetItemEntry : INotifyPropertyChanged
{
  private bool isselected;

  public UIDocument uidoc { set; get; }

  public ElementId elemid { set; get; }

  public string elemName { set; get; }

  public bool IsSelected
  {
    get => this.isselected;
    set
    {
      this.isselected = value;
      this.NotifyPropertyChanged(nameof (IsSelected));
    }
  }

  public ViewSheetItemEntry()
  {
  }

  public ViewSheetItemEntry(ElementId id, string Name, bool condition, UIDocument uidocs)
  {
    this.elemid = id;
    this.elemName = Name;
    this.IsSelected = condition;
    this.uidoc = uidocs;
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
