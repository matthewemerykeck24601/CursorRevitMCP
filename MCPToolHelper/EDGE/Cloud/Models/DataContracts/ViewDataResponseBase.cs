// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.ViewDataResponseBase
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.ComponentModel;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public abstract class ViewDataResponseBase
{
  [Browsable(false)]
  public ViewDataError Error { get; set; }

  public bool IsOk() => this.Error == null;
}
