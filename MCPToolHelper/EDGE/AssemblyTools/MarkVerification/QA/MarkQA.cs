// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.QA.MarkQA
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.QA;

public class MarkQA
{
  public bool bFamilyTypeTest = true;
  public bool bCompareAllParameters = true;
  public bool bCompareMaterialVolumes = true;
  public bool bCompareAddons_VolMatCountFamily = true;
  public bool bComparePlates_NamesAndCounts = true;
  public bool bCompareAddons_LocationAndOrientation = true;
  public bool bComparePlate_LocationAndOrientation = true;
  public bool bCompareSolidsFaces = true;
  public bool traditionalApproach = true;
  public bool originalStateIsTraditonal = true;

  public void SetSwitches(bool[] switches)
  {
    this.bFamilyTypeTest = switches[0];
    this.bCompareAllParameters = switches[1];
    this.bCompareMaterialVolumes = switches[2];
    this.bCompareAddons_VolMatCountFamily = switches[3];
    this.bComparePlates_NamesAndCounts = switches[4];
    this.bCompareAddons_LocationAndOrientation = switches[5];
    this.bComparePlate_LocationAndOrientation = switches[6];
    this.bCompareSolidsFaces = switches[7];
    this.traditionalApproach = switches[8];
  }

  public bool[] GetSwitches()
  {
    return new bool[9]
    {
      this.bFamilyTypeTest,
      this.bCompareAllParameters,
      this.bCompareMaterialVolumes,
      this.bCompareAddons_VolMatCountFamily,
      this.bComparePlates_NamesAndCounts,
      this.bCompareAddons_LocationAndOrientation,
      this.bComparePlate_LocationAndOrientation,
      this.bCompareSolidsFaces,
      this.traditionalApproach
    };
  }

  public MarkQA()
  {
  }

  public MarkQA(MarkQA otherMarkQA)
  {
    bool[] flagArray = new bool[9];
    this.SetSwitches(otherMarkQA.GetSwitches());
  }
}
