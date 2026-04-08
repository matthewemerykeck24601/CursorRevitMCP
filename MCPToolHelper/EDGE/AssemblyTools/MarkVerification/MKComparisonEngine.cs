// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.MKComparisonEngine
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.AssemblyTools.MarkVerification.GeometryComparer;
using EDGE.AssemblyTools.MarkVerification.InitialPresentation;
using EDGE.AssemblyTools.MarkVerification.QA;
using EDGE.AssemblyTools.MarkVerification.ResultsPresentation;
using EDGE.AssemblyTools.MarkVerification.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.ElementUtils;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification;

public class MKComparisonEngine
{
  private static MKTolerances _toleranceComparer = (MKTolerances) null;
  private static UIDocument _uiDoc;
  private static Dictionary<string, string> _userSettingDictionaryPrefix;
  private static Dictionary<string, string> _userSettingDictionarySuffix;
  private static string _incrementor;
  private static bool _letterIncrementor = false;
  private static XYZ direction = (XYZ) null;
  private static MarkVerificationData _MVData;
  private static Dictionary<string, GeometryVerificationFamily> _GeomVerificationStore = new Dictionary<string, GeometryVerificationFamily>();
  public static Dictionary<string, List<Autodesk.Revit.DB.ElementId>> allthePlates = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
  public static Dictionary<string, List<Autodesk.Revit.DB.ElementId>> excludedPlates = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
  private static List<string> alphabetList = new List<string>()
  {
    "A",
    "B",
    "C",
    "D",
    "E",
    "F",
    "G",
    "H",
    "I",
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
  public static MarkQA MarkQACopy;
  private static bool cancelOut = false;
  public static Guid MemberVolumeCastParameterGuid = Guid.Empty;
  public static Guid ConstructionProductGuid = Guid.Empty;
  public static Dictionary<string, Guid> parameterGuidDictionary = new Dictionary<string, Guid>();

  public static List<MarkResult> RunMarkVerifictionOnExisting(
    UIDocument uiDoc,
    IEnumerable<IGrouping<string, Autodesk.Revit.DB.FamilyInstance>> groupedSFElementsToVerify,
    MarkQA copy)
  {
    MKComparisonEngine.cancelOut = false;
    MKComparisonEngine._MVData = new MarkVerificationData(uiDoc.Document, ComparisonOption.DoNotRound);
    if (MKComparisonEngine._MVData.cancel)
      return (List<MarkResult>) null;
    if (!MKComparisonEngine._MVData.IsValidForMarkVerification)
      return (List<MarkResult>) null;
    MKComparisonEngine.parameterGuidDictionary = new Dictionary<string, Guid>();
    MKComparisonEngine.MemberVolumeCastParameterGuid = Guid.Empty;
    MKComparisonEngine.ConstructionProductGuid = Guid.Empty;
    MKComparisonEngine.MarkQACopy = new MarkQA(copy);
    MKComparisonEngine._GeomVerificationStore.Clear();
    groupedSFElementsToVerify.Select<IGrouping<string, Autodesk.Revit.DB.FamilyInstance>, string>((Func<IGrouping<string, Autodesk.Revit.DB.FamilyInstance>, string>) (e => e.Key)).ToList<string>();
    MKComparisonEngine._uiDoc = uiDoc;
    MKComparisonEngine._toleranceComparer = new MKTolerances(ComparisonOption.DoNotRound, MKComparisonEngine._uiDoc.Document);
    List<MarkResult> markResultList = new List<MarkResult>();
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("ComparisionResults:");
    foreach (IGrouping<string, Autodesk.Revit.DB.FamilyInstance> grp in groupedSFElementsToVerify)
    {
      MKComparisonEngine.allthePlates.Clear();
      MKComparisonEngine.excludedPlates.Clear();
      string reason = "";
      MarkResult markResult = MKComparisonEngine.CompareSameMarks(grp, out reason);
      if (markResult == null)
        return (List<MarkResult>) null;
      markResultList.Add(markResult);
      TestResult testResult = markResult.TestResults.LastOrDefault<TestResult>();
      if (testResult != null)
        stringBuilder.AppendLine($"    {grp.Key}: {(testResult.Passed ? "PASS" : "FAIL -> " + reason)}");
    }
    return markResultList;
  }

  private static void DetailFunction(List<TestResult> testResults)
  {
    foreach (TestResult testResult in testResults)
    {
      Autodesk.Revit.DB.ElementId structuralFramingId1 = testResult.StandardStructuralFramingId;
      Autodesk.Revit.DB.FamilyInstance element1 = MKComparisonEngine._uiDoc.Document.GetElement(structuralFramingId1) as Autodesk.Revit.DB.FamilyInstance;
      Autodesk.Revit.DB.ElementId structuralFramingId2 = testResult.FailingStructuralFramingId;
      Autodesk.Revit.DB.FamilyInstance element2 = MKComparisonEngine._uiDoc.Document.GetElement(structuralFramingId2) as Autodesk.Revit.DB.FamilyInstance;
      Dictionary<string, List<string>> dictionary1 = new Dictionary<string, List<string>>();
      string testName = testResult.TestName;
      if (testName != null)
      {
        switch (testName.Length)
        {
          case 23:
            switch (testName[8])
            {
              case 'A':
                if (testName == "Compare Addon Locations")
                {
                  List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> addonLocationList = MKComparisonEngine.FinishLocation(element1, element2);
                  if (addonLocationList == null)
                    return;
                  List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> collection = MKComparisonEngine.AddonLocation(element1, element2);
                  if (collection == null)
                    return;
                  addonLocationList.AddRange((IEnumerable<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) collection);
                  addonLocationList.Sort((Comparison<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.FamilyName, q.FamilyName)));
                  testResult.Locations = addonLocationList;
                  continue;
                }
                continue;
              case 'M':
                if (testName == "Compare Member Geometry")
                  continue;
                continue;
              case 'P':
                if (testName == "Compare Plate Locations")
                {
                  List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> addonLocationList = MKComparisonEngine.EmbedLocation(element1, element2);
                  if (addonLocationList == null)
                    return;
                  addonLocationList.Sort((Comparison<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.FamilyName, q.FamilyName)));
                  testResult.Locations = addonLocationList;
                  continue;
                }
                continue;
              case 'y':
                if (testName == "Family Types Comparison")
                {
                  string str1 = $"{element1.Symbol.FamilyName} - {element1.Symbol.Name}";
                  string str2 = $"{element2.Symbol.FamilyName} - {element2.Symbol.Name}";
                  testResult.ActualResult = str2;
                  testResult.Expectedfailingreason = str1;
                  continue;
                }
                continue;
              default:
                continue;
            }
          case 25:
            if (testName == "Compare Family Parameters")
            {
              Dictionary<string, List<string>> dictionary2 = MKComparisonEngine.checkParameters(element1, element2);
              MVEEnhancedDetails mveEnhancedDetails = new MVEEnhancedDetails();
              mveEnhancedDetails.IdOfElement = testResult.FailingStructuralFramingId;
              mveEnhancedDetails.ParametersAndValues = new Dictionary<string, List<string>>();
              List<string> stringList = new List<string>();
              mveEnhancedDetails.StandardId = testResult.StandardStructuralFramingId;
              foreach (string key in dictionary2.Keys)
              {
                dictionary1.Add(key, new List<string>()
                {
                  dictionary2[key].ElementAt<string>(0),
                  dictionary2[key].ElementAt<string>(1)
                });
                stringList.Add(key);
              }
              stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
              foreach (string key in stringList)
                mveEnhancedDetails.ParametersAndValues.Add(key, dictionary1[key]);
              testResult.MVE = mveEnhancedDetails;
              continue;
            }
            continue;
          case 29:
            if (testName == "Compare Main Material Volumes")
            {
              Dictionary<string, List<string>> dictionary3 = MKComparisonEngine.checkVolume(element1, element2);
              CompareVolume compareVolume = new CompareVolume();
              compareVolume.IdOfElement = testResult.FailingStructuralFramingId;
              compareVolume.StandardId = testResult.StandardStructuralFramingId;
              foreach (string key in dictionary3.Keys)
                dictionary1.Add(key, new List<string>()
                {
                  dictionary3[key].ElementAt<string>(0),
                  dictionary3[key].ElementAt<string>(1)
                });
              compareVolume.ParametersAndValues = dictionary1;
              testResult.CV = compareVolume;
              continue;
            }
            continue;
          case 37:
            if (testName == "Compare Plate Family Types and Counts")
            {
              List<PlateTypes> plateTypesList = MKComparisonEngine.PlateNames(element1, element2);
              if (plateTypesList == null)
                return;
              plateTypesList.Sort((Comparison<PlateTypes>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.FamilyName, q.FamilyName)));
              testResult.PlateTypes = plateTypesList;
              continue;
            }
            continue;
          case 48 /*0x30*/:
            if (testName == "Addon Family Types, Counts, and Material Volumes")
            {
              List<dataForAddonTest> dataForAddonTestList = MKComparisonEngine.AddonFinish(element1, element2);
              dataForAddonTestList.Sort((Comparison<dataForAddonTest>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.FamilyTypeName, q.FamilyTypeName)));
              testResult.DataForAddOn = dataForAddonTestList;
              continue;
            }
            continue;
          default:
            continue;
        }
      }
    }
  }

  private static Dictionary<string, List<string>> checkVolume(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance incoming)
  {
    Units units = MKComparisonEngine._uiDoc.Document.GetUnits();
    Dictionary<string, List<string>> failedparams = new Dictionary<string, List<string>>();
    MKComparisonEngine.SpecificParameters(standard, incoming, MKToleranceAspect.Volume, out failedparams);
    ICollection<Autodesk.Revit.DB.ElementId> materialIds1 = standard.GetMaterialIds(false);
    ICollection<Autodesk.Revit.DB.ElementId> materialIds2 = incoming.GetMaterialIds(false);
    foreach (Autodesk.Revit.DB.ElementId elementId in (IEnumerable<Autodesk.Revit.DB.ElementId>) materialIds1)
    {
      List<string> stringList = new List<string>();
      double materialVolume1 = standard.GetMaterialVolume(elementId);
      string str1 = UnitFormatUtils.Format(units, SpecTypeId.Volume, materialVolume1, false);
      if (materialIds2.Contains(elementId))
      {
        double materialVolume2 = incoming.GetMaterialVolume(elementId);
        if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(materialVolume1, materialVolume2, MKToleranceAspect.Volume))
        {
          string str2 = UnitFormatUtils.Format(units, SpecTypeId.Volume, materialVolume2, false);
          stringList.Add(str1);
          stringList.Add(str2);
          Autodesk.Revit.DB.Element element = MKComparisonEngine._uiDoc.Document.GetElement(elementId);
          failedparams.Add(element.Name, stringList);
        }
      }
      else
      {
        string name = MKComparisonEngine._uiDoc.ActiveView.Document.GetElement(elementId).Name;
        stringList.Add(str1);
        stringList.Add("No Matching Material Found");
        failedparams.Add(name, stringList);
      }
    }
    if (!failedparams.ContainsKey("MEMBER_VOLUME_CAST"))
    {
      Parameter parameter1 = standard.LookupParameter("MEMBER_VOLUME_CAST");
      Parameter parameter2 = incoming.LookupParameter("MEMBER_VOLUME_CAST");
      if (parameter1 == null || parameter2 == null)
        throw new Exception("MVCNULL");
      if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(parameter1.AsDouble(), parameter2.AsDouble(), MKToleranceAspect.Volume))
        failedparams.Add("MEMBER_VOLUME_CAST", new List<string>()
        {
          $"{parameter1.AsDouble().ToString()} {parameter1.GetUnitTypeId().ToString()}",
          $"{parameter2.AsDouble().ToString()} {parameter2.GetUnitTypeId().ToString()}"
        });
    }
    return failedparams;
  }

  private static Dictionary<string, List<string>> checkParameters(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance incoming)
  {
    Dictionary<string, List<string>> failedparams1 = new Dictionary<string, List<string>>();
    Dictionary<string, List<string>> failedparams2 = new Dictionary<string, List<string>>();
    Dictionary<string, List<string>> failedparams3 = new Dictionary<string, List<string>>();
    Dictionary<string, List<string>> failedparams4 = new Dictionary<string, List<string>>();
    Dictionary<string, List<string>> failedparams5 = new Dictionary<string, List<string>>();
    Dictionary<string, List<string>> dictionary1 = new Dictionary<string, List<string>>();
    Autodesk.Revit.DB.FamilyInstance standard1 = standard.SuperComponent != null ? standard.SuperComponent as Autodesk.Revit.DB.FamilyInstance : standard;
    Autodesk.Revit.DB.FamilyInstance incoming1 = incoming.SuperComponent != null ? incoming.SuperComponent as Autodesk.Revit.DB.FamilyInstance : incoming;
    MKComparisonEngine.SpecificParameters(standard1, incoming1, MKToleranceAspect.Geometry, out failedparams1);
    Dictionary<string, List<string>> dictionary2 = failedparams1;
    MKComparisonEngine.SpecificParameters(standard1, incoming1, MKToleranceAspect.FinishArea, out failedparams2);
    foreach (KeyValuePair<string, List<string>> keyValuePair in failedparams2)
      dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
    MKComparisonEngine.SpecificParameters(standard1, incoming1, MKToleranceAspect.Weight, out failedparams3);
    foreach (KeyValuePair<string, List<string>> keyValuePair in failedparams3)
      dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
    MKComparisonEngine.SpecificParameters(standard1, incoming1, MKToleranceAspect.Strength, out failedparams4);
    foreach (KeyValuePair<string, List<string>> keyValuePair in failedparams4)
      dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
    MKComparisonEngine.SpecificParameters(standard1, incoming1, MKToleranceAspect.Text, out failedparams5);
    foreach (KeyValuePair<string, List<string>> keyValuePair in failedparams5)
      dictionary2.Add(keyValuePair.Key, keyValuePair.Value);
    return dictionary2;
  }

  private static bool SpecificParameters(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance incoming,
    MKToleranceAspect compareType,
    out Dictionary<string, List<string>> failedparams)
  {
    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
    List<string> parameterNameStrings;
    switch (compareType)
    {
      case MKToleranceAspect.Volume:
        parameterNameStrings = MKUtils.VolumeParameterNameStrings;
        break;
      case MKToleranceAspect.FinishArea:
        parameterNameStrings = MKUtils.AreaParameterNameStrings;
        break;
      case MKToleranceAspect.Weight:
        parameterNameStrings = MKUtils.WeightParameterNameStrings;
        break;
      case MKToleranceAspect.Strength:
        parameterNameStrings = MKUtils.StrengthParameterNameStrings;
        break;
      case MKToleranceAspect.Geometry:
        parameterNameStrings = MKUtils.LengthParameterNameStrings;
        break;
      default:
        parameterNameStrings = MKUtils.TextParameterNameStrings;
        break;
    }
    Document document = standard.GetTopLevelElement().Document;
    foreach (string str1 in parameterNameStrings)
    {
      Parameter parameter1 = standard.LookupParameter(str1);
      Parameter parameter2 = incoming.LookupParameter(str1);
      List<string> stringList = new List<string>();
      Units units = document.GetUnits();
      if (parameter1 == null && parameter2 != null || parameter1 != null && parameter2 == null)
      {
        stringList.Add(parameter1.AsString());
        stringList.Add(parameter2.AsString());
        dictionary.Add(str1, stringList);
        failedparams = dictionary;
        if (parameterNameStrings.Last<string>().Equals(str1))
          return false;
      }
      if (parameter1 != null || parameter2 != null)
      {
        double val1 = UnitUtils.ConvertFromInternalUnits(parameter1.AsDouble(), UnitTypeId.PoundsForce);
        double val2 = UnitUtils.ConvertFromInternalUnits(parameter2.AsDouble(), UnitTypeId.PoundsForce);
        if ((compareType != MKToleranceAspect.Weight || !MKComparisonEngine._toleranceComparer.NewValuesAreEqual(val1, val2, compareType)) && (compareType == MKToleranceAspect.Weight || compareType == MKToleranceAspect.Text || !MKComparisonEngine._toleranceComparer.NewValuesAreEqual(parameter1.AsDouble(), parameter2.AsDouble(), compareType)) && (compareType != MKToleranceAspect.Text || !(parameter1.AsString() == parameter2.AsString())))
        {
          if (compareType == MKToleranceAspect.Text)
          {
            stringList.Add(parameter1.AsString());
            stringList.Add(parameter2.AsString());
            dictionary.Add(str1, stringList);
            failedparams = dictionary;
          }
          else
          {
            Autodesk.Revit.DB.ForgeTypeId dataType = parameter1.Definition.GetDataType();
            string str2 = UnitFormatUtils.Format(units, dataType, parameter1.AsDouble(), false);
            string str3 = UnitFormatUtils.Format(units, dataType, parameter2.AsDouble(), false);
            stringList.Add(str2);
            stringList.Add(str3);
            dictionary.Add(str1, stringList);
            failedparams = dictionary;
          }
          if (parameterNameStrings.Last<string>().Equals(str1))
            return false;
        }
      }
    }
    failedparams = dictionary;
    return true;
  }

  private static List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> EmbedLocation(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance incoming)
  {
    GeometryVerificationFamily verificationFamily1 = MKComparisonEngine.GetGeometryVerificationFamily(standard);
    if (verificationFamily1 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) null;
    }
    GeometryVerificationFamily verificationFamily2 = MKComparisonEngine.GetGeometryVerificationFamily(incoming);
    if (verificationFamily2 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) null;
    }
    Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary1 = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
    List<EDGEEmbedComponent> embeds1 = verificationFamily2.Embeds;
    List<EDGEEmbedComponent> embeds2 = verificationFamily1.Embeds;
    Dictionary<int, List<EDGEAssemblyComponent_Base>> dictionary2 = new Dictionary<int, List<EDGEAssemblyComponent_Base>>();
    for (int key = 0; key < 4; ++key)
    {
      List<MVEfailedResults> mvEfailedResultsList = new List<MVEfailedResults>();
      List<EDGEAssemblyComponent_Base> failedAddons;
      LocationCompareResult locationCompareResult = MKComparisonEngine.CompareEmbed(verificationFamily1, verificationFamily2, out failedAddons);
      if (failedAddons.Count > 0)
        dictionary2.Add(key, failedAddons);
      if (locationCompareResult.Equals((object) LocationCompareResult.Failed))
        verificationFamily2.Rotate90ForTest();
    }
    List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> addonLocationList = new List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>();
    if (dictionary2.Count > 0)
    {
      List<EDGEAssemblyComponent_Base> source1 = dictionary2.Values.OrderBy<List<EDGEAssemblyComponent_Base>, int>((Func<List<EDGEAssemblyComponent_Base>, int>) (e => e.Count)).FirstOrDefault<List<EDGEAssemblyComponent_Base>>();
      if (source1.Count != 0)
      {
        List<Autodesk.Revit.DB.Element> elementList = new List<Autodesk.Revit.DB.Element>();
        foreach (IGrouping<string, EDGEAssemblyComponent_Base> source2 in source1.GroupBy<EDGEAssemblyComponent_Base, string>((Func<EDGEAssemblyComponent_Base, string>) (e => e.FamilyTypeName)))
        {
          EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation addonLocation = new EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation(MKComparisonEngine.processEmbedKey(source2.FirstOrDefault<EDGEAssemblyComponent_Base>()), source2.Select<EDGEAssemblyComponent_Base, Autodesk.Revit.DB.ElementId>((Func<EDGEAssemblyComponent_Base, Autodesk.Revit.DB.ElementId>) (e => e.Id)).ToList<Autodesk.Revit.DB.ElementId>(), false);
          addonLocationList.Add(addonLocation);
        }
      }
    }
    return addonLocationList;
  }

  private static string processEmbedKey(EDGEAssemblyComponent_Base eac)
  {
    string familyName = (MKComparisonEngine._uiDoc.Document.GetElement(eac.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName;
    string name = (MKComparisonEngine._uiDoc.Document.GetElement(eac.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name;
    return !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
  }

  private static LocationCompareResult CompareEmbed(
    GeometryVerificationFamily standard,
    GeometryVerificationFamily incoming,
    out List<EDGEAssemblyComponent_Base> failedAddons)
  {
    failedAddons = new List<EDGEAssemblyComponent_Base>();
    bool flag1 = false;
    Dictionary<string, List<EDGEAssemblyComponent_Base>> dictionary1 = new Dictionary<string, List<EDGEAssemblyComponent_Base>>();
    foreach (EDGEAssemblyComponent_Base embed in standard.Embeds)
    {
      string familyName = embed.inst.Symbol.FamilyName;
      string name = embed.inst.Symbol.Name;
      string key = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      if (dictionary1.ContainsKey(key))
        dictionary1[key].Add(embed);
      else
        dictionary1.Add(key, new List<EDGEAssemblyComponent_Base>()
        {
          embed
        });
    }
    Dictionary<string, List<EDGEAssemblyComponent_Base>> dictionary2 = new Dictionary<string, List<EDGEAssemblyComponent_Base>>();
    foreach (EDGEAssemblyComponent_Base embed in incoming.Embeds)
    {
      string familyName = embed.inst.Symbol.FamilyName;
      string name = embed.inst.Symbol.Name;
      string key = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      if (dictionary2.ContainsKey(key))
        dictionary2[key].Add(embed);
      else
        dictionary2.Add(key, new List<EDGEAssemblyComponent_Base>()
        {
          embed
        });
    }
    foreach (string key in dictionary1.Keys.Union<string>((IEnumerable<string>) dictionary2.Keys).ToList<string>())
    {
      bool flag2 = false;
      bool flag3 = false;
      MVEfailedResults mvEfailedResults = new MVEfailedResults();
      if (!dictionary1.ContainsKey(key))
        flag2 = true;
      if (!dictionary2.ContainsKey(key))
        flag3 = true;
      if (!flag2 && !flag3)
      {
        List<EDGEAssemblyComponent_Base> assemblyComponentBaseList1 = dictionary1[key];
        List<EDGEAssemblyComponent_Base> assemblyComponentBaseList2 = dictionary2[key];
        List<Autodesk.Revit.DB.ElementId> elementIdList = new List<Autodesk.Revit.DB.ElementId>();
        foreach (EDGEAssemblyComponent_Base assemblyComponentBase in assemblyComponentBaseList2)
        {
          int num = 0;
          bool flag4 = false;
          foreach (EDGEAssemblyComponent_Base other in assemblyComponentBaseList1)
          {
            LocationCompareResult locationCompareResult = assemblyComponentBase.Matches(other);
            switch (locationCompareResult)
            {
              case LocationCompareResult.PlateRotationWarning:
              case LocationCompareResult.Success:
                if (locationCompareResult == LocationCompareResult.PlateRotationWarning)
                  flag1 = true;
                flag4 = true;
                continue;
              default:
                ++num;
                continue;
            }
          }
          if (!flag4)
            failedAddons.Add(assemblyComponentBase);
        }
      }
    }
    if (failedAddons.Count > 0)
      return LocationCompareResult.Failed;
    return !flag1 ? LocationCompareResult.Success : LocationCompareResult.PlateRotationWarning;
  }

  private static List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> AddonLocation(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance incoming)
  {
    GeometryVerificationFamily verificationFamily1 = MKComparisonEngine.GetGeometryVerificationFamily(standard);
    if (verificationFamily1 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) null;
    }
    GeometryVerificationFamily verificationFamily2 = MKComparisonEngine.GetGeometryVerificationFamily(incoming);
    if (verificationFamily2 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) null;
    }
    List<EDGEAddonComponent> addons1 = verificationFamily2.Addons;
    List<EDGEAddonComponent> addons2 = verificationFamily1.Addons;
    Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary1 = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
    Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary2 = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
    Dictionary<int, List<EDGEAssemblyComponent_Base>> dictionary3 = new Dictionary<int, List<EDGEAssemblyComponent_Base>>();
    for (int key = 0; key < 4; ++key)
    {
      List<EDGEAssemblyComponent_Base> failedAddons;
      LocationCompareResult locationCompareResult = MKComparisonEngine.CompareAddons(verificationFamily1, verificationFamily2, out failedAddons);
      if (failedAddons.Count > 0)
        dictionary3.Add(key, failedAddons);
      if (locationCompareResult.Equals((object) LocationCompareResult.Failed))
        verificationFamily2.Rotate90ForTest();
    }
    List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> addonLocationList = new List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>();
    if (dictionary3.Count > 0)
    {
      List<EDGEAssemblyComponent_Base> source1 = dictionary3.Values.OrderBy<List<EDGEAssemblyComponent_Base>, int>((Func<List<EDGEAssemblyComponent_Base>, int>) (e => e.Count)).FirstOrDefault<List<EDGEAssemblyComponent_Base>>();
      if (source1.Count != 0)
      {
        List<Autodesk.Revit.DB.Element> elementList = new List<Autodesk.Revit.DB.Element>();
        foreach (IGrouping<string, EDGEAssemblyComponent_Base> source2 in source1.GroupBy<EDGEAssemblyComponent_Base, string>((Func<EDGEAssemblyComponent_Base, string>) (e => e.FamilyTypeName)))
        {
          EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation addonLocation = new EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation(MKComparisonEngine.processAddonKey(source2.FirstOrDefault<EDGEAssemblyComponent_Base>()), source2.Select<EDGEAssemblyComponent_Base, Autodesk.Revit.DB.ElementId>((Func<EDGEAssemblyComponent_Base, Autodesk.Revit.DB.ElementId>) (e => e.Id)).ToList<Autodesk.Revit.DB.ElementId>(), false);
          addonLocationList.Add(addonLocation);
        }
      }
    }
    return addonLocationList;
  }

  private static string processAddonKey(EDGEAssemblyComponent_Base eac)
  {
    string familyName = (MKComparisonEngine._uiDoc.Document.GetElement(eac.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName;
    string name = (MKComparisonEngine._uiDoc.Document.GetElement(eac.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name;
    string str1 = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
    string str2 = "";
    if ((eac as EDGEAddonComponent).MaterialId != (Autodesk.Revit.DB.ElementId) null)
      str2 = MKComparisonEngine._uiDoc.Document.GetElement((eac as EDGEAddonComponent).MaterialId).Name;
    return string.IsNullOrEmpty(str2) ? str1 : $"{str1} ({str2})";
  }

  private static LocationCompareResult CompareAddons(
    GeometryVerificationFamily standard,
    GeometryVerificationFamily incoming,
    out List<EDGEAssemblyComponent_Base> failedAddons)
  {
    failedAddons = new List<EDGEAssemblyComponent_Base>();
    List<EDGEAssemblyComponent_Base> list1 = ((IEnumerable<EDGEAssemblyComponent_Base>) standard.Addons).ToList<EDGEAssemblyComponent_Base>();
    List<EDGEAssemblyComponent_Base> list2 = ((IEnumerable<EDGEAssemblyComponent_Base>) incoming.Addons).ToList<EDGEAssemblyComponent_Base>();
    bool flag1 = false;
    Dictionary<string, List<EDGEAssemblyComponent_Base>> dictionary1 = new Dictionary<string, List<EDGEAssemblyComponent_Base>>();
    foreach (EDGEAssemblyComponent_Base assemblyComponentBase in list1)
    {
      string familyName = (MKComparisonEngine._uiDoc.Document.GetElement(assemblyComponentBase.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName;
      string name = (MKComparisonEngine._uiDoc.Document.GetElement(assemblyComponentBase.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name;
      string str1 = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      string str2 = "";
      if ((assemblyComponentBase as EDGEAddonComponent).MaterialId != (Autodesk.Revit.DB.ElementId) null)
        str2 = MKComparisonEngine._uiDoc.Document.GetElement((assemblyComponentBase as EDGEAddonComponent).MaterialId).Name;
      string key = string.IsNullOrEmpty(str2) ? str1 : $"{str1} ({str2})";
      if (dictionary1.ContainsKey(key))
        dictionary1[key].Add(assemblyComponentBase);
      else
        dictionary1.Add(key, new List<EDGEAssemblyComponent_Base>()
        {
          assemblyComponentBase
        });
    }
    Dictionary<string, List<EDGEAssemblyComponent_Base>> dictionary2 = new Dictionary<string, List<EDGEAssemblyComponent_Base>>();
    foreach (EDGEAssemblyComponent_Base assemblyComponentBase in list2)
    {
      string familyName = (MKComparisonEngine._uiDoc.Document.GetElement(assemblyComponentBase.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName;
      string name = (MKComparisonEngine._uiDoc.Document.GetElement(assemblyComponentBase.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name;
      string str3 = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      string str4 = "";
      if ((assemblyComponentBase as EDGEAddonComponent).MaterialId != (Autodesk.Revit.DB.ElementId) null)
        str4 = MKComparisonEngine._uiDoc.Document.GetElement((assemblyComponentBase as EDGEAddonComponent).MaterialId).Name;
      string key = string.IsNullOrEmpty(str4) ? str3 : $"{str3} ({str4})";
      if (dictionary2.ContainsKey(key))
        dictionary2[key].Add(assemblyComponentBase);
      else
        dictionary2.Add(key, new List<EDGEAssemblyComponent_Base>()
        {
          assemblyComponentBase
        });
    }
    List<string> list3 = dictionary1.Keys.Union<string>((IEnumerable<string>) dictionary2.Keys).ToList<string>();
    foreach (string key in list3)
    {
      MVEfailedResults mvEfailedResults = new MVEfailedResults();
      bool flag2 = false;
      bool flag3 = false;
      if (!dictionary1.ContainsKey(key))
        flag2 = true;
      if (!dictionary2.ContainsKey(key))
        flag3 = true;
      if (!flag2 && !flag3)
      {
        List<EDGEAssemblyComponent_Base> assemblyComponentBaseList1 = dictionary1[key];
        List<EDGEAssemblyComponent_Base> assemblyComponentBaseList2 = dictionary2[key];
        List<Autodesk.Revit.DB.ElementId> elementIdList = new List<Autodesk.Revit.DB.ElementId>();
        foreach (EDGEAssemblyComponent_Base assemblyComponentBase in assemblyComponentBaseList2)
        {
          int num = 0;
          bool flag4 = false;
          foreach (EDGEAssemblyComponent_Base other in assemblyComponentBaseList1)
          {
            LocationCompareResult locationCompareResult = assemblyComponentBase.Matches(other);
            switch (locationCompareResult)
            {
              case LocationCompareResult.PlateRotationWarning:
              case LocationCompareResult.Success:
                if (locationCompareResult == LocationCompareResult.PlateRotationWarning)
                  flag1 = true;
                flag4 = true;
                goto label_36;
              default:
                ++num;
                continue;
            }
          }
label_36:
          if (!flag4)
            failedAddons.Add(assemblyComponentBase);
        }
      }
    }
    if (failedAddons.Count > 0)
      return LocationCompareResult.Failed;
    return !flag1 ? LocationCompareResult.Success : LocationCompareResult.PlateRotationWarning;
  }

  private static List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> FinishLocation(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance incoming)
  {
    GeometryVerificationFamily verificationFamily1 = MKComparisonEngine.GetGeometryVerificationFamily(standard);
    if (verificationFamily1 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) null;
    }
    GeometryVerificationFamily verificationFamily2 = MKComparisonEngine.GetGeometryVerificationFamily(incoming);
    if (verificationFamily2 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>) null;
    }
    Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary1 = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
    List<EDGEFinishComponent> finishes1 = verificationFamily2.Finishes;
    List<EDGEFinishComponent> finishes2 = verificationFamily1.Finishes;
    Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary2 = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
    Dictionary<int, List<EDGEAssemblyComponent_Base>> dictionary3 = new Dictionary<int, List<EDGEAssemblyComponent_Base>>();
    foreach (EDGEFinishComponent edgeFinishComponent in finishes1)
    {
      string name = edgeFinishComponent._finish.Name;
      if (dictionary2.ContainsKey(name))
        dictionary2[name].Add(edgeFinishComponent.Id);
      else
        dictionary2.Add(name, new List<Autodesk.Revit.DB.ElementId>()
        {
          edgeFinishComponent.Id
        });
    }
    foreach (EDGEFinishComponent edgeFinishComponent in finishes2)
    {
      string name = edgeFinishComponent._finish.Name;
      if (dictionary2.ContainsKey(name))
        dictionary2[name].Add(edgeFinishComponent.Id);
      else
        dictionary2.Add(name, new List<Autodesk.Revit.DB.ElementId>()
        {
          edgeFinishComponent.Id
        });
    }
    for (int index = 0; index < 4; ++index)
    {
      List<MVEfailedResults> mveFailedResutls = new List<MVEfailedResults>();
      if (!MKComparisonEngine.CompareFinishes(verificationFamily1, verificationFamily2, out mveFailedResutls))
      {
        verificationFamily2.Rotate90ForTest();
      }
      else
      {
        foreach (MVEfailedResults mvEfailedResults in mveFailedResutls)
        {
          if (mvEfailedResults.FailedId != null)
          {
            foreach (Autodesk.Revit.DB.ElementId elementId in mvEfailedResults.FailedId)
            {
              if (mvEfailedResults.FamilyName != null && dictionary2.ContainsKey(mvEfailedResults.FamilyName))
                dictionary2[mvEfailedResults.FamilyName].Remove(elementId);
            }
            if (dictionary2.ContainsKey(mvEfailedResults.FamilyName) && dictionary2[mvEfailedResults.FamilyName].Count<Autodesk.Revit.DB.ElementId>() == 0)
              dictionary2.Remove(mvEfailedResults.FamilyName);
          }
        }
      }
    }
    List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation> addonLocationList = new List<EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation>();
    foreach (KeyValuePair<string, List<Autodesk.Revit.DB.ElementId>> keyValuePair in dictionary2)
    {
      string key = keyValuePair.Key;
      List<Autodesk.Revit.DB.ElementId> elementIds = keyValuePair.Value;
      foreach (EDGEFinishComponent edgeFinishComponent in finishes2)
      {
        if (elementIds.Contains(edgeFinishComponent.Id))
          elementIds.Remove(edgeFinishComponent.Id);
      }
      EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation addonLocation = new EDGE.AssemblyTools.MarkVerification.ResultsPresentation.AddonLocation(key, elementIds, false);
      addonLocationList.Add(addonLocation);
    }
    return addonLocationList;
  }

  private static bool CompareFinishes(
    GeometryVerificationFamily standard,
    GeometryVerificationFamily incoming,
    out List<MVEfailedResults> mveFailedResutls)
  {
    List<MVEfailedResults> mvEfailedResultsList = new List<MVEfailedResults>();
    Dictionary<string, List<EDGEFinishComponent>> dictionary1 = new Dictionary<string, List<EDGEFinishComponent>>();
    foreach (EDGEFinishComponent finish in standard.Finishes)
    {
      string name = MKComparisonEngine._uiDoc.Document.GetElement(finish.Id).Name;
      if (dictionary1.ContainsKey(name))
        dictionary1[name].Add(finish);
      else
        dictionary1.Add(name, new List<EDGEFinishComponent>()
        {
          finish
        });
    }
    Dictionary<string, List<EDGEFinishComponent>> dictionary2 = new Dictionary<string, List<EDGEFinishComponent>>();
    foreach (EDGEFinishComponent finish in incoming.Finishes)
    {
      string name = MKComparisonEngine._uiDoc.Document.GetElement(finish.Id).Name;
      if (dictionary2.ContainsKey(name))
        dictionary2[name].Add(finish);
      else
        dictionary2.Add(name, new List<EDGEFinishComponent>()
        {
          finish
        });
    }
    foreach (string key in dictionary1.Keys.Union<string>((IEnumerable<string>) dictionary2.Keys).ToList<string>())
    {
      MVEfailedResults mvEfailedResults1 = new MVEfailedResults();
      bool flag1 = false;
      bool flag2 = false;
      string str = key;
      if (!dictionary1.ContainsKey(key))
        flag1 = true;
      if (!dictionary2.ContainsKey(key))
        flag2 = true;
      if (!flag1 && !flag2)
      {
        List<EDGEFinishComponent> edgeFinishComponentList1 = dictionary1[key];
        List<EDGEFinishComponent> edgeFinishComponentList2 = dictionary2[key];
        List<Autodesk.Revit.DB.ElementId> elementIdList = new List<Autodesk.Revit.DB.ElementId>();
        bool flag3 = false;
        foreach (EDGEFinishComponent edgeFinishComponent in edgeFinishComponentList2)
        {
          MVEfailedResults mvEfailedResults2 = new MVEfailedResults();
          using (List<EDGEFinishComponent>.Enumerator enumerator1 = edgeFinishComponentList1.GetEnumerator())
          {
label_29:
            while (enumerator1.MoveNext())
            {
              EDGEFinishComponent current = enumerator1.Current;
              IEnumerable<TransformableSolid> transformableSolids = (IEnumerable<TransformableSolid>) standard.SolidsByFinish[current.Id].OrderByDescending<TransformableSolid, double>((Func<TransformableSolid, double>) (i => i.Volume));
              IOrderedEnumerable<TransformableSolid> orderedEnumerable = incoming.SolidsByFinish[edgeFinishComponent.Id].OrderByDescending<TransformableSolid, double>((Func<TransformableSolid, double>) (i => i.Volume));
              IEnumerator<TransformableSolid> enumerator2 = transformableSolids.GetEnumerator();
              IEnumerator<TransformableSolid> enumerator3 = orderedEnumerable.GetEnumerator();
              do
              {
                if (!enumerator2.MoveNext() || !enumerator3.MoveNext())
                  goto label_29;
              }
              while (!enumerator2.Current.FinishEquals(enumerator3.Current));
              flag3 = true;
              elementIdList.Add(edgeFinishComponent.Id);
              elementIdList.Add(current.Id);
              mvEfailedResults2.FailedId = elementIdList;
              mvEfailedResults2.FamilyName = str;
              mvEfailedResults1 = mvEfailedResults2;
              mvEfailedResultsList.Add(mvEfailedResults1);
            }
          }
        }
        mveFailedResutls = mvEfailedResultsList;
        if (!flag3)
          return false;
      }
      else
      {
        int num = !flag1 & flag2 ? 1 : 0;
      }
      mvEfailedResultsList.Add(mvEfailedResults1);
    }
    mveFailedResutls = mvEfailedResultsList;
    return true;
  }

  private static List<PlateTypes> PlateNames(Autodesk.Revit.DB.FamilyInstance standard, Autodesk.Revit.DB.FamilyInstance incoming)
  {
    int familyCount = 0;
    int actualCount = 0;
    string message = (string) null;
    List<PlateTypes> plateTypesList = new List<PlateTypes>();
    GeometryVerificationFamily verificationFamily1 = MKComparisonEngine.GetGeometryVerificationFamily(standard);
    if (verificationFamily1 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<PlateTypes>) null;
    }
    GeometryVerificationFamily verificationFamily2 = MKComparisonEngine.GetGeometryVerificationFamily(incoming);
    if (verificationFamily2 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return (List<PlateTypes>) null;
    }
    List<Autodesk.Revit.DB.ElementId> elementIdList1 = new List<Autodesk.Revit.DB.ElementId>();
    List<Autodesk.Revit.DB.ElementId> elementIdList2 = new List<Autodesk.Revit.DB.ElementId>();
    Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary1 = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
    foreach (EDGEEmbedComponent embed in verificationFamily1.Embeds)
    {
      string familyName = embed.inst.Symbol.FamilyName;
      string name = embed.inst.Symbol.Name;
      string key = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      Autodesk.Revit.DB.ElementId id = embed.Id;
      if (dictionary1.ContainsKey(key))
        dictionary1[key].Add(id);
      else
        dictionary1.Add(key, new List<Autodesk.Revit.DB.ElementId>() { id });
    }
    Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary2 = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
    foreach (EDGEEmbedComponent embed in verificationFamily2.Embeds)
    {
      string familyName = embed.inst.Symbol.FamilyName;
      string name = embed.inst.Symbol.Name;
      string key = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      Autodesk.Revit.DB.ElementId id = embed.Id;
      if (dictionary2.ContainsKey(key))
        dictionary2[key].Add(id);
      else
        dictionary2.Add(key, new List<Autodesk.Revit.DB.ElementId>() { id });
    }
    foreach (string key in dictionary1.Keys.Union<string>((IEnumerable<string>) dictionary2.Keys).ToList<string>())
    {
      bool flag1 = false;
      bool flag2 = false;
      List<Autodesk.Revit.DB.ElementId> expectedList = new List<Autodesk.Revit.DB.ElementId>();
      List<Autodesk.Revit.DB.ElementId> actualList = new List<Autodesk.Revit.DB.ElementId>();
      bool flag3 = false;
      string familyName = key;
      if (!dictionary1.ContainsKey(key))
        flag1 = true;
      if (!dictionary2.ContainsKey(key))
        flag2 = true;
      if (!flag1 && !flag2)
      {
        foreach (Autodesk.Revit.DB.ElementId elementId in dictionary1[key])
          actualList.Add(elementId);
        foreach (Autodesk.Revit.DB.ElementId elementId in dictionary2[key])
          expectedList.Add(elementId);
        if (!dictionary1[key].Count<Autodesk.Revit.DB.ElementId>().Equals(dictionary2[key].Count<Autodesk.Revit.DB.ElementId>()))
        {
          familyCount = dictionary1[key].Count<Autodesk.Revit.DB.ElementId>();
          actualCount = dictionary2[key].Count<Autodesk.Revit.DB.ElementId>();
        }
        else
          flag3 = true;
      }
      else if (flag1 && !flag2)
      {
        foreach (Autodesk.Revit.DB.ElementId elementId in dictionary2[key])
          expectedList.Add(elementId);
        familyCount = 0;
        actualCount = dictionary2[key].Count<Autodesk.Revit.DB.ElementId>();
      }
      else
      {
        foreach (Autodesk.Revit.DB.ElementId elementId in dictionary1[key])
          actualList.Add(elementId);
        familyCount = dictionary1[key].Count<Autodesk.Revit.DB.ElementId>();
        message = "No Matching Finish found";
        actualCount = 0;
      }
      if (!flag3)
      {
        PlateTypes plateTypes = new PlateTypes(familyName, actualCount, familyCount, message, actualList, expectedList);
        plateTypesList.Add(plateTypes);
      }
    }
    return plateTypesList;
  }

  private static List<dataForAddonTest> AddonFinish(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance incoming)
  {
    bool volumePass1 = true;
    List<dataForAddonTest> dataForAddonTestList1 = new List<dataForAddonTest>();
    List<EDGEFinishComponent> finishesForHost1 = MKComparisonEngine._MVData.GetFinishesForHost(standard);
    List<EDGEFinishComponent> finishesForHost2 = MKComparisonEngine._MVData.GetFinishesForHost(incoming);
    Dictionary<string, List<EDGEFinishComponent>> dictionary1 = new Dictionary<string, List<EDGEFinishComponent>>();
    foreach (EDGEFinishComponent edgeFinishComponent in finishesForHost1)
    {
      string name = edgeFinishComponent._finish.Name;
      if (dictionary1.ContainsKey(name))
        dictionary1[name].Add(edgeFinishComponent);
      else
        dictionary1.Add(name, new List<EDGEFinishComponent>()
        {
          edgeFinishComponent
        });
    }
    Dictionary<string, List<EDGEFinishComponent>> dictionary2 = new Dictionary<string, List<EDGEFinishComponent>>();
    foreach (EDGEFinishComponent edgeFinishComponent in finishesForHost2)
    {
      string name = edgeFinishComponent._finish.Name;
      if (dictionary2.ContainsKey(name))
        dictionary2[name].Add(edgeFinishComponent);
      else
        dictionary2.Add(name, new List<EDGEFinishComponent>()
        {
          edgeFinishComponent
        });
    }
    foreach (string key in dictionary1.Keys.Union<string>((IEnumerable<string>) dictionary2.Keys).ToList<string>())
    {
      bool flag1 = false;
      bool flag2 = false;
      string familyTypeName = key;
      string message = "";
      bool addonPass = false;
      List<Autodesk.Revit.DB.ElementId> elementIdList1 = new List<Autodesk.Revit.DB.ElementId>();
      List<Autodesk.Revit.DB.ElementId> elementIdList2 = new List<Autodesk.Revit.DB.ElementId>();
      List<Autodesk.Revit.DB.ElementId> mismatchedId = new List<Autodesk.Revit.DB.ElementId>();
      if (!dictionary1.ContainsKey(key))
        flag1 = true;
      if (!dictionary2.ContainsKey(key))
        flag2 = true;
      int familycount;
      int actualcount;
      if (!flag1 && !flag2)
      {
        List<EDGEFinishComponent> list1 = dictionary1[key].OrderBy<EDGEFinishComponent, double>((Func<EDGEFinishComponent, double>) (s => s.MaterialVolume_InInternalUnits)).ToList<EDGEFinishComponent>();
        List<EDGEFinishComponent> list2 = dictionary2[key].OrderBy<EDGEFinishComponent, double>((Func<EDGEFinishComponent, double>) (s => s.MaterialVolume_InInternalUnits)).ToList<EDGEFinishComponent>();
        List<EDGEFinishComponent> edgeFinishComponentList = new List<EDGEFinishComponent>();
        familycount = dictionary1[key].Count<EDGEFinishComponent>();
        actualcount = dictionary2[key].Count<EDGEFinishComponent>();
        if (list1.Count >= list2.Count)
        {
          int count1 = list1.Count;
        }
        else
        {
          int count2 = list2.Count;
        }
        foreach (EDGEFinishComponent edgeFinishComponent1 in list2)
        {
          Autodesk.Revit.DB.ElementId id1 = edgeFinishComponent1.Id;
          if (!elementIdList2.Contains(id1))
            elementIdList2.Add(id1);
          foreach (EDGEFinishComponent edgeFinishComponent2 in list1)
          {
            Autodesk.Revit.DB.ElementId id2 = edgeFinishComponent2.Id;
            if (!elementIdList1.Contains(id2))
              elementIdList1.Add(id2);
            if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(edgeFinishComponent1.MaterialVolume_InInternalUnits, edgeFinishComponent2.MaterialVolume_InInternalUnits, MKToleranceAspect.AddonVolume))
            {
              if (!mismatchedId.Contains(edgeFinishComponent1.Id))
                mismatchedId.Add(edgeFinishComponent1.Id);
              volumePass1 = false;
              if (edgeFinishComponentList.Contains(edgeFinishComponent1))
                edgeFinishComponentList.Remove(edgeFinishComponent1);
            }
            else if (!edgeFinishComponentList.Contains(edgeFinishComponent1))
              edgeFinishComponentList.Add(edgeFinishComponent1);
          }
        }
        if (elementIdList2.Count<Autodesk.Revit.DB.ElementId>() != elementIdList1.Count<Autodesk.Revit.DB.ElementId>())
          message = "Count Mismatch";
      }
      else if (flag1 && !flag2)
      {
        familycount = 0;
        actualcount = dictionary2[key].Count<EDGEFinishComponent>();
        message = key;
        foreach (EDGEFinishComponent edgeFinishComponent in dictionary2[key])
          elementIdList1.Add(edgeFinishComponent.Id);
      }
      else
      {
        familycount = dictionary1[key].Count<EDGEFinishComponent>();
        actualcount = 0;
        foreach (EDGEFinishComponent edgeFinishComponent in dictionary1[key])
          elementIdList2.Add(edgeFinishComponent.Id);
      }
      if (familycount != actualcount || !volumePass1)
        dataForAddonTestList1.Add(new dataForAddonTest(mismatchedId, familyTypeName, familycount, actualcount, volumePass1, elementIdList1, elementIdList2, addonPass, message));
    }
    bool volumePass2 = true;
    List<dataForAddonTest> dataForAddonTestList2 = new List<dataForAddonTest>();
    List<EDGEAddonComponent> addonsForHost1 = MKComparisonEngine._MVData.GetAddonsForHost(standard);
    List<EDGEAddonComponent> addonsForHost2 = MKComparisonEngine._MVData.GetAddonsForHost(incoming);
    Dictionary<string, List<EDGEAddonComponent>> dictionary3 = new Dictionary<string, List<EDGEAddonComponent>>();
    foreach (EDGEAddonComponent edgeAddonComponent in addonsForHost1)
    {
      string familyName = (MKComparisonEngine._uiDoc.Document.GetElement(edgeAddonComponent.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName;
      string name = (MKComparisonEngine._uiDoc.Document.GetElement(edgeAddonComponent.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name;
      string str1 = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      string str2 = "";
      if (edgeAddonComponent.MaterialId != (Autodesk.Revit.DB.ElementId) null)
        str2 = MKComparisonEngine._uiDoc.Document.GetElement(edgeAddonComponent.MaterialId).Name;
      string key = string.IsNullOrEmpty(str2) ? str1 : $"{str1} ({str2})";
      if (dictionary3.ContainsKey(key))
        dictionary3[key].Add(edgeAddonComponent);
      else
        dictionary3.Add(key, new List<EDGEAddonComponent>()
        {
          edgeAddonComponent
        });
    }
    Dictionary<string, List<EDGEAddonComponent>> dictionary4 = new Dictionary<string, List<EDGEAddonComponent>>();
    foreach (EDGEAddonComponent edgeAddonComponent in addonsForHost2)
    {
      string familyName = (MKComparisonEngine._uiDoc.Document.GetElement(edgeAddonComponent.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName;
      string name = (MKComparisonEngine._uiDoc.Document.GetElement(edgeAddonComponent.Id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name;
      string str3 = !familyName.Equals(name) ? $"{familyName} - {name}" : familyName;
      string str4 = "";
      if (edgeAddonComponent.MaterialId != (Autodesk.Revit.DB.ElementId) null)
        str4 = MKComparisonEngine._uiDoc.Document.GetElement(edgeAddonComponent.MaterialId).Name;
      string key = string.IsNullOrEmpty(str4) ? str3 : $"{str3} ({str4})";
      if (dictionary4.ContainsKey(key))
        dictionary4[key].Add(edgeAddonComponent);
      else
        dictionary4.Add(key, new List<EDGEAddonComponent>()
        {
          edgeAddonComponent
        });
    }
    foreach (string key in dictionary3.Keys.Union<string>((IEnumerable<string>) dictionary4.Keys).ToList<string>())
    {
      bool flag3 = false;
      bool flag4 = false;
      List<Autodesk.Revit.DB.ElementId> elementIdList3 = new List<Autodesk.Revit.DB.ElementId>();
      List<Autodesk.Revit.DB.ElementId> mismatchedId = new List<Autodesk.Revit.DB.ElementId>();
      string message = "";
      List<Autodesk.Revit.DB.ElementId> elementIdList4 = new List<Autodesk.Revit.DB.ElementId>();
      bool addonPass = true;
      string familyTypeName = key;
      if (!dictionary3.ContainsKey(key))
        flag3 = true;
      if (!dictionary4.ContainsKey(key))
        flag4 = true;
      int familycount;
      int actualcount;
      if (!flag3 && !flag4)
      {
        List<EDGEAddonComponent> list3 = dictionary3[key].OrderBy<EDGEAddonComponent, double>((Func<EDGEAddonComponent, double>) (s => s.MaterialVolume_InInternalUnits)).ToList<EDGEAddonComponent>();
        List<EDGEAddonComponent> list4 = dictionary4[key].OrderBy<EDGEAddonComponent, double>((Func<EDGEAddonComponent, double>) (s => s.MaterialVolume_InInternalUnits)).ToList<EDGEAddonComponent>();
        List<EDGEAddonComponent> edgeAddonComponentList = new List<EDGEAddonComponent>();
        familycount = dictionary3[key].Count<EDGEAddonComponent>();
        actualcount = dictionary4[key].Count<EDGEAddonComponent>();
        if (list3.Count >= list4.Count)
        {
          int count3 = list3.Count;
        }
        else
        {
          int count4 = list4.Count;
        }
        foreach (EDGEAddonComponent edgeAddonComponent1 in list4)
        {
          Autodesk.Revit.DB.ElementId id3 = edgeAddonComponent1.Id;
          if (!elementIdList3.Contains(id3))
            elementIdList3.Add(id3);
          foreach (EDGEAddonComponent edgeAddonComponent2 in list3)
          {
            Autodesk.Revit.DB.ElementId id4 = edgeAddonComponent2.Id;
            if (!elementIdList4.Contains(id4))
              elementIdList4.Add(id4);
            if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(edgeAddonComponent1.MaterialVolume_InInternalUnits, edgeAddonComponent2.MaterialVolume_InInternalUnits, MKToleranceAspect.AddonVolume))
            {
              if (!mismatchedId.Contains(edgeAddonComponent1.Id))
                mismatchedId.Add(edgeAddonComponent1.Id);
              volumePass2 = false;
              if (edgeAddonComponentList.Contains(edgeAddonComponent1))
                edgeAddonComponentList.Remove(edgeAddonComponent1);
            }
            else if (!edgeAddonComponentList.Contains(edgeAddonComponent1))
              edgeAddonComponentList.Add(edgeAddonComponent1);
          }
        }
        if (elementIdList4.Count<Autodesk.Revit.DB.ElementId>() != elementIdList3.Count<Autodesk.Revit.DB.ElementId>())
          message = "Count Mismatch";
      }
      else if (flag3 && !flag4)
      {
        familycount = 0;
        message = key;
        actualcount = dictionary4[key].Count<EDGEAddonComponent>();
        foreach (EDGEAddonComponent edgeAddonComponent in dictionary4[key])
          elementIdList3.Add(edgeAddonComponent.Id);
      }
      else if (!flag3 & flag4)
      {
        familycount = dictionary3[key].Count<EDGEAddonComponent>();
        actualcount = 0;
        foreach (EDGEAddonComponent edgeAddonComponent in dictionary3[key])
          elementIdList4.Add(edgeAddonComponent.Id);
      }
      else
        break;
      if (familycount != actualcount || !volumePass2)
        dataForAddonTestList1.Add(new dataForAddonTest(mismatchedId, familyTypeName, familycount, actualcount, volumePass2, elementIdList3, elementIdList4, addonPass, message));
    }
    return dataForAddonTestList1;
  }

  private static MarkResult CompareSameMarks(
    IGrouping<string, Autodesk.Revit.DB.FamilyInstance> grp,
    out string reason)
  {
    reason = "";
    MarkResult markResult = new MarkResult();
    markResult.PieceCount = grp.Count<Autodesk.Revit.DB.FamilyInstance>();
    markResult.ControlMark = grp.Key;
    Autodesk.Revit.DB.FamilyInstance elem = grp.First<Autodesk.Revit.DB.FamilyInstance>();
    if (grp.Any<Autodesk.Revit.DB.FamilyInstance>())
    {
      string str = "";
      if (MKComparisonEngine.ConstructionProductGuid != Guid.Empty)
      {
        Parameter parameter = elem.get_Parameter(MKComparisonEngine.ConstructionProductGuid);
        if (parameter != null && parameter.HasValue)
          str = parameter.AsString();
      }
      if (string.IsNullOrWhiteSpace(str))
      {
        Parameter parameter = Parameters.LookupParameter((Autodesk.Revit.DB.Element) elem, "CONSTRUCTION_PRODUCT");
        if (parameter != null)
          MKComparisonEngine.ConstructionProductGuid = parameter.GUID;
        if (parameter.HasValue)
          str = parameter.AsString();
      }
      markResult.ConstructionProduct = str;
    }
    if (grp.Count<Autodesk.Revit.DB.FamilyInstance>() == 1)
    {
      reason += "Only one instance exists in project";
      markResult.Verified = true;
      markResult.Description = reason;
      List<Autodesk.Revit.DB.FamilyInstance> members = new List<Autodesk.Revit.DB.FamilyInstance>()
      {
        elem
      };
      markResult.GroupTestResults = new List<GroupTestResult>()
      {
        new GroupTestResult(members, new List<TestResult>())
      };
      return markResult;
    }
    Document document = elem.Document;
    Dictionary<int, GroupTestResult> dictionary = new Dictionary<int, GroupTestResult>();
    int key1 = 1;
    foreach (Autodesk.Revit.DB.FamilyInstance famInst in (IEnumerable<Autodesk.Revit.DB.FamilyInstance>) grp)
    {
      bool flag1 = false;
      foreach (int key2 in dictionary.Keys)
      {
        string reason1 = "";
        if (dictionary[key2].GroupMembers.Any<Autodesk.Revit.DB.FamilyInstance>())
        {
          List<Autodesk.Revit.DB.FamilyInstance> groupMembers = dictionary[key2].GroupMembers;
          List<TestResult> results = MKComparisonEngine.RunComparisonTestsOnInstances(document, groupMembers, famInst, out reason1);
          markResult.PlateRotated = MKComparisonEngine.TestsHadPlateRotationFailure(results);
          markResult.UseCountMultiplier = MKComparisonEngine.TestsHadPlateUseCountMultiplier(results);
          if (MKComparisonEngine.allthePlates.Count<KeyValuePair<string, List<Autodesk.Revit.DB.ElementId>>>() > 0)
          {
            markResult.PlateRotated = true;
            List<Plates> platesList = new List<Plates>();
            foreach (string key3 in MKComparisonEngine.allthePlates.Keys)
              platesList.Add(new Plates()
              {
                Names = key3,
                Ids = MKComparisonEngine.allthePlates[key3]
              });
            markResult.FailedRotationList = platesList;
          }
          if (MKComparisonEngine.excludedPlates.Count<KeyValuePair<string, List<Autodesk.Revit.DB.ElementId>>>() > 0)
          {
            markResult.UseCountMultiplier = true;
            List<Plates> platesList = new List<Plates>();
            foreach (string key4 in MKComparisonEngine.excludedPlates.Keys)
              platesList.Add(new Plates()
              {
                Names = key4,
                Ids = MKComparisonEngine.excludedPlates[key4]
              });
            markResult.CountMultiplierList = platesList;
          }
          if (MKComparisonEngine.TestsPassed(results))
          {
            flag1 = true;
            bool flag2 = false;
            string uniqueId = famInst.UniqueId;
            foreach (Autodesk.Revit.DB.Element groupMember in dictionary[key2].GroupMembers)
            {
              if (groupMember.UniqueId.Contains(uniqueId))
              {
                flag2 = true;
                break;
              }
            }
            if (!flag2)
            {
              dictionary[key2].GroupMembers.Add(famInst);
              break;
            }
            break;
          }
        }
      }
      if (!flag1)
      {
        GroupTestResult groupTestResult = new GroupTestResult(new List<Autodesk.Revit.DB.FamilyInstance>()
        {
          famInst
        });
        dictionary.Add(key1, groupTestResult);
        ++key1;
      }
    }
    int key5 = 0;
    int num = 0;
    foreach (int key6 in dictionary.Keys)
    {
      if (dictionary[key6].GroupMembers.Count > num)
      {
        key5 = key6;
        num = dictionary[key6].GroupMembers.Count;
      }
    }
    List<Autodesk.Revit.DB.FamilyInstance> groupMembers1 = dictionary[key5].GroupMembers;
    bool flag3 = true;
    foreach (int key7 in dictionary.Keys)
    {
      Autodesk.Revit.DB.FamilyInstance famInst = dictionary[key7].GroupMembers.First<Autodesk.Revit.DB.FamilyInstance>();
      List<TestResult> testResultList = MKComparisonEngine.RunComparisonTestsOnInstances(document, groupMembers1, famInst, out reason);
      if (!MKComparisonEngine.MarkQACopy.traditionalApproach)
      {
        List<TestResult> testResults = new List<TestResult>();
        foreach (TestResult testResult in testResultList)
        {
          if (!testResult.Passed && !testResult.NotUsed)
            testResults.Add(testResult);
          if (testResult.Passed && testResult.FailedPlateRotationTest)
            testResults.Add(testResult);
        }
        MKComparisonEngine.DetailFunction(testResults);
        if (MKComparisonEngine.cancelOut)
          return (MarkResult) null;
        markResult.GroupTestResults.Add(new GroupTestResult(dictionary[key7].GroupMembers, MKComparisonEngine.detailVisualTemplate(testResultList)));
      }
      if (MKComparisonEngine.MarkQACopy.traditionalApproach)
      {
        markResult.GroupTestResults.Add(new GroupTestResult(dictionary[key7].GroupMembers, testResultList));
        if (testResultList.Any<TestResult>())
        {
          if (!testResultList.Last<TestResult>().Passed)
            flag3 = false;
          if (markResult.TestResults.Any<TestResult>())
          {
            if (markResult.TestResults.Last<TestResult>().Passed)
            {
              markResult.TestResults.Clear();
              markResult.TestResults.AddRange((IEnumerable<TestResult>) testResultList);
            }
          }
          else
            markResult.TestResults.AddRange((IEnumerable<TestResult>) testResultList);
        }
      }
    }
    if (MKComparisonEngine.MarkQACopy.traditionalApproach)
    {
      if (flag3)
        markResult.Verified = true;
    }
    else
    {
      bool flag4 = false;
      foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
      {
        foreach (TestResult groupResult in groupTestResult.GroupResults)
        {
          if (!groupResult.Passed && groupResult.NotUsed && !groupResult.FailedPlateRotationTest || groupResult.Passed && !groupResult.NotUsed && !groupResult.FailedPlateRotationTest)
          {
            flag4 = true;
          }
          else
          {
            flag4 = false;
            break;
          }
        }
        if (!flag4)
          break;
      }
      if (flag4)
        markResult.Verified = true;
      markResult.TestResults = MKComparisonEngine.MainPageResults(markResult.GroupTestResults);
    }
    return markResult;
  }

  private static List<TestResult> MainPageResults(List<GroupTestResult> groups)
  {
    TestResult testResult1 = new TestResult(MKTest.FamilyType, true, 0);
    TestResult testResult2 = new TestResult(MKTest.ParameterComparison, true, 0);
    TestResult testResult3 = new TestResult(MKTest.MainMaterialVolume, true, 0);
    TestResult testResult4 = new TestResult(MKTest.AddonFamilyVolumeCount, true, 0);
    TestResult testResult5 = new TestResult(MKTest.PlateNamesCounts, true, 0);
    TestResult testResult6 = new TestResult(MKTest.AddonLocation, true, 0);
    TestResult testResult7 = new TestResult(MKTest.PlateLocation, true, 0);
    TestResult testResult8 = new TestResult(MKTest.Geometry, true, 0);
    List<TestResult> testResultList = new List<TestResult>();
    foreach (GroupTestResult group in groups)
    {
      if (groups.Count<GroupTestResult>() > 1 && group.Equals((object) groups.First<GroupTestResult>()))
      {
        bool flag = false;
        foreach (TestResult groupResult in group.GroupResults)
        {
          if (!groupResult.Passed && !groupResult.NotUsed)
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          continue;
      }
      foreach (TestResult groupResult in group.GroupResults)
      {
        string testName = groupResult.TestName;
        if (testName != null)
        {
          switch (testName.Length)
          {
            case 23:
              switch (testName[8])
              {
                case 'A':
                  if (testName == "Compare Addon Locations")
                  {
                    if (MKComparisonEngine.MarkQACopy.bCompareAddons_LocationAndOrientation)
                    {
                      if (!groupResult.Passed && !groupResult.NotUsed)
                      {
                        if (testResult6.Passed || testResult6.NotUsed)
                        {
                          testResult6 = groupResult;
                          continue;
                        }
                        continue;
                      }
                      if (groupResult.Passed && !groupResult.NotUsed && (testResult6.Passed || testResult6.NotUsed))
                      {
                        testResult6 = groupResult;
                        continue;
                      }
                      continue;
                    }
                    testResult6 = new TestResult(MKTest.AddonLocation, true, 0);
                    continue;
                  }
                  continue;
                case 'M':
                  if (testName == "Compare Member Geometry")
                  {
                    if (MKComparisonEngine.MarkQACopy.bCompareSolidsFaces)
                    {
                      if (!groupResult.Passed && !groupResult.NotUsed)
                      {
                        if (testResult8.Passed || testResult8.NotUsed)
                        {
                          testResult8 = groupResult;
                          continue;
                        }
                        continue;
                      }
                      if (groupResult.Passed && !groupResult.NotUsed && (testResult8.Passed || testResult8.NotUsed))
                      {
                        testResult8 = groupResult;
                        continue;
                      }
                      continue;
                    }
                    testResult8 = new TestResult(MKTest.Geometry, true, 0);
                    continue;
                  }
                  continue;
                case 'P':
                  if (testName == "Compare Plate Locations")
                  {
                    if (MKComparisonEngine.MarkQACopy.bComparePlate_LocationAndOrientation)
                    {
                      if (!groupResult.Passed && !groupResult.NotUsed)
                      {
                        if (testResult7.Passed || testResult7.NotUsed)
                        {
                          testResult7 = groupResult;
                          continue;
                        }
                        continue;
                      }
                      if (groupResult.Passed && !groupResult.NotUsed && (testResult7.Passed || testResult7.NotUsed))
                      {
                        testResult7 = groupResult;
                        continue;
                      }
                      continue;
                    }
                    testResult7 = new TestResult(MKTest.PlateLocation, true, 0);
                    continue;
                  }
                  continue;
                case 'y':
                  if (testName == "Family Types Comparison")
                  {
                    if (MKComparisonEngine.MarkQACopy.bFamilyTypeTest)
                    {
                      if (!groupResult.Passed && !groupResult.NotUsed)
                      {
                        if (testResult1.Passed || testResult1.NotUsed)
                        {
                          testResult1 = groupResult;
                          continue;
                        }
                        continue;
                      }
                      if (groupResult.Passed && !groupResult.NotUsed && (testResult1.Passed || testResult1.NotUsed))
                      {
                        testResult1 = groupResult;
                        continue;
                      }
                      continue;
                    }
                    testResult1 = new TestResult(MKTest.FamilyType, true, 0);
                    continue;
                  }
                  continue;
                default:
                  continue;
              }
            case 25:
              if (testName == "Compare Family Parameters")
              {
                if (MKComparisonEngine.MarkQACopy.bCompareAllParameters)
                {
                  if (!groupResult.Passed && !groupResult.NotUsed)
                  {
                    if (testResult2.Passed || testResult2.NotUsed)
                    {
                      testResult2 = groupResult;
                      continue;
                    }
                    continue;
                  }
                  if (groupResult.Passed && !groupResult.NotUsed && (testResult2.Passed || testResult2.NotUsed))
                  {
                    testResult2 = groupResult;
                    continue;
                  }
                  continue;
                }
                testResult2 = new TestResult(MKTest.ParameterComparison, true, 0);
                continue;
              }
              continue;
            case 29:
              if (testName == "Compare Main Material Volumes")
              {
                if (MKComparisonEngine.MarkQACopy.bCompareMaterialVolumes)
                {
                  if (!groupResult.Passed && !groupResult.NotUsed)
                  {
                    if (testResult3.Passed || testResult3.NotUsed)
                    {
                      testResult3 = groupResult;
                      continue;
                    }
                    continue;
                  }
                  if (groupResult.Passed && !groupResult.NotUsed && (testResult3.Passed || testResult3.NotUsed))
                  {
                    testResult3 = groupResult;
                    continue;
                  }
                  continue;
                }
                testResult3 = new TestResult(MKTest.MainMaterialVolume, true, 0);
                continue;
              }
              continue;
            case 37:
              if (testName == "Compare Plate Family Types and Counts")
              {
                if (MKComparisonEngine.MarkQACopy.bComparePlates_NamesAndCounts)
                {
                  if (!groupResult.Passed && !groupResult.NotUsed)
                  {
                    if (testResult5.Passed || testResult5.NotUsed)
                    {
                      testResult5 = groupResult;
                      continue;
                    }
                    continue;
                  }
                  if (groupResult.Passed && !groupResult.NotUsed && (testResult5.Passed || testResult5.NotUsed))
                  {
                    testResult5 = groupResult;
                    continue;
                  }
                  continue;
                }
                testResult5 = new TestResult(MKTest.PlateNamesCounts, true, 0);
                continue;
              }
              continue;
            case 48 /*0x30*/:
              if (testName == "Addon Family Types, Counts, and Material Volumes")
              {
                if (MKComparisonEngine.MarkQACopy.bCompareAddons_VolMatCountFamily)
                {
                  if (!groupResult.Passed && !groupResult.NotUsed)
                  {
                    if (testResult4.Passed || testResult4.NotUsed)
                    {
                      testResult4 = groupResult;
                      continue;
                    }
                    continue;
                  }
                  if (groupResult.Passed && !groupResult.NotUsed && (testResult4.Passed || testResult4.NotUsed))
                  {
                    testResult4 = groupResult;
                    continue;
                  }
                  continue;
                }
                testResult4 = new TestResult(MKTest.AddonFamilyVolumeCount, true, 0);
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
    }
    testResultList.Add(testResult1);
    testResultList.Add(testResult2);
    testResultList.Add(testResult3);
    testResultList.Add(testResult4);
    testResultList.Add(testResult5);
    testResultList.Add(testResult6);
    testResultList.Add(testResult7);
    testResultList.Add(testResult8);
    return testResultList;
  }

  private static List<TestResult> detailVisualTemplate(List<TestResult> AllResults)
  {
    TestResult testResult1 = new TestResult(MKTest.FamilyType, true, 0);
    TestResult testResult2 = new TestResult(MKTest.ParameterComparison, true, 0);
    TestResult testResult3 = new TestResult(MKTest.MainMaterialVolume, true, 0);
    TestResult testResult4 = new TestResult(MKTest.AddonFamilyVolumeCount, true, 0);
    TestResult testResult5 = new TestResult(MKTest.PlateNamesCounts, true, 0);
    TestResult testResult6 = new TestResult(MKTest.AddonLocation, true, 0);
    TestResult testResult7 = new TestResult(MKTest.PlateLocation, true, 0);
    TestResult testResult8 = new TestResult(MKTest.Geometry, true, 0);
    foreach (TestResult allResult in AllResults)
    {
      if (allResult.TestName.Equals("Family Types Comparison"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult1.Passed || testResult1.NotUsed)
            testResult1 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult1.Passed || testResult1.NotUsed))
          testResult1 = allResult;
      }
      else if (allResult.TestName.Equals("Compare Family Parameters"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult2.Passed || testResult2.NotUsed)
            testResult2 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult2.Passed || testResult2.NotUsed))
          testResult2 = allResult;
      }
      else if (allResult.TestName.Equals("Compare Main Material Volumes"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult3.Passed || testResult3.NotUsed)
            testResult3 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult3.Passed || testResult3.NotUsed))
          testResult3 = allResult;
      }
      else if (allResult.TestName.Equals("Addon Family Types, Counts, and Material Volumes"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult4.Passed || testResult4.NotUsed)
            testResult4 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult4.Passed || testResult4.NotUsed))
          testResult4 = allResult;
      }
      else if (allResult.TestName.Equals("Compare Plate Family Types and Counts"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult5.Passed || testResult5.NotUsed)
            testResult5 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult5.Passed || testResult5.NotUsed))
          testResult5 = allResult;
      }
      else if (allResult.TestName.Equals("Compare Addon Locations"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult6.Passed || testResult6.NotUsed)
            testResult6 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult6.Passed || testResult6.NotUsed))
          testResult6 = allResult;
      }
      else if (allResult.TestName.Equals("Compare Plate Locations"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult7.Passed || testResult7.NotUsed)
            testResult7 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult7.Passed || testResult7.NotUsed))
          testResult7 = allResult;
      }
      else if (allResult.TestName.Equals("Compare Member Geometry"))
      {
        if (!allResult.Passed && !allResult.NotUsed)
        {
          if (testResult8.Passed || testResult8.NotUsed)
            testResult8 = allResult;
        }
        else if (allResult.Passed && !allResult.NotUsed && (!testResult8.Passed || testResult8.NotUsed))
          testResult8 = allResult;
      }
    }
    return new List<TestResult>()
    {
      testResult1,
      testResult2,
      testResult3,
      testResult4,
      testResult5,
      testResult6,
      testResult7,
      testResult8
    };
  }

  private static List<TestResult> RunComparisonTestsOnInstances(
    Document revitDoc,
    List<Autodesk.Revit.DB.FamilyInstance> standards,
    Autodesk.Revit.DB.FamilyInstance famInst,
    out string reason)
  {
    List<TestResult> collection = new List<TestResult>();
    List<TestResult> testResultList = new List<TestResult>();
    bool traditionalApproach = MKComparisonEngine.MarkQACopy.traditionalApproach;
    int integerValue = famInst.Id.IntegerValue;
    reason = "";
    bool flag = false;
    foreach (Autodesk.Revit.DB.FamilyInstance standard in standards)
    {
      if (!MKComparisonEngine.CompareFamilyAndType(standard, famInst))
      {
        reason += "Family and Type Names do not match";
        if (traditionalApproach)
          collection.Add(new TestResult(MKTest.FamilyType, false, standard.Id, famInst.Id));
        else
          collection.Add(new TestResult(MKTest.FamilyType, false, standard.Id, famInst.Id, standard, famInst));
        return collection;
      }
      if (!traditionalApproach)
        collection.Add(new TestResult(MKTest.FamilyType, true, standard.Id));
      else
        collection.Add(new TestResult(MKTest.FamilyType, true));
      if (MKComparisonEngine.MarkQACopy.bCompareAllParameters)
      {
        Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
        if (!MKComparisonEngine.CompareAllParameters(standard, famInst))
        {
          reason += "Element standard parameter values do not match.";
          if (traditionalApproach)
            collection.Add(new TestResult(MKTest.ParameterComparison, false, standard.Id, famInst.Id));
          else
            collection.Add(new TestResult(MKTest.ParameterComparison, false, standard.Id, famInst.Id));
          if (traditionalApproach)
            return collection;
        }
        else
          collection.Add(new TestResult(MKTest.ParameterComparison, true));
      }
      if (MKComparisonEngine.MarkQACopy.bCompareMaterialVolumes)
      {
        if (!MKComparisonEngine.CompareMaterialVolumes(standard, famInst))
        {
          reason += "Elements contain multiple materials with mis-matched volumes.";
          if (traditionalApproach)
            collection.Add(new TestResult(MKTest.MainMaterialVolume, false, standard.Id, famInst.Id, standard));
          else
            collection.Add(new TestResult(MKTest.MainMaterialVolume, false, standard.Id, famInst.Id, standard, famInst));
          if (traditionalApproach)
            return collection;
        }
        else
          collection.Add(new TestResult(MKTest.MainMaterialVolume, true));
      }
      if (MKComparisonEngine.MarkQACopy.bCompareAddons_VolMatCountFamily)
      {
        string message = "";
        List<dataForAddonTest> dataForAddonTestList = new List<dataForAddonTest>();
        if (!MKComparisonEngine.CompareAddons_VolMatCountFamily(standard, famInst, out message))
        {
          reason = $"{reason}Elements' addon components do not match for number, volume, or material.  Detailed Message: {message}";
          collection.Add(new TestResult(MKTest.AddonFamilyVolumeCount, false, standard.Id, famInst.Id, famInst));
          if (traditionalApproach)
            return collection;
        }
        else
          collection.Add(new TestResult(MKTest.AddonFamilyVolumeCount, true));
      }
      if (MKComparisonEngine.MarkQACopy.bComparePlates_NamesAndCounts)
      {
        Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
        string plateCompareDetailedMessage = "";
        if (!MKComparisonEngine.ComparePlates_NamesAndCounts(revitDoc, standard, famInst, out plateCompareDetailedMessage))
        {
          reason = $"{reason}Elements' Plate names or counts do not match.  Detailed Message: {plateCompareDetailedMessage}";
          TestResult testResult = new TestResult(MKTest.PlateNamesCounts, false, standard.Id, famInst.Id);
          if (MKComparisonEngine.excludedPlates.Count > 0)
          {
            testResult.EmbedsExcludedBool = true;
            if (testResult.EmbedsExcludedDictionary != null)
            {
              foreach (string key in MKComparisonEngine.excludedPlates.Keys)
              {
                if (testResult.EmbedsExcludedDictionary.ContainsKey(key))
                {
                  foreach (Autodesk.Revit.DB.ElementId elementId in MKComparisonEngine.excludedPlates[key])
                  {
                    if (!testResult.EmbedsExcludedDictionary[key].Contains(elementId))
                      testResult.EmbedsExcludedDictionary[key].Add(elementId);
                  }
                }
                else
                  testResult.EmbedsExcludedDictionary.Add(key, MKComparisonEngine.excludedPlates[key]);
              }
            }
            else
              testResult.EmbedsExcludedDictionary = MKComparisonEngine.excludedPlates;
          }
          collection.Add(testResult);
          if (traditionalApproach)
            return collection;
        }
        else
        {
          TestResult testResult = new TestResult(MKTest.PlateNamesCounts, true);
          if (MKComparisonEngine.excludedPlates.Count > 0)
          {
            testResult.EmbedsExcludedBool = true;
            if (testResult.EmbedsExcludedDictionary != null)
            {
              foreach (string key in MKComparisonEngine.excludedPlates.Keys)
              {
                if (testResult.EmbedsExcludedDictionary.ContainsKey(key))
                {
                  foreach (Autodesk.Revit.DB.ElementId elementId in MKComparisonEngine.excludedPlates[key])
                  {
                    if (!testResult.EmbedsExcludedDictionary[key].Contains(elementId))
                      testResult.EmbedsExcludedDictionary[key].Add(elementId);
                  }
                }
                else
                  testResult.EmbedsExcludedDictionary.Add(key, MKComparisonEngine.excludedPlates[key]);
              }
            }
            else
              testResult.EmbedsExcludedDictionary = MKComparisonEngine.excludedPlates;
          }
          collection.Add(testResult);
        }
      }
      if (MKComparisonEngine.MarkQACopy.bCompareAddons_LocationAndOrientation)
      {
        Dictionary<string, List<Autodesk.Revit.DB.ElementId>> dictionary = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
        LocationCompareResult locationResult;
        if (!MKComparisonEngine.RunGeometryComparisonOnStructuralFraming(standard, famInst, out locationResult, true, false, false, true))
        {
          TestResult testResult = new TestResult(MKTest.AddonLocation, false, standard.Id, famInst.Id);
          testResult.FailedPlateRotationTest = locationResult == LocationCompareResult.PlateRotationWarning;
          if (MKComparisonEngine.excludedPlates.Count > 0)
          {
            testResult.EmbedsExcludedBool = true;
            if (testResult.EmbedsExcludedDictionary != null)
            {
              foreach (string key in MKComparisonEngine.excludedPlates.Keys)
              {
                if (testResult.EmbedsExcludedDictionary.ContainsKey(key))
                {
                  foreach (Autodesk.Revit.DB.ElementId elementId in MKComparisonEngine.excludedPlates[key])
                  {
                    if (!testResult.EmbedsExcludedDictionary[key].Contains(elementId))
                      testResult.EmbedsExcludedDictionary[key].Add(elementId);
                  }
                }
                else
                  testResult.EmbedsExcludedDictionary.Add(key, MKComparisonEngine.excludedPlates[key]);
              }
            }
            else
              testResult.EmbedsExcludedDictionary = MKComparisonEngine.excludedPlates;
          }
          collection.Add(testResult);
          if (traditionalApproach)
            return collection;
        }
        else
        {
          if (MKComparisonEngine.cancelOut)
            return (List<TestResult>) null;
          if (traditionalApproach)
          {
            collection.Add(new TestResult(MKTest.AddonLocation, true));
          }
          else
          {
            TestResult testResult = new TestResult(MKTest.AddonLocation, true, standard.Id, famInst.Id);
            if (MKComparisonEngine.excludedPlates.Count > 0)
            {
              testResult.EmbedsExcludedBool = true;
              if (testResult.EmbedsExcludedDictionary != null)
              {
                foreach (string key in MKComparisonEngine.excludedPlates.Keys)
                {
                  if (testResult.EmbedsExcludedDictionary.ContainsKey(key))
                  {
                    foreach (Autodesk.Revit.DB.ElementId elementId in MKComparisonEngine.excludedPlates[key])
                    {
                      if (!testResult.EmbedsExcludedDictionary[key].Contains(elementId))
                        testResult.EmbedsExcludedDictionary[key].Add(elementId);
                    }
                  }
                  else
                    testResult.EmbedsExcludedDictionary.Add(key, MKComparisonEngine.excludedPlates[key]);
                }
              }
              else
                testResult.EmbedsExcludedDictionary = MKComparisonEngine.excludedPlates;
            }
            collection.Add(testResult);
          }
        }
      }
      if (MKComparisonEngine.MarkQACopy.bComparePlate_LocationAndOrientation)
      {
        LocationCompareResult locationResult;
        if (!MKComparisonEngine.RunGeometryComparisonOnStructuralFraming(standard, famInst, out locationResult, false, true, false, true))
        {
          TestResult testResult = new TestResult(MKTest.PlateLocation, false, standard.Id, famInst.Id);
          testResult.FailedPlateRotationTest = locationResult == LocationCompareResult.PlateRotationWarning;
          if (MKComparisonEngine.excludedPlates.Count > 0)
          {
            testResult.EmbedsExcludedBool = true;
            if (testResult.EmbedsExcludedDictionary != null)
            {
              foreach (string key in MKComparisonEngine.excludedPlates.Keys)
              {
                if (testResult.EmbedsExcludedDictionary.ContainsKey(key))
                {
                  foreach (Autodesk.Revit.DB.ElementId elementId in MKComparisonEngine.excludedPlates[key])
                  {
                    if (!testResult.EmbedsExcludedDictionary[key].Contains(elementId))
                      testResult.EmbedsExcludedDictionary[key].Add(elementId);
                  }
                }
                else
                  testResult.EmbedsExcludedDictionary.Add(key, MKComparisonEngine.excludedPlates[key]);
              }
            }
            else
              testResult.EmbedsExcludedDictionary = MKComparisonEngine.excludedPlates;
          }
          collection.Add(testResult);
          if (traditionalApproach)
            return collection;
        }
        else
        {
          if (MKComparisonEngine.cancelOut)
            return (List<TestResult>) null;
          if (locationResult == LocationCompareResult.PlateRotationWarning)
            flag = true;
          if (traditionalApproach)
          {
            TestResult testResult = new TestResult(MKTest.PlateLocation, true);
            if (flag)
              testResult.FailedPlateRotationTest = flag;
            if (MKComparisonEngine.excludedPlates.Count > 0)
              testResult.EmbedsExcludedBool = true;
            collection.Add(testResult);
          }
          else
          {
            TestResult testResult = new TestResult(MKTest.PlateLocation, true, standard.Id, famInst.Id);
            if (flag)
            {
              testResult.FailedPlateRotationTest = flag;
              if (MKComparisonEngine.allthePlates.Keys.Count > 0)
              {
                if (testResult.FailedRotationPlates != null)
                {
                  foreach (KeyValuePair<string, List<Autodesk.Revit.DB.ElementId>> allthePlate in MKComparisonEngine.allthePlates)
                  {
                    if (testResult.FailedRotationPlates.ContainsKey(allthePlate.Key))
                    {
                      foreach (Autodesk.Revit.DB.ElementId elementId in allthePlate.Value)
                      {
                        if (!testResult.FailedRotationPlates[allthePlate.Key].Contains(elementId))
                          testResult.FailedRotationPlates[allthePlate.Key].Add(elementId);
                      }
                    }
                    else
                      testResult.FailedRotationPlates.Add(allthePlate.Key, allthePlate.Value);
                  }
                }
                else
                  testResult.FailedRotationPlates = MKComparisonEngine.allthePlates;
              }
            }
            if (MKComparisonEngine.excludedPlates.Count > 0)
            {
              testResult.EmbedsExcludedBool = true;
              if (testResult.EmbedsExcludedDictionary != null)
              {
                foreach (string key in MKComparisonEngine.excludedPlates.Keys)
                {
                  if (testResult.EmbedsExcludedDictionary.ContainsKey(key))
                  {
                    foreach (Autodesk.Revit.DB.ElementId elementId in MKComparisonEngine.excludedPlates[key])
                    {
                      if (!testResult.EmbedsExcludedDictionary[key].Contains(elementId))
                        testResult.EmbedsExcludedDictionary[key].Add(elementId);
                    }
                  }
                  else
                    testResult.EmbedsExcludedDictionary.Add(key, MKComparisonEngine.excludedPlates[key]);
                }
              }
              else
                testResult.EmbedsExcludedDictionary = MKComparisonEngine.excludedPlates;
            }
            collection.Add(testResult);
          }
        }
      }
      if (MKComparisonEngine.MarkQACopy.bCompareSolidsFaces)
      {
        LocationCompareResult locationResult;
        if (!MKComparisonEngine.RunGeometryComparisonOnStructuralFraming(standard, famInst, out locationResult, false, false, true, traditionalApproach))
        {
          collection.Add(new TestResult(MKTest.Geometry, false, standard.Id, famInst.Id)
          {
            FailedPlateRotationTest = locationResult == LocationCompareResult.PlateRotationWarning
          });
          if (traditionalApproach)
            return collection;
        }
        else
        {
          if (MKComparisonEngine.cancelOut)
            return (List<TestResult>) null;
          if (traditionalApproach)
            collection.Add(new TestResult(MKTest.AddonLocation, true, standard.Id, famInst.Id));
          else
            collection.Add(new TestResult(MKTest.Geometry, true, standard.Id, famInst.Id));
        }
      }
      if (!traditionalApproach)
      {
        if (standards.First<Autodesk.Revit.DB.FamilyInstance>().Equals((object) standard))
        {
          testResultList.AddRange((IEnumerable<TestResult>) collection);
        }
        else
        {
          foreach (TestResult testResult in collection)
          {
            if (!testResult.Passed || testResult.Passed && testResult.FailedPlateRotationTest)
            {
              testResultList.AddRange((IEnumerable<TestResult>) collection);
              break;
            }
          }
        }
      }
      collection = new List<TestResult>();
    }
    if (traditionalApproach)
    {
      if (MKComparisonEngine.MarkQACopy.bFamilyTypeTest)
        collection.Add(new TestResult(MKTest.FamilyType, true));
      if (MKComparisonEngine.MarkQACopy.bCompareAllParameters)
        collection.Add(new TestResult(MKTest.ParameterComparison, true));
      if (MKComparisonEngine.MarkQACopy.bCompareMaterialVolumes)
        collection.Add(new TestResult(MKTest.MainMaterialVolume, true));
      if (MKComparisonEngine.MarkQACopy.bCompareAddons_VolMatCountFamily)
        collection.Add(new TestResult(MKTest.AddonFamilyVolumeCount, true));
      if (MKComparisonEngine.MarkQACopy.bComparePlates_NamesAndCounts)
      {
        TestResult testResult = new TestResult(MKTest.PlateNamesCounts, true);
        if (MKComparisonEngine.excludedPlates.Count > 0)
          testResult.EmbedsExcludedBool = true;
        collection.Add(testResult);
      }
      if (MKComparisonEngine.MarkQACopy.bCompareAddons_LocationAndOrientation)
        collection.Add(new TestResult(MKTest.AddonLocation, true));
      if (MKComparisonEngine.MarkQACopy.bComparePlate_LocationAndOrientation)
      {
        TestResult testResult = new TestResult(MKTest.PlateLocation, true);
        if (flag)
          testResult.FailedPlateRotationTest = flag;
        if (MKComparisonEngine.excludedPlates.Count > 0)
          testResult.EmbedsExcludedBool = true;
        collection.Add(testResult);
      }
      if (MKComparisonEngine.MarkQACopy.bCompareSolidsFaces)
        collection.Add(new TestResult(MKTest.Geometry, true));
    }
    return traditionalApproach ? collection : testResultList;
  }

  private static bool RunGeometryComparisonOnStructuralFraming(
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance other,
    out LocationCompareResult locationResult,
    bool addonLocation,
    bool embededLocation,
    bool solid,
    bool traditional)
  {
    using (new Autodesk.Revit.DB.Transaction(standard.Document, "Geometry Comparison"))
    {
      Dictionary<string, List<Autodesk.Revit.DB.ElementId>> platesWarning = new Dictionary<string, List<Autodesk.Revit.DB.ElementId>>();
      locationResult = LocationCompareResult.Failed;
      GeometryVerificationFamily verificationFamily1 = MKComparisonEngine.GetGeometryVerificationFamily(standard);
      if (verificationFamily1 == null)
      {
        MKComparisonEngine.cancelOut = true;
        return false;
      }
      GeometryVerificationFamily verificationFamily2 = MKComparisonEngine.GetGeometryVerificationFamily(other);
      if (verificationFamily2 == null)
      {
        MKComparisonEngine.cancelOut = true;
        return false;
      }
      if (verificationFamily1.embedsExcludedByCountMultiplier)
      {
        foreach (string key in verificationFamily1.embedsExcluded.Keys)
        {
          if (MKComparisonEngine.excludedPlates.ContainsKey(key))
          {
            foreach (Autodesk.Revit.DB.ElementId elementId in verificationFamily1.embedsExcluded[key])
            {
              if (!MKComparisonEngine.excludedPlates[key].Contains(elementId))
                MKComparisonEngine.excludedPlates[key].Add(elementId);
            }
          }
          else
            MKComparisonEngine.excludedPlates.Add(key, verificationFamily1.embedsExcluded[key]);
        }
      }
      if (verificationFamily2.embedsExcludedByCountMultiplier)
      {
        foreach (string key in verificationFamily2.embedsExcluded.Keys)
        {
          if (MKComparisonEngine.excludedPlates.ContainsKey(key))
          {
            foreach (Autodesk.Revit.DB.ElementId elementId in verificationFamily2.embedsExcluded[key])
            {
              if (!MKComparisonEngine.excludedPlates[key].Contains(elementId))
                MKComparisonEngine.excludedPlates[key].Add(elementId);
            }
          }
          else
            MKComparisonEngine.excludedPlates.Add(key, verificationFamily2.embedsExcluded[key]);
        }
      }
      bool flag = false;
      if (addonLocation)
      {
        flag = verificationFamily1.FinishMatches(verificationFamily2, out locationResult);
        if (flag)
          flag = verificationFamily1.AddOnLocationMatches(verificationFamily2, out locationResult);
      }
      else if (embededLocation)
        flag = verificationFamily1.EmbedLocationMatches(verificationFamily2, out locationResult, out platesWarning, traditional);
      else if (solid)
        flag = verificationFamily1.SolidMatches(verificationFamily2, out locationResult, traditional);
      if (platesWarning != null)
      {
        foreach (string key in platesWarning.Keys)
        {
          if (MKComparisonEngine.allthePlates.ContainsKey(key))
          {
            foreach (Autodesk.Revit.DB.ElementId elementId in platesWarning[key])
            {
              if (!MKComparisonEngine.allthePlates[key].Contains(elementId))
                MKComparisonEngine.allthePlates[key].Add(elementId);
            }
          }
          else
            MKComparisonEngine.allthePlates.Add(key, platesWarning[key]);
        }
      }
      return flag;
    }
  }

  private static GeometryVerificationFamily GetGeometryVerificationFamily(
    Autodesk.Revit.DB.FamilyInstance famInst,
    bool bVisualize = false)
  {
    if (MKComparisonEngine._GeomVerificationStore.ContainsKey(famInst.UniqueId))
      return MKComparisonEngine._GeomVerificationStore[famInst.UniqueId];
    GeometryVerificationFamily verificationFamily = new GeometryVerificationFamily(famInst, MKComparisonEngine._MVData, bVisualize);
    if (verificationFamily.failed)
      return (GeometryVerificationFamily) null;
    MKComparisonEngine._GeomVerificationStore.Add(famInst.UniqueId, verificationFamily);
    return verificationFamily;
  }

  private static bool ComparePlates_NamesAndCounts(
    Document revitDoc,
    Autodesk.Revit.DB.FamilyInstance standard,
    Autodesk.Revit.DB.FamilyInstance famInst,
    out string plateCompareDetailedMessage)
  {
    plateCompareDetailedMessage = "";
    GeometryVerificationFamily verificationFamily1 = MKComparisonEngine.GetGeometryVerificationFamily(standard);
    if (verificationFamily1 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return false;
    }
    GeometryVerificationFamily verificationFamily2 = MKComparisonEngine.GetGeometryVerificationFamily(famInst);
    if (verificationFamily2 == null)
    {
      MKComparisonEngine.cancelOut = true;
      return false;
    }
    List<Autodesk.Revit.DB.FamilyInstance> source1 = new List<Autodesk.Revit.DB.FamilyInstance>();
    List<Autodesk.Revit.DB.FamilyInstance> source2 = new List<Autodesk.Revit.DB.FamilyInstance>();
    foreach (EDGEEmbedComponent embed in verificationFamily1.Embeds)
    {
      if (embed.inst != null)
      {
        source1.Add(embed.inst);
      }
      else
      {
        Autodesk.Revit.DB.Element element = revitDoc.GetElement(embed.Id);
        if (element != null && element is Autodesk.Revit.DB.FamilyInstance familyInstance)
          source1.Add(familyInstance);
      }
    }
    foreach (EDGEEmbedComponent embed in verificationFamily2.Embeds)
    {
      if (embed.inst != null)
      {
        source2.Add(embed.inst);
      }
      else
      {
        Autodesk.Revit.DB.Element element = revitDoc.GetElement(embed.Id);
        if (element != null && element is Autodesk.Revit.DB.FamilyInstance familyInstance)
          source2.Add(familyInstance);
      }
    }
    Dictionary<string, List<string>> dictionary1 = new Dictionary<string, List<string>>();
    List<string> stringList = new List<string>();
    if (source1.Count<Autodesk.Revit.DB.FamilyInstance>() != source2.Count<Autodesk.Revit.DB.FamilyInstance>())
    {
      plateCompareDetailedMessage += "Plate Counts do not match.";
      return false;
    }
    List<IGrouping<string, Autodesk.Revit.DB.FamilyInstance>> list1 = source1.GroupBy<Autodesk.Revit.DB.FamilyInstance, string>((Func<Autodesk.Revit.DB.FamilyInstance, string>) (s => s.Symbol.FamilyName + s.Symbol.Name)).ToList<IGrouping<string, Autodesk.Revit.DB.FamilyInstance>>();
    List<IGrouping<string, Autodesk.Revit.DB.FamilyInstance>> list2 = source2.GroupBy<Autodesk.Revit.DB.FamilyInstance, string>((Func<Autodesk.Revit.DB.FamilyInstance, string>) (s => s.Symbol.FamilyName + s.Symbol.Name)).ToList<IGrouping<string, Autodesk.Revit.DB.FamilyInstance>>();
    Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
    foreach (IGrouping<string, Autodesk.Revit.DB.FamilyInstance> source3 in list1)
      dictionary2.Add(source3.Key, source3.Count<Autodesk.Revit.DB.FamilyInstance>());
    Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
    foreach (IGrouping<string, Autodesk.Revit.DB.FamilyInstance> source4 in list2)
      dictionary3.Add(source4.Key, source4.Count<Autodesk.Revit.DB.FamilyInstance>());
    if (dictionary2.Keys.Count<string>() == 0 && dictionary3.Keys.Count<string>() > 0)
      dictionary1.Add("No Plates Found", (List<string>) null);
    foreach (string key in dictionary2.Keys)
    {
      if (!dictionary3.ContainsKey(key))
      {
        plateCompareDetailedMessage = $"{plateCompareDetailedMessage}Inst 2 FamilyTypes does not contain {key}";
        return false;
      }
      if (dictionary2[key] != dictionary3[key])
      {
        ref string local = ref plateCompareDetailedMessage;
        string[] strArray = new string[7]
        {
          plateCompareDetailedMessage,
          "Inst 1 FamilyTypes count for ",
          key,
          ": ",
          null,
          null,
          null
        };
        int num = dictionary2[key];
        strArray[4] = num.ToString();
        strArray[5] = " does not match Inst2 count: ";
        num = dictionary3[key];
        strArray[6] = num.ToString();
        string str = string.Concat(strArray);
        local = str;
        return false;
      }
    }
    return true;
  }

  private static bool CompareAddons_VolMatCountFamily(
    Autodesk.Revit.DB.FamilyInstance inst1,
    Autodesk.Revit.DB.FamilyInstance inst2,
    out string message)
  {
    message = "";
    Dictionary<string, List<string>> dictionary1 = new Dictionary<string, List<string>>();
    List<EDGEFinishComponent> finishesForHost1 = MKComparisonEngine._MVData.GetFinishesForHost(inst1);
    List<EDGEFinishComponent> finishesForHost2 = MKComparisonEngine._MVData.GetFinishesForHost(inst2);
    if (finishesForHost1.Count != finishesForHost2.Count)
    {
      message = $"{message}Finish counts do not match: ElementId: {inst1.Id?.ToString()} addon count {finishesForHost1.Count<EDGEFinishComponent>().ToString()} not equal to ElementId: {inst2.Id?.ToString()} addon count: {finishesForHost1.Count<EDGEFinishComponent>().ToString()}";
      return false;
    }
    List<IGrouping<string, EDGEFinishComponent>> list1 = finishesForHost1.GroupBy<EDGEFinishComponent, string>((Func<EDGEFinishComponent, string>) (s => s.FamilyTypeName)).ToList<IGrouping<string, EDGEFinishComponent>>();
    List<IGrouping<string, EDGEFinishComponent>> list2 = finishesForHost2.GroupBy<EDGEFinishComponent, string>((Func<EDGEFinishComponent, string>) (s => s.FamilyTypeName)).ToList<IGrouping<string, EDGEFinishComponent>>();
    Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
    foreach (IGrouping<string, EDGEFinishComponent> source in list1)
      dictionary2.Add(source.Key, source.Count<EDGEFinishComponent>());
    Dictionary<string, int> dictionary3 = new Dictionary<string, int>();
    foreach (IGrouping<string, EDGEFinishComponent> source in list2)
      dictionary3.Add(source.Key, source.Count<EDGEFinishComponent>());
    double val1_1 = finishesForHost1.Sum<EDGEFinishComponent>((Func<EDGEFinishComponent, double>) (s => s.MaterialVolume_InInternalUnits));
    double val2_1 = finishesForHost2.Sum<EDGEFinishComponent>((Func<EDGEFinishComponent, double>) (s => s.MaterialVolume_InInternalUnits));
    if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(val1_1, val2_1, MKToleranceAspect.AddonVolume))
    {
      message = $"{message}Finish total volumes are not equal.  Total 1: {val1_1.ToString()} Total 2: {val2_1.ToString()}";
      return false;
    }
    foreach (string key in dictionary2.Keys)
    {
      if (!dictionary3.ContainsKey(key))
      {
        message = $"{message}Inst 2 finish types does not contain {key}";
        return false;
      }
      if (dictionary2[key] != dictionary3[key])
      {
        message = $"{message}Inst 1 finish types count for {key}: {dictionary2[key].ToString()} does not match Inst2 count: {dictionary3[key].ToString()}";
        return false;
      }
    }
    IEnumerable<EDGEAddonComponent> addonsForHost1 = (IEnumerable<EDGEAddonComponent>) MKComparisonEngine._MVData.GetAddonsForHost(inst1);
    IEnumerable<EDGEAddonComponent> addonsForHost2 = (IEnumerable<EDGEAddonComponent>) MKComparisonEngine._MVData.GetAddonsForHost(inst2);
    if (addonsForHost1.Count<EDGEAddonComponent>() != addonsForHost2.Count<EDGEAddonComponent>())
    {
      message = $"{message}Addon counts do not match: ElementId: {inst1.Id?.ToString()} addon count {addonsForHost1.Count<EDGEAddonComponent>().ToString()} not equal to ElementId: {inst2.Id?.ToString()} addon count: {addonsForHost2.Count<EDGEAddonComponent>().ToString()}";
      return false;
    }
    double val1_2 = addonsForHost1.Sum<EDGEAddonComponent>((Func<EDGEAddonComponent, double>) (s => s.MaterialVolume_InInternalUnits));
    double val2_2 = addonsForHost2.Sum<EDGEAddonComponent>((Func<EDGEAddonComponent, double>) (s => s.MaterialVolume_InInternalUnits));
    if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(val1_2, val2_2, MKToleranceAspect.AddonVolume))
    {
      message = $"{message}Addon total volumes are not equal.  Total 1: {val1_2.ToString()} Total 2: {val2_2.ToString()}";
      return false;
    }
    List<IGrouping<string, EDGEAddonComponent>> list3 = addonsForHost1.GroupBy<EDGEAddonComponent, string>((Func<EDGEAddonComponent, string>) (s => s.FamilyTypeName)).ToList<IGrouping<string, EDGEAddonComponent>>();
    List<IGrouping<string, EDGEAddonComponent>> list4 = addonsForHost2.GroupBy<EDGEAddonComponent, string>((Func<EDGEAddonComponent, string>) (s => s.FamilyTypeName)).ToList<IGrouping<string, EDGEAddonComponent>>();
    Dictionary<string, int> dictionary4 = new Dictionary<string, int>();
    foreach (IGrouping<string, EDGEAddonComponent> source in list3)
      dictionary4.Add(source.Key, source.Count<EDGEAddonComponent>());
    Dictionary<string, int> dictionary5 = new Dictionary<string, int>();
    foreach (IGrouping<string, EDGEAddonComponent> source in list4)
      dictionary5.Add(source.Key, source.Count<EDGEAddonComponent>());
    foreach (string key in dictionary4.Keys)
    {
      List<string> stringList = new List<string>();
      if (!dictionary5.ContainsKey(key))
      {
        message = $"{message}Inst 2 FamilyTypes does not contain {key}";
        return false;
      }
      if (dictionary4[key] != dictionary5[key])
      {
        message = $"{message}Inst 1 FamilyTypes count for {key}: {dictionary4[key].ToString()} does not match Inst2 count: {dictionary5[key].ToString()}";
        return false;
      }
    }
    IEnumerable<\u003C\u003Ef__AnonymousType0<string, double, Autodesk.Revit.DB.ElementId>> source1 = addonsForHost1.Select(s => new
    {
      key = s.FamilyTypeName + (s.MaterialId == (Autodesk.Revit.DB.ElementId) null ? "" : s.MaterialId.ToString()),
      data = s.MaterialVolume_InInternalUnits,
      id = s.Id
    });
    IEnumerable<\u003C\u003Ef__AnonymousType0<string, double, Autodesk.Revit.DB.ElementId>> source2 = addonsForHost2.Select(s => new
    {
      key = s.FamilyTypeName + (s.MaterialId == (Autodesk.Revit.DB.ElementId) null ? "" : s.MaterialId.ToString()),
      data = s.MaterialVolume_InInternalUnits,
      id = s.Id
    });
    foreach (string str in source1.Select(s => s.key).Distinct<string>())
    {
      string distinctKey = str;
      \u003C\u003Ef__AnonymousType0<string, double, Autodesk.Revit.DB.ElementId>[] array1 = source1.Where(s => s.key == distinctKey).OrderBy(s => s.data).ToArray();
      \u003C\u003Ef__AnonymousType0<string, double, Autodesk.Revit.DB.ElementId>[] array2 = source2.Where(s => s.key == distinctKey).OrderBy(s => s.data).ToArray();
      if (array1.Count() != array2.Count())
      {
        message = $"{message}Material Specific addon counts do not match for addons in {distinctKey}";
        return false;
      }
      for (int index = 0; index < array1.Count(); ++index)
      {
        if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(array1[index].data, array2[index].data, MKToleranceAspect.AddonVolume))
        {
          message = $"{message}Volume Mismatch for material specific addon comparison.  Key: {array2[index].key} with volume {array2[index].data.ToString()}";
          return false;
        }
      }
    }
    return true;
  }

  private static bool CompareMaterialVolumes(Autodesk.Revit.DB.FamilyInstance inst1, Autodesk.Revit.DB.FamilyInstance inst2)
  {
    if (!MKComparisonEngine.CompareParameters(inst1, inst2, MKToleranceAspect.Volume))
      return false;
    ICollection<Autodesk.Revit.DB.ElementId> materialIds1 = inst1.GetMaterialIds(false);
    ICollection<Autodesk.Revit.DB.ElementId> materialIds2 = inst2.GetMaterialIds(false);
    if (materialIds1.Count != materialIds2.Count)
      return false;
    foreach (Autodesk.Revit.DB.ElementId materialId in (IEnumerable<Autodesk.Revit.DB.ElementId>) materialIds1)
    {
      if (!materialIds2.Contains(materialId))
        return false;
      double materialVolume1 = inst1.GetMaterialVolume(materialId);
      double materialVolume2 = inst2.GetMaterialVolume(materialId);
      if (!MKComparisonEngine._toleranceComparer.NewValuesAreEqual(materialVolume1, materialVolume2, MKToleranceAspect.Volume))
        return false;
    }
    Parameter parameter1;
    Parameter parameter2;
    if (MKComparisonEngine.MemberVolumeCastParameterGuid != Guid.Empty)
    {
      parameter1 = inst1.get_Parameter(MKComparisonEngine.MemberVolumeCastParameterGuid);
      parameter2 = inst2.get_Parameter(MKComparisonEngine.MemberVolumeCastParameterGuid);
    }
    else
    {
      parameter1 = inst1.LookupParameter("MEMBER_VOLUME_CAST");
      parameter2 = inst2.LookupParameter("MEMBER_VOLUME_CAST");
      if (parameter1 != null)
        MKComparisonEngine.MemberVolumeCastParameterGuid = parameter1.GUID;
      else if (parameter2 != null)
        MKComparisonEngine.MemberVolumeCastParameterGuid = parameter2.GUID;
    }
    if (parameter1 == null || parameter2 == null)
      throw new Exception("MVCNULL");
    if (MKComparisonEngine._toleranceComparer.NewValuesAreEqual(parameter1.AsDouble(), parameter2.AsDouble(), MKToleranceAspect.Volume))
      return true;
    List<string> stringList = new List<string>();
    return false;
  }

  private static bool CompareAllParameters(Autodesk.Revit.DB.FamilyInstance inst1, Autodesk.Revit.DB.FamilyInstance inst2)
  {
    Autodesk.Revit.DB.FamilyInstance inst1_1 = inst1.SuperComponent != null ? inst1.SuperComponent as Autodesk.Revit.DB.FamilyInstance : inst1;
    Autodesk.Revit.DB.FamilyInstance inst2_1 = inst2.SuperComponent != null ? inst2.SuperComponent as Autodesk.Revit.DB.FamilyInstance : inst2;
    return MKComparisonEngine.CompareParameters(inst1_1, inst2_1, MKToleranceAspect.Geometry) && MKComparisonEngine.CompareParameters(inst1_1, inst2_1, MKToleranceAspect.FinishArea) && MKComparisonEngine.CompareParameters(inst1_1, inst2_1, MKToleranceAspect.Weight) && MKComparisonEngine.CompareParameters(inst1_1, inst2_1, MKToleranceAspect.Strength) && MKComparisonEngine.CompareParameters(inst1_1, inst2_1, MKToleranceAspect.Text);
  }

  private static bool CompareFamilyAndType(Autodesk.Revit.DB.FamilyInstance inst1, Autodesk.Revit.DB.FamilyInstance inst2)
  {
    return inst1.Symbol.FamilyName + inst1.Symbol.Name == inst2.Symbol.FamilyName + inst2.Symbol.Name;
  }

  public static Dictionary<string, MarkGroupResultData> RunInitialComparison(
    UIDocument uiDoc,
    IEnumerable<Autodesk.Revit.DB.FamilyInstance> SFElementsToProcess,
    HashSet<string> existingControlMarks,
    MarkQA MarkQAForInitial)
  {
    MKComparisonEngine.cancelOut = false;
    MKComparisonEngine._GeomVerificationStore.Clear();
    MKComparisonEngine.ConstructionProductGuid = Guid.Empty;
    MKComparisonEngine.MemberVolumeCastParameterGuid = Guid.Empty;
    MKComparisonEngine.parameterGuidDictionary = new Dictionary<string, Guid>();
    MKComparisonEngine._MVData = new MarkVerificationData(uiDoc.Document, ComparisonOption.DoNotRound);
    if (!MKComparisonEngine._MVData.IsValidForMarkVerification)
      return (Dictionary<string, MarkGroupResultData>) null;
    MKComparisonEngine.MarkQACopy = new MarkQA(MarkQAForInitial);
    MKComparisonEngine._uiDoc = uiDoc;
    Document document = MKComparisonEngine._uiDoc.Document;
    List<Autodesk.Revit.DB.FamilyInstance> list = SFElementsToProcess.ToList<Autodesk.Revit.DB.FamilyInstance>();
    MKComparisonEngine._toleranceComparer = new MKTolerances(ComparisonOption.DoNotRound, document);
    Dictionary<string, MarkGroupResultData> controlMarkBuckets = new Dictionary<string, MarkGroupResultData>();
    MKComparisonEngine._userSettingDictionaryPrefix = MKComparisonEngine.GetUserMarkPrefixSettingsDictionary(document);
    MKComparisonEngine._userSettingDictionarySuffix = MKComparisonEngine.GetUserMarkSuffixSettingsDictionary(document);
    foreach (Autodesk.Revit.DB.FamilyInstance familyInstance in list)
    {
      bool flag = false;
      foreach (string key in controlMarkBuckets.Keys)
      {
        string reason = "";
        if (controlMarkBuckets[key].GroupMembers.Any<Autodesk.Revit.DB.FamilyInstance>())
        {
          List<Autodesk.Revit.DB.FamilyInstance> groupMembers = controlMarkBuckets[key].GroupMembers;
          List<TestResult> results = MKComparisonEngine.RunComparisonTestsOnInstances(document, groupMembers, familyInstance, out reason);
          if (MKComparisonEngine.TestsPassed(results))
          {
            flag = true;
            if (MKComparisonEngine.TestsHadPlateRotationFailure(results))
              controlMarkBuckets[key].FailedPlateRotationComparison = true;
            if (MKComparisonEngine.TestsHadPlateUseCountMultiplier(results))
              controlMarkBuckets[key].CountMultiplierExcluded = true;
            if (!controlMarkBuckets[key].GroupMembers.Select<Autodesk.Revit.DB.FamilyInstance, string>((Func<Autodesk.Revit.DB.FamilyInstance, string>) (s => s.UniqueId)).Contains<string>(familyInstance.UniqueId))
            {
              controlMarkBuckets[key].GroupMembers.Add(familyInstance);
              break;
            }
            break;
          }
        }
      }
      if (!flag)
      {
        string constructionProduct = "";
        if (MKComparisonEngine.ConstructionProductGuid != Guid.Empty)
        {
          Parameter parameter = familyInstance.get_Parameter(MKComparisonEngine.ConstructionProductGuid);
          if (parameter != null && parameter.HasValue)
            constructionProduct = parameter.AsString();
        }
        if (string.IsNullOrWhiteSpace(constructionProduct))
        {
          Parameter parameter = Parameters.LookupParameter((Autodesk.Revit.DB.Element) familyInstance, "CONSTRUCTION_PRODUCT");
          if (parameter != null)
            MKComparisonEngine.ConstructionProductGuid = parameter.GUID;
          if (parameter.HasValue)
            constructionProduct = parameter.AsString();
        }
        string newTempControlMark = MKComparisonEngine.GetNewTempControlMark(familyInstance, existingControlMarks, MKComparisonEngine._userSettingDictionaryPrefix, MKComparisonEngine._userSettingDictionarySuffix, constructionProduct);
        controlMarkBuckets.Add(newTempControlMark, new MarkGroupResultData()
        {
          GroupMembers = {
            familyInstance
          },
          ConstructionProduct = constructionProduct
        });
      }
    }
    return MKComparisonEngine.UpdateControlMarkFromControlNumber2(controlMarkBuckets, existingControlMarks);
  }

  public static int GetIncrementorDetails(out string leadingZeroes, string incrementor = "")
  {
    string str = string.IsNullOrEmpty(incrementor) ? MKComparisonEngine._incrementor : incrementor;
    leadingZeroes = new string('0', str.Length);
    try
    {
      return Convert.ToInt32(str);
    }
    catch
    {
      leadingZeroes = "00";
      return 1;
    }
  }

  private static Dictionary<string, MarkGroupResultData> UpdateControlMarkFromControlNumber2(
    Dictionary<string, MarkGroupResultData> controlMarkBuckets,
    HashSet<string> existingControlMarks)
  {
    Dictionary<string, MarkGroupResultData> dictionary = new Dictionary<string, MarkGroupResultData>();
    foreach (IEnumerable<MarkGroupResultData> me in controlMarkBuckets.Values.GroupBy<MarkGroupResultData, string>((Func<MarkGroupResultData, string>) (s => s.ConstructionProduct)).ToList<IGrouping<string, MarkGroupResultData>>())
    {
      List<MarkGroupResultData> list = me.NatrualSort<MarkGroupResultData>((Func<MarkGroupResultData, string>) (s => s.GetLowestControlNumber())).ToList<MarkGroupResultData>();
      string leadingZeroes;
      int incrementorDetails = MKComparisonEngine.GetIncrementorDetails(out leadingZeroes);
      foreach (MarkGroupResultData markGroupResultData in list)
      {
        string constructionProduct1 = MKComparisonEngine.GetMarkPrefixGivenConstructionProduct(markGroupResultData.ConstructionProduct);
        string constructionProduct2 = MKComparisonEngine.GetMarkSuffixGivenConstructionProduct(markGroupResultData.ConstructionProduct);
        string str;
        do
        {
          str = MKComparisonEngine._letterIncrementor ? $"{constructionProduct1}{MKComparisonEngine.intToLetterIncrement(incrementorDetails)}{constructionProduct2}" : string.Format($"{{0}}{{1:{leadingZeroes}}}{{2}}", (object) constructionProduct1, (object) incrementorDetails, (object) constructionProduct2);
          ++incrementorDetails;
        }
        while (existingControlMarks.Contains(str));
        markGroupResultData.ControlMark = str;
        dictionary.Add(markGroupResultData.ControlMark, markGroupResultData);
        existingControlMarks.Add(str);
      }
    }
    return dictionary;
  }

  public static Dictionary<string, string> GetUserMarkPrefixSettingsDictionary(
    Document revitDoc,
    out string incrementor)
  {
    if (MKComparisonEngine._userSettingDictionaryPrefix != null)
      MKComparisonEngine._userSettingDictionaryPrefix.Clear();
    else
      MKComparisonEngine._userSettingDictionaryPrefix = new Dictionary<string, string>();
    Dictionary<string, string> settingsDictionary = Utils.SettingsUtils.Settings.LoadMarkVerificationSettings(revitDoc, App.MarkPrefixFolderPath, out MKComparisonEngine._incrementor, out MKComparisonEngine._letterIncrementor);
    incrementor = MKComparisonEngine._incrementor;
    return settingsDictionary;
  }

  public static Dictionary<string, string> GetUserMarkSuffixSettingsDictionary(
    Document revitDoc,
    out string incrementor)
  {
    if (MKComparisonEngine._userSettingDictionarySuffix != null)
      MKComparisonEngine._userSettingDictionarySuffix.Clear();
    else
      MKComparisonEngine._userSettingDictionarySuffix = new Dictionary<string, string>();
    Dictionary<string, string> settingsDictionary = Utils.SettingsUtils.Settings.LoadMarkVerificationSettings(revitDoc, App.MarkPrefixFolderPath, out MKComparisonEngine._incrementor, out MKComparisonEngine._letterIncrementor, true);
    incrementor = MKComparisonEngine._incrementor;
    return settingsDictionary;
  }

  public static Dictionary<string, string> GetUserMarkPrefixSettingsDictionary(Document revitDoc)
  {
    return MKComparisonEngine.GetUserMarkPrefixSettingsDictionary(revitDoc, out string _);
  }

  public static Dictionary<string, string> GetUserMarkSuffixSettingsDictionary(Document revitDoc)
  {
    return MKComparisonEngine.GetUserMarkSuffixSettingsDictionary(revitDoc, out string _);
  }

  private static bool TestsPassed(List<TestResult> results)
  {
    if (results == null || results.Count == 0)
      return false;
    if (results.Count <= 0)
      return results.Last<TestResult>().Passed;
    foreach (TestResult result in results)
    {
      if (!result.Passed)
        return false;
    }
    return results.Last<TestResult>().Passed;
  }

  private static bool TestsHadPlateRotationFailure(List<TestResult> results)
  {
    bool flag = false;
    foreach (TestResult result in results)
    {
      if (result.FailedPlateRotationTest)
      {
        flag = true;
        break;
      }
    }
    return flag;
  }

  private static bool TestsHadPlateUseCountMultiplier(List<TestResult> results)
  {
    bool flag = false;
    foreach (TestResult result in results)
    {
      if (result.EmbedsExcludedBool)
      {
        flag = true;
        break;
      }
    }
    return flag;
  }

  private static string GetMarkPrefixGivenConstructionProduct(string constructionProduct)
  {
    string constructionProduct1 = constructionProduct;
    if (MKComparisonEngine._userSettingDictionaryPrefix.ContainsKey(constructionProduct) && MKComparisonEngine._userSettingDictionaryPrefix[constructionProduct] != "")
      constructionProduct1 = MKComparisonEngine._userSettingDictionaryPrefix[constructionProduct];
    return constructionProduct1;
  }

  private static string GetMarkSuffixGivenConstructionProduct(string constructionProduct)
  {
    string constructionProduct1 = "";
    if (MKComparisonEngine._userSettingDictionarySuffix.ContainsKey(constructionProduct) && MKComparisonEngine._userSettingDictionarySuffix[constructionProduct] != "")
      constructionProduct1 = MKComparisonEngine._userSettingDictionarySuffix[constructionProduct];
    return constructionProduct1;
  }

  private static string GetNewTempControlMark(
    Autodesk.Revit.DB.FamilyInstance incomingInstance,
    HashSet<string> existingControlMarks,
    Dictionary<string, string> userSettingDictionaryPrefix,
    Dictionary<string, string> userSettingDictionarySuffix,
    string constructionProduct = null)
  {
    string key = "";
    if (constructionProduct == null)
    {
      if (MKComparisonEngine.ConstructionProductGuid != Guid.Empty)
      {
        Parameter parameter = incomingInstance.get_Parameter(MKComparisonEngine.ConstructionProductGuid);
        if (parameter != null && parameter.HasValue)
          key = parameter.AsString();
      }
      if (string.IsNullOrWhiteSpace(constructionProduct))
      {
        Parameter parameter = Parameters.LookupParameter((Autodesk.Revit.DB.Element) incomingInstance, "CONSTRUCTION_PRODUCT");
        if (parameter != null)
          MKComparisonEngine.ConstructionProductGuid = parameter.GUID;
        if (parameter.HasValue)
          key = parameter.AsString();
      }
    }
    else
      key = constructionProduct;
    string str = "";
    if (userSettingDictionaryPrefix.ContainsKey(key) && userSettingDictionaryPrefix[key] != "")
      key = userSettingDictionaryPrefix[key];
    if (userSettingDictionarySuffix.ContainsKey(key) && userSettingDictionarySuffix[key] != "")
      key = userSettingDictionarySuffix[key];
    string leadingZeroes;
    int incrementorDetails = MKComparisonEngine.GetIncrementorDetails(out leadingZeroes);
    string newTempControlMark;
    do
    {
      newTempControlMark = MKComparisonEngine._letterIncrementor ? $"{key}{MKComparisonEngine.intToLetterIncrement(incrementorDetails)}{str}_initial" : string.Format($"{{0}}{{1:{leadingZeroes}}}{{2}}_initial", (object) key, (object) incrementorDetails, (object) str);
      ++incrementorDetails;
    }
    while (existingControlMarks.Contains(newTempControlMark));
    existingControlMarks.Add(newTempControlMark);
    return newTempControlMark;
  }

  private static bool CompareParameters(
    Autodesk.Revit.DB.FamilyInstance inst1,
    Autodesk.Revit.DB.FamilyInstance inst2,
    MKToleranceAspect compareType)
  {
    List<string> parameterNameStrings;
    switch (compareType)
    {
      case MKToleranceAspect.Volume:
        parameterNameStrings = MKUtils.VolumeParameterNameStrings;
        break;
      case MKToleranceAspect.FinishArea:
        parameterNameStrings = MKUtils.AreaParameterNameStrings;
        break;
      case MKToleranceAspect.Weight:
        parameterNameStrings = MKUtils.WeightParameterNameStrings;
        break;
      case MKToleranceAspect.Strength:
        parameterNameStrings = MKUtils.StrengthParameterNameStrings;
        break;
      case MKToleranceAspect.Geometry:
        parameterNameStrings = MKUtils.LengthParameterNameStrings;
        break;
      default:
        parameterNameStrings = MKUtils.TextParameterNameStrings;
        break;
    }
    foreach (string str in parameterNameStrings)
    {
      Parameter parameter1;
      Parameter parameter2;
      if (MKComparisonEngine.parameterGuidDictionary.ContainsKey(str))
      {
        Guid parameterGuid = MKComparisonEngine.parameterGuidDictionary[str];
        parameter1 = inst1.get_Parameter(parameterGuid);
        parameter2 = inst2.get_Parameter(parameterGuid);
      }
      else
      {
        parameter1 = inst1.LookupParameter(str);
        parameter2 = inst2.LookupParameter(str);
        if (parameter1 != null)
          MKComparisonEngine.parameterGuidDictionary.Add(str, parameter1.GUID);
        else if (parameter2 != null)
          MKComparisonEngine.parameterGuidDictionary.Add(str, parameter2.GUID);
      }
      if (parameter1 != null || parameter2 != null)
      {
        if (parameter1 == null || parameter2 == null)
          return false;
        if (compareType == MKToleranceAspect.Weight)
        {
          double val1 = UnitUtils.ConvertFromInternalUnits(parameter1.AsDouble(), UnitTypeId.PoundsForce);
          double val2 = UnitUtils.ConvertFromInternalUnits(parameter2.AsDouble(), UnitTypeId.PoundsForce);
          if (MKComparisonEngine._toleranceComparer.NewValuesAreEqual(val1, val2, compareType))
            continue;
        }
        if ((compareType == MKToleranceAspect.Weight || compareType == MKToleranceAspect.Text || !MKComparisonEngine._toleranceComparer.NewValuesAreEqual(parameter1.AsDouble(), parameter2.AsDouble(), compareType)) && (compareType != MKToleranceAspect.Text || !(parameter1.AsString() == parameter2.AsString())))
          return false;
      }
    }
    return true;
  }

  private static string intToLetterIncrement(int value)
  {
    List<int> intList = new List<int>() { 0 };
    string letterIncrement = "";
    int baseInt = MKComparisonEngine.alphabetList.Count<string>();
    int num1 = value;
    if (num1 <= baseInt)
      return MKComparisonEngine.alphabetList[num1 - 1];
    for (; num1 > baseInt; num1 -= baseInt)
      intList = MKComparisonEngine.incrementDigitList(intList, baseInt);
    intList[0] = num1;
    intList.Reverse(0, intList.Count<int>());
    foreach (int num2 in intList)
      letterIncrement += MKComparisonEngine.alphabetList[num2 - 1];
    return letterIncrement;
  }

  private static List<int> incrementDigitList(List<int> digitList, int baseInt)
  {
    int index = 1;
    bool flag = false;
    while (!flag)
    {
      if (digitList.Count < index + 1)
      {
        digitList.Add(1);
        flag = true;
      }
      else if (digitList[index] == baseInt)
      {
        digitList[index] = 1;
        ++index;
      }
      else
      {
        ++digitList[index];
        flag = true;
      }
    }
    return digitList;
  }

  public class EmptyStringsAreLast : IComparer<string>
  {
    public int Compare(string x, string y)
    {
      if (string.IsNullOrEmpty(y) && !string.IsNullOrEmpty(x))
        return -1;
      return !string.IsNullOrEmpty(y) && string.IsNullOrEmpty(x) ? 1 : string.Compare(x, y);
    }
  }
}
