// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.AssemblyCreationRevit
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.Forms;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AssemblyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class AssemblyCreationRevit : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document revitDoc = activeUiDocument.Document;
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Assembly Creation and Updating Revit Must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Assembly Creation and Updating Revit must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    ICollection<AssemblyInstance> assemblyInstances = (ICollection<AssemblyInstance>) new Collection<AssemblyInstance>();
    if (!ModelLockingUtils.ShowPermissionsDialog(revitDoc, ModelLockingToolPermissions.AssemblyCreation))
    {
      message = $"User: {revitDoc.Application.Username} is not allowed to create Assemblies.  Please contact your BIM Manager if you believe this is in error.";
      return (Result) -1;
    }
    App.DialogSwitches.SuspendModelLockingforOperation = true;
    using (Transaction transaction1 = new Transaction(revitDoc, "Assembly Creation and Updating Revit"))
    {
      using (Transaction transaction2 = new Transaction(revitDoc, "Update Assembly Parameters"))
      {
        using (Transaction transaction3 = new Transaction(revitDoc, "Disassemble"))
        {
          using (TransactionGroup transactionGroup = new TransactionGroup(revitDoc))
          {
            transactionGroup.SetName("Assembly Creation and Updating");
            if (transactionGroup.Start() != TransactionStatus.Started)
            {
              TaskDialog.Show("Error", "Unable to start transaction group, please contact support");
              return (Result) 1;
            }
            try
            {
              bool transactionCancelled = false;
              bool flag1 = false;
              List<ElementInstance> IdsList = new List<ElementInstance>();
              Dictionary<ElementId, List<ElementId>> dictionaryForLegalIds = new Dictionary<ElementId, List<ElementId>>();
              Dictionary<ElementId, List<ElementId>> dictionaryForIllegalIds = new Dictionary<ElementId, List<ElementId>>();
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
              int num1 = (int) transaction1.Start();
              HashSet<ElementId> elementIdSet = new HashSet<ElementId>();
              foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
              {
                Element element1 = revitDoc.GetElement(id);
                if (element1 is AssemblyInstance existingAssembly)
                {
                  if (!elementIdSet.Contains(existingAssembly.Id))
                  {
                    elementIdSet.Add(existingAssembly.Id);
                    AssemblyInstance assemblyInstance = AssemblyInstances.UpdateExistingAssemblyInstance(ref transactionCancelled, IdsList, element1, existingAssembly, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                    if (!transactionCancelled && assemblyInstance != null)
                    {
                      assemblyInstances.Add(assemblyInstance);
                      flag1 = true;
                    }
                  }
                }
                else if (revitDoc.GetElement(element1.AssemblyInstanceId) is AssemblyInstance element3)
                {
                  if (!elementIdSet.Contains(element3.Id))
                  {
                    elementIdSet.Add(element3.Id);
                    AssemblyInstance assemblyInstance = AssemblyInstances.UpdateExistingAssemblyInstance(ref transactionCancelled, IdsList, element1, element3, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                    if (assemblyInstance != null && !transactionCancelled)
                    {
                      assemblyInstances.Add(assemblyInstance);
                      flag1 = true;
                    }
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
                    if (!elementIdSet.Contains(element3.Id))
                    {
                      elementIdSet.Add(element3.Id);
                      AssemblyInstance assemblyInstance = AssemblyInstances.UpdateExistingAssemblyInstance(ref transactionCancelled, IdsList, element1, element3, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                      if (assemblyInstance != null && !transactionCancelled)
                      {
                        assemblyInstances.Add(assemblyInstance);
                        flag1 = true;
                      }
                    }
                  }
                  else
                  {
                    AssemblyInstance assemblyInstance = AssemblyInstances.CreateNewAssemblyInstance(ref transactionCancelled, IdsList, element1, commandData, ref dictionaryForLegalIds, ref dictionaryForIllegalIds);
                    if (assemblyInstance != null)
                    {
                      assemblyInstances.Add(assemblyInstance);
                      flag1 = true;
                    }
                  }
                }
              }
              if (transaction1.Commit() != TransactionStatus.Committed)
                return (Result) 1;
              List<Element> invalidWeightInputs = new List<Element>();
              int num2 = (int) transaction2.Start();
              Dictionary<AssemblyInstance, List<ElementId>> ROCMList = new Dictionary<AssemblyInstance, List<ElementId>>();
              if (!transactionCancelled)
                Utils.AssemblyUtils.Parameters.UpdateParameters(revitDoc, assemblyInstances, out ROCMList, out invalidWeightInputs);
              if (invalidWeightInputs.Count > 0)
              {
                TaskDialog taskDialog = new TaskDialog("Assembly Creation & Updating (Reivit) Warning");
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
                  if (transaction2.HasStarted())
                  {
                    int num3 = (int) transaction2.RollBack();
                  }
                  if (transactionGroup.HasStarted())
                  {
                    int num4 = (int) transactionGroup.RollBack();
                  }
                  return (Result) 1;
                }
              }
              foreach (AssemblyInstance assembly in (IEnumerable<AssemblyInstance>) assemblyInstances)
                AssemblyCreationRevit.UpdateParameterHoldandIdentity(assembly, revitDoc);
              int num5 = (int) transaction2.Commit();
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
              if (App.DialogSwitches.IntersectionWarning)
              {
                TaskDialog taskDialog = new TaskDialog("Check for Intersections");
                taskDialog.MainContent = "Would you like to check for intersections in the created assembly?";
                taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Check For Intersections");
                taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Do Not Check");
                taskDialog.VerificationText = "Don't show this again?";
                TaskDialogResult taskDialogResult = taskDialog.Show();
                if (taskDialog.WasVerificationChecked())
                  App.DialogSwitches.IntersectionWarning = false;
                if (taskDialogResult.Equals((object) (TaskDialogResult) 1001))
                {
                  flag2 = true;
                  App.DialogSwitches.CheckForIntersect = true;
                }
                else if (taskDialogResult.Equals((object) (TaskDialogResult) 1002))
                {
                  App.DialogSwitches.CheckForIntersect = false;
                }
                else
                {
                  App.DialogSwitches.CheckForIntersect = false;
                  return (Result) 1;
                }
              }
              else
                flag2 = App.DialogSwitches.CheckForIntersect || App.DialogSwitches.CheckForIntersect && false;
              if (flag2)
              {
                foreach (AssemblyInstance assemblyInstance in (IEnumerable<AssemblyInstance>) assemblyInstances)
                {
                  List<ElementId> elementIdList1 = new List<ElementId>();
                  ElementId id = (ElementId) null;
                  ref List<ElementId> local1 = ref elementIdList1;
                  ref ElementId local2 = ref id;
                  List<ElementId> elementIdList2;
                  ref List<ElementId> local3 = ref elementIdList2;
                  List<string> stringList = AssemblyInstances.CheckIntersectingAssemElements(assemblyInstance, out local1, out local2, out local3);
                  if (elementIdList1.Count > 0)
                  {
                    if (elementIdList1.Count > 0)
                    {
                      activeUiDocument.Selection.SetElementIds((ICollection<ElementId>) elementIdList1);
                      activeUiDocument.ShowElements((ICollection<ElementId>) elementIdList1);
                    }
                    StringBuilder stringBuilder1 = new StringBuilder();
                    if (elementIdList2.Count > 0)
                    {
                      TaskDialog taskDialog = new TaskDialog("Intersections Warning");
                      taskDialog.MainInstruction = "One or more pieces of element geometry were unable to be processed properly. The intersection tests results maybe incorrect for the following elements: ";
                      foreach (ElementId elementId in elementIdList2)
                        stringBuilder1.AppendLine(elementId.ToString());
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
                      int num6 = (int) transaction3.Start();
                      if (revitDoc.GetElement(id) is AssemblyInstance)
                        (revitDoc.GetElement(id) as AssemblyInstance).Disassemble();
                      int num7 = (int) transaction3.Commit();
                    }
                  }
                  StringBuilder stringBuilder = new StringBuilder();
                  if (elementIdList2.Count > 0)
                  {
                    TaskDialog taskDialog = new TaskDialog("Intersections Warning");
                    taskDialog.MainInstruction = "One or more pieces of element geometry were unable to be processed properly. The intersection tests results maybe incorrect for the following elements: ";
                    foreach (ElementId elementId in elementIdList2)
                      stringBuilder.AppendLine(elementId.ToString());
                    taskDialog.ExpandedContent = stringBuilder.ToString();
                    taskDialog.Show();
                  }
                }
              }
              if (transactionGroup.Assimilate() != TransactionStatus.Committed)
              {
                TaskDialog.Show("Error", "Unable to commit transaction group.  Please contact support");
                return (Result) 1;
              }
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
                EDGEWarning edgeWarning = new EDGEWarning();
                edgeWarning.lblVersionInfo.Text = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                int num8 = (int) edgeWarning.ShowDialog();
              }
              return (Result) 0;
            }
            catch (Exception ex)
            {
              if (transaction1.HasStarted())
              {
                int num9 = (int) transaction1.RollBack();
              }
              if (transaction2.HasStarted())
              {
                int num10 = (int) transaction2.RollBack();
              }
              if (ex.ToString().Contains("User aborted the pick operation"))
                return (Result) 1;
              if (ex.ToString().Contains("Object reference not set to an instance of an object."))
              {
                message = "Error: A selected Element is invalid for Assembly Creation and Updating. Only Structural Framing Elements and Assemblies are valid selections.";
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

  private static ICollection<string> GetExistingAssemblyNames(Document doc)
  {
    return (ICollection<string>) new FilteredElementCollector(doc).WherePasses((ElementFilter) new ElementClassFilter(typeof (AssemblyInstance))).Cast<AssemblyInstance>().Select<AssemblyInstance, string>((Func<AssemblyInstance, string>) (assembly => assembly.AssemblyTypeName)).ToList<string>();
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
        AssemblyCreationRevit.checkAndSetOnHold(topLevelElement, paramElemOH1);
      }
      else
      {
        Parameter paramElemOH2 = AssemblyCreationRevit.typeParameterreturn(assembly, (Element) null, name2, revitdoc, out elem1);
        if (paramElemOH2 != null && !paramElemOH2.IsReadOnly)
          AssemblyCreationRevit.checkAndSetOnHold(topLevelElement, paramElemOH2);
      }
    }
    else
    {
      Element elem2;
      Parameter parameter3 = AssemblyCreationRevit.typeParameterreturn((AssemblyInstance) null, topLevelElement, name2, revitdoc, out elem2);
      if (parameter3 != null && parameter3.HasValue)
      {
        Parameter paramElemOH3 = assembly.LookupParameter(name2);
        if (assembly != null && paramElemOH3 != null)
        {
          AssemblyCreationRevit.checkAndSetOnHold(topLevelElement, paramElemOH3);
        }
        else
        {
          Parameter paramElemOH4 = AssemblyCreationRevit.typeParameterreturn(assembly, (Element) null, name2, revitdoc, out elem1);
          if (paramElemOH4 != null && !paramElemOH4.IsReadOnly)
            AssemblyCreationRevit.checkAndSetOnHold(elem2, paramElemOH4);
        }
      }
    }
    if (parameter2 != null && parameter2.HasValue)
    {
      Parameter parameter4 = assembly.LookupParameter(name1);
      if (assembly != null && parameter4 != null)
      {
        AssemblyCreationRevit.checkAndSetIdentityComment(topLevelElement, parameter4);
      }
      else
      {
        Parameter parameter5 = AssemblyCreationRevit.typeParameterreturn(assembly, (Element) null, name1, revitdoc, out elem1);
        if (parameter5 == null || parameter5.IsReadOnly)
          return;
        AssemblyCreationRevit.checkAndSetIdentityComment(topLevelElement, parameter5);
      }
    }
    else
    {
      Element elem3;
      Parameter parameter6 = AssemblyCreationRevit.typeParameterreturn((AssemblyInstance) null, topLevelElement, name1, revitdoc, out elem3);
      if (parameter6 == null || !parameter6.HasValue)
        return;
      Parameter parameter7 = assembly.LookupParameter(name1);
      if (assembly != null && parameter7 != null)
      {
        AssemblyCreationRevit.checkAndSetIdentityComment(topLevelElement, parameter7);
      }
      else
      {
        Parameter parameter8 = AssemblyCreationRevit.typeParameterreturn(assembly, (Element) null, name1, revitdoc, out elem1);
        if (parameter8 == null || parameter8.IsReadOnly)
          return;
        AssemblyCreationRevit.checkAndSetIdentityComment(elem3, parameter8);
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
