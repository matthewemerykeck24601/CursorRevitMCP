// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.TicketPopulatorCore
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketTemplateTools.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.TicketUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

internal class TicketPopulatorCore
{
  public static Result Execute(
    UIApplication revitApp,
    string strLastUsedScale,
    bool bMultiTicket,
    int intMultiSheetCurrentSheetNumber,
    string message)
  {
    UIDocument activeUiDocument = revitApp.ActiveUIDocument;
    Document revitDoc = activeUiDocument.Document;
    using (Transaction transaction = new Transaction(revitDoc, "Ticket Populator"))
    {
      if (transaction.Start() != TransactionStatus.Started)
      {
        TaskDialog.Show("Error", "Unable to start transaction group, please contact support");
        return (Result) 1;
      }
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        string pathName = revitDoc.PathName;
        if (revitApp.ActiveUIDocument.Selection.GetElementIds().Count < 1)
        {
          int num = (int) MessageBox.Show("Please select one assembly. Current selection is empty.");
          return (Result) 1;
        }
        ElementMulticategoryFilter multicategoryFilter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_StructuralFraming,
          BuiltInCategory.OST_Assemblies
        });
        List<ElementId> list1 = revitApp.ActiveUIDocument.Selection.GetElementIds().ToList<ElementId>().Where<ElementId>((Func<ElementId, bool>) (eid => multicategoryFilter.PassesFilter(revitDoc.GetElement(eid)))).ToList<ElementId>();
        bool flag1 = false;
        bool flag2 = false;
        List<ElementId> elementIdList1 = new List<ElementId>();
        List<string> stringList1 = new List<string>();
        int num1 = 0;
        int num2 = 0;
        foreach (ElementId id1 in list1)
        {
          ElementId id2 = (ElementId) null;
          Element element1 = revitDoc.GetElement(id1);
          if (element1 is FamilyInstance structFramingElem)
          {
            Element flatElement = (Element) AssemblyInstances.GetFlatElement(revitDoc, structFramingElem);
            if (flatElement != null)
            {
              id2 = flatElement.AssemblyInstanceId;
              if (!stringList1.Contains(id2.ToString()) && id2.ToString() != "-1" && revitDoc.GetElement(id2) is AssemblyInstance element2)
              {
                if (Utils.ElementUtils.Parameters.GetParameterAsBool((Element) element2, "HARDWARE_DETAIL"))
                  flag1 = true;
                else if (element2.GetStructuralFramingElement() != null)
                {
                  elementIdList1.Add(id2);
                  stringList1.Add(id2.ToString());
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
                elementIdList1.Add(id2);
              else
                flag2 = true;
            }
          }
          if (!(revitDoc.GetElement(id2) is AssemblyInstance))
            ++num1;
        }
        if (elementIdList1.Count > 1)
        {
          int num3 = (int) MessageBox.Show($"Please select only one assembly.  Current selection includes {elementIdList1.Count.ToString()} elements.");
          return (Result) 1;
        }
        if (flag1)
        {
          int num4 = (int) MessageBox.Show("Please select one valid assembly. The selection includes a hardware detail assembly. Please run the Hardware Detail tool for this assembly.");
          return (Result) 1;
        }
        if (flag2)
        {
          int num5 = (int) MessageBox.Show("Please select one valid assembly.  Please select a structural framing assembly.");
          return (Result) 1;
        }
        if (num1 == list1.Count)
        {
          int num6 = (int) MessageBox.Show("The Elements you selected are not an Assembly Instance. Please select one assembly.");
          return (Result) 1;
        }
        List<ElementId> source1 = elementIdList1;
        Element element = revitDoc.GetElement(source1.First<ElementId>());
        if (!(element is AssemblyInstance))
        {
          if (AssemblyInstances.GetFlatElement(revitDoc, element as FamilyInstance).AssemblyInstanceId != (ElementId) null)
          {
            if (revitDoc.GetElement(AssemblyInstances.GetFlatElement(revitDoc, element as FamilyInstance).AssemblyInstanceId) is AssemblyInstance)
            {
              element = revitDoc.GetElement(AssemblyInstances.GetFlatElement(revitDoc, element as FamilyInstance).AssemblyInstanceId);
            }
            else
            {
              int num7 = (int) MessageBox.Show($"The selected Element with Mark Number {Utils.ElementUtils.Parameters.GetParameterAsString(element, "CONTROL_MARK")} is not an Assembly Instance. Please select an assembly.");
              return (Result) 1;
            }
          }
          else
          {
            int num8 = (int) MessageBox.Show($"The selected Element with Mark Number {Utils.ElementUtils.Parameters.GetParameterAsString(element, "CONTROL_MARK")} is not an Assembly Instance. Please select an assembly.");
            return (Result) 1;
          }
        }
        AssemblyInstance assInstance = element as AssemblyInstance;
        if (activeUiDocument.Document.IsWorkshared)
        {
          List<ElementId> relinquishList;
          if (!CheckElementsOwnership.CheckOwnership("Ticket Populator", new List<ElementId>()
          {
            assInstance.Id
          }, revitDoc, activeUiDocument, out relinquishList))
          {
            int num9 = (int) transaction.RollBack();
            if (relinquishList.Count > 0)
            {
              RelinquishOptions generalCategories = new RelinquishOptions(false);
              generalCategories.CheckedOutElements = true;
              TransactWithCentralOptions options = new TransactWithCentralOptions();
              WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
            }
            return (Result) 1;
          }
        }
        List<Element> list2 = assInstance.GetMemberIds().ToList<ElementId>().Select<ElementId, Element>((Func<ElementId, Element>) (e => revitDoc.GetElement(e))).ToList<Element>();
        int num10 = 0;
        foreach (Element elem in list2)
        {
          foreach (Solid instanceSolid in Solids.GetInstanceSolids(elem))
          {
            if (instanceSolid.Volume > 0.0)
              ++num10;
          }
        }
        if (num10 < 1)
        {
          TaskDialog.Show("Ticket Populator Error", "No valid solid geometry in assembly structural framing. Could not run Ticket Populator.");
          return (Result) -1;
        }
        if (!assInstance.AllowsAssemblyViewCreation())
        {
          int num11 = (int) MessageBox.Show("Assembly views cannot be created for this assembly instance.  This is likely because assembly views have already been created for one instance of this assembly type.  Please acquire views on this instance or select the original instance and then run this command again.");
          return (Result) 1;
        }
        string constructionParamAsString = TicketParamUtils.GetConstructionParamAsString(assInstance);
        IEnumerable<BoundingBoxXYZ> source2 = (IEnumerable<BoundingBoxXYZ>) assInstance.GetMemberIds().Select<ElementId, BoundingBoxXYZ>((Func<ElementId, BoundingBoxXYZ>) (e => revitDoc.GetElement(e).get_BoundingBox((Autodesk.Revit.DB.View) null))).OrderByDescending<BoundingBoxXYZ, double>((Func<BoundingBoxXYZ, double>) (b => b.Max.DistanceTo(b.Min)));
        BoundingBoxXYZ boundingBoxXyz1 = (BoundingBoxXYZ) null;
        Transform transform1 = (Transform) null;
        if (source2.Any<BoundingBoxXYZ>())
        {
          boundingBoxXyz1 = source2.First<BoundingBoxXYZ>();
          Transform transform2 = boundingBoxXyz1.Transform;
          transform1 = assInstance.GetTransform();
        }
        PopulatorForm populatorForm = new PopulatorForm(revitDoc);
        populatorForm.assemblyBoundingBox = boundingBoxXyz1;
        populatorForm.assemblyTransform = transform1;
        populatorForm.TitleBlockNames = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).ToElements().Select<Element, string>((Func<Element, string>) (p => p.Name)).ToList<string>();
        populatorForm.ConstructionProduct = constructionParamAsString;
        populatorForm.defaultTemplateName = App.TemplateHistoryManager.GetDefaultTemplateName(pathName, constructionParamAsString);
        populatorForm.AssemblyName = assInstance.Name;
        populatorForm.ScaleUnits = ScalesManager.GetScaleUnitsForDocument(revitDoc);
        populatorForm.SelectedScale = strLastUsedScale;
        IEnumerable<ViewSheet> source3 = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).ToElements().Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId == assInstance.Id));
        List<string> list3 = source3.ToList<ViewSheet>().Select<ViewSheet, string>((Func<ViewSheet, string>) (s => s.SheetNumber)).ToList<string>();
        if (source3.Any<ViewSheet>())
        {
          intMultiSheetCurrentSheetNumber = source3.Count<ViewSheet>() + 1;
          while (list3.Contains(intMultiSheetCurrentSheetNumber.ToString()))
            ++intMultiSheetCurrentSheetNumber;
        }
        populatorForm.strMultiSheetNumberOverride = intMultiSheetCurrentSheetNumber.ToString();
        populatorForm.TicketTemplateSettingsPath = App.TicketTemplateSettingsPath;
        int num12 = (int) populatorForm.ShowDialog();
        if (populatorForm.DialogResult == DialogResult.Cancel)
        {
          populatorForm.Dispose();
          return (Result) 1;
        }
        if (activeUiDocument.Document.IsWorkshared)
        {
          TicketTemplate selectedTemplate = populatorForm.SelectedTemplate;
          if (selectedTemplate != null)
          {
            List<string> list4 = source1.Select<ElementId, AssemblyInstance>((Func<ElementId, AssemblyInstance>) (assemblyId => revitDoc.GetElement(assemblyId) as AssemblyInstance)).Select<AssemblyInstance, Element>((Func<AssemblyInstance, Element>) (assembly => assembly.GetStructuralFramingElement())).Select<Element, string>((Func<Element, string>) (sfElem => Utils.ElementUtils.Parameters.GetParameterAsString(sfElem, "DESIGN_NUMBER"))).ToList<string>();
            List<TicketLegendInfo> ticketLegendInfoList = new List<TicketLegendInfo>();
            List<ElementId> elementIdList2 = new List<ElementId>();
            List<string> namesToSearch = new List<string>();
            ticketLegendInfoList.AddRange((IEnumerable<TicketLegendInfo>) selectedTemplate.LegendInfos);
            foreach (TicketLegendInfo ticketLegendInfo in ticketLegendInfoList)
            {
              if (ticketLegendInfo.IsStrandPatternTemplate)
              {
                foreach (string newValue in list4)
                  namesToSearch.Add(ticketLegendInfo.LegendName.Replace("TEMPLATE", newValue));
              }
              else
                namesToSearch.Add(ticketLegendInfo.LegendName);
            }
            List<ElementId> relinquishList;
            if (!CheckElementsOwnership.CheckOwnership("Ticket Populator", new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (view => view.ViewType == ViewType.Legend && namesToSearch.Contains(view.Name))).Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (view => view.Id)).ToList<ElementId>(), revitDoc, activeUiDocument, out relinquishList))
            {
              int num13 = (int) transaction.RollBack();
              if (relinquishList.Count > 0)
              {
                RelinquishOptions generalCategories = new RelinquishOptions(false);
                generalCategories.CheckedOutElements = true;
                TransactWithCentralOptions options = new TransactWithCentralOptions();
                WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
              }
              return (Result) 1;
            }
          }
        }
        int selectedScaleFactor = populatorForm.SelectedScaleFactor;
        strLastUsedScale = populatorForm.SelectedScale;
        App.TemplateHistoryManager.Push(pathName, constructionParamAsString, populatorForm.SelectedTemplate.TemplateName);
        int result1 = 0;
        if (int.TryParse(populatorForm.strMultiSheetNumberOverride, out result1))
          intMultiSheetCurrentSheetNumber = result1;
        SubTransaction subTransaction1 = new SubTransaction(revitDoc);
        Dictionary<string, List<string>> viewsNotCreated = new Dictionary<string, List<string>>();
        if (subTransaction1.Start() == TransactionStatus.Started)
        {
          string AssemblySheetNumber = bMultiTicket ? populatorForm.strMultiSheetNumberOverride : intMultiSheetCurrentSheetNumber.ToString();
          ViewSheet ticket = populatorForm.SelectedTemplate.CreateTicket(assInstance, revitDoc, populatorForm.SelectedTitleBlock, AssemblySheetNumber, selectedScaleFactor, out string _, out List<Autodesk.Revit.DB.View> _, out viewsNotCreated);
          if (subTransaction1.Commit() != TransactionStatus.Committed)
          {
            message = "Failed to commit Views Creation transaction.";
            return (Result) -1;
          }
          if (viewsNotCreated.Count > 0)
          {
            string str1 = "The following views were created but could not be placed on the sheet. This could be due to the assembly being hidden in those views.\n";
            string str2 = "";
            foreach (KeyValuePair<string, List<string>> keyValuePair in viewsNotCreated)
            {
              str2 = $"{str2}\n{keyValuePair.Key}: ";
              int num14 = 1;
              int count = keyValuePair.Value.Count;
              foreach (string str3 in keyValuePair.Value)
              {
                str2 += num14 == count ? str3 + "." : str3 + ", ";
                ++num14;
              }
            }
            new TaskDialog("Ticket Populator")
            {
              MainContent = (str1 + str2)
            }.Show();
          }
          SubTransaction subTransaction2 = new SubTransaction(revitDoc);
          if (subTransaction2.Start() == TransactionStatus.Started)
          {
            try
            {
              string errMessage;
              Result functionResult;
              Dictionary<ElementId, string> instancesForAssembly = TicketBOMCore.GetScheduleInstancesForAssembly(revitDoc, revitApp.ActiveUIDocument, assInstance.Id, out errMessage, out functionResult);
              if (instancesForAssembly == null)
              {
                message = errMessage;
                return functionResult;
              }
              IEnumerable<FamilyInstance> source4 = new FilteredElementCollector(revitDoc, ticket.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements().Cast<FamilyInstance>();
              XYZ point = XYZ.Zero;
              double num15 = 0.0;
              double y = 0.0;
              if (source4.Any<FamilyInstance>())
              {
                FamilyInstance familyInstance = source4.First<FamilyInstance>();
                point = (familyInstance.Location as LocationPoint).Point;
                try
                {
                  num15 = familyInstance.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble();
                  y = familyInstance.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble();
                }
                catch
                {
                }
                if (PopulatorQA.inHouse)
                  TemplateDebugUtils.DebugDrawCircleAtPoint(point, revitDoc, (Autodesk.Revit.DB.View) ticket, "TitleBlock Location", false);
              }
              ScheduleSheetInstance scheduleSheetInstance1 = (ScheduleSheetInstance) null;
              XYZ xyz = point;
              double num16 = 1.0 / 72.0;
              List<TicketScheduleInfo> list5 = populatorForm.SelectedTemplate.ScheduleInfos.OrderBy<TicketScheduleInfo, int>((Func<TicketScheduleInfo, int>) (s => s.iScheduleOrderIndex)).ToList<TicketScheduleInfo>();
              IEnumerable<ViewSchedule> source5 = instancesForAssembly.Keys.Select<ElementId, Element>((Func<ElementId, Element>) (o => revitDoc.GetElement(o))).Cast<ViewSchedule>();
              IEnumerable<string> templateScheduleNames = list5.Select<TicketScheduleInfo, string>((Func<TicketScheduleInfo, string>) (s => s.BOMScheduleNameString.ToUpper()));
              HashSet<ElementId> elementIdSet = new HashSet<ElementId>();
              foreach (TicketScheduleInfo ticketScheduleInfo in list5)
              {
                if (TicketBOMCore.templateToSuffix.ContainsKey(ticketScheduleInfo.BOMScheduleNameString))
                  ticketScheduleInfo.BOMScheduleNameString = TicketBOMCore.templateToSuffix[ticketScheduleInfo.BOMScheduleNameString];
                source5.Select<ViewSchedule, string>((Func<ViewSchedule, string>) (s => s.Name)).ToList<string>();
                string schedInfoNewScheduleName = ticketScheduleInfo.GetNewScheduleName(assInstance.Name).ToUpper();
                IEnumerable<ViewSchedule> source6 = source5.Where<ViewSchedule>((Func<ViewSchedule, bool>) (o => o.Name.ToUpper().Contains(schedInfoNewScheduleName)));
                if (source6.Any<ViewSchedule>())
                {
                  if (source6.Count<ViewSchedule>() > 1)
                  {
                    TaskDialog taskDialog = new TaskDialog("Multiple Schedule Matches");
                    taskDialog.MainInstruction = "More than one schedule has been returned by BOMCore for schedule type " + ticketScheduleInfo.BOMScheduleNameString.ToString();
                    taskDialog.MainContent = "Only the first schedule will be placed in this situation.";
                    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "OK");
                    taskDialog.Show();
                  }
                  ViewSchedule viewSchedule = source6.First<ViewSchedule>();
                  elementIdSet.Add(viewSchedule.Id);
                  ScheduleSheetInstance scheduleSheetInstance2 = ScheduleSheetInstance.Create(revitDoc, ticket.Id, viewSchedule.Id, XYZ.Zero);
                  BoundingBoxXYZ boundingBoxXyz2 = scheduleSheetInstance2.get_BoundingBox((Autodesk.Revit.DB.View) ticket);
                  if (populatorForm.SelectedTemplate.BOMJustification == BOMJustification.Bottom)
                  {
                    scheduleSheetInstance2.Point += new XYZ(0.0, boundingBoxXyz2.Max.Y - boundingBoxXyz2.Min.Y - num16, 0.0);
                  }
                  else
                  {
                    int bomJustification = (int) populatorForm.SelectedTemplate.BOMJustification;
                  }
                  if (populatorForm.SelectedTemplate.bStackSchedules)
                  {
                    if (scheduleSheetInstance1 == null)
                    {
                      xyz = point + populatorForm.SelectedTemplate.BOMAnchorPosition;
                    }
                    else
                    {
                      BoundingBoxXYZ boundingBoxXyz3 = scheduleSheetInstance1.get_BoundingBox((Autodesk.Revit.DB.View) ticket);
                      if (populatorForm.SelectedTemplate.BOMJustification == BOMJustification.Bottom)
                        xyz += new XYZ(0.0, boundingBoxXyz3.Max.Y - boundingBoxXyz3.Min.Y - num16, 0.0);
                      else if (populatorForm.SelectedTemplate.BOMJustification == BOMJustification.Top)
                        xyz -= new XYZ(0.0, boundingBoxXyz3.Max.Y - boundingBoxXyz3.Min.Y - num16, 0.0);
                    }
                    scheduleSheetInstance2.Point += xyz;
                  }
                  else
                    scheduleSheetInstance2.Point += ticketScheduleInfo.vectorToAnchorPoint;
                  if (PopulatorQA.inHouse)
                    TemplateDebugUtils.DebugDrawCircleAtPoint(scheduleSheetInstance2.Point, revitDoc, (Autodesk.Revit.DB.View) ticket, "CurrentInstancePoint", false);
                  scheduleSheetInstance1 = scheduleSheetInstance2;
                }
                else
                  QA.InHouseMessage("Could not find schedule associated with this view.  ScheduleInfoName: " + ticketScheduleInfo.BOMScheduleNameString);
              }
              if (templateScheduleNames.Count<string>() != 0)
              {
                IEnumerable<ViewSchedule> viewSchedules = source5.Where<ViewSchedule>((Func<ViewSchedule, bool>) (s => !templateScheduleNames.Contains<string>(s.Name.ToUpper())));
                BoundingBoxXYZ boundingBoxXyz4 = (BoundingBoxXYZ) null;
                foreach (ViewSchedule viewSchedule in viewSchedules)
                {
                  if (!elementIdSet.Contains(viewSchedule.Id))
                  {
                    ScheduleSheetInstance scheduleSheetInstance3 = ScheduleSheetInstance.Create(revitDoc, ticket.Id, viewSchedule.Id, XYZ.Zero);
                    if (boundingBoxXyz4 == null)
                      xyz = point + new XYZ(num15 * 1.1, y, 0.0);
                    else
                      xyz -= new XYZ(0.0, boundingBoxXyz4.Max.Y - boundingBoxXyz4.Min.Y - num16, 0.0);
                    scheduleSheetInstance3.Point += xyz;
                    boundingBoxXyz4 = scheduleSheetInstance3.get_BoundingBox((Autodesk.Revit.DB.View) ticket);
                  }
                }
              }
              if (subTransaction2.Commit() != TransactionStatus.Committed)
              {
                message = "unable to commit BOM Populator Transaction.";
                return (Result) -1;
              }
            }
            catch (Exception ex)
            {
              message = $"Unhandled exception in BOM Schedule Generator: {ex.Message.ToString()} Inner: {ex.InnerException.ToString()}";
            }
            try
            {
              List<Element> invalidWeightInputsOut = new List<Element>();
              List<Element> weightParametesDoNotExistOut = new List<Element>();
              Result result2 = TitleblockPopCore.PopulateTicketTitleBlock(activeUiDocument, ref message, ticket, out invalidWeightInputsOut, out weightParametesDoNotExistOut);
              if (result2 != null)
              {
                int num17 = (int) transaction.RollBack();
                return result2;
              }
              if (weightParametesDoNotExistOut.Count > 0)
              {
                TaskDialog taskDialog = new TaskDialog("Ticket Populator Warning");
                taskDialog.MainContent = "Neither the UNIT_WEIGHT or WEIGHT_PER_UNIT parameters exist for one or more of the elements (shown below). This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
                taskDialog.ExpandedContent += "Elements:\n";
                List<string> stringList2 = new List<string>();
                foreach (Element elem in weightParametesDoNotExistOut)
                {
                  string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
                  if (!stringList2.Contains(str))
                    stringList2.Add(str);
                }
                stringList2.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
                foreach (string str in stringList2)
                  taskDialog.ExpandedContent += str;
                taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
                if (taskDialog.Show() != 1)
                {
                  int num18 = (int) transaction.RollBack();
                  return (Result) 1;
                }
              }
              if (invalidWeightInputsOut.Count > 0)
              {
                TaskDialog taskDialog = new TaskDialog("Ticket Populator Warning");
                taskDialog.MainContent = "There are one or more elements (shown below) in which the UNIT_WEIGHT or WEIGHT_PER_UNIT has a value equal to or less than zero. This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
                taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
                taskDialog.ExpandedContent += "Elements:\n";
                List<string> stringList3 = new List<string>();
                foreach (Element elem in invalidWeightInputsOut)
                {
                  string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
                  if (!stringList3.Contains(str))
                    stringList3.Add(str);
                }
                stringList3.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
                foreach (string str in stringList3)
                  taskDialog.ExpandedContent += str;
                if (taskDialog.Show() != 1)
                {
                  int num19 = (int) transaction.RollBack();
                  return (Result) 1;
                }
              }
            }
            catch (Exception ex)
            {
              message = $"Unhandled exception in TitleBlock Info Populator: {ex.Message.ToString()} Inner: {ex.InnerException.ToString()}";
            }
            if (activeUiDocument.Document.IsWorkshared)
            {
              ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
              elementsToCheckout.Add(assInstance.Id);
              WorksharingUtils.CheckoutElements(activeUiDocument.Document, elementsToCheckout);
            }
            if (transaction.Commit() != TransactionStatus.Committed)
            {
              TaskDialog.Show("Error", "Unable to commit transaction group.  Please contact support");
              return (Result) 1;
            }
            revitApp.ActiveUIDocument.RequestViewChange((Autodesk.Revit.DB.View) ticket);
            return (Result) 0;
          }
          message = "unable to start BOM Populator transaction.";
          return (Result) -1;
        }
        message = "Unable to start Views transaction.";
        return (Result) -1;
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("General Exception occurred in Ticket Populator: " + ex.Message);
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }
}
