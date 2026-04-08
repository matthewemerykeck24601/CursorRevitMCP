// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.InsulationMarking
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.AssemblyTools.MarkVerification.GeometryComparer;
using EDGE.AssemblyTools.MarkVerification.InitialPresentation;
using EDGE.AssemblyTools.MarkVerification.ResultsPresentation;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.ElementUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.GeometryTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class InsulationMarking : IExternalCommand
{
  private Document revitDoc;
  private static MKTolerances _toleranceComparer;
  private static UIDocument _uiDoc;
  public static Dictionary<string, InsulationVerificationFamily> _GeomVerificationStore = new Dictionary<string, InsulationVerificationFamily>();
  public static MarkVerificationData _MVData;
  private int counter;
  private int counter2;
  private Dictionary<string, MarkGroupResultData> originalbuckets = new Dictionary<string, MarkGroupResultData>();

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    this.revitDoc = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    List<FamilyInstance> familyInstanceList1 = new List<FamilyInstance>();
    List<string> stringList = new List<string>();
    bool wholeModelSelected = false;
    if (this.revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Insulation Marking Tool must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Insulation Marking must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    Dictionary<string, List<FamilyInstance>> dictionary = new Dictionary<string, List<FamilyInstance>>();
    using (Transaction transaction = new Transaction(this.revitDoc, "Insulation Marking"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        InsulationMarking._GeomVerificationStore.Clear();
        InsulationMarking._MVData = new MarkVerificationData(this.revitDoc, ComparisonOption.DoNotRound, true);
        Autodesk.Revit.UI.Selection.Selection selection = application.ActiveUIDocument.Selection;
        bool insulationNotSelected = false;
        bool menuClosed = false;
        bool flag1 = true;
        bool hardwareDetail = false;
        List<Element> zeroVols;
        IEnumerable<Element> source = InsulationMarking.InsulationFilter(this.revitDoc, selection, out zeroVols, out insulationNotSelected, out menuClosed, out wholeModelSelected, out hardwareDetail);
        if (menuClosed)
          return (Result) 1;
        if (insulationNotSelected)
        {
          new TaskDialog("Warning")
          {
            MainInstruction = "No insulation elements were selected. Please run the tool again and select insulation elements.",
            CommonButtons = ((TaskDialogCommonButtons) 1)
          }.Show();
          return (Result) 1;
        }
        if (source == null)
        {
          new TaskDialog("Warning")
          {
            MainInstruction = "One or more of the elements selected were not insulation elements. Please run the tool again and only select insulation elements.",
            CommonButtons = ((TaskDialogCommonButtons) 1)
          }.Show();
          return (Result) 1;
        }
        if (zeroVols.Count > 0)
        {
          StringBuilder stringBuilder = new StringBuilder("The following IDs had zero or negative volume\n");
          foreach (Element element in zeroVols)
            stringBuilder.AppendLine(element.Id.ToString());
          if (new TaskDialog("Warning")
          {
            MainInstruction = "One or more insulation elements had zero volume and will not be marked. If you choose to continue, they will have any existing insulation mark deleted. Continue?",
            ExpandedContent = stringBuilder.ToString(),
            CommonButtons = ((TaskDialogCommonButtons) 9)
          }.Show() == 2)
            return (Result) 1;
          foreach (Element element in zeroVols)
          {
            Parameter parameter = element.LookupParameter("INSULATION_MARK");
            if (parameter != null)
            {
              parameter.Set("");
              flag1 = false;
            }
            (element.LookupParameter("INSULATION_LOCK") ?? element.LookupParameter("LOCKED_CHECKBOX"))?.Set(0);
          }
        }
        if (source.Count<Element>() <= 0)
        {
          new TaskDialog("Insulation Marking")
          {
            MainInstruction = "No insulation could be found in the project."
          }.Show();
          return (Result) 1;
        }
        if (hardwareDetail)
          new TaskDialog("Warning")
          {
            MainInstruction = "One or more of the elements selected are marked as hardware detail elements. The tool will not mark these elements but continue for all valid elements.",
            CommonButtons = ((TaskDialogCommonButtons) 1)
          }.Show();
        IEnumerable<FamilyInstance> SFElementsToProcess = source.Cast<FamilyInstance>();
        bool flag2 = false;
        List<ElementId> list = source.Select<Element, ElementId>((Func<Element, ElementId>) (s => s.Id)).Distinct<ElementId>().ToList<ElementId>();
        if (this.revitDoc.IsWorkshared && !CheckElementsOwnership.CheckOwnership("Insulation Marking", list, this.revitDoc, activeUiDocument, out List<ElementId> _, false))
          return (Result) 1;
        foreach (FamilyInstance elem in SFElementsToProcess)
        {
          Parameter parameter1 = elem.LookupParameter("INSULATION_MARK");
          if (parameter1 == null)
          {
            new TaskDialog("Warning")
            {
              MainInstruction = "The INSULATION_MARK parameter either does not exist or is a type parameter for one or more insulation elements in the project."
            }.Show();
            return (Result) 1;
          }
          if (parameter1.IsReadOnly)
          {
            new TaskDialog("Warning")
            {
              MainInstruction = "The INSULATION_MARK parameter is read-only for one or more insulation elements in the project."
            }.Show();
            return (Result) 1;
          }
          Parameter parameter2 = elem.LookupParameter("INSULATION_HOST_GUID");
          if (parameter2 == null)
          {
            new TaskDialog("Warning")
            {
              MainInstruction = "The INSULATION_HOST_GUID parameter either does not exist or is a type parameter for one or more insulation elements in the project."
            }.Show();
            return (Result) 1;
          }
          if (parameter2.IsReadOnly)
          {
            new TaskDialog("Warning")
            {
              MainInstruction = "The INSULATION_HOST_GUID parameter is read-only for one or more insulation elements in the project."
            }.Show();
            return (Result) 1;
          }
          Parameter parameter3 = elem.LookupParameter("INSULATION_LOCK");
          if (parameter3 == null)
          {
            new TaskDialog("Warning")
            {
              MainInstruction = "The INSULATION_LOCK parameter either does not exist or is a type parameter for one or more insulation elements in the project."
            }.Show();
            return (Result) 1;
          }
          if (parameter3.IsReadOnly)
          {
            new TaskDialog("Warning")
            {
              MainInstruction = "The INSULATION_LOCK parameter is read-only for one or more insulation elements in the project."
            }.Show();
            return (Result) 1;
          }
          if (Parameters.GetParameterAsBool((Element) elem, "INSULATION_LOCK"))
          {
            if (Parameters.GetParameterAsString((Element) elem, "INSULATION_HOST_GUID").Equals(elem.UniqueId))
            {
              string parameterAsString = Parameters.GetParameterAsString((Element) elem, "INSULATION_MARK");
              if (!dictionary.ContainsKey(parameterAsString))
                dictionary.Add(parameterAsString, new List<FamilyInstance>()
                {
                  elem
                });
              else
                dictionary[parameterAsString].Add(elem);
            }
            else
              flag2 = true;
          }
        }
        if (flag2 && TaskDialog.Show("Insulation Marking", "One or more elements were \"locked\" but had mismatched INSULATION_HOST_GUIDs. Insulation Marking will treat these elements as unlocked. Proceed with insulation marking?", (TaskDialogCommonButtons) 9) != 1)
        {
          int num2 = (int) transaction.RollBack();
          return (Result) 1;
        }
        int highestMarkUsed = 0;
        List<string> availableMarks = new List<string>();
        if (!wholeModelSelected)
          highestMarkUsed = InsulationMarking.GetExistingMarks(this.revitDoc, out availableMarks);
        if (dictionary.Count > 0)
        {
          foreach (KeyValuePair<string, List<FamilyInstance>> keyValuePair in dictionary)
          {
            List<FamilyInstance> familyInstanceList2 = keyValuePair.Value;
            if (InsulationMarking.RunInitialComparison(activeUiDocument, this.revitDoc, (IEnumerable<FamilyInstance>) familyInstanceList2, availableMarks, highestMarkUsed, new Dictionary<string, MarkGroupResultData>(), wholeModelSelected).Keys.Count > 1)
              stringList.Add(keyValuePair.Key.ToString());
            else
              this.originalbuckets.Add(keyValuePair.Key, new MarkGroupResultData()
              {
                GroupMembers = familyInstanceList2.ToList<FamilyInstance>()
              });
          }
        }
        if (stringList.Count > 0)
        {
          stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
          string str = string.Join("; \t", stringList.ToArray());
          new TaskDialog("Geometry Mismatch")
          {
            MainInstruction = $"The geometry for one or more insulation elements of a control mark does not match the rest of the mark's elements.\nInsulation marks: \n{str} \n"
          }.Show();
          return (Result) 1;
        }
        this.originalbuckets = InsulationMarking.RunInitialComparison(activeUiDocument, this.revitDoc, SFElementsToProcess, availableMarks, highestMarkUsed, this.originalbuckets, wholeModelSelected);
        foreach (KeyValuePair<string, MarkGroupResultData> originalbucket in this.originalbuckets)
        {
          if (originalbucket.Value.GroupMembers.Any<FamilyInstance>())
          {
            foreach (FamilyInstance groupMember in originalbucket.Value.GroupMembers)
            {
              if (!InsulationMarking.isLocked(groupMember))
              {
                Parameter parameter = groupMember.LookupParameter("INSULATION_MARK");
                groupMember.LookupParameter("INSULATION_LOCK").Set(1);
                groupMember.LookupParameter("INSULATION_HOST_GUID").Set(groupMember.UniqueId);
                string key = originalbucket.Key;
                parameter?.Set(key);
                flag1 = false;
              }
            }
          }
        }
        if (flag1)
        {
          TaskDialog.Show("Insulation Marking", "No insulation was marked.");
          int num3 = (int) transaction.RollBack();
          return (Result) 1;
        }
        if (this.originalbuckets.Keys.Count > 0)
        {
          if (transaction.Commit() == TransactionStatus.Committed)
            TaskDialog.Show("Insulation Marking", "Insulation Successfully Marked.");
        }
      }
      catch (Exception ex)
      {
        int num = (int) transaction.RollBack();
        if (ex.Message == "The user aborted the pick operation.")
          return (Result) 1;
        TaskDialog.Show("Insulation Marking", ex.Message);
        return (Result) 1;
      }
      return (Result) 0;
    }
  }

  internal static IEnumerable<Element> InsulationFilter(
    Document revitdoc,
    Autodesk.Revit.UI.Selection.Selection sel,
    out List<Element> zeroVols,
    out bool insulationNotSelected,
    out bool menuClosed,
    out bool wholeModelSelected,
    out bool hardwareDetail)
  {
    List<Element> source1 = new List<Element>();
    zeroVols = new List<Element>();
    insulationNotSelected = false;
    menuClosed = false;
    hardwareDetail = false;
    wholeModelSelected = false;
    ICollection<ElementId> elementIds1 = sel.GetElementIds();
    List<Element> elementList = new List<Element>();
    int num1 = elementIds1.Count<ElementId>();
    if (num1 > 0)
    {
      List<Element> list = new FilteredElementCollector(revitdoc, elementIds1).OfClass(typeof (FamilyInstance)).ToList<Element>();
      if (num1 != list.Count<Element>())
        return (IEnumerable<Element>) null;
      foreach (Element elem in list)
      {
        if (!elem.Category.Id.IntegerValue.Equals(-2001350) && !elem.Category.Id.IntegerValue.Equals(-2000151) || !Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").ToUpper().ToUpper().Contains("INSULATION"))
          return (IEnumerable<Element>) null;
        if (!Parameters.GetParameterAsBool(elem, "HARDWARE_DETAIL"))
          source1.Add(elem);
        else
          hardwareDetail = true;
      }
    }
    if (num1 == 0)
    {
      TaskDialog taskDialog = new TaskDialog("Insulation Selection Menu");
      taskDialog.MainContent = "What insulation elements from the model do you want marked?";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "All Insulation Elements within the Model");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Insulation Elements within the Active View");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Selected Insulation Elements");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult.Equals((object) (TaskDialogResult) 2))
        menuClosed = true;
      if (taskDialogResult.Equals((object) (TaskDialogResult) 1001))
      {
        wholeModelSelected = true;
        ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_SpecialityEquipment,
          BuiltInCategory.OST_GenericModel
        });
        source1 = new FilteredElementCollector(revitdoc).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (s => Parameters.GetParameterAsString(s, "MANUFACTURE_COMPONENT").ToUpper().ToUpper().Contains("INSULATION") && !Parameters.GetParameterAsBool(s, "HARDWARE_DETAIL"))).ToList<Element>();
      }
      if (taskDialogResult.Equals((object) (TaskDialogResult) 1003))
      {
        InsulationSelectionFilter insulationSelectionFilter = new InsulationSelectionFilter();
        IList<Reference> source2 = sel.PickObjects((ObjectType) 1, (ISelectionFilter) insulationSelectionFilter, "Select Insulation");
        if (source2.Count<Reference>() <= 0)
        {
          insulationNotSelected = true;
          return (IEnumerable<Element>) null;
        }
        ICollection<ElementId> elementIds2 = (ICollection<ElementId>) new List<ElementId>();
        foreach (Reference reference in (IEnumerable<Reference>) source2)
        {
          Element element = revitdoc.GetElement(reference);
          elementIds2.Add(element.Id);
        }
        int num2 = elementIds2.Count<ElementId>();
        source1 = new FilteredElementCollector(revitdoc, elementIds2).OfClass(typeof (FamilyInstance)).ToList<Element>();
        int num3 = source1.Count<Element>();
        if (num2 != num3)
          return (IEnumerable<Element>) null;
      }
      if (taskDialogResult.Equals((object) (TaskDialogResult) 1002))
      {
        View activeView = revitdoc.ActiveView;
        ElementMulticategoryFilter multicategoryFilter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_SpecialityEquipment,
          BuiltInCategory.OST_GenericModel
        });
        FilteredElementCollector elementCollector = new FilteredElementCollector(revitdoc, activeView.Id).OfClass(typeof (FamilyInstance));
        zeroVols = new List<Element>();
        ElementMulticategoryFilter filter = multicategoryFilter;
        source1 = elementCollector.WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (s => Parameters.GetParameterAsString(s, "MANUFACTURE_COMPONENT").ToUpper().ToUpper().Contains("INSULATION") && !Parameters.GetParameterAsBool(s, "HARDWARE_DETAIL"))).ToList<Element>();
      }
    }
    foreach (Element elem in source1)
    {
      if (elem.GetElementVolume() <= 0.0)
        zeroVols.Add(elem);
    }
    foreach (Element element in zeroVols)
    {
      if (source1.Contains(element))
        source1.Remove(element);
    }
    return (IEnumerable<Element>) source1;
  }

  public static Dictionary<string, MarkGroupResultData> RunInitialComparison(
    UIDocument uiDoc,
    Document revitdoc,
    IEnumerable<FamilyInstance> SFElementsToProcess,
    List<string> availableMarks,
    int highestMarkUsed,
    Dictionary<string, MarkGroupResultData> ifdictionaryexists,
    bool wholeModelSelected)
  {
    int num = 0;
    bool flag1 = false;
    InsulationMarking._uiDoc = uiDoc;
    Document document = InsulationMarking._uiDoc.Document;
    List<FamilyInstance> familyInstanceList = new List<FamilyInstance>();
    InsulationMarking._toleranceComparer = new MKTolerances(ComparisonOption.DoNotRound, document);
    Dictionary<string, MarkGroupResultData> dictionary = ifdictionaryexists;
    if (dictionary.Count > 0)
    {
      SFElementsToProcess.ToList<FamilyInstance>();
      foreach (FamilyInstance familyInstance in SFElementsToProcess)
      {
        if (InsulationMarking.isUnlocked(familyInstance))
          familyInstanceList.Add(familyInstance);
      }
    }
    else
      familyInstanceList = SFElementsToProcess.ToList<FamilyInstance>();
    foreach (FamilyInstance familyInstance in familyInstanceList)
    {
      FamilyInstance incomingInstance = familyInstance;
      bool flag2 = false;
      foreach (KeyValuePair<string, MarkGroupResultData> keyValuePair in dictionary)
      {
        string reason = "";
        if (keyValuePair.Value.GroupMembers.Any<FamilyInstance>())
        {
          List<FamilyInstance> groupMembers = keyValuePair.Value.GroupMembers;
          if (!groupMembers.Any<FamilyInstance>((Func<FamilyInstance, bool>) (e => !e.Symbol.FamilyName.Equals(incomingInstance.Symbol.FamilyName))) && !groupMembers.Any<FamilyInstance>((Func<FamilyInstance, bool>) (e => !e.LookupParameter("Material").AsElementId().Equals((object) incomingInstance.LookupParameter("Material").AsElementId()))) && !groupMembers.Any<FamilyInstance>((Func<FamilyInstance, bool>) (e => !e.GetTypeId().Equals((object) incomingInstance.GetTypeId()))) && InsulationMarking.TestsPassed(InsulationMarking.RunComparisonTestsOnInstances(document, groupMembers, incomingInstance, out reason)))
          {
            flag2 = true;
            if (!keyValuePair.Value.GroupMembers.Select<FamilyInstance, string>((Func<FamilyInstance, string>) (s => s.UniqueId)).Contains<string>(incomingInstance.UniqueId))
            {
              keyValuePair.Value.GroupMembers.Add(incomingInstance);
              break;
            }
            break;
          }
        }
      }
      if (!flag2)
      {
        if (num >= highestMarkUsed)
          flag1 = true;
        if (wholeModelSelected | flag1)
        {
          ++num;
          while (dictionary.Keys.Contains<string>(num.ToString()))
            ++num;
          dictionary.Add(num.ToString(), new MarkGroupResultData()
          {
            GroupMembers = {
              incomingInstance
            }
          });
        }
        else
        {
          for (++num; !availableMarks.Contains(num.ToString()); ++num)
          {
            if (num >= highestMarkUsed)
            {
              ++num;
              break;
            }
          }
          while (dictionary.Keys.Contains<string>(num.ToString()))
            ++num;
          dictionary.Add(num.ToString(), new MarkGroupResultData()
          {
            GroupMembers = {
              incomingInstance
            }
          });
        }
      }
    }
    return dictionary;
  }

  public static int GetExistingMarks(Document revitDoc, out List<string> availableMarks)
  {
    availableMarks = new List<string>();
    List<Element> elementList = new List<Element>();
    List<string> source = new List<string>();
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_SpecialityEquipment,
      BuiltInCategory.OST_GenericModel
    });
    foreach (Element element in new FilteredElementCollector(revitDoc).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (s => Parameters.GetParameterAsString(s, "MANUFACTURE_COMPONENT").ToUpper().ToUpper().Contains("INSULATION"))).ToList<Element>())
    {
      string str = element.LookupParameter("INSULATION_MARK").AsString();
      if (!source.Contains(str) && str != null && str != "")
        source.Add(str);
    }
    int result1 = 0;
    int num = source.Count<string>();
    if (num > 1)
      source.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    bool flag = true;
    if (num != 0)
      flag = int.TryParse(source.Last<string>(), out result1);
    if (!flag && num > 2)
    {
      for (int index = num - 2; index >= 0; --index)
      {
        int result2 = 0;
        if (int.TryParse(source[index], out result2))
        {
          result1 = result2;
          break;
        }
      }
    }
    for (int index = 1; index < result1; ++index)
    {
      if (!source.Contains(index.ToString()))
        availableMarks.Add(index.ToString());
    }
    return result1;
  }

  private static List<TestResult> RunComparisonTestsOnInstances(
    Document revitDoc,
    List<FamilyInstance> standards,
    FamilyInstance famInst,
    out string reason)
  {
    List<TestResult> testResultList = new List<TestResult>();
    reason = "";
    foreach (FamilyInstance standard in standards)
    {
      if (!InsulationMarking.RunGeometryComparisonOnInsulation(standard, famInst, false, false, true))
      {
        TestResult testResult = new TestResult(MKTest.Geometry, false, standard.Id, famInst.Id);
        testResultList.Add(testResult);
        return testResultList;
      }
      testResultList.Add(new TestResult(MKTest.AddonLocation, true));
      testResultList = new List<TestResult>();
    }
    testResultList.Add(new TestResult(MKTest.Geometry, true));
    return testResultList;
  }

  private static bool RunGeometryComparisonOnInsulation(
    FamilyInstance standard,
    FamilyInstance other,
    bool addonLocation,
    bool embededLocation,
    bool solid)
  {
    return InsulationMarking.GetInsulationVerificationFamily(standard).InsulationMatches(InsulationMarking.GetInsulationVerificationFamily(other));
  }

  private static InsulationVerificationFamily GetInsulationVerificationFamily(
    FamilyInstance famInst,
    bool bVisualize = false)
  {
    if (InsulationMarking._GeomVerificationStore.ContainsKey(famInst.UniqueId))
      return InsulationMarking._GeomVerificationStore[famInst.UniqueId];
    InsulationVerificationFamily verificationFamily = new InsulationVerificationFamily(famInst, InsulationMarking._MVData);
    InsulationMarking._GeomVerificationStore.Add(famInst.UniqueId, verificationFamily);
    return verificationFamily;
  }

  private static bool TestsPassed(List<TestResult> results)
  {
    return results != null && results.Any<TestResult>() && results.Last<TestResult>().Passed;
  }

  public static bool isLocked(FamilyInstance familyInstance)
  {
    int num = Parameters.GetParameterAsBool((Element) familyInstance, "INSULATION_LOCK") ? 1 : 0;
    string parameterAsString = Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_HOST_GUID");
    return num != 0 && parameterAsString.Equals(familyInstance.UniqueId);
  }

  public static bool isUnlocked(FamilyInstance familyInstance)
  {
    int num = Parameters.GetParameterAsBool((Element) familyInstance, "INSULATION_LOCK") ? 1 : 0;
    string parameterAsString = Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_HOST_GUID");
    return num == 0 || !parameterAsString.Equals(familyInstance.UniqueId);
  }
}
