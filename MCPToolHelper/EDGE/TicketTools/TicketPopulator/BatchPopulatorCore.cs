// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.BatchPopulatorCore
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.CopyTicketAnnotation;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketTemplateTools.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.TicketUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

internal class BatchPopulatorCore
{
  public static List<ElementId> CullSelectionForTicketPopulator(
    Document revitDoc,
    List<ElementId> selectedAssembly,
    out string error)
  {
    if (selectedAssembly.Count == 0)
    {
      error = "No elements selected;";
      return new List<ElementId>();
    }
    ElementMulticategoryFilter multicategoryFilter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_StructuralFraming,
      BuiltInCategory.OST_Assemblies
    });
    selectedAssembly = selectedAssembly.Where<ElementId>((Func<ElementId, bool>) (eid => multicategoryFilter.PassesFilter(revitDoc.GetElement(eid)))).ToList<ElementId>();
    if (selectedAssembly.Count == 0)
    {
      error = "No assembly/structural framing elements selected;";
      return new List<ElementId>();
    }
    error = "";
    bool flag1 = false;
    bool flag2 = false;
    List<ElementId> source1 = new List<ElementId>();
    List<string> stringList = new List<string>();
    int num1 = 0;
    int num2 = 0;
    foreach (ElementId id1 in selectedAssembly)
    {
      ElementId id2 = (ElementId) null;
      Element element1 = revitDoc.GetElement(id1);
      if (element1 is FamilyInstance structFramingElem)
      {
        Element flatElement = (Element) AssemblyInstances.GetFlatElement(revitDoc, structFramingElem);
        if (flatElement != null)
        {
          id2 = flatElement.AssemblyInstanceId;
          if (!stringList.Contains(id2.ToString()) && id2.ToString() != "-1" && revitDoc.GetElement(id2) is AssemblyInstance element2)
          {
            if (Utils.ElementUtils.Parameters.GetParameterAsBool((Element) element2, "HARDWARE_DETAIL"))
              flag1 = true;
            else if (element2.GetStructuralFramingElement() != null)
            {
              source1.Add(id2);
              stringList.Add(id2.ToString());
            }
            else
              flag2 = true;
          }
          ++num2;
        }
      }
      else
      {
        id2 = id1;
        if (element1 is AssemblyInstance assemblyInstance)
        {
          if (Utils.ElementUtils.Parameters.GetParameterAsBool((Element) assemblyInstance, "HARDWARE_DETAIL"))
            flag1 = true;
          else if (assemblyInstance.GetStructuralFramingElement() != null)
            source1.Add(id2);
          else
            flag2 = true;
        }
      }
      if (!(revitDoc.GetElement(id2) is AssemblyInstance))
        ++num1;
    }
    List<ElementId> list = source1.Distinct<ElementId>().ToList<ElementId>().GroupBy<ElementId, string, ElementId>((Func<ElementId, string>) (a => revitDoc.GetElement(a).Name), (Func<string, IEnumerable<ElementId>, ElementId>) ((key, group) => group.First<ElementId>())).ToList<ElementId>();
    if (num1 == selectedAssembly.Count)
    {
      error = "The Elements you selected are not Assembly Instances. Please select assemblies.";
      return new List<ElementId>();
    }
    if (flag1)
    {
      if (list.Count == 0)
      {
        error = "The selection includes hardware detail assemblies. Please run the Hardware Detail tool for these assemblies.";
        return new List<ElementId>();
      }
    }
    else if (flag2 && list.Count == 0)
    {
      error = "The selected assemblies do not contain a structural framing element.";
      return new List<ElementId>();
    }
    List<ElementId> listofassemblyWithMorethanOneSF = new List<ElementId>();
    List<ElementId> listofassemblyWithNoSF = new List<ElementId>();
    List<ElementId> listofassemblyWithNoSolidinSF = new List<ElementId>();
    foreach (ElementId id in list.ToList<ElementId>())
    {
      AssemblyInstance element = revitDoc.GetElement(id) as AssemblyInstance;
      IEnumerable<Element> source2 = element.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(element.Document.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320));
      if (source2.Count<Element>() == 1)
      {
        List<Solid> solidList = new List<Solid>();
        if (Solids.GetInstanceSolids(source2.First<Element>()).Count<Solid>() == 0)
          listofassemblyWithNoSolidinSF.Add(id);
      }
      else if (source2.Count<Element>() == 0)
        listofassemblyWithNoSF.Add(id);
      else
        listofassemblyWithMorethanOneSF.Add(id);
    }
    if (listofassemblyWithMorethanOneSF.Count<ElementId>() > 0)
    {
      string str = string.Join(",\n", listofassemblyWithMorethanOneSF.Select<ElementId, string>((Func<ElementId, string>) (id => revitDoc.GetElement(id).Name)).ToList<string>().ToArray());
      new TaskDialog("Batch Ticket Population")
      {
        MainContent = ("The following assemblies have more than one structural framing element. The operation will continue without these assemblies: \n" + str)
      }.Show();
    }
    if (listofassemblyWithNoSF.Count<ElementId>() > 0)
    {
      string str = string.Join(",\n", listofassemblyWithNoSF.Select<ElementId, string>((Func<ElementId, string>) (id => revitDoc.GetElement(id).Name)).ToList<string>().ToArray());
      new TaskDialog("Batch Ticket Population")
      {
        MainContent = ("The following assemblies do not contain a structural framing element. The operation will continue without these assemblies: \n" + str)
      }.Show();
    }
    if (listofassemblyWithNoSolidinSF.Count<ElementId>() > 0)
    {
      string str = string.Join(",\n", listofassemblyWithNoSolidinSF.Select<ElementId, string>((Func<ElementId, string>) (id => revitDoc.GetElement(id).Name)).ToList<string>().ToArray());
      new TaskDialog("Batch Ticket Population")
      {
        MainContent = ("The following assemblies do not contain any solids in the structural framing element. The operation will continue without these assemblies: \n" + str)
      }.Show();
    }
    list.RemoveAll((Predicate<ElementId>) (assem => listofassemblyWithNoSolidinSF.Contains(assem) || listofassemblyWithNoSF.Contains(assem) || listofassemblyWithMorethanOneSF.Contains(assem)));
    if (list.Count != 0)
      return list;
    error = "No selected assemblies were valid for Batch Ticket Population. The operation will terminate.";
    return new List<ElementId>();
  }

  public static Element GetAssemblyFromElement(Element elem, out string error)
  {
    error = "";
    Document document = elem.Document;
    Element assemblyFromElement = elem;
    if (!(assemblyFromElement is AssemblyInstance))
    {
      if (AssemblyInstances.GetFlatElement(document, assemblyFromElement as FamilyInstance).AssemblyInstanceId != (ElementId) null)
      {
        if (document.GetElement(AssemblyInstances.GetFlatElement(document, assemblyFromElement as FamilyInstance).AssemblyInstanceId) is AssemblyInstance)
        {
          assemblyFromElement = document.GetElement(AssemblyInstances.GetFlatElement(document, assemblyFromElement as FamilyInstance).AssemblyInstanceId);
        }
        else
        {
          string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(assemblyFromElement, "CONTROL_MARK");
          error = $"The selected Element with Mark Number {parameterAsString} is not an Assembly Instance. Please select an assembly.";
          return (Element) null;
        }
      }
      else
      {
        string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(assemblyFromElement, "CONTROL_MARK");
        error = $"The selected Element with Mark Number {parameterAsString} is not an Assembly Instance. Please select an assembly.";
        return (Element) null;
      }
    }
    return assemblyFromElement;
  }

  public static BatchPopulatorForm InitializeForm(
    Element elem,
    string strLastUsedScale,
    out string error,
    bool viewSpreadSheet,
    bool bAutoTicket = false)
  {
    error = "";
    Document revitDoc = elem.Document;
    BatchPopulatorForm batchPopulatorForm = new BatchPopulatorForm(revitDoc, viewSpreadSheet, bAutoTicket);
    int num1 = 1;
    Element assemblyFromElement = BatchPopulatorCore.GetAssemblyFromElement(elem, out error);
    if (assemblyFromElement == null)
      return (BatchPopulatorForm) null;
    AssemblyInstance assembly = assemblyFromElement as AssemblyInstance;
    List<Element> list = assembly.GetMemberIds().ToList<ElementId>().Select<ElementId, Element>((Func<ElementId, Element>) (e => revitDoc.GetElement(e))).ToList<Element>();
    int num2 = 0;
    foreach (Element elem1 in list)
    {
      foreach (Solid instanceSolid in Solids.GetInstanceSolids(elem1))
      {
        if (instanceSolid.Volume > 0.0)
          ++num2;
      }
    }
    if (num2 < 1)
    {
      error = "No valid solid geometry in assembly elements. Could not run Ticket Populator.";
      return (BatchPopulatorForm) null;
    }
    if (!assembly.AllowsAssemblyViewCreation())
    {
      error = "Assembly views cannot be created for this assembly instance.  This is likely because assembly views have already been created for one instance of this assembly type.  Please acquire views on this instance or select the original instance and then run this command again.";
      return (BatchPopulatorForm) null;
    }
    string constructionParamAsString = TicketParamUtils.GetConstructionParamAsString(assembly);
    IEnumerable<BoundingBoxXYZ> source = (IEnumerable<BoundingBoxXYZ>) assembly.GetMemberIds().Select<ElementId, BoundingBoxXYZ>((Func<ElementId, BoundingBoxXYZ>) (e => revitDoc.GetElement(e).get_BoundingBox((Autodesk.Revit.DB.View) null))).OrderByDescending<BoundingBoxXYZ, double>((Func<BoundingBoxXYZ, double>) (b => b.Max.DistanceTo(b.Min)));
    BoundingBoxXYZ boundingBoxXyz = (BoundingBoxXYZ) null;
    Transform transform1 = (Transform) null;
    if (source.Any<BoundingBoxXYZ>())
    {
      boundingBoxXyz = source.First<BoundingBoxXYZ>();
      Transform transform2 = boundingBoxXyz.Transform;
      transform1 = assembly.GetTransform();
    }
    batchPopulatorForm.TitleBlockNames = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).ToElements().Select<Element, string>((Func<Element, string>) (p => p.Name)).ToList<string>();
    batchPopulatorForm.defaultTemplateNames = App.TemplateHistoryManager.GetDefaultTemplateNames_BATCH(revitDoc.PathName, constructionParamAsString);
    batchPopulatorForm.assemblyBoundingBox = boundingBoxXyz;
    batchPopulatorForm.assemblyTransform = transform1;
    batchPopulatorForm.ConstructionProduct = constructionParamAsString;
    batchPopulatorForm.AssemblyName = assembly.Name;
    batchPopulatorForm.ScaleUnits = ScalesManager.GetScaleUnitsForDocument(revitDoc);
    batchPopulatorForm.SelectedScale = strLastUsedScale;
    batchPopulatorForm.strMultiSheetNumberOverride = num1.ToString();
    batchPopulatorForm.TicketTemplateSettingsPath = App.TicketTemplateSettingsPath;
    return batchPopulatorForm;
  }

  public static bool InvokeTitleBlockPopulator(
    UIDocument uidoc,
    ViewSheet newSheet,
    out string error,
    out List<Element> invalidWeightList,
    out List<Element> missingWeightParameter,
    bool bShowErrors = false)
  {
    error = "";
    invalidWeightList = new List<Element>();
    missingWeightParameter = new List<Element>();
    try
    {
      string message = "";
      if (TitleblockPopCore.PopulateTicketTitleBlock(uidoc, ref message, newSheet, out invalidWeightList, out missingWeightParameter, bShowErrors) != 1)
        return true;
      error = "Ticket title block population failed.";
      return false;
    }
    catch (Exception ex)
    {
      error = $"Unhandled exception in TitleBlock Info Populator: {ex.Message.ToString()} Inner: {ex.InnerException.ToString()}";
      return false;
    }
  }

  public static Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>> PopulateViews(
    UIDocument uidoc,
    Dictionary<ElementId, Dictionary<TicketTemplate, ViewSheet>> SheetDict,
    AssemblyInstance assInstance,
    List<TicketTemplate> templates,
    string strMultiSheetNumberOverride,
    string titleBlock,
    int intMultiSheetCurrentSheetNumber,
    int iViewsScale,
    bool bMultiTicket,
    bool bFirstAssem,
    StringBuilder ErrorBuilder,
    Dictionary<string, List<string>> viewsNotCreated,
    out List<Autodesk.Revit.DB.View> AssemblyViews,
    out Dictionary<string, List<string>> viewsNotCreatedOut,
    out List<Element> invalidWeightParametersMaster,
    out List<Element> missingWeightParametersMaster,
    out string error)
  {
    error = "";
    Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>> dictionary = new Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>>();
    Document document = assInstance.Document;
    SheetDict.Add(assInstance.Id, new Dictionary<TicketTemplate, ViewSheet>());
    bool flag = true;
    AssemblyViews = new List<Autodesk.Revit.DB.View>();
    viewsNotCreatedOut = new Dictionary<string, List<string>>();
    invalidWeightParametersMaster = new List<Element>();
    missingWeightParametersMaster = new List<Element>();
    foreach (TicketTemplate template in templates)
    {
      string errorContent = "";
      string AssemblySheetNumber = bMultiTicket ? strMultiSheetNumberOverride : intMultiSheetCurrentSheetNumber.ToString();
      List<Autodesk.Revit.DB.View> createdViews;
      ViewSheet ticket = template.CreateTicket(assInstance, document, string.IsNullOrEmpty(titleBlock) ? template.TitleBlockName : titleBlock, AssemblySheetNumber, iViewsScale, out errorContent, out createdViews, out viewsNotCreatedOut, false);
      if (errorContent != "")
      {
        ErrorBuilder.AppendLine($"Assembly: {assInstance.Name}        Ticket Template: {template.TemplateName}");
        ErrorBuilder.AppendLine("============");
        ErrorBuilder.AppendLine(errorContent);
      }
      foreach (KeyValuePair<string, List<string>> keyValuePair in viewsNotCreatedOut)
      {
        if (viewsNotCreated.ContainsKey(keyValuePair.Key))
          viewsNotCreated[keyValuePair.Key].AddRange((IEnumerable<string>) keyValuePair.Value);
        else
          viewsNotCreated.Add(keyValuePair.Key, keyValuePair.Value);
      }
      List<Element> invalidWeightList = new List<Element>();
      List<Element> missingWeightParameter = new List<Element>();
      if (!BatchPopulatorCore.InvokeTitleBlockPopulator(uidoc, ticket, out error, out invalidWeightList, out missingWeightParameter, bFirstAssem & flag))
        return (Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>>) null;
      if (invalidWeightList.Count > 0)
        invalidWeightParametersMaster.AddRange((IEnumerable<Element>) invalidWeightList);
      if (missingWeightParameter.Count > 0)
        missingWeightParametersMaster.AddRange((IEnumerable<Element>) missingWeightParameter);
      dictionary.Add(ticket, createdViews);
      AssemblyViews.AddRange((IEnumerable<Autodesk.Revit.DB.View>) createdViews);
      SheetDict[assInstance.Id].Add(template, ticket);
      ++intMultiSheetCurrentSheetNumber;
      flag = false;
    }
    viewsNotCreatedOut = viewsNotCreated;
    return dictionary;
  }

  public static bool PopulateBOM(
    UIDocument uidoc,
    AssemblyInstance assInstance,
    Dictionary<TicketTemplate, ViewSheet> AssemSheetDict,
    out string error)
  {
    error = "";
    try
    {
      Document revitDoc = assInstance.Document;
      foreach (TicketTemplate key in AssemSheetDict.Keys)
      {
        string errMessage;
        Dictionary<ElementId, string> instancesForAssembly = TicketBOMCore.GetScheduleInstancesForAssembly(revitDoc, uidoc, assInstance.Id, out errMessage, out Result _);
        if (instancesForAssembly == null)
        {
          error = errMessage;
          return false;
        }
        IEnumerable<FamilyInstance> source1 = new FilteredElementCollector(revitDoc, AssemSheetDict[key].Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements().Cast<FamilyInstance>();
        XYZ point = XYZ.Zero;
        double num1 = 0.0;
        double y = 0.0;
        if (source1.Any<FamilyInstance>())
        {
          FamilyInstance familyInstance = source1.First<FamilyInstance>();
          point = (familyInstance.Location as LocationPoint).Point;
          try
          {
            num1 = familyInstance.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble();
            y = familyInstance.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble();
          }
          catch
          {
          }
          if (PopulatorQA.inHouse)
            TemplateDebugUtils.DebugDrawCircleAtPoint(point, revitDoc, (Autodesk.Revit.DB.View) AssemSheetDict[key], "TitleBlock Location", false);
        }
        ScheduleSheetInstance scheduleSheetInstance1 = (ScheduleSheetInstance) null;
        XYZ xyz = point;
        double num2 = 1.0 / 72.0;
        List<TicketScheduleInfo> list = key.ScheduleInfos.OrderBy<TicketScheduleInfo, int>((Func<TicketScheduleInfo, int>) (s => s.iScheduleOrderIndex)).ToList<TicketScheduleInfo>();
        IEnumerable<ViewSchedule> source2 = instancesForAssembly.Keys.Select<ElementId, Element>((Func<ElementId, Element>) (o => revitDoc.GetElement(o))).Cast<ViewSchedule>();
        IEnumerable<string> templateScheduleNames = list.Select<TicketScheduleInfo, string>((Func<TicketScheduleInfo, string>) (s => s.BOMScheduleNameString.ToUpper()));
        HashSet<ElementId> elementIdSet = new HashSet<ElementId>();
        foreach (TicketScheduleInfo ticketScheduleInfo in list)
        {
          string scheduleNameString = ticketScheduleInfo.BOMScheduleNameString;
          if (TicketBOMCore.templateToSuffix.ContainsKey(ticketScheduleInfo.BOMScheduleNameString))
            ticketScheduleInfo.BOMScheduleNameString = TicketBOMCore.templateToSuffix[ticketScheduleInfo.BOMScheduleNameString];
          source2.Select<ViewSchedule, string>((Func<ViewSchedule, string>) (s => s.Name)).ToList<string>();
          string schedInfoNewScheduleName = ticketScheduleInfo.GetNewScheduleName(assInstance.Name).ToUpper();
          IEnumerable<ViewSchedule> source3 = source2.Where<ViewSchedule>((Func<ViewSchedule, bool>) (o => o.Name.ToUpper().Contains(schedInfoNewScheduleName)));
          if (source3.Any<ViewSchedule>())
          {
            if (source3.Count<ViewSchedule>() > 1)
            {
              TaskDialog taskDialog = new TaskDialog("Multiple Schedule Matches");
              taskDialog.MainInstruction = "More than one schedule has been returned by BOMCore for schedule type " + ticketScheduleInfo.BOMScheduleNameString.ToString();
              taskDialog.MainContent = "Only the first schedule will be placed in this situation.";
              taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "OK");
              taskDialog.Show();
            }
            ViewSchedule viewSchedule = source3.First<ViewSchedule>();
            elementIdSet.Add(viewSchedule.Id);
            ScheduleSheetInstance scheduleSheetInstance2 = ScheduleSheetInstance.Create(revitDoc, AssemSheetDict[key].Id, viewSchedule.Id, XYZ.Zero);
            BoundingBoxXYZ boundingBoxXyz1 = scheduleSheetInstance2.get_BoundingBox((Autodesk.Revit.DB.View) AssemSheetDict[key]);
            if (key.BOMJustification == BOMJustification.Bottom)
            {
              scheduleSheetInstance2.Point += new XYZ(0.0, boundingBoxXyz1.Max.Y - boundingBoxXyz1.Min.Y - num2, 0.0);
            }
            else
            {
              int bomJustification = (int) key.BOMJustification;
            }
            if (key.bStackSchedules)
            {
              if (scheduleSheetInstance1 == null)
              {
                xyz = point + key.BOMAnchorPosition;
              }
              else
              {
                BoundingBoxXYZ boundingBoxXyz2 = scheduleSheetInstance1.get_BoundingBox((Autodesk.Revit.DB.View) AssemSheetDict[key]);
                if (key.BOMJustification == BOMJustification.Bottom)
                  xyz += new XYZ(0.0, boundingBoxXyz2.Max.Y - boundingBoxXyz2.Min.Y - num2, 0.0);
                else if (key.BOMJustification == BOMJustification.Top)
                  xyz -= new XYZ(0.0, boundingBoxXyz2.Max.Y - boundingBoxXyz2.Min.Y - num2, 0.0);
              }
              scheduleSheetInstance2.Point += xyz;
            }
            else
              scheduleSheetInstance2.Point += ticketScheduleInfo.vectorToAnchorPoint;
            if (PopulatorQA.inHouse)
              TemplateDebugUtils.DebugDrawCircleAtPoint(scheduleSheetInstance2.Point, revitDoc, (Autodesk.Revit.DB.View) AssemSheetDict[key], "CurrentInstancePoint", false);
            scheduleSheetInstance1 = scheduleSheetInstance2;
          }
          else
            QA.InHouseMessage("Could not find schedule associated with this view.  ScheduleInfoName: " + ticketScheduleInfo.BOMScheduleNameString);
        }
        if (templateScheduleNames.Count<string>() != 0)
        {
          IEnumerable<ViewSchedule> viewSchedules = source2.Where<ViewSchedule>((Func<ViewSchedule, bool>) (s => !templateScheduleNames.Contains<string>(s.Name.ToUpper())));
          BoundingBoxXYZ boundingBoxXyz = (BoundingBoxXYZ) null;
          foreach (ViewSchedule viewSchedule in viewSchedules)
          {
            if (!elementIdSet.Contains(viewSchedule.Id))
            {
              ScheduleSheetInstance scheduleSheetInstance3 = ScheduleSheetInstance.Create(revitDoc, AssemSheetDict[key].Id, viewSchedule.Id, XYZ.Zero);
              if (boundingBoxXyz == null)
                xyz = point + new XYZ(num1 * 1.1, y, 0.0);
              else
                xyz -= new XYZ(0.0, boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num2, 0.0);
              scheduleSheetInstance3.Point += xyz;
              boundingBoxXyz = scheduleSheetInstance3.get_BoundingBox((Autodesk.Revit.DB.View) AssemSheetDict[key]);
            }
          }
        }
      }
      return true;
    }
    catch (Exception ex)
    {
      error = $"Unhandled exception in BOM Schedule Generator: {ex.Message.ToString()} Inner: {ex.InnerException.ToString()}";
      return false;
    }
  }

  public static Result Execute(
    UIApplication revitApp,
    string strLastUsedScale,
    bool bMultiTicket,
    int intMultiSheetCurrentSheetNumber,
    List<ElementId> selectedIds,
    string message,
    out ViewSheet AssemblySheet,
    out Dictionary<ElementId, Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>>> CreatedSheets,
    out List<ElementId> relinquishList,
    out bool viewSpreadSheetEnhancement,
    bool bAutoTicket = false)
  {
    viewSpreadSheetEnhancement = true;
    relinquishList = new List<ElementId>();
    AssemblySheet = (ViewSheet) null;
    CreatedSheets = new Dictionary<ElementId, Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>>>();
    StringBuilder ErrorBuilder = new StringBuilder();
    UIDocument activeUiDocument = revitApp.ActiveUIDocument;
    Document revitDoc = activeUiDocument.Document;
    try
    {
      string pathName = revitDoc.PathName;
      List<ElementId> list1 = selectedIds.ToList<ElementId>();
      bool bFirstAssem = true;
      Dictionary<ElementId, Dictionary<TicketTemplate, ViewSheet>> SheetDict = new Dictionary<ElementId, Dictionary<TicketTemplate, ViewSheet>>();
      int iViewsScale = -1;
      List<TicketTemplate> ticketTemplateList = new List<TicketTemplate>();
      int num1 = -1;
      string titleBlock = "";
      SourceAssembly sourceAssembly = (SourceAssembly) null;
      if (!CheckElementsOwnership.CheckOwnership("Ticket Populator", list1, revitDoc, activeUiDocument, out relinquishList))
        return (Result) 1;
      Dictionary<string, List<string>> viewsNotCreated = new Dictionary<string, List<string>>();
      List<Element> elementList1 = new List<Element>();
      List<Element> elementList2 = new List<Element>();
      foreach (Element element1 in list1.Select<ElementId, Element>((Func<ElementId, Element>) (eid => revitDoc.GetElement(eid))))
      {
        AssemblyInstance assInstance = element1 as AssemblyInstance;
        assInstance.GetMemberIds().ToList<ElementId>().Select<ElementId, Element>((Func<ElementId, Element>) (e => revitDoc.GetElement(e))).ToList<Element>();
        string error1 = "";
        string constructionParamAsString = TicketParamUtils.GetConstructionParamAsString(assInstance);
        IEnumerable<ViewSheet> source1 = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).ToElements().Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId == assInstance.Id));
        List<string> list2 = source1.ToList<ViewSheet>().Select<ViewSheet, string>((Func<ViewSheet, string>) (s => s.SheetNumber)).ToList<string>();
        if (source1.Any<ViewSheet>())
        {
          intMultiSheetCurrentSheetNumber = source1.Count<ViewSheet>() + 1;
          while (list2.Contains(intMultiSheetCurrentSheetNumber.ToString()))
            ++intMultiSheetCurrentSheetNumber;
        }
        if (bFirstAssem)
        {
          BatchPopulatorForm batchPopulatorForm = BatchPopulatorCore.InitializeForm((Element) assInstance, strLastUsedScale, out error1, App.ViewSpreadsheet, bAutoTicket);
          if (batchPopulatorForm == null)
          {
            int num2 = (int) MessageBox.Show($"{assInstance.Name}: {error1}");
            return (Result) 1;
          }
          if (batchPopulatorForm.ShowDialog() == DialogResult.Cancel)
          {
            viewSpreadSheetEnhancement = batchPopulatorForm.ViewSpreadSheetEnhancement;
            App.ViewSpreadsheet = viewSpreadSheetEnhancement;
            return (Result) 1;
          }
          viewSpreadSheetEnhancement = batchPopulatorForm.ViewSpreadSheetEnhancement;
          App.ViewSpreadsheet = viewSpreadSheetEnhancement;
          iViewsScale = batchPopulatorForm.SelectedScaleFactor;
          strLastUsedScale = batchPopulatorForm.SelectedScale;
          ticketTemplateList = batchPopulatorForm.SelectedTemplates;
          if (ticketTemplateList.Count > 0)
          {
            List<string> list3 = list1.Select<ElementId, AssemblyInstance>((Func<ElementId, AssemblyInstance>) (assemblyId => revitDoc.GetElement(assemblyId) as AssemblyInstance)).Select<AssemblyInstance, Element>((Func<AssemblyInstance, Element>) (assembly => assembly.GetStructuralFramingElement())).Select<Element, string>((Func<Element, string>) (sfElem => Utils.ElementUtils.Parameters.GetParameterAsString(sfElem, "DESIGN_NUMBER"))).ToList<string>();
            List<TicketLegendInfo> ticketLegendInfoList = new List<TicketLegendInfo>();
            List<ElementId> elementIdList = new List<ElementId>();
            List<string> namesToSearch = new List<string>();
            foreach (TicketTemplate ticketTemplate in ticketTemplateList)
              ticketLegendInfoList.AddRange((IEnumerable<TicketLegendInfo>) ticketTemplate.LegendInfos);
            foreach (TicketLegendInfo ticketLegendInfo in ticketLegendInfoList)
            {
              if (ticketLegendInfo.IsStrandPatternTemplate)
              {
                foreach (string newValue in list3)
                  namesToSearch.Add(ticketLegendInfo.LegendName.Replace("TEMPLATE", newValue));
              }
              else
                namesToSearch.Add(ticketLegendInfo.LegendName);
            }
            if (!CheckElementsOwnership.CheckOwnership("Ticket Populator", new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (view => view.ViewType == ViewType.Legend && namesToSearch.Contains(view.Name))).Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (view => view.Id)).ToList<ElementId>(), revitDoc, activeUiDocument, out relinquishList))
              return (Result) 1;
          }
          titleBlock = batchPopulatorForm.SelectedTitleBlock;
          if (bAutoTicket)
            sourceAssembly = batchPopulatorForm.CopySource;
        }
        if (ticketTemplateList.Count > 0)
          App.TemplateHistoryManager.Push_BATCH(pathName, constructionParamAsString, ticketTemplateList.Select<TicketTemplate, string>((Func<TicketTemplate, string>) (t => t.TemplateName)).ToList<string>());
        if (num1 != -1)
          intMultiSheetCurrentSheetNumber = num1;
        SubTransaction subTransaction = new SubTransaction(revitDoc);
        List<Element> invalidWeightParametersMaster = new List<Element>();
        List<Element> missingWeightParametersMaster = new List<Element>();
        if (subTransaction.Start() == TransactionStatus.Started)
        {
          List<Autodesk.Revit.DB.View> AssemblyViews = new List<Autodesk.Revit.DB.View>();
          Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>> dictionary1 = new Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>>();
          Dictionary<string, List<string>> viewsNotCreatedOut = new Dictionary<string, List<string>>();
          string error2;
          Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>> dictionary2 = BatchPopulatorCore.PopulateViews(activeUiDocument, SheetDict, assInstance, ticketTemplateList, num1.ToString(), titleBlock, intMultiSheetCurrentSheetNumber, iViewsScale, bMultiTicket, bFirstAssem, ErrorBuilder, viewsNotCreated, out AssemblyViews, out viewsNotCreatedOut, out invalidWeightParametersMaster, out missingWeightParametersMaster, out error2);
          if (dictionary2 == null)
          {
            TaskDialog.Show("Error", error2);
            int num3 = (int) subTransaction.RollBack();
            return (Result) 1;
          }
          if (invalidWeightParametersMaster.Count > 0)
          {
            foreach (Element element2 in invalidWeightParametersMaster)
            {
              if (!elementList1.Contains(element2))
                elementList1.Add(element2);
            }
          }
          if (missingWeightParametersMaster.Count > 0)
          {
            foreach (Element element3 in missingWeightParametersMaster)
            {
              if (!elementList2.Contains(element3))
                elementList2.Add(element3);
            }
          }
          foreach (KeyValuePair<string, List<string>> keyValuePair in viewsNotCreatedOut)
          {
            if (viewsNotCreated.ContainsKey(keyValuePair.Key))
            {
              if (viewsNotCreated[keyValuePair.Key].Count<string>() != keyValuePair.Value.Count<string>())
              {
                foreach (string str in keyValuePair.Value)
                {
                  if (!viewsNotCreated[keyValuePair.Key].Contains(str))
                    viewsNotCreated[keyValuePair.Key].Add(str);
                }
              }
            }
            else
              viewsNotCreated.Add(keyValuePair.Key, keyValuePair.Value);
          }
          List<ViewSheet> list4 = dictionary2.Keys.ToList<ViewSheet>();
          CreatedSheets.Add(assInstance.Id, new Dictionary<ViewSheet, List<Autodesk.Revit.DB.View>>());
          int num4 = (int) subTransaction.Commit();
          foreach (ViewSheet key in dictionary2.Keys)
            CreatedSheets[assInstance.Id].Add(key, dictionary2[key]);
          AssemblySheet = AssemblySheet == null ? list4.First<ViewSheet>() : AssemblySheet;
          if (sourceAssembly != null)
          {
            foreach (ViewSheet viewSheet in list4)
            {
              ViewSheet sheet = viewSheet;
              ViewSheet source2 = sourceAssembly.Sheets.FirstOrDefault<ViewSheet>((Func<ViewSheet, bool>) (s => s.SheetNumber.Equals(sheet.SheetNumber)));
              if (source2 != null)
                EDGE.TicketTools.CopyTicketViews.CopyTicketViews.copyTicketViews(source2, sheet);
            }
            foreach (Autodesk.Revit.DB.View view1 in AssemblyViews)
            {
              Autodesk.Revit.DB.View createdView = view1;
              Autodesk.Revit.DB.View viewSheetCopyFrom = sourceAssembly.Views.FirstOrDefault<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (view => view.Name.Equals(createdView.Name)));
              if (viewSheetCopyFrom != null)
                CopyTicketAnnotations.copyTicketAnnotations((Element) viewSheetCopyFrom, (Element) createdView);
            }
          }
          bFirstAssem = false;
        }
        else
        {
          message = "Unable to start Views transaction.";
          return (Result) -1;
        }
      }
      if (viewsNotCreated.Count > 0)
      {
        string str1 = "The following views were created but could not be placed on the sheet. This could be due to the assembly being hidden in those views.\n";
        string str2 = "";
        foreach (KeyValuePair<string, List<string>> keyValuePair in viewsNotCreated)
        {
          str2 = $"{str2}\n{keyValuePair.Key}: ";
          int num5 = 1;
          int count = keyValuePair.Value.Count;
          foreach (string str3 in keyValuePair.Value)
          {
            str2 += num5 == count ? str3 + "." : str3 + ", ";
            ++num5;
          }
        }
        new TaskDialog("Batch Ticket Populator")
        {
          MainContent = (str1 + str2)
        }.Show();
      }
      string str4 = "Batch Ticket Populator Warning";
      if (bAutoTicket)
        str4 = "Auto Ticket Generation Message";
      if (elementList2.Count > 0)
      {
        TaskDialog taskDialog = new TaskDialog(str4);
        taskDialog.MainContent = "Neither the UNIT_WEIGHT or WEIGHT_PER_UNIT parameters exist for one or more of the elements (shown below). This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
        taskDialog.ExpandedContent += "Elements:\n";
        List<string> stringList = new List<string>();
        foreach (Element elem in elementList2)
        {
          string str5 = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
          if (!stringList.Contains(str5))
            stringList.Add(str5);
        }
        stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
        foreach (string str6 in stringList)
          taskDialog.ExpandedContent += str6;
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
        if (taskDialog.Show() != 1)
          return (Result) 1;
      }
      if (elementList1.Count > 0)
      {
        TaskDialog taskDialog = new TaskDialog(str4);
        taskDialog.MainContent = "There are one or more elements (shown below) in which the UNIT_WEIGHT or WEIGHT_PER_UNIT has a value equal to or less than zero. This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
        taskDialog.ExpandedContent += "Elements:\n";
        List<string> stringList = new List<string>();
        foreach (Element elem in elementList1)
        {
          string str7 = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
          if (!stringList.Contains(str7))
            stringList.Add(str7);
        }
        stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
        foreach (string str8 in stringList)
          taskDialog.ExpandedContent += str8;
        if (taskDialog.Show() != 1)
          return (Result) 1;
      }
      foreach (Element element in list1.Select<ElementId, Element>((Func<ElementId, Element>) (eid => revitDoc.GetElement(eid))))
      {
        ElementId id = element.Id;
        AssemblyInstance assInstance = element as AssemblyInstance;
        SubTransaction subTransaction = new SubTransaction(revitDoc);
        if (subTransaction.Start() == TransactionStatus.Started)
        {
          string error;
          if (!BatchPopulatorCore.PopulateBOM(activeUiDocument, assInstance, SheetDict[id], out error))
          {
            TaskDialog.Show("Error", error);
            int num6 = (int) subTransaction.RollBack();
            return (Result) 1;
          }
          int num7 = (int) subTransaction.Commit();
          if (activeUiDocument.Document.IsWorkshared)
          {
            ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
            elementsToCheckout.Add(assInstance.Id);
            WorksharingUtils.CheckoutElements(activeUiDocument.Document, elementsToCheckout);
          }
        }
        else
        {
          message = "unable to start BOM Populator transaction.";
          return (Result) -1;
        }
      }
      if (ErrorBuilder.Length > 0)
      {
        new TaskDialog("EDGE Warning")
        {
          MainInstruction = "Warnings encountered during ticket legend population",
          MainContent = "See expanded content for specific legends not placed.  This is typically due to EDGE not being able to locate the legend view in the project list of legend views",
          ExpandedContent = ErrorBuilder.ToString(),
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = true
        }.Show();
        QA.LogLine(ErrorBuilder.ToString());
      }
      return (Result) 0;
    }
    catch (Exception ex)
    {
      TaskDialog.Show("Batch Ticket Populator", "General Exception occurred in Ticket Populator: " + ex.StackTrace);
      return (Result) -1;
    }
  }
}
