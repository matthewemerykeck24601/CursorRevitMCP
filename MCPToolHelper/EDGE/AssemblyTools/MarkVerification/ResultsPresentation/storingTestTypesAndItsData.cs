// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.storingTestTypesAndItsData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class storingTestTypesAndItsData
{
  private string _testName;
  private List<object> _listofData;
  private string _pass = "PASS";

  public string TestName
  {
    get => this._testName;
    set => this._testName = value;
  }

  public List<object> ListofData
  {
    get => this._listofData;
    set => this._listofData = value;
  }

  public string Pass
  {
    get => this._pass;
    set => this._pass = value;
  }
}
