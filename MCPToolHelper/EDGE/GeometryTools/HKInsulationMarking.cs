// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.HKInsulationMarking
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.AssemblyTools.MarkVerification.GeometryComparer;
using EDGE.AssemblyTools.MarkVerification.InitialPresentation;
using EDGE.AssemblyTools.MarkVerification.Tools;
using EDGE.InsulationTools.InsulationMarking.UtilityFunction;
using EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.AssemblyUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class HKInsulationMarking : IExternalCommand
{
  private List<string> marksList = new List<string>()
  {
    "A",
    "B",
    "C",
    "D",
    "E",
    "F",
    "G",
    "H",
    "J",
    "K",
    "L",
    "M",
    "N",
    "O",
    "P",
    "Q",
    "R",
    "S",
    "T",
    "U",
    "V",
    "W",
    "X",
    "Y",
    "Z"
  };

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Insulation Marking")
      {
        MainInstruction = "Insulation Marking Tool must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Insulation Marking must be run in the project environment. Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    using (Transaction transaction = new Transaction(document, "Insulation Marking Per Wall"))
    {
      try
      {
        Autodesk.Revit.UI.Selection.Selection selection = activeUiDocument.Selection;
        List<Element> elementList1 = this.WallFamilyInstances(document, selection);
        if (elementList1 == null || elementList1.Count == 0)
          return (Result) 1;
        List<Element> elementList2 = new List<Element>();
        MKTolerances mkTolerances = new MKTolerances(document);
        int num1 = (int) transaction.Start();
        EDGE.GeometryTools.InsulationMarking._MVData = new MarkVerificationData(document, ComparisonOption.DoNotRound, true);
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = true;
        List<Element> zerovols1 = new List<Element>();
        IEnumerable<Element> source1 = new FilteredElementCollector(document).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (x => !Utils.ElementUtils.Parameters.GetParameterAsBool(x, "HARDWARE_DETAIL")));
        List<Element> elementList3 = Marking.filterforInsulatMarking(source1.Where<Element>((Func<Element, bool>) (elem => string.IsNullOrWhiteSpace(Utils.ElementUtils.Parameters.GetParameterAsString(elem, "HOST_GUID")))).ToList<Element>(), out zerovols1);
        if (elementList3.Count > 0)
        {
          Dictionary<string, List<ElementId>> dictionary = new Dictionary<string, List<ElementId>>();
          foreach (Element element in elementList3)
          {
            FamilyInstance familyInstance = element as FamilyInstance;
            string key = $"{familyInstance.Symbol.Family.Name} - {familyInstance.Name}";
            if (familyInstance.Symbol.Family.Name == familyInstance.Name)
              key = familyInstance.Name;
            if (dictionary.ContainsKey(key))
            {
              dictionary[key].Add(familyInstance.Id);
            }
            else
            {
              dictionary.Add(key, new List<ElementId>());
              dictionary[key].Add(familyInstance.Id);
            }
          }
          TaskDialog taskDialog1 = new TaskDialog("Insulation Marking");
          taskDialog1.MainContent = "One or more pieces of insulation in the model are not hosted correctly. These pieces will not be marked. Run BOM Product Hosting and try again.";
          foreach (KeyValuePair<string, List<ElementId>> keyValuePair in dictionary)
          {
            TaskDialog taskDialog2 = taskDialog1;
            taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{keyValuePair.Key}: ";
            bool flag4 = true;
            foreach (ElementId elementId in keyValuePair.Value)
            {
              if (flag4)
              {
                taskDialog1.ExpandedContent += elementId.ToString();
                flag4 = false;
              }
              else
              {
                TaskDialog taskDialog3 = taskDialog1;
                taskDialog3.ExpandedContent = $"{taskDialog3.ExpandedContent}, {elementId.ToString()}";
              }
            }
            taskDialog1.ExpandedContent += "\n";
          }
          taskDialog1.Show();
        }
        if (Marking.filterforInsulatMarking(source1.Where<Element>((Func<Element, bool>) (elem => Utils.ElementUtils.Parameters.LookupParameter(elem, "SUFFIX_INSULATION_MARKING") == null)).ToList<Element>(), out zerovols1).Count > 0)
        {
          TaskDialog.Show("Insulation Marking", "Insulation Marking failed. One or more insulation families within the model lack the SUFFIX_INSULATION_MARKING parameter. Please add this to the family and try again.");
          int num2 = (int) transaction.RollBack();
          return (Result) 1;
        }
        List<Element> elementList4 = Marking.filterforInsulatMarking(source1.Where<Element>((Func<Element, bool>) (elem => string.IsNullOrWhiteSpace(Utils.ElementUtils.Parameters.GetParameterAsString(elem, "SUFFIX_INSULATION_MARKING")))).ToList<Element>(), out zerovols1);
        double num3 = -1.0;
        double num4 = -1.0;
        double num5 = -1.0;
        ElementId elementId1 = ElementId.InvalidElementId;
        foreach (Element elem in elementList4)
        {
          bool flag5 = false;
          double num6 = Math.Round(Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_THICKNESS"), 6);
          double num7 = Math.Round(Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_WIDTH_MAX"), 6);
          double num8 = Math.Round(Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_LENGTH_MAX"), 6);
          ElementId typeId = elem.GetTypeId();
          if (num3 == -1.0)
            num3 = num6;
          else if (num3 != num6)
            flag5 = true;
          if (num4 == -1.0)
            num4 = num7;
          else if (num4 != num7)
            flag5 = true;
          if (num5 == -1.0)
            num5 = num8;
          else if (num5 != num8)
            flag5 = true;
          if (elementId1 == ElementId.InvalidElementId)
            elementId1 = typeId;
          else if (elementId1 != typeId)
            flag5 = true;
          if (flag5)
          {
            new TaskDialog("Insulation Marking")
            {
              MainContent = "Insulation Marking failed. Differences were found in the standard insulation configuration. Please ensure that all standard insulation is of the same family type and has the same values assigned for the following parameters: DIM_THICKNESS, DIM_MAX_LENGTH, and DIM_MAX_WIDTH."
            }.Show();
            int num9 = (int) transaction.RollBack();
            return (Result) -1;
          }
        }
        List<string> wallPanelGuids = new List<string>();
        elementList1.ForEach((Action<Element>) (elem => wallPanelGuids.Add(elem.UniqueId.ToString())));
        List<Element> zerovols2 = new List<Element>();
        Marking.filterforInsulatMarking(source1.Where<Element>((Func<Element, bool>) (elem => wallPanelGuids.Contains(Utils.ElementUtils.Parameters.GetParameterAsString(elem, "HOST_GUID")))).ToList<Element>(), out zerovols2);
        if (zerovols2.Count > 0)
        {
          StringBuilder stringBuilder = new StringBuilder("The following IDs had zero or negative volume\n");
          foreach (Element element in zerovols2)
            stringBuilder.AppendLine(element.Id.ToString());
          if (new TaskDialog("Warning")
          {
            MainInstruction = "One or more insulation elements had zero volume and will not be marked. If you choose to continue, they will have any existing insulation mark deleted. Continue?",
            ExpandedContent = stringBuilder.ToString(),
            CommonButtons = ((TaskDialogCommonButtons) 9)
          }.Show() != 1)
          {
            int num10 = (int) transaction.RollBack();
            return (Result) 1;
          }
          foreach (Element element in zerovols2)
          {
            Parameter parameter = element.LookupParameter("INSULATION_MARK");
            if (parameter != null)
            {
              parameter.Set("");
              flag3 = false;
            }
            (element.LookupParameter("INSULATION_LOCK") ?? element.LookupParameter("LOCKED_CHECKBOX"))?.Set(0);
          }
        }
        foreach (Element element in elementList1)
        {
          EDGE.GeometryTools.InsulationMarking._GeomVerificationStore.Clear();
          string wallGUID = element.UniqueId;
          Dictionary<string, List<FamilyInstance>> dictionary1 = new Dictionary<string, List<FamilyInstance>>();
          List<Element> zerovols3 = new List<Element>();
          List<Element> elementList5 = Marking.filterforInsulatMarking(source1.Where<Element>((Func<Element, bool>) (elem => Utils.ElementUtils.Parameters.GetParameterAsString(elem, "HOST_GUID") == wallGUID)).ToList<Element>(), out zerovols3);
          List<FamilyInstance> source2 = new List<FamilyInstance>();
          foreach (Element elem in elementList5)
          {
            if ((elem.Category.Id.IntegerValue.Equals(-2001350) || elem.Category.Id.IntegerValue.Equals(-2000151)) && Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").ToUpper().ToUpper().Contains("INSULATION") && elem is FamilyInstance)
            {
              FamilyInstance familyInstance = elem as FamilyInstance;
              if (string.IsNullOrWhiteSpace(Utils.ElementUtils.Parameters.GetParameterAsString((Element) familyInstance, "SUFFIX_INSULATION_MARKING")))
              {
                double num11 = Math.Round(Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) familyInstance, "DIM_THICKNESS"), 6);
                double num12 = Math.Round(Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) familyInstance, "DIM_WIDTH_MAX"), 6);
                double val1 = Math.Round(Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) familyInstance, "DIM_LENGTH_MAX"), 6) * num12 * num11;
                double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) familyInstance, "Volume");
                if (mkTolerances.NewValuesAreEqual(val1, parameterAsDouble, MKToleranceAspect.Insulation_Volume))
                {
                  if (EDGE.GeometryTools.InsulationMarking.isLocked(familyInstance))
                  {
                    if (Utils.ElementUtils.Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_MARK") != "A")
                    {
                      if (!flag2)
                      {
                        if (TaskDialog.Show("Insulation Marking", "One or more elements were \"locked\" but should be marked A. Insulation Marking will treat these elements as unlocked. Proceed with insulation marking?", (TaskDialogCommonButtons) 9) != 1)
                        {
                          int num13 = (int) transaction.RollBack();
                          return (Result) 1;
                        }
                        flag2 = true;
                      }
                      if (!Utils.ElementUtils.Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_HOST_GUID").Equals(familyInstance.UniqueId) && !flag1)
                      {
                        if (TaskDialog.Show("Insulation Marking", "One or more elements were \"locked\" but had mismatched INSULATION_HOST_GUIDs. Insulation Marking will treat these elements as unlocked. Proceed with insulation marking?", (TaskDialogCommonButtons) 9) != 1)
                        {
                          int num14 = (int) transaction.RollBack();
                          return (Result) 1;
                        }
                        flag1 = true;
                      }
                      Parameter parameter1 = familyInstance.LookupParameter("INSULATION_MARK");
                      Parameter parameter2 = familyInstance.LookupParameter("INSULATION_LOCK");
                      Parameter parameter3 = familyInstance.LookupParameter("INSULATION_HOST_GUID");
                      parameter2.Set(1);
                      string uniqueId = familyInstance.UniqueId;
                      parameter3.Set(uniqueId);
                      if (parameter1 != null)
                      {
                        parameter1.Set("A");
                        flag3 = false;
                      }
                    }
                    else
                      continue;
                  }
                  string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_HOST_GUID");
                  if (!parameterAsString.Equals(familyInstance.UniqueId) && !string.IsNullOrWhiteSpace(parameterAsString) && !flag1)
                  {
                    if (TaskDialog.Show("Insulation Marking", "One or more elements were \"locked\" but had mismatched INSULATION_HOST_GUIDs. Insulation Marking will treat these elements as unlocked. Proceed with insulation marking?", (TaskDialogCommonButtons) 9) != 1)
                    {
                      int num15 = (int) transaction.RollBack();
                      return (Result) 1;
                    }
                    flag1 = true;
                  }
                  Parameter parameter = familyInstance.LookupParameter("INSULATION_MARK");
                  familyInstance.LookupParameter("INSULATION_LOCK").Set(1);
                  familyInstance.LookupParameter("INSULATION_HOST_GUID").Set(familyInstance.UniqueId);
                  if (parameter != null)
                  {
                    parameter.Set("A");
                    flag3 = false;
                  }
                }
                else
                {
                  if (EDGE.GeometryTools.InsulationMarking.isLocked(familyInstance) && Utils.ElementUtils.Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_MARK") == "A")
                  {
                    new TaskDialog("Geometry Mismatch")
                    {
                      MainInstruction = "Insulation marking failed. The geometry for one or more insulation elements of a control mark does not match the rest of the mark's elements.\nInsulation marks: \nA \n"
                    }.Show();
                    int num16 = (int) transaction.RollBack();
                    return (Result) 1;
                  }
                  source2.Add(familyInstance);
                }
              }
              else
                source2.Add(familyInstance);
            }
          }
          if (source2.Count<FamilyInstance>() == 0)
          {
            elementList2.Add(element);
          }
          else
          {
            if (document.IsWorkshared && !CheckElementsOwnership.CheckOwnership("Insulation Marking Per Wall", ((IEnumerable<Element>) source2).Select<Element, ElementId>((Func<Element, ElementId>) (s => s.Id)).Distinct<ElementId>().ToList<ElementId>(), document, activeUiDocument, out List<ElementId> _, false))
            {
              new TaskDialog("Insulation Marking")
              {
                MainContent = "One or more pieces of insulation are not owned by another user."
              }.Show();
              int num17 = (int) transaction.RollBack();
              return (Result) -1;
            }
            bool flag6 = false;
            foreach (FamilyInstance familyInstance in source2)
            {
              Parameter parameter4 = familyInstance.LookupParameter("INSULATION_MARK");
              if (parameter4 == null)
              {
                new TaskDialog("Warning")
                {
                  MainInstruction = "The INSULATION_MARK parameter either does not exist or is a type parameter for one or more insulation elements in the project."
                }.Show();
                int num18 = (int) transaction.RollBack();
                return (Result) 1;
              }
              if (parameter4.IsReadOnly)
              {
                new TaskDialog("Warning")
                {
                  MainInstruction = "The INSULATION_MARK parameter is read-only for one or more insulation elements in the project."
                }.Show();
                int num19 = (int) transaction.RollBack();
                return (Result) 1;
              }
              Parameter parameter5 = familyInstance.LookupParameter("INSULATION_HOST_GUID");
              if (parameter5 == null)
              {
                new TaskDialog("Warning")
                {
                  MainInstruction = "The INSULATION_HOST_GUID parameter either does not exist or is a type parameter for one or more insulation elements in the project."
                }.Show();
                int num20 = (int) transaction.RollBack();
                return (Result) 1;
              }
              if (parameter5.IsReadOnly)
              {
                new TaskDialog("Warning")
                {
                  MainInstruction = "The INSULATION_HOST_GUID parameter is read-only for one or more insulation elements in the project."
                }.Show();
                int num21 = (int) transaction.RollBack();
                return (Result) 1;
              }
              Parameter parameter6 = familyInstance.LookupParameter("INSULATION_LOCK");
              if (parameter6 == null)
              {
                new TaskDialog("Warning")
                {
                  MainInstruction = "The INSULATION_LOCK parameter either does not exist or is a type parameter for one or more insulation elements in the project."
                }.Show();
                int num22 = (int) transaction.RollBack();
                return (Result) 1;
              }
              if (parameter6.IsReadOnly)
              {
                new TaskDialog("Warning")
                {
                  MainInstruction = "The INSULATION_LOCK parameter is read-only for one or more insulation elements in the project."
                }.Show();
                int num23 = (int) transaction.RollBack();
                return (Result) 1;
              }
            }
            Dictionary<string, List<FamilyInstance>> dictionary2 = new Dictionary<string, List<FamilyInstance>>();
            foreach (FamilyInstance elem in source2)
            {
              string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) elem, "SUFFIX_INSULATION_MARKING");
              if (dictionary2.ContainsKey(parameterAsString))
              {
                dictionary2[parameterAsString].Add(elem);
              }
              else
              {
                List<FamilyInstance> familyInstanceList = new List<FamilyInstance>()
                {
                  elem
                };
                dictionary2.Add(parameterAsString, familyInstanceList);
              }
            }
            foreach (KeyValuePair<string, List<FamilyInstance>> keyValuePair1 in dictionary2)
            {
              Dictionary<FamilyInstance, XYZ> source3 = new Dictionary<FamilyInstance, XYZ>();
              List<XYZ> source4 = new List<XYZ>();
              foreach (FamilyInstance familyInstance in keyValuePair1.Value)
              {
                XYZ xyz = this.LeftMostProjectedPoint(element, (Element) familyInstance, document);
                source3.Add(familyInstance, xyz);
                source4.Add(xyz);
              }
              bool workPlaneFlipped = (element as FamilyInstance).IsWorkPlaneFlipped;
              List<XYZ> xyzList = new List<XYZ>();
              if (source4.Count<XYZ>() > 0)
                xyzList = workPlaneFlipped || PlacementUtilities.IsMirrored(element) ? source4.OrderByDescending<XYZ, double>((Func<XYZ, double>) (e => PlacementUtilities.RoundValue(e.Z))).ThenBy<XYZ, double>((Func<XYZ, double>) (p => PlacementUtilities.RoundValue(p.X))).ToList<XYZ>() : source4.OrderByDescending<XYZ, double>((Func<XYZ, double>) (e => PlacementUtilities.RoundValue(e.Z))).ThenByDescending<XYZ, double>((Func<XYZ, double>) (p => PlacementUtilities.RoundValue(p.X))).ToList<XYZ>();
              List<FamilyInstance> SFElementsToProcess = new List<FamilyInstance>();
              foreach (XYZ xyz in xyzList)
              {
                XYZ point = xyz;
                SFElementsToProcess.Add(source3.FirstOrDefault<KeyValuePair<FamilyInstance, XYZ>>((Func<KeyValuePair<FamilyInstance, XYZ>, bool>) (x => x.Value == point)).Key);
              }
              List<string> availableMarks = new List<string>();
              Dictionary<string, MarkGroupResultData> dictionary3 = new Dictionary<string, MarkGroupResultData>();
              foreach (FamilyInstance elem in SFElementsToProcess)
              {
                if (Utils.ElementUtils.Parameters.GetParameterAsBool((Element) elem, "INSULATION_LOCK"))
                {
                  if (Utils.ElementUtils.Parameters.GetParameterAsString((Element) elem, "INSULATION_HOST_GUID").Equals(elem.UniqueId))
                  {
                    string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) elem, "INSULATION_MARK");
                    if (!dictionary1.ContainsKey(parameterAsString))
                      dictionary1.Add(parameterAsString, new List<FamilyInstance>()
                      {
                        elem
                      });
                    else
                      dictionary1[parameterAsString].Add(elem);
                  }
                  else
                    flag6 = true;
                }
              }
              if (flag6 && !flag1)
              {
                if (TaskDialog.Show("Insulation Marking", "One or more elements were \"locked\" but had mismatched INSULATION_HOST_GUIDs. Insulation Marking will treat these elements as unlocked. Proceed with insulation marking?", (TaskDialogCommonButtons) 9) != 1)
                {
                  int num24 = (int) transaction.RollBack();
                  return (Result) 1;
                }
                flag1 = true;
              }
              List<FamilyInstance> familyInstanceList1 = new List<FamilyInstance>();
              List<string> stringList = new List<string>();
              if (dictionary1.Count > 0)
              {
                foreach (KeyValuePair<string, List<FamilyInstance>> keyValuePair2 in dictionary1)
                {
                  List<FamilyInstance> familyInstanceList2 = keyValuePair2.Value;
                  if (EDGE.GeometryTools.InsulationMarking.RunInitialComparison(activeUiDocument, document, (IEnumerable<FamilyInstance>) familyInstanceList2, availableMarks, 0, new Dictionary<string, MarkGroupResultData>(), false).Keys.Count > 1)
                    stringList.Add(keyValuePair2.Key.ToString());
                  else
                    dictionary3.Add(keyValuePair2.Key, new MarkGroupResultData()
                    {
                      GroupMembers = familyInstanceList2.ToList<FamilyInstance>()
                    });
                }
              }
              if (stringList.Count > 0)
              {
                string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element, "CONTROL_MARK");
                string str1 = element.Id.ToString();
                stringList.Sort();
                string str2 = string.Join(", ", stringList.ToArray());
                new TaskDialog("Geometry Mismatch")
                {
                  MainInstruction = $"Insulation marking failed. The geometry for one or more insulation elements of a control mark does not match the rest of the mark's elements. The wall panel below was found to have mismatched geometry for the listed insulation marks: \n{parameterAsString} ({str1}) - {str2} \n"
                }.Show();
                int num25 = (int) transaction.RollBack();
                return (Result) 1;
              }
              bool standard = string.IsNullOrWhiteSpace(keyValuePair1.Key);
              Dictionary<string, MarkGroupResultData> ifdictionaryexists = new Dictionary<string, MarkGroupResultData>();
              foreach (KeyValuePair<string, MarkGroupResultData> keyValuePair3 in dictionary3)
              {
                if (keyValuePair3.Key.EndsWith(keyValuePair1.Key))
                {
                  int numericMark = this.getNumericMark(keyValuePair3.Key, standard, keyValuePair1.Key);
                  ifdictionaryexists.Add(numericMark.ToString(), keyValuePair3.Value);
                }
              }
              foreach (KeyValuePair<string, MarkGroupResultData> keyValuePair4 in EDGE.GeometryTools.InsulationMarking.RunInitialComparison(activeUiDocument, document, (IEnumerable<FamilyInstance>) SFElementsToProcess, availableMarks, 0, ifdictionaryexists, false))
              {
                if (keyValuePair4.Value.GroupMembers.Any<FamilyInstance>())
                {
                  List<FamilyInstance> groupMembers = keyValuePair4.Value.GroupMembers;
                  string mark = this.createMark(keyValuePair4.Key, keyValuePair1.Key);
                  foreach (FamilyInstance familyInstance in groupMembers)
                  {
                    if (!EDGE.GeometryTools.InsulationMarking.isLocked(familyInstance))
                    {
                      Parameter parameter = familyInstance.LookupParameter("INSULATION_MARK");
                      familyInstance.LookupParameter("INSULATION_LOCK").Set(1);
                      familyInstance.LookupParameter("INSULATION_HOST_GUID").Set(familyInstance.UniqueId);
                      if (parameter != null && mark != null)
                      {
                        parameter.Set(mark);
                        flag3 = false;
                      }
                    }
                  }
                }
              }
            }
          }
        }
        if (flag3)
        {
          TaskDialog.Show("Insulation Marking", "No insulation was marked.");
          int num26 = (int) transaction.RollBack();
          return (Result) 1;
        }
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        if (ex.Message == "The user aborted the pick operation.")
          return (Result) 1;
        TaskDialog.Show("Insulation Marking", ex.Message);
        return (Result) 1;
      }
      TaskDialog.Show("Insulation Marking", "Insulation Successfully Marked.");
      int num27 = (int) transaction.Commit();
    }
    return (Result) 0;
  }

  private List<Element> WallFamilyInstances(Document revitDoc, Autodesk.Revit.UI.Selection.Selection preselectedElements)
  {
    List<Element> elementList1 = new List<Element>();
    List<ElementId> list1 = preselectedElements.GetElementIds().ToList<ElementId>();
    if (list1.Count > 0)
    {
      List<Element> elementList2 = new List<Element>();
      foreach (ElementId id in list1)
      {
        Element element = revitDoc.GetElement(id);
        if (element is AssemblyInstance)
          element = (element as AssemblyInstance).GetStructuralFramingElement();
        if (Utils.ElementUtils.Parameters.GetParameterAsString(element, "CONSTRUCTION_PRODUCT").ToUpper().ToUpper().Contains("WALL"))
        {
          elementList2.Add(element);
        }
        else
        {
          TaskDialog.Show("Insulation Marking", "One or more elements selected are not wall panel elements. Please select only wall panel elements for processing and try again.");
          return (List<Element>) null;
        }
      }
      if (elementList2.Count == 0)
      {
        TaskDialog.Show("Insulation Marking", "No wall panel elements were selected. Please select one or more wall panel elements for processing and try again.");
        return (List<Element>) null;
      }
      foreach (Element elem in elementList2)
      {
        if (Utils.ElementUtils.Parameters.GetParameterAsString(elem, "CONSTRUCTION_PRODUCT").ToUpper().Contains("WALL"))
          elementList1.Add(elem);
      }
      if (elementList1.Count == 0)
      {
        int num = (int) MessageBox.Show("Please select wall panel elments.");
        return (List<Element>) null;
      }
    }
    else
    {
      TaskDialog taskDialog = new TaskDialog("Insulation Selection Menu");
      taskDialog.MainContent = "What insulation elements from the model do you want marked?";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "All Insulation Elements within the Model");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Insulation Elements that are Hosted to Wall Panels within the Active View");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Insulation Elements that are Hosted to the Selected Wall Panels");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 2)
        return (List<Element>) null;
      if (taskDialogResult == 1001)
      {
        List<Element> list2 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).ToList<Element>();
        if (list2.Count == 0)
        {
          int num = (int) MessageBox.Show("No wall panel elements found in the model.");
          return (List<Element>) null;
        }
        foreach (Element elem in list2)
        {
          if (Utils.ElementUtils.Parameters.GetParameterAsString(elem, "CONSTRUCTION_PRODUCT").ToUpper().Contains("WALL"))
            elementList1.Add(elem);
        }
        if (elementList1.Count == 0)
        {
          int num = (int) MessageBox.Show("No wall panel elements found in the model.");
          return (List<Element>) null;
        }
      }
      else if (taskDialogResult == 1002)
      {
        List<Element> list3 = new FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).ToList<Element>();
        if (list3.Count == 0)
        {
          int num = (int) MessageBox.Show("No wall panel elements found in the active view.");
          return (List<Element>) null;
        }
        foreach (Element elem in list3)
        {
          if (Utils.ElementUtils.Parameters.GetParameterAsString(elem, "CONSTRUCTION_PRODUCT").ToUpper().Contains("WALL"))
            elementList1.Add(elem);
        }
        if (elementList1.Count == 0)
        {
          int num = (int) MessageBox.Show("No wall panel elements found in the active view.");
          return (List<Element>) null;
        }
      }
      else if (taskDialogResult == 1003)
      {
        WallPanelSelectionFilter panelSelectionFilter = new WallPanelSelectionFilter();
        IList<Reference> referenceList = preselectedElements.PickObjects((ObjectType) 1, (ISelectionFilter) panelSelectionFilter, "Select Wall Panel");
        if (referenceList.Count == 0)
        {
          int num = (int) MessageBox.Show("No wall panels seleected. Insulation Marking canceled.");
          return (List<Element>) null;
        }
        foreach (Reference reference in (IEnumerable<Reference>) referenceList)
        {
          Element element = revitDoc.GetElement(reference);
          elementList1.Add(element);
        }
      }
    }
    return elementList1;
  }

  private XYZ LeftMostProjectedPoint(Element wallPanel, Element insulation, Document revitDoc)
  {
    Transform transform1 = (wallPanel as FamilyInstance).GetTransform();
    Transform transform2 = (insulation as FamilyInstance).GetTransform();
    XYZ xyz1 = new XYZ();
    bool flag = PlacementUtilities.IsMirrored(wallPanel);
    PlanarFace botFace;
    bool bSymbol;
    PlacementUtilities.GetInsulationFace(revitDoc, insulation, out PlanarFace _, out botFace, out bSymbol);
    List<XYZ> source = new List<XYZ>();
    BoundingBoxUV boundingBox = botFace.GetBoundingBox();
    XYZ point1 = botFace.Evaluate(boundingBox.Min);
    XYZ point2 = botFace.Evaluate(boundingBox.Max);
    UV params1 = new UV(boundingBox.Min.U, boundingBox.Max.V);
    UV params2 = new UV(boundingBox.Max.U, boundingBox.Min.V);
    XYZ point3 = botFace.Evaluate(params1);
    XYZ point4 = botFace.Evaluate(params2);
    if (bSymbol)
    {
      XYZ point5 = transform2.OfPoint(point1);
      XYZ point6 = transform2.OfPoint(point3);
      XYZ point7 = transform2.OfPoint(point4);
      XYZ point8 = transform2.OfPoint(point2);
      XYZ xyz2 = transform1.Inverse.OfPoint(point5);
      XYZ xyz3 = transform1.Inverse.OfPoint(point6);
      XYZ xyz4 = transform1.Inverse.OfPoint(point7);
      XYZ xyz5 = transform1.Inverse.OfPoint(point8);
      source.Add(xyz2);
      source.Add(xyz3);
      source.Add(xyz4);
      source.Add(xyz5);
    }
    else
    {
      XYZ xyz6 = transform1.Inverse.OfPoint(point1);
      XYZ xyz7 = transform1.Inverse.OfPoint(point3);
      XYZ xyz8 = transform1.Inverse.OfPoint(point4);
      XYZ xyz9 = transform1.Inverse.OfPoint(point2);
      source.Add(xyz6);
      source.Add(xyz7);
      source.Add(xyz8);
      source.Add(xyz9);
    }
    double x;
    double y;
    double z;
    if (flag)
    {
      x = double.MaxValue;
      y = double.MaxValue;
      z = double.MaxValue;
    }
    else
    {
      x = double.MinValue;
      y = double.MinValue;
      z = double.MaxValue;
    }
    foreach (XYZ xyz10 in source.ToList<XYZ>())
    {
      if (flag)
      {
        if (xyz10.X < x)
          x = xyz10.X;
        if (xyz10.Y < y)
          y = xyz10.Y;
        if (xyz10.Z < z)
          z = xyz10.Z;
      }
      else
      {
        if (xyz10.X > x)
          x = xyz10.X;
        if (xyz10.Y > y)
          y = xyz10.Y;
        if (xyz10.Z < z)
          z = xyz10.Z;
      }
    }
    return new XYZ(x, y, z);
  }

  private string createMark(string markString, string suffix)
  {
    int result;
    if (!int.TryParse(markString, out result))
      return markString;
    if (string.IsNullOrWhiteSpace(suffix))
      ++result;
    int num = 1;
    while (result > this.marksList.Count<string>())
    {
      result -= this.marksList.Count<string>();
      ++num;
    }
    string empty = string.Empty;
    for (int index = 1; index <= num; ++index)
      empty += this.marksList[result - 1];
    return empty + suffix;
  }

  private int getNumericMark(string mark, bool standard, string suffix)
  {
    if (!standard && mark.EndsWith(suffix))
      mark = mark.Substring(0, mark.Length - suffix.Length);
    char[] charArray = mark.ToCharArray();
    string subString = charArray[0].ToString();
    int result = 0;
    if (this.marksList.Contains(subString))
    {
      int num1 = charArray.Length - 1;
      int num2 = this.marksList.FindIndex((Predicate<string>) (x => x == subString)) + 1;
      int num3 = this.marksList.Count<string>();
      if (standard)
        --num2;
      for (int index = 0; index < num1; ++index)
        result += num3;
      result += num2;
    }
    else if (!int.TryParse(mark, out result))
      return 0;
    return result;
  }
}
