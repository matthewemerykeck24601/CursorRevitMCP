// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class AddonLocation
{
  private string _familyName;
  private List<ElementId> _elementIds;
  private bool _addons;
  private string _warningFamily;
  private List<ElementId> _warningId;

  public AddonLocation(
    string familyName,
    List<ElementId> elementIds,
    bool addons,
    List<ElementId> warning = null)
  {
    this._familyName = familyName;
    this._elementIds = elementIds;
    this._addons = addons;
    this._warningId = warning;
  }

  public string FamilyName
  {
    get => this._familyName;
    set => this._familyName = value;
  }

  public List<ElementId> ElementIds
  {
    get => this._elementIds;
    set => this._elementIds = value;
  }

  public bool Addons
  {
    get => this._addons;
    set => this._addons = value;
  }

  public string WarningFamily
  {
    get => this._warningFamily;
    set => this._warningFamily = value;
  }

  public List<ElementId> WarningId
  {
    get => this._warningId;
    set => this._warningId = value;
  }
}
