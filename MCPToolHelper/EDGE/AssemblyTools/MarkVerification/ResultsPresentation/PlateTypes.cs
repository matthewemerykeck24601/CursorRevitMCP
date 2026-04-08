// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.PlateTypes
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class PlateTypes
{
  private string _message;
  private string _familyName;
  private int _actualCount;
  private int _familyCount;
  private List<ElementId> _actualList;
  private List<ElementId> _expectedList;

  public PlateTypes(
    string familyName,
    int actualCount,
    int familyCount,
    string message,
    List<ElementId> actualList,
    List<ElementId> expectedList)
  {
    this._familyName = familyName;
    this._familyCount = familyCount;
    this._actualCount = actualCount;
    this._message = message;
    this._actualList = actualList;
    this._expectedList = expectedList;
  }

  public string Message
  {
    get => this._message;
    set => this._message = value;
  }

  public string FamilyName
  {
    get => this._familyName;
    set => this._familyName = value;
  }

  public int ActualCount
  {
    get => this._actualCount;
    set => this._actualCount = value;
  }

  public int FamilyCount
  {
    get => this._familyCount;
    set => this._familyCount = value;
  }

  public List<ElementId> ActualList
  {
    get => this._actualList;
    set => this._actualList = value;
  }

  public List<ElementId> ExpectedList
  {
    get => this._expectedList;
    set => this._expectedList = value;
  }
}
