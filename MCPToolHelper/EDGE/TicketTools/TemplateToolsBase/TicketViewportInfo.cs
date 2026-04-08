// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.TicketViewportInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.Xml.Serialization;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

public class TicketViewportInfo
{
  public TemplateViewOrientation mSectionViewOrientation;
  public string ViewTemplateName;
  public ViewportRotation RotationOnSheet;
  public double SectionViewCropRotation;
  public string originalViewName;
  public double offsetFromNearestView;
  public bool IsSection;
  public int originalScale;
  [XmlIgnore]
  public ElementId sourceViewId = ElementId.InvalidElementId;
  [XmlIgnore]
  public ElementId sourceViewPortTypeId = ElementId.InvalidElementId;
  private Guid mViewportGUID;
  private Outline mViewOutline;
  private XYZ mBoxCenterPoint;

  public void SetViewportGuid(Guid viewGuid) => this.mViewportGUID = viewGuid;

  public void SetViewOutline(Outline viewOutline) => this.mViewOutline = viewOutline;

  public Outline GetViewOutline() => this.mViewOutline;

  public void SetBoxCenterPoint(XYZ centerpoint) => this.mBoxCenterPoint = centerpoint;

  public XYZ GetBoxCenterPoint() => this.mBoxCenterPoint;

  public TicketViewportInfo()
  {
  }

  public TicketViewportInfo(
    string viewName,
    string viewTemplateName,
    ViewportRotation rotationOnSheet,
    double cropRotation,
    int scale,
    bool view3DOrtho = false)
  {
    this.originalViewName = viewName;
    this.ViewTemplateName = viewTemplateName;
    this.RotationOnSheet = rotationOnSheet;
    this.SectionViewCropRotation = cropRotation;
    this.originalScale = scale;
    try
    {
      if (view3DOrtho)
        this.ResolveViewOrientation("3D ORTHO");
      else
        this.ResolveViewOrientation(viewName);
    }
    catch
    {
      PopulatorQA.DebugMessage("Failed to resolve view name: " + viewName);
    }
  }

  public View CreateDetailView(Document revitDoc) => throw new NotImplementedException();

  public static bool IsTicketTemplateView(View testView)
  {
    string upper = testView.Name.ToUpper();
    return upper.Contains("ELEVATION TOP") || upper.Contains("ELEVATION FRONT") || upper.Contains("ELEVATION BACK") || upper.Contains("ELEVATION LEFT") || upper.Contains("ELEVATION RIGHT") || upper.Contains("ELEVATION BOTTOM") || upper.Contains("DETAIL SECTION A") || upper.Contains("DETAIL SECTION B") || upper.Contains("PLAN DETAIL") || upper.Contains("3D ORTHO");
  }

  private void ResolveViewOrientation(string viewName)
  {
    this.IsSection = true;
    string upper = viewName.ToUpper();
    if (upper.Contains("ELEVATION TOP"))
      this.mSectionViewOrientation = TemplateViewOrientation.ElevationTop;
    else if (upper.Contains("ELEVATION FRONT"))
      this.mSectionViewOrientation = TemplateViewOrientation.ElevationFront;
    else if (upper.Contains("ELEVATION BACK"))
      this.mSectionViewOrientation = TemplateViewOrientation.ElevationBack;
    else if (upper.Contains("ELEVATION LEFT"))
      this.mSectionViewOrientation = TemplateViewOrientation.ElevationLeft;
    else if (upper.Contains("ELEVATION RIGHT"))
      this.mSectionViewOrientation = TemplateViewOrientation.ElevationRight;
    else if (upper.Contains("ELEVATION BOTTOM"))
      this.mSectionViewOrientation = TemplateViewOrientation.ElevationBottom;
    else if (upper.Contains("DETAIL SECTION A"))
      this.mSectionViewOrientation = TemplateViewOrientation.DetailSectionA;
    else if (upper.Contains("DETAIL SECTION B"))
      this.mSectionViewOrientation = TemplateViewOrientation.DetailSectionB;
    else if (upper.Contains("PLAN DETAIL"))
    {
      this.mSectionViewOrientation = TemplateViewOrientation.HorizontalDetail;
    }
    else
    {
      if (!upper.Contains("3D ORTHO"))
        throw new Exception($"Unable to resolve ViewOrietnation for view name: {viewName} upper: {upper}");
      this.IsSection = false;
    }
  }

  public AssemblyDetailViewOrientation GetAssemblyDetailViewOrientation()
  {
    switch (this.mSectionViewOrientation)
    {
      case TemplateViewOrientation.ElevationTop:
        return AssemblyDetailViewOrientation.ElevationTop;
      case TemplateViewOrientation.ElevationBottom:
        return AssemblyDetailViewOrientation.ElevationBottom;
      case TemplateViewOrientation.ElevationFront:
        return AssemblyDetailViewOrientation.ElevationFront;
      case TemplateViewOrientation.ElevationBack:
        return AssemblyDetailViewOrientation.ElevationBack;
      case TemplateViewOrientation.ElevationLeft:
        return AssemblyDetailViewOrientation.ElevationLeft;
      case TemplateViewOrientation.ElevationRight:
        return AssemblyDetailViewOrientation.ElevationRight;
      case TemplateViewOrientation.HorizontalDetail:
        return AssemblyDetailViewOrientation.HorizontalDetail;
      case TemplateViewOrientation.DetailSectionA:
        return AssemblyDetailViewOrientation.DetailSectionA;
      case TemplateViewOrientation.DetailSectionB:
        return AssemblyDetailViewOrientation.DetailSectionB;
      default:
        throw new Exception("Unable to resolve assembly detail section orientation. GetAssemblyDetailViewOrietnation(): TicketViewportInfo.cs");
    }
  }
}
