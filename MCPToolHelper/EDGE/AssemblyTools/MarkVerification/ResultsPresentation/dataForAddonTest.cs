// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.dataForAddonTest
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class dataForAddonTest
{
  private string _message;
  private bool _addonPass;
  private List<ElementId> _mismatchedId;
  private bool _volumePass;
  private string _familyTypeName;
  private int _familyTypeCount;
  private int _actualCount;
  private List<ElementId> _listforactual;
  private List<ElementId> _listforexpected;

  public dataForAddonTest(
    List<ElementId> mismatchedId,
    string familyTypeName,
    int familycount,
    int actualcount,
    bool volumePass,
    List<ElementId> actuallist,
    List<ElementId> explist,
    bool addonPass,
    string message = null)
  {
    this._mismatchedId = mismatchedId;
    this._familyTypeName = familyTypeName;
    this._familyTypeCount = familycount;
    this._actualCount = actualcount;
    this._volumePass = volumePass;
    this._listforactual = actuallist;
    this._listforexpected = explist;
    this._addonPass = addonPass;
    this._message = message;
  }

  public string Message
  {
    get => this._message;
    set => this._message = value;
  }

  public bool AddonPass
  {
    get => this._addonPass;
    set => this._addonPass = value;
  }

  public List<ElementId> MismatchedId
  {
    get => this._mismatchedId;
    set => this._mismatchedId = value;
  }

  public bool VolumePass
  {
    get => this._volumePass;
    set => this._volumePass = value;
  }

  public string FamilyTypeName
  {
    get => this._familyTypeName;
    set => this._familyTypeName = value;
  }

  public int FamilyTypeCount
  {
    get => this._familyTypeCount;
    set => this._familyTypeCount = value;
  }

  public int ActualCount
  {
    get => this._actualCount;
    set => this._actualCount = value;
  }

  public List<ElementId> ListforActual
  {
    get => this._listforactual;
    set => this._listforactual = value;
  }

  public List<ElementId> ListforExpected
  {
    get => this._listforexpected;
    set => this._listforexpected = value;
  }
}
