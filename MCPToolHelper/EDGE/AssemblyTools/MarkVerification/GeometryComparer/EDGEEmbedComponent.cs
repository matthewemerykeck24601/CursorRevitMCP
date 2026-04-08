// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.EDGEEmbedComponent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.AssemblyTools.MarkVerification.Tools;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

public class EDGEEmbedComponent(
  FamilyInstance componentInstance,
  FamilyInstance hostInstance,
  ComparisonOption compareOption) : EDGEAssemblyComponent_Base(componentInstance, hostInstance, compareOption, true)
{
}
