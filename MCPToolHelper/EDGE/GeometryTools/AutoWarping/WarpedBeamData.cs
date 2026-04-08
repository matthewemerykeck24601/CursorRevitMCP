// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.WarpedBeamData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

internal class WarpedBeamData
{
  public ElementId Id { get; set; }

  public XYZ MemberPlaneNormalVector { get; set; }
}
