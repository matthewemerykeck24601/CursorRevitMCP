// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.TicketTemplate
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.CloneTicket;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

public class TicketTemplate
{
  public int TemplateScale;
  public ScaleUnits ScaleUnits;
  public string TitleBlockName;
  public string TemplateName;
  public string TemplateManufacturerName;
  public List<AnchorGroup> AnchorGroups;
  public List<TicketScheduleInfo> ScheduleInfos;
  public List<TicketLegendInfo> LegendInfos;
  public bool bStackSchedules;
  private XYZ titleLocation;
  public string ConstructionProduct;
  [XmlIgnore]
  public XYZ titleBlockLocationPoint;
  [XmlIgnore]
  public List<SectionViewInfo> cloneTicketSectionViewList = new List<SectionViewInfo>();
  public SimpleVector BOMAnchorPosition;
  public BOMJustification BOMJustification;
  public SimpleVector TitleLocationToUpperLeftCorner;

  public TicketTemplate()
  {
    this.AnchorGroups = new List<AnchorGroup>();
    this.ScheduleInfos = new List<TicketScheduleInfo>();
    this.LegendInfos = new List<TicketLegendInfo>();
  }

  internal ViewSheet CreateTicket(
    AssemblyInstance assInstance,
    Document revitDoc,
    string SelectedTitleblockFromPopForm,
    string AssemblySheetNumber,
    int ScaleToUse,
    out string errorContent,
    out List<View> createdViews,
    out Dictionary<string, List<string>> viewsNotCreated,
    bool ShowErrorMessage = true)
  {
    viewsNotCreated = new Dictionary<string, List<string>>();
    createdViews = new List<View>();
    errorContent = "";
    this.TitleBlockName = SelectedTitleblockFromPopForm;
    IEnumerable<FamilySymbol> source1 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).ToElements().Where<Element>((Func<Element, bool>) (p => p.Name == this.TitleBlockName)).Cast<FamilySymbol>();
    ElementId titleBlockId = ElementId.InvalidElementId;
    if (source1.Any<FamilySymbol>())
      titleBlockId = source1.Select<FamilySymbol, ElementId>((Func<FamilySymbol, ElementId>) (p => p.Id)).First<ElementId>();
    ViewSheet sheet = AssemblyViewUtils.CreateSheet(revitDoc, assInstance.Id, titleBlockId);
    sheet.SheetNumber = AssemblySheetNumber;
    sheet.Name = assInstance.Name;
    IEnumerable<Element> elements = (IEnumerable<Element>) new FilteredElementCollector(revitDoc, sheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements();
    Element element1 = elements.Count<Element>() == 1 ? elements.First<Element>() : (Element) null;
    if (element1 == null)
    {
      this.titleLocation = new XYZ(0.0, 0.0, 0.0);
    }
    else
    {
      this.titleLocation = (element1.Location as LocationPoint).Point;
      XYZ xyz = new XYZ(0.0, 0.0, 0.0);
    }
    XYZ xyz1 = this.titleLocation + this.TitleLocationToUpperLeftCorner;
    foreach (AnchorGroup anchorGroup in this.AnchorGroups)
    {
      int count = createdViews.Count;
      Dictionary<string, List<string>> viewsNotCreated1 = new Dictionary<string, List<string>>();
      if (ScaleToUse == 0)
        createdViews.AddRange((IEnumerable<View>) anchorGroup.CreateViews(assInstance, this.TemplateScale, revitDoc, xyz1, sheet.Id, out viewsNotCreated1, this.TemplateScale));
      else
        createdViews.AddRange((IEnumerable<View>) anchorGroup.CreateViews(assInstance, ScaleToUse, revitDoc, xyz1, sheet.Id, out viewsNotCreated1, this.TemplateScale));
      if (createdViews.Count > count)
        anchorGroup.PositionGroup(xyz1);
      foreach (KeyValuePair<string, List<string>> keyValuePair in viewsNotCreated1)
      {
        if (viewsNotCreated.ContainsKey(keyValuePair.Key))
          viewsNotCreated[keyValuePair.Key].AddRange((IEnumerable<string>) keyValuePair.Value);
        else
          viewsNotCreated.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }
    IEnumerable<View> source2 = new FilteredElementCollector(revitDoc).OfClass(typeof (View)).Cast<View>().Where<View>((Func<View, bool>) (view => view.ViewType == ViewType.Legend));
    StringBuilder stringBuilder = new StringBuilder();
    foreach (TicketLegendInfo legendInfo1 in this.LegendInfos)
    {
      TicketLegendInfo legendInfo = legendInfo1;
      IEnumerable<View> source3 = (IEnumerable<View>) null;
      if (legendInfo.IsStrandPatternTemplate)
      {
        Element element2 = revitDoc.GetElement(assInstance.Id);
        if (element2 == null)
        {
          stringBuilder.AppendLine($"Edge Encountered an error attempting to find the strand pattern design number. Assembly instance Id: {assInstance.Id?.ToString()} does not exist in the model.  Strand Pattern not applied to ticket.");
        }
        else
        {
          Element structuralFramingElement = Utils.AssemblyUtils.Parameters.GetStructuralFramingElement(element2 as AssemblyInstance);
          if (structuralFramingElement == null)
          {
            stringBuilder.AppendLine("ERROR---  Unable to get structural framing element for this assembly: " + element2.Id?.ToString());
            continue;
          }
          string designNumber = Utils.ElementUtils.Parameters.GetParameterAsString(structuralFramingElement, "DESIGN_NUMBER");
          if (string.IsNullOrEmpty(designNumber))
          {
            stringBuilder.AppendLine($"Design Number parameter for element id: {assInstance.Id?.ToString()}:{assInstance.Name} was null or empty.  Please verify design number parameter for this element.");
          }
          else
          {
            source3 = source2.Where<View>((Func<View, bool>) (view => this.StrandPatternMatchesDesignNumber(view.Name, designNumber)));
            if (!source3.Any<View>())
              stringBuilder.AppendLine($"Ticket Populator failed to find strand pattern legend for design number: {designNumber}Expected to find a legend which contains both \"STRAND PATTERN\" and \"{designNumber}\" but couldn't find it.  Verify that legends contain such a legend view.{Environment.NewLine}");
          }
        }
      }
      else
        source3 = source2.Where<View>((Func<View, bool>) (view => view.Name == legendInfo.LegendName));
      if (source3 != null && source3.Any<View>())
      {
        View view = source3.First<View>();
        try
        {
          Viewport viewport = Viewport.Create(revitDoc, sheet.Id, view.Id, new XYZ(0.0, 0.0, 0.0));
          if (viewport != null)
          {
            XYZ outlineUpperLeftCorner = EDGERCreateTemplate.GetOutlineUpperLeftCorner(viewport.GetBoxOutline());
            XYZ xyz2 = xyz1 + legendInfo.VectorToUpperLeft - outlineUpperLeftCorner;
            viewport.SetBoxCenter(viewport.GetBoxCenter() + xyz2);
          }
          else
            stringBuilder.AppendLine($"Viewport created for View: {view.Name} was empty!  No legend was placed.");
        }
        catch (Exception ex)
        {
          QA.LogError("TicketTemplate PlaceLegendViews", $"Encountered exception while placing legends in sheet {sheet.Name} specific issue: {ex.Message}");
        }
      }
      else
      {
        QA.LogLine($"  **  WARNING  **: Unable to locate template legend view: {legendInfo.LegendName}This legend will not be placed on the new ticket.");
        stringBuilder.AppendLine($"  **  WARNING  **: Unable to locate template legend view: {legendInfo.LegendName}This legend will not be placed on the new ticket.");
      }
    }
    if (stringBuilder.Length > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE Warning");
      taskDialog.MainInstruction = "Warnings encountered during ticket legend population";
      taskDialog.MainContent = "See expanded content for specific legends not placed.  This is typically due to EDGE not being able to locate the legend view in the project list of legend views";
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AllowCancellation = true;
      errorContent = stringBuilder.ToString();
      if (ShowErrorMessage)
        taskDialog.Show();
      QA.LogLine(stringBuilder.ToString());
    }
    return sheet;
  }

  private bool StrandPatternMatchesDesignNumber(string legendName, string designNumber)
  {
    return legendName.ToUpper().Contains("STRAND PATTERN " + designNumber.ToUpper()) || legendName.ToUpper().Contains("END PATTERN " + designNumber.ToUpper()) || legendName.ToUpper().Contains("REINFORCING PATTERN " + designNumber.ToUpper());
  }
}
