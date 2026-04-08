// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.MemberDetails
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class MemberDetails
{
  private string _elemId;
  private string _controlMark;
  private string _controlNumber;

  public MemberDetails(string id, string mk, string cn)
  {
    this.ElementId = id;
    this.ControlMark = mk;
    this.ControlNumber = cn;
  }

  public string ElementId
  {
    get => this._elemId;
    set => this._elemId = value;
  }

  public string ControlMark
  {
    get => this._controlMark;
    set => this._controlMark = value;
  }

  public string ControlNumber
  {
    get => this._controlNumber;
    set => this._controlNumber = value;
  }
}
