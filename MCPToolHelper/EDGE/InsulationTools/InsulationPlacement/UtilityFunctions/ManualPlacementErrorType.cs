// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.ManualPlacementErrorType
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;

public enum ManualPlacementErrorType
{
  NoMessage,
  NoValidElement,
  TooManySelected,
  InvalidView,
  UnlockView,
  Success,
  FamilySymbolNotFound,
  PointsOutsideLoop,
  LessThanLenMin,
  LessThanWidMin,
  LessThanLenPlusWidMin,
  GreaterThanLenMax,
  GreaterThanWidMax,
  GreaterThanLenPlusWidMax,
  InsulationCutIntoMultiple,
  InvalidDimensions,
}
