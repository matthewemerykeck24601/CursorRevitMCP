// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.SectionViewInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class SectionViewInfo
{
  public ElementId sourceViewAssociatedAssemblyId;
  public string sourceViewName;
  public int sourceViewScale;
  public XYZ sourceViewDirection;
  public Transform sourceViewCropBoxTransform;
  public BoundingBoxXYZ sourceCropBox;
  public ViewportRotation RotationOnSheet;
  public XYZ locationPoint;
  public bool cropBoxActive;
  public bool cropBoxVisible;
  public ElementId sourceViewId;
  public ElementId viewPortTypeId;

  public SectionViewInfo(
    View sourceView,
    ViewportRotation rotationOnSheet,
    XYZ locationPoint,
    ElementId viewPortTypeId)
  {
    this.sourceViewAssociatedAssemblyId = sourceView.AssociatedAssemblyInstanceId;
    this.sourceViewName = sourceView.Name;
    this.sourceViewScale = sourceView.Scale;
    this.sourceViewDirection = sourceView.ViewDirection;
    this.sourceViewCropBoxTransform = sourceView.CropBox.Transform;
    this.sourceCropBox = sourceView.CropBox;
    this.cropBoxActive = sourceView.CropBoxActive;
    this.cropBoxVisible = sourceView.CropBoxVisible;
    this.sourceViewId = sourceView.Id;
    this.viewPortTypeId = viewPortTypeId;
    this.RotationOnSheet = rotationOnSheet;
    this.locationPoint = locationPoint;
  }
}
