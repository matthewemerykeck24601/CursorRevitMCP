// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.CloneTicket
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using EDGE.TicketTools.AutoDimensioning;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketPopulator;
using EDGE.TicketTools.TicketTemplateTools.Debugging;
using EDGE.UserSettingTools.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.IEnumerable_Extensions;
using Utils.MiscUtils;
using Utils.SettingsUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

[Transaction(TransactionMode.Manual)]
public class CloneTicket : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document revitDoc = activeUiDocument.Document;
    UIApplication application = commandData.Application;
    double num1 = 1.0 / 192.0;
    if (revitDoc.IsFamilyDocument)
    {
      int num2 = (int) MessageBox.Show("Clone Ticket cannot be used within a family document.");
      return (Result) 1;
    }
    List<AssemblyInstance> list1 = new FilteredElementCollector(revitDoc).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
    List<AssemblyInstance> possibleSourceAssemblies = new List<AssemblyInstance>();
    foreach (AssemblyInstance assemblyInstance in list1.ToList<AssemblyInstance>())
    {
      AssemblyInstance elem = assemblyInstance;
      if (elem.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(elem.Document.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320)).Count<Element>() != 1 || Utils.ElementUtils.Parameters.GetParameterAsBool((Element) elem, "HARDWARE_DETAIL"))
        list1.Remove(elem);
      else if (new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId.Equals((object) elem.Id))).ToList<ViewSheet>().Count<ViewSheet>() > 0)
        possibleSourceAssemblies.Add(elem);
    }
    Dictionary<string, List<AssemblyInstance>> dictionary1 = new Dictionary<string, List<AssemblyInstance>>();
    Dictionary<string, List<ElementId>> dictionary2 = new Dictionary<string, List<ElementId>>();
    foreach (AssemblyInstance assemblyInstance in list1.ToList<AssemblyInstance>())
    {
      if (dictionary1.ContainsKey(assemblyInstance.Name))
        dictionary1[assemblyInstance.Name].Add(assemblyInstance);
      else
        dictionary1.Add(assemblyInstance.Name, new List<AssemblyInstance>()
        {
          assemblyInstance
        });
      if (dictionary2.ContainsKey(assemblyInstance.Name))
        dictionary2[assemblyInstance.Name].Add(assemblyInstance.Id);
      else
        dictionary2.Add(assemblyInstance.Name, new List<ElementId>()
        {
          assemblyInstance.Id
        });
    }
    ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
    bool flag1 = true;
    string str1 = string.Empty;
    if (elementIds.Count > 0)
    {
      foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
      {
        Element element = revitDoc.GetElement(id);
        if (element is AssemblyInstance)
        {
          AssemblyInstance elem = element as AssemblyInstance;
          if ((element as AssemblyInstance).GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(element.Document.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320)).Count<Element>() != 1 || Utils.ElementUtils.Parameters.GetParameterAsBool((Element) elem, "HARDWARE_DETAIL"))
          {
            flag1 = false;
            break;
          }
          if (str1 == string.Empty)
            str1 = elem.Name;
          else if (str1 != elem.Name)
          {
            flag1 = false;
            break;
          }
        }
        else
        {
          flag1 = false;
          break;
        }
      }
    }
    else
      flag1 = false;
    foreach (string key in dictionary1.Keys)
    {
      if (dictionary1[key].Count > 1)
      {
        List<ElementId> list2 = dictionary1[key].Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (x => x.Id)).Where<ElementId>((Func<ElementId, bool>) (x => possibleSourceAssemblies.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (y => y.Id)).Contains<ElementId>(x))).ToList<ElementId>();
        if (list2.Count<ElementId>() > 0)
        {
          ElementId elementId = list2.FirstOrDefault<ElementId>();
          foreach (AssemblyInstance assemblyInstance in dictionary1[key])
          {
            if (assemblyInstance.Id != elementId)
              list1.Remove(assemblyInstance);
          }
          if (flag1 && str1 == key)
          {
            elementIds.Clear();
            elementIds.Add(elementId);
          }
        }
        else
        {
          ElementId id = dictionary1[key].FirstOrDefault<AssemblyInstance>().Id;
          foreach (AssemblyInstance assemblyInstance in dictionary1[key])
          {
            if (assemblyInstance.Id != id)
              list1.Remove(assemblyInstance);
          }
          if (flag1 && str1 == key)
            elementIds.Clear();
        }
      }
      else if (dictionary1[key].Count == 1)
      {
        List<ElementId> list3 = dictionary1[key].Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (x => x.Id)).Where<ElementId>((Func<ElementId, bool>) (x => possibleSourceAssemblies.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (y => y.Id)).Contains<ElementId>(x))).ToList<ElementId>();
        if (flag1 && str1 == key && list3.Count == 0)
        {
          elementIds.Clear();
          flag1 = false;
        }
      }
    }
    bool preselectedSource = false;
    if (elementIds.Count<ElementId>() == 1 & flag1)
    {
      possibleSourceAssemblies = new List<AssemblyInstance>()
      {
        revitDoc.GetElement(elementIds.First<ElementId>()) as AssemblyInstance
      };
      preselectedSource = true;
    }
    bool flag2 = true;
    bool flag3 = true;
    if (possibleSourceAssemblies.Count == 0)
      flag2 = false;
    if (list1.Count < 2)
      flag3 = false;
    string str2 = string.Empty;
    if (!flag2)
      str2 = "There are no valid assemblies with tickets to clone. Please create a ticket to clone and try again.";
    else if (!flag3)
      str2 = "There must be another assembly to clone to. Please create a new assembly to clone to and try again.";
    if (!flag2 || !flag3)
    {
      new TaskDialog("Clone Ticket")
      {
        MainInstruction = str2
      }.Show();
      return (Result) 1;
    }
    AssemblySelectionWindow selectionWindow = new AssemblySelectionWindow(list1, possibleSourceAssemblies, revitDoc, preselectedSource);
    if (!selectionWindow.ShowDialog().GetValueOrDefault())
      return (Result) 1;
    AssemblyInstance sourceAssembly = list1[list1.FindIndex((Predicate<AssemblyInstance>) (x => x.Id.Equals((object) selectionWindow.selectedSource)))];
    List<AssemblyInstance> list4 = list1.Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (x => selectionWindow.selectedTargets.Contains(x.Id))).ToList<AssemblyInstance>();
    List<ElementId> relinquishList1;
    if (activeUiDocument.Document.IsWorkshared && !CheckElementsOwnership.CheckOwnership("Clone Ticket", list4.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (assemblyInstance => assemblyInstance.Id)).ToList<ElementId>(), revitDoc, activeUiDocument, out relinquishList1))
    {
      if (relinquishList1.Count > 0)
      {
        RelinquishOptions generalCategories = new RelinquishOptions(false);
        generalCategories.CheckedOutElements = true;
        TransactWithCentralOptions options = new TransactWithCentralOptions();
        WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
      }
      return (Result) 1;
    }
    List<AutoTicketSettingsTools> ticketSettingsToolsList1 = AutoTicketSettingsReader.ReaderforAutoTicketSettings(revitDoc, "CLONE TICKET", false, out List<AutoTicketAppendStringParameterData> _);
    if (ticketSettingsToolsList1 != null)
    {
      foreach (AutoTicketSettingsTools ticketSettingsTools in ticketSettingsToolsList1)
      {
        if (ticketSettingsTools is CloneTicketSamePointTolerance)
        {
          CloneTicketSamePointTolerance samePointTolerance = ticketSettingsTools as CloneTicketSamePointTolerance;
          string feetValue = string.IsNullOrEmpty(samePointTolerance.FeetValue) ? "" : samePointTolerance.FeetValue;
          string inchesValue = string.IsNullOrEmpty(samePointTolerance.inchesValue) ? "" : samePointTolerance.inchesValue;
          if (!string.IsNullOrEmpty(feetValue) || !string.IsNullOrEmpty(inchesValue))
            num1 = FeetAndInchesRounding.covertFeetInchToDouble(new string[2]
            {
              feetValue,
              inchesValue
            });
        }
      }
    }
    List<ViewSheet> list5 = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId.Equals((object) sourceAssembly.Id))).ToList<ViewSheet>();
    Autodesk.Revit.DB.View activeView = application.ActiveUIDocument.ActiveView;
    foreach (ViewSheet viewSheet in list5)
    {
      if (viewSheet.IsValidObject)
        application.ActiveUIDocument.RequestViewChange((Autodesk.Revit.DB.View) viewSheet);
    }
    List<ViewSheet> list6 = list5.OrderBy<ViewSheet, string>((Func<ViewSheet, string>) (s => s.SheetNumber)).ToList<ViewSheet>();
    List<string> source1 = new List<string>();
    foreach (ViewSheet viewSheet in list6)
    {
      if (new FilteredElementCollector(revitDoc, viewSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).ToElementIds().ToList<ElementId>().Count > 1)
        source1.Add($"Sheet {viewSheet.SheetNumber} - {viewSheet.Name}");
    }
    if (source1.Count<string>() > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Clone Ticket");
      taskDialog1.MainContent = "The ticket you are cloning has more than one title block. Only one title block will be cloned. Do you wish to continue?\n";
      foreach (string str3 in source1)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str3}\n";
      }
      taskDialog1.CommonButtons = (TaskDialogCommonButtons) 10;
      if (taskDialog1.Show() != 6)
        return (Result) 1;
    }
    Dictionary<ViewSheet, sheetListItem> dictionary3 = new Dictionary<ViewSheet, sheetListItem>();
    BOMFormattingWindow formattingWindow = new BOMFormattingWindow(list6);
    if (!formattingWindow.ShowDialog().GetValueOrDefault())
      return (Result) 1;
    BOMFormattingViewModel dataContext = formattingWindow.DataContext as BOMFormattingViewModel;
    foreach (ViewSheet key in list6)
    {
      foreach (sheetListItem sheet in dataContext.SheetList)
      {
        if (sheet.SheetID.Equals(EDGE.TicketTools.CloneTicket.CloneTicket.getSheetTitle(key.SheetNumber, key.Name)))
        {
          dictionary3.Add(key, sheet);
          break;
        }
      }
    }
    Transaction transaction1 = new Transaction(revitDoc, "Create Temp Template");
    int num3 = (int) transaction1.Start();
    Parameter parameter1 = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    List<TicketBOMCore.BOMScheduleName> scheduleNamesList = TicketBOMCore.GetOrderedScheduleNamesList(parameter1 == null ? "default" : (parameter1.AsString() == null ? "default" : parameter1.AsString().ToUpper()));
    Dictionary<ViewSheet, TicketTemplate> dictionary4 = new Dictionary<ViewSheet, TicketTemplate>();
    Dictionary<ViewSheet, Dictionary<Dictionary<string, ElementId>, bool>> dictionary5 = new Dictionary<ViewSheet, Dictionary<Dictionary<string, ElementId>, bool>>();
    Dictionary<ViewSheet, Dictionary<string, List<TicketScheduleInfo>>> dictionary6 = new Dictionary<ViewSheet, Dictionary<string, List<TicketScheduleInfo>>>();
    foreach (ViewSheet key in list6)
    {
      Dictionary<Dictionary<string, ElementId>, bool> dictionary7 = new Dictionary<Dictionary<string, ElementId>, bool>();
      Dictionary<string, List<TicketScheduleInfo>> dictionary8 = new Dictionary<string, List<TicketScheduleInfo>>();
      TicketTemplate ticketTemplate = new TicketTemplate();
      Transform AssemblyTransform = (Transform) null;
      if (sourceAssembly != null)
        AssemblyTransform = sourceAssembly.GetTransform();
      string sheetTitle = EDGE.TicketTools.CloneTicket.CloneTicket.getSheetTitle(key.SheetNumber, key.Name);
      BOMJustification bomJustification = BOMJustification.Top;
      if (dictionary3[key].AlignmentSelection == "Bottom")
        bomJustification = BOMJustification.Bottom;
      bool flag4 = false;
      if (dictionary3[key].StackingSelection == "Stack")
        flag4 = true;
      ViewSheet templateSheet = key;
      bool flag5 = true;
      List<AnchorGroup> collection = new List<AnchorGroup>();
      XYZ titleBlockLocation = (XYZ) null;
      string message1 = "";
      string titleBlockName = "";
      Outline outline1 = TitleBlockGeometryAnalyzer.Analyze(templateSheet, out titleBlockLocation, out titleBlockName, out message1);
      List<TicketViewportInfo> ticketViewportInfoList = new List<TicketViewportInfo>();
      ICollection<ElementId> allViewports = templateSheet.GetAllViewports();
      List<ElementId> elementIdList = new List<ElementId>();
      List<TicketViewportInfo> source2 = new List<TicketViewportInfo>();
      int scale = key.Scale;
      foreach (ElementId id in (IEnumerable<ElementId>) allViewports)
      {
        Viewport element1 = revitDoc.GetElement(id) as Viewport;
        ElementId typeId1 = element1.GetTypeId();
        if (element1 != null)
        {
          Autodesk.Revit.DB.View element2 = revitDoc.GetElement(element1.ViewId) as Autodesk.Revit.DB.View;
          if (element2.AssociatedAssemblyInstanceId != (ElementId) null && !element2.AssociatedAssemblyInstanceId.Equals((object) ElementId.InvalidElementId))
            element2.IsolateElementTemporary(sourceAssembly.GetStructuralFramingElement().Id);
          bool view3DOrtho = false;
          if (!TicketViewportInfo.IsTicketTemplateView(element2))
          {
            ElementId typeId2 = element2.GetTypeId();
            if (!revitDoc.GetElement(typeId2).Name.Contains("3D View"))
            {
              XYZ locationPoint = new XYZ(0.0, 0.0, 0.0);
              if (titleBlockLocation != null)
                locationPoint = element1.GetBoxCenter() - titleBlockLocation;
              SectionViewInfo sectionViewInfo = new SectionViewInfo(element2, element1.Rotation, locationPoint, typeId1);
              ticketTemplate.cloneTicketSectionViewList.Add(sectionViewInfo);
              continue;
            }
            view3DOrtho = true;
          }
          string name = element2.ViewTemplateId == ElementId.InvalidElementId ? "" : revitDoc.GetElement(element2.ViewTemplateId).Name;
          bool bCompatibility = false;
          double viewCropRotation = EDGERCreateTemplate.GetSectionViewCropRotation(element2 as ViewSection, AssemblyTransform, bCompatibility);
          TicketViewportInfo ticketViewportInfo = new TicketViewportInfo(element2.Name, name, element1.Rotation, viewCropRotation, element2.Scale, view3DOrtho);
          ticketViewportInfo.SetViewOutline(element1.GetBoxOutline());
          ticketViewportInfo.SetBoxCenterPoint(element1.GetBoxCenter() - titleBlockLocation);
          ticketViewportInfo.sourceViewId = element2.Id;
          ticketViewportInfo.sourceViewPortTypeId = typeId1;
          source2.Add(ticketViewportInfo);
          if (scale == -1)
            scale = element2.Scale;
        }
      }
      foreach (TicketViewportInfo viewportInfo1 in source2.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.mSectionViewOrientation == TemplateViewOrientation.ElevationTop || s.mSectionViewOrientation == TemplateViewOrientation.ElevationBottom)).ToList<TicketViewportInfo>())
      {
        AnchorGroup anchorGroup = new AnchorGroup();
        source2.Remove(viewportInfo1);
        anchorGroup.SetView(AnchorPosition.Main, viewportInfo1);
        XYZ anchorCenterpoint = viewportInfo1.GetBoxCenterPoint();
        IEnumerable<TicketViewportInfo> source3 = source2.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && s.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom && s.mSectionViewOrientation != TemplateViewOrientation.ThreeDOrtho && TicketPopCreationUtils.WithinViewAlignmentTolerance(new XYZ(0.0, s.GetBoxCenterPoint().Y, 0.0), new XYZ(0.0, anchorCenterpoint.Y, 0.0), 0.1)));
        IEnumerable<TicketViewportInfo> source4 = (IEnumerable<TicketViewportInfo>) source3.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().X < anchorCenterpoint.X)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().X));
        if (source4.Any<TicketViewportInfo>())
        {
          TicketViewportInfo viewportInfo2 = source4.ToList<TicketViewportInfo>().Last<TicketViewportInfo>();
          if (viewportInfo2.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo2.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
          {
            anchorGroup.SetView(AnchorPosition.Left, viewportInfo2);
            source2.Remove(source4.ToList<TicketViewportInfo>().Last<TicketViewportInfo>());
          }
        }
        IEnumerable<TicketViewportInfo> source5 = (IEnumerable<TicketViewportInfo>) source3.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().X > anchorCenterpoint.X)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().X));
        if (source5.Any<TicketViewportInfo>())
        {
          TicketViewportInfo viewportInfo3 = source5.ToList<TicketViewportInfo>().First<TicketViewportInfo>();
          if (viewportInfo3.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo3.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
          {
            anchorGroup.SetView(AnchorPosition.Right, viewportInfo3);
            source2.Remove(source5.ToList<TicketViewportInfo>().First<TicketViewportInfo>());
          }
        }
        IEnumerable<TicketViewportInfo> source6 = source2.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && s.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom && s.mSectionViewOrientation != TemplateViewOrientation.ThreeDOrtho && TicketPopCreationUtils.WithinViewAlignmentTolerance(new XYZ(0.0, s.GetBoxCenterPoint().X, 0.0), new XYZ(0.0, anchorCenterpoint.X, 0.0), 0.1)));
        IEnumerable<TicketViewportInfo> source7 = (IEnumerable<TicketViewportInfo>) source6.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().Y > anchorCenterpoint.Y)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().Y));
        if (source7.Any<TicketViewportInfo>())
        {
          TicketViewportInfo viewportInfo4 = source7.ToList<TicketViewportInfo>().First<TicketViewportInfo>();
          if (viewportInfo4.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo4.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
          {
            anchorGroup.SetView(AnchorPosition.Top, viewportInfo4);
            source2.Remove(source7.ToList<TicketViewportInfo>().First<TicketViewportInfo>());
          }
        }
        IEnumerable<TicketViewportInfo> source8 = (IEnumerable<TicketViewportInfo>) source6.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().Y < anchorCenterpoint.Y)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().Y));
        if (source8.Any<TicketViewportInfo>())
        {
          TicketViewportInfo viewportInfo5 = source8.ToList<TicketViewportInfo>().Last<TicketViewportInfo>();
          if (viewportInfo5.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo5.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
          {
            anchorGroup.SetView(AnchorPosition.Bottom, viewportInfo5);
            source2.Remove(source8.ToList<TicketViewportInfo>().Last<TicketViewportInfo>());
          }
        }
        Outline outline2 = anchorGroup.GetOutline();
        if (PopulatorQA.inHouse)
          TemplateDebugUtils.DebugDrawOutline(outline2, revitDoc, (Autodesk.Revit.DB.View) templateSheet);
        XYZ RevitVector = EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline2) - EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1);
        anchorGroup.simpleVectorToUperLeft = new SimpleVector(RevitVector);
        collection.Add(anchorGroup);
      }
      foreach (TicketViewportInfo viewportInfo in source2)
      {
        AnchorGroup anchorGroup = new AnchorGroup();
        anchorGroup.SetView(AnchorPosition.Main, viewportInfo);
        viewportInfo.GetBoxCenterPoint();
        Outline outline3 = anchorGroup.GetOutline();
        if (PopulatorQA.inHouse)
          TemplateDebugUtils.DebugDrawOutline(outline3, revitDoc, (Autodesk.Revit.DB.View) templateSheet);
        XYZ RevitVector = EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline3) - EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1);
        anchorGroup.simpleVectorToUperLeft = new SimpleVector(RevitVector);
        collection.Add(anchorGroup);
      }
      ticketTemplate.AnchorGroups.AddRange((IEnumerable<AnchorGroup>) collection);
      ticketTemplate.TemplateName = sheetTitle;
      ticketTemplate.TemplateScale = scale;
      ticketTemplate.ScaleUnits = ScalesManager.GetScaleUnitsForDocument(revitDoc);
      ticketTemplate.TitleBlockName = titleBlockName;
      ticketTemplate.TitleLocationToUpperLeftCorner = new SimpleVector(EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1) - titleBlockLocation);
      ticketTemplate.BOMJustification = bomJustification;
      ticketTemplate.bStackSchedules = flag4;
      ticketTemplate.titleBlockLocationPoint = titleBlockLocation;
      XYZ outlineUpperLeftCorner = EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1);
      foreach (ElementId id in (IEnumerable<ElementId>) allViewports)
      {
        if (revitDoc.GetElement(id) is Viewport element3 && revitDoc.GetElement(element3.ViewId) is Autodesk.Revit.DB.View element4 && element4.ViewType == ViewType.Legend)
        {
          SimpleVector vectorToUpperLeft = new SimpleVector(EDGERCreateTemplate.GetOutlineUpperLeftCorner(element3.GetBoxOutline()) - outlineUpperLeftCorner);
          bool strandPatternTemplate = element4.Name.ToUpper().Contains("END PATTERN TEMPLATE") || element4.Name.ToUpper().Contains("STRAND PATTERN TEMPLATE") || element4.Name.ToUpper().Contains("REINFORCING PATTERN TEMPLATE");
          ticketTemplate.LegendInfos.Add(new TicketLegendInfo(element4.Name, vectorToUpperLeft, strandPatternTemplate));
        }
      }
      IEnumerable<ScheduleSheetInstance> source9 = new FilteredElementCollector(revitDoc, templateSheet.Id).OfClass(typeof (ScheduleSheetInstance)).ToElements().Cast<ScheduleSheetInstance>();
      bool Conforms;
      if (source9.Any<ScheduleSheetInstance>())
      {
        if (!ticketTemplate.bStackSchedules)
        {
          foreach (ScheduleSheetInstance scheduleSheetInstance in source9)
          {
            ScheduleSheetInstance ssInstance = scheduleSheetInstance;
            TicketScheduleInfo ticketScheduleInfo = new TicketScheduleInfo();
            ticketScheduleInfo.SetBOMScheduleNameStringFromScheduleName(ssInstance.Name, sourceAssembly.Name, out Conforms);
            flag5 &= Conforms;
            bool flag6 = false;
            ViewSchedule element = revitDoc.GetElement(ssInstance.ScheduleId) as ViewSchedule;
            int index = scheduleNamesList.FindIndex((Predicate<TicketBOMCore.BOMScheduleName>) (x => x.GetTemplateName() == ssInstance.Name));
            if (index > 0)
            {
              string existingString = scheduleNamesList[index].GetExistingString();
              if (!string.IsNullOrWhiteSpace(existingString))
                ticketScheduleInfo.BOMScheduleNameString = existingString;
            }
            else if (!Conforms)
            {
              ScheduleDefinition definition = element.Definition;
              foreach (ScheduleFilter filter in (IEnumerable<ScheduleFilter>) definition.GetFilters())
              {
                if (definition.GetField(filter.FieldId).GetName().ToUpper() == "CONSTRUCTION_PRODUCT_HOST")
                {
                  flag6 = true;
                  break;
                }
              }
              dictionary7.Add(new Dictionary<string, ElementId>()
              {
                {
                  ssInstance.Name,
                  ssInstance.ScheduleId
                }
              }, flag6);
              ticketScheduleInfo.BOMScheduleNameString = element.Name;
            }
            else
            {
              ScheduleDefinition definition = element.Definition;
              foreach (ScheduleFilter filter in (IEnumerable<ScheduleFilter>) definition.GetFilters())
              {
                if (definition.GetField(filter.FieldId).GetName().ToUpper() == "CONSTRUCTION_PRODUCT_HOST")
                {
                  flag6 = true;
                  break;
                }
              }
              if (!flag6 && !scheduleNamesList.Select<TicketBOMCore.BOMScheduleName, string>((Func<TicketBOMCore.BOMScheduleName, string>) (x => x.GetTemplateName())).Contains<string>(ticketScheduleInfo.BOMScheduleNameString) && !scheduleNamesList.Select<TicketBOMCore.BOMScheduleName, string>((Func<TicketBOMCore.BOMScheduleName, string>) (x => x.GetExistingString())).Contains<string>(ticketScheduleInfo.BOMScheduleNameString))
              {
                dictionary7.Add(new Dictionary<string, ElementId>()
                {
                  {
                    ssInstance.Name,
                    ssInstance.ScheduleId
                  }
                }, flag6);
                ticketScheduleInfo.BOMScheduleNameString = element.Name;
              }
            }
            XYZ xyz;
            if (ticketScheduleInfo.BOMJustification == BOMJustification.Bottom)
            {
              xyz = ssInstance.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min + new XYZ(1.0 / 144.0, 1.0 / 144.0, 0.0);
            }
            else
            {
              BoundingBoxXYZ boundingBoxXyz = ssInstance.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet);
              xyz = new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z) + new XYZ(1.0 / 144.0, -1.0 / 144.0, 0.0);
            }
            ticketScheduleInfo.vectorToAnchorPoint = new SimpleVector(xyz - titleBlockLocation);
            ticketTemplate.ScheduleInfos.Add(ticketScheduleInfo);
            if (dictionary8.ContainsKey(element.Name))
            {
              dictionary8[element.Name].Add(ticketScheduleInfo);
            }
            else
            {
              List<TicketScheduleInfo> ticketScheduleInfoList = new List<TicketScheduleInfo>()
              {
                ticketScheduleInfo
              };
              dictionary8.Add(element.Name, ticketScheduleInfoList);
            }
          }
        }
        else
        {
          IEnumerable<ScheduleSheetInstance> source10 = bomJustification == BOMJustification.Top ? (IEnumerable<ScheduleSheetInstance>) source9.OrderByDescending<ScheduleSheetInstance, double>((Func<ScheduleSheetInstance, double>) (s => s.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min.Y)) : (IEnumerable<ScheduleSheetInstance>) source9.OrderBy<ScheduleSheetInstance, double>((Func<ScheduleSheetInstance, double>) (s => s.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min.Y));
          if (source10.Any<ScheduleSheetInstance>())
          {
            int num4 = 0;
            foreach (ScheduleSheetInstance scheduleSheetInstance in source10)
            {
              ScheduleSheetInstance ssInstance = scheduleSheetInstance;
              TicketScheduleInfo ticketScheduleInfo = new TicketScheduleInfo();
              ticketScheduleInfo.SetBOMScheduleNameStringFromScheduleName(ssInstance.Name, sourceAssembly.Name, out Conforms);
              flag5 &= Conforms;
              bool flag7 = false;
              ViewSchedule element = revitDoc.GetElement(ssInstance.ScheduleId) as ViewSchedule;
              int index = scheduleNamesList.FindIndex((Predicate<TicketBOMCore.BOMScheduleName>) (x => x.GetTemplateName() == ssInstance.Name));
              if (index > 0)
              {
                string existingString = scheduleNamesList[index].GetExistingString();
                if (!string.IsNullOrWhiteSpace(existingString))
                  ticketScheduleInfo.BOMScheduleNameString = existingString;
              }
              else if (!Conforms)
              {
                ScheduleDefinition definition = element.Definition;
                foreach (ScheduleFilter filter in (IEnumerable<ScheduleFilter>) definition.GetFilters())
                {
                  if (definition.GetField(filter.FieldId).GetName().ToUpper() == "CONSTRUCTION_PRODUCT_HOST")
                  {
                    flag7 = true;
                    break;
                  }
                }
                dictionary7.Add(new Dictionary<string, ElementId>()
                {
                  {
                    ssInstance.Name,
                    ssInstance.ScheduleId
                  }
                }, flag7);
                ticketScheduleInfo.BOMScheduleNameString = element.Name;
              }
              if (Conforms)
              {
                ScheduleDefinition definition = element.Definition;
                foreach (ScheduleFilter filter in (IEnumerable<ScheduleFilter>) definition.GetFilters())
                {
                  if (definition.GetField(filter.FieldId).GetName().ToUpper() == "CONSTRUCTION_PRODUCT_HOST")
                  {
                    flag7 = true;
                    break;
                  }
                }
                if (!flag7 && !scheduleNamesList.Select<TicketBOMCore.BOMScheduleName, string>((Func<TicketBOMCore.BOMScheduleName, string>) (x => x.GetTemplateName())).Contains<string>(ticketScheduleInfo.BOMScheduleNameString) && !scheduleNamesList.Select<TicketBOMCore.BOMScheduleName, string>((Func<TicketBOMCore.BOMScheduleName, string>) (x => x.GetExistingString())).Contains<string>(ticketScheduleInfo.BOMScheduleNameString))
                {
                  dictionary7.Add(new Dictionary<string, ElementId>()
                  {
                    {
                      ssInstance.Name,
                      ssInstance.ScheduleId
                    }
                  }, flag7);
                  ticketScheduleInfo.BOMScheduleNameString = element.Name;
                }
              }
              ticketScheduleInfo.iScheduleOrderIndex = num4;
              ticketTemplate.ScheduleInfos.Add(ticketScheduleInfo);
              if (dictionary8.ContainsKey(element.Name))
              {
                dictionary8[element.Name].Add(ticketScheduleInfo);
              }
              else
              {
                List<TicketScheduleInfo> ticketScheduleInfoList = new List<TicketScheduleInfo>()
                {
                  ticketScheduleInfo
                };
                dictionary8.Add(element.Name, ticketScheduleInfoList);
              }
              ++num4;
            }
            ScheduleSheetInstance scheduleSheetInstance1 = source10.First<ScheduleSheetInstance>();
            XYZ xyz;
            if (bomJustification == BOMJustification.Bottom)
            {
              xyz = scheduleSheetInstance1.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min + new XYZ(1.0 / 144.0, 1.0 / 144.0, 0.0);
            }
            else
            {
              BoundingBoxXYZ boundingBoxXyz = scheduleSheetInstance1.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet);
              xyz = new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z) + new XYZ(1.0 / 144.0, -1.0 / 144.0, 0.0);
            }
            ticketTemplate.BOMAnchorPosition = new SimpleVector(xyz - titleBlockLocation);
          }
        }
      }
      dictionary4.Add(key, ticketTemplate);
      dictionary5.Add(key, dictionary7);
      dictionary6.Add(key, dictionary8);
    }
    int num5 = (int) transaction1.RollBack();
    Dictionary<ViewSheet, List<ElementId>> dictionary9 = new Dictionary<ViewSheet, List<ElementId>>();
    Dictionary<ViewSheet, List<ElementId>> dictionary10 = new Dictionary<ViewSheet, List<ElementId>>();
    foreach (ViewSheet key1 in dictionary5.Keys)
    {
      List<ElementId> elementIdList1 = new List<ElementId>();
      List<ElementId> elementIdList2 = new List<ElementId>();
      foreach (Dictionary<string, ElementId> key2 in dictionary5[key1].Keys)
      {
        if (!dictionary5[key1][key2])
        {
          TaskDialog taskDialog3 = new TaskDialog("Clone Ticket");
          TaskDialog taskDialog4 = taskDialog3;
          string[] strArray = new string[5]
          {
            "The schedule: ",
            null,
            null,
            null,
            null
          };
          KeyValuePair<string, ElementId> keyValuePair = key2.First<KeyValuePair<string, ElementId>>();
          strArray[1] = keyValuePair.Key;
          strArray[2] = " on ";
          strArray[3] = key1.Title;
          strArray[4] = " does not have a template or a filter based on the CONSTRUCTION_PRODUCT_HOST. Should the tool clone this schedule or ignore it?";
          string str4 = string.Concat(strArray);
          taskDialog4.MainContent = str4;
          taskDialog3.CommonButtons = (TaskDialogCommonButtons) 6;
          if (taskDialog3.Show() != 6)
          {
            List<ElementId> elementIdList3 = elementIdList1;
            keyValuePair = key2.First<KeyValuePair<string, ElementId>>();
            ElementId elementId = keyValuePair.Value;
            elementIdList3.Add(elementId);
          }
        }
        else
          elementIdList2.Add(key2.First<KeyValuePair<string, ElementId>>().Value);
      }
      dictionary9.Add(key1, elementIdList1);
      dictionary10.Add(key1, elementIdList2);
    }
    ElementMulticategoryFilter multicategoryFilter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_StructuralFraming,
      BuiltInCategory.OST_Assemblies
    });
    List<ElementId> list7 = application.ActiveUIDocument.Selection.GetElementIds().ToList<ElementId>().Where<ElementId>((Func<ElementId, bool>) (eid => multicategoryFilter.PassesFilter(revitDoc.GetElement(eid)))).ToList<ElementId>();
    if (activeUiDocument.Document.IsWorkshared && dictionary4.Count > 0)
    {
      List<string> list8 = list7.Select<ElementId, AssemblyInstance>((Func<ElementId, AssemblyInstance>) (assemblyId => revitDoc.GetElement(assemblyId) as AssemblyInstance)).Select<AssemblyInstance, Element>((Func<AssemblyInstance, Element>) (assembly => assembly.GetStructuralFramingElement())).Select<Element, string>((Func<Element, string>) (sfElem => Utils.ElementUtils.Parameters.GetParameterAsString(sfElem, "DESIGN_NUMBER"))).ToList<string>();
      List<TicketLegendInfo> ticketLegendInfoList = new List<TicketLegendInfo>();
      List<ElementId> elementIdList = new List<ElementId>();
      List<string> namesToSearch = new List<string>();
      foreach (ViewSheet key in dictionary4.Keys)
      {
        if (dictionary4[key] != null)
          ticketLegendInfoList.AddRange((IEnumerable<TicketLegendInfo>) dictionary4[key].LegendInfos);
      }
      foreach (TicketLegendInfo ticketLegendInfo in ticketLegendInfoList)
      {
        if (ticketLegendInfo.IsStrandPatternTemplate)
        {
          foreach (string newValue in list8)
            namesToSearch.Add(ticketLegendInfo.LegendName.Replace("TEMPLATE", newValue));
        }
        else
          namesToSearch.Add(ticketLegendInfo.LegendName);
      }
      List<ElementId> relinquishList2;
      if (!CheckElementsOwnership.CheckOwnership("Ticket Populator", new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (view => view.ViewType == ViewType.Legend && namesToSearch.Contains(view.Name))).Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (view => view.Id)).ToList<ElementId>(), revitDoc, activeUiDocument, out relinquishList2))
      {
        if (relinquishList2.Count > 0)
        {
          RelinquishOptions generalCategories = new RelinquishOptions(false);
          generalCategories.CheckedOutElements = true;
          TransactWithCentralOptions options = new TransactWithCentralOptions();
          WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
        }
        return (Result) 1;
      }
    }
    bool bShowErrors = false;
    List<string> source11 = new List<string>();
    List<string> stringList1 = new List<string>();
    Dictionary<string, List<string>> dictionary11 = new Dictionary<string, List<string>>();
    Dictionary<string, string> dictionary12 = new Dictionary<string, string>();
    AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(revitDoc);
    LocationInFormAnalyzer locationInFormAnalyzer1 = new LocationInFormAnalyzer(sourceAssembly, 1.0);
    foreach (AssemblyInstance assemblyInstance1 in list4)
    {
      AssemblyInstance assemblyInstance = assemblyInstance1;
      Dictionary<ViewSheet, ViewSheet> dictionary13 = new Dictionary<ViewSheet, ViewSheet>();
      using (Transaction transaction2 = new Transaction(revitDoc, $"Clone Ticket: {sourceAssembly.Name} to {assemblyInstance.Name}"))
      {
        int num6 = (int) transaction2.Start();
        try
        {
          List<ViewSheet> list9 = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId.Equals((object) assemblyInstance.Id))).ToList<ViewSheet>();
          List<int> intList = new List<int>();
          int num7 = 0;
          foreach (ViewSheet viewSheet in list9)
          {
            double result;
            if (double.TryParse(viewSheet.SheetNumber, out result))
            {
              int num8 = (int) Math.Floor(result);
              intList.Add(num8);
              if (num8 > num7)
                num7 = num8;
            }
          }
          int num9;
          int num10 = num9 = num7 + 1;
          bool flag8 = false;
          foreach (ViewSheet viewSheet in list6)
          {
            List<FamilyInstance> list10 = new FilteredElementCollector(revitDoc, viewSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>();
            if (list10.Count == 0)
            {
              int num11 = (int) transaction2.RollBack();
              stringList1.Add(assemblyInstance.Name);
              dictionary12.Add(assemblyInstance.Name, "One or more of the source tickets do not contain a title block.");
              flag8 = true;
              break;
            }
            ViewSheet sheet = AssemblyViewUtils.CreateSheet(revitDoc, assemblyInstance.Id, list10.FirstOrDefault<FamilyInstance>().Symbol.Id);
            sheet.Name = assemblyInstance.Name;
            string empty = string.Empty;
            double result;
            if (double.TryParse(viewSheet.SheetNumber, out result))
            {
              int num12 = (int) Math.Floor(result);
              while (empty == string.Empty)
              {
                if (!intList.Contains(num12))
                {
                  empty = num12.ToString();
                  intList.Add(num12);
                  if (num12 == num10)
                    ++num10;
                }
                else
                  ++num12;
              }
            }
            sheet.SheetNumber = !(empty != string.Empty) ? num10++.ToString() : empty;
            Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter((Element) sheet, "IS_CLOUD_SHEET");
            if (parameter2 != null && !parameter2.IsReadOnly)
              parameter2.Set(Utils.ElementUtils.Parameters.GetParameterAsInt((Element) viewSheet, "IS_CLOUD_SHEET"));
            dictionary13.Add(viewSheet, sheet);
          }
          if (flag8)
            continue;
        }
        catch (Exception ex)
        {
          int num13 = (int) transaction2.RollBack();
          stringList1.Add(assemblyInstance.Name);
          dictionary12.Add(assemblyInstance.Name, "Unable to create a new ticket.");
          continue;
        }
        try
        {
          List<Element> invalidWeightInputsOut = new List<Element>();
          List<Element> weightParametesDoNotExistOut = new List<Element>();
          bool flag9 = false;
          foreach (ViewSheet key in dictionary13.Keys)
          {
            ViewSheet viewSheet = dictionary13[key];
            Result result = TitleblockPopCore.PopulateTicketTitleBlock(activeUiDocument, ref message, viewSheet, out invalidWeightInputsOut, out weightParametesDoNotExistOut, bShowErrors);
            bShowErrors = false;
            if (result != null)
            {
              int num14 = (int) transaction2.RollBack();
              stringList1.Add(assemblyInstance.Name);
              dictionary12.Add(assemblyInstance.Name, "Title block population failed.");
              flag9 = true;
              break;
            }
            string str5 = "";
            Parameter parameter3 = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
            if (parameter3 != null && !string.IsNullOrEmpty(parameter3.AsString()))
              str5 = parameter3.AsString();
            string str6 = string.IsNullOrEmpty(App.TBPopFolderPath) ? $"C:/EDGEforREVIT/{str5}_TitleBlock_Mapping.xml" : $"{App.TBPopFolderPath}\\{str5}_TitleBlock_Mapping.xml";
            TBParameterSettings parameterSettings = new TBParameterSettings();
            List<string> stringList2 = new List<string>()
            {
              "TKT_LENGTH",
              "DIM_LENGTH",
              "TKT_WIDTH",
              "TKT_DEPTH",
              "TKT_ARCH_VOL_1",
              "TKT_ARCH_VOL_2",
              "TKT_ARCH_VOL_3",
              "TKT_ARCH_VOL_4",
              "TKT_ARCH_VOL_TOT",
              "TKT_STRUCT_CUYDS",
              "TKT_TOTAL_REQUIRED",
              "TKT_CUYDS",
              "TKT_WT",
              "TKT_WEIGHT"
            };
            if (File.Exists(str6) && parameterSettings.LoadTicketTemplateSettings(str6))
            {
              List<DatagridItemEntry> tbParameterList = parameterSettings.TBParameterList;
              foreach (DatagridItemEntry datagridItemEntry in tbParameterList)
              {
                if (tbParameterList.Count > 0 && !stringList2.Contains(datagridItemEntry.mappingToParam))
                {
                  Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter((Element) viewSheet, datagridItemEntry.mappingToParam);
                  if (Utils.ElementUtils.Parameters.LookupParameter((Element) key, datagridItemEntry.mappingToParam) != null && parameter4 != null && !parameter4.HasValue)
                  {
                    if (parameter4.StorageType == StorageType.String)
                      parameter4.Set(Utils.ElementUtils.Parameters.GetParameterAsString((Element) key, datagridItemEntry.mappingToParam));
                    else if (parameter4.StorageType == StorageType.Integer)
                      parameter4.Set(Utils.ElementUtils.Parameters.GetParameterAsInt((Element) key, datagridItemEntry.mappingToParam));
                    else if (parameter4.StorageType == StorageType.Double)
                      parameter4.Set(Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) key, datagridItemEntry.mappingToParam));
                    else if (parameter4.StorageType == StorageType.ElementId)
                      parameter4.Set(Utils.ElementUtils.Parameters.GetParameterAsElementId((Element) key, datagridItemEntry.mappingToParam));
                  }
                }
              }
            }
          }
          if (flag9)
            continue;
        }
        catch (Exception ex)
        {
          int num15 = (int) transaction2.RollBack();
          stringList1.Add(assemblyInstance.Name);
          dictionary12.Add(assemblyInstance.Name, "Ticket title block population failed.");
          continue;
        }
        try
        {
          new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Schedules).OfClass(typeof (ViewSchedule)).ToElements().Cast<ViewSchedule>().ToList<ViewSchedule>().Select<ViewSchedule, string>((Func<ViewSchedule, string>) (x => x.Name)).ToList<string>();
          Dictionary<string, string> dictionary14 = new Dictionary<string, string>();
          bool flag10 = false;
          foreach (ViewSheet key in dictionary4.Keys)
          {
            foreach (ElementId id in dictionary9[key])
            {
              ViewSchedule schedule = revitDoc.GetElement(id) as ViewSchedule;
              foreach (TicketScheduleInfo ticketScheduleInfo in dictionary4[key].ScheduleInfos.Where<TicketScheduleInfo>((Func<TicketScheduleInfo, bool>) (x => x.BOMScheduleNameString == schedule.Name)).ToList<TicketScheduleInfo>())
                dictionary4[key].ScheduleInfos.Remove(ticketScheduleInfo);
            }
            string createSchedulesErrorMessage = string.Empty;
            if (TicketPopCreationUtils.PlaceBOM(revitDoc, application, dictionary13[key], assemblyInstance, dictionary4[key], dictionary6[key], out createSchedulesErrorMessage) != null)
            {
              int num16 = (int) transaction2.RollBack();
              flag10 = true;
              stringList1.Add(assemblyInstance.Name);
              dictionary12.Add(assemblyInstance.Name, "Unable to generate BOM schedule. ");
              break;
            }
            if (createSchedulesErrorMessage != string.Empty)
            {
              foreach (string str7 in ((IEnumerable<string>) createSchedulesErrorMessage.Split('\n')).ToList<string>())
              {
                if (dictionary11.ContainsKey(assemblyInstance.Name))
                {
                  if (!dictionary11[assemblyInstance.Name].Contains(str7))
                    dictionary11[assemblyInstance.Name].Add(str7);
                }
                else
                  dictionary11.Add(assemblyInstance.Name, new List<string>()
                  {
                    str7
                  });
              }
            }
          }
          if (flag10)
            continue;
        }
        catch (Exception ex)
        {
          int num17 = (int) transaction2.RollBack();
          stringList1.Add(assemblyInstance.Name);
          dictionary12.Add(assemblyInstance.Name, "Unable to generate and place BOM schedule.");
          continue;
        }
        Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>> dictionary15 = new Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>>();
        Dictionary<ElementId, ElementId> dictionary16 = new Dictionary<ElementId, ElementId>();
        try
        {
          List<string> stringList3 = new List<string>();
          bool flag11 = false;
          IEnumerable<Autodesk.Revit.DB.View> source12 = (IEnumerable<Autodesk.Revit.DB.View>) null;
          foreach (ViewSheet key3 in dictionary4.Keys)
          {
            IEnumerable<Element> elements1 = (IEnumerable<Element>) new FilteredElementCollector(revitDoc, key3.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements();
            Element element = elements1.Count<Element>() == 1 ? elements1.First<Element>() : (Element) null;
            XYZ xyz = element != null ? (element.Location as LocationPoint).Point : new XYZ(0.0, 0.0, 0.0);
            List<Autodesk.Revit.DB.View> viewList1 = new List<Autodesk.Revit.DB.View>();
            List<Autodesk.Revit.DB.View> viewList2 = new List<Autodesk.Revit.DB.View>();
            XYZ anchorGroupUpperLeftCorner = xyz + dictionary4[key3].TitleLocationToUpperLeftCorner;
            List<FamilyInstance> list11 = new FilteredElementCollector(revitDoc, dictionary13[key3].Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>();
            foreach (AnchorGroup anchorGroup in dictionary4[key3].AnchorGroups)
            {
              int count = viewList1.Count;
              Dictionary<string, List<string>> viewsNotCreated = new Dictionary<string, List<string>>();
              viewList1.AddRange((IEnumerable<Autodesk.Revit.DB.View>) anchorGroup.CreateViews(assemblyInstance, -1, revitDoc, anchorGroupUpperLeftCorner, dictionary13[key3].Id, out viewsNotCreated, dictionary4[key3].TemplateScale));
              XYZ point = (list11.FirstOrDefault<FamilyInstance>().Location as LocationPoint).Point;
              if (viewList1.Count > count)
              {
                anchorGroup.PositionGroupCloneTicket(point);
                anchorGroup.copyViewParameters(revitDoc);
                Dictionary<ElementId, ElementId> sourceToTargetMapping = anchorGroup.getSourceToTargetMapping();
                foreach (ElementId key4 in sourceToTargetMapping.Keys)
                  dictionary16.Add(key4, sourceToTargetMapping[key4]);
              }
            }
            bool flag12 = false;
            List<ElementId> elementIdList = new List<ElementId>();
            foreach (SectionViewInfo ticketSectionView in dictionary4[key3].cloneTicketSectionViewList)
            {
              ViewSection viewSection = (ViewSection) null;
              ElementId viewId = ElementId.InvalidElementId;
              if (ticketSectionView.sourceViewAssociatedAssemblyId != (ElementId) null && !ticketSectionView.sourceViewAssociatedAssemblyId.Equals((object) ElementId.InvalidElementId))
              {
                bool failed;
                viewSection = this.createCustomSectionCut(revitDoc, assemblyInstance, ticketSectionView, out failed);
                if (viewSection == null)
                {
                  if (failed)
                  {
                    int num18 = (int) transaction2.RollBack();
                    stringList1.Add(assemblyInstance.Name);
                    dictionary12.Add(assemblyInstance.Name, $"Unable to clone the view, {ticketSectionView.sourceViewName}, from the source.");
                    flag12 = true;
                    break;
                  }
                  continue;
                }
                viewId = viewSection.Id;
              }
              else if (ticketSectionView.sourceViewName.ToUpper().Contains("END PATTERN TEMPLATE") || ticketSectionView.sourceViewName.ToUpper().Contains("STRAND PATTERN TEMPLATE") || ticketSectionView.sourceViewName.ToUpper().Contains("REINFORCING PATTERN TEMPLATE"))
              {
                if (source12 == null)
                  source12 = new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (view => view.ViewType == ViewType.Legend));
                Element structuralFramingElement = Utils.AssemblyUtils.Parameters.GetStructuralFramingElement(assemblyInstance);
                if (structuralFramingElement != null)
                {
                  string designNumber = Utils.ElementUtils.Parameters.GetParameterAsString(structuralFramingElement, "DESIGN_NUMBER");
                  if (!string.IsNullOrEmpty(designNumber))
                  {
                    IEnumerable<Autodesk.Revit.DB.View> source13 = source12.Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (view => this.StrandPatternMatchesDesignNumber(view.Name, designNumber)));
                    if (source13.Any<Autodesk.Revit.DB.View>())
                      viewId = source13.First<Autodesk.Revit.DB.View>().Id;
                    else
                      continue;
                  }
                  else
                    continue;
                }
              }
              else if (ticketSectionView.sourceViewName == "INSULATION DETAIL - " + sourceAssembly.Name)
              {
                Autodesk.Revit.DB.View view = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Views).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (x => x.ViewType == ViewType.Legend && x.Name == "INSULATION DETAIL - " + assemblyInstance.Name)).FirstOrDefault<Autodesk.Revit.DB.View>();
                if (view != null)
                  viewId = view.Id;
                else
                  continue;
              }
              else
                viewId = ticketSectionView.sourceViewId;
              if (viewId == ElementId.InvalidElementId)
              {
                int num19 = (int) transaction2.RollBack();
                stringList1.Add(assemblyInstance.Name);
                dictionary12.Add(assemblyInstance.Name, $"Unable to clone the view, {ticketSectionView.sourceViewName}, from the source.");
                flag12 = true;
                break;
              }
              if (!elementIdList.Contains(viewId))
              {
                Viewport target = Viewport.Create(revitDoc, dictionary13[key3].Id, viewId, new XYZ(0.0, 0.0, 0.0));
                elementIdList.Add(viewId);
                if (target == null)
                {
                  int num20 = (int) transaction2.RollBack();
                  stringList1.Add(assemblyInstance.Name);
                  dictionary12.Add(assemblyInstance.Name, $"Unable to clone the view, {ticketSectionView.sourceViewName}, from the source.");
                  flag12 = true;
                  break;
                }
                target.ChangeTypeId(ticketSectionView.viewPortTypeId);
                XYZ point = (list11.FirstOrDefault<FamilyInstance>().Location as LocationPoint).Point;
                target.Rotation = ticketSectionView.RotationOnSheet;
                target.SetBoxCenter(point + ticketSectionView.locationPoint);
                if (viewSection != null)
                {
                  EDGE.TicketTools.CloneTicket.CloneTicket.copyViewParameters(revitDoc, ticketSectionView.sourceViewId, target);
                  viewList1.Add((Autodesk.Revit.DB.View) viewSection);
                  dictionary16.Add(ticketSectionView.sourceViewId, viewSection.Id);
                }
              }
            }
            if (flag12)
            {
              flag11 = true;
              break;
            }
            dictionary15.Add(key3, viewList1);
            foreach (Autodesk.Revit.DB.View view in viewList1)
              stringList3.Add(view.Name);
          }
          if (flag11)
            continue;
        }
        catch (Exception ex)
        {
          int num21 = (int) transaction2.RollBack();
          stringList1.Add(assemblyInstance.Name);
          dictionary12.Add(assemblyInstance.Name, "Unable to clone one or more views from the source.");
          continue;
        }
        try
        {
          foreach (ViewSheet key in dictionary13.Keys)
            EDGE.TicketTools.CopyTicketViews.CopyTicketViews.copyTicketViews(key, dictionary13[key], true);
        }
        catch (Exception ex)
        {
          int num22 = (int) transaction2.RollBack();
          stringList1.Add(assemblyInstance.Name);
          dictionary12.Add(assemblyInstance.Name, "Failed to clone annotations on the ticket.");
          continue;
        }
        try
        {
          bool flag13 = false;
          foreach (ElementId key in dictionary16.Keys)
          {
            Autodesk.Revit.DB.View element5 = revitDoc.GetElement(key) as Autodesk.Revit.DB.View;
            Autodesk.Revit.DB.View element6 = revitDoc.GetElement(dictionary16[key]) as Autodesk.Revit.DB.View;
            if (!this.copyAnnotations(revitDoc, (Element) element5, (Element) element6))
            {
              int num23 = (int) transaction2.RollBack();
              stringList1.Add(assemblyInstance.Name);
              dictionary12.Add(assemblyInstance.Name, $"Failed to clone view annotations from {element5.Name} to {element6.Name}.");
              flag13 = true;
              break;
            }
          }
          if (!flag13)
          {
            foreach (ViewSheet key in dictionary13.Keys)
            {
              if (!this.copyAnnotations(revitDoc, (Element) key, (Element) dictionary13[key], true))
              {
                int num24 = (int) transaction2.RollBack();
                stringList1.Add(assemblyInstance.Name);
                dictionary12.Add(assemblyInstance.Name, $"Failed to clone ticket annotations from {key.Name} to {dictionary13[key].Name}.");
                flag13 = true;
                break;
              }
            }
            if (flag13)
              continue;
          }
          else
            continue;
        }
        catch (Exception ex)
        {
          int num25 = (int) transaction2.RollBack();
          stringList1.Add(assemblyInstance.Name);
          dictionary12.Add(assemblyInstance.Name, "Failed to clone annotations.");
          continue;
        }
        try
        {
          List<AutoTicketAppendStringParameterData> appendStringData;
          List<AutoTicketSettingsTools> ticketSettingsToolsList2 = AutoTicketSettingsReader.ReaderforAutoTicketSettings(revitDoc, "AUTO-TICKET GENERATION", false, out appendStringData);
          CalloutStyle autoTicketStyleSettings = (CalloutStyle) null;
          AutoTicketAppendString ticketAppendString = (AutoTicketAppendString) null;
          AutoTicketMinimumDimension minimumDimension = (AutoTicketMinimumDimension) null;
          AutoTicketCustomValues ticketCustomValues = (AutoTicketCustomValues) null;
          LocationInFormAnalyzer locationInFormAnalyzer2 = new LocationInFormAnalyzer(assemblyInstance, 1.0);
          SortingInfoCloneTicketElement key5 = new SortingInfoCloneTicketElement("STRUCT FRAMING", FormLocation.None);
          if (ticketSettingsToolsList2 != null)
          {
            foreach (AutoTicketSettingsTools ticketSettingsTools in ticketSettingsToolsList2)
            {
              if (ticketSettingsTools is AutoTicketCalloutAndDimensionTexts)
              {
                AutoTicketCalloutAndDimensionTexts andDimensionTexts = ticketSettingsTools as AutoTicketCalloutAndDimensionTexts;
                autoTicketStyleSettings = new CalloutStyle(revitDoc, AutoTicketEnhancementWorkflow.AutoTicketGeneration, andDimensionTexts.CalloutFamily, andDimensionTexts.OverallDimension, andDimensionTexts.GeneralDimension, andDimensionTexts.TextStyle, andDimensionTexts.UseEQ);
              }
              if (ticketSettingsTools is AutoTicketAppendString)
                ticketAppendString = ticketSettingsTools as AutoTicketAppendString;
              if (ticketSettingsTools is AutoTicketMinimumDimension)
                minimumDimension = ticketSettingsTools as AutoTicketMinimumDimension;
              if (ticketSettingsTools is AutoTicketCustomValues)
                ticketCustomValues = ticketSettingsTools as AutoTicketCustomValues;
            }
          }
          bool flag14 = false;
          Element structuralFramingElement1 = sourceAssembly.GetStructuralFramingElement();
          Element structuralFramingElement2 = assemblyInstance.GetStructuralFramingElement();
          bool bSymbol1;
          List<Solid> symbolSolids1 = Solids.GetSymbolSolids(structuralFramingElement1, out bSymbol1, options: new Options()
          {
            ComputeReferences = true
          });
          bool bSymbol2;
          List<Solid> symbolSolids2 = Solids.GetSymbolSolids(structuralFramingElement2, out bSymbol2, options: new Options()
          {
            ComputeReferences = true
          });
          foreach (ElementId key6 in dictionary16.Keys)
          {
            Autodesk.Revit.DB.View element7 = revitDoc.GetElement(key6) as Autodesk.Revit.DB.View;
            Autodesk.Revit.DB.View element8 = revitDoc.GetElement(dictionary16[key6]) as Autodesk.Revit.DB.View;
            List<Element> list12 = new FilteredElementCollector(revitDoc, element8.Id).OfCategory(BuiltInCategory.OST_SpecialityEquipment).Where<Element>((Func<Element, bool>) (elem => !Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).Symbol.FamilyName != "CONNECTOR_COMPONENT")).ToList<Element>();
            list12.AddRange((IEnumerable<Element>) new FilteredElementCollector(revitDoc, element8.Id).OfCategory(BuiltInCategory.OST_GenericModel).ToList<Element>());
            Dictionary<SortingInfoCloneTicketElement, List<Element>> targetLikeElementDictionary = new Dictionary<SortingInfoCloneTicketElement, List<Element>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
            foreach (Element element9 in list12)
            {
              SortingInfoCloneTicketElement elementSortingObject = this.getElementSortingObject(element9, locationInFormValues, locationInFormAnalyzer2);
              if (elementSortingObject != (SortingInfoCloneTicketElement) null)
              {
                if (targetLikeElementDictionary.ContainsKey(elementSortingObject))
                  targetLikeElementDictionary[elementSortingObject].Add(element9);
                else
                  targetLikeElementDictionary.Add(elementSortingObject, new List<Element>()
                  {
                    element9
                  });
              }
            }
            targetLikeElementDictionary.Add(key5, new FilteredElementCollector(revitDoc, element8.Id).OfCategory(BuiltInCategory.OST_StructuralFraming).ToList<Element>());
            List<Element> list13 = new FilteredElementCollector(revitDoc, element7.Id).OfCategory(BuiltInCategory.OST_SpecialityEquipment).Where<Element>((Func<Element, bool>) (elem => !Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).Symbol.FamilyName != "CONNECTOR_COMPONENT")).ToList<Element>();
            list13.AddRange((IEnumerable<Element>) new FilteredElementCollector(revitDoc, element7.Id).OfCategory(BuiltInCategory.OST_GenericModel).ToList<Element>());
            List<ElementId> list14 = sourceAssembly.GetMemberIds().ToList<ElementId>();
            List<Element> elementList1 = new List<Element>();
            List<Element> list15 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) list14).OfCategory(BuiltInCategory.OST_SpecialityEquipment).Where<Element>((Func<Element, bool>) (elem => !Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).Symbol.FamilyName != "CONNECTOR_COMPONENT")).ToList<Element>();
            list15.AddRange((IEnumerable<Element>) new FilteredElementCollector(revitDoc, (ICollection<ElementId>) list14).OfCategory(BuiltInCategory.OST_GenericModel).ToList<Element>());
            List<ElementId> list16 = list13.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>();
            foreach (Element element10 in list15)
            {
              if (element10.IsHidden(element7) && !list16.Contains(element10.Id))
                list13.Add(element10);
            }
            Dictionary<SortingInfoCloneTicketElement, List<Element>> sourceLikeElementDictionary = new Dictionary<SortingInfoCloneTicketElement, List<Element>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
            foreach (Element element11 in list13)
            {
              SortingInfoCloneTicketElement elementSortingObject = this.getElementSortingObject(element11, locationInFormValues, locationInFormAnalyzer1);
              if (elementSortingObject != (SortingInfoCloneTicketElement) null && targetLikeElementDictionary.ContainsKey(elementSortingObject))
              {
                if (sourceLikeElementDictionary.ContainsKey(elementSortingObject))
                  sourceLikeElementDictionary[elementSortingObject].Add(element11);
                else
                  sourceLikeElementDictionary.Add(elementSortingObject, new List<Element>()
                  {
                    element11
                  });
              }
            }
            sourceLikeElementDictionary.Add(key5, new FilteredElementCollector(revitDoc, element7.Id).OfCategory(BuiltInCategory.OST_StructuralFraming).ToList<Element>());
            Dictionary<DimensionEdge, List<SortingInfoCloneTicketElement>> dictionary17 = new Dictionary<DimensionEdge, List<SortingInfoCloneTicketElement>>();
            dictionary17.Add(DimensionEdge.Top, new List<SortingInfoCloneTicketElement>());
            dictionary17.Add(DimensionEdge.Bottom, new List<SortingInfoCloneTicketElement>());
            dictionary17.Add(DimensionEdge.Right, new List<SortingInfoCloneTicketElement>());
            dictionary17.Add(DimensionEdge.Left, new List<SortingInfoCloneTicketElement>());
            List<Dimension> list17 = new FilteredElementCollector(revitDoc, element7.Id).OfCategory(BuiltInCategory.OST_Dimensions).Cast<Dimension>().ToList<Dimension>();
            Dictionary<DimensionEdge, List<Dimension>> dictionary18 = new Dictionary<DimensionEdge, List<Dimension>>();
            dictionary18.Add(DimensionEdge.Top, new List<Dimension>());
            dictionary18.Add(DimensionEdge.Bottom, new List<Dimension>());
            dictionary18.Add(DimensionEdge.Left, new List<Dimension>());
            dictionary18.Add(DimensionEdge.Right, new List<Dimension>());
            XYZ rightDirection = element7.RightDirection;
            XYZ upDirection = element7.UpDirection;
            BoundingBoxXYZ sourceBoundingBox = structuralFramingElement1.get_BoundingBox(element7);
            BoundingBoxXYZ targetBoundingBox = structuralFramingElement2.get_BoundingBox(element8);
            Transform viewTransform1 = Utils.ViewUtils.ViewUtils.GetViewTransform(element7);
            Transform viewTransform2 = Utils.ViewUtils.ViewUtils.GetViewTransform(element8);
            Dictionary<ElementId, StructuralFramingBoundsObject> boundsObjectDictOut1 = new Dictionary<ElementId, StructuralFramingBoundsObject>();
            Dictionary<ElementId, StructuralFramingBoundsObject> boundsObjectDictOut2 = new Dictionary<ElementId, StructuralFramingBoundsObject>();
            StructuralFramingBoundsObject sourceBoundsObject1;
            if (boundsObjectDictOut1.ContainsKey(structuralFramingElement1.Id))
            {
              sourceBoundsObject1 = boundsObjectDictOut1[structuralFramingElement1.Id];
            }
            else
            {
              sourceBoundsObject1 = new StructuralFramingBoundsObject(structuralFramingElement1 as FamilyInstance, element7, true);
              boundsObjectDictOut1.Add(structuralFramingElement1.Id, sourceBoundsObject1);
            }
            StructuralFramingBoundsObject targetBoundsObject1;
            if (boundsObjectDictOut2.ContainsKey(structuralFramingElement2.Id))
            {
              targetBoundsObject1 = boundsObjectDictOut2[structuralFramingElement2.Id];
            }
            else
            {
              targetBoundsObject1 = new StructuralFramingBoundsObject(structuralFramingElement2 as FamilyInstance, element8, true);
              boundsObjectDictOut2.Add(structuralFramingElement2.Id, targetBoundsObject1);
            }
            foreach (Dimension dimension in list17)
            {
              if (dimension.Curve is Line)
              {
                Line curve = dimension.Curve as Line;
                XYZ xyz = viewTransform1.Inverse.OfPoint(curve.Origin);
                if (curve.Direction.IsAlmostEqualTo(rightDirection) || curve.Direction.IsAlmostEqualTo(rightDirection.Negate()))
                {
                  double num26 = (sourceBoundsObject1.yMax + sourceBoundsObject1.yMin) / 2.0;
                  if (xyz.Y >= num26)
                    dictionary18[DimensionEdge.Top].Add(dimension);
                  else
                    dictionary18[DimensionEdge.Bottom].Add(dimension);
                }
                else if (curve.Direction.IsAlmostEqualTo(upDirection) || curve.Direction.IsAlmostEqualTo(upDirection.Negate()))
                {
                  double num27 = (sourceBoundsObject1.xMax + sourceBoundsObject1.xMin) / 2.0;
                  if (xyz.X >= num27)
                    dictionary18[DimensionEdge.Right].Add(dimension);
                  else
                    dictionary18[DimensionEdge.Left].Add(dimension);
                }
              }
            }
            List<ReferencedPoint> targetTopReferences = new List<ReferencedPoint>();
            List<ReferencedPoint> source14 = new List<ReferencedPoint>();
            List<ReferencedPoint> source15 = new List<ReferencedPoint>();
            List<ReferencedPoint> targetLeftReferences = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList3 = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList4 = new List<ReferencedPoint>();
            List<ReferencedPoint> refPoints1 = new List<ReferencedPoint>();
            List<ReferencedPoint> refPoints2 = new List<ReferencedPoint>();
            List<ReferencedPoint> refPoints3 = new List<ReferencedPoint>();
            List<ReferencedPoint> refPoints4 = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList5 = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList6 = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList7 = new List<ReferencedPoint>();
            List<ReferencedPoint> referencedPointList8 = new List<ReferencedPoint>();
            double upDownMidpointTarget;
            double leftRightMidpointTarget;
            double upDownMidpointSource;
            double leftRightMidpointSource;
            this.getViewsOrientationAndMidPointData(targetBoundsObject1, sourceBoundsObject1, out upDownMidpointTarget, out leftRightMidpointTarget, out upDownMidpointSource, out leftRightMidpointSource);
            Dictionary<DimensionEdge, ReferencedPoint> dictionary19 = new Dictionary<DimensionEdge, ReferencedPoint>();
            Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> dictionary20 = new Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
            Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> dictionary21 = new Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
            foreach (SortingInfoCloneTicketElement key7 in targetLikeElementDictionary.Keys)
            {
              List<Element> top;
              List<Element> bottom;
              List<Element> right;
              List<Element> left;
              List<List<ElementId>> alignmentGroupingsHorizontal;
              List<List<ElementId>> alignmentGroupingsVertical;
              this.midPointSorting(targetLikeElementDictionary[key7], element8, viewTransform2, upDownMidpointTarget, leftRightMidpointTarget, boundsObjectDictOut2, out top, out bottom, out right, out left, num1, out alignmentGroupingsHorizontal, out alignmentGroupingsVertical, out boundsObjectDictOut2);
              dictionary20.Add(key7, alignmentGroupingsHorizontal);
              dictionary21.Add(key7, alignmentGroupingsVertical);
              foreach (Element element12 in top)
              {
                Element elem = element12;
                if (key7 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Top, element8);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  targetTopReferences.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Top, element8, symbolSolids2, bSymbol2, num1);
                  framingReferencesList.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  referencedPointList1.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
              foreach (Element element13 in bottom)
              {
                Element elem = element13;
                if (key7 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Bottom, element8);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  source14.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Bottom, element8, symbolSolids2, bSymbol2, num1);
                  framingReferencesList.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  referencedPointList2.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
              foreach (Element element14 in right)
              {
                Element elem = element14;
                if (key7 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Right, element8);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  source15.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Right, element8, symbolSolids2, bSymbol2, num1);
                  framingReferencesList.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  referencedPointList3.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
              foreach (Element element15 in left)
              {
                Element elem = element15;
                if (key7 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Left, element8);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  targetLeftReferences.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Left, element8, symbolSolids2, bSymbol2, num1);
                  framingReferencesList.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  referencedPointList4.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
            }
            Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> dictionary22 = new Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
            Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> dictionary23 = new Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
            foreach (SortingInfoCloneTicketElement key8 in sourceLikeElementDictionary.Keys)
            {
              List<Element> top;
              List<Element> bottom;
              List<Element> right;
              List<Element> left;
              List<List<ElementId>> alignmentGroupingsHorizontal;
              List<List<ElementId>> alignmentGroupingsVertical;
              this.midPointSorting(sourceLikeElementDictionary[key8], element7, viewTransform1, upDownMidpointSource, leftRightMidpointSource, boundsObjectDictOut1, out top, out bottom, out right, out left, num1, out alignmentGroupingsHorizontal, out alignmentGroupingsVertical, out boundsObjectDictOut1);
              dictionary22.Add(key8, alignmentGroupingsHorizontal);
              dictionary23.Add(key8, alignmentGroupingsVertical);
              foreach (Element element16 in top)
              {
                Element elem = element16;
                if (key8 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Top, element7);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  refPoints1.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Top, element7, symbolSolids1, bSymbol1, num1);
                  framingReferencesList.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  referencedPointList5.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
              foreach (Element element17 in bottom)
              {
                Element elem = element17;
                if (key8 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Bottom, element7);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  refPoints2.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Bottom, element7, symbolSolids1, bSymbol1, num1);
                  referencedPointList6.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
              foreach (Element element18 in right)
              {
                Element elem = element18;
                if (key8 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Right, element7);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  refPoints3.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Right, element7, symbolSolids1, bSymbol1, num1);
                  framingReferencesList.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  referencedPointList7.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
              foreach (Element element19 in left)
              {
                Element elem = element19;
                if (key8 != key5)
                {
                  List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, DimensionEdge.Left, element7);
                  dimLineReference.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  refPoints4.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
                }
                else
                {
                  List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, DimensionEdge.Left, element7, symbolSolids1, bSymbol1, num1);
                  framingReferencesList.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                  referencedPointList8.AddRange((IEnumerable<ReferencedPoint>) framingReferencesList);
                }
              }
            }
            List<ReferencedPoint> source16 = this.sortReferencedPointList(this.checkReferencedPointsForLocalPoint(refPoints1, viewTransform1), DimensionEdge.Top);
            List<ReferencedPoint> source17 = this.sortReferencedPointList(this.checkReferencedPointsForLocalPoint(refPoints2, viewTransform1), DimensionEdge.Bottom);
            List<ReferencedPoint> source18 = this.sortReferencedPointList(this.checkReferencedPointsForLocalPoint(refPoints3, viewTransform2), DimensionEdge.Right);
            List<ReferencedPoint> source19 = this.sortReferencedPointList(this.checkReferencedPointsForLocalPoint(refPoints4, viewTransform2), DimensionEdge.Left);
            List<DimensionInfo> source20 = new List<DimensionInfo>();
            Dictionary<DimensionEdge, List<ReferencedPoint>> referncePlanesAndLines1 = this.GetElementsReferncePlanesAndLines(revitDoc, structuralFramingElement1, element7, viewTransform1);
            Dictionary<DimensionEdge, List<ReferencedPoint>> referncePlanesAndLines2 = this.GetElementsReferncePlanesAndLines(revitDoc, structuralFramingElement2, element8, viewTransform2);
            foreach (DimensionEdge key9 in dictionary18.Keys)
            {
              DimensionEdge dimensionKey = key9;
              List<List<Dimension>> dimensionListList = this.groupDimensions(dictionary18[dimensionKey], viewTransform1, dimensionKey);
              foreach (Dimension dimension in dictionary18[dimensionKey])
              {
                bool allSFrefs = true;
                int sfCounter = 0;
                foreach (Reference reference in dimension.References)
                {
                  if (reference.ElementId != structuralFramingElement1.Id)
                    allSFrefs = false;
                  ++sfCounter;
                }
                Dictionary<ReferencedPoint, List<ReferencedPoint>> source21 = new Dictionary<ReferencedPoint, List<ReferencedPoint>>();
                List<ReferencedPoint> referencedPointList9 = new List<ReferencedPoint>();
                Dictionary<ElementId, ElementReferenceInfo> dictionary24 = new Dictionary<ElementId, ElementReferenceInfo>();
                Parameter parameter5 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimension, BuiltInParameter.LINEAR_DIM_TYPE);
                bool flag15 = false;
                if (parameter5 != null && parameter5.AsValueString() == "Ordinate")
                  flag15 = true;
                List<XYZ> xyzList = new List<XYZ>();
                if (dimension.NumberOfSegments == 0)
                {
                  XYZ origin = dimension.Origin;
                  XYZ xyz1 = viewTransform1.Inverse.OfPoint(origin);
                  if (!flag15)
                  {
                    double? nullable = dimension.Value;
                    double num28 = 2.0;
                    double num29 = (nullable.HasValue ? new double?(nullable.GetValueOrDefault() / num28) : new double?()).Value;
                    if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                    {
                      XYZ xyz2 = new XYZ(xyz1.X - num29, xyz1.Y, xyz1.Z);
                      xyzList.Add(xyz2);
                      XYZ xyz3 = new XYZ(xyz1.X + num29, xyz1.Y, xyz1.Z);
                      xyzList.Add(xyz3);
                    }
                    else
                    {
                      XYZ xyz4 = new XYZ(xyz1.X, xyz1.Y - num29, xyz1.Z);
                      xyzList.Add(xyz4);
                      XYZ xyz5 = new XYZ(xyz1.X, xyz1.Y + num29, xyz1.Z);
                      xyzList.Add(xyz5);
                    }
                  }
                  else
                  {
                    xyzList.Add(xyz1);
                    if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                    {
                      XYZ xyz6 = new XYZ(xyz1.X + dimension.Value.Value, xyz1.Y, xyz1.Z);
                      xyzList.Add(xyz6);
                    }
                    else
                    {
                      XYZ xyz7 = new XYZ(xyz1.X, xyz1.Y + dimension.Value.Value, xyz1.Z);
                      xyzList.Add(xyz7);
                    }
                  }
                }
                foreach (DimensionSegment segment in dimension.Segments)
                {
                  XYZ origin = segment.Origin;
                  XYZ xyz = viewTransform1.Inverse.OfPoint(origin);
                  if (!flag15)
                  {
                    double? nullable1 = segment.Value;
                    double num30 = 0.0;
                    if (!(nullable1.GetValueOrDefault() == num30 & nullable1.HasValue))
                    {
                      double? nullable2 = segment.Value;
                      double num31 = 2.0;
                      double num32 = (nullable2.HasValue ? new double?(nullable2.GetValueOrDefault() / num31) : new double?()).Value;
                      if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                      {
                        XYZ returnLocal2 = new XYZ(xyz.X - num32, xyz.Y, xyz.Z);
                        if (!xyzList.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(returnLocal2))).Any<XYZ>())
                          xyzList.Add(returnLocal2);
                        XYZ returnLocal = new XYZ(xyz.X + num32, xyz.Y, xyz.Z);
                        if (!xyzList.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(returnLocal))).Any<XYZ>())
                          xyzList.Add(returnLocal);
                      }
                      else
                      {
                        XYZ returnLocal2 = new XYZ(xyz.X, xyz.Y - num32, xyz.Z);
                        if (!xyzList.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(returnLocal2))).Any<XYZ>())
                          xyzList.Add(returnLocal2);
                        XYZ returnLocal = new XYZ(xyz.X, xyz.Y + num32, xyz.Z);
                        if (!xyzList.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(returnLocal))).Any<XYZ>())
                          xyzList.Add(returnLocal);
                      }
                    }
                  }
                  else
                    xyzList.Add(xyz);
                }
                Dictionary<ElementId, List<ReferencedPoint>> dictionary25 = new Dictionary<ElementId, List<ReferencedPoint>>();
                Dictionary<GeometryObject, List<XYZ>> previouslyUsedDict = new Dictionary<GeometryObject, List<XYZ>>();
                bool overall = false;
                bool lowerExtent = false;
                bool flag16 = false;
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences = DimensioningGeometryUtils.GetDimensioningPointsReferences(structuralFramingElement1 as FamilyInstance, element7);
                DimensionEdge key10 = dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom ? DimensionEdge.Left : DimensionEdge.Bottom;
                DimensionEdge key11;
                switch (key10)
                {
                  case DimensionEdge.Left:
                    key11 = DimensionEdge.Right;
                    break;
                  case DimensionEdge.Right:
                    key11 = DimensionEdge.Left;
                    break;
                  case DimensionEdge.Top:
                    key11 = DimensionEdge.Bottom;
                    break;
                  case DimensionEdge.Bottom:
                    key11 = DimensionEdge.Top;
                    break;
                  default:
                    key11 = DimensionEdge.None;
                    break;
                }
                ReferencedPoint referencedPoint1 = pointsReferences[dimensionKey][key10];
                ReferencedPoint referencedPoint2 = pointsReferences[dimensionKey][key11];
                if (referencedPoint1.LocalPoint.IsAlmostEqualTo(referencedPoint2.LocalPoint))
                {
                  string str8 = "Unable to correctly identify edges of the structural framing element to dimension. Curved geometry can result in incorrect dimensions and errors. ";
                  if (dictionary11.ContainsKey(assemblyInstance.Name))
                  {
                    if (!dictionary11[assemblyInstance.Name].Contains(str8))
                      dictionary11[assemblyInstance.Name].Add(str8);
                  }
                  else
                    dictionary11.Add(assemblyInstance.Name, new List<string>()
                    {
                      str8
                    });
                }
                Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>> dictionary26 = DimReferencesManager.InstrumentSFContours(revitDoc, structuralFramingElement1.Id, element7);
                List<ReferencedPoint> source22 = new List<ReferencedPoint>();
                if (dictionary26 != null)
                {
                  foreach (int key12 in dictionary26.Keys)
                  {
                    if (key12 > -1)
                      source22.AddRange((IEnumerable<ReferencedPoint>) dictionary26[key12][dimensionKey]);
                  }
                }
                List<XYZ> list18 = source22.Select<ReferencedPoint, XYZ>((Func<ReferencedPoint, XYZ>) (x => x.LocalPoint)).ToList<XYZ>();
                bool blockout = true;
                Dictionary<Reference, GeometryObject> source23 = new Dictionary<Reference, GeometryObject>();
                foreach (Reference reference in dimension.References)
                {
                  Element elemBound = revitDoc.GetElement(reference.ElementId);
                  SortingInfoCloneTicketElement cloneTicketElement = new SortingInfoCloneTicketElement(string.Empty, FormLocation.None);
                  foreach (SortingInfoCloneTicketElement key13 in sourceLikeElementDictionary.Keys)
                  {
                    if (sourceLikeElementDictionary[key13].FindIndex((Predicate<Element>) (x => x.Id == elemBound.Id)) != -1)
                    {
                      cloneTicketElement = key13;
                      break;
                    }
                  }
                  ReferencedPoint referencedPoint = (ReferencedPoint) null;
                  if (cloneTicketElement == key5)
                  {
                    XYZ referencedLocalPointXYZ = new XYZ(0.0, 0.0, 0.0);
                    XYZ point1 = new XYZ(0.0, 0.0, 0.0);
                    GeometryObject objectFromReference = elemBound.GetGeometryObjectFromReference(reference);
                    if (!(objectFromReference == (GeometryObject) null))
                    {
                      GeometryObject geometryObject = (GeometryObject) null;
                      foreach (Solid solid in symbolSolids1)
                      {
                        if ((GeometryObject) solid != (GeometryObject) null)
                        {
                          foreach (GeometryObject edge in solid.Edges)
                          {
                            if (objectFromReference == edge)
                            {
                              geometryObject = edge;
                              break;
                            }
                          }
                          if (!(geometryObject != (GeometryObject) null))
                          {
                            foreach (GeometryObject face in solid.Faces)
                            {
                              if (objectFromReference == face)
                              {
                                geometryObject = face;
                                break;
                              }
                            }
                            if (geometryObject != (GeometryObject) null)
                              break;
                          }
                          else
                            break;
                        }
                      }
                      if (geometryObject == (GeometryObject) null)
                        geometryObject = objectFromReference;
                      switch (geometryObject)
                      {
                        case Edge _:
                        case Curve _:
                          bool runAgain;
                          referencedLocalPointXYZ = this.getEndPoint(geometryObject, elemBound as FamilyInstance, viewTransform1, dimensionKey, xyzList, num1, previouslyUsedDict, bSymbol1, true, out runAgain);
                          if (runAgain)
                          {
                            source23.Add(reference, geometryObject);
                            continue;
                          }
                          point1 = viewTransform1.OfPoint(referencedLocalPointXYZ);
                          if (previouslyUsedDict.ContainsKey(geometryObject))
                          {
                            previouslyUsedDict[geometryObject].Add(referencedLocalPointXYZ);
                            break;
                          }
                          previouslyUsedDict.Add(geometryObject, new List<XYZ>()
                          {
                            referencedLocalPointXYZ
                          });
                          break;
                        case PlanarFace _:
                          point1 = this.getPlanerFacePoint(geometryObject as PlanarFace, elemBound as FamilyInstance, viewTransform1, bSymbol1);
                          referencedLocalPointXYZ = viewTransform1.Inverse.OfPoint(point1);
                          break;
                        case CylindricalFace _:
                          string str9 = "One or more dimensions contain references to curved geometry. This may cause errors when cloning dimensions.";
                          if (dictionary11.ContainsKey(assemblyInstance.Name))
                          {
                            if (!dictionary11[assemblyInstance.Name].Contains(str9))
                            {
                              dictionary11[assemblyInstance.Name].Add(str9);
                              continue;
                            }
                            continue;
                          }
                          dictionary11.Add(assemblyInstance.Name, new List<string>()
                          {
                            str9
                          });
                          continue;
                        default:
                          using (List<ReferencedPoint>.Enumerator enumerator = referncePlanesAndLines1[dimensionKey].GetEnumerator())
                          {
                            while (enumerator.MoveNext())
                            {
                              ReferencedPoint current = enumerator.Current;
                              if (current.Reference.ConvertToStableRepresentation(revitDoc) == reference.ConvertToStableRepresentation(revitDoc))
                              {
                                referencedLocalPointXYZ = current.LocalPoint;
                                point1 = current.Point;
                                break;
                              }
                            }
                            break;
                          }
                      }
                      if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                        xyzList.RemoveAll((Predicate<XYZ>) (x => x.X.ApproximatelyEquals(referencedLocalPointXYZ.X)));
                      else
                        xyzList.RemoveAll((Predicate<XYZ>) (x => x.Y.ApproximatelyEquals(referencedLocalPointXYZ.Y)));
                      bool flag17 = true;
                      bool flag18 = false;
                      if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                      {
                        if (referencedPoint1 != null && !lowerExtent && referencedPoint1.LocalPoint.X.ApproximatelyEquals(referencedLocalPointXYZ.X))
                        {
                          lowerExtent = true;
                          flag17 = false;
                          flag18 = true;
                        }
                        if (referencedPoint2 != null && !flag16 && referencedPoint2.LocalPoint.X.ApproximatelyEquals(referencedLocalPointXYZ.X))
                        {
                          flag16 = true;
                          flag17 = false;
                          flag18 = true;
                        }
                        if (!flag18 && !list18.Where<XYZ>((Func<XYZ, bool>) (point => point.X.ApproximatelyEquals(referencedLocalPointXYZ.X))).Any<XYZ>())
                          blockout = false;
                      }
                      else
                      {
                        if (referencedPoint1 != null && !lowerExtent && referencedPoint1.LocalPoint.Y.ApproximatelyEquals(referencedLocalPointXYZ.Y))
                        {
                          lowerExtent = true;
                          flag17 = false;
                          flag18 = true;
                        }
                        if (referencedPoint2 != null && !flag16 && referencedPoint2.LocalPoint.Y.ApproximatelyEquals(referencedLocalPointXYZ.Y))
                        {
                          flag16 = true;
                          flag17 = false;
                          flag18 = true;
                        }
                        if (!flag18 && !list18.Where<XYZ>((Func<XYZ, bool>) (point => point.X.ApproximatelyEquals(referencedLocalPointXYZ.X))).Any<XYZ>())
                          blockout = false;
                      }
                      if (flag17)
                        referencedPoint = new ReferencedPoint(reference, point1, referencedLocalPointXYZ, elemBound.Id);
                    }
                    else
                      continue;
                  }
                  else
                  {
                    blockout = false;
                    List<ReferencedPoint> referencedPointList10 = new List<ReferencedPoint>();
                    List<ReferencedPoint> source24 = !dictionary25.ContainsKey(reference.ElementId) ? HiddenGeomReferenceCalculator.GetDimLineReference(elemBound as FamilyInstance, dimensionKey, element7) : dictionary25[reference.ElementId];
                    if (source24 != null && source24.Any<ReferencedPoint>())
                    {
                      string stableRepresentation = reference.ConvertToStableRepresentation(revitDoc);
                      foreach (ReferencedPoint referencedPoint3 in source24)
                      {
                        if (referencedPoint3.Reference.ConvertToStableRepresentation(revitDoc) == stableRepresentation)
                        {
                          referencedPoint = referencedPoint3;
                          referencedPoint3.elementId = elemBound.Id;
                          referencedPoint.LocalPoint = viewTransform1.Inverse.OfPoint(referencedPoint3.Point);
                          break;
                        }
                      }
                      if (referencedPoint != null)
                      {
                        if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                          xyzList.RemoveAll((Predicate<XYZ>) (x => x.X.ApproximatelyEquals(referencedPoint.LocalPoint.X)));
                        else
                          xyzList.RemoveAll((Predicate<XYZ>) (x => x.Y.ApproximatelyEquals(referencedPoint.LocalPoint.Y)));
                      }
                      else
                      {
                        string str10 = "Failed to process one or more dimension references to non-structural framing elements. Please ensure that the element's family contains Edge Dim Lines and that the dimension references them and not element geometry. ";
                        if (dictionary11.ContainsKey(sourceAssembly.Name))
                        {
                          if (!dictionary11[sourceAssembly.Name].Contains(str10))
                            dictionary11[sourceAssembly.Name].Add(str10);
                        }
                        else
                          dictionary11.Add(sourceAssembly.Name, new List<string>()
                          {
                            str10
                          });
                      }
                    }
                    else
                      continue;
                  }
                  if (referencedPoint != null)
                    referencedPointList9.Add(referencedPoint);
                }
                int num33 = 0;
                bool first = true;
                while (source23.Count<KeyValuePair<Reference, GeometryObject>>() > 0)
                {
                  Dictionary<Reference, GeometryObject> dictionary27 = new Dictionary<Reference, GeometryObject>();
                  foreach (Reference key14 in source23.Keys)
                  {
                    Element element20 = revitDoc.GetElement(key14);
                    bool runAgain;
                    XYZ referencedLocalPointXYZ = this.getEndPoint(source23[key14], element20 as FamilyInstance, viewTransform1, dimensionKey, xyzList, num1, previouslyUsedDict, bSymbol1, first, out runAgain);
                    if (runAgain)
                    {
                      dictionary27.Add(key14, source23[key14]);
                    }
                    else
                    {
                      XYZ point2 = viewTransform1.OfPoint(referencedLocalPointXYZ);
                      if (previouslyUsedDict.ContainsKey(source23[key14]))
                        previouslyUsedDict[source23[key14]].Add(referencedLocalPointXYZ);
                      else
                        previouslyUsedDict.Add(source23[key14], new List<XYZ>()
                        {
                          referencedLocalPointXYZ
                        });
                      if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                        xyzList.RemoveAll((Predicate<XYZ>) (x => x.X.ApproximatelyEquals(referencedLocalPointXYZ.X)));
                      else
                        xyzList.RemoveAll((Predicate<XYZ>) (x => x.Y.ApproximatelyEquals(referencedLocalPointXYZ.Y)));
                      bool flag19 = true;
                      bool flag20 = false;
                      if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                      {
                        if (referencedPoint1 != null && !lowerExtent && referencedPoint1.LocalPoint.X.ApproximatelyEquals(referencedLocalPointXYZ.X))
                        {
                          lowerExtent = true;
                          flag19 = false;
                          flag20 = true;
                        }
                        if (referencedPoint2 != null && !flag16 && referencedPoint2.LocalPoint.X.ApproximatelyEquals(referencedLocalPointXYZ.X))
                        {
                          flag16 = true;
                          flag19 = false;
                          flag20 = true;
                        }
                        if (!flag20 && !list18.Where<XYZ>((Func<XYZ, bool>) (point => point.X.ApproximatelyEquals(referencedLocalPointXYZ.X))).Any<XYZ>())
                          blockout = false;
                      }
                      else
                      {
                        if (referencedPoint1 != null && !lowerExtent && referencedPoint1.LocalPoint.Y.ApproximatelyEquals(referencedLocalPointXYZ.Y))
                        {
                          lowerExtent = true;
                          flag19 = false;
                          flag20 = true;
                        }
                        if (referencedPoint2 != null && !flag16 && referencedPoint2.LocalPoint.Y.ApproximatelyEquals(referencedLocalPointXYZ.Y))
                        {
                          flag16 = true;
                          flag19 = false;
                          flag20 = true;
                        }
                        if (!flag20 && !list18.Where<XYZ>((Func<XYZ, bool>) (point => point.X.ApproximatelyEquals(referencedLocalPointXYZ.X))).Any<XYZ>())
                          blockout = false;
                      }
                      ReferencedPoint referencedPoint4 = (ReferencedPoint) null;
                      if (flag19)
                        referencedPoint4 = new ReferencedPoint(key14, point2, referencedLocalPointXYZ, element20.Id);
                      if (referencedPoint4 != null)
                        referencedPointList9.Add(referencedPoint4);
                    }
                  }
                  source23 = dictionary27;
                  if (num33 > 3)
                    first = false;
                  if (num33 <= 6)
                    ++num33;
                  else
                    break;
                }
                if (flag16 & lowerExtent && dimension.References.Size == 2)
                {
                  overall = true;
                  blockout = false;
                }
                List<ReferencedPoint> referencedPointList11 = new List<ReferencedPoint>();
                bool? side = new bool?();
                SortingInfoCloneTicketElement majorityType = new SortingInfoCloneTicketElement(string.Empty, FormLocation.None);
                int majorityCount = 0;
                List<SortingInfoCloneTicketElement> types = new List<SortingInfoCloneTicketElement>();
                if (!blockout && !overall)
                {
                  Dictionary<ElementId, ElementReferenceInfo> dictionary28 = new Dictionary<ElementId, ElementReferenceInfo>();
                  List<ReferencedPoint> second = new List<ReferencedPoint>();
                  foreach (ReferencedPoint referencedPoint5 in referencedPointList9)
                  {
                    ReferencedPoint referencedPoint = referencedPoint5;
                    if (!sourceLikeElementDictionary.Where<KeyValuePair<SortingInfoCloneTicketElement, List<Element>>>((Func<KeyValuePair<SortingInfoCloneTicketElement, List<Element>>, bool>) (kvp => kvp.Value.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Contains<ElementId>(referencedPoint.Reference.ElementId))).Any<KeyValuePair<SortingInfoCloneTicketElement, List<Element>>>())
                      second.Add(referencedPoint);
                  }
                  List<ReferencedPoint> list19 = referencedPointList9.Except<ReferencedPoint>((IEnumerable<ReferencedPoint>) second).ToList<ReferencedPoint>();
                  foreach (ReferencedPoint referencedPoint6 in list19)
                  {
                    ReferencedPoint referencedPoint = referencedPoint6;
                    if (dictionary28.ContainsKey(referencedPoint.elementId))
                    {
                      dictionary28[referencedPoint.elementId].sourceReferences.Add(referencedPoint);
                    }
                    else
                    {
                      ElementReferenceInfo elementReferenceInfo = new ElementReferenceInfo(referencedPoint.elementId, this.getElementSortingObject(referencedPoint.elementId, revitDoc, locationInFormValues, locationInFormAnalyzer1), new List<ReferencedPoint>()
                      {
                        referencedPoint
                      });
                      dictionary28.Add(referencedPoint.elementId, elementReferenceInfo);
                    }
                    SortingInfoCloneTicketElement cloneTicketElement = new SortingInfoCloneTicketElement(string.Empty, FormLocation.None);
                    foreach (SortingInfoCloneTicketElement key15 in sourceLikeElementDictionary.Keys)
                    {
                      if (sourceLikeElementDictionary[key15].FindIndex((Predicate<Element>) (x => x.Id == referencedPoint.Reference.ElementId)) != -1)
                      {
                        cloneTicketElement = key15;
                        break;
                      }
                    }
                    if (targetLikeElementDictionary.ContainsKey(cloneTicketElement))
                    {
                      if (cloneTicketElement == key5)
                      {
                        List<ReferencedPoint> refPoints5 = new List<ReferencedPoint>();
                        foreach (Element element21 in targetLikeElementDictionary[cloneTicketElement])
                        {
                          Element elem = element21;
                          List<ReferencedPoint> referencedPointList12 = new List<ReferencedPoint>();
                          if (!dictionary24.ContainsKey(elem.Id))
                          {
                            List<ReferencedPoint> framingReferencesList = this.GetStructuralFramingReferencesList(elem as FamilyInstance, dimensionKey, element8, symbolSolids2, bSymbol2, num1);
                            framingReferencesList.AddRange((IEnumerable<ReferencedPoint>) referncePlanesAndLines2[dimensionKey]);
                            if (framingReferencesList.Count<ReferencedPoint>() != 0)
                            {
                              List<ReferencedPoint> referencedPointList13 = this.removeDoubleReferencedPoints(this.sortReferencedPointList(framingReferencesList, dimensionKey), dimensionKey);
                              ElementReferenceInfo elementReferenceInfo = new ElementReferenceInfo(elem.Id, cloneTicketElement, referencedPointList13, referencedPointList13.FirstOrDefault<ReferencedPoint>(), referencedPointList13.LastOrDefault<ReferencedPoint>());
                              dictionary24.Add(elem.Id, elementReferenceInfo);
                              if (referencedPointList13 != null)
                              {
                                referencedPointList13.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                                refPoints5.AddRange((IEnumerable<ReferencedPoint>) referencedPointList13);
                              }
                            }
                          }
                          else
                          {
                            List<ReferencedPoint> referencedPoints = dictionary24[elem.Id].referencedPoints;
                            refPoints5.AddRange((IEnumerable<ReferencedPoint>) referencedPoints);
                          }
                        }
                        List<ReferencedPoint> referencedPointList14 = this.sortReferencedPointList(this.removeDoubleReferencedPoints(this.checkReferencedPointsForLocalPoint(refPoints5, viewTransform2), dimensionKey), dimensionKey);
                        source21.Add(referencedPoint, referencedPointList14);
                      }
                      else
                      {
                        List<ReferencedPoint> refsList = new List<ReferencedPoint>();
                        foreach (Element element22 in targetLikeElementDictionary[cloneTicketElement])
                        {
                          Element elem = element22;
                          List<ReferencedPoint> referencedPointList15 = new List<ReferencedPoint>();
                          if (!dictionary24.ContainsKey(elem.Id))
                          {
                            List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(elem as FamilyInstance, dimensionKey, element8);
                            foreach (ReferencedPoint referencedPoint7 in dimLineReference)
                              referencedPoint7.elementId = elem.Id;
                            if (dimLineReference.Count<ReferencedPoint>() != 0)
                            {
                              List<ReferencedPoint> referencedPointList16 = this.sortReferencedPointList(this.checkReferencedPointsForLocalPoint(dimLineReference, viewTransform2), dimensionKey);
                              ElementReferenceInfo elementReferenceInfo = new ElementReferenceInfo(elem.Id, cloneTicketElement, referencedPointList16, referencedPointList16.FirstOrDefault<ReferencedPoint>(), referencedPointList16.LastOrDefault<ReferencedPoint>());
                              dictionary24.Add(elem.Id, elementReferenceInfo);
                              if (referencedPointList16 != null)
                              {
                                referencedPointList16.ForEach((Action<ReferencedPoint>) (x => x.elementId = elem.Id));
                                refsList.AddRange((IEnumerable<ReferencedPoint>) referencedPointList16);
                              }
                            }
                          }
                          else
                          {
                            List<ReferencedPoint> referencedPoints = dictionary24[elem.Id].referencedPoints;
                            refsList.AddRange((IEnumerable<ReferencedPoint>) referencedPoints);
                          }
                        }
                        List<ReferencedPoint> referencedPointList17 = this.sortReferencedPointList(this.removeDoubleReferencedPoints(refsList, dimensionKey), dimensionKey);
                        source21.Add(referencedPoint, referencedPointList17);
                      }
                    }
                  }
                  List<ElementId> elementIdList4 = new List<ElementId>();
                  foreach (Element element23 in targetLikeElementDictionary[key5])
                  {
                    if (dictionary24.ContainsKey(element23.Id))
                      elementIdList4.Add(element23.Id);
                  }
                  referencedPointList9 = this.sortReferencedPointList(list19, dimensionKey);
                  bool flag21 = false;
                  bool flag22 = false;
                  bool flag23 = false;
                  bool flag24 = false;
                  if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                  {
                    foreach (ReferencedPoint referencedPoint8 in referencedPointList9)
                    {
                      if (source16.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint8.elementId) && !source17.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint8.elementId))
                      {
                        flag21 = true;
                        if (flag22)
                          break;
                      }
                      if (!source16.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint8.elementId) && source17.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint8.elementId))
                      {
                        flag22 = true;
                        if (flag21)
                          break;
                      }
                      if (source16.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint8.elementId) && source17.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint8.elementId))
                      {
                        flag21 = true;
                        flag22 = true;
                        break;
                      }
                    }
                    if (flag21)
                    {
                      foreach (ReferencedPoint referencedPoint9 in targetTopReferences)
                      {
                        if (!elementIdList4.Contains(referencedPoint9.elementId))
                          elementIdList4.Add(referencedPoint9.elementId);
                      }
                    }
                    if (flag22)
                    {
                      foreach (ReferencedPoint referencedPoint10 in source14)
                      {
                        if (!elementIdList4.Contains(referencedPoint10.elementId))
                          elementIdList4.Add(referencedPoint10.elementId);
                      }
                    }
                    if (!flag21 && !flag22)
                    {
                      foreach (ReferencedPoint referencedPoint11 in targetTopReferences)
                      {
                        if (!elementIdList4.Contains(referencedPoint11.elementId))
                          elementIdList4.Add(referencedPoint11.elementId);
                      }
                      foreach (ReferencedPoint referencedPoint12 in source14)
                      {
                        if (!elementIdList4.Contains(referencedPoint12.elementId))
                          elementIdList4.Add(referencedPoint12.elementId);
                      }
                    }
                  }
                  else if (dimensionKey == DimensionEdge.Left || dimensionKey == DimensionEdge.Right)
                  {
                    foreach (ReferencedPoint referencedPoint13 in referencedPointList9)
                    {
                      if (source18.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint13.elementId) && !source19.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint13.elementId))
                      {
                        flag23 = true;
                        if (flag24)
                          break;
                      }
                      if (!source18.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint13.elementId) && source19.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint13.elementId))
                      {
                        flag24 = true;
                        if (flag23)
                          break;
                      }
                      if (source18.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint13.elementId) && source19.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint13.elementId))
                      {
                        flag23 = true;
                        flag24 = true;
                        break;
                      }
                    }
                    if (flag23)
                    {
                      foreach (ReferencedPoint referencedPoint14 in source15)
                      {
                        if (!elementIdList4.Contains(referencedPoint14.elementId))
                          elementIdList4.Add(referencedPoint14.elementId);
                      }
                    }
                    if (flag24)
                    {
                      foreach (ReferencedPoint referencedPoint15 in targetLeftReferences)
                      {
                        if (!elementIdList4.Contains(referencedPoint15.elementId))
                          elementIdList4.Add(referencedPoint15.elementId);
                      }
                    }
                    if (!flag24 && !flag23)
                    {
                      foreach (ReferencedPoint referencedPoint16 in source15)
                      {
                        if (!elementIdList4.Contains(referencedPoint16.elementId))
                          elementIdList4.Add(referencedPoint16.elementId);
                      }
                      foreach (ReferencedPoint referencedPoint17 in targetLeftReferences)
                      {
                        if (!elementIdList4.Contains(referencedPoint17.elementId))
                          elementIdList4.Add(referencedPoint17.elementId);
                      }
                    }
                  }
                  majorityType = new SortingInfoCloneTicketElement(string.Empty, FormLocation.None);
                  types = new List<SortingInfoCloneTicketElement>();
                  int num34 = 0;
                  foreach (SortingInfoCloneTicketElement key16 in sourceLikeElementDictionary.Keys)
                  {
                    int num35 = sourceLikeElementDictionary[key16].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).Count<ElementId>();
                    if (num35 > num34 && key16 != key5)
                    {
                      num34 = num35;
                      majorityType = key16;
                    }
                    if (num35 > 0 && key16 != key5 && !types.Contains(key16))
                      types.Add(key16);
                  }
                  List<ElementId> source25 = new List<ElementId>();
                  Dictionary<ElementId, ElementId> sourceToTarget = new Dictionary<ElementId, ElementId>();
                  sourceToTarget.Add(structuralFramingElement1.Id, structuralFramingElement2.Id);
                  bool flag25 = false;
                  bool flag26 = true;
                  if (majorityType != new SortingInfoCloneTicketElement(string.Empty, FormLocation.None))
                  {
                    foreach (Element element24 in sourceLikeElementDictionary[majorityType])
                    {
                      if (flag21 & flag22 || flag24 & flag23)
                      {
                        if (!referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id))
                        {
                          flag26 = false;
                          break;
                        }
                      }
                      else if (flag21)
                      {
                        if (source16.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id) && !referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id))
                        {
                          flag26 = false;
                          break;
                        }
                      }
                      else if (flag22)
                      {
                        if (source17.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id) && !referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id))
                        {
                          flag26 = false;
                          break;
                        }
                      }
                      else if (flag24)
                      {
                        if (source19.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id) && !referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id))
                        {
                          flag26 = false;
                          break;
                        }
                      }
                      else if (flag23 && source18.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id) && !referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(element24.Id))
                      {
                        flag26 = false;
                        break;
                      }
                    }
                    if (flag26 && targetLikeElementDictionary.ContainsKey(majorityType))
                    {
                      if (flag21 & flag22)
                      {
                        source25.AddRange(targetTopReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        source25.AddRange(source14.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        source25 = source25.Distinct<ElementId>().ToList<ElementId>();
                        flag25 = true;
                      }
                      else if (flag24 & flag23)
                      {
                        source25.AddRange(targetLeftReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        source25.AddRange(source15.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        source25 = source25.Distinct<ElementId>().ToList<ElementId>();
                        flag25 = true;
                      }
                      else if (flag21)
                      {
                        source25.AddRange(targetTopReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        flag25 = true;
                      }
                      else if (flag22)
                      {
                        source25.AddRange(source14.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        flag25 = true;
                      }
                      else if (flag24)
                      {
                        source25.AddRange(targetLeftReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        flag25 = true;
                      }
                      else if (flag23)
                      {
                        source25.AddRange(source15.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)));
                        flag25 = true;
                      }
                      if (source25.Count != 0 && !dictionary17[dimensionKey].Contains(majorityType))
                        dictionary17[dimensionKey].Add(majorityType);
                    }
                  }
                  List<ElementId> elementIdList5 = source25.Distinct<ElementId>().ToList<ElementId>();
                  Dictionary<ElementId, ElementId> allBetween = new Dictionary<ElementId, ElementId>();
                  List<ReferencedPoint> source26 = new List<ReferencedPoint>();
                  if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                  {
                    if (flag21)
                      source26.AddRange(source16.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => sourceLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))));
                    if (flag22)
                      source26.AddRange(source17.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => sourceLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))));
                  }
                  else if (dimensionKey == DimensionEdge.Right || dimensionKey == DimensionEdge.Left)
                  {
                    if (flag23)
                      source26.AddRange(source18.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => sourceLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))));
                    if (flag24)
                      source26.AddRange(source19.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => sourceLikeElementDictionary[majorityType].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))));
                  }
                  List<ReferencedPoint> source27 = this.sortReferencedPointList(source26.Distinct<ReferencedPoint>().ToList<ReferencedPoint>(), dimensionKey);
                  ElementId key17 = (ElementId) null;
                  ElementId elementId1 = (ElementId) null;
                  bool flag27 = false;
                  bool flag28 = false;
                  bool flag29 = true;
                  int num36 = 0;
                  foreach (ReferencedPoint referencedPoint18 in source27)
                  {
                    if (referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint18.elementId) && key17 == (ElementId) null)
                    {
                      key17 = referencedPoint18.elementId;
                      if (flag29)
                        flag27 = true;
                      ++num36;
                    }
                    else if (referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint18.elementId) && key17 != (ElementId) null)
                    {
                      if (key17 != referencedPoint18.elementId)
                      {
                        elementId1 = referencedPoint18.elementId;
                        ++num36;
                      }
                    }
                    else if (!referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint18.elementId))
                    {
                      if (key17 != (ElementId) null && elementId1 != (ElementId) null)
                        allBetween.Add(key17, elementId1);
                      key17 = (ElementId) null;
                      elementId1 = (ElementId) null;
                      flag29 = false;
                    }
                  }
                  bool flag30 = false;
                  ElementId firstLike = (ElementId) null;
                  ElementId lastLike = (ElementId) null;
                  if (source27.Count<ReferencedPoint>() != 0 && !flag26)
                  {
                    if (referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(source27.LastOrDefault<ReferencedPoint>().elementId))
                      flag28 = true;
                    if (num36 == 2)
                    {
                      ReferencedPoint referencedPoint19 = source27.FirstOrDefault<ReferencedPoint>();
                      ReferencedPoint referencedPoint20 = source27.LastOrDefault<ReferencedPoint>();
                      if (referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint19.elementId) && referencedPointList9.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint20.elementId))
                      {
                        flag30 = true;
                        firstLike = referencedPoint19.elementId;
                        lastLike = referencedPoint20.elementId;
                        flag28 = false;
                        flag27 = false;
                        allBetween = new Dictionary<ElementId, ElementId>();
                      }
                    }
                  }
                  majorityCount = elementIdList5.Count;
                  Dictionary<ElementId, ElementReferenceInfo> sourceElemRefInfo = this.sortSourceElementReferenceInfoDictionary(dictionary28, dimensionKey);
                  dictionary24 = this.sortTargetElementReferenceInfoDictionary(dictionary24, dimensionKey);
                  Dictionary<ElementId, ElementId> dictionary29 = new Dictionary<ElementId, ElementId>();
                  foreach (ElementId key18 in sourceElemRefInfo.Keys)
                  {
                    StructuralFramingBoundsObject sourceMinObject;
                    if (boundsObjectDictOut1.ContainsKey(structuralFramingElement1.Id))
                    {
                      sourceMinObject = boundsObjectDictOut1[structuralFramingElement1.Id];
                    }
                    else
                    {
                      sourceMinObject = new StructuralFramingBoundsObject(structuralFramingElement1 as FamilyInstance, element7, true);
                      boundsObjectDictOut1.Add(structuralFramingElement1.Id, sourceMinObject);
                    }
                    StructuralFramingBoundsObject targetMinObject;
                    if (boundsObjectDictOut2.ContainsKey(structuralFramingElement2.Id))
                    {
                      targetMinObject = boundsObjectDictOut2[structuralFramingElement2.Id];
                    }
                    else
                    {
                      targetMinObject = new StructuralFramingBoundsObject(structuralFramingElement2 as FamilyInstance, element8, true);
                      boundsObjectDictOut2.Add(structuralFramingElement2.Id, targetMinObject);
                    }
                    bool flag31 = false;
                    if (targetLikeElementDictionary.ContainsKey(sourceElemRefInfo[key18].type))
                    {
                      List<ElementId> elementIdList6 = new List<ElementId>();
                      foreach (Element element25 in targetLikeElementDictionary[sourceElemRefInfo[key18].type])
                      {
                        if (dictionary24.ContainsKey(element25.Id))
                        {
                          List<List<ElementId>> alignmentLists = new List<List<ElementId>>();
                          ElementId id = element25.Id;
                          ElementId alignedIdClosestToEdge;
                          if (!dictionary29.ContainsKey(element25.Id))
                          {
                            if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                            {
                              if (dictionary21.ContainsKey(sourceElemRefInfo[key18].type))
                                alignmentLists = dictionary21[sourceElemRefInfo[key18].type];
                            }
                            else if (dictionary20.ContainsKey(sourceElemRefInfo[key18].type))
                              alignmentLists = dictionary20[sourceElemRefInfo[key18].type];
                            alignedIdClosestToEdge = this.getAlignedIdClosestToEdge(element25.Id, alignmentLists, dimensionKey, dictionary24);
                            dictionary29.Add(element25.Id, alignedIdClosestToEdge);
                          }
                          else
                            alignedIdClosestToEdge = dictionary29[element25.Id];
                          if (elementIdList4.Contains(alignedIdClosestToEdge) && !elementIdList6.Contains(alignedIdClosestToEdge))
                            elementIdList6.Add(alignedIdClosestToEdge);
                        }
                      }
                      if (!(sourceElemRefInfo[key18].type == majorityType & flag30))
                      {
                        double num37 = -1.0;
                        ElementReferenceInfo elementReferenceInfo1 = (ElementReferenceInfo) null;
                        foreach (ElementId key19 in elementIdList6)
                        {
                          if (dictionary24.ContainsKey(key19))
                          {
                            ElementReferenceInfo target = dictionary24[key19];
                            double distance;
                            if (this.compareElementLocation(sourceElemRefInfo[key18], target, sourceMinObject, targetMinObject, viewTransform1, viewTransform2, num1, dimensionKey, out distance) && (num37 == -1.0 || distance < num37))
                            {
                              elementReferenceInfo1 = target;
                              num37 = distance;
                              flag31 = true;
                            }
                          }
                        }
                        if (flag31 && elementReferenceInfo1 != null)
                        {
                          if (!elementIdList5.Contains(elementReferenceInfo1.elemId))
                            elementIdList5.Add(elementReferenceInfo1.elemId);
                          if (!sourceToTarget.ContainsKey(key18))
                            sourceToTarget.Add(key18, elementReferenceInfo1.elemId);
                          elementIdList5 = this.CheckBounds(elementReferenceInfo1.elemId, key18, elementIdList5, dictionary24.Keys.ToList<ElementId>(), allBetween, sourceToTarget);
                          if (!dictionary17[dimensionKey].Contains(elementReferenceInfo1.type))
                            dictionary17[dimensionKey].Add(elementReferenceInfo1.type);
                          if (elementReferenceInfo1.type == majorityType && !flag25)
                            ++majorityCount;
                        }
                        else if (!(sourceElemRefInfo[key18].type != majorityType) && !flag31)
                        {
                          double num38 = -1.0;
                          ElementReferenceInfo elementReferenceInfo2 = (ElementReferenceInfo) null;
                          foreach (ElementId key20 in elementIdList6)
                          {
                            if (dictionary24.ContainsKey(key20))
                            {
                              ElementReferenceInfo target = dictionary24[key20];
                              if (num38 == -1.0)
                              {
                                num38 = this.getDistanceBetweenElements(sourceElemRefInfo[key18], target, sourceMinObject, targetMinObject, dimensionKey);
                                elementReferenceInfo2 = target;
                              }
                              else
                              {
                                double distanceBetweenElements = this.getDistanceBetweenElements(sourceElemRefInfo[key18], target, sourceMinObject, targetMinObject, dimensionKey);
                                if (num38 > distanceBetweenElements)
                                {
                                  num38 = this.getDistanceBetweenElements(sourceElemRefInfo[key18], target, sourceMinObject, targetMinObject, dimensionKey);
                                  elementReferenceInfo2 = target;
                                }
                              }
                            }
                          }
                          if (elementReferenceInfo2 != null)
                          {
                            if (!elementIdList5.Contains(elementReferenceInfo2.elemId))
                              elementIdList5.Add(elementReferenceInfo2.elemId);
                            if (!sourceToTarget.ContainsKey(key18))
                              sourceToTarget.Add(key18, elementReferenceInfo2.elemId);
                            elementIdList5 = this.CheckBounds(elementReferenceInfo2.elemId, key18, elementIdList5, dictionary24.Keys.ToList<ElementId>(), allBetween, sourceToTarget);
                            if (!dictionary17[dimensionKey].Contains(elementReferenceInfo2.type))
                              dictionary17[dimensionKey].Add(elementReferenceInfo2.type);
                            if (elementReferenceInfo2.type == majorityType && !flag25)
                              ++majorityCount;
                          }
                        }
                      }
                    }
                  }
                  if (flag27)
                  {
                    ReferencedPoint key21 = (ReferencedPoint) null;
                    foreach (ElementReferenceInfo elementReferenceInfo in sourceElemRefInfo.Values)
                    {
                      if (elementReferenceInfo.type == majorityType)
                      {
                        key21 = elementReferenceInfo.sourceReferences.First<ReferencedPoint>();
                        break;
                      }
                    }
                    if (key21 != null && sourceToTarget.ContainsKey(key21.elementId))
                    {
                      ElementId elementId2 = sourceToTarget[key21.elementId];
                      if (elementId2 != (ElementId) null)
                      {
                        foreach (ReferencedPoint referencedPoint21 in source21[key21])
                        {
                          if (!referencedPoint21.elementId.Equals((object) elementId2))
                          {
                            ElementId elementId3 = referencedPoint21.elementId;
                            if (dictionary29.ContainsKey(referencedPoint21.elementId))
                              elementId3 = dictionary29[referencedPoint21.elementId];
                            if (!elementIdList5.Contains(elementId3) && elementIdList4.Contains(elementId3))
                              elementIdList5.Add(elementId3);
                          }
                          else
                            break;
                        }
                      }
                    }
                  }
                  if (flag28)
                  {
                    ReferencedPoint key22 = (ReferencedPoint) null;
                    foreach (ElementReferenceInfo elementReferenceInfo in sourceElemRefInfo.Values.Reverse<ElementReferenceInfo>())
                    {
                      if (elementReferenceInfo.type == majorityType)
                      {
                        key22 = elementReferenceInfo.sourceReferences.First<ReferencedPoint>();
                        break;
                      }
                    }
                    if (key22 != null && sourceToTarget.ContainsKey(key22.elementId))
                    {
                      ElementId elementId4 = sourceToTarget[key22.elementId];
                      if (elementId4 != (ElementId) null)
                      {
                        List<ReferencedPoint> referencedPointList18 = source21[key22];
                        referencedPointList18.Reverse();
                        foreach (ReferencedPoint referencedPoint22 in referencedPointList18)
                        {
                          if (!referencedPoint22.elementId.Equals((object) elementId4))
                          {
                            ElementId elementId5 = referencedPoint22.elementId;
                            if (dictionary29.ContainsKey(referencedPoint22.elementId))
                              elementId5 = dictionary29[referencedPoint22.elementId];
                            if (!elementIdList5.Contains(elementId5) && elementIdList4.Contains(elementId5))
                              elementIdList5.Add(elementId5);
                          }
                          else
                            break;
                        }
                      }
                    }
                  }
                  if (flag30)
                  {
                    List<ElementId> list20 = source21.Where<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>>((Func<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>, bool>) (x => x.Key.elementId == firstLike)).ToList<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>>().FirstOrDefault<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>>().Value.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Distinct<ElementId>().ToList<ElementId>();
                    List<ElementId> list21 = source21.Where<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>>((Func<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>, bool>) (x => x.Key.elementId == lastLike)).ToList<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>>().FirstOrDefault<KeyValuePair<ReferencedPoint, List<ReferencedPoint>>>().Value.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Distinct<ElementId>().ToList<ElementId>();
                    ElementId key23 = list20.FirstOrDefault<ElementId>();
                    ElementId key24 = list21.LastOrDefault<ElementId>();
                    elementIdList5.RemoveAll(new Predicate<ElementId>(list20.Contains));
                    elementIdList5.RemoveAll(new Predicate<ElementId>(list21.Contains));
                    if (key23 != (ElementId) null)
                    {
                      ElementId elementId6 = key23;
                      if (dictionary29.ContainsKey(key23))
                        elementId6 = dictionary29[key23];
                      elementIdList5.Add(elementId6);
                      if (sourceToTarget.ContainsKey(source27.FirstOrDefault<ReferencedPoint>().elementId))
                        sourceToTarget[source27.FirstOrDefault<ReferencedPoint>().elementId] = elementId6;
                      else
                        sourceToTarget.Add(source27.FirstOrDefault<ReferencedPoint>().elementId, elementId6);
                    }
                    if (key24 != (ElementId) null)
                    {
                      ElementId elementId7 = key24;
                      if (dictionary29.ContainsKey(key24))
                        elementId7 = dictionary29[key24];
                      elementIdList5.Add(elementId7);
                      if (sourceToTarget.ContainsKey(source27.LastOrDefault<ReferencedPoint>().elementId))
                        sourceToTarget[source27.LastOrDefault<ReferencedPoint>().elementId] = elementId7;
                      else
                        sourceToTarget.Add(source27.LastOrDefault<ReferencedPoint>().elementId, elementId7);
                    }
                    majorityCount = 2;
                  }
                  side = new bool?();
                  if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                  {
                    if (flag21 & flag22)
                      side = new bool?();
                    else if (flag21)
                      side = new bool?(true);
                    else if (flag22)
                      side = new bool?(false);
                  }
                  else if (dimensionKey == DimensionEdge.Left || dimensionKey == DimensionEdge.Right)
                  {
                    if (flag23 & flag24)
                      side = new bool?();
                    else if (flag24)
                      side = new bool?(true);
                    else if (flag23)
                      side = new bool?(false);
                  }
                  referencedPointList11 = this.GetTargetReferencesFromElements(revitDoc, elementIdList5, sourceToTarget, sourceElemRefInfo, dictionary24, boundsObjectDictOut1, boundsObjectDictOut2, element7, element8, viewTransform2, num1, majorityType, lowerExtent, flag16, dimensionKey);
                }
                bool multipleElmeentTypes = false;
                if (blockout)
                {
                  Dictionary<int, Dictionary<DimensionEdge, List<ReferencedPoint>>> dictionary30 = DimReferencesManager.InstrumentSFContours(revitDoc, structuralFramingElement2.Id, element8);
                  if (dictionary30 != null)
                  {
                    foreach (int key25 in (IEnumerable<int>) dictionary30.Keys.OrderBy<int, int>((Func<int, int>) (j => j)))
                    {
                      if (key25 > -1)
                      {
                        Dictionary<DimensionEdge, List<DimensionElement>> quadrant = BatchAutoDimension.DetermineQuadrant(revitDoc, element8, structuralFramingElement2 as FamilyInstance, BatchAutoDimension.ConvertSFToDimElem(dictionary30[key25], element8), true, out Dictionary<DimensionEdge, int> _);
                        if (quadrant.ContainsKey(dimensionKey))
                          dictionary30[key25][dimensionKey] = quadrant[dimensionKey].Select<DimensionElement, ReferencedPoint>((Func<DimensionElement, ReferencedPoint>) (dE => dE.refP)).ToList<ReferencedPoint>();
                        referencedPointList11.AddRange((IEnumerable<ReferencedPoint>) dictionary30[key25][dimensionKey]);
                      }
                    }
                  }
                  referencedPointList11 = this.sortReferencedPointList(this.removeDoubleReferencedPoints(referencedPointList11, dimensionKey), dimensionKey);
                }
                else
                {
                  foreach (ReferencedPoint referencedPoint23 in referencedPointList11)
                  {
                    if (dictionary24.ContainsKey(referencedPoint23.elementId) && dictionary24[referencedPoint23.elementId].type != majorityType && dictionary24[referencedPoint23.elementId].type != key5)
                      multipleElmeentTypes = true;
                  }
                }
                Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> dictionary31 = new Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
                Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> alignDict = dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom ? dictionary21 : dictionary20;
                if (!(!overall & flag16 & lowerExtent) || referencedPointList11.Count<ReferencedPoint>() != 0)
                {
                  DimensionInfo dimensionInfo = new DimensionInfo(dimension, referencedPointList11, referencedPointList9, dimensionKey, side, majorityType, majorityCount, types, allSFrefs, sfCounter, lowerExtent, flag16, multipleElmeentTypes, alignDict, overall, blockout);
                  source20.Add(dimensionInfo);
                }
              }
              List<DimensionInfo> list22 = source20.Where<DimensionInfo>((Func<DimensionInfo, bool>) (x => x.dimEdge == dimensionKey)).ToList<DimensionInfo>();
              foreach (List<Dimension> dimensionList1 in dimensionListList)
              {
                List<Dimension> dims = dimensionList1;
                if (dims.Count != 1)
                {
                  List<DimensionInfo> list23 = list22.Where<DimensionInfo>((Func<DimensionInfo, bool>) (x => dims.Select<Dimension, ElementId>((Func<Dimension, ElementId>) (y => y.Id)).Contains<ElementId>(x.sourceDimension.Id))).ToList<DimensionInfo>();
                  if (list23.Count == dims.Count)
                  {
                    int index1 = list23.FindIndex((Predicate<DimensionInfo>) (x => x.sourceDimension.Id == dims[0].Id));
                    if (index1 >= 0)
                    {
                      SortingInfoCloneTicketElement majority = list23[index1].majorityType;
                      bool flag32 = false;
                      foreach (Dimension dimension in dims)
                      {
                        Dimension dim = dimension;
                        int index2 = list23.FindIndex((Predicate<DimensionInfo>) (x => x.sourceDimension.Id == dim.Id));
                        if (index2 < 0)
                          flag32 = true;
                        else if (list23[index2].majorityType != majority)
                          flag32 = true;
                      }
                      if (!flag32 && targetLikeElementDictionary.ContainsKey(majority))
                      {
                        Dictionary<Dimension, double> dictionary32 = new Dictionary<Dimension, double>();
                        foreach (Dimension key26 in dims)
                        {
                          if (key26.NumberOfSegments == 0)
                          {
                            if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                              dictionary32.Add(key26, key26.Origin.X);
                            else
                              dictionary32.Add(key26, key26.Origin.Y);
                          }
                          else
                          {
                            DimensionSegment dimensionSegment = key26.Segments.get_Item(0);
                            if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                              dictionary32.Add(key26, dimensionSegment.Origin.X);
                            else
                              dictionary32.Add(key26, dimensionSegment.Origin.Y);
                          }
                        }
                        List<Dimension> dimensionList2 = new List<Dimension>();
                        for (int index3 = 0; index3 < dims.Count; ++index3)
                        {
                          if (dimensionList2.Count == 0)
                          {
                            dimensionList2.Add(dims[index3]);
                          }
                          else
                          {
                            int count = dimensionList2.Count;
                            for (int index4 = 0; index4 < count; ++index4)
                            {
                              if (dictionary32[dims[index3]] < dictionary32[dimensionList2[index4]])
                                dimensionList2.Insert(index4, dims[index3]);
                            }
                            if (!dimensionList2.Contains(dims[index3]))
                              dimensionList2.Add(dims[index3]);
                          }
                        }
                        for (int i = 0; i < dimensionList2.Count; i++)
                        {
                          int j = i + 1;
                          if (j < dims.Count)
                          {
                            int index5 = source20.FindIndex((Predicate<DimensionInfo>) (x => x.sourceDimension.Id == dims[i].Id));
                            int index6 = source20.FindIndex((Predicate<DimensionInfo>) (x => x.sourceDimension.Id == dims[j].Id));
                            XYZ list1Extreme;
                            XYZ list2Extreme;
                            if (index5 >= 0 && index6 >= 0 && this.getTwoNearestExtremes(source20[index5].sourceReferencedPoints, source20[index6].sourceReferencedPoints, dimensionKey, out list1Extreme, out list2Extreme))
                            {
                              bool flag33 = true;
                              if (dimensionKey == DimensionEdge.Top || dimensionKey == DimensionEdge.Bottom)
                              {
                                if (list2Extreme.X < list1Extreme.X)
                                  flag33 = false;
                                double num39 = (list2Extreme.X + list1Extreme.X) / 2.0;
                                List<ReferencedPoint> refPoints6 = new List<ReferencedPoint>();
                                if (source20[index5].topLeft.HasValue && source20[index6].topLeft.HasValue)
                                {
                                  bool? topLeft1 = source20[index5].topLeft;
                                  bool? topLeft2 = source20[index6].topLeft;
                                  if (topLeft1.GetValueOrDefault() == topLeft2.GetValueOrDefault() & topLeft1.HasValue == topLeft2.HasValue)
                                  {
                                    if (source20[index5].topLeft.GetValueOrDefault())
                                    {
                                      refPoints6.AddRange((IEnumerable<ReferencedPoint>) targetTopReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
                                      goto label_1154;
                                    }
                                    refPoints6.AddRange((IEnumerable<ReferencedPoint>) source14.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
                                    goto label_1154;
                                  }
                                }
                                refPoints6.AddRange((IEnumerable<ReferencedPoint>) targetTopReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
                                refPoints6.AddRange((IEnumerable<ReferencedPoint>) source14.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId) && targetTopReferences.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (z => z.elementId)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
label_1154:
                                List<ReferencedPoint> referencedPointList19 = this.checkReferencedPointsForLocalPoint(refPoints6, viewTransform2);
                                List<ReferencedPoint> referencedPointList20 = new List<ReferencedPoint>();
                                foreach (ReferencedPoint referencedPoint in referencedPointList19)
                                {
                                  bool flag34 = true;
                                  foreach (DimensionInfo dimensionInfo in list23)
                                  {
                                    if (dimensionInfo.dimReferencesToPlace.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint.elementId))
                                    {
                                      flag34 = false;
                                      break;
                                    }
                                  }
                                  if (flag34)
                                    referencedPointList20.Add(referencedPoint);
                                }
                                foreach (ReferencedPoint referencedPoint in referencedPointList20)
                                {
                                  if (referencedPoint.LocalPoint == null)
                                    referencedPoint.LocalPoint = viewTransform2.Inverse.OfPoint(referencedPoint.Point);
                                  if (flag33)
                                  {
                                    if (referencedPoint.LocalPoint.X > num39)
                                      source20[index6].dimReferencesToPlace.Add(referencedPoint);
                                    else if (referencedPoint.LocalPoint.X < num39)
                                      source20[index5].dimReferencesToPlace.Insert(0, referencedPoint);
                                    else if (referencedPoint.LocalPoint.X == num39)
                                    {
                                      if (source20[index6].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.X - referencedPoint.LocalPoint.X > source20[index5].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.X - referencedPoint.LocalPoint.X)
                                        source20[index5].dimReferencesToPlace.Add(referencedPoint);
                                      else
                                        source20[index6].dimReferencesToPlace.Insert(0, referencedPoint);
                                    }
                                  }
                                  else if (referencedPoint.LocalPoint.X > num39)
                                    source20[index5].dimReferencesToPlace.Add(referencedPoint);
                                  else if (referencedPoint.LocalPoint.X < num39)
                                    source20[index6].dimReferencesToPlace.Insert(0, referencedPoint);
                                  else if (referencedPoint.LocalPoint.X == num39)
                                  {
                                    if (source20[index6].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.X - referencedPoint.LocalPoint.X > source20[index5].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.X - referencedPoint.LocalPoint.X)
                                      source20[index5].dimReferencesToPlace.Add(referencedPoint);
                                    else
                                      source20[index6].dimReferencesToPlace.Insert(0, referencedPoint);
                                  }
                                  SortingInfoCloneTicketElement elementSortingObject = this.getElementSortingObject(referencedPoint.elementId, revitDoc, locationInFormValues, locationInFormAnalyzer2);
                                  if (elementSortingObject != (SortingInfoCloneTicketElement) null && !dictionary17[dimensionKey].Contains(elementSortingObject))
                                    dictionary17[dimensionKey].Add(elementSortingObject);
                                }
                              }
                              else if (dimensionKey == DimensionEdge.Left || dimensionKey == DimensionEdge.Left)
                              {
                                if (list2Extreme.Y < list1Extreme.Y)
                                  flag33 = false;
                                double num40 = (list2Extreme.X + list1Extreme.X) / 2.0;
                                List<ReferencedPoint> refPoints7 = new List<ReferencedPoint>();
                                if (source20[index5].topLeft.HasValue && source20[index6].topLeft.HasValue)
                                {
                                  bool? topLeft3 = source20[index5].topLeft;
                                  bool? topLeft4 = source20[index6].topLeft;
                                  if (topLeft3.GetValueOrDefault() == topLeft4.GetValueOrDefault() & topLeft3.HasValue == topLeft4.HasValue)
                                  {
                                    if (source20[index5].topLeft.GetValueOrDefault())
                                    {
                                      refPoints7.AddRange((IEnumerable<ReferencedPoint>) targetLeftReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
                                      goto label_1200;
                                    }
                                    refPoints7.AddRange((IEnumerable<ReferencedPoint>) source15.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
                                    goto label_1200;
                                  }
                                }
                                refPoints7.AddRange((IEnumerable<ReferencedPoint>) targetLeftReferences.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
                                refPoints7.AddRange((IEnumerable<ReferencedPoint>) source15.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => targetLikeElementDictionary[majority].Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)).Contains<ElementId>(x.elementId) && targetLeftReferences.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (z => z.elementId)).Contains<ElementId>(x.elementId))).ToList<ReferencedPoint>());
label_1200:
                                List<ReferencedPoint> referencedPointList21 = this.checkReferencedPointsForLocalPoint(refPoints7, viewTransform2);
                                List<ReferencedPoint> refsList = new List<ReferencedPoint>();
                                foreach (ReferencedPoint referencedPoint in referencedPointList21)
                                {
                                  bool flag35 = true;
                                  foreach (DimensionInfo dimensionInfo in list23)
                                  {
                                    if (dimensionInfo.dimReferencesToPlace.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).Contains<ElementId>(referencedPoint.elementId))
                                    {
                                      flag35 = false;
                                      break;
                                    }
                                  }
                                  if (flag35)
                                    refsList.Add(referencedPoint);
                                }
                                foreach (ReferencedPoint sortReferencedPoint in this.sortReferencedPointList(refsList, dimensionKey))
                                {
                                  if (flag33)
                                  {
                                    if (sortReferencedPoint.LocalPoint.Y > num40)
                                      source20[index6].dimReferencesToPlace.Add(sortReferencedPoint);
                                    else if (sortReferencedPoint.LocalPoint.Y < num40)
                                      source20[index5].dimReferencesToPlace.Insert(0, sortReferencedPoint);
                                    else if (sortReferencedPoint.LocalPoint.Y == num40)
                                    {
                                      if (source20[index6].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.Y - sortReferencedPoint.LocalPoint.Y > source20[index5].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.Y - sortReferencedPoint.LocalPoint.Y)
                                        source20[index5].dimReferencesToPlace.Add(sortReferencedPoint);
                                      else
                                        source20[index6].dimReferencesToPlace.Insert(0, sortReferencedPoint);
                                    }
                                  }
                                  else if (sortReferencedPoint.LocalPoint.Y > num40)
                                    source20[index5].dimReferencesToPlace.Add(sortReferencedPoint);
                                  else if (sortReferencedPoint.LocalPoint.Y < num40)
                                    source20[index6].dimReferencesToPlace.Insert(0, sortReferencedPoint);
                                  else if (sortReferencedPoint.LocalPoint.Y == num40)
                                  {
                                    if (source20[index6].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.Y - sortReferencedPoint.LocalPoint.Y > source20[index5].dimReferencesToPlace.Last<ReferencedPoint>().LocalPoint.Y - sortReferencedPoint.LocalPoint.Y)
                                      source20[index5].dimReferencesToPlace.Add(sortReferencedPoint);
                                    else
                                      source20[index6].dimReferencesToPlace.Insert(0, sortReferencedPoint);
                                  }
                                  SortingInfoCloneTicketElement elementSortingObject = this.getElementSortingObject(sortReferencedPoint.elementId, revitDoc, locationInFormValues, locationInFormAnalyzer2);
                                  if (elementSortingObject != (SortingInfoCloneTicketElement) null && !dictionary17[dimensionKey].Contains(elementSortingObject))
                                    dictionary17[dimensionKey].Add(elementSortingObject);
                                }
                              }
                            }
                          }
                          else
                            break;
                        }
                      }
                    }
                  }
                }
              }
            }
            List<ElementId> elementIdList7 = new List<ElementId>();
            List<IndependentTag> list24 = new FilteredElementCollector(revitDoc, element7.Id).OfCategory(BuiltInCategory.OST_MultiCategoryTags).Cast<IndependentTag>().ToList<IndependentTag>();
            list24.AddRange((IEnumerable<IndependentTag>) new FilteredElementCollector(revitDoc, element7.Id).OfCategory(BuiltInCategory.OST_StructuralFramingTags).Cast<IndependentTag>().ToList<IndependentTag>());
            Dictionary<SortingInfoCloneTicketElement, List<IndependentTag>> dictionary33 = new Dictionary<SortingInfoCloneTicketElement, List<IndependentTag>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
            foreach (IndependentTag independentTag in list24)
            {
              SortingInfoCloneTicketElement elementSortingObject = this.getElementSortingObject(independentTag.GetTaggedReferences().FirstOrDefault<Reference>().ElementId, revitDoc, locationInFormValues, locationInFormAnalyzer1);
              if (elementSortingObject != (SortingInfoCloneTicketElement) null)
              {
                if (dictionary33.ContainsKey(elementSortingObject))
                  dictionary33[elementSortingObject].Add(independentTag);
                else
                  dictionary33.Add(elementSortingObject, new List<IndependentTag>()
                  {
                    independentTag
                  });
              }
            }
            Dictionary<IndependentTag, Dictionary<Reference, ElementId>> dictionary34 = new Dictionary<IndependentTag, Dictionary<Reference, ElementId>>();
            source16.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).ToList<ElementId>();
            source17.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).ToList<ElementId>();
            source18.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).ToList<ElementId>();
            source19.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).ToList<ElementId>();
            StructuralFramingBoundsObject framingBoundsObject1;
            if (boundsObjectDictOut1.ContainsKey(structuralFramingElement1.Id))
            {
              framingBoundsObject1 = boundsObjectDictOut1[structuralFramingElement1.Id];
            }
            else
            {
              framingBoundsObject1 = new StructuralFramingBoundsObject(structuralFramingElement1 as FamilyInstance, element7, true);
              boundsObjectDictOut1.Add(structuralFramingElement1.Id, framingBoundsObject1);
            }
            StructuralFramingBoundsObject framingBoundsObject2;
            if (boundsObjectDictOut2.ContainsKey(structuralFramingElement2.Id))
            {
              framingBoundsObject2 = boundsObjectDictOut2[structuralFramingElement2.Id];
            }
            else
            {
              framingBoundsObject2 = new StructuralFramingBoundsObject(structuralFramingElement2 as FamilyInstance, element8, true);
              boundsObjectDictOut2.Add(structuralFramingElement2.Id, framingBoundsObject2);
            }
            foreach (SortingInfoCloneTicketElement key27 in dictionary33.Keys)
            {
              List<IndependentTag> independentTagList = dictionary33[key27];
              List<Element> elementList2 = new List<Element>();
              if (targetLikeElementDictionary.ContainsKey(key27))
                elementList2.AddRange((IEnumerable<Element>) targetLikeElementDictionary[key27]);
              foreach (IndependentTag key28 in independentTagList)
              {
                Dictionary<Reference, ElementId> dictionary35 = new Dictionary<Reference, ElementId>();
                if (key27 == key5 || key27.sortingName == "A")
                {
                  ElementId id = assemblyInstance.GetStructuralFramingElement().Id;
                  dictionary35.Add(key28.GetTaggedReferences().First<Reference>(), id);
                }
                else
                {
                  foreach (Reference taggedReference in (IEnumerable<Reference>) key28.GetTaggedReferences())
                  {
                    if (!(taggedReference.ElementId == (ElementId) null))
                    {
                      LocationPoint location = revitDoc.GetElement(taggedReference.ElementId).Location as LocationPoint;
                      XYZ xyz8 = viewTransform1.Inverse.OfPoint(location.Point);
                      XYZ xyz9 = new XYZ(framingBoundsObject1.xMin, framingBoundsObject1.yMin, xyz8.Z);
                      XYZ xyz10 = xyz8 - xyz9;
                      double num41 = 0.0;
                      Element element26 = (Element) null;
                      for (int index = 0; index < elementList2.Count; ++index)
                      {
                        XYZ point = (elementList2[index].Location as LocationPoint).Point;
                        XYZ xyz11 = viewTransform2.Inverse.OfPoint(point);
                        XYZ xyz12 = new XYZ(framingBoundsObject2.xMin, framingBoundsObject2.yMin, xyz11.Z);
                        XYZ source28 = xyz11 - xyz12;
                        double num42 = xyz10.DistanceTo(source28);
                        if (num42 <= num1)
                        {
                          element26 = elementList2[index];
                          break;
                        }
                        if (index == 0)
                        {
                          element26 = elementList2[index];
                          num41 = num42;
                        }
                        else if (num42 < num41)
                        {
                          element26 = elementList2[index];
                          num41 = num42;
                        }
                      }
                      if (element26 != null)
                      {
                        ElementId id = element26.Id;
                        elementList2.Remove(element26);
                        dictionary35.Add(taggedReference, id);
                      }
                    }
                  }
                }
                if (dictionary35.Count > 0)
                  dictionary34.Add(key28, dictionary35);
              }
            }
            StructuralFramingBoundsObject sourceBoundsObject2;
            if (boundsObjectDictOut1.ContainsKey(structuralFramingElement1.Id))
            {
              sourceBoundsObject2 = boundsObjectDictOut1[structuralFramingElement1.Id];
            }
            else
            {
              sourceBoundsObject2 = new StructuralFramingBoundsObject(structuralFramingElement1 as FamilyInstance, element7, true);
              boundsObjectDictOut1.Add(structuralFramingElement1.Id, sourceBoundsObject2);
            }
            StructuralFramingBoundsObject targetBoundsObject2;
            if (boundsObjectDictOut2.ContainsKey(structuralFramingElement2.Id))
            {
              targetBoundsObject2 = boundsObjectDictOut2[structuralFramingElement2.Id];
            }
            else
            {
              targetBoundsObject2 = new StructuralFramingBoundsObject(structuralFramingElement2 as FamilyInstance, element8, true);
              boundsObjectDictOut2.Add(structuralFramingElement2.Id, targetBoundsObject2);
            }
            AssemblyBoundingBoxInfo assemblyBoundingBoxInfo = new AssemblyBoundingBoxInfo(viewTransform1, viewTransform2, sourceBoundingBox, targetBoundingBox, sourceBoundsObject2, targetBoundsObject2);
            targetBoundingBox.Transform.Inverse.OfPoint(targetBoundingBox.Max);
            targetBoundingBox.Transform.Inverse.OfPoint(targetBoundingBox.Min);
            foreach (IndependentTag key29 in dictionary34.Keys)
            {
              Element element27 = revitDoc.GetElement(dictionary34[key29].FirstOrDefault<KeyValuePair<Reference, ElementId>>().Value);
              Reference reference1 = new Reference(element27);
              Reference reference2 = key29.GetTaggedReferences().FirstOrDefault<Reference>();
              XYZ point3 = (revitDoc.GetElement(reference2).Location as LocationPoint).Point;
              XYZ point4 = (element27.Location as LocationPoint).Point;
              XYZ xyz13 = new XYZ();
              if (key29.HasLeader && key29.LeaderEndCondition == LeaderEndCondition.Free && reference2 != null)
              {
                XYZ leaderEnd = key29.GetLeaderEnd(reference2);
                xyz13 = viewTransform1.Inverse.OfPoint(leaderEnd) - viewTransform1.Inverse.OfPoint(point3);
              }
              XYZ tagHeadPosition = key29.TagHeadPosition;
              XYZ point5 = viewTransform1.Inverse.OfPoint(tagHeadPosition);
              XYZ pnt = viewTransform2.OfPoint(point5);
              bool flag36 = false;
              bool flag37 = false;
              if (assemblyBoundingBoxInfo.sourceXMax >= point5.X && assemblyBoundingBoxInfo.sourceXMin <= point5.X)
                flag36 = true;
              if (assemblyBoundingBoxInfo.sourceYMax >= point5.Y && assemblyBoundingBoxInfo.sourceYMin <= point5.Y)
                flag37 = true;
              XYZ xyz14 = (XYZ) null;
              if (flag36 & flag37 && Utils.ElementUtils.Parameters.GetParameterAsBool((Element) key29, "Leader Line"))
              {
                if (Utils.ElementUtils.Parameters.GetParameterAsInt((Element) key29, "Leader Type") != 0)
                  xyz14 = tagHeadPosition - key29.GetLeaderEnd(key29.GetTaggedReferences().First<Reference>());
              }
              else if (flag37)
              {
                if (assemblyBoundingBoxInfo.sourceXMax <= point5.X)
                {
                  XYZ xyz15 = viewTransform2.Inverse.OfVector(new XYZ(assemblyBoundingBoxInfo.targetXMax - assemblyBoundingBoxInfo.sourceXMax, 0.0, 0.0));
                  pnt += xyz15;
                }
                else if (assemblyBoundingBoxInfo.sourceXMin >= point5.X)
                {
                  XYZ xyz16 = viewTransform2.Inverse.OfVector(new XYZ(assemblyBoundingBoxInfo.targetXMin - assemblyBoundingBoxInfo.sourceXMin, 0.0, 0.0));
                  pnt += xyz16;
                }
              }
              else if (flag36)
              {
                if (assemblyBoundingBoxInfo.sourceYMax <= point5.Y)
                {
                  XYZ xyz17 = viewTransform2.Inverse.OfVector(new XYZ(0.0, assemblyBoundingBoxInfo.targetYMax - assemblyBoundingBoxInfo.sourceYMax, 0.0));
                  pnt += xyz17;
                }
                else if (assemblyBoundingBoxInfo.sourceYMin >= point5.Y)
                {
                  XYZ xyz18 = viewTransform2.Inverse.OfVector(new XYZ(0.0, assemblyBoundingBoxInfo.targetYMin - assemblyBoundingBoxInfo.sourceYMin, 0.0));
                  pnt += xyz18;
                }
              }
              else
              {
                if (assemblyBoundingBoxInfo.sourceXMax <= point5.X)
                {
                  XYZ xyz19 = viewTransform2.Inverse.OfVector(new XYZ(assemblyBoundingBoxInfo.targetXMax - assemblyBoundingBoxInfo.sourceXMax, 0.0, 0.0));
                  pnt += xyz19;
                }
                else if (assemblyBoundingBoxInfo.sourceXMin >= point5.X)
                {
                  XYZ xyz20 = viewTransform2.Inverse.OfVector(new XYZ(assemblyBoundingBoxInfo.targetXMin - assemblyBoundingBoxInfo.sourceXMin, 0.0, 0.0));
                  pnt += xyz20;
                }
                if (assemblyBoundingBoxInfo.sourceYMax <= point5.Y)
                {
                  XYZ xyz21 = viewTransform2.Inverse.OfVector(new XYZ(0.0, assemblyBoundingBoxInfo.targetYMax - assemblyBoundingBoxInfo.sourceYMax, 0.0));
                  pnt += xyz21;
                }
                else if (assemblyBoundingBoxInfo.sourceYMin >= point5.Y)
                {
                  XYZ xyz22 = viewTransform2.Inverse.OfVector(new XYZ(0.0, assemblyBoundingBoxInfo.targetYMin - assemblyBoundingBoxInfo.sourceYMin, 0.0));
                  pnt += xyz22;
                }
              }
              IndependentTag independentTag = !key29.IsMulticategoryTag ? IndependentTag.Create(revitDoc, element8.Id, reference1, key29.HasLeader, TagMode.TM_ADDBY_CATEGORY, key29.TagOrientation, pnt) : IndependentTag.Create(revitDoc, element8.Id, reference1, key29.HasLeader, TagMode.TM_ADDBY_MULTICATEGORY, key29.TagOrientation, pnt);
              if (independentTag != null)
              {
                elementIdList7.Add(reference1.ElementId);
                independentTag.ChangeTypeId(key29.GetTypeId());
                if (independentTag.HasLeader && !independentTag.IsMaterialTag)
                {
                  independentTag.TagHeadPosition = pnt;
                  independentTag.LeaderEndCondition = key29.LeaderEndCondition;
                  if (key29.LeaderEndCondition == LeaderEndCondition.Free)
                  {
                    XYZ point6 = viewTransform2.Inverse.OfPoint(point4) + xyz13;
                    XYZ pntEnd = viewTransform2.OfPoint(point6);
                    independentTag.SetLeaderEnd(reference1, pntEnd);
                    if (xyz14 != null)
                    {
                      XYZ xyz23 = pntEnd + xyz14;
                      independentTag.TagHeadPosition = xyz23;
                    }
                  }
                }
                List<Reference> referencesToTag = new List<Reference>();
                int num43 = 1;
                foreach (Reference key30 in dictionary34[key29].Keys)
                {
                  Reference referenceTagged = new Reference(revitDoc.GetElement(dictionary34[key29][key30]));
                  if (num43 != 1)
                    referencesToTag.Add(referenceTagged);
                  if (key29.HasLeader && key29.HasLeaderElbow(key30))
                  {
                    XYZ vec = viewTransform1.Inverse.OfPoint(key29.GetLeaderElbow(key30)) - viewTransform1.Inverse.OfPoint(key29.TagHeadPosition);
                    XYZ pntElbow = independentTag.TagHeadPosition + viewTransform2.OfVector(vec);
                    independentTag.SetLeaderElbow(referenceTagged, pntElbow);
                  }
                  ++num43;
                }
                if (referencesToTag.Count > 0)
                  independentTag.AddReferences((IList<Reference>) referencesToTag);
                independentTag.ChangeTypeId(key29.GetTypeId());
              }
            }
            TicketAutoDim_Command ticketAutoDimCommand = new TicketAutoDim_Command();
            foreach (DimensionInfo dimInfo in source20)
            {
              string warningMessage;
              if (!this.createDimension(revitDoc, dimInfo, element8, assemblyInstance, sourceAssembly, dimInfo.sourceDimension.DimensionType, autoTicketStyleSettings.textNoteStyle, ticketAppendString, minimumDimension, appendStringData, ticketCustomValues, assemblyBoundingBoxInfo, out warningMessage))
              {
                int num44 = (int) transaction2.RollBack();
                stringList1.Add(assemblyInstance.Name);
                dictionary12.Add(assemblyInstance.Name, $"Failed to clone a dimension ({dimInfo.sourceDimension.Id?.ToString()}).");
                flag14 = true;
                break;
              }
              if (warningMessage != string.Empty)
              {
                if (dictionary11.ContainsKey(assemblyInstance.Name))
                {
                  if (!dictionary11[assemblyInstance.Name].Contains(warningMessage))
                    dictionary11[assemblyInstance.Name].Add(warningMessage);
                }
                else
                  dictionary11.Add(assemblyInstance.Name, new List<string>()
                  {
                    warningMessage
                  });
              }
            }
            if (!flag14)
            {
              Element structuralFramingElement3 = assemblyInstance.GetStructuralFramingElement();
              List<SpreadSheetData> sheetsDataToReturn;
              AutoTicketHandler.checkParams(new List<ElementId>()
              {
                assemblyInstance.Id
              }, revitDoc, out sheetsDataToReturn);
              Dictionary<AssemblyInstance, List<SpreadSheetData>> dictionary36 = new Dictionary<AssemblyInstance, List<SpreadSheetData>>();
              Dictionary<AssemblyInstance, List<SpreadSheetData>> source29 = SpreadSheetData.assignType(sheetsDataToReturn, revitDoc);
              Dictionary<DimensionEdge, List<ElementId>> dictionary37 = new Dictionary<DimensionEdge, List<ElementId>>();
              dictionary37.Add(DimensionEdge.Top, new List<ElementId>());
              dictionary37.Add(DimensionEdge.Bottom, new List<ElementId>());
              dictionary37.Add(DimensionEdge.Right, new List<ElementId>());
              dictionary37.Add(DimensionEdge.Left, new List<ElementId>());
              foreach (DimensionInfo dimensionInfo in source20)
              {
                Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>> dictionary38 = new Dictionary<SortingInfoCloneTicketElement, List<List<ElementId>>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
                if (dimensionInfo.dimEdge == DimensionEdge.Top || dimensionInfo.dimEdge == DimensionEdge.Bottom)
                {
                  foreach (SortingInfoCloneTicketElement key31 in dictionary21.Keys)
                    dictionary38.Add(key31, dictionary21[key31]);
                }
                else
                {
                  foreach (SortingInfoCloneTicketElement key32 in dictionary20.Keys)
                    dictionary38.Add(key32, dictionary20[key32]);
                }
                foreach (ReferencedPoint referencedPoint in dimensionInfo.dimReferencesToPlace)
                {
                  ElementId elemId = !(referencedPoint.elementId != (ElementId) null) ? referencedPoint.Reference.ElementId : referencedPoint.elementId;
                  if (elemId != (ElementId) null)
                  {
                    dictionary37[dimensionInfo.dimEdge].Add(elemId);
                    SortingInfoCloneTicketElement elementSortingObject = this.getElementSortingObject(elemId, revitDoc, locationInFormValues, locationInFormAnalyzer2);
                    if (elementSortingObject != (SortingInfoCloneTicketElement) null && dictionary38.ContainsKey(elementSortingObject))
                    {
                      foreach (List<ElementId> collection in dictionary38[elementSortingObject])
                      {
                        if (collection.Contains(elemId))
                        {
                          dictionary37[dimensionInfo.dimEdge].AddRange((IEnumerable<ElementId>) collection);
                          break;
                        }
                      }
                    }
                    dictionary37[dimensionInfo.dimEdge] = dictionary37[dimensionInfo.dimEdge].Distinct<ElementId>().ToList<ElementId>();
                  }
                }
              }
              List<ElementId> source30 = new List<ElementId>();
              List<ElementId> elementIdList8 = new List<ElementId>();
              Dictionary<SortingInfoCloneTicketElement, Dictionary<DimensionEdge, bool>> cloneTicketSideDictionary = new Dictionary<SortingInfoCloneTicketElement, Dictionary<DimensionEdge, bool>>((IEqualityComparer<SortingInfoCloneTicketElement>) new SortingInfoCloneTicketElementComparer());
              foreach (SortingInfoCloneTicketElement key33 in targetLikeElementDictionary.Keys)
              {
                List<Element> source31 = targetLikeElementDictionary[key33];
                if (sourceLikeElementDictionary.ContainsKey(key33))
                {
                  List<ElementId> list25 = source31.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(targetTopReferences.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).ToList<ElementId>();
                  List<ElementId> list26 = source31.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(source14.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).ToList<ElementId>();
                  List<ElementId> list27 = source31.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(targetLeftReferences.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).ToList<ElementId>();
                  List<ElementId> list28 = source31.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(source15.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).ToList<ElementId>();
                  List<ElementId> list29 = sourceLikeElementDictionary[key33].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>();
                  Dictionary<DimensionEdge, bool> dictionary39 = new Dictionary<DimensionEdge, bool>()
                  {
                    {
                      DimensionEdge.Top,
                      false
                    },
                    {
                      DimensionEdge.Bottom,
                      false
                    },
                    {
                      DimensionEdge.Left,
                      false
                    },
                    {
                      DimensionEdge.Right,
                      false
                    }
                  };
                  cloneTicketSideDictionary.Add(key33, dictionary39);
                  if (list25.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Top]).Count<ElementId>() == 0 && list25.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Bottom]).Count<ElementId>() == 0 && list25.Count<ElementId>() != 0)
                  {
                    source30.AddRange((IEnumerable<ElementId>) list25);
                    if (!list29.Intersect<ElementId>(source16.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).Any<ElementId>() && list25.Except<ElementId>((IEnumerable<ElementId>) list26).Count<ElementId>() != 0)
                      cloneTicketSideDictionary[key33][DimensionEdge.Top] = true;
                  }
                  if (list26.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Top]).Count<ElementId>() == 0 && list26.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Bottom]).Count<ElementId>() == 0 && list26.Count<ElementId>() != 0)
                  {
                    source30.AddRange((IEnumerable<ElementId>) list26);
                    if (!list29.Intersect<ElementId>(source17.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).Any<ElementId>() && list26.Except<ElementId>((IEnumerable<ElementId>) list25).Count<ElementId>() != 0)
                      cloneTicketSideDictionary[key33][DimensionEdge.Bottom] = true;
                  }
                  if (list27.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Left]).Count<ElementId>() == 0 && list27.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Right]).Count<ElementId>() == 0 && list27.Count<ElementId>() != 0)
                  {
                    source30.AddRange((IEnumerable<ElementId>) list27);
                    if (!list29.Intersect<ElementId>(source19.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).Any<ElementId>() && list27.Except<ElementId>((IEnumerable<ElementId>) list28).Count<ElementId>() != 0)
                      cloneTicketSideDictionary[key33][DimensionEdge.Left] = true;
                  }
                  if (list28.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Left]).Count<ElementId>() == 0 && list28.Intersect<ElementId>((IEnumerable<ElementId>) dictionary37[DimensionEdge.Right]).Count<ElementId>() == 0 && list28.Count<ElementId>() != 0)
                  {
                    source30.AddRange((IEnumerable<ElementId>) list28);
                    if (!list29.Intersect<ElementId>(source18.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (y => y.elementId))).Any<ElementId>() && list28.Except<ElementId>((IEnumerable<ElementId>) list27).Count<ElementId>() != 0)
                      cloneTicketSideDictionary[key33][DimensionEdge.Right] = true;
                  }
                }
                else
                {
                  source30.AddRange(source31.Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)));
                  elementIdList8.AddRange(source31.Select<Element, ElementId>((Func<Element, ElementId>) (y => y.Id)));
                  Dictionary<DimensionEdge, bool> dictionary40 = new Dictionary<DimensionEdge, bool>()
                  {
                    {
                      DimensionEdge.Top,
                      true
                    },
                    {
                      DimensionEdge.Bottom,
                      true
                    },
                    {
                      DimensionEdge.Left,
                      true
                    },
                    {
                      DimensionEdge.Right,
                      true
                    }
                  };
                  cloneTicketSideDictionary.Add(key33, dictionary40);
                }
              }
              List<ElementId> list30 = source30.Distinct<ElementId>().ToList<ElementId>();
              foreach (ElementId id in list30.ToList<ElementId>())
              {
                string controlMark = revitDoc.GetElement(id).GetControlMark();
                if (controlMark == "" && revitDoc.GetElement(id) is FamilyInstance element28)
                  controlMark = element28.Symbol.FamilyName;
                if (controlMark != "" && !(id == structuralFramingElement3.Id))
                {
                  bool flag38 = false;
                  bool flag39 = false;
                  SpreadSheetData spreadSheetData = source29.Where<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>((Func<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>, bool>) (e => e.Key.Name == assemblyInstance.Name)).FirstOrDefault<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>().Value.Where<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.Name == controlMark)).FirstOrDefault<SpreadSheetData>();
                  if (spreadSheetData != null)
                  {
                    string name = element8.Name;
                    if (name.Contains("Top") || name.Contains("Bottom"))
                    {
                      bool? dimTopBottom = spreadSheetData.DimTopBottom;
                      bool flag40 = false;
                      if (dimTopBottom.GetValueOrDefault() == flag40 & dimTopBottom.HasValue)
                      {
                        list30.Remove(id);
                      }
                      else
                      {
                        dimTopBottom = spreadSheetData.DimTopBottom;
                        if (!dimTopBottom.HasValue)
                        {
                          flag38 = true;
                          flag39 = true;
                        }
                      }
                    }
                    else if (name.Contains("Front") || name.Contains("Back"))
                    {
                      bool? dimFrontBack = spreadSheetData.DimFrontBack;
                      bool flag41 = false;
                      if (dimFrontBack.GetValueOrDefault() == flag41 & dimFrontBack.HasValue)
                      {
                        list30.Remove(id);
                      }
                      else
                      {
                        dimFrontBack = spreadSheetData.DimFrontBack;
                        if (!dimFrontBack.HasValue)
                        {
                          flag38 = true;
                          flag39 = true;
                        }
                      }
                    }
                    else if (name.Contains("Left") || name.Contains("Right"))
                    {
                      bool? dimLeftRight = spreadSheetData.DimLeftRight;
                      bool flag42 = false;
                      if (dimLeftRight.GetValueOrDefault() == flag42 & dimLeftRight.HasValue)
                      {
                        list30.Remove(id);
                      }
                      else
                      {
                        dimLeftRight = spreadSheetData.DimLeftRight;
                        if (!dimLeftRight.HasValue)
                        {
                          flag38 = true;
                          flag39 = true;
                        }
                      }
                    }
                  }
                  else
                  {
                    flag38 = true;
                    flag39 = true;
                  }
                  if (flag38 && !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(revitDoc.GetElement(id) as FamilyInstance) && flag39)
                    list30.Remove(id);
                }
              }
              Dictionary<string, Dictionary<FormLocation, List<ElementId>>> dictionary41 = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
              Dictionary<string, Dictionary<FormLocation, List<ElementId>>> lifDict1 = AutoTicketHandler.createLIFDict(revitDoc, locationInFormAnalyzer2, (ICollection<ElementId>) list30, locationInFormValues);
              List<ElementId> viewVisible = new FilteredElementCollector(revitDoc, element8.Id).ToElementIds().ToList<ElementId>();
              foreach (string key34 in lifDict1.Keys)
              {
                foreach (FormLocation key35 in lifDict1[key34].Keys)
                  lifDict1[key34][key35].RemoveAll((Predicate<ElementId>) (eid => !viewVisible.Contains(eid)));
              }
              lifDict1.Remove(structuralFramingElement3.GetControlMark());
              bool bFirst = false;
              BatchAutoDimension.DimensionView(revitDoc, element8, bFirst, lifDict1, autoTicketStyleSettings, ticketAppendString, ticketCustomValues, minimumDimension, appendStringData, true, cloneTicketSideDictionary);
              bool flag43 = true;
              FilteredElementCollector source32 = new FilteredElementCollector(revitDoc);
              source32.OfCategory(BuiltInCategory.OST_MultiCategoryTags);
              source32.OfClass(typeof (FamilySymbol));
              if (source32.Count<Element>() == 0)
                flag43 = false;
              FamilySymbol leftTagFamily = (FamilySymbol) null;
              FamilySymbol rightTagFamily = (FamilySymbol) null;
              FamilySymbol leftTagFamilyTYP = (FamilySymbol) null;
              FamilySymbol rightTagFamilyTYP = (FamilySymbol) null;
              foreach (FamilySymbol familySymbol in source32)
              {
                string str11 = autoTicketStyleSettings != null ? autoTicketStyleSettings.calloutStyle : "AUTO_TICKET_CALLOUT";
                if (familySymbol.FamilyName == str11)
                {
                  if (familySymbol.Name == "Left Justified")
                    leftTagFamily = familySymbol;
                  else if (familySymbol.Name == "Left Justified TYP")
                    leftTagFamilyTYP = familySymbol;
                  else if (familySymbol.Name == "Right Justified")
                    rightTagFamily = familySymbol;
                  else if (familySymbol.Name == "Right Justified TYP")
                    rightTagFamilyTYP = familySymbol;
                }
              }
              if (leftTagFamilyTYP == null && leftTagFamily != null)
                leftTagFamilyTYP = leftTagFamily;
              if (rightTagFamilyTYP == null && rightTagFamily != null)
                rightTagFamilyTYP = rightTagFamily;
              foreach (ElementId id in elementIdList8.ToList<ElementId>())
              {
                string controlMark = revitDoc.GetElement(id).GetControlMark();
                if (controlMark == "" && revitDoc.GetElement(id) is FamilyInstance element29)
                  controlMark = element29.Symbol.FamilyName;
                if (controlMark != "" && !(id == structuralFramingElement3.Id))
                {
                  bool flag44 = false;
                  bool flag45 = false;
                  SpreadSheetData spreadSheetData = source29.Where<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>((Func<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>, bool>) (e => e.Key.Name == assemblyInstance.Name)).FirstOrDefault<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>().Value.Where<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.Name == controlMark)).FirstOrDefault<SpreadSheetData>();
                  if (spreadSheetData != null)
                  {
                    string name = element8.Name;
                    bool? nullable = spreadSheetData.CalloutAlways;
                    if (!nullable.HasValue)
                    {
                      nullable = spreadSheetData.CalloutDim;
                      if (!nullable.HasValue)
                      {
                        flag44 = true;
                        flag45 = true;
                        goto label_1478;
                      }
                    }
                    nullable = spreadSheetData.CalloutAlways;
                    bool flag46 = false;
                    if (nullable.GetValueOrDefault() == flag46 & nullable.HasValue)
                    {
                      nullable = spreadSheetData.CalloutDim;
                      if (!nullable.GetValueOrDefault())
                      {
                        nullable = spreadSheetData.CalloutDim;
                        if (nullable.HasValue)
                        {
                          nullable = spreadSheetData.CalloutDim;
                          bool flag47 = false;
                          if (nullable.GetValueOrDefault() == flag47 & nullable.HasValue)
                          {
                            elementIdList8.Remove(id);
                            goto label_1478;
                          }
                          goto label_1478;
                        }
                      }
                      if (name.Contains("Top") || name.Contains("Bottom"))
                      {
                        nullable = spreadSheetData.DimTopBottom;
                        bool flag48 = false;
                        if (nullable.GetValueOrDefault() == flag48 & nullable.HasValue)
                          elementIdList8.Remove(id);
                      }
                      else if (name.Contains("Front") || name.Contains("Back"))
                      {
                        nullable = spreadSheetData.DimFrontBack;
                        bool flag49 = false;
                        if (nullable.GetValueOrDefault() == flag49 & nullable.HasValue)
                          elementIdList8.Remove(id);
                      }
                      else if (name.Contains("Left") || name.Contains("Right"))
                      {
                        nullable = spreadSheetData.DimLeftRight;
                        bool flag50 = false;
                        if (nullable.GetValueOrDefault() == flag50 & nullable.HasValue)
                          elementIdList8.Remove(id);
                      }
                    }
label_1478:
                    if (flag44 && !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(revitDoc.GetElement(id) as FamilyInstance) && flag45)
                      elementIdList8.Remove(id);
                  }
                }
              }
              Dictionary<string, Dictionary<FormLocation, List<ElementId>>> dictionary42 = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
              Dictionary<string, Dictionary<FormLocation, List<ElementId>>> lifDict2 = AutoTicketHandler.createLIFDict(revitDoc, locationInFormAnalyzer2, (ICollection<ElementId>) elementIdList8, locationInFormValues);
              foreach (string key36 in lifDict2.Keys)
              {
                foreach (FormLocation key37 in lifDict2[key36].Keys)
                  lifDict2[key36][key37].RemoveAll((Predicate<ElementId>) (eid => !viewVisible.Contains(eid)));
              }
              lifDict2.Remove(structuralFramingElement3.GetControlMark());
              if (flag43)
                new AutoTicketCalloutHandler(element8, revitDoc, lifDict2).processAllCallOuts(leftTagFamily, rightTagFamily, leftTagFamilyTYP, rightTagFamilyTYP);
            }
            else
              break;
          }
          if (flag14)
            continue;
        }
        catch (Exception ex)
        {
          int num45 = (int) transaction2.RollBack();
          stringList1.Add(assemblyInstance.Name);
          dictionary12.Add(assemblyInstance.Name, "Failed to clone dimensions and callouts");
          continue;
        }
        if (activeUiDocument.Document.IsWorkshared)
        {
          ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
          elementsToCheckout.Add(assemblyInstance.Id);
          WorksharingUtils.CheckoutElements(activeUiDocument.Document, elementsToCheckout);
        }
        int num46 = (int) transaction2.Commit();
        source11.Add(assemblyInstance.Name);
      }
    }
    Dictionary<string, List<string>> me = new Dictionary<string, List<string>>();
    foreach (string key38 in dictionary11.Keys)
    {
      foreach (string key39 in dictionary11[key38])
      {
        if (me.ContainsKey(key39))
          me[key39].Add(key38);
        else
          me.Add(key39, new List<string>() { key38 });
      }
    }
    me.NatrualSort<KeyValuePair<string, List<string>>>((Func<KeyValuePair<string, List<string>>, string>) (kvp => kvp.Key));
    foreach (string key in me.Keys)
    {
      TaskDialog taskDialog5 = new TaskDialog("Clone Ticket Warning");
      taskDialog5.MainInstruction = key;
      if (key == "Failed to process one or more dimension references to non-structural framing elements. Please ensure that the element's family contains Edge Dim Lines and that the dimension references them and not element geometry. ")
      {
        taskDialog5.Show();
      }
      else
      {
        taskDialog5.MainContent = me[key].Count<string>() == 1 ? "Assembly :" : "Assemblies :";
        foreach (string str12 in me[key])
        {
          if (str12 != me[key].Last<string>())
          {
            TaskDialog taskDialog6 = taskDialog5;
            taskDialog6.MainContent = $"{taskDialog6.MainContent} {str12.Trim()},";
          }
          else
          {
            TaskDialog taskDialog7 = taskDialog5;
            taskDialog7.MainContent = $"{taskDialog7.MainContent} {str12.Trim()}.";
          }
        }
        taskDialog5.Show();
      }
    }
    if (source11.Count<string>() == list4.Count<AssemblyInstance>())
    {
      TaskDialog taskDialog8 = new TaskDialog("Clone Ticket");
      taskDialog8.MainContent = "All tickets successfully cloned.";
      taskDialog8.ExpandedContent = "Cloned Assemblies:\n";
      foreach (string str13 in source11)
      {
        TaskDialog taskDialog9 = taskDialog8;
        taskDialog9.ExpandedContent = $"{taskDialog9.ExpandedContent}{str13}\n";
      }
      taskDialog8.Show();
    }
    else if (source11.Count<string>() != 0)
    {
      TaskDialog taskDialog10 = new TaskDialog("Clone Ticket");
      taskDialog10.MainContent = "Clone Ticket is finished. One or more of the selected assemblies were not cloned.";
      taskDialog10.ExpandedContent = "Cloning Successful:\n";
      foreach (string str14 in source11)
      {
        TaskDialog taskDialog11 = taskDialog10;
        taskDialog11.ExpandedContent = $"{taskDialog11.ExpandedContent}{str14}\n";
      }
      taskDialog10.ExpandedContent += "\nCloning Failed:\n";
      foreach (string key in stringList1)
      {
        if (dictionary12.ContainsKey(key))
        {
          TaskDialog taskDialog12 = taskDialog10;
          taskDialog12.ExpandedContent = $"{taskDialog12.ExpandedContent}{key}: {dictionary12[key]}\n";
        }
        else
        {
          TaskDialog taskDialog13 = taskDialog10;
          taskDialog13.ExpandedContent = $"{taskDialog13.ExpandedContent}{key}\n";
        }
      }
      taskDialog10.Show();
    }
    else if (source11.Count<string>() == 0)
    {
      TaskDialog taskDialog14 = new TaskDialog("Clone Ticket");
      taskDialog14.MainContent = "Clone Ticket failed. No tickets were created.";
      taskDialog14.ExpandedContent += "Cloning Failed:\n";
      foreach (string key in stringList1)
      {
        if (dictionary12.ContainsKey(key))
        {
          TaskDialog taskDialog15 = taskDialog14;
          taskDialog15.ExpandedContent = $"{taskDialog15.ExpandedContent}{key}: {dictionary12[key]}\n";
        }
        else
        {
          TaskDialog taskDialog16 = taskDialog14;
          taskDialog16.ExpandedContent = $"{taskDialog16.ExpandedContent}{key}\n";
        }
      }
      taskDialog14.Show();
    }
    return (Result) 0;
  }

  public static string getSheetTitle(string sheetNumber, string name)
  {
    return $"Sheet {sheetNumber} - {name}";
  }

  private ViewSection createCustomSectionCut(
    Document revitDoc,
    AssemblyInstance targetAssemblyInstance,
    SectionViewInfo info,
    out bool failed)
  {
    Transform transform1 = (revitDoc.GetElement(info.sourceViewAssociatedAssemblyId) as AssemblyInstance).GetTransform();
    Transform inverse = transform1.Inverse;
    Transform transform2 = targetAssemblyInstance.GetTransform();
    Autodesk.Revit.DB.View element1 = revitDoc.GetElement(info.sourceViewId) as Autodesk.Revit.DB.View;
    failed = false;
    XYZ xyz1 = inverse.OfVector(info.sourceViewDirection);
    AssemblyDetailViewOrientation direction;
    if (xyz1.IsAlmostEqualTo(XYZ.BasisY.Negate()))
      direction = AssemblyDetailViewOrientation.ElevationFront;
    else if (xyz1.IsAlmostEqualTo(XYZ.BasisZ.Negate()))
      direction = AssemblyDetailViewOrientation.ElevationBottom;
    else if (xyz1.IsAlmostEqualTo(XYZ.BasisX.Negate()))
      direction = AssemblyDetailViewOrientation.ElevationLeft;
    else if (xyz1.IsAlmostEqualTo(XYZ.BasisY))
      direction = AssemblyDetailViewOrientation.ElevationBack;
    else if (xyz1.IsAlmostEqualTo(XYZ.BasisX))
    {
      direction = AssemblyDetailViewOrientation.ElevationRight;
    }
    else
    {
      if (!xyz1.IsAlmostEqualTo(XYZ.BasisZ))
        return (ViewSection) null;
      direction = AssemblyDetailViewOrientation.ElevationTop;
    }
    ViewSection detailSection = AssemblyViewUtils.CreateDetailSection(revitDoc, targetAssemblyInstance.Id, direction);
    if (!this.setDetailViewSectionName(detailSection, info.sourceViewName, out string _))
    {
      failed = true;
      return (ViewSection) null;
    }
    detailSection.Scale = info.sourceViewScale;
    detailSection.CropBoxActive = info.cropBoxActive;
    detailSection.CropBoxVisible = info.cropBoxVisible;
    XYZ vec = inverse.OfVector(info.sourceViewCropBoxTransform.BasisX);
    transform2.OfVector(vec).Normalize();
    ElementId referencingDetail = detailSection.GetParameter(ParameterTypeId.IdParam).AsElementId();
    Element element2 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Viewers).Where<Element>((Func<Element, bool>) (elem => elem.GetParameter(ParameterTypeId.IdParam).AsElementId().Equals((object) referencingDetail))).ToList<Element>().FirstOrDefault<Element>();
    XYZ endpoint1 = (detailSection.CropBox.Min + detailSection.CropBox.Max) / 2.0;
    Line bound = Line.CreateBound(endpoint1, endpoint1 + detailSection.CropBox.Transform.BasisZ);
    double viewCropRotation = EDGERCreateTemplate.GetSectionViewCropRotation(element1 as ViewSection, transform1, true);
    ElementTransformUtils.RotateElement(revitDoc, element2.Id, bound, viewCropRotation);
    XYZ xyz2 = info.sourceViewCropBoxTransform.Origin - transform1.Origin;
    XYZ xyz3 = detailSection.CropBox.Transform.Origin + xyz2 - detailSection.CropBox.Transform.Origin;
    ElementTransformUtils.MoveElement(revitDoc, element2.Id, xyz2 - xyz3);
    BoundingBoxXYZ boundingBoxXyz = new BoundingBoxXYZ();
    boundingBoxXyz.Max = new XYZ(info.sourceCropBox.Max.X, info.sourceCropBox.Max.Y, info.sourceCropBox.Max.Z);
    boundingBoxXyz.Min = new XYZ(info.sourceCropBox.Min.X, info.sourceCropBox.Min.Y, info.sourceCropBox.Min.Z);
    Transform transform3 = new Transform(detailSection.CropBox.Transform);
    boundingBoxXyz.Transform = transform3;
    detailSection.CropBox = boundingBoxXyz;
    return detailSection;
  }

  private bool setDetailViewSectionName(
    ViewSection viewSection,
    string sourceViewName,
    out string NewName)
  {
    NewName = sourceViewName;
    try
    {
      viewSection.Name = sourceViewName;
    }
    catch (Exception ex)
    {
      if (!ex.Message.Contains("Name must be unique.\r\nParameter name: name"))
        return true;
      string oldValue = sourceViewName.Trim();
      char[] charArray = oldValue.ToCharArray();
      bool flag1 = false;
      for (int index = charArray.Length - 1; index > -1; --index)
      {
        if (char.IsWhiteSpace(charArray[index]))
          flag1 = true;
        if (char.IsNumber(charArray[index]))
        {
          oldValue = oldValue.Remove(oldValue.Length - 1, 1);
        }
        else
        {
          if (!flag1)
          {
            oldValue = sourceViewName.Trim();
            break;
          }
          break;
        }
      }
      if (string.IsNullOrWhiteSpace(oldValue))
        oldValue = sourceViewName.Trim();
      int result;
      int num1 = int.TryParse(sourceViewName.Replace(oldValue, "").Trim(), out result) ? 1 : 0;
      int num2 = 0;
      string str = oldValue.Trim();
      if (num1 != 0)
      {
        int num3 = result;
        int num4;
        return this.setDetailViewSectionName(viewSection, $"{str} {(num4 = num3 + 1).ToString()}", out NewName);
      }
      bool flag2 = false;
      while (!flag2)
        flag2 = this.setDetailViewSectionName(viewSection, $"{str} {(++num2).ToString()}", out NewName);
    }
    return true;
  }

  public static void copyViewParameters(Document revitDoc, ElementId sourceViewId, Viewport target)
  {
    Autodesk.Revit.DB.View element1 = revitDoc.GetElement(sourceViewId) as Autodesk.Revit.DB.View;
    Autodesk.Revit.DB.View element2 = revitDoc.GetElement(target.ViewId) as Autodesk.Revit.DB.View;
    Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter((Element) element2, "Detail Level");
    Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter((Element) element2, "Parts Visibility");
    Parameter parameter3 = Utils.ElementUtils.Parameters.LookupParameter((Element) element2, "Discipline");
    Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter((Element) element2, "View Template");
    Parameter parameter5 = Utils.ElementUtils.Parameters.LookupParameter((Element) element2, "Title on Sheet");
    Parameter parameter6 = Utils.ElementUtils.Parameters.LookupParameter((Element) element2, "IS_CLOUD_VIEW");
    parameter4.Set(Utils.ElementUtils.Parameters.GetParameterAsElementId((Element) element1, "View Template"));
    if (!parameter1.IsReadOnly)
      parameter1.Set(Utils.ElementUtils.Parameters.GetParameterAsInt((Element) element1, "Detail Level"));
    if (!parameter2.IsReadOnly)
      parameter2.Set(Utils.ElementUtils.Parameters.GetParameterAsInt((Element) element1, "Parts Visibility"));
    if (!parameter3.IsReadOnly)
      parameter3.Set(Utils.ElementUtils.Parameters.GetParameterAsInt((Element) element1, "Discipline"));
    if (!parameter5.IsReadOnly)
      parameter5.Set(Utils.ElementUtils.Parameters.GetParameterAsString((Element) element1, "Title on Sheet"));
    if (parameter6.IsReadOnly)
      return;
    parameter6.Set(Utils.ElementUtils.Parameters.GetParameterAsInt((Element) element1, "IS_CLOUD_VIEW"));
  }

  private bool StrandPatternMatchesDesignNumber(string legendName, string designNumber)
  {
    return legendName.ToUpper().Contains("STRAND PATTERN " + designNumber.ToUpper()) || legendName.ToUpper().Contains("END PATTERN " + designNumber.ToUpper()) || legendName.ToUpper().Contains("REINFORCING PATTERN " + designNumber.ToUpper());
  }

  private bool copyAnnotations(
    Document revitDoc,
    Element viewSheetCopyFrom,
    Element viewSheetCopyTo,
    bool Sheet = false)
  {
    string g = "f84b5886-3580-4842-9f4b-9f08ce6644d8";
    Schema schema = Schema.Lookup(new Guid(g));
    if (schema == null)
    {
      SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("f84b5886-3580-4842-9f4b-9f08ce6644d8"));
      schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
      schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
      schemaBuilder.AddSimpleField("PTACDimensionGuid", typeof (string)).SetDocumentation("Identifier for PTAC's dimension labels.");
      schemaBuilder.SetSchemaName("PTAC_Dimensions");
      schema = schemaBuilder.Finish();
    }
    Autodesk.Revit.DB.View view = viewSheetCopyFrom as Autodesk.Revit.DB.View;
    XYZ origin1 = (revitDoc.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance).GetTransform().Origin;
    Autodesk.Revit.DB.View destinationView = viewSheetCopyTo as Autodesk.Revit.DB.View;
    XYZ origin2 = (revitDoc.GetElement(destinationView.AssociatedAssemblyInstanceId) as AssemblyInstance).GetTransform().Origin;
    if (view.ViewType == ViewType.ThreeD || view.ViewType == ViewType.Schedule)
      return true;
    DetailCurve detailCurve = (DetailCurve) null;
    if (!Sheet)
    {
      Line bound = Line.CreateBound(origin1, origin1 + view.UpDirection);
      detailCurve = revitDoc.Create.NewDetailCurve(view, (Curve) bound);
      if (revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories.Contains("EdgeRotationLine"))
      {
        detailCurve.LineStyle = (Element) revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories.get_Item("EdgeRotationLine").GetGraphicsStyle(GraphicsStyleType.Projection);
      }
      else
      {
        TaskDialog.Show("Clone Ticket", "Line Style \"EdgeRotationLine\" not found, please run the shared parameter updater and try again.");
        revitDoc.Delete(detailCurve.Id);
        return false;
      }
    }
    FilteredElementCollector source = new FilteredElementCollector(revitDoc, view.Id).OwnedByView(view.Id);
    List<ElementId> elementsToCopy = new List<ElementId>();
    foreach (Element element in source.ToList<Element>())
    {
      if (!Sheet)
      {
        if (element.Category != null && (element is TextNote || element is FilledRegion || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_DetailComponents).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_RevisionClouds).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_InsulationLines).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_GenericAnnotation).Id))
        {
          if (element is TextNote && element.GetEntitySchemaGuids().Contains(new Guid(g)))
          {
            Entity entity = element.GetEntity(schema);
            if (entity != null && entity.Get<string>(schema.GetField("PTACDimensionGuid")).Equals(element.UniqueId))
              continue;
          }
          elementsToCopy.Add(element.Id);
        }
      }
      else if (element.Category != null && (element is TextNote || element is FilledRegion || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_DetailComponents).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_GenericAnnotation).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_RevisionClouds).Id || element.Category.Id == revitDoc.Settings.Categories.get_Item(BuiltInCategory.OST_MultiCategoryTags).Id))
      {
        if (element is TextNote && element.GetEntitySchemaGuids().Contains(new Guid(g)))
        {
          Entity entity = element.GetEntity(schema);
          if (entity != null && entity.Get<string>(schema.GetField("PTACDimensionGuid")).Equals(element.UniqueId))
            continue;
        }
        if (!(element is AnnotationSymbol))
          elementsToCopy.Add(element.Id);
      }
    }
    if (elementsToCopy.Count < 1)
      return true;
    CopyPasteOptions options = new CopyPasteOptions();
    ICollection<ElementId> elementIds = ElementTransformUtils.CopyElements(view, (ICollection<ElementId>) elementsToCopy, destinationView, Transform.Identity, options);
    if (!Sheet)
    {
      foreach (ElementId id in elementIds.ToList<ElementId>())
      {
        if (revitDoc.GetElement(id) == null)
          elementIds.Remove(id);
      }
      XYZ xyz1 = new XYZ();
      XYZ xyz2 = new XYZ();
      foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
      {
        if (revitDoc.GetElement(id) is DetailCurve && (revitDoc.GetElement(id) as DetailCurve).LineStyle.Name == "EdgeRotationLine")
        {
          DetailCurve element = revitDoc.GetElement(id) as DetailCurve;
          Line geometryCurve = element.GeometryCurve as Line;
          XYZ origin3 = geometryCurve.Origin;
          XYZ direction = geometryCurve.Direction;
          double num = destinationView.RightDirection.DotProduct(direction);
          double d = Math.Max(Math.Min(destinationView.UpDirection.DotProduct(direction), 1.0), -1.0);
          double angle = num <= 0.0 ? -Math.Acos(d) : Math.Acos(d);
          ElementTransformUtils.RotateElements(revitDoc, elementIds, Line.CreateBound(origin3, origin3 + destinationView.ViewDirection), angle);
          XYZ translation = origin2 - origin3;
          ElementTransformUtils.MoveElements(revitDoc, elementIds, translation);
          revitDoc.Delete(detailCurve.Id);
          revitDoc.Delete(element.Id);
          return true;
        }
      }
    }
    return true;
  }

  private GeometryObject DONOTUSEGetGeometryObjectFromReference(
    Reference symbolRef,
    Document dbDoc,
    Autodesk.Revit.DB.View sourceView)
  {
    Options options = new Options();
    options.ComputeReferences = true;
    options.View = sourceView;
    options.IncludeNonVisibleObjects = true;
    Element element = dbDoc.GetElement(symbolRef.ElementId);
    string[] strArray = symbolRef.ConvertToStableRepresentation(dbDoc).Split(':');
    string str = $"{strArray[3]}:{strArray[4]}:{strArray[5]}";
    foreach (GeometryObject geometryObject in element.get_Geometry(options))
    {
      GeometryInstance geometryInstance = geometryObject as GeometryInstance;
      if ((GeometryObject) geometryInstance != (GeometryObject) null)
      {
        foreach (GeometryObject objectFromReference1 in geometryInstance.GetInstanceGeometry())
        {
          switch (objectFromReference1)
          {
            case Line _:
              Line line = objectFromReference1 as Line;
              if (line.Reference != null)
              {
                string stableRepresentation = line.Reference.ConvertToStableRepresentation(dbDoc);
                if (str.Contains(stableRepresentation))
                  return objectFromReference1;
                continue;
              }
              continue;
            case Curve _:
              if ((objectFromReference1 as Curve).Reference != null)
              {
                string stableRepresentation = (objectFromReference1 as Curve).Reference.ConvertToStableRepresentation(dbDoc);
                if (str.Contains(stableRepresentation))
                  return objectFromReference1;
                continue;
              }
              continue;
            case Solid _:
              Solid solid = objectFromReference1 as Solid;
              if ((GeometryObject) solid != (GeometryObject) null)
              {
                foreach (Edge edge in solid.Edges)
                {
                  if (edge.Reference != null)
                  {
                    string stableRepresentation = edge.Reference.ConvertToStableRepresentation(dbDoc);
                    if (str.Contains(stableRepresentation))
                      return (GeometryObject) edge;
                  }
                }
              }
              IEnumerator enumerator = solid.Faces.GetEnumerator();
              try
              {
                while (enumerator.MoveNext())
                {
                  GeometryObject current = (GeometryObject) enumerator.Current;
                  if (current is PlanarFace)
                  {
                    PlanarFace objectFromReference2 = current as PlanarFace;
                    if (objectFromReference2.Reference != null)
                    {
                      string stableRepresentation = objectFromReference2.Reference.ConvertToStableRepresentation(dbDoc);
                      if (str.Contains(stableRepresentation))
                        return (GeometryObject) objectFromReference2;
                    }
                    else
                      continue;
                  }
                  if (current is CylindricalFace)
                  {
                    CylindricalFace objectFromReference3 = current as CylindricalFace;
                    if (objectFromReference3.Reference != null)
                    {
                      string stableRepresentation = objectFromReference3.Reference.ConvertToStableRepresentation(dbDoc);
                      if (str.Contains(stableRepresentation))
                        return (GeometryObject) objectFromReference3;
                    }
                  }
                }
                continue;
              }
              finally
              {
                if (enumerator is IDisposable disposable)
                  disposable.Dispose();
              }
            default:
              continue;
          }
        }
      }
    }
    GeometryObject objectFromReference = element.GetGeometryObjectFromReference(symbolRef);
    return objectFromReference != (GeometryObject) null ? objectFromReference : (GeometryObject) null;
  }

  private List<ReferencedPoint> GetStructuralFramingReferencesList(
    FamilyInstance sfElem,
    DimensionEdge dimEdge,
    Autodesk.Revit.DB.View view,
    List<Solid> solidList,
    bool bSymbol,
    double samePointTolerance)
  {
    List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
    List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
    List<ReferencedPoint> list = new List<ReferencedPoint>();
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    foreach (Solid solid in solidList)
    {
      if ((GeometryObject) solid != (GeometryObject) null)
      {
        foreach (Edge edge in solid.Edges)
        {
          int index;
          Reference endPointReference = this.getCurveEndPointReference(edge, sfElem, viewTransform, dimEdge, out index);
          if (index != -1)
          {
            XYZ point = edge.AsCurve().GetEndPoint(index);
            if (bSymbol)
              point = sfElem.GetTransform().OfPoint(point);
            XYZ localPoint = viewTransform.Inverse.OfPoint(point);
            if (endPointReference != null)
            {
              ReferencedPoint referencedPoint = new ReferencedPoint(endPointReference, point, localPoint, sfElem.Id);
              if (referencedPoint != null)
                referencedPointList2 = this.CheckAndUpdateListForSamePoint(referencedPointList2, referencedPoint, dimEdge);
            }
          }
        }
        foreach (GeometryObject face in solid.Faces)
        {
          if (face is PlanarFace)
          {
            PlanarFace planarFace = face as PlanarFace;
            Reference reference = planarFace.Reference;
            XYZ point = planarFace.Origin;
            if (bSymbol)
              point = sfElem.GetTransform().OfPoint(point);
            XYZ localPoint = viewTransform.Inverse.OfPoint(point);
            if (reference != null)
            {
              ReferencedPoint referencedPoint = new ReferencedPoint(reference, point, localPoint, sfElem.Id);
              if (referencedPoint != null)
                list = this.CheckAndUpdateListForSamePoint(list, referencedPoint, dimEdge);
            }
          }
        }
      }
    }
    referencedPointList1.AddRange((IEnumerable<ReferencedPoint>) referencedPointList2);
    foreach (ReferencedPoint referencedPoint in list)
    {
      ReferencedPoint refPoint = referencedPoint;
      switch (dimEdge)
      {
        case DimensionEdge.Left:
        case DimensionEdge.Right:
          if (!referencedPointList1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.Y.ApproximatelyEquals(refPoint.LocalPoint.Y))).Any<ReferencedPoint>())
          {
            referencedPointList1.Add(refPoint);
            continue;
          }
          continue;
        case DimensionEdge.Top:
        case DimensionEdge.Bottom:
          if (!referencedPointList1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.X.ApproximatelyEquals(refPoint.LocalPoint.X))).Any<ReferencedPoint>())
          {
            referencedPointList1.Add(refPoint);
            continue;
          }
          continue;
        default:
          continue;
      }
    }
    return this.sortReferencedPointList(referencedPointList1, dimEdge);
  }

  private List<ReferencedPoint> DONOTUSEGetElementReferencesList(
    FamilyInstance elem,
    DimensionEdge dimEdge,
    Autodesk.Revit.DB.View view,
    double samePointTolerance)
  {
    Options options = new Options();
    options.ComputeReferences = true;
    options.View = view;
    options.IncludeNonVisibleObjects = true;
    List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
    List<ReferencedPoint> collection = new List<ReferencedPoint>();
    List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
    List<ReferencedPoint> referencedPointList3 = new List<ReferencedPoint>();
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    foreach (GeometryObject geometryObject1 in elem.get_Geometry(options))
    {
      GeometryInstance geometryInstance = geometryObject1 as GeometryInstance;
      if ((GeometryObject) geometryInstance != (GeometryObject) null)
      {
        foreach (GeometryObject geometryObject2 in geometryInstance.GetInstanceGeometry())
        {
          if (geometryObject2 is Line)
          {
            Line line = geometryObject2 as Line;
            int index;
            Reference reffy = this.DONOTUSEgetLineEndPointReference(line, viewTransform, dimEdge, out index);
            if (index != -1)
            {
              XYZ endPoint = line.GetEndPoint(index);
              XYZ localPoint = viewTransform.Inverse.OfPoint(endPoint);
              if (reffy == null || new ReferencedPoint(reffy, endPoint, localPoint, elem.Id) == null)
                ;
            }
          }
          else if (geometryObject2 is Solid)
          {
            Solid solid = geometryObject2 as Solid;
            if ((GeometryObject) solid != (GeometryObject) null)
            {
              foreach (Edge edge in solid.Edges)
              {
                int index;
                Reference endPointReference = this.getCurveEndPointReference(edge, elem, viewTransform, dimEdge, out index);
                if (index != -1)
                {
                  XYZ endPoint = edge.AsCurve().GetEndPoint(index);
                  XYZ localPoint = viewTransform.Inverse.OfPoint(endPoint);
                  if (endPointReference != null)
                  {
                    ReferencedPoint referencedPoint = new ReferencedPoint(endPointReference, endPoint, localPoint, elem.Id);
                  }
                }
              }
            }
            foreach (GeometryObject face in solid.Faces)
            {
              if (face is PlanarFace)
              {
                PlanarFace planarFace = face as PlanarFace;
                Reference reference = planarFace.Reference;
                XYZ origin = planarFace.Origin;
                XYZ localPoint = viewTransform.Inverse.OfPoint(origin);
                ReferencedPoint referencedPoint = new ReferencedPoint(reference, origin, localPoint, elem.Id);
              }
              if (face is CylindricalFace)
              {
                CylindricalFace cylindricalFace = face as CylindricalFace;
                Reference reference = cylindricalFace.Reference;
                XYZ origin = cylindricalFace.Origin;
                XYZ localPoint = viewTransform.Inverse.OfPoint(origin);
                ReferencedPoint referencedPoint = new ReferencedPoint(reference, origin, localPoint, elem.Id);
              }
            }
          }
        }
      }
    }
    referencedPointList1.AddRange((IEnumerable<ReferencedPoint>) collection);
    foreach (ReferencedPoint referencedPoint in referencedPointList2)
    {
      ReferencedPoint refPoint = referencedPoint;
      switch (dimEdge)
      {
        case DimensionEdge.Left:
        case DimensionEdge.Right:
          if (!referencedPointList1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.Y.ApproximatelyEquals(refPoint.LocalPoint.Y, samePointTolerance))).Any<ReferencedPoint>())
          {
            referencedPointList1.Add(refPoint);
            continue;
          }
          continue;
        case DimensionEdge.Top:
        case DimensionEdge.Bottom:
          if (!referencedPointList1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.X.ApproximatelyEquals(refPoint.LocalPoint.X, samePointTolerance))).Any<ReferencedPoint>())
          {
            referencedPointList1.Add(refPoint);
            continue;
          }
          continue;
        default:
          continue;
      }
    }
    foreach (ReferencedPoint referencedPoint in referencedPointList3)
    {
      ReferencedPoint refPoint = referencedPoint;
      switch (dimEdge)
      {
        case DimensionEdge.Left:
        case DimensionEdge.Right:
          if (!referencedPointList1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.Y.ApproximatelyEquals(refPoint.LocalPoint.Y, samePointTolerance))).Any<ReferencedPoint>())
          {
            referencedPointList1.Add(refPoint);
            continue;
          }
          continue;
        case DimensionEdge.Top:
        case DimensionEdge.Bottom:
          if (!referencedPointList1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.X.ApproximatelyEquals(refPoint.LocalPoint.X, samePointTolerance))).Any<ReferencedPoint>())
          {
            referencedPointList1.Add(refPoint);
            continue;
          }
          continue;
        default:
          continue;
      }
    }
    return this.sortReferencedPointList(referencedPointList1, dimEdge);
  }

  private List<ReferencedPoint> CheckAndUpdateListForSamePoint(
    List<ReferencedPoint> list,
    ReferencedPoint referencedPoint,
    DimensionEdge dimEdge)
  {
    switch (dimEdge)
    {
      case DimensionEdge.Left:
      case DimensionEdge.Right:
        List<ReferencedPoint> list1 = list.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.Y.ApproximatelyEquals(referencedPoint.LocalPoint.Y))).ToList<ReferencedPoint>();
        if (list1.Count<ReferencedPoint>() > 0)
        {
          ReferencedPoint referencedPoint1 = list1.FirstOrDefault<ReferencedPoint>();
          if (dimEdge == DimensionEdge.Right)
          {
            if (referencedPoint1.LocalPoint.X < referencedPoint.LocalPoint.X)
            {
              list.Remove(referencedPoint1);
              list.Add(referencedPoint);
              break;
            }
            break;
          }
          if (referencedPoint1.LocalPoint.X > referencedPoint.LocalPoint.X)
          {
            list.Remove(referencedPoint1);
            list.Add(referencedPoint);
            break;
          }
          break;
        }
        list.Add(referencedPoint);
        break;
      case DimensionEdge.Top:
      case DimensionEdge.Bottom:
        List<ReferencedPoint> list2 = list.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (x => x.LocalPoint.X.ApproximatelyEquals(referencedPoint.LocalPoint.X))).ToList<ReferencedPoint>();
        if (list2.Count<ReferencedPoint>() > 0)
        {
          ReferencedPoint referencedPoint2 = list2.FirstOrDefault<ReferencedPoint>();
          if (dimEdge == DimensionEdge.Top)
          {
            if (referencedPoint2.LocalPoint.Y < referencedPoint.LocalPoint.Y)
            {
              list.Remove(referencedPoint2);
              list.Add(referencedPoint);
              break;
            }
            break;
          }
          if (referencedPoint2.LocalPoint.Y > referencedPoint.LocalPoint.Y)
          {
            list.Remove(referencedPoint2);
            list.Add(referencedPoint);
            break;
          }
          break;
        }
        list.Add(referencedPoint);
        break;
    }
    return list;
  }

  private Dictionary<DimensionEdge, List<ReferencedPoint>> DONOTUSEGetStructuralFramingTargetReferencesDictionary(
    FamilyInstance sfElem,
    Autodesk.Revit.DB.View view,
    double samePointTolerance)
  {
    return new Dictionary<DimensionEdge, List<ReferencedPoint>>();
  }

  private Curve DONOTUSEGetGeometryCurveFromReference(
    Reference symbolRef,
    Document dbDoc,
    Autodesk.Revit.DB.View sourceView)
  {
    GeometryObject objectFromReference = this.DONOTUSEGetGeometryObjectFromReference(symbolRef, dbDoc, sourceView);
    switch (objectFromReference)
    {
      case Curve _:
        return objectFromReference as Curve;
      case Edge _:
        return (objectFromReference as Edge).AsCurve();
      default:
        return (Curve) null;
    }
  }

  private List<ReferencedPoint> DONOTUSEGetStructuralFramingReferencePoints(
    Element structFraming,
    DimensionEdge dimEdge,
    Autodesk.Revit.DB.View view)
  {
    List<ReferencedPoint> source = new List<ReferencedPoint>();
    Options options = new Options();
    options.ComputeReferences = true;
    options.View = view;
    Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    foreach (GeometryObject geometryObject1 in structFraming.get_Geometry(options))
    {
      GeometryInstance geometryInstance = geometryObject1 as GeometryInstance;
      if ((GeometryObject) geometryInstance != (GeometryObject) null)
      {
        foreach (GeometryObject geometryObject2 in geometryInstance.GetInstanceGeometry())
        {
          if (geometryObject2 is Curve)
          {
            Reference reference1 = (geometryObject2 as Curve).Reference;
          }
          else if (geometryObject2 is Solid)
          {
            Solid solid = geometryObject2 as Solid;
            if ((GeometryObject) solid != (GeometryObject) null)
            {
              foreach (Edge edge in solid.Edges)
              {
                XYZ direction = (edge.AsCurve() as Line).Direction;
                if ((!direction.IsAlmostEqualTo(view.UpDirection) && !direction.Negate().IsAlmostEqualTo(view.UpDirection) || dimEdge != DimensionEdge.Left && dimEdge != DimensionEdge.Right) && (!direction.IsAlmostEqualTo(view.RightDirection) && !direction.Negate().IsAlmostEqualTo(view.RightDirection) || dimEdge != DimensionEdge.Top && dimEdge != DimensionEdge.Bottom))
                {
                  Reference reference2 = edge.Reference;
                }
              }
            }
          }
        }
      }
    }
    return source.Distinct<ReferencedPoint>().ToList<ReferencedPoint>();
  }

  private Dictionary<DimensionEdge, List<ReferencedPoint>> GetElementsReferncePlanesAndLines(
    Document revitDoc,
    Element element,
    Autodesk.Revit.DB.View view,
    Transform viewTransform)
  {
    Dictionary<DimensionEdge, List<ReferencedPoint>> referncePlanesAndLines = new Dictionary<DimensionEdge, List<ReferencedPoint>>();
    referncePlanesAndLines.Add(DimensionEdge.Top, new List<ReferencedPoint>());
    referncePlanesAndLines.Add(DimensionEdge.Bottom, new List<ReferencedPoint>());
    referncePlanesAndLines.Add(DimensionEdge.Left, new List<ReferencedPoint>());
    referncePlanesAndLines.Add(DimensionEdge.Right, new List<ReferencedPoint>());
    XYZ rightDirection = view.RightDirection;
    XYZ upDirection = view.UpDirection;
    List<FamilyInstanceReferenceType> instanceReferenceTypeList = new List<FamilyInstanceReferenceType>()
    {
      FamilyInstanceReferenceType.Left,
      FamilyInstanceReferenceType.CenterLeftRight,
      FamilyInstanceReferenceType.Right,
      FamilyInstanceReferenceType.Front,
      FamilyInstanceReferenceType.CenterFrontBack,
      FamilyInstanceReferenceType.Back,
      FamilyInstanceReferenceType.Bottom,
      FamilyInstanceReferenceType.CenterElevation,
      FamilyInstanceReferenceType.Top,
      FamilyInstanceReferenceType.StrongReference,
      FamilyInstanceReferenceType.WeakReference
    };
    List<ReferencedPoint> collection1 = new List<ReferencedPoint>();
    List<ReferencedPoint> collection2 = new List<ReferencedPoint>();
    using (SubTransaction subTransaction = new SubTransaction(revitDoc))
    {
      int num1 = (int) subTransaction.Start();
      foreach (FamilyInstanceReferenceType referenceType in instanceReferenceTypeList)
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        foreach (Reference reference in familyInstance.GetReferences(referenceType).ToList<Reference>())
        {
          if (reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE)
          {
            Plane plane = SketchPlane.Create(revitDoc, reference).GetPlane();
            bool flag = rightDirection.IsAlmostEqualTo(plane.Normal, 1E-05) | rightDirection.IsAlmostEqualTo(plane.Normal.Negate(), 1E-05);
            int num2 = upDirection.IsAlmostEqualTo(plane.Normal, 1E-05) | upDirection.IsAlmostEqualTo(plane.Normal.Negate(), 1E-05) ? 1 : 0;
            XYZ origin = plane.Origin;
            XYZ localPoint = viewTransform.Inverse.OfPoint(origin);
            ReferencedPoint referencedPoint = new ReferencedPoint(reference, origin, localPoint, familyInstance.Id);
            if (flag)
              collection1.Add(referencedPoint);
            if (num2 != 0)
              collection2.Add(referencedPoint);
          }
        }
      }
      int num3 = (int) subTransaction.RollBack();
    }
    referncePlanesAndLines[DimensionEdge.Top].AddRange((IEnumerable<ReferencedPoint>) collection1);
    referncePlanesAndLines[DimensionEdge.Bottom].AddRange((IEnumerable<ReferencedPoint>) collection1);
    referncePlanesAndLines[DimensionEdge.Left].AddRange((IEnumerable<ReferencedPoint>) collection2);
    referncePlanesAndLines[DimensionEdge.Right].AddRange((IEnumerable<ReferencedPoint>) collection2);
    return referncePlanesAndLines;
  }

  private Reference DONOTUSEgetLineEndPointReference(
    Line line,
    Transform viewTransform,
    DimensionEdge dimEdge,
    out int index)
  {
    XYZ endPoint1 = line.GetEndPoint(0);
    XYZ endPoint2 = line.GetEndPoint(1);
    index = -1;
    XYZ source = viewTransform.Inverse.OfPoint(endPoint1);
    XYZ xyz1 = viewTransform.Inverse.OfPoint(endPoint2);
    XYZ xyz2 = (XYZ) null;
    switch (dimEdge)
    {
      case DimensionEdge.Left:
        xyz2 = source.X >= xyz1.X ? xyz1 : source;
        break;
      case DimensionEdge.Right:
        xyz2 = source.X <= xyz1.X ? xyz1 : source;
        break;
      case DimensionEdge.Top:
        xyz2 = source.Y >= xyz1.Y ? source : xyz1;
        break;
      case DimensionEdge.Bottom:
        xyz2 = source.Y <= xyz1.Y ? source : xyz1;
        break;
    }
    Reference endPointReference;
    if (xyz2.IsAlmostEqualTo(source))
    {
      endPointReference = line.GetEndPointReference(0);
      index = 0;
    }
    else
    {
      endPointReference = line.GetEndPointReference(1);
      index = 1;
    }
    return endPointReference;
  }

  private Reference getCurveEndPointReference(
    Edge curve,
    FamilyInstance inst,
    Transform viewTransform,
    DimensionEdge dimEdge,
    out int index)
  {
    XYZ endPoint1 = curve.AsCurve().GetEndPoint(0);
    XYZ endPoint2 = curve.AsCurve().GetEndPoint(1);
    index = -1;
    XYZ point1 = inst.GetTransform().OfPoint(endPoint1);
    XYZ point2 = inst.GetTransform().OfPoint(endPoint2);
    XYZ source = viewTransform.Inverse.OfPoint(point1);
    XYZ xyz1 = viewTransform.Inverse.OfPoint(point2);
    XYZ xyz2 = (XYZ) null;
    switch (dimEdge)
    {
      case DimensionEdge.Left:
        xyz2 = source.X >= xyz1.X ? xyz1 : source;
        break;
      case DimensionEdge.Right:
        xyz2 = source.X <= xyz1.X ? xyz1 : source;
        break;
      case DimensionEdge.Top:
        xyz2 = source.Y >= xyz1.Y ? source : xyz1;
        break;
      case DimensionEdge.Bottom:
        xyz2 = source.Y <= xyz1.Y ? source : xyz1;
        break;
    }
    Reference endPointReference;
    if (xyz2.IsAlmostEqualTo(source))
    {
      endPointReference = curve.GetEndPointReference(0);
      index = 0;
    }
    else
    {
      endPointReference = curve.GetEndPointReference(1);
      index = 1;
    }
    return endPointReference;
  }

  private XYZ getEndPoint(
    GeometryObject geoObject,
    FamilyInstance inst,
    Transform viewTransform,
    DimensionEdge dimEdge,
    List<XYZ> knownRefLocations,
    double samePointToelrance,
    Dictionary<GeometryObject, List<XYZ>> previouslyUsedDict,
    bool symbolBool,
    bool first,
    out bool runAgain)
  {
    runAgain = false;
    Curve curve = (Curve) null;
    switch (geoObject)
    {
      case Edge _:
        curve = (geoObject as Edge).AsCurve();
        break;
      case Curve _:
        curve = geoObject as Curve;
        break;
    }
    XYZ endPoint0 = curve.GetEndPoint(0);
    XYZ endPoint1 = curve.GetEndPoint(1);
    if (symbolBool)
    {
      endPoint0 = inst.GetTransform().OfPoint(endPoint0);
      endPoint1 = inst.GetTransform().OfPoint(endPoint1);
    }
    endPoint0 = viewTransform.Inverse.OfPoint(endPoint0);
    endPoint1 = viewTransform.Inverse.OfPoint(endPoint1);
    XYZ endPoint = (XYZ) null;
    bool flag1 = false;
    bool flag2 = false;
    if (previouslyUsedDict.ContainsKey(geoObject))
    {
      if (previouslyUsedDict[geoObject].Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(endPoint0))).Any<XYZ>())
        flag1 = true;
      if (previouslyUsedDict[geoObject].Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(endPoint1))).Any<XYZ>())
        flag2 = true;
    }
    if (flag1 & flag2 || !flag1 && !flag2)
    {
      bool flag3 = false;
      bool flag4 = false;
      if (dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom)
      {
        if (knownRefLocations.Where<XYZ>((Func<XYZ, bool>) (x => x.X.ApproximatelyEquals(endPoint0.X))).Any<XYZ>())
          flag3 = true;
        if (knownRefLocations.Where<XYZ>((Func<XYZ, bool>) (x => x.X.ApproximatelyEquals(endPoint1.X))).Any<XYZ>())
          flag4 = true;
        if (flag3 & flag4 & first && !endPoint0.X.ApproximatelyEquals(endPoint1.X))
        {
          runAgain = true;
          return (XYZ) null;
        }
      }
      else
      {
        if (knownRefLocations.Where<XYZ>((Func<XYZ, bool>) (x => x.Y.ApproximatelyEquals(endPoint0.Y))).Any<XYZ>())
          flag3 = true;
        if (knownRefLocations.Where<XYZ>((Func<XYZ, bool>) (x => x.Y.ApproximatelyEquals(endPoint1.Y))).Any<XYZ>())
          flag4 = true;
        if (flag3 & flag4 & first && !endPoint0.Y.ApproximatelyEquals(endPoint1.Y))
        {
          runAgain = true;
          return (XYZ) null;
        }
      }
      if (flag3 & flag4 || !flag3 && !flag4)
      {
        switch (dimEdge)
        {
          case DimensionEdge.Left:
            endPoint = endPoint0.X >= endPoint1.X ? endPoint1 : endPoint0;
            break;
          case DimensionEdge.Right:
            endPoint = endPoint0.X <= endPoint1.X ? endPoint1 : endPoint0;
            break;
          case DimensionEdge.Top:
            endPoint = endPoint0.Y >= endPoint1.Y ? endPoint0 : endPoint1;
            break;
          case DimensionEdge.Bottom:
            endPoint = endPoint0.Y <= endPoint1.Y ? endPoint0 : endPoint1;
            break;
        }
      }
      else
        endPoint = !flag3 ? endPoint1 : endPoint0;
    }
    else
      endPoint = !flag1 ? endPoint0 : endPoint1;
    return endPoint;
  }

  private XYZ getPlanerFacePoint(
    PlanarFace surface,
    FamilyInstance inst,
    Transform viewTransform,
    bool symbolBool)
  {
    XYZ point = surface.Origin;
    if (symbolBool)
      point = inst.GetTransform().OfPoint(point);
    return point;
  }

  private void getViewsOrientationAndMidPointData(
    StructuralFramingBoundsObject targetBoundsObject,
    StructuralFramingBoundsObject sourceBoundsObject,
    out double upDownMidpointTarget,
    out double leftRightMidpointTarget,
    out double upDownMidpointSource,
    out double leftRightMidpointSource)
  {
    upDownMidpointTarget = (targetBoundsObject.yMax + targetBoundsObject.yMin) / 2.0;
    upDownMidpointSource = (sourceBoundsObject.yMax + sourceBoundsObject.yMin) / 2.0;
    leftRightMidpointTarget = (targetBoundsObject.xMax + targetBoundsObject.xMin) / 2.0;
    leftRightMidpointSource = (sourceBoundsObject.xMax + sourceBoundsObject.xMin) / 2.0;
  }

  private void midPointSorting(
    List<Element> elems,
    Autodesk.Revit.DB.View view,
    Transform viewTransform,
    double upDownMidpoint,
    double leftRightMidpoint,
    Dictionary<ElementId, StructuralFramingBoundsObject> boundsObjectDict,
    out List<Element> top,
    out List<Element> bottom,
    out List<Element> right,
    out List<Element> left,
    double samePointTolernace,
    out List<List<ElementId>> alignmentGroupingsHorizontal,
    out List<List<ElementId>> alignmentGroupingsVertical,
    out Dictionary<ElementId, StructuralFramingBoundsObject> boundsObjectDictOut)
  {
    top = new List<Element>();
    bottom = new List<Element>();
    right = new List<Element>();
    left = new List<Element>();
    alignmentGroupingsHorizontal = new List<List<ElementId>>();
    alignmentGroupingsVertical = new List<List<ElementId>>();
    Dictionary<double, List<Element>> dictionary1 = new Dictionary<double, List<Element>>();
    Dictionary<double, List<Element>> dictionary2 = new Dictionary<double, List<Element>>();
    foreach (Element elem in elems)
    {
      StructuralFramingBoundsObject framingBoundsObject;
      if (boundsObjectDict.ContainsKey(elem.Id))
        framingBoundsObject = boundsObjectDict[elem.Id];
      else if (elem is FamilyInstance)
      {
        framingBoundsObject = new StructuralFramingBoundsObject(elem as FamilyInstance, view, true);
        boundsObjectDict.Add(elem.Id, framingBoundsObject);
      }
      else
        continue;
      XYZ xyz = new XYZ((framingBoundsObject.xMax + framingBoundsObject.xMin) / 2.0, (framingBoundsObject.yMax + framingBoundsObject.yMin) / 2.0, 0.0);
      double num1 = framingBoundsObject.yMax;
      double num2 = framingBoundsObject.yMin;
      double num3 = framingBoundsObject.xMax;
      double num4 = framingBoundsObject.xMin;
      if (num1 < num2)
      {
        double num5 = num1;
        num1 = num2;
        num2 = num5;
      }
      if (num1 < upDownMidpoint)
        bottom.Add(elem);
      else if (num2 > upDownMidpoint)
        top.Add(elem);
      else if (num1 >= upDownMidpoint && num2 <= upDownMidpoint)
      {
        bottom.Add(elem);
        top.Add(elem);
      }
      if (num3 < num4)
      {
        double num6 = num3;
        num3 = num4;
        num4 = num6;
      }
      if (num3 < leftRightMidpoint)
        left.Add(elem);
      else if (num4 > leftRightMidpoint)
        right.Add(elem);
      else if (num3 >= leftRightMidpoint && num4 <= leftRightMidpoint)
      {
        right.Add(elem);
        left.Add(elem);
      }
      double key1 = 0.0;
      bool flag1 = false;
      double num7 = 1.0;
      foreach (double key2 in dictionary1.Keys)
      {
        if (xyz.Y.ApproximatelyEquals(key2, 1.0 / 96.0))
        {
          double num8 = Math.Abs(xyz.Y - key2);
          if (num8 < num7)
          {
            flag1 = true;
            key1 = key2;
            num7 = num8;
          }
        }
      }
      if (flag1)
        dictionary1[key1].Add(elem);
      else
        dictionary1.Add(xyz.Y, new List<Element>() { elem });
      double key3 = 0.0;
      bool flag2 = false;
      double num9 = 1.0;
      foreach (double key4 in dictionary2.Keys)
      {
        if (xyz.X.ApproximatelyEquals(key4, 1.0 / 96.0))
        {
          double num10 = Math.Abs(xyz.X - key4);
          if (num10 < num9)
          {
            flag2 = true;
            key3 = key4;
            num9 = num10;
          }
        }
      }
      if (flag2)
        dictionary2[key3].Add(elem);
      else
        dictionary2.Add(xyz.X, new List<Element>() { elem });
    }
    foreach (double key in dictionary1.Keys)
    {
      alignmentGroupingsHorizontal.Add(dictionary1[key].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>());
      if (left.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(dictionary1[key].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id))).Any<ElementId>())
      {
        left.AddRange((IEnumerable<Element>) dictionary1[key]);
        left = left.Distinct<Element>().ToList<Element>();
      }
      if (right.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(dictionary1[key].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id))).Any<ElementId>())
      {
        right.AddRange((IEnumerable<Element>) dictionary1[key]);
        right = right.Distinct<Element>().ToList<Element>();
      }
    }
    foreach (double key in dictionary2.Keys)
    {
      alignmentGroupingsVertical.Add(dictionary2[key].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>());
      if (top.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(dictionary2[key].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id))).Any<ElementId>())
      {
        top.AddRange((IEnumerable<Element>) dictionary2[key]);
        top = top.Distinct<Element>().ToList<Element>();
      }
      if (bottom.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).Intersect<ElementId>(dictionary2[key].Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id))).Any<ElementId>())
      {
        bottom.AddRange((IEnumerable<Element>) dictionary2[key]);
        bottom = bottom.Distinct<Element>().ToList<Element>();
      }
    }
    boundsObjectDictOut = boundsObjectDict;
  }

  private List<ReferencedPoint> sortReferencedPointList(
    List<ReferencedPoint> refsList,
    DimensionEdge dimEdge)
  {
    if (refsList.Count < 2)
      return refsList;
    switch (dimEdge)
    {
      case DimensionEdge.Left:
      case DimensionEdge.Right:
        refsList = refsList.OrderBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (x => x.LocalPoint.Y)).ToList<ReferencedPoint>();
        break;
      case DimensionEdge.Top:
      case DimensionEdge.Bottom:
        refsList = refsList.OrderBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (x => x.LocalPoint.X)).ToList<ReferencedPoint>();
        break;
    }
    return refsList;
  }

  private Dictionary<ElementId, ElementReferenceInfo> sortSourceElementReferenceInfoDictionary(
    Dictionary<ElementId, ElementReferenceInfo> dictionary,
    DimensionEdge dimEdge)
  {
    Dictionary<ElementId, ElementReferenceInfo> returnDictionary = new Dictionary<ElementId, ElementReferenceInfo>();
    List<ElementReferenceInfo> list = dictionary.Values.ToList<ElementReferenceInfo>();
    list.ForEach((Action<ElementReferenceInfo>) (x => x.sourceReferences = this.sortReferencedPointList(x.sourceReferences, dimEdge)));
    (dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom ? list.OrderBy<ElementReferenceInfo, double>((Func<ElementReferenceInfo, double>) (x => x.sourceReferences.FirstOrDefault<ReferencedPoint>().LocalPoint.X)).ToList<ElementReferenceInfo>() : list.OrderBy<ElementReferenceInfo, double>((Func<ElementReferenceInfo, double>) (x => x.sourceReferences.FirstOrDefault<ReferencedPoint>().LocalPoint.X)).ToList<ElementReferenceInfo>()).ForEach((Action<ElementReferenceInfo>) (x => returnDictionary.Add(x.elemId, x)));
    return returnDictionary;
  }

  private Dictionary<ElementId, ElementReferenceInfo> sortTargetElementReferenceInfoDictionary(
    Dictionary<ElementId, ElementReferenceInfo> dictionary,
    DimensionEdge dimEdge)
  {
    Dictionary<ElementId, ElementReferenceInfo> returnDictionary = new Dictionary<ElementId, ElementReferenceInfo>();
    List<ElementReferenceInfo> list = dictionary.Values.ToList<ElementReferenceInfo>();
    (dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom ? list.OrderBy<ElementReferenceInfo, double>((Func<ElementReferenceInfo, double>) (x => x.lowerBound.LocalPoint.X)).ToList<ElementReferenceInfo>() : list.OrderBy<ElementReferenceInfo, double>((Func<ElementReferenceInfo, double>) (x => x.lowerBound.LocalPoint.X)).ToList<ElementReferenceInfo>()).ForEach((Action<ElementReferenceInfo>) (x => returnDictionary.Add(x.elemId, x)));
    return returnDictionary;
  }

  private List<ReferencedPoint> removeDoubleReferencedPoints(
    List<ReferencedPoint> refsList,
    DimensionEdge dimEdge)
  {
    List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
    IEnumerable<IGrouping<double, ReferencedPoint>> groupings1 = (IEnumerable<IGrouping<double, ReferencedPoint>>) null;
    switch (dimEdge)
    {
      case DimensionEdge.Left:
      case DimensionEdge.Right:
        groupings1 = refsList.GroupBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (x => Math.Round(x.LocalPoint.Y, 7)));
        break;
      case DimensionEdge.Top:
      case DimensionEdge.Bottom:
        groupings1 = refsList.GroupBy<ReferencedPoint, double>((Func<ReferencedPoint, double>) (x => Math.Round(x.LocalPoint.X, 7)));
        break;
    }
    foreach (IEnumerable<ReferencedPoint> source in groupings1)
    {
      IEnumerable<IGrouping<ElementReferenceType, ReferencedPoint>> groupings2 = source.GroupBy<ReferencedPoint, ElementReferenceType>((Func<ReferencedPoint, ElementReferenceType>) (x => x.Reference.ElementReferenceType));
      List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
      List<ReferencedPoint> collection1 = new List<ReferencedPoint>();
      foreach (IGrouping<ElementReferenceType, ReferencedPoint> collection2 in groupings2)
      {
        ElementReferenceType key = collection2.Key;
        if (key.Equals((object) ElementReferenceType.REFERENCE_TYPE_LINEAR))
        {
          referencedPointList2.AddRange((IEnumerable<ReferencedPoint>) collection2);
        }
        else
        {
          key = collection2.Key;
          if (key.Equals((object) ElementReferenceType.REFERENCE_TYPE_SURFACE))
            collection1.AddRange((IEnumerable<ReferencedPoint>) collection2);
        }
      }
      List<ReferencedPoint> referencedPointList3 = new List<ReferencedPoint>();
      if (referencedPointList2.Count<ReferencedPoint>() == 0)
        referencedPointList3.AddRange((IEnumerable<ReferencedPoint>) collection1);
      else
        referencedPointList3.AddRange((IEnumerable<ReferencedPoint>) referencedPointList2);
      ReferencedPoint referencedPoint1 = (ReferencedPoint) null;
      foreach (ReferencedPoint referencedPoint2 in referencedPointList3)
      {
        if (referencedPoint1 == null)
          referencedPoint1 = referencedPoint2;
        if (Math.Abs(referencedPoint2.LocalPoint.Z) > referencedPoint1.LocalPoint.Z)
          referencedPoint1 = referencedPoint2;
      }
      if (referencedPoint1 != null)
        referencedPointList1.Add(referencedPoint1);
    }
    return referencedPointList1;
  }

  private bool DONOTUSElikeElementCheck(ElementId id1, ElementId id2, Document revitDoc)
  {
    if (id1 == (ElementId) null || id1 == ElementId.InvalidElementId || id2 == (ElementId) null || id2 == ElementId.InvalidElementId)
      return false;
    Element element1 = revitDoc.GetElement(id1);
    Element element2 = revitDoc.GetElement(id2);
    return element1 != null && element2 != null && this.likeElementCheck(element1, element2, revitDoc);
  }

  private bool likeElementCheck(Element elem1, Element elem2, Document revitDoc)
  {
    Category category = Category.GetCategory(revitDoc, BuiltInCategory.OST_StructuralFraming);
    return elem1.Category.Id.Equals((object) category.Id) && elem2.Category.Id.Equals((object) category.Id) || this.getElementSortingName(elem1) == this.getElementSortingName(elem2);
  }

  private string getElementSortingName(ElementId elemId, Document revitDoc)
  {
    return elemId == (ElementId) null || elemId == ElementId.InvalidElementId ? string.Empty : this.getElementSortingName(revitDoc.GetElement(elemId));
  }

  private string getElementSortingName(Element elem)
  {
    if (elem.Category.Name == "Structural Framing")
      return "STRUCT FRAMING";
    if (elem.Category.Name == "Assemblies")
      return "ASSEMBLIES";
    Parameter parameter = Utils.ElementUtils.Parameters.LookupParameter(elem, "CONTROL_MARK");
    string empty = string.Empty;
    string elementSortingName;
    if (parameter != null)
    {
      elementSortingName = parameter.AsString();
      if (elementSortingName == null || string.IsNullOrWhiteSpace(elementSortingName))
        elementSortingName = (elem as FamilyInstance).Symbol.FamilyName;
    }
    else
      elementSortingName = (elem as FamilyInstance).Symbol.FamilyName;
    return elementSortingName;
  }

  private SortingInfoCloneTicketElement getElementSortingObject(
    Element element,
    AutoTicketLIF lifSettings,
    LocationInFormAnalyzer locationInFormAnalyzer)
  {
    string elementSortingName = this.getElementSortingName(element);
    FormLocation formLocation;
    if (!Utils.ElementUtils.Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").ToUpper().Contains("EMBED"))
    {
      formLocation = FormLocation.None;
    }
    else
    {
      string str = LocationInFormAnalyzer.ProcessAndAssignLIF(element.Document, lifSettings, element.Id, locationInFormAnalyzer);
      formLocation = !(str == lifSettings.TIF) ? (!(str == lifSettings.BIF) ? (!(str == lifSettings.SIF) ? FormLocation.None : FormLocation.SIF) : FormLocation.BIF) : FormLocation.TIF;
    }
    int location = (int) formLocation;
    return new SortingInfoCloneTicketElement(elementSortingName, (FormLocation) location);
  }

  private SortingInfoCloneTicketElement getElementSortingObject(
    ElementId elemId,
    Document revitDoc,
    AutoTicketLIF lifSettings,
    LocationInFormAnalyzer locationInFormAnalyzer)
  {
    return elemId == (ElementId) null || elemId == ElementId.InvalidElementId ? new SortingInfoCloneTicketElement(string.Empty, FormLocation.None) : this.getElementSortingObject(revitDoc.GetElement(elemId), lifSettings, locationInFormAnalyzer);
  }

  private List<ReferencedPoint> checkReferencedPointsForLocalPoint(
    List<ReferencedPoint> refPoints,
    Transform transform)
  {
    foreach (ReferencedPoint refPoint in refPoints)
    {
      if (refPoint.LocalPoint == null)
        refPoint.LocalPoint = transform.Inverse.OfPoint(refPoint.Point);
    }
    return refPoints;
  }

  private List<List<Dimension>> groupDimensions(
    List<Dimension> list,
    Transform transform,
    DimensionEdge dimEdge)
  {
    List<List<Dimension>> dimensionListList = new List<List<Dimension>>();
    Dictionary<XYZ, List<Dimension>> source = new Dictionary<XYZ, List<Dimension>>();
    foreach (Dimension dimension in list)
    {
      XYZ point = dimension.NumberOfSegments != 0 ? dimension.Segments.get_Item(0).Origin : dimension.Origin;
      if (point != null)
      {
        XYZ key1 = transform.Inverse.OfPoint(point);
        if (source.Count<KeyValuePair<XYZ, List<Dimension>>>() == 0)
        {
          source.Add(key1, new List<Dimension>()
          {
            dimension
          });
        }
        else
        {
          bool flag = false;
          foreach (XYZ key2 in source.Keys)
          {
            switch (dimEdge)
            {
              case DimensionEdge.Left:
              case DimensionEdge.Right:
                if (key2.Z.ApproximatelyEquals(key1.Z) && key2.Z.ApproximatelyEquals(key1.Z))
                {
                  source[key2].Add(dimension);
                  flag = true;
                  goto label_14;
                }
                continue;
              case DimensionEdge.Top:
              case DimensionEdge.Bottom:
                if (key2.Y.ApproximatelyEquals(key1.Y) && key2.Z.ApproximatelyEquals(key1.Z))
                {
                  source[key2].Add(dimension);
                  flag = true;
                  goto label_14;
                }
                continue;
              default:
                continue;
            }
          }
label_14:
          if (!flag)
            source.Add(key1, new List<Dimension>()
            {
              dimension
            });
        }
      }
    }
    foreach (XYZ key in source.Keys)
      dimensionListList.Add(source[key]);
    return dimensionListList;
  }

  private bool getTwoNearestExtremes(
    List<ReferencedPoint> list1,
    List<ReferencedPoint> list2,
    DimensionEdge dimEdge,
    out XYZ list1Extreme,
    out XYZ list2Extreme)
  {
    list1Extreme = new XYZ(0.0, 0.0, 0.0);
    list2Extreme = new XYZ(0.0, 0.0, 0.0);
    if (list1 == null || list1.Count == 0 || list2 == null || list2.Count == 0)
      return false;
    list1 = this.sortReferencedPointList(list1, dimEdge);
    XYZ localPoint1 = list1[0].LocalPoint;
    list1.Reverse();
    XYZ localPoint2 = list1[0].LocalPoint;
    list2 = this.sortReferencedPointList(list2, dimEdge);
    XYZ localPoint3 = list2[0].LocalPoint;
    list2.Reverse();
    XYZ localPoint4 = list2[0].LocalPoint;
    switch (dimEdge)
    {
      case DimensionEdge.Left:
      case DimensionEdge.Right:
        if (localPoint2.Y < localPoint3.Y)
        {
          list1Extreme = localPoint2;
          list2Extreme = localPoint3;
          break;
        }
        if (localPoint4.Y >= localPoint1.Y)
          return false;
        list2Extreme = localPoint4;
        list1Extreme = localPoint1;
        break;
      case DimensionEdge.Top:
      case DimensionEdge.Bottom:
        if (localPoint2.X < localPoint3.X)
        {
          list1Extreme = localPoint2;
          list2Extreme = localPoint3;
          break;
        }
        if (localPoint4.X >= localPoint1.X)
          return false;
        list2Extreme = localPoint4;
        list1Extreme = localPoint1;
        break;
    }
    return true;
  }

  private XYZ getCorrectDimMovementVector(
    Document revitDoc,
    Dimension newDimension,
    Dimension sourceDimension,
    AssemblyBoundingBoxInfo assemblyBBInfo,
    DimensionEdge dimEdge)
  {
    XYZ zero1 = XYZ.Zero;
    XYZ xyz1;
    if (newDimension.Segments.IsEmpty)
    {
      Line curve = newDimension.Curve as Line;
      xyz1 = assemblyBBInfo.targetViewTransform.Inverse.OfPoint(curve.Origin);
    }
    else
    {
      DimensionSegment dimensionSegment = newDimension.Segments.get_Item(0);
      xyz1 = assemblyBBInfo.targetViewTransform.Inverse.OfPoint(dimensionSegment.Origin);
    }
    XYZ zero2 = XYZ.Zero;
    XYZ xyz2;
    if (sourceDimension.Segments.IsEmpty)
    {
      Line curve = sourceDimension.Curve as Line;
      xyz2 = assemblyBBInfo.sourceViewTransform.Inverse.OfPoint(curve.Origin);
    }
    else
    {
      DimensionSegment dimensionSegment = sourceDimension.Segments.get_Item(0);
      xyz2 = assemblyBBInfo.sourceViewTransform.Inverse.OfPoint(dimensionSegment.Origin);
    }
    XYZ vec = new XYZ(0.0, 0.0, 0.0);
    double num1 = 0.0;
    switch (dimEdge)
    {
      case DimensionEdge.Left:
        if (xyz2.X < assemblyBBInfo.sourceXMax && xyz2.X > assemblyBBInfo.sourceXMin)
        {
          double num2 = double.NaN;
          foreach (Reference reference in sourceDimension.References)
          {
            Element element = revitDoc.GetElement(reference);
            if (element.Location is LocationPoint)
            {
              XYZ xyz3 = assemblyBBInfo.sourceViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
              if (num2.Equals(double.NaN) || Math.Abs(num2) < Math.Abs(xyz3.X - xyz2.X))
                num2 = xyz3.X - xyz2.X;
            }
          }
          if (num2 != double.NaN)
          {
            foreach (Reference reference in newDimension.References)
            {
              Element element = revitDoc.GetElement(reference);
              if (element.Location is LocationPoint)
              {
                XYZ xyz4 = assemblyBBInfo.targetViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
                double num3 = xyz1.X - (xyz4.X - num2);
                if (Math.Abs(num3) > Math.Abs(num1))
                  num1 = num3;
              }
            }
            vec = new XYZ(-num1, 0.0, 0.0);
            break;
          }
          break;
        }
        vec = new XYZ(xyz2.X - assemblyBBInfo.sourceXMin - (xyz1.X - assemblyBBInfo.targetXMin), 0.0, 0.0);
        break;
      case DimensionEdge.Right:
        if (xyz2.X < assemblyBBInfo.sourceXMax && xyz2.X > assemblyBBInfo.sourceXMin)
        {
          double num4 = double.NaN;
          foreach (Reference reference in sourceDimension.References)
          {
            Element element = revitDoc.GetElement(reference);
            if (element.Location is LocationPoint)
            {
              XYZ xyz5 = assemblyBBInfo.sourceViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
              if (num4.Equals(double.NaN) || Math.Abs(num4) < Math.Abs(xyz5.X - xyz2.X))
                num4 = xyz5.X - xyz2.X;
            }
          }
          if (num4 != double.NaN)
          {
            foreach (Reference reference in newDimension.References)
            {
              Element element = revitDoc.GetElement(reference);
              if (element.Location is LocationPoint)
              {
                XYZ xyz6 = assemblyBBInfo.targetViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
                double num5 = xyz1.X - (xyz6.X - num4);
                if (Math.Abs(num5) > Math.Abs(num1))
                  num1 = num5;
              }
            }
            vec = new XYZ(-num1, 0.0, 0.0);
            break;
          }
          break;
        }
        vec = new XYZ(xyz2.X - assemblyBBInfo.sourceXMax - (xyz1.X - assemblyBBInfo.targetXMax), 0.0, 0.0);
        break;
      case DimensionEdge.Top:
        if (xyz2.Y < assemblyBBInfo.sourceYMax && xyz2.Y > assemblyBBInfo.sourceYMin)
        {
          double num6 = double.NaN;
          foreach (Reference reference in sourceDimension.References)
          {
            Element element = revitDoc.GetElement(reference);
            if (element.Location is LocationPoint)
            {
              XYZ xyz7 = assemblyBBInfo.sourceViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
              if (num6.Equals(double.NaN) || Math.Abs(num6) < Math.Abs(xyz7.Y - xyz2.Y))
                num6 = xyz7.Y - xyz2.Y;
            }
          }
          if (num6 != double.NaN)
          {
            foreach (Reference reference in newDimension.References)
            {
              Element element = revitDoc.GetElement(reference);
              if (element.Location is LocationPoint)
              {
                XYZ xyz8 = assemblyBBInfo.targetViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
                double num7 = xyz1.Y - (xyz8.Y - num6);
                if (Math.Abs(num7) > Math.Abs(num1))
                  num1 = num7;
              }
            }
            vec = new XYZ(0.0, -num1, 0.0);
            break;
          }
          break;
        }
        vec = new XYZ(0.0, xyz2.Y - assemblyBBInfo.sourceYMax - (xyz1.Y - assemblyBBInfo.targetYMax), 0.0);
        break;
      case DimensionEdge.Bottom:
        if (xyz2.Y < assemblyBBInfo.sourceYMax && xyz2.Y > assemblyBBInfo.sourceYMin)
        {
          double num8 = double.NaN;
          foreach (Reference reference in sourceDimension.References)
          {
            Element element = revitDoc.GetElement(reference);
            if (element.Location is LocationPoint)
            {
              XYZ xyz9 = assemblyBBInfo.sourceViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
              if (num8.Equals(double.NaN) || Math.Abs(num8) < Math.Abs(xyz9.Y - xyz2.Y))
                num8 = xyz9.Y - xyz2.Y;
            }
          }
          if (num8 != double.NaN)
          {
            foreach (Reference reference in newDimension.References)
            {
              Element element = revitDoc.GetElement(reference);
              if (element.Location is LocationPoint)
              {
                XYZ xyz10 = assemblyBBInfo.targetViewTransform.Inverse.OfPoint((element.Location as LocationPoint).Point);
                double num9 = xyz1.Y - (xyz10.Y - num8);
                if (Math.Abs(num9) > Math.Abs(num1))
                  num1 = num9;
              }
            }
            vec = new XYZ(0.0, -num1, 0.0);
            break;
          }
          break;
        }
        double num10 = assemblyBBInfo.sourceYMin - xyz2.Y;
        vec = new XYZ(0.0, assemblyBBInfo.targetYMin - xyz1.Y - num10, 0.0);
        break;
    }
    return assemblyBBInfo.targetViewTransform.OfVector(vec);
  }

  private List<ElementId> CheckBounds(
    ElementId possibleMatchId,
    ElementId sourceId,
    List<ElementId> elemIds,
    List<ElementId> potientialTargets,
    Dictionary<ElementId, ElementId> allBetween,
    Dictionary<ElementId, ElementId> sourceToTarget)
  {
    List<ElementId> elementIdList = elemIds;
    int num1 = 1;
    foreach (ElementId key in allBetween.Keys)
    {
      if (allBetween[key] == sourceId && sourceToTarget.ContainsKey(key))
      {
        ElementId elementId1 = sourceToTarget[key];
        ElementId elementId2 = possibleMatchId;
        int num2 = potientialTargets.IndexOf(elementId1);
        int num3 = potientialTargets.IndexOf(elementId2);
        if (num2 != -1 && num3 != -1)
        {
          int num4 = num2;
          int num5 = num4 + 1;
          for (int index = num4; index < num3; ++index)
          {
            ElementId potientialTarget = potientialTargets[index];
            if (!elementIdList.Contains(potientialTarget))
              elementIdList.Add(potientialTarget);
          }
        }
      }
      ++num1;
    }
    return elementIdList;
  }

  private bool compareElementLocation(
    ElementReferenceInfo source,
    ElementReferenceInfo target,
    StructuralFramingBoundsObject sourceMinObject,
    StructuralFramingBoundsObject targetMinObject,
    Transform sourceViewTransform,
    Transform targetViewTransform,
    double tolerance,
    DimensionEdge dimEdge,
    out double distance)
  {
    distance = -1.0;
    if (!source.Source)
      return false;
    ReferencedPoint referencedPoint1 = target.referencedPoints.FirstOrDefault<ReferencedPoint>();
    ReferencedPoint referencedPoint2 = target.referencedPoints.LastOrDefault<ReferencedPoint>();
    bool flag = false;
    XYZ xyz1 = new XYZ(sourceMinObject.xMin, sourceMinObject.yMin, 0.0);
    XYZ xyz2 = new XYZ(targetMinObject.xMin, targetMinObject.yMin, 0.0);
    foreach (ReferencedPoint sourceReference in source.sourceReferences)
    {
      double me;
      double other1;
      double other2;
      if (dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom)
      {
        me = sourceReference.LocalPoint.X - xyz1.X;
        other1 = referencedPoint2.LocalPoint.X - xyz2.X;
        other2 = referencedPoint1.LocalPoint.X - xyz2.X;
      }
      else
      {
        me = sourceReference.LocalPoint.Y - xyz1.Y;
        other1 = referencedPoint2.LocalPoint.Y - xyz2.Y;
        other2 = referencedPoint1.LocalPoint.Y - xyz2.Y;
      }
      if (me > other2 && me < other1)
      {
        flag = true;
        break;
      }
      if (me.ApproximatelyEquals(other2, tolerance))
      {
        flag = true;
        break;
      }
      if (me.ApproximatelyEquals(other1, tolerance))
      {
        flag = true;
        break;
      }
    }
    if (!flag)
      return flag;
    foreach (ReferencedPoint sourceReference in source.sourceReferences)
    {
      foreach (ReferencedPoint referencedPoint3 in target.referencedPoints)
      {
        double num = dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom ? Math.Abs(sourceReference.LocalPoint.X - referencedPoint3.LocalPoint.X) : Math.Abs(sourceReference.LocalPoint.Y - referencedPoint3.LocalPoint.Y);
        if (distance > num || distance == -1.0)
          distance = num;
      }
    }
    return flag;
  }

  private ElementId getAlignedIdClosestToEdge(
    ElementId inputId,
    List<List<ElementId>> alignmentLists,
    DimensionEdge edge,
    Dictionary<ElementId, ElementReferenceInfo> elementReferenceInfoDict)
  {
    List<ElementId> source = new List<ElementId>();
    foreach (List<ElementId> alignmentList in alignmentLists)
    {
      if (alignmentList.Contains(inputId))
      {
        source = alignmentList;
        break;
      }
    }
    if (source.Count<ElementId>() == 0)
      return inputId;
    double num = this.getClosestReferenceToEdgeValue(elementReferenceInfoDict[inputId], edge);
    ElementId alignedIdClosestToEdge = inputId;
    foreach (ElementId key in source)
    {
      double referenceToEdgeValue = this.getClosestReferenceToEdgeValue(elementReferenceInfoDict[key], edge);
      if (edge == DimensionEdge.Top || edge == DimensionEdge.Right)
      {
        if (referenceToEdgeValue > num)
        {
          num = referenceToEdgeValue;
          alignedIdClosestToEdge = key;
        }
      }
      else if (referenceToEdgeValue < num)
      {
        num = referenceToEdgeValue;
        alignedIdClosestToEdge = key;
      }
    }
    return alignedIdClosestToEdge;
  }

  private double getClosestReferenceToEdgeValue(ElementReferenceInfo info, DimensionEdge edge)
  {
    switch (edge)
    {
      case DimensionEdge.Left:
        return info.lowerBound.LocalPoint.X;
      case DimensionEdge.Right:
        return info.upperBound.LocalPoint.X;
      case DimensionEdge.Top:
        return info.upperBound.LocalPoint.Y;
      case DimensionEdge.Bottom:
        return info.lowerBound.LocalPoint.Y;
      default:
        return 0.0;
    }
  }

  private double getDistanceBetweenElements(
    ElementReferenceInfo source,
    ElementReferenceInfo target,
    StructuralFramingBoundsObject sourceMinObject,
    StructuralFramingBoundsObject targetMinObject,
    DimensionEdge dimEdge)
  {
    double distanceBetweenElements = -1.0;
    XYZ xyz1 = new XYZ(sourceMinObject.xMin, sourceMinObject.yMin, 0.0);
    XYZ xyz2 = new XYZ(targetMinObject.xMin, targetMinObject.yMin, 0.0);
    foreach (ReferencedPoint sourceReference in source.sourceReferences)
    {
      foreach (ReferencedPoint referencedPoint in target.referencedPoints)
      {
        double num = dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom ? Math.Abs(sourceReference.LocalPoint.X - xyz1.X - (referencedPoint.LocalPoint.X - xyz2.X)) : Math.Abs(sourceReference.LocalPoint.Y - xyz1.Y - (referencedPoint.LocalPoint.Y - xyz2.Y));
        if (distanceBetweenElements > num || distanceBetweenElements == -1.0)
          distanceBetweenElements = num;
      }
    }
    return distanceBetweenElements;
  }

  private List<ReferencedPoint> GetTargetReferencesFromElements(
    Document revitDoc,
    List<ElementId> newElementidsToTarget,
    Dictionary<ElementId, ElementId> sourceToTarget,
    Dictionary<ElementId, ElementReferenceInfo> sourceElemRefInfo,
    Dictionary<ElementId, ElementReferenceInfo> targetElemRefInfo,
    Dictionary<ElementId, StructuralFramingBoundsObject> sourceBoundsDictionary,
    Dictionary<ElementId, StructuralFramingBoundsObject> targetBoundsDictionary,
    Autodesk.Revit.DB.View sourceView,
    Autodesk.Revit.DB.View targetView,
    Transform targetViewTransform,
    double samePointTolereance,
    SortingInfoCloneTicketElement majority,
    bool lowerExtent,
    bool upperExtent,
    DimensionEdge dimEdge)
  {
    Dictionary<List<double>, int> source1 = new Dictionary<List<double>, int>();
    List<ReferencedPoint> refPoints = new List<ReferencedPoint>();
    foreach (ElementId key1 in sourceToTarget.Keys)
    {
      if (sourceElemRefInfo.ContainsKey(key1) && targetElemRefInfo.ContainsKey(sourceToTarget[key1]))
      {
        ElementReferenceInfo elementReferenceInfo1 = sourceElemRefInfo[key1];
        ElementReferenceInfo elementReferenceInfo2 = targetElemRefInfo[sourceToTarget[key1]];
        if (!(elementReferenceInfo1.type != elementReferenceInfo2.type))
        {
          XYZ xyz1 = new XYZ(0.0, 0.0, 0.0);
          XYZ xyz2 = new XYZ(0.0, 0.0, 0.0);
          StructuralFramingBoundsObject framingBoundsObject1 = !sourceBoundsDictionary.ContainsKey(key1) ? new StructuralFramingBoundsObject(revitDoc.GetElement(key1) as FamilyInstance, sourceView, true) : sourceBoundsDictionary[key1];
          StructuralFramingBoundsObject framingBoundsObject2;
          if (targetBoundsDictionary.ContainsKey(sourceToTarget[key1]))
          {
            framingBoundsObject2 = targetBoundsDictionary[sourceToTarget[key1]];
          }
          else
          {
            framingBoundsObject2 = new StructuralFramingBoundsObject(revitDoc.GetElement(sourceToTarget[key1]) as FamilyInstance, targetView, true);
            targetBoundsDictionary.Add(key1, framingBoundsObject2);
          }
          XYZ xyz3 = new XYZ(framingBoundsObject1.xMin, framingBoundsObject1.yMin, 0.0);
          XYZ xyz4 = new XYZ(framingBoundsObject2.xMin, framingBoundsObject2.yMin, 0.0);
          List<ReferencedPoint> refsList1 = this.sortReferencedPointList(elementReferenceInfo1.sourceReferences, dimEdge);
          List<ReferencedPoint> refsList2 = this.sortReferencedPointList(elementReferenceInfo2.referencedPoints, dimEdge);
          List<ReferencedPoint> referencedPointList = this.removeDoubleReferencedPoints(refsList1, dimEdge);
          List<ReferencedPoint> source2 = this.removeDoubleReferencedPoints(refsList2, dimEdge);
          if (elementReferenceInfo2.type.sortingName == "STRUCT FRAMING")
          {
            if (lowerExtent && source2.Count<ReferencedPoint>() > 0)
              source2.Remove(source2.First<ReferencedPoint>());
            if (upperExtent && source2.Count<ReferencedPoint>() > 0)
              source2.Remove(source2.Last<ReferencedPoint>());
          }
          List<double> key2 = new List<double>();
          foreach (ReferencedPoint referencedPoint1 in referencedPointList)
          {
            double num1 = dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom ? referencedPoint1.LocalPoint.X - xyz3.X : referencedPoint1.LocalPoint.Y - xyz3.Y;
            key2.Add(num1);
            ReferencedPoint referencedPoint2 = (ReferencedPoint) null;
            double num2 = -1.0;
            foreach (ReferencedPoint referencedPoint3 in source2)
            {
              ElementReferenceType elementReferenceType = referencedPoint3.Reference.ElementReferenceType;
              if (elementReferenceType.Equals((object) ElementReferenceType.REFERENCE_TYPE_SURFACE))
              {
                elementReferenceType = referencedPoint1.Reference.ElementReferenceType;
                if (!elementReferenceType.Equals((object) ElementReferenceType.REFERENCE_TYPE_SURFACE))
                  continue;
              }
              double num3 = Math.Abs((dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom ? referencedPoint3.LocalPoint.X - xyz4.X : referencedPoint3.LocalPoint.Y - xyz4.Y) - num1);
              if ((!(elementReferenceInfo1.type.sortingName == "STRUCT FRAMING") || num3 <= samePointTolereance) && (num3 < num2 || num2 == -1.0))
              {
                num2 = num3;
                referencedPoint2 = referencedPoint3;
              }
            }
            if (referencedPoint2 != null)
            {
              refPoints.Add(referencedPoint2);
              source2.Remove(referencedPoint2);
            }
            if (elementReferenceInfo1.type == majority)
            {
              key2.Sort();
              List<double> key3 = (List<double>) null;
              foreach (List<double> key4 in source1.Keys)
              {
                if (key4.Count == key2.Count)
                {
                  bool flag = true;
                  for (int index = 0; index < key2.Count; ++index)
                  {
                    flag = key4[index].ApproximatelyEquals(key2[index], samePointTolereance);
                    if (!flag)
                      break;
                  }
                  if (flag)
                  {
                    key3 = key4;
                    break;
                  }
                }
              }
              if (key3 != null)
                source1[key3]++;
              else
                source1.Add(key2, 1);
            }
          }
        }
      }
    }
    bool flag1 = false;
    List<double> doubleList = new List<double>();
    if (source1.Count<KeyValuePair<List<double>, int>>() > 0)
    {
      doubleList = source1.Keys.FirstOrDefault<List<double>>();
      foreach (List<double> key in source1.Keys)
      {
        if (source1[key] > source1[key])
          doubleList = key;
      }
    }
    else
      flag1 = true;
    newElementidsToTarget.RemoveAll(new Predicate<ElementId>(((Enumerable) sourceToTarget.Values).Contains<ElementId>));
    foreach (ElementId elementId in newElementidsToTarget)
    {
      if (targetElemRefInfo.ContainsKey(elementId))
      {
        ElementReferenceInfo elementReferenceInfo = targetElemRefInfo[elementId];
        List<ReferencedPoint> referencedPointList = this.sortReferencedPointList(elementReferenceInfo.referencedPoints, dimEdge);
        Element element = revitDoc.GetElement(elementId);
        StructuralFramingBoundsObject framingBoundsObject;
        if (targetBoundsDictionary.ContainsKey(elementId))
        {
          framingBoundsObject = targetBoundsDictionary[elementId];
        }
        else
        {
          framingBoundsObject = new StructuralFramingBoundsObject(element as FamilyInstance, targetView, true);
          targetBoundsDictionary.Add(elementId, framingBoundsObject);
        }
        XYZ xyz = new XYZ(framingBoundsObject.xMin, framingBoundsObject.yMin, 0.0);
        if (!(elementReferenceInfo.type.sortingName == "STRUCT FRAMING"))
        {
          if (elementReferenceInfo.type != majority && !flag1)
          {
            foreach (double num4 in doubleList)
            {
              ReferencedPoint referencedPoint4 = (ReferencedPoint) null;
              double num5 = -1.0;
              foreach (ReferencedPoint referencedPoint5 in referencedPointList)
              {
                double num6 = Math.Abs((dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom ? referencedPoint5.LocalPoint.X - xyz.X : referencedPoint5.LocalPoint.Y - xyz.Y) - num4);
                if (num6 < num5 || num5 == -1.0)
                {
                  num5 = num6;
                  referencedPoint4 = referencedPoint5;
                }
              }
              if (referencedPoint4 != null)
              {
                refPoints.Add(referencedPoint4);
                referencedPointList.Remove(referencedPoint4);
              }
            }
          }
          else if (element is FamilyInstance)
          {
            List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculator.GetDimLineReference(element as FamilyInstance, dimEdge, targetView);
            foreach (ReferencedPoint referencedPoint in dimLineReference)
              referencedPoint.elementId = referencedPoint.Reference.ElementId;
            refPoints.AddRange((IEnumerable<ReferencedPoint>) dimLineReference);
          }
        }
      }
    }
    return this.sortReferencedPointList(this.checkReferencedPointsForLocalPoint(refPoints, targetViewTransform), dimEdge);
  }

  private bool DONOTUSEcompareReferenceLocation(
    XYZ point1,
    XYZ point2,
    double tolerance,
    DimensionEdge dimEdge)
  {
    double me;
    double other;
    if (dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom)
    {
      me = point1.X;
      other = point2.X;
    }
    else
    {
      me = point1.Y;
      other = point2.Y;
    }
    return me.ApproximatelyEquals(other, tolerance);
  }

  private double DONOTUSEdistanceBetweenReferencePoints(
    XYZ point1,
    XYZ point2,
    DimensionEdge dimEdge)
  {
    double num1;
    double num2;
    if (dimEdge == DimensionEdge.Top || dimEdge == DimensionEdge.Bottom)
    {
      num1 = point1.X;
      num2 = point2.X;
    }
    else
    {
      num1 = point1.Y;
      num2 = point2.Y;
    }
    return Math.Abs(num2 - num1);
  }

  private bool createDimension(
    Document revitDoc,
    DimensionInfo dimInfo,
    Autodesk.Revit.DB.View targetView,
    AssemblyInstance targetAssembly,
    AssemblyInstance sourceAssembly,
    DimensionType sourceDimType,
    TextNoteType sourceTextNoteType,
    AutoTicketAppendString appendStringSettings,
    AutoTicketMinimumDimension minDimSettings,
    List<AutoTicketAppendStringParameterData> appendStringParameterDatas,
    AutoTicketCustomValues customValueSettings,
    AssemblyBoundingBoxInfo assemblyBoundingBoxInfo,
    out string warningMessage)
  {
    warningMessage = string.Empty;
    SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("f84b5886-3580-4842-9f4b-9f08ce6644d8"));
    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    schemaBuilder.AddSimpleField("PTACDimensionGuid", typeof (string)).SetDocumentation("Identifier for PTAC's dimension labels.");
    schemaBuilder.SetSchemaName("PTAC_Dimensions");
    Schema schema = schemaBuilder.Finish();
    double minDim = 0.0;
    if (minDimSettings != null)
      minDim = FeetAndInchesRounding.covertFeetInchToDouble(new string[2]
      {
        string.IsNullOrEmpty(minDimSettings.FeetValue) ? "" : minDimSettings.FeetValue,
        string.IsNullOrEmpty(minDimSettings.inchesValue) ? "" : minDimSettings.inchesValue
      });
    if (minDim <= 0.0)
      minDim = 0.0;
    sourceAssembly.GetStructuralFramingElement();
    Element sfElem = targetAssembly.GetStructuralFramingElement();
    FamilyInstance familyInstance = sfElem as FamilyInstance;
    DimPlacementCalculator placementCalculator = new DimPlacementCalculator(familyInstance, targetView as ViewSection);
    List<Reference> referenceList = new List<Reference>();
    List<ElementId> list = dimInfo.dimReferencesToPlace.Select<ReferencedPoint, ElementId>((Func<ReferencedPoint, ElementId>) (x => x.elementId)).ToList<ElementId>();
    Element elem = (Element) null;
    FamilyInstance famInst = (FamilyInstance) null;
    if (list.Count > 0 && list.FirstOrDefault<ElementId>() != (ElementId) null)
    {
      elem = revitDoc.GetElement(list.FirstOrDefault<ElementId>());
      famInst = elem as FamilyInstance;
    }
    bool flag1 = list.Where<ElementId>((Func<ElementId, bool>) (x => x != sfElem.Id && x != (ElementId) null)).Any<ElementId>();
    DimPlacementInfo dimPlacementInfo = placementCalculator.GetDimPlacementInfo(dimInfo.dimEdge, sourceDimType);
    DimReferencesManager refManager = new DimReferencesManager(revitDoc, dimPlacementInfo.DimensionFromEdge, dimPlacementInfo.DimAlongDirection, placementCalculator.GetDirectionVectorForDimPlacement(dimInfo.dimEdge));
    if (dimInfo.lowerSFExtent || dimInfo.upperSFExtent)
    {
      StructuralFramingBoundsObject framingBoundsObject = new StructuralFramingBoundsObject(familyInstance, targetView);
      Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, (Autodesk.Revit.DB.View) (targetView as ViewSection));
      DimensionEdge dimensionFromEdge = dimPlacementInfo.DimensionFromEdge;
      DimensionEdge key;
      switch (dimensionFromEdge)
      {
        case DimensionEdge.Left:
          key = DimensionEdge.Right;
          break;
        case DimensionEdge.Right:
          key = DimensionEdge.Left;
          break;
        case DimensionEdge.Top:
          key = DimensionEdge.Bottom;
          break;
        case DimensionEdge.Bottom:
          key = DimensionEdge.Top;
          break;
        default:
          key = DimensionEdge.None;
          break;
      }
      if (dimInfo.lowerSFExtent)
      {
        ReferencedPoint refPoint = pointsReferences[dimInfo.dimEdge][dimensionFromEdge];
        if (refManager.PointIsOverlappedSF(refPoint).Count == 0)
          refManager.DimReferences.Append(refPoint.Reference);
      }
      if (dimInfo.upperSFExtent)
      {
        ReferencedPoint refPoint = pointsReferences[dimInfo.dimEdge][key];
        if (refManager.PointIsOverlappedSF(refPoint).Count == 0)
          refManager.DimReferences.Append(refPoint.Reference);
      }
      if ((dimInfo.lowerSFExtent || dimInfo.upperSFExtent) && pointsReferences[dimInfo.dimEdge][dimensionFromEdge].LocalPoint.IsAlmostEqualTo(pointsReferences[dimInfo.dimEdge][key].LocalPoint))
      {
        string str = "Unable to correctly identify edges of the structural framing element to dimension. Curved geometry can result in incorrect dimensions and errors. ";
        warningMessage = str;
      }
    }
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (ReferencedPoint referencedPoint in dimInfo.dimReferencesToPlace.Distinct<ReferencedPoint>())
    {
      if (referencedPoint.elementId == sfElem.Id)
      {
        if (refManager.PointIsOverlappedSF(referencedPoint).Count == 0)
          refManager.DimReferences.Append(referencedPoint.Reference);
      }
      else if (refManager.AddReference(referencedPoint) != DimReferencesManager.addResult.DoNotAdd && !elementIdList.Contains(referencedPoint.elementId))
        elementIdList.Add(referencedPoint.elementId);
    }
    bool flag2 = false;
    foreach (Reference dimReference in refManager.DimReferences)
    {
      if (dimReference.ElementId.IntegerValue != sfElem.Id.IntegerValue)
        flag2 = true;
      if (!elementIdList.Contains(dimReference.ElementId))
        elementIdList.Add(dimReference.ElementId);
    }
    if (flag1 != flag2)
      return true;
    List<ElementId> source1 = new List<ElementId>();
    foreach (ElementId elementId1 in elementIdList.Distinct<ElementId>())
    {
      if (elementId1 != sfElem.Id && !source1.Contains(elementId1))
        source1.Add(elementId1);
      foreach (SortingInfoCloneTicketElement allType in dimInfo.allTypes)
      {
        if (!(allType.sortingName == "STRUCT FRAMING") && dimInfo.alignmentDictionary.ContainsKey(allType))
        {
          bool flag3 = false;
          foreach (List<ElementId> first in dimInfo.alignmentDictionary[allType])
          {
            if (first.Contains(elementId1))
            {
              foreach (ElementId elementId2 in first.Except<ElementId>((IEnumerable<ElementId>) elementIdList))
              {
                if (!source1.Contains(elementId2))
                  source1.Add(elementId2);
              }
              flag3 = false;
              break;
            }
          }
          if (flag3)
            break;
        }
      }
    }
    int count = source1.Count<ElementId>();
    if (minDim > 0.0 && !dimInfo.allSFRefs && refManager.DimReferences.Size > 0)
      refManager.DimReferences = TicketAutoDim_Command.AccountForMinRefs(revitDoc, refManager, dimPlacementInfo, minDim);
    Dictionary<ElementId, string> dictionary = new Dictionary<ElementId, string>();
    bool flag4 = true;
    if (!dimInfo.allSFRefs)
    {
      bool flag5 = true;
      string str = string.Empty;
      foreach (Reference dimReference in refManager.DimReferences)
      {
        if (!(dimReference.ElementId == sfElem.Id) && !dictionary.ContainsKey(dimReference.ElementId))
        {
          string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(revitDoc.GetElement(dimReference.ElementId), "LOCATION_IN_FORM");
          if (flag5)
          {
            str = parameterAsString;
            flag5 = false;
          }
          if (parameterAsString != str)
          {
            flag4 = false;
            break;
          }
        }
      }
    }
    XYZ vec1 = new XYZ(0.0, 0.0, 0.0);
    if (dimInfo.sourceDimension.Curve is Line)
      vec1 = (dimInfo.sourceDimension.Curve as Line).Direction;
    XYZ vec2 = assemblyBoundingBoxInfo.sourceViewTransform.Inverse.OfVector(vec1);
    XYZ xyz1 = assemblyBoundingBoxInfo.targetViewTransform.OfVector(vec2);
    Line line = dimPlacementInfo.PlacementLine;
    DetailLine detailLine;
    if (xyz1.IsAlmostEqualTo(new XYZ(0.0, 0.0, 0.0)))
    {
      detailLine = revitDoc.Create.NewDetailCurve(targetView, (Curve) Line.CreateBound(XYZ.Zero, dimPlacementInfo.PlacementLine.Direction.CrossProduct(targetView.ViewDirection).Normalize().Negate())) as DetailLine;
    }
    else
    {
      detailLine = revitDoc.Create.NewDetailCurve(targetView, (Curve) Line.CreateBound(XYZ.Zero, xyz1.CrossProduct(targetView.ViewDirection).Normalize().Negate())) as DetailLine;
      line = Line.CreateBound(dimPlacementInfo.PlacementLine.Origin - xyz1 * 2.0, dimPlacementInfo.PlacementLine.Origin + xyz1 * 2.0);
    }
    refManager.DimReferences.Append(detailLine.GeometryCurve.Reference);
    if (refManager.DimReferences.Size < 3)
    {
      revitDoc.Delete(detailLine.Id);
      return true;
    }
    Dimension dimension = revitDoc.Create.NewDimension(targetView, line, refManager.DimReferences);
    revitDoc.Delete(detailLine.Id);
    revitDoc.Regenerate();
    if (dimension == null)
      return false;
    XYZ dimMovementVector1 = this.getCorrectDimMovementVector(revitDoc, dimension, dimInfo.sourceDimension, assemblyBoundingBoxInfo, dimInfo.dimEdge);
    ElementTransformUtils.MoveElement(revitDoc, dimension.Id, dimMovementVector1);
    dimension.DimensionType = dimInfo.sourceDimension.DimensionType;
    Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimension, BuiltInParameter.LINEAR_DIM_TYPE);
    bool runningDimStyle = false;
    if (parameter1 != null && parameter1.AsValueString() == "Ordinate")
      runningDimStyle = true;
    List<AutoTicketDimSegment> dimSegs = new List<AutoTicketDimSegment>();
    double accuracy = revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).Accuracy;
    XYZ xyz2 = new XYZ();
    switch (dimInfo.dimEdge)
    {
      case DimensionEdge.Left:
        targetView.RightDirection.Negate().Normalize();
        break;
      case DimensionEdge.Right:
        targetView.RightDirection.Negate().Normalize();
        break;
      case DimensionEdge.Top:
        targetView.UpDirection.Normalize();
        break;
      case DimensionEdge.Bottom:
        targetView.UpDirection.Normalize();
        break;
    }
    foreach (DimensionSegment segment in dimension.Segments)
    {
      dimSegs.Add(new AutoTicketDimSegment(dimension, segment, dimPlacementInfo.DimAlongDirection, runningDimStyle));
      double num = segment.Value.Value;
    }
    dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE);
    dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE);
    XYZ xyz3 = new XYZ();
    double width = 0.0;
    double val2_1 = double.MaxValue;
    foreach (DimensionSegment segment in dimension.Segments)
    {
      double num = segment.Value.Value;
      width += num;
      XYZ xyz4 = segment.Origin + dimPlacementInfo.DimAlongDirection.CrossProduct(targetView.ViewDirection) * 0.5;
      XYZ source2 = segment.Origin - dimPlacementInfo.DimAlongDirection * num / 2.0;
      val2_1 = Math.Min(dimPlacementInfo.DimAlongDirection.DotProduct(source2), val2_1);
    }
    double midProj = val2_1 + width / 2.0;
    if (!runningDimStyle)
      AutoTicketDimSegment.HandleUpsets(width, midProj, dimSegs, dimInfo.dimEdge, accuracy);
    if (dimInfo.dimEdge == DimensionEdge.Bottom || dimInfo.dimEdge == DimensionEdge.Right)
    {
      XYZ xyz5 = dimPlacementInfo.DimAlongDirection.CrossProduct(targetView.ViewDirection).Normalize();
      List<double> source3 = new List<double>();
      double val2_2 = xyz5.DotProduct(dimPlacementInfo.ExtremePoint[ExtremePointType.SFBound]);
      if (dimPlacementInfo.ExtremePoint.ContainsKey(ExtremePointType.Bound))
        val2_2 = Math.Max(xyz5.DotProduct(dimPlacementInfo.ExtremePoint[ExtremePointType.Bound]), val2_2);
      double textBoundForDim = DimPlacementCalculator.GetTextBoundForDim(dimension, targetView);
      double flatTextBound = DimPlacementCalculator.GetFlatTextBound(dimension, targetView);
      XYZ source4;
      XYZ source5;
      if (dimension.Segments != null && dimension.NumberOfSegments > 0)
      {
        source4 = dimension.Segments.get_Item(0).Origin + xyz5.Negate() * textBoundForDim;
        source5 = dimension.Segments.get_Item(0).Origin + xyz5.Negate() * flatTextBound;
      }
      else
      {
        source4 = dimension.Origin + xyz5.Negate() * textBoundForDim;
        source5 = dimension.Origin + xyz5.Negate() * flatTextBound;
      }
      double num1 = xyz5.DotProduct(source4);
      source3.Add(num1);
      double num2 = xyz5.DotProduct(source5);
      source3.Add(num2);
      source3.Sort();
      if (source3.First<double>() != val2_2)
      {
        XYZ translation = xyz5 * Math.Abs(source3.Last<double>() - source3.First<double>());
        ElementTransformUtils.MoveElement(revitDoc, dimension.Id, translation);
        dimPlacementInfo.TextPlacementPoint += translation;
        if (!runningDimStyle)
        {
          foreach (AutoTicketDimSegment ticketDimSegment in dimSegs)
          {
            if (ticketDimSegment.hasLeader)
              ticketDimSegment.setTextPosition(ticketDimSegment.position + translation);
            else
              ticketDimSegment.setTextPosition(ticketDimSegment.segment.TextPosition);
          }
        }
      }
    }
    ElementId defaultElementTypeId = revitDoc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
    TextNoteOptions options = new TextNoteOptions();
    options.Rotation = dimPlacementInfo.TextAngle;
    options.TypeId = defaultElementTypeId;
    string text = "";
    string str1 = "OVERALL";
    string str2 = "CONTOUR";
    string str3 = "BLOCKOUT";
    if (customValueSettings != null)
    {
      str1 = customValueSettings.Overall;
      str2 = customValueSettings.Contour;
      str3 = customValueSettings.Blockout;
    }
    if (!dimInfo.allSFRefs && elem != null)
    {
      if (appendStringSettings != null)
      {
        if (appendStringParameterDatas == null || appendStringParameterDatas.Count == 0)
          appendStringParameterDatas = AutoTicketSettingsReader.returnDefaultParameters();
        string input = appendStringSettings.AppendStringValue;
        if (dimInfo.multipleTypes)
          input = input.Replace("{{DESC}}", "");
        if (!flag4)
          input = input.Replace("{{LIF}}", "");
        text = new AppendStringEvaluate(input, elem, famInst, count, appendStringParameterDatas).resultant.Replace("-  -", "-");
        if (string.IsNullOrEmpty(text))
          return false;
      }
      else
      {
        string str4 = !string.IsNullOrWhiteSpace(elem.GetIdentityDescriptionShort()) ? elem.GetIdentityDescriptionShort() : (famInst != null ? famInst.Symbol.FamilyName : "");
        string str5 = dictionary[elem.Id];
        if (!string.IsNullOrWhiteSpace(str5))
          str5 = $" ({str5})";
        if (!dimInfo.multipleTypes && !flag4)
          text = $"({count.ToString()})";
        else if (!dimInfo.multipleTypes)
          text = $"({count.ToString()}) {str5}";
        else if (!flag4)
          text = $"({count.ToString()}) {str4}";
        else
          text = $"({count.ToString()}) {str4}{str5}";
      }
      if (dimension.References.Size <= 2)
      {
        if (dimInfo.overallDimension)
        {
          text = str1;
        }
        else
        {
          bool flag6 = true;
          if (!dimInfo.allSFRefs)
          {
            foreach (Reference reference in dimension.References)
            {
              if (reference.ElementId != sfElem.Id)
              {
                flag6 = false;
                break;
              }
            }
          }
          if (flag6)
            text = str2;
        }
      }
    }
    else if (dimInfo.allSFRefs && dimension.References.Size > 2)
      text = str2;
    else if (dimension.References.Size <= 2)
    {
      if (dimInfo.overallDimension)
      {
        text = str1;
      }
      else
      {
        bool flag7 = true;
        if (!dimInfo.allSFRefs)
        {
          foreach (Reference reference in dimension.References)
          {
            if (reference.ElementId != sfElem.Id)
            {
              flag7 = false;
              break;
            }
          }
        }
        if (flag7)
          text = str2;
      }
    }
    if (dimInfo.blockoutDimension)
      text = str3;
    TextNote note = TextNote.Create(revitDoc, targetView.Id, dimPlacementInfo.TextPlacementPoint + dimMovementVector1, text, options);
    if (note != null)
    {
      revitDoc.Regenerate();
      if (sourceTextNoteType != null)
        note.TextNoteType = sourceTextNoteType;
      note.Coord = placementCalculator.BumpNote(note, dimInfo.dimEdge);
    }
    Parameter parameter2 = dimInfo.sourceDimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ);
    Parameter parameter3 = dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ);
    string str6 = string.Empty;
    if (parameter2 != null && parameter3 != null)
    {
      str6 = parameter2.AsValueString();
      int num = parameter2.AsInteger();
      parameter3.Set(num);
    }
    switch (str6)
    {
      case "Equality Text":
        Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimInfo.sourceDimension, BuiltInParameter.EQUALITY_TEXT_FOR_CONTINUOUS_LINEAR_DIM);
        Parameter parameter5 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimension, BuiltInParameter.EQUALITY_TEXT_FOR_CONTINUOUS_LINEAR_DIM);
        if (parameter4 != null && parameter5 != null)
        {
          string str7 = parameter4.AsValueString();
          parameter5.Set(str7);
          break;
        }
        break;
      case "Equality Formula":
        Parameter parameter6 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimInfo.sourceDimension, BuiltInParameter.EQUALITY_TEXT_FOR_CONTINUOUS_LINEAR_DIM);
        Parameter parameter7 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimension, BuiltInParameter.EQUALITY_TEXT_FOR_CONTINUOUS_LINEAR_DIM);
        if (parameter6 != null && parameter7 != null)
        {
          string str8 = parameter6.AsValueString();
          parameter7.Set(str8);
        }
        Parameter parameter8 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimInfo.sourceDimension, BuiltInParameter.EQUALITY_FORMULA);
        Parameter parameter9 = Utils.ElementUtils.Parameters.LookupParameter((Element) dimension, BuiltInParameter.EQUALITY_FORMULA);
        if (parameter8 != null && parameter9 != null)
        {
          ElementId elementId = parameter8.AsElementId();
          parameter9.Set(elementId);
          break;
        }
        break;
    }
    if (note != null)
    {
      double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) note.TextNoteType, BuiltInParameter.TEXT_SIZE);
      XYZ xyz6 = targetView.ViewDirection.CrossProduct(dimPlacementInfo.DimAlongDirection).Normalize();
      double num = parameterAsDouble * 1.5 / 2.0 * (double) targetView.Scale;
      XYZ translation = xyz6.Normalize() * num;
      ElementTransformUtils.MoveElement(revitDoc, note.Id, translation);
      Entity entity = new Entity(schema);
      schema.GetField("FileName");
      entity.Set<string>("PTACDimensionGuid", note.UniqueId);
      note.SetEntity(entity);
    }
    if (!runningDimStyle)
    {
      XYZ dimMovementVector2 = this.getCorrectDimMovementVector(revitDoc, dimension, dimInfo.sourceDimension, assemblyBoundingBoxInfo, dimInfo.dimEdge);
      ElementTransformUtils.MoveElement(revitDoc, dimension.Id, dimMovementVector2);
      if (note != null)
        ElementTransformUtils.MoveElement(revitDoc, note.Id, dimMovementVector2);
    }
    return true;
  }
}
