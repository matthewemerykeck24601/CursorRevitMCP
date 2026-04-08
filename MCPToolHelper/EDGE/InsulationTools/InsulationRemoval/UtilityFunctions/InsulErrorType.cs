// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationRemoval.UtilityFunctions.InsulErrorType
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.InsulationTools.InsulationRemoval.UtilityFunctions;

public enum InsulErrorType
{
  Failure = 1,
  NoElements = 2,
  ElementsRemoved = 3,
  ReadOnly = 4,
  NoElementsSelGroup = 5,
  Success = 6,
}
