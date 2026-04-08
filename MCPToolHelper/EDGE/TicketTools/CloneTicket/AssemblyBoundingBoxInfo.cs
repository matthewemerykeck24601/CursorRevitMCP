// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.AssemblyBoundingBoxInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.AutoDimensioning;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class AssemblyBoundingBoxInfo
{
  public double targetXMax;
  public double targetYMax;
  public double targetXMin;
  public double targetYMin;
  public double sourceXMax;
  public double sourceYMax;
  public double sourceXMin;
  public double sourceYMin;
  public Transform sourceViewTransform;
  public Transform targetViewTransform;
  public BoundingBoxXYZ sourceBoundingBox;
  public BoundingBoxXYZ targetBoundingBox;

  public AssemblyBoundingBoxInfo(
    Transform sourceViewTrans,
    Transform targetViewTrans,
    BoundingBoxXYZ sourceBoundingBox,
    BoundingBoxXYZ targetBoundingBox,
    StructuralFramingBoundsObject sourceBoundsObject,
    StructuralFramingBoundsObject targetBoundsObject)
  {
    this.sourceViewTransform = sourceViewTrans;
    this.targetViewTransform = targetViewTrans;
    this.sourceBoundingBox = sourceBoundingBox;
    this.targetBoundingBox = targetBoundingBox;
    targetBoundingBox.Transform.Inverse.OfPoint(targetBoundingBox.Max);
    targetBoundingBox.Transform.Inverse.OfPoint(targetBoundingBox.Min);
    sourceBoundingBox.Transform.Inverse.OfPoint(sourceBoundingBox.Max);
    sourceBoundingBox.Transform.Inverse.OfPoint(sourceBoundingBox.Min);
    this.targetXMax = targetBoundsObject.xMax;
    this.targetYMax = targetBoundsObject.yMax;
    this.targetXMin = targetBoundsObject.xMin;
    this.targetYMin = targetBoundsObject.yMin;
    this.sourceXMax = sourceBoundsObject.xMax;
    this.sourceYMax = sourceBoundsObject.yMax;
    this.sourceXMin = sourceBoundsObject.xMin;
    this.sourceYMin = sourceBoundsObject.yMin;
  }
}
