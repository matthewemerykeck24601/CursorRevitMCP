// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.AVFViewTimer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

#nullable disable
namespace EDGE.AssemblyTools;

internal class AVFViewTimer
{
  public View viewWithAVFResults;
  public int timerCounter;
  public int FaceIndex;
  public int PointIndex;
  public SpatialFieldManager manager;

  public AVFViewTimer(View view, int faceIndex, int pointIndex, SpatialFieldManager sfm)
  {
    this.viewWithAVFResults = view;
    this.FaceIndex = faceIndex;
    this.PointIndex = pointIndex;
    this.timerCounter = 0;
    this.manager = sfm;
  }
}
