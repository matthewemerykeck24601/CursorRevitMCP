// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.PinPlacement.errorMessageInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.InsulationTools.PinPlacement;

public class errorMessageInfo
{
  private Dictionary<string, List<Element>> _listOfElements;
  private FamilySymbol _familySymbol;

  public Dictionary<string, List<Element>> ListOfElements
  {
    get => this._listOfElements;
    set => this._listOfElements = value;
  }

  public FamilySymbol FamilySymbol
  {
    get => this._familySymbol;
    set => this._familySymbol = value;
  }
}
