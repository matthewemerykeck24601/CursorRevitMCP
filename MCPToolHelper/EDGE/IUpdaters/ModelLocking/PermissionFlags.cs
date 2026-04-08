// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.PermissionFlags
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

[Flags]
internal enum PermissionFlags : short
{
  None = 0,
  Admin = 1,
  Geometry = 2,
  ConnectionsHardware = 4,
  RebarHandling = 8,
  Assemblies = 16, // 0x0010
}
