// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.CatalogAction
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;

#nullable disable
namespace EDGE.RebarTools;

[Flags]
public enum CatalogAction : short
{
  NoActionSelected = 0,
  CreateConformingMark = 1,
  CreateConformingMarkForALL = 2,
  ReplaceWithDictionaryMark = 4,
  ReplaceWithDictionaryMarkForALL = 8,
  DisableBarMarkAutomation = 16, // 0x0010
  UpdateWithNewMarkNumber = 32, // 0x0020
  CatalogMarkAsIs = 64, // 0x0040
  CatalogMarkAsIsForALL = 128, // 0x0080
  CloseDocument = 256, // 0x0100
}
