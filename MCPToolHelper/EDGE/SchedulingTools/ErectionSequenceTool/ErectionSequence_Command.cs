// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ErectionSequenceTool.ErectionSequence_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.SchedulingTools.ErectionSequenceTool;

[Transaction(TransactionMode.Manual)]
public class ErectionSequence_Command : IExternalCommand
{
  private string ESParamName = "ERECTION_SEQUENCE_NUMBER";
  private string ESZNameParamName = "ERECTION_SEQUENCE_ZONE_NAME";
  private string ESZNumberParamName = "ERECTION_SEQUENCE_ZONE_NUMBER";
  private Document revit_Doc;
  private bool workSharingFail;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = commandData.Application.ActiveUIDocument.Document;
    this.revit_Doc = document;
    string str1 = "Erection Sequence Tool";
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = (str1 + " Must be run in the Project Environment"),
        MainContent = $"You are currently in the family editor, {str1} must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (document.ActiveView.ViewType != ViewType.ThreeD)
    {
      new TaskDialog("3D View")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = (str1 + " Must be run in 3D View"),
        MainContent = $"You are currently in {document.ActiveView.ViewType.ToString()}. {str1} must be run in the 3D View.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (!this.CheckFilter_Template(document, activeUiDocument, new Dictionary<string, bool>()
    {
      {
        "ERECTION_SEQUENCE_UNASSIGNED",
        true
      },
      {
        "ERECTION_SEQUENCE_ASSIGNED",
        true
      },
      {
        "ERECTION SEQUENCE ASSIGNING",
        false
      },
      {
        "ERECTION SEQUENCE ASSIGNED",
        false
      }
    }))
      return (Result) 1;
    List<Element> list1 = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (x => x is FamilyInstance elem1 && !elem1.HasSuperComponent())).ToList<Element>();
    Dictionary<int, List<Parameter>> parametersDictionary = new Dictionary<int, List<Parameter>>();
    if (list1.Count == 0)
    {
      new TaskDialog("Erection Sequence")
      {
        MainInstruction = "No Pieces in the Model",
        MainContent = "Erection Sequence requires at least one piece (structural framing element) to exist within the model."
      }.Show();
      return (Result) 1;
    }
    List<ErectionSequenceZone> erectionSequenceZoneList = ErectionSequenceZoneExtensibleStorage.ReadErectionSequenceZones(document) ?? new List<ErectionSequenceZone>();
    Dictionary<int, List<Element>> dictionary1 = new Dictionary<int, List<Element>>();
    foreach (int key in erectionSequenceZoneList.Select<ErectionSequenceZone, int>((Func<ErectionSequenceZone, int>) (x => x.ZoneIndex)))
    {
      if (!dictionary1.ContainsKey(key))
        dictionary1.Add(key, new List<Element>());
    }
    Parameter parameter1 = Parameters.LookupParameter(list1.First<Element>(), this.ESParamName);
    Parameter parameter2 = Parameters.LookupParameter(list1.First<Element>(), this.ESZNameParamName);
    Parameter parameter3 = Parameters.LookupParameter(list1.First<Element>(), this.ESZNumberParamName);
    if (parameter1 == null || parameter2 == null || parameter3 == null)
    {
      new TaskDialog("Erection Sequence")
      {
        MainInstruction = "Erection Sequence Shared Parameters Missing",
        MainContent = "One or more Erection Sequence shared parameters do not exist. Please run Project Shared Parameters and try again."
      }.Show();
      return (Result) 1;
    }
    Dictionary<Element, int> dictionary2 = new Dictionary<Element, int>();
    List<Element> elements1 = new List<Element>();
    List<Element> elementList1 = new List<Element>();
    foreach (Element element in list1)
    {
      Parameter parameter4 = Parameters.LookupParameter(element, this.ESZNumberParamName);
      if (parameter4 == null)
      {
        new TaskDialog("Erection Sequence")
        {
          MainInstruction = "Erection Sequence Shared Parameters Missing",
          MainContent = "One or more Erection Sequence shared parameters do not exist. Please run Project Shared Parameters and try again."
        }.Show();
        return (Result) 1;
      }
      Parameter parameter5 = Parameters.LookupParameter(element, this.ESZNameParamName);
      if (parameter5 == null)
      {
        new TaskDialog("Erection Sequence")
        {
          MainInstruction = "Erection Sequence Shared Parameters Missing",
          MainContent = "One or more Erection Sequence shared parameters do not exist. Please run Project Shared Parameters and try again."
        }.Show();
        return (Result) 1;
      }
      int zoneValue = -1;
      if (!parameter4.HasValue)
      {
        if (!string.IsNullOrEmpty(parameter5.AsString()))
          dictionary2.Add(element, 0);
      }
      else
      {
        zoneValue = parameter4.AsInteger();
        if (zoneValue > 0 && !dictionary1.ContainsKey(zoneValue))
        {
          elements1.Add(element);
          continue;
        }
        if (dictionary1.ContainsKey(zoneValue))
          dictionary1[zoneValue].Add(element);
        string str2 = (string) null;
        if (parameter5.HasValue)
          str2 = parameter5.AsString();
        if (zoneValue <= 0)
        {
          if (!string.IsNullOrWhiteSpace(str2))
            dictionary2.Add(element, zoneValue);
        }
        else
        {
          ErectionSequenceZone erectionSequenceZone = erectionSequenceZoneList.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.ZoneIndex == zoneValue)).FirstOrDefault<ErectionSequenceZone>();
          if (erectionSequenceZone != null && erectionSequenceZone.ZoneName != str2)
            dictionary2.Add(element, zoneValue);
        }
      }
      if ((zoneValue < 1 || !dictionary1.ContainsKey(zoneValue)) && Parameters.GetParameterAsInt(element, this.ESParamName) > 0)
        elementList1.Add(element);
    }
    bool flag1 = elements1.Count > 0;
    bool flag2 = dictionary2.Count > 0;
    if (flag1 | flag2)
    {
      TaskDialog taskDialog = new TaskDialog("Erection Sequence");
      taskDialog.MainInstruction = "Invalid Erection Sequence Zones";
      taskDialog.MainContent = "One or more pieces have invalid erection sequence zone name or number parameter values.";
      if (flag1)
        taskDialog.MainContent += "\n\nOne or more pieces in the model have invalid erection sequence zone numbers.";
      if (flag2)
        taskDialog.MainContent += "\n\nOne or more pieces in the model have erection sequence zone names that do not match their current erection zone number.";
      if (flag1)
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Clear Invalid Erection Sequence Zone Numbers", "Clear all erection sequence parameters for all pieces with invalid zone numbers.");
      if (flag2)
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Correct Mismatched Erection Sequence Number and Name", "Edit erection sequence zone name to match current zone number.");
      if (flag1 & flag2)
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Clear Invalid Erection Sequence Zone Numbers and Correct Mismatched Erection Sequence Number and Name");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1004, "Continue");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 2)
        return (Result) 1;
      if (taskDialogResult == 1001)
      {
        using (Transaction transaction = new Transaction(document, "Clear Invalid Erection Sequence Zones"))
        {
          int num1 = (int) transaction.Start();
          this.ClearErectionSequenceParameters(elements1);
          int num2 = (int) transaction.Commit();
        }
      }
      else if (taskDialogResult == 1002)
      {
        using (Transaction transaction = new Transaction(document, "Correct Erection Sequence Zone Names"))
        {
          int num3 = (int) transaction.Start();
          if (this.RenameErectionSequenceZones(dictionary2, erectionSequenceZoneList))
          {
            int num4 = (int) transaction.Commit();
          }
          else
          {
            int num5 = (int) transaction.RollBack();
          }
        }
      }
      else if (taskDialogResult == 1003)
      {
        using (Transaction transaction = new Transaction(document, "Clear Invalid Erection Sequence Zones"))
        {
          int num6 = (int) transaction.Start();
          this.ClearErectionSequenceParameters(elements1);
          int num7 = (int) transaction.Commit();
        }
        using (Transaction transaction = new Transaction(document, "Correct Erection Sequence Zone Names"))
        {
          int num8 = (int) transaction.Start();
          if (this.RenameErectionSequenceZones(dictionary2, erectionSequenceZoneList))
          {
            int num9 = (int) transaction.Commit();
          }
          else
          {
            int num10 = (int) transaction.RollBack();
          }
        }
        this.workSharingFail = false;
      }
    }
    if (elementList1.Count > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Erection Sequence");
      taskDialog1.MainInstruction = "Erection Sequence Numbers Assigned Without Zone";
      taskDialog1.MainContent = "One or more pieces had an erection sequence assigned but did not belong to a valid zone. Please reset the ERECTION_SEQUENCE_NUMBER parameter to zero before attempting to apply a new value.";
      string parameterAsString1 = Parameters.GetParameterAsString(elementList1[0], "CONTROL_MARK");
      taskDialog1.ExpandedContent = !string.IsNullOrWhiteSpace(parameterAsString1) ? $"Pieces:\n{parameterAsString1} - {elementList1[0].Id?.ToString()}" : "Pieces:\n" + elementList1[0].Id?.ToString();
      for (int index = 1; index < elementList1.Count; ++index)
      {
        string parameterAsString2 = Parameters.GetParameterAsString(elementList1[index], "CONTROL_MARK");
        if (string.IsNullOrWhiteSpace(parameterAsString2))
        {
          TaskDialog taskDialog2 = taskDialog1;
          taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent},\n{elementList1[index].Id?.ToString()}";
        }
        else
        {
          TaskDialog taskDialog3 = taskDialog1;
          taskDialog3.ExpandedContent = $"{taskDialog3.ExpandedContent},\n{parameterAsString2} - {elementList1[index].Id?.ToString()}";
        }
      }
      taskDialog1.Show();
    }
    ErectionSequenceZone selectedZone = (ErectionSequenceZone) null;
    bool flag3 = false;
    while (!flag3)
    {
      if (selectedZone == null)
      {
        IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
        for (int index = 0; index < erectionSequenceZoneList.Count; ++index)
          erectionSequenceZoneList[index].ZoneCount = ErectionSequence_Command.GetZoneElementCount(document, erectionSequenceZoneList[index].ZoneIndex);
        ErectionSequenceZoneWindow sequenceZoneWindow = new ErectionSequenceZoneWindow(document, erectionSequenceZoneList, mainWindowHandle);
        sequenceZoneWindow.ShowDialog();
        if (sequenceZoneWindow.cancel)
          return (Result) 0;
        selectedZone = sequenceZoneWindow.SelectedZone;
        erectionSequenceZoneList = ErectionSequenceZoneExtensibleStorage.ReadErectionSequenceZones(document);
      }
      int num11 = 1;
      List<int> intList = new List<int>();
      if (parametersDictionary.Count > 0)
        parametersDictionary.Clear();
      foreach (Element elem2 in list1)
      {
        if (Parameters.GetParameterAsInt(elem2, this.ESZNumberParamName) == selectedZone.ZoneIndex)
        {
          Parameter parameter6 = Parameters.LookupParameter(elem2, this.ESParamName);
          if (parameter6 == null)
          {
            new TaskDialog("Erection Sequence")
            {
              MainInstruction = "Erection Sequence Number Parameter Missing",
              MainContent = "ERECTION_SEQUENCE_NUMBER parameter does not exist. Please run Project Shared Parameters and try again."
            }.Show();
            return (Result) 1;
          }
          if (parameter6.HasValue)
          {
            int key = parameter6.AsInteger();
            if (key > 0)
            {
              if (parametersDictionary.ContainsKey(key))
              {
                parametersDictionary[key].Add(parameter6);
                intList.Add(key);
              }
              else
                parametersDictionary.Add(key, new List<Parameter>()
                {
                  parameter6
                });
              if (elem2 is FamilyInstance inst)
              {
                foreach (Element elem3 in ErectionSequence_Command.GetWarpableListToEdit(inst))
                {
                  if (elem3.Id.IntegerValue != elem2.Id.IntegerValue)
                  {
                    Parameter parameter7 = Parameters.LookupParameter(elem3, this.ESParamName);
                    if (parameter7 != null)
                    {
                      if (parametersDictionary.ContainsKey(key))
                        parametersDictionary[key].Add(parameter7);
                      else
                        parametersDictionary.Add(key, new List<Parameter>()
                        {
                          parameter7
                        });
                    }
                  }
                }
              }
            }
          }
        }
      }
      if (intList.Count > 0)
      {
        intList.Sort();
        TaskDialog taskDialog4 = new TaskDialog("Erection Sequence");
        taskDialog4.MainInstruction = "Multiple pieces within the selected zone have been assigned the same erection sequence number. Would you like to continue? This may result in unexpected results.";
        taskDialog4.CommonButtons = (TaskDialogCommonButtons) 6;
        taskDialog4.ExpandedContent = "Multiple Erection Sequence Numbers:\n";
        foreach (int key in intList)
        {
          TaskDialog taskDialog5 = taskDialog4;
          taskDialog5.ExpandedContent = $"{taskDialog5.ExpandedContent}{key.ToString()} - ";
          int num12 = 1;
          foreach (Parameter parameter8 in parametersDictionary[key])
          {
            if (parameter8.Element is FamilyInstance element && !element.HasSuperComponent())
            {
              taskDialog4.ExpandedContent += element.Id.ToString();
              if (num12 != parametersDictionary[key].Count)
                taskDialog4.ExpandedContent += ", ";
              ++num12;
            }
          }
          taskDialog4.ExpandedContent += "\n";
        }
        if (taskDialog4.Show() != 6)
        {
          selectedZone = (ErectionSequenceZone) null;
          continue;
        }
      }
      if (parametersDictionary.Count > 0)
      {
        List<int> list2 = parametersDictionary.Keys.ToList<int>();
        list2.Sort();
        bool flag4 = false;
        int num13 = 1;
        if (list2[0] == 1)
        {
          foreach (int num14 in list2)
          {
            if (num14 != num13)
            {
              flag4 = true;
              break;
            }
            ++num13;
          }
        }
        else
          flag4 = true;
        TaskDialog taskDialog6 = new TaskDialog("Erection Sequence");
        taskDialog6.MainInstruction = "Some pieces within the selected zone have already been assigned an erection sequence number. How would you like to start assigning new values?";
        taskDialog6.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog6.AddCommandLink((TaskDialogCommandLinkId) 1001, "Reset Erection Sequence Values", "Reassign all erection sequence numbers on pieces within the selected zone and start assigning pieces at 1.");
        TaskDialog taskDialog7 = taskDialog6;
        int num15 = list2.Last<int>() + 1;
        string str3 = $"Will assign new erection sequence numbers starting with {num15.ToString()}.";
        taskDialog7.AddCommandLink((TaskDialogCommandLinkId) 1002, "Start at Next Available Value", str3);
        if (flag4)
        {
          TaskDialog taskDialog8 = taskDialog6;
          num15 = parametersDictionary.Count + 1;
          string str4 = $"Resequence existing values on pieces within the selected zone to remove gaps in numbering and start assigning new values at {num15.ToString()}.";
          taskDialog8.AddCommandLink((TaskDialogCommandLinkId) 1003, "Resequence Existing Values", str4);
        }
        TaskDialogResult taskDialogResult = taskDialog6.Show();
        if (taskDialogResult == 1001)
        {
          using (Transaction transaction = new Transaction(document, "Reset Erection Sequence Numbers"))
          {
            int num16 = (int) transaction.Start();
            if (this.revit_Doc.IsWorkshared && !this.CheckZoneSequences(parametersDictionary))
            {
              TaskDialog.Show("Erection Sequence Update", "The elements necessary for updating the Erection Sequence Zones are not currently editable. Please coordinate with project members to allow for ownership of the elements and then reload the latest from central.");
              int num17 = (int) transaction.RollBack();
              selectedZone = (ErectionSequenceZone) null;
              continue;
            }
            foreach (int key in list2)
            {
              foreach (Parameter parameter9 in parametersDictionary[key])
                parameter9.Set(0);
            }
            int num18 = (int) transaction.Commit();
          }
        }
        else if (taskDialogResult == 1002)
          num11 = list2.Last<int>() + 1;
        else if (taskDialogResult == 1003)
        {
          int num19 = 1;
          using (Transaction transaction = new Transaction(document, "Resequence Erection Sequence Numbers"))
          {
            int num20 = (int) transaction.Start();
            if (this.revit_Doc.IsWorkshared && !this.CheckZoneSequences(parametersDictionary))
            {
              TaskDialog.Show("Erection Sequence Update", "The elements necessary for updating the Erection Sequence Zones are not currently editable. Please coordinate with project members to allow for ownership of the elements.");
              int num21 = (int) transaction.RollBack();
              selectedZone = (ErectionSequenceZone) null;
              continue;
            }
            foreach (int key in list2)
            {
              if (key != num19)
              {
                foreach (Parameter parameter10 in parametersDictionary[key])
                  parameter10.Set(num19);
              }
              ++num19;
            }
            int num22 = (int) transaction.Commit();
          }
          num11 = num19;
        }
        else
        {
          selectedZone = (ErectionSequenceZone) null;
          continue;
        }
      }
      bool flag5 = false;
      while (!flag5)
      {
        try
        {
          Reference reference = activeUiDocument.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new ErectionSequenceFilter(selectedZone.ZoneIndex), $"Pick piece to assign an erection sequence number of {num11.ToString()} for the {selectedZone.ZoneName} zone.");
          if (reference != null)
          {
            Element element = document.GetElement(reference);
            if (element != null)
            {
              using (Transaction transaction = new Transaction(document, $"Assign Erection Sequence - Zone: {selectedZone.ZoneName} Number: {num11.ToString()}"))
              {
                List<Element> elementList2 = new List<Element>();
                if (element is FamilyInstance inst)
                {
                  List<Element> warpableListToEdit = ErectionSequence_Command.GetWarpableListToEdit(inst);
                  int num23 = (int) transaction.Start();
                  if (this.revit_Doc.IsWorkshared && !ErectionSequence_Command.checkElementOwnershipCondition((ICollection<ElementId>) warpableListToEdit.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>(), this.revit_Doc))
                  {
                    int num24 = (int) transaction.RollBack();
                  }
                  else
                  {
                    foreach (Element elem4 in warpableListToEdit)
                    {
                      Parameter parameter11 = Parameters.LookupParameter(elem4, this.ESParamName);
                      if (parameter11 != null)
                      {
                        Parameter parameter12 = Parameters.LookupParameter(elem4, this.ESZNumberParamName);
                        if (parameter12 != null)
                        {
                          Parameter parameter13 = Parameters.LookupParameter(elem4, this.ESZNameParamName);
                          if (parameter13 != null)
                          {
                            parameter11.Set(num11);
                            parameter12.Set(selectedZone.ZoneIndex);
                            parameter13.Set(selectedZone.ZoneName);
                          }
                          else
                            break;
                        }
                        else
                          break;
                      }
                      else
                        break;
                    }
                    int num25 = (int) transaction.Commit();
                    ++num11;
                  }
                }
              }
            }
          }
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          TaskDialog taskDialog = new TaskDialog("Erection Sequence");
          taskDialog.MainInstruction = "Continue Assigning Erection Sequence?";
          taskDialog.MainContent = "Would you like to continue assigning erection sequence parameters?";
          if (selectedZone.ZoneIndex < erectionSequenceZoneList.Count)
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue Using Next Zone", "Return to assigning erection sequence parameters using the next zone.");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Select New Zone", "Return to the zone selection window to select a new zone.");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Continue with Current Zone", "Return to assigning erection sequence parameters using the currently selected zone.");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1004, "Finished", "Close Erection Sequence tool.");
          TaskDialogResult taskDialogResult = taskDialog.Show();
          bool flag6;
          if (taskDialogResult == 1001)
          {
            selectedZone = erectionSequenceZoneList.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.ZoneIndex == selectedZone.ZoneIndex + 1)).FirstOrDefault<ErectionSequenceZone>();
            flag6 = true;
            break;
          }
          if (taskDialogResult == 1002)
          {
            flag6 = true;
            selectedZone = (ErectionSequenceZone) null;
            break;
          }
          if (taskDialogResult != 1003)
          {
            flag6 = true;
            flag3 = true;
            break;
          }
        }
        catch (Exception ex)
        {
          new TaskDialog("Erection Sequence")
          {
            MainInstruction = "Error Occured Setting Erection Sequence Parameters."
          }.Show();
          return (Result) 0;
        }
      }
    }
    return (Result) 0;
  }

  public static ICollection<Element> returnUnownedElems(List<ElementId> listToTest, Document doc)
  {
    return (ICollection<Element>) WorksharingUtils.CheckoutElements(doc, (ICollection<ElementId>) listToTest).Select<ElementId, Element>((Func<ElementId, Element>) (e => e.GetElement())).ToList<Element>();
  }

  private bool CheckZoneSequences(
    Dictionary<int, List<Parameter>> parametersDictionary)
  {
    List<Element> source = new List<Element>();
    List<List<Parameter>> list = parametersDictionary.Values.ToList<List<Parameter>>();
    for (int index = 0; index < list.Count; ++index)
      source.AddRange(list[index].Select<Parameter, Element>((Func<Parameter, Element>) (p => p.Element)));
    return ErectionSequence_Command.checkElementOwnershipCondition((ICollection<ElementId>) source.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>(), this.revit_Doc, true);
  }

  public static bool checkElementOwnershipCondition(
    ICollection<ElementId> listToTest,
    Document doc,
    bool hideWarnings = false)
  {
    ICollection<ElementId> elementIds1 = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> elementIds2 = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> list = (ICollection<ElementId>) ErectionSequence_Command.returnUnownedElems(listToTest.ToList<ElementId>(), doc).Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
    foreach (ElementId elementId in (IEnumerable<ElementId>) listToTest)
    {
      if (!list.Contains(elementId))
      {
        elementIds1.Add(elementId);
      }
      else
      {
        switch (WorksharingUtils.GetModelUpdatesStatus(doc, elementId))
        {
          case ModelUpdatesStatus.DeletedInCentral:
          case ModelUpdatesStatus.UpdatedInCentral:
            elementIds2.Add(elementId);
            continue;
          default:
            if (WorksharingUtils.GetCheckoutStatus(doc, elementId) != CheckoutStatus.OwnedByCurrentUser)
            {
              elementIds1.Add(elementId);
              continue;
            }
            continue;
        }
      }
    }
    if (elementIds1.Count != 0)
    {
      if (!hideWarnings)
        new TaskDialog("Erection Sequence Update")
        {
          MainInstruction = "The elements necessary for updating the Erection Sequence Zones are not currently editable. Please coordinate with project members to allow for ownership of the elements, and then reload the latest from central."
        }.Show();
      return false;
    }
    if (elementIds2.Count == 0)
      return true;
    if (!hideWarnings)
      new TaskDialog("Erection Sequence Update")
      {
        MainInstruction = "Elements have been updated in the central model! Process will be canceled. Please try reloading the latest version."
      }.Show();
    return false;
  }

  public static int GetZoneElementCount(Document revitDoc, int zoneIndex)
  {
    int zoneElementCount = 0;
    List<Element> list = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).Where<Element>((Func<Element, bool>) (e => Parameters.GetParameterAsInt(e, "ERECTION_SEQUENCE_ZONE_NUMBER") == zoneIndex && !e.HasSuperComponent())).ToList<Element>();
    if (list.Count > 0)
      zoneElementCount = list.Count;
    return zoneElementCount;
  }

  private bool CheckFilter_Template(
    Document revitDoc,
    UIDocument uiDoc,
    Dictionary<string, bool> itemList)
  {
    bool flag = true;
    string empty = string.Empty;
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    List<string> stringList3 = new List<string>();
    List<string> stringList4 = new List<string>();
    List<View> list = new FilteredElementCollector(revitDoc).WhereElementIsNotElementType().OfClass(typeof (View)).Cast<View>().Where<View>((Func<View, bool>) (v => v.IsTemplate)).ToList<View>();
    FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfClass(typeof (ParameterFilterElement));
    foreach (KeyValuePair<string, bool> keyValuePair in itemList)
    {
      KeyValuePair<string, bool> pair = keyValuePair;
      Autodesk.Revit.DB.FilterElement filterElement = (Autodesk.Revit.DB.FilterElement) null;
      if (pair.Value)
      {
        if (elementCollector != null)
        {
          foreach (ParameterFilterElement parameterFilterElement in elementCollector)
          {
            if (parameterFilterElement.Name.Equals(pair.Key))
            {
              filterElement = (Autodesk.Revit.DB.FilterElement) parameterFilterElement;
              break;
            }
          }
          if (filterElement == null)
            stringList1.Add(pair.Key);
          else if (!uiDoc.ActiveView.IsFilterApplied(filterElement.Id) && pair.Key != "ERECTION_SEQUENCE_UNASSIGNED")
            stringList2.Add(pair.Key);
        }
      }
      else if (list.Where<View>((Func<View, bool>) (v => v.Name.Equals(pair.Key))).Count<View>() == 0)
        stringList3.Add(pair.Key);
      else if (pair.Key != "ERECTION SEQUENCE ASSIGNED")
      {
        list.FirstOrDefault<View>((Func<View, bool>) (v => v.Name.Equals(pair.Key)));
        Element element = uiDoc.ActiveView.ViewTemplateId.GetElement();
        if (element != null)
        {
          if (!element.Name.Equals(pair.Key))
            stringList4.Add(pair.Key);
        }
        else
          stringList4.Add(pair.Key);
      }
    }
    TaskDialog taskDialog = new TaskDialog("Erection Sequence Tool");
    taskDialog.MainInstruction = "Filter - Template Information";
    if (stringList1.Count + stringList3.Count > 0)
    {
      string str1 = "Unable to find the view template or filter.  The following are not found in the current project:\n";
      foreach (string str2 in stringList1)
        str1 = $"{str1}\n{str2} - Filter";
      foreach (string str3 in stringList3)
        str1 = $"{str1}\n{str3} - Template";
      string str4 = str1 + "\n\nFor additional information regarding view templates and filters, please consult the help file.";
      taskDialog.MainContent = str4;
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue", "Resume assigning the erection sequence with unknown view graphics.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel", "Abort the erection sequence operation.");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult != 1001)
        return taskDialogResult == 1002 && false;
      flag = false;
    }
    if (stringList2.Count + stringList4.Count <= 0)
      return true;
    string str5 = "View template or filter not applied.  The following are not applied to the active view:\n";
    foreach (string str6 in stringList2)
      str5 = $"{str5}\n{str6} - Filter";
    foreach (string str7 in stringList4)
      str5 = $"{str5}\n{str7} - Template";
    string str8 = str5 + "\n\nFor additional information regarding view templates and filters, please consult the help file.";
    taskDialog.MainContent = str8;
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue", "Resume assigning the erection sequence with unknown view graphics.");
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel", "Abort the erection sequence operation.");
    TaskDialogResult taskDialogResult1 = taskDialog.Show();
    if (taskDialogResult1 != 1001)
      return taskDialogResult1 == 1002 && false;
    int num = false ? 1 : 0;
    return true;
  }

  private void ClearErectionSequenceParameters(Element element)
  {
    List<Element> elementList = new List<Element>();
    if (element is FamilyInstance inst)
      elementList = ErectionSequence_Command.GetWarpableListToEdit(inst);
    foreach (Element elem in elementList)
    {
      Parameters.LookupParameter(elem, this.ESParamName)?.Set(0);
      Parameters.LookupParameter(elem, this.ESZNameParamName)?.Set("");
      Parameters.LookupParameter(elem, this.ESZNumberParamName)?.Set(0);
    }
  }

  private void ClearErectionSequenceParameters(List<Element> elements)
  {
    List<Element> collection1 = new List<Element>();
    foreach (Element element in elements)
    {
      List<Element> collection2 = new List<Element>();
      if (element is FamilyInstance inst)
        collection2 = ErectionSequence_Command.GetWarpableListToEdit(inst);
      collection1.AddRange((IEnumerable<Element>) collection2);
    }
    List<Element> source = new List<Element>();
    source.AddRange((IEnumerable<Element>) elements);
    source.AddRange((IEnumerable<Element>) collection1);
    List<Element> list = source.Distinct<Element>().ToList<Element>();
    if (this.revit_Doc.IsWorkshared && !ErectionSequence_Command.checkElementOwnershipCondition((ICollection<ElementId>) list.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).Distinct<ElementId>().ToList<ElementId>(), this.revit_Doc))
    {
      this.workSharingFail = true;
    }
    else
    {
      foreach (Element element in list)
        this.ClearErectionSequenceParameters(element);
    }
  }

  private bool RenameErectionSequenceZones(
    Dictionary<Element, int> dictionary,
    List<ErectionSequenceZone> zones)
  {
    Dictionary<ErectionSequenceZone, List<Element>> dictionary1 = new Dictionary<ErectionSequenceZone, List<Element>>();
    Document document = (Document) null;
    foreach (Element key1 in dictionary.Keys)
    {
      if (document == null)
        document = key1.Document;
      int zoneNumber = dictionary[key1];
      ErectionSequenceZone key2;
      if (zoneNumber <= 0)
      {
        key2 = new ErectionSequenceZone();
        key2.ZoneName = string.Empty;
        key2.ZoneIndex = 0;
      }
      else
        key2 = zones.Where<ErectionSequenceZone>((Func<ErectionSequenceZone, bool>) (x => x.ZoneIndex == zoneNumber)).FirstOrDefault<ErectionSequenceZone>();
      if (key2 != null)
      {
        List<Element> collection = new List<Element>();
        if (key1 is FamilyInstance inst)
          collection = ErectionSequence_Command.GetWarpableListToEdit(inst);
        if (dictionary1.ContainsKey(key2))
          dictionary1[key2].AddRange((IEnumerable<Element>) collection);
        else
          dictionary1.Add(key2, collection);
      }
    }
    if (document.IsWorkshared)
    {
      List<Element> source = new List<Element>();
      foreach (List<Element> collection in dictionary1.Values)
        source.AddRange((IEnumerable<Element>) collection);
      if (!ErectionSequence_Command.checkElementOwnershipCondition((ICollection<ElementId>) source.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>(), this.revit_Doc, this.workSharingFail))
        return false;
    }
    foreach (ErectionSequenceZone key in dictionary1.Keys)
    {
      foreach (Element elem in dictionary1[key])
      {
        Parameters.LookupParameter(elem, this.ESZNumberParamName)?.Set(key.ZoneIndex);
        Parameters.LookupParameter(elem, this.ESZNameParamName)?.Set(key.ZoneName);
      }
    }
    return true;
  }

  public static List<Element> GetWarpableListToEdit(FamilyInstance inst)
  {
    List<Element> warpableListToEdit = new List<Element>();
    Element elem = (Element) inst;
    if (inst.HasSuperComponent())
      elem = inst.GetSuperComponent();
    warpableListToEdit.Add(elem);
    foreach (ElementId id in elem.GetSubComponentIds().ToList<ElementId>())
    {
      Element element = inst.Document.GetElement(id);
      if (element != null && element.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
        warpableListToEdit.Add(element);
    }
    return warpableListToEdit;
  }
}
