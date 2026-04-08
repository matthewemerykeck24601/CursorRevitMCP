// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.AnchorGroup
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

public class AnchorGroup
{
  public TicketViewportInfo MainView;
  public SimpleVector simpleVectorToUperLeft;
  public TicketViewportInfo TopView;
  public double TopDistance;
  public TicketViewportInfo BottomView;
  public double BottomDistance;
  public TicketViewportInfo LeftView;
  public double LeftDistance;
  public TicketViewportInfo RightView;
  public double RightDistance;
  private IEnumerable<ViewSection> mSectionViewTemplatesInProject;
  private IEnumerable<View3D> m3DViewTemplatesInProject;
  private ViewSection MainViewSection;
  private ViewSection TopViewSection;
  private ViewSection BottomViewSection;
  private ViewSection LeftViewSection;
  private ViewSection RightViewSection;
  private Viewport MainViewport;
  private Viewport TopViewport;
  private Viewport BottomViewport;
  private Viewport LeftViewport;
  private Viewport RightViewport;
  private View3D Main3DOrthoView;

  public Outline GetOutline()
  {
    Outline outline1 = new Outline(new XYZ(0.0, 0.0, 0.0), new XYZ(0.0, 0.0, 0.0));
    if (this.MainView == null)
      return outline1;
    Outline outline2 = new Outline(this.MainView.GetViewOutline());
    Outline returnedOutline = (Outline) null;
    this.DebugPrintOutline(outline2, "Befotre adding views");
    if (this.GetOutline(AnchorPosition.Main, out returnedOutline))
      this.AddOutlinePoints(outline2, returnedOutline);
    this.DebugPrintOutline(outline2, "After Main");
    if (this.GetOutline(AnchorPosition.Top, out returnedOutline))
      this.AddOutlinePoints(outline2, returnedOutline);
    this.DebugPrintOutline(outline2, "After Top");
    if (this.GetOutline(AnchorPosition.Bottom, out returnedOutline))
      this.AddOutlinePoints(outline2, returnedOutline);
    this.DebugPrintOutline(outline2, "After Bottom");
    if (this.GetOutline(AnchorPosition.Left, out returnedOutline))
      this.AddOutlinePoints(outline2, returnedOutline);
    this.DebugPrintOutline(outline2, "After Left");
    if (this.GetOutline(AnchorPosition.Right, out returnedOutline))
      this.AddOutlinePoints(outline2, returnedOutline);
    this.DebugPrintOutline(outline2, "After Right, Just before Returning this outline.");
    return outline2;
  }

  private void AddOutlinePoints(Outline group, Outline individual)
  {
    group.AddPoint(individual.MinimumPoint);
    group.AddPoint(individual.MaximumPoint);
  }

  private void DebugPrintOutline(Outline outline, string message)
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("Outline: " + message);
    stringBuilder.AppendLine("     Maximumpoint: " + outline.MaximumPoint.ToString());
    stringBuilder.AppendLine("     MinimumPoint: " + outline.MinimumPoint.ToString());
  }

  private bool GetOutline(AnchorPosition viewPosition, out Outline returnedOutline)
  {
    returnedOutline = (Outline) null;
    switch (viewPosition)
    {
      case AnchorPosition.Left:
        if (this.LeftView == null)
          return false;
        returnedOutline = this.LeftView.GetViewOutline();
        break;
      case AnchorPosition.Right:
        if (this.RightView == null)
          return false;
        returnedOutline = this.RightView.GetViewOutline();
        break;
      case AnchorPosition.Top:
        if (this.TopView == null)
          return false;
        returnedOutline = this.TopView.GetViewOutline();
        break;
      case AnchorPosition.Bottom:
        if (this.BottomView == null)
          return false;
        returnedOutline = this.BottomView.GetViewOutline();
        break;
      case AnchorPosition.Main:
        if (this.MainView == null)
          return false;
        returnedOutline = this.MainView.GetViewOutline();
        break;
    }
    return true;
  }

  public int GetScale() => this.MainView == null ? -1 : this.MainView.originalScale;

  private ElementId GetSectionMarkElementId(Document revitDoc, ElementId SectionViewId)
  {
    FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc);
    elementCollector.OfCategory(BuiltInCategory.OST_Viewers);
    IEnumerable<Element> source = elementCollector.ToElements().Where<Element>((Func<Element, bool>) (s => s.get_Parameter(BuiltInParameter.ID_PARAM).AsElementId() == SectionViewId));
    return source.Any<Element>() ? source.First<Element>().Id : ElementId.InvalidElementId;
  }

  private bool RotateSectionView(ViewSection sectionView, double angle, Document revitDoc)
  {
    try
    {
      ElementId sectionMarkElementId = this.GetSectionMarkElementId(revitDoc, sectionView.Id);
      BoundingBoxXYZ cropBox = sectionView.CropBox;
      XYZ center;
      XYZ endpoint2 = (center = (revitDoc.GetElement(sectionView.AssociatedAssemblyInstanceId) as AssemblyInstance).GetCenter()) + sectionView.ViewDirection;
      Line bound = Line.CreateBound(center, endpoint2);
      ElementTransformUtils.RotateElement(revitDoc, sectionMarkElementId, bound, angle);
      return true;
    }
    catch (Exception ex)
    {
      QA.InHouseMessage($"Rotate View Failed for view: {sectionView.Name} exception: {ex.Message}");
      return false;
    }
  }

  public List<View> CreateViews(
    AssemblyInstance assInstance,
    int Scale,
    Document revitDoc,
    XYZ anchorGroupUpperLeftCorner,
    ElementId sheetId,
    out Dictionary<string, List<string>> viewsNotCreated,
    int templateOverallScale)
  {
    viewsNotCreated = new Dictionary<string, List<string>>();
    List<View> views = new List<View>();
    this.mSectionViewTemplatesInProject = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSection)).ToElements().Cast<ViewSection>().Where<ViewSection>((Func<ViewSection, bool>) (p => p.IsTemplate));
    this.m3DViewTemplatesInProject = new FilteredElementCollector(revitDoc).OfClass(typeof (View3D)).ToElements().Cast<View3D>().Where<View3D>((Func<View3D, bool>) (p => p.IsTemplate));
    TicketWindowOutline ticketWindowOutline1 = (TicketWindowOutline) null;
    if (this.MainView != null && this.MainView.IsSection)
    {
      this.MainViewSection = AssemblyViewUtils.CreateDetailSection(revitDoc, assInstance.Id, this.MainView.GetAssemblyDetailViewOrientation());
      if (Scale == -1)
        this.MainViewSection.Scale = this.MainView.originalScale;
      else
        this.MainViewSection.Scale = Scale;
      this.MainViewSection.ViewTemplateId = this.GetViewTempalteId(this.MainView.ViewTemplateName);
      revitDoc.Regenerate();
      this.MainViewport = Viewport.Create(revitDoc, sheetId, this.MainViewSection.Id, new XYZ(0.0, 0.0, 0.0));
      if (this.MainViewport == null)
      {
        if (viewsNotCreated.ContainsKey(assInstance.Name))
          viewsNotCreated[assInstance.Name].Add(this.MainViewSection.Name);
        else
          viewsNotCreated.Add(assInstance.Name, new List<string>()
          {
            this.MainViewSection.Name
          });
      }
      else
      {
        if (this.MainView.sourceViewPortTypeId != ElementId.InvalidElementId)
          this.MainViewport.ChangeTypeId(this.MainView.sourceViewPortTypeId);
        this.MainViewport.Rotation = this.MainView.RotationOnSheet;
        if (!this.RotateSectionView(this.MainViewSection, this.MainView.SectionViewCropRotation, revitDoc))
          QA.InHouseMessage("Rotate MainView Failed");
        ticketWindowOutline1 = new TicketWindowOutline(this.MainViewport.GetBoxOutline());
        this.MainView.SetViewOutline(this.MainViewport.GetBoxOutline());
        views.Add((View) this.MainViewSection);
      }
    }
    else if (this.MainView != null)
    {
      this.Main3DOrthoView = AssemblyViewUtils.Create3DOrthographic(revitDoc, assInstance.Id);
      if (Scale == -1)
        this.Main3DOrthoView.Scale = this.MainView.originalScale;
      else
        this.Main3DOrthoView.Scale = Scale;
      this.Main3DOrthoView.ViewTemplateId = this.GetViewTempalteId(this.MainView.ViewTemplateName);
      try
      {
        this.Main3DOrthoView.Name = this.MainView.originalViewName;
      }
      catch (Exception ex)
      {
      }
      this.MainViewport = Viewport.Create(revitDoc, sheetId, this.Main3DOrthoView.Id, new XYZ(0.0, 0.0, 0.0));
      if (this.MainViewport == null)
      {
        if (viewsNotCreated.ContainsKey(assInstance.Name))
          viewsNotCreated[assInstance.Name].Add(this.Main3DOrthoView.Name);
        else
          viewsNotCreated.Add(assInstance.Name, new List<string>()
          {
            this.Main3DOrthoView.Name
          });
      }
      else
      {
        if (this.MainView.sourceViewPortTypeId != ElementId.InvalidElementId)
          this.MainViewport.ChangeTypeId(this.MainView.sourceViewPortTypeId);
        this.MainViewport.Rotation = this.MainView.RotationOnSheet;
        ticketWindowOutline1 = new TicketWindowOutline(this.MainViewport.GetBoxOutline());
        this.MainView.SetViewOutline(this.MainViewport.GetBoxOutline());
        views.Add((View) this.Main3DOrthoView);
      }
    }
    if (this.TopView != null)
    {
      this.TopViewSection = Scale != -1 ? this.CreateViewSection(assInstance, this.TopView, revitDoc, Scale) : this.CreateViewSection(assInstance, this.TopView, revitDoc, this.TopView.originalScale);
      this.TopViewport = Viewport.Create(revitDoc, sheetId, this.TopViewSection.Id, new XYZ(0.0, 0.0, 0.0));
      if (this.TopViewport == null)
      {
        if (viewsNotCreated.ContainsKey(assInstance.Name))
          viewsNotCreated[assInstance.Name].Add(this.TopViewSection.Name);
        else
          viewsNotCreated.Add(assInstance.Name, new List<string>()
          {
            this.TopViewSection.Name
          });
      }
      else
      {
        if (this.TopView.sourceViewPortTypeId != ElementId.InvalidElementId)
          this.TopViewport.ChangeTypeId(this.TopView.sourceViewPortTypeId);
        this.TopViewport.Rotation = this.TopView.RotationOnSheet;
        if (!this.RotateSectionView(this.TopViewSection, this.TopView.SectionViewCropRotation, revitDoc))
          QA.InHouseMessage("Rotate TopView Failed");
        TicketWindowOutline ticketWindowOutline2 = new TicketWindowOutline(this.TopViewport.GetBoxOutline());
        this.TopViewport.SetBoxCenter(new XYZ(0.0, 0.0, 0.0) + new XYZ(0.0, ticketWindowOutline1.Height / 2.0 + this.TopDistance, 0.0));
        this.TopView.SetViewOutline(this.TopViewport.GetBoxOutline());
        views.Add((View) this.TopViewSection);
      }
    }
    if (this.BottomView != null)
    {
      this.BottomViewSection = Scale != -1 ? this.CreateViewSection(assInstance, this.BottomView, revitDoc, Scale) : this.CreateViewSection(assInstance, this.BottomView, revitDoc, this.BottomView.originalScale);
      this.BottomViewport = Viewport.Create(revitDoc, sheetId, this.BottomViewSection.Id, new XYZ(0.0, 0.0, 0.0));
      if (this.BottomViewport == null)
      {
        if (viewsNotCreated.ContainsKey(assInstance.Name))
          viewsNotCreated[assInstance.Name].Add(this.BottomViewSection.Name);
        else
          viewsNotCreated.Add(assInstance.Name, new List<string>()
          {
            this.BottomViewSection.Name
          });
      }
      else
      {
        if (this.BottomView.sourceViewPortTypeId != ElementId.InvalidElementId)
          this.BottomViewport.ChangeTypeId(this.BottomView.sourceViewPortTypeId);
        this.BottomViewport.Rotation = this.BottomView.RotationOnSheet;
        if (!this.RotateSectionView(this.BottomViewSection, this.BottomView.SectionViewCropRotation, revitDoc))
          QA.InHouseMessage("Rotate BottomView Failed");
        TicketWindowOutline ticketWindowOutline3 = new TicketWindowOutline(this.BottomViewport.GetBoxOutline());
        this.BottomViewport.SetBoxCenter(new XYZ(0.0, 0.0, 0.0) - new XYZ(0.0, ticketWindowOutline1.Height / 2.0 + this.BottomDistance, 0.0));
        this.BottomView.SetViewOutline(this.BottomViewport.GetBoxOutline());
        views.Add((View) this.BottomViewSection);
      }
    }
    if (this.LeftView != null)
    {
      this.LeftViewSection = Scale != -1 ? this.CreateViewSection(assInstance, this.LeftView, revitDoc, Scale) : this.CreateViewSection(assInstance, this.LeftView, revitDoc, this.LeftView.originalScale);
      this.LeftViewport = Viewport.Create(revitDoc, sheetId, this.LeftViewSection.Id, new XYZ(0.0, 0.0, 0.0));
      if (this.LeftViewport == null)
      {
        if (viewsNotCreated.ContainsKey(assInstance.Name))
          viewsNotCreated[assInstance.Name].Add(this.LeftViewSection.Name);
        else
          viewsNotCreated.Add(assInstance.Name, new List<string>()
          {
            this.LeftViewSection.Name
          });
      }
      else
      {
        if (this.LeftView.sourceViewPortTypeId != ElementId.InvalidElementId)
          this.LeftViewport.ChangeTypeId(this.LeftView.sourceViewPortTypeId);
        this.LeftViewport.Rotation = this.LeftView.RotationOnSheet;
        if (!this.RotateSectionView(this.LeftViewSection, this.LeftView.SectionViewCropRotation, revitDoc))
          QA.InHouseMessage("Rotate LeftView Failed");
        TicketWindowOutline ticketWindowOutline4 = new TicketWindowOutline(this.LeftViewport.GetBoxOutline());
        this.LeftViewport.SetBoxCenter(new XYZ(0.0, 0.0, 0.0) - new XYZ(ticketWindowOutline1.Width / 2.0 + this.LeftDistance, 0.0, 0.0));
        this.LeftView.SetViewOutline(this.LeftViewport.GetBoxOutline());
        views.Add((View) this.LeftViewSection);
      }
    }
    if (this.RightView != null)
    {
      this.RightViewSection = Scale != -1 ? this.CreateViewSection(assInstance, this.RightView, revitDoc, Scale) : this.CreateViewSection(assInstance, this.RightView, revitDoc, this.RightView.originalScale);
      this.RightViewport = Viewport.Create(revitDoc, sheetId, this.RightViewSection.Id, new XYZ(0.0, 0.0, 0.0));
      if (this.RightViewport == null)
      {
        if (viewsNotCreated.ContainsKey(assInstance.Name))
          viewsNotCreated[assInstance.Name].Add(this.RightViewSection.Name);
        else
          viewsNotCreated.Add(assInstance.Name, new List<string>()
          {
            this.RightViewSection.Name
          });
      }
      else
      {
        if (this.RightView.sourceViewPortTypeId != ElementId.InvalidElementId)
          this.RightViewport.ChangeTypeId(this.RightView.sourceViewPortTypeId);
        this.RightViewport.Rotation = this.RightView.RotationOnSheet;
        if (!this.RotateSectionView(this.RightViewSection, this.RightView.SectionViewCropRotation, revitDoc))
          QA.InHouseMessage("Rotate RightView Failed");
        TicketWindowOutline ticketWindowOutline5 = new TicketWindowOutline(this.RightViewport.GetBoxOutline());
        this.RightViewport.SetBoxCenter(new XYZ(0.0, 0.0, 0.0) + new XYZ(ticketWindowOutline1.Width / 2.0 + this.RightDistance, 0.0, 0.0));
        this.RightView.SetViewOutline(this.RightViewport.GetBoxOutline());
        views.Add((View) this.RightViewSection);
      }
    }
    return views;
  }

  private ViewSection CreateViewSection(
    AssemblyInstance assInstance,
    TicketViewportInfo info,
    Document revitDoc,
    int Scale)
  {
    ViewSection detailSection = AssemblyViewUtils.CreateDetailSection(revitDoc, assInstance.Id, info.GetAssemblyDetailViewOrientation());
    detailSection.ViewTemplateId = this.GetViewTempalteId(info.ViewTemplateName);
    detailSection.Scale = Scale;
    revitDoc.Regenerate();
    return detailSection;
  }

  private ElementId GetViewTempalteId(string ViewTemplateName)
  {
    ElementId invalidElementId = ElementId.InvalidElementId;
    if (this.mSectionViewTemplatesInProject.Where<ViewSection>((Func<ViewSection, bool>) (s => s.Name == ViewTemplateName)).Any<ViewSection>())
      return this.mSectionViewTemplatesInProject.Where<ViewSection>((Func<ViewSection, bool>) (s => s.Name == ViewTemplateName)).Select<ViewSection, ElementId>((Func<ViewSection, ElementId>) (s => s.Id)).First<ElementId>();
    return this.m3DViewTemplatesInProject.Where<View3D>((Func<View3D, bool>) (s => s.Name == ViewTemplateName)).Any<View3D>() ? this.m3DViewTemplatesInProject.Where<View3D>((Func<View3D, bool>) (s => s.Name == ViewTemplateName)).Select<View3D, ElementId>((Func<View3D, ElementId>) (s => s.Id)).First<ElementId>() : invalidElementId;
  }

  public void SetView(AnchorPosition position, TicketViewportInfo viewportInfo)
  {
    switch (position)
    {
      case AnchorPosition.Left:
        this.LeftView = viewportInfo;
        if (this.MainView == null)
          break;
        this.LeftDistance = this.MainView.GetViewOutline().MinimumPoint.X - this.LeftView.GetBoxCenterPoint().X;
        break;
      case AnchorPosition.Right:
        this.RightView = viewportInfo;
        if (this.MainView == null)
          break;
        this.RightDistance = this.RightView.GetBoxCenterPoint().X - this.MainView.GetViewOutline().MaximumPoint.X;
        break;
      case AnchorPosition.Top:
        this.TopView = viewportInfo;
        if (this.MainView == null)
          break;
        this.TopDistance = this.TopView.GetBoxCenterPoint().Y - this.MainView.GetViewOutline().MaximumPoint.Y;
        break;
      case AnchorPosition.Bottom:
        this.BottomView = viewportInfo;
        if (this.MainView == null)
          break;
        this.BottomDistance = this.MainView.GetViewOutline().MinimumPoint.Y - this.BottomView.GetBoxCenterPoint().Y;
        break;
      case AnchorPosition.Main:
        this.MainView = viewportInfo;
        break;
    }
  }

  internal void PositionGroup(XYZ titleBlockUpperLeftCorner)
  {
    XYZ outlineUpperLeftCorner = EDGERCreateTemplate.GetOutlineUpperLeftCorner(this.GetOutline());
    XYZ xyz = titleBlockUpperLeftCorner + this.simpleVectorToUperLeft - outlineUpperLeftCorner;
    if (this.MainViewport != null)
      this.MainViewport.SetBoxCenter(this.MainViewport.GetBoxCenter() + xyz);
    if (this.TopViewport != null)
      this.TopViewport.SetBoxCenter(this.TopViewport.GetBoxCenter() + xyz);
    if (this.BottomViewport != null)
      this.BottomViewport.SetBoxCenter(this.BottomViewport.GetBoxCenter() + xyz);
    if (this.LeftViewport != null)
      this.LeftViewport.SetBoxCenter(this.LeftViewport.GetBoxCenter() + xyz);
    if (this.RightViewport == null)
      return;
    this.RightViewport.SetBoxCenter(this.RightViewport.GetBoxCenter() + xyz);
  }

  internal void PositionGroupCloneTicket(XYZ titleBlockUpperLeftCorner)
  {
    if (this.MainViewport != null)
      this.MainViewport.SetBoxCenter(titleBlockUpperLeftCorner + this.MainView.GetBoxCenterPoint());
    if (this.TopViewport != null)
      this.TopViewport.SetBoxCenter(titleBlockUpperLeftCorner + this.TopView.GetBoxCenterPoint());
    if (this.BottomViewport != null)
      this.BottomViewport.SetBoxCenter(titleBlockUpperLeftCorner + this.BottomView.GetBoxCenterPoint());
    if (this.LeftViewport != null)
      this.LeftViewport.SetBoxCenter(titleBlockUpperLeftCorner + this.LeftView.GetBoxCenterPoint());
    if (this.RightViewport == null)
      return;
    this.RightViewport.SetBoxCenter(titleBlockUpperLeftCorner + this.RightView.GetBoxCenterPoint());
  }

  internal void copyViewParameters(Document revitDoc)
  {
    if (this.MainViewport != null)
      EDGE.TicketTools.CloneTicket.CloneTicket.copyViewParameters(revitDoc, this.MainView.sourceViewId, this.MainViewport);
    if (this.TopViewport != null)
      EDGE.TicketTools.CloneTicket.CloneTicket.copyViewParameters(revitDoc, this.TopView.sourceViewId, this.TopViewport);
    if (this.BottomViewport != null)
      EDGE.TicketTools.CloneTicket.CloneTicket.copyViewParameters(revitDoc, this.BottomView.sourceViewId, this.BottomViewport);
    if (this.LeftViewport != null)
      EDGE.TicketTools.CloneTicket.CloneTicket.copyViewParameters(revitDoc, this.LeftView.sourceViewId, this.LeftViewport);
    if (this.RightViewport == null)
      return;
    EDGE.TicketTools.CloneTicket.CloneTicket.copyViewParameters(revitDoc, this.RightView.sourceViewId, this.RightViewport);
  }

  internal Dictionary<ElementId, ElementId> getSourceToTargetMapping()
  {
    Dictionary<ElementId, ElementId> sourceToTargetMapping = new Dictionary<ElementId, ElementId>();
    if (this.MainViewport != null && this.MainViewSection != null)
      sourceToTargetMapping.Add(this.MainView.sourceViewId, this.MainViewSection.Id);
    if (this.TopViewport != null && this.TopViewSection != null)
      sourceToTargetMapping.Add(this.TopView.sourceViewId, this.TopViewSection.Id);
    if (this.BottomViewport != null && this.BottomViewSection != null)
      sourceToTargetMapping.Add(this.BottomView.sourceViewId, this.BottomViewSection.Id);
    if (this.LeftViewport != null && this.LeftViewSection != null)
      sourceToTargetMapping.Add(this.LeftView.sourceViewId, this.LeftViewSection.Id);
    if (this.RightViewport != null && this.RightViewSection != null)
      sourceToTargetMapping.Add(this.RightView.sourceViewId, this.RightViewSection.Id);
    return sourceToTargetMapping;
  }
}
