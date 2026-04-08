// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.HWDetailTemplate
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail;

public class HWDetailTemplate
{
  public int TemplateScale;
  public ScaleUnits ScaleUnits;
  public string TitleBlockName;
  public string TemplateName;
  public string TemplateManufacturerName;
  public List<AnchorGroup> AnchorGroups;
  public List<HardwareDetailScheduleInfo> ScheduleList;
  public List<TicketLegendInfo> LegendInfos;
  public bool bStackSchedules;
  public SimpleVector BOMAnchorPosition;
  public BOMJustification BOMJustification;
  public SimpleVector TitleLocationToUpperLeftCorner;
  private XYZ titleLocation;

  public HWDetailTemplate()
  {
    this.AnchorGroups = new List<AnchorGroup>();
    this.ScheduleList = new List<HardwareDetailScheduleInfo>();
    this.LegendInfos = new List<TicketLegendInfo>();
  }

  internal ViewSheet CreateHardwareDetail(
    Document revitDoc,
    AssemblyInstance assembly,
    string selectedTitleBlockName,
    string AssemblySheetNumber,
    int ScaleToUse,
    bool ShowErrorMessage)
  {
    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
    List<View> viewList = new List<View>();
    List<FamilySymbol> list = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).Cast<FamilySymbol>().ToList<FamilySymbol>();
    if (list == null || list.Count == 0)
      return (ViewSheet) null;
    FamilySymbol familySymbol = string.IsNullOrWhiteSpace(selectedTitleBlockName) ? list.Where<FamilySymbol>((Func<FamilySymbol, bool>) (x => x.Name == this.TitleBlockName)).FirstOrDefault<FamilySymbol>() : list.Where<FamilySymbol>((Func<FamilySymbol, bool>) (x => x.Name == selectedTitleBlockName)).FirstOrDefault<FamilySymbol>();
    if (familySymbol == null)
      return (ViewSheet) null;
    ViewSheet sheet = AssemblyViewUtils.CreateSheet(revitDoc, assembly.Id, familySymbol.Id);
    if (sheet == null)
      return (ViewSheet) null;
    sheet.SheetNumber = AssemblySheetNumber;
    sheet.Name = assembly.Name;
    IEnumerable<Element> elements = (IEnumerable<Element>) new FilteredElementCollector(revitDoc, sheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements();
    Element element = elements.Count<Element>() == 1 ? elements.First<Element>() : (Element) null;
    if (element == null)
    {
      this.titleLocation = new XYZ(0.0, 0.0, 0.0);
    }
    else
    {
      this.titleLocation = (element.Location as LocationPoint).Point;
      XYZ xyz = new XYZ(0.0, 0.0, 0.0);
    }
    XYZ xyz1 = this.titleLocation + this.TitleLocationToUpperLeftCorner;
    foreach (AnchorGroup anchorGroup in this.AnchorGroups)
    {
      int count = viewList.Count;
      Dictionary<string, List<string>> viewsNotCreated = new Dictionary<string, List<string>>();
      if (ScaleToUse == 0)
        viewList.AddRange((IEnumerable<View>) anchorGroup.CreateViews(assembly, this.TemplateScale, revitDoc, xyz1, sheet.Id, out viewsNotCreated, this.TemplateScale));
      else
        viewList.AddRange((IEnumerable<View>) anchorGroup.CreateViews(assembly, ScaleToUse, revitDoc, xyz1, sheet.Id, out viewsNotCreated, this.TemplateScale));
      if (viewList.Count > count)
        anchorGroup.PositionGroup(xyz1);
      foreach (KeyValuePair<string, List<string>> keyValuePair in viewsNotCreated)
      {
        if (dictionary.ContainsKey(keyValuePair.Key))
          dictionary[keyValuePair.Key].AddRange((IEnumerable<string>) keyValuePair.Value);
        else
          dictionary.Add(keyValuePair.Key, keyValuePair.Value);
      }
    }
    IEnumerable<View> source1 = new FilteredElementCollector(revitDoc).OfClass(typeof (View)).Cast<View>().Where<View>((Func<View, bool>) (view => view.ViewType == ViewType.Legend));
    StringBuilder stringBuilder = new StringBuilder();
    foreach (TicketLegendInfo legendInfo1 in this.LegendInfos)
    {
      TicketLegendInfo legendInfo = legendInfo1;
      IEnumerable<View> source2 = source1.Where<View>((Func<View, bool>) (view => view.Name == legendInfo.LegendName));
      if (source2 != null && source2.Any<View>())
      {
        View view = source2.First<View>();
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
        QA.LogLine($"  **  WARNING  **: Unable to locate template legend view: {legendInfo.LegendName}This legend will not be placed on the new hardware detail.");
        stringBuilder.AppendLine($"  **  WARNING  **: Unable to locate template legend view: {legendInfo.LegendName}This legend will not be placed on the new hardware detail.");
      }
    }
    if (stringBuilder.Length > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE Warning");
      taskDialog.MainInstruction = "Warnings encountered during hardware detail legend population";
      taskDialog.MainContent = "See expanded content for specific legends not placed.  This is typically due to EDGE not being able to locate the legend view in the project list of legend views";
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AllowCancellation = true;
      if (ShowErrorMessage)
        taskDialog.Show();
      QA.LogLine(stringBuilder.ToString());
    }
    return sheet;
  }
}
