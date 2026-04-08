// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.MVEEnhancedDetails
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class MVEEnhancedDetails
{
  private ElementId _standardId;
  private ElementId _idOfElement;
  private Dictionary<string, List<string>> _parametersAndValues;

  public ElementId StandardId
  {
    get => this._standardId;
    set => this._standardId = value;
  }

  public ElementId IdOfElement
  {
    get => this._idOfElement;
    set => this._idOfElement = value;
  }

  public Dictionary<string, List<string>> ParametersAndValues
  {
    get => this._parametersAndValues;
    set => this._parametersAndValues = value;
  }
}
