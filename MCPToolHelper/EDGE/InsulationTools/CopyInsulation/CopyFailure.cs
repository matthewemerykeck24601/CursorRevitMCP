// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.CopyInsulation.CopyFailure
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.InsulationTools.CopyInsulation;

public class CopyFailure
{
  public AssemblyInstance SourceAssembly;
  public AssemblyInstance TargetAssembly;
  public FamilyInstance Insulation;
  public FamilyInstance Pin;
  public CopyFailure.reasonEnum Reason;

  public CopyFailure()
  {
  }

  public CopyFailure(
    AssemblyInstance sourceAssembly,
    FamilyInstance insulation,
    FamilyInstance pin,
    AssemblyInstance targetAssembly = null,
    CopyFailure.reasonEnum reason = CopyFailure.reasonEnum.CopyFailure)
  {
    this.SourceAssembly = sourceAssembly;
    this.TargetAssembly = targetAssembly;
    this.Insulation = insulation;
    this.Pin = pin;
    this.Reason = reason;
  }

  public enum reasonEnum
  {
    CopyFailure,
    CopyPinFailure,
    InvalidSFElement,
    InvalidInsulation,
    InvalidPin,
    InvalidSourceTransform,
    InvalidTargetTransform,
    InvalidSourceInsulationTransform,
    InvalidTargetInsulationTransform,
  }
}
