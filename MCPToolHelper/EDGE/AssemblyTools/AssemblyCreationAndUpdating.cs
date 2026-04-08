// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.AssemblyCreationAndUpdating
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using EDGE.RebarTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.DocumentUtils;
using Utils.ElementUtils;
using Utils.Exceptions;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AssemblyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class AssemblyCreationAndUpdating : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    List<ElementInstance> IdsList = new List<ElementInstance>();
    Dictionary<ElementId, List<ElementId>> dictionaryForLegalIds = new Dictionary<ElementId, List<ElementId>>();
    Dictionary<ElementId, List<ElementId>> dictionaryForIllegalIds = new Dictionary<ElementId, List<ElementId>>();
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document revitDoc = activeUiDocument.Document;
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Assembly Creation and Updating Must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Assembly Creation and Updating must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    ICollection<AssemblyInstance> assemblyInstances = (ICollection<AssemblyInstance>) new Collection<AssemblyInstance>();
    if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.AssemblyCreation))
      return (Result) 1;
    App.DialogSwitches.SuspendModelLockingforOperation = true;
    using (Transaction transaction1 = new Transaction(revitDoc, "Assembly Creation and Updating"))
    {
      using (Transaction transaction2 = new Transaction(revitDoc, "Update Assembly Names"))
      {
        using (Transaction transaction3 = new Transaction(revitDoc, "Update Assembly Parameters"))
        {
          using (new Transaction(revitDoc, "Disassemble"))
          {
            using (TransactionGroup transactionGroup = new TransactionGroup(revitDoc))
            {
              transactionGroup.SetName("Assembly Creation Updating and Renaming");
              if (transactionGroup.Start() != TransactionStatus.Started)
              {
                TaskDialog.Show("Error", "Unable to start transaction group, please contact support");
                return (Result) 1;
              }
              try
              {
                ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
                if (elementIds.Count == 0)
                {
                  ISelectionFilter selFilter = (ISelectionFilter) new OnlyAssembliesAndStructFraming();
                  elementIds = References.PickNewReferences(activeUiDocument, selFilter, "Select existing assemblies or new structural framing elements to be made an assembly.");
                }
                if (elementIds == null || elementIds.Count == 0)
                  return (Result) 1;
                List<BuiltInCategory> categories = new List<BuiltInCategory>();
                categories.Add(BuiltInCategory.OST_StructuralFraming);
                categories.Add(BuiltInCategory.OST_Assemblies);
                ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) categories);
                IEnumerable<Element> source = (IEnumerable<Element>) new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIds.ToList<ElementId>()).WherePasses((ElementFilter) filter);
                if (source.Count<Element>() == elementIds.Count)
                  source = source.Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsBool(e, "HARDWARE_DETAIL")));
                if (source.Count<Element>() != elementIds.Count)
                {
                  TaskDialog.Show("Error", "Selection contains elements which are not structural framing elements or assemblies. Please refine your selection prior to running Assembly Creation, Updating, and Renaming.");
                  return (Result) 1;
                }
                if (elementIds.Select<ElementId, Element>((Func<ElementId, Element>) (eid => revitDoc.GetElement(eid))).Where<Element>((Func<Element, bool>) (e => e is FamilyInstance)).GroupBy<Element, string>((Func<Element, string>) (s => s.GetControlMark())).Where<IGrouping<string, Element>>((Func<IGrouping<string, Element>, bool>) (grp => grp.Count<Element>() > 1)).Any<IGrouping<string, Element>>())
                {
                  TaskDialog.Show("Error", "Selection contains elements with duplicate control marks.  Please correct this issue before re-running Assembly Creation Updating and Renaming");
                  return (Result) 1;
                }
                if (AssemblyCreationAndUpdating.ControlMarksContainIllegalCharacters(elementIds, revitDoc))
                  return (Result) 1;
                ICollection<string> existingAssemblyNames = AssemblyCreationAndUpdating.GetExistingAssemblyNames(revitDoc);
                ICollection<ElementId> errorGeneratingElements;
                if (AssemblyCreationAndUpdating.NamingConflictExists(revitDoc, existingAssemblyNames, (IEnumerable<ElementId>) elementIds, out errorGeneratingElements))
                {
                  new TaskDialog("Assembly Creation and Updating")
                  {
                    FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                    MainInstruction = "The creation of Assemblies from one or more of the selected structural framing elements would be in conflict with a previously existing Assembly Name.",
                    MainContent = $"Each unique Assembly must have a unique name and the name(s) generated within this tool are derived directly from the Structural Framing member's CONTROL_MARK parameter value. {Environment.NewLine}{Environment.NewLine}Possible problems causing this issue include (but are not limited to):{Environment.NewLine} (1) Incorrectly named assemblies.{Environment.NewLine} (2) Incorrect CONTROL_MARK parameter values leading to the duplicate names of assemblies.{Environment.NewLine}{Environment.NewLine}Press \"Close\" to select the Elements."
                  }.Show();
                  activeUiDocument.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
                  activeUiDocument.Selection.SetElementIds(errorGeneratingElements);
                  return (Result) 1;
                }
                int num1 = (int) transaction1.Start();
                HashSet<ElementId> elementIdSet1 = new HashSet<ElementId>();
                bool transactionCancelled = false;
                bool flag1 = false;
                foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
                {
                  Element element1 = revitDoc.GetElement(id);
                  transactionCancelled = false;
                  if (element1 is AssemblyInstance existingAssembly)
                  {
                    if (!elementIdSet1.Contains(existingAssembly.Id))
                    {
                      elementIdSet1.Add(existingAssembly.Id);
                      AssemblyInstance assemblyInstance = AssemblyInstances.UpdateExistingAssemblyInstance(ref transactionCancelled, IdsList, element1, existingAssembly, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                      if (!transactionCancelled && assemblyInstance != null)
                      {
                        assemblyInstances.Add(assemblyInstance);
                        flag1 = true;
                      }
                      else if (transactionCancelled)
                        return (Result) 1;
                    }
                  }
                  else if (revitDoc.GetElement(element1.AssemblyInstanceId) is AssemblyInstance element3)
                  {
                    if (!elementIdSet1.Contains(element3.Id))
                    {
                      elementIdSet1.Add(element3.Id);
                      AssemblyInstance assemblyInstance = AssemblyInstances.UpdateExistingAssemblyInstance(ref transactionCancelled, IdsList, element1, element3, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                      if (assemblyInstance != null && !transactionCancelled)
                      {
                        assemblyInstances.Add(assemblyInstance);
                        flag1 = true;
                      }
                      else if (transactionCancelled)
                        return (Result) 1;
                    }
                  }
                  else
                  {
                    foreach (Element element2 in ((FamilyInstance) element1).GetSubComponentIds().Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)))
                    {
                      if (!element2.AssemblyInstanceId.Equals((object) ElementId.InvalidElementId))
                      {
                        element3 = revitDoc.GetElement(element2.AssemblyInstanceId) as AssemblyInstance;
                        break;
                      }
                    }
                    if (element3 != null)
                    {
                      if (!elementIdSet1.Contains(element3.Id))
                      {
                        elementIdSet1.Add(element3.Id);
                        AssemblyInstance assemblyInstance = AssemblyInstances.UpdateExistingAssemblyInstance(ref transactionCancelled, IdsList, element1, element3, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                        if (assemblyInstance != null && !transactionCancelled)
                        {
                          assemblyInstances.Add(assemblyInstance);
                          flag1 = true;
                        }
                        else if (transactionCancelled)
                          return (Result) 1;
                      }
                    }
                    else
                    {
                      AssemblyInstance assemblyInstance = AssemblyInstances.CreateNewAssemblyInstance(ref transactionCancelled, IdsList, element1, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                      if (assemblyInstance != null && !transactionCancelled)
                      {
                        assemblyInstances.Add(assemblyInstance);
                        flag1 = true;
                      }
                      else if (transactionCancelled)
                        return (Result) 1;
                    }
                  }
                }
                if (transaction1.Commit() != TransactionStatus.Committed)
                  return (Result) 1;
                int num2 = (int) transaction2.Start();
                if (!transactionCancelled)
                  Utils.AssemblyUtils.Parameters.UpdateNames(assemblyInstances);
                int num3 = (int) transaction2.Commit();
                List<Element> invalidWeightInputs = new List<Element>();
                int num4 = (int) transaction3.Start();
                Dictionary<AssemblyInstance, List<ElementId>> ROCMList = new Dictionary<AssemblyInstance, List<ElementId>>();
                if (assemblyInstances.Count > 0 && !transactionCancelled)
                {
                  Utils.AssemblyUtils.Parameters.UpdateParameters(revitDoc, assemblyInstances, out ROCMList, out invalidWeightInputs);
                  if (App.RebarManager.IsAutomatedDocument(revitDoc) && revitDoc.MfgName().Equals("CORESLAB OKC"))
                    CoreSlabMarkingUtil.MarkAssembledRebar(revitDoc, (IEnumerable<AssemblyInstance>) assemblyInstances);
                }
                if (invalidWeightInputs.Count > 0)
                {
                  TaskDialog taskDialog = new TaskDialog("Assembly Creation, Updating, & Renaming Warning");
                  taskDialog.MainContent = "There are one or more elements (shown below) in which the UNIT_WEIGHT or WEIGHT_PER_UNIT has a value equal to or less than zero. This may cause issues running other EDGE tools with the created assemblies. Would you like to continue running the tool?";
                  taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
                  taskDialog.ExpandedContent += "Elements:\n";
                  List<string> stringList = new List<string>();
                  foreach (Element elem in invalidWeightInputs)
                  {
                    string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
                    if (!stringList.Contains(str))
                      stringList.Add(str);
                  }
                  stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
                  foreach (string str in stringList)
                    taskDialog.ExpandedContent += str;
                  if (taskDialog.Show() != 1)
                  {
                    if (transaction3.HasStarted())
                    {
                      int num5 = (int) transaction3.RollBack();
                    }
                    if (transactionGroup.HasStarted())
                    {
                      int num6 = (int) transactionGroup.RollBack();
                    }
                    return (Result) 1;
                  }
                }
                activeUiDocument.Selection.SetElementIds((ICollection<ElementId>) assemblyInstances.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (assembly => assembly.Id)).ToList<ElementId>());
                if (ROCMList.Keys.Count > 0)
                {
                  string str = "";
                  foreach (AssemblyInstance key in ROCMList.Keys)
                  {
                    str = $"{str}{key.Name}: \n";
                    foreach (ElementId elementId in ROCMList[key])
                      str = $"{str}{elementId.IntegerValue.ToString()}, ";
                    str = str.Substring(0, str.Length - 2);
                    str += "\n\n";
                  }
                  new TaskDialog("Assembly Creation and Updating")
                  {
                    MainContent = "One or more created assemblies contained members whose COUNT_MULTIPLIERs could not be written to. Assemblies were still created and COUNT_MULTIPLIERs were written for applicable elements. Expand for details.",
                    ExpandedContent = str
                  }.Show();
                }
                bool flag2 = false;
                if (App.DialogSwitches.IntersectionWarning2)
                {
                  TaskDialog taskDialog = new TaskDialog("Check for Intersections");
                  taskDialog.MainContent = "Would you like to check for intersections in the created assembly?";
                  taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Check For Intersections");
                  taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Do Not Check");
                  taskDialog.VerificationText = "Don't show this again?";
                  TaskDialogResult taskDialogResult = taskDialog.Show();
                  if (taskDialog.WasVerificationChecked())
                    App.DialogSwitches.IntersectionWarning2 = false;
                  if (taskDialogResult.Equals((object) (TaskDialogResult) 1001))
                  {
                    flag2 = true;
                    App.DialogSwitches.CheckForIntersect2 = true;
                  }
                  else if (taskDialogResult.Equals((object) (TaskDialogResult) 1002))
                  {
                    App.DialogSwitches.CheckForIntersect2 = false;
                  }
                  else
                  {
                    App.DialogSwitches.CheckForIntersect2 = false;
                    return (Result) 1;
                  }
                }
                else
                  flag2 = App.DialogSwitches.CheckForIntersect2 || App.DialogSwitches.CheckForIntersect2 && false;
                if (flag2)
                {
                  foreach (AssemblyInstance assemblyInstance in (IEnumerable<AssemblyInstance>) assemblyInstances)
                  {
                    List<ElementId> intersectingElementIds = new List<ElementId>();
                    ElementId assemblyId = assemblyInstance.Id;
                    List<ElementId> skippedSolids;
                    List<string> stringList = AssemblyInstances.CheckIntersectingAssemElements(assemblyInstance, out intersectingElementIds, out assemblyId, out skippedSolids);
                    if (intersectingElementIds.Count > 0)
                    {
                      if (intersectingElementIds.Count > 0)
                      {
                        activeUiDocument.Selection.SetElementIds((ICollection<ElementId>) intersectingElementIds);
                        activeUiDocument.ShowElements((ICollection<ElementId>) intersectingElementIds);
                      }
                      StringBuilder stringBuilder1 = new StringBuilder();
                      if (skippedSolids.Count > 0)
                      {
                        TaskDialog taskDialog = new TaskDialog("Intersections Warning");
                        taskDialog.MainInstruction = "Due to the geometry of some elements, they were not able to be fully processed during the intersection check. Intersection results for the elements below may be inaccurate:";
                        foreach (ElementId id in skippedSolids)
                        {
                          if (revitDoc.GetElement(id) is FamilyInstance element4)
                          {
                            string name = element4.Name;
                            ElementId typeId = element4.GetTypeId();
                            if (revitDoc.GetElement(typeId) is ElementType element)
                            {
                              string str = element.Name.ToString();
                              if (name != str)
                                stringBuilder1.AppendLine($"{name}: {str} - {id.ToString()}");
                              else
                                stringBuilder1.AppendLine($"{name}: {id.ToString()}");
                            }
                            else
                              stringBuilder1.AppendLine($"{name}: {id.ToString()}");
                          }
                          else
                            stringBuilder1.AppendLine(id.ToString());
                        }
                        taskDialog.ExpandedContent = stringBuilder1.ToString();
                        taskDialog.Show();
                      }
                      StringBuilder stringBuilder2 = new StringBuilder();
                      TaskDialog taskDialog1 = new TaskDialog("Intersections Found ");
                      taskDialog1.MainContent = "Intersections Found";
                      taskDialog1.MainInstruction = "The following intersections were found in the assembly created: ";
                      taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue with Assembly Creation");
                      taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Disassemble");
                      foreach (string str in stringList)
                        stringBuilder2.AppendLine(str);
                      taskDialog1.ExpandedContent = stringBuilder2.ToString();
                      if (!taskDialog1.Show().Equals((object) (TaskDialogResult) 1001))
                      {
                        if (revitDoc.GetElement(assemblyId) is AssemblyInstance)
                          (revitDoc.GetElement(assemblyId) as AssemblyInstance).Disassemble();
                        int num7 = (int) transaction3.Commit();
                        int num8 = (int) transactionGroup.Assimilate();
                        return (Result) 0;
                      }
                    }
                    else
                    {
                      StringBuilder stringBuilder = new StringBuilder();
                      if (skippedSolids.Count > 0)
                      {
                        TaskDialog taskDialog = new TaskDialog("Intersections Warning");
                        taskDialog.MainInstruction = "Due to the geometry of some elements, they were not able to be fully processed during the intersection check. Intersection results for the elements below may be inaccurate:";
                        foreach (ElementId id in skippedSolids)
                        {
                          if (revitDoc.GetElement(id) is FamilyInstance element5)
                          {
                            string name = element5.Name;
                            ElementId typeId = element5.GetTypeId();
                            if (revitDoc.GetElement(typeId) is ElementType element)
                            {
                              string str = element.Name.ToString();
                              if (name != str)
                                stringBuilder.AppendLine($"{name}: {str} - {id.ToString()}");
                              else
                                stringBuilder.AppendLine($"{name}: {id.ToString()}");
                            }
                            else
                              stringBuilder.AppendLine($"{name}: {id.ToString()}");
                          }
                          else
                            stringBuilder.AppendLine(id.ToString());
                        }
                        taskDialog.ExpandedContent = stringBuilder.ToString();
                        taskDialog.Show();
                      }
                    }
                  }
                }
                foreach (AssemblyInstance assembly in (IEnumerable<AssemblyInstance>) assemblyInstances)
                  AssemblyCreationAndUpdating.UpdateParameterHoldandIdentity(assembly, revitDoc);
                int num9 = (int) transaction3.Commit();
                List<ElementId> elementIdList = new List<ElementId>();
                foreach (AssemblyInstance assemblyInstance in (IEnumerable<AssemblyInstance>) assemblyInstances)
                {
                  try
                  {
                    if (assemblyInstance != null)
                      elementIdList.Add(assemblyInstance.Id);
                  }
                  catch
                  {
                  }
                }
                if (elementIdList.Count > 0)
                {
                  activeUiDocument.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
                  activeUiDocument.ShowElements((ICollection<ElementId>) elementIdList);
                }
                if (elementIdList.Count == 0)
                  flag1 = false;
                if (flag1)
                {
                  WarningWindow warningWindow = new WarningWindow(Process.GetCurrentProcess().MainWindowHandle);
                  if (dictionaryForIllegalIds.Count > 0)
                  {
                    warningWindow.boolCheckboxOn = true;
                    warningWindow.Topmost = true;
                    warningWindow.ShowDialog();
                    if (warningWindow.getCondition)
                    {
                      HashSet<ElementId> elementIdSet2 = new HashSet<ElementId>();
                      foreach (KeyValuePair<ElementId, List<ElementId>> keyValuePair in dictionaryForIllegalIds)
                      {
                        foreach (ElementId elementId in keyValuePair.Value)
                          elementIdSet2.Add(elementId);
                      }
                      activeUiDocument.Selection.SetElementIds((ICollection<ElementId>) elementIdSet2);
                    }
                  }
                  else
                  {
                    warningWindow.boolCheckboxOn = false;
                    warningWindow.ShowDialog();
                  }
                }
                if (transactionGroup.Assimilate() == TransactionStatus.Committed)
                  return (Result) 0;
                TaskDialog.Show("Error", "Unable to commit transaction group.  Please contact support");
                return (Result) 1;
              }
              catch (Exception ex)
              {
                if (transaction1.HasStarted())
                {
                  int num10 = (int) transaction1.RollBack();
                }
                if (transaction2.HasStarted())
                {
                  int num11 = (int) transaction2.RollBack();
                }
                if (transaction3.HasStarted())
                {
                  int num12 = (int) transaction3.RollBack();
                }
                if (ex.ToString().Contains("User aborted the pick operation"))
                  return (Result) 1;
                if (ex.ToString().Contains("Object reference not set to an instance of an object."))
                {
                  message = "Error: A selected Element is invalid for Assembly Creation and Updating. Either an invalid Element was selected, or there may be parameters missing.";
                  return (Result) -1;
                }
                if (ex.ToString().Contains("This assembly type name is not valid"))
                {
                  message = "Error: An Assembly with this name already exists in the project. Assembly names must be unique in their naming category.";
                  return (Result) -1;
                }
                if (ex.ToString().Contains("One or more element ids was not permitted for membership in the assembly instance."))
                {
                  message = "Error: One or more elements was not permitted for membership in the assembly because it is already a member of another assembly.";
                  return (Result) -1;
                }
                if (ex is EdgeException)
                {
                  message = ex.Message;
                  return (Result) -1;
                }
                message = ex.ToString();
                return (Result) -1;
              }
              finally
              {
                App.DialogSwitches.SuspendModelLockingforOperation = false;
              }
            }
          }
        }
      }
    }
  }

  private static ICollection<string> GetExistingAssemblyNames(Document doc)
  {
    return (ICollection<string>) new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Assemblies).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Select<AssemblyInstance, string>((Func<AssemblyInstance, string>) (assembly => assembly.AssemblyTypeName)).ToList<string>();
  }

  private static bool NamingConflictExists(
    Document revitDoc,
    ICollection<string> assemblyNames,
    IEnumerable<ElementId> selectedElementIds,
    out ICollection<ElementId> errorGeneratingElements)
  {
    ICollection<Element> source = StructuralFraming.RefineNestedFamilies((IEnumerable<ElementId>) selectedElementIds.ToList<ElementId>(), revitDoc);
    errorGeneratingElements = (ICollection<ElementId>) source.Where<Element>((Func<Element, bool>) (e => e.AssemblyInstanceId.IntegerValue == -1)).Where<Element>((Func<Element, bool>) (e => assemblyNames.Contains(Utils.ElementUtils.Parameters.GetParameterAsString(e, "CONTROL_MARK")))).Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
    return errorGeneratingElements.Count > 0;
  }

  public static bool ControlMarksContainIllegalCharacters(
    ICollection<ElementId> selectedIdList,
    Document revitDoc)
  {
    Regex illegalCharMatcher = new Regex("[<>;:\\\\\\[\\]\\{\\}\\|\\`\\~\\?]");
    IEnumerable<Element> source = StructuralFraming.RefineNestedFamilies((IEnumerable<ElementId>) selectedIdList, revitDoc).Where<Element>((Func<Element, bool>) (elem => illegalCharMatcher.Match(elem.GetControlMark()).Success));
    if (!source.Any<Element>())
      return false;
    new TaskDialog("Illegal Characters in Control Marks")
    {
      AllowCancellation = false,
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainInstruction = "Illegal Characters Found in Control Marks",
      MainContent = "One of the following characters was found in the structural framing control mark: <>;:\\[]|{}`~?  Please correct these control marks before running EDGE^R Assembly Creation Updating and Renaming Tool.  See expanded content for specific control marks",
      ExpandedContent = string.Join(Environment.NewLine, (IEnumerable<string>) source.Select<Element, string>((Func<Element, string>) (elem => elem.GetControlMark())).ToList<string>())
    }.Show();
    return true;
  }

  public static void UpdateParameterHoldandIdentity(AssemblyInstance assembly, Document revitdoc)
  {
    string name1 = "IDENTITY_COMMENT";
    string name2 = "ON_HOLD";
    Element topLevelElement = assembly.GetStructuralFramingElement().GetTopLevelElement();
    Parameter parameter1 = topLevelElement.LookupParameter(name2);
    Parameter parameter2 = topLevelElement.LookupParameter(name1);
    Element elem1;
    if (parameter1 != null && parameter1.HasValue)
    {
      Parameter paramElemOH1 = assembly.LookupParameter(name2);
      if (assembly != null && paramElemOH1 != null)
      {
        AssemblyCreationAndUpdating.checkAndSetOnHold(topLevelElement, paramElemOH1);
      }
      else
      {
        Parameter paramElemOH2 = AssemblyCreationAndUpdating.typeParameterreturn(assembly, (Element) null, name2, revitdoc, out elem1);
        if (paramElemOH2 != null && !paramElemOH2.IsReadOnly)
          AssemblyCreationAndUpdating.checkAndSetOnHold(topLevelElement, paramElemOH2);
      }
    }
    else
    {
      Element elem2;
      Parameter parameter3 = AssemblyCreationAndUpdating.typeParameterreturn((AssemblyInstance) null, topLevelElement, name2, revitdoc, out elem2);
      if (parameter3 != null && parameter3.HasValue)
      {
        Parameter paramElemOH3 = assembly.LookupParameter(name2);
        if (assembly != null && paramElemOH3 != null)
        {
          AssemblyCreationAndUpdating.checkAndSetOnHold(topLevelElement, paramElemOH3);
        }
        else
        {
          Parameter paramElemOH4 = AssemblyCreationAndUpdating.typeParameterreturn(assembly, (Element) null, name2, revitdoc, out elem1);
          if (paramElemOH4 != null && !paramElemOH4.IsReadOnly)
            AssemblyCreationAndUpdating.checkAndSetOnHold(elem2, paramElemOH4);
        }
      }
    }
    if (parameter2 != null && parameter2.HasValue)
    {
      Parameter parameter4 = assembly.LookupParameter(name1);
      if (assembly != null && parameter4 != null)
      {
        AssemblyCreationAndUpdating.checkAndSetIdentityComment(topLevelElement, parameter4);
      }
      else
      {
        Parameter parameter5 = AssemblyCreationAndUpdating.typeParameterreturn(assembly, (Element) null, name1, revitdoc, out elem1);
        if (parameter5 == null || parameter5.IsReadOnly)
          return;
        AssemblyCreationAndUpdating.checkAndSetIdentityComment(topLevelElement, parameter5);
      }
    }
    else
    {
      Element elem3;
      Parameter parameter6 = AssemblyCreationAndUpdating.typeParameterreturn((AssemblyInstance) null, topLevelElement, name1, revitdoc, out elem3);
      if (parameter6 == null || !parameter6.HasValue)
        return;
      Parameter parameter7 = assembly.LookupParameter(name1);
      if (assembly != null && parameter7 != null)
      {
        AssemblyCreationAndUpdating.checkAndSetIdentityComment(topLevelElement, parameter7);
      }
      else
      {
        Parameter parameter8 = AssemblyCreationAndUpdating.typeParameterreturn(assembly, (Element) null, name1, revitdoc, out elem1);
        if (parameter8 == null || parameter8.IsReadOnly)
          return;
        AssemblyCreationAndUpdating.checkAndSetIdentityComment(elem3, parameter8);
      }
    }
  }

  public static Parameter typeParameterreturn(
    AssemblyInstance assembly,
    Element element,
    string value,
    Document revitdoc,
    out Element elem)
  {
    Element elem1 = assembly == null ? element.GetTopLevelElement() : assembly.GetTopLevelElement();
    Parameter parameter = elem1.GetTopLevelElement().LookupParameter(value);
    Element element1 = (Element) null;
    if (parameter == null || parameter.IsReadOnly)
    {
      ElementId typeId = elem1.GetTypeId();
      if (typeId != ElementId.InvalidElementId)
      {
        element1 = revitdoc.GetElement(typeId);
        parameter = element1.LookupParameter(value);
      }
    }
    elem = element1;
    return parameter;
  }

  public static void checkAndSetOnHold(Element element, Parameter paramElemOH)
  {
    int num1;
    switch (Utils.ElementUtils.Parameters.GetParameterAsBool(element, "ON_HOLD").ToString())
    {
      case "True":
        num1 = 1;
        break;
      case "False":
        num1 = 0;
        break;
      default:
        num1 = -1;
        break;
    }
    int num2 = num1;
    if (num2 == -1)
      return;
    paramElemOH.Set(num2);
  }

  public static void checkAndSetIdentityComment(Element element, Parameter parameter)
  {
    string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element, "IDENTITY_COMMENT");
    if (string.IsNullOrWhiteSpace(parameterAsString))
      return;
    parameter.Set(parameterAsString);
  }
}
