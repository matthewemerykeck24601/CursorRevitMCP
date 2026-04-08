// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.MVEfailedResults
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class MVEfailedResults
{
  private List<ElementId> _failedId;
  private string _warningFamily;
  private string _familyName;

  public List<ElementId> FailedId
  {
    get => this._failedId;
    set => this._failedId = value;
  }

  public string FamilyName
  {
    get => this._familyName;
    set => this._familyName = value;
  }
}
