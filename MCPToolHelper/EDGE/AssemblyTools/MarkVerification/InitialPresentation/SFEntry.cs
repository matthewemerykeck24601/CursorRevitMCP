// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.InitialPresentation.SFEntry
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.ComponentModel;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.InitialPresentation;

public class SFEntry : INotifyPropertyChanged
{
  private bool isselected;

  public UIDocument uidoc { set; get; }

  public ElementId elemid { set; get; }

  public string elemName { set; get; }

  public string mkNumber { set; get; }

  public bool IsSelected
  {
    get => this.isselected;
    set
    {
      this.isselected = value;
      this.NotifyPropertyChanged(nameof (IsSelected));
    }
  }

  public SFEntry(ElementId id, string Name, bool condition, UIDocument uidocs)
  {
    this.elemid = id;
    this.elemName = Name;
    this.IsSelected = condition;
    this.uidoc = uidocs;
    this.mkNumber = Parameters.GetParameterAsString(this.uidoc.Document.GetElement(id), "CONTROL_MARK");
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
