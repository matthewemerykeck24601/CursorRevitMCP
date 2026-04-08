// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.WarningMessage
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.TicketTools.CopyTicketAnnotation;

public class WarningMessage : INotifyPropertyChanged
{
  public string message { set; get; }

  public WarningMessage()
  {
  }

  public WarningMessage(string information) => this.message = information;

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string v)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(v));
  }
}
