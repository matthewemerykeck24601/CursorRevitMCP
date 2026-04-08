// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.AutoWarping.OverlappingEdgeException
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace EDGE.GeometryTools.AutoWarping;

public class OverlappingEdgeException : Exception
{
  public List<List<WarpingEdge>> collisionEdges;

  public OverlappingEdgeException(List<List<WarpingEdge>> edges) => this.collisionEdges = edges;
}
