// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationMarking.InsulationMarkingMain
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationMarking;

[Transaction(TransactionMode.Manual)]
public class InsulationMarkingMain : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    Document document1 = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    bool bMetric = Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(document1);
    if (document1.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Insulation Marking Tool must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Insulation Marking must be run in the project environment.  Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document1, "Insulation Marking"))
    {
      int num1 = (int) transactionGroup.Start();
      TaskDialog taskDialog = new TaskDialog("Insulation Marking");
      taskDialog.Title = "Insulation Marking";
      taskDialog.TitleAutoPrefix = true;
      taskDialog.AllowCancellation = true;
      taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
      taskDialog.MainInstruction = "Insulation Marking";
      taskDialog.MainContent = "Select the scope for marking insulation.";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Mark Insulation for the Whole Project.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Mark Insulation for the Active View");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Mark Insulation for a Selection Group");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      Result result = (Result) 0;
      List<AssemblyInstance> assemblyInstanceList;
      if (taskDialogResult == 1001)
        assemblyInstanceList = InsulationMarkingScope.WholeModel(document1, "Insulation Marking");
      else if (taskDialogResult == 1002)
        assemblyInstanceList = InsulationMarkingScope.ActiveModel(document1, activeUiDocument);
      else if (taskDialogResult == 1003)
      {
        assemblyInstanceList = InsulationMarkingScope.SelectionList(document1, activeUiDocument, "Insulation Marking");
        if (assemblyInstanceList == null)
          return (Result) 1;
      }
      else
      {
        int num2 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      if (result == 1 || result == -1)
      {
        int num3 = (int) transactionGroup.RollBack();
        return result;
      }
      if (assemblyInstanceList.Count == 0)
      {
        new TaskDialog("Warning")
        {
          MainContent = "There were no Assembly Instance(s) found in the selection."
        }.Show();
        return (Result) 1;
      }
      Dictionary<AssemblyInstance, List<Element>> dictionary1 = new Dictionary<AssemblyInstance, List<Element>>();
      Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
      Dictionary<string, List<string>> dictionary3 = new Dictionary<string, List<string>>();
      List<string> values1 = new List<string>();
      using (Transaction transaction = new Transaction(document1, "Insulation Marking"))
      {
        bool flag1 = false;
        Dictionary<string, List<string>> dictionary4 = new Dictionary<string, List<string>>();
        List<string> stringList = new List<string>();
        List<string> values2 = new List<string>();
        foreach (AssemblyInstance assInst in assemblyInstanceList)
        {
          List<ElementId> elementIdList = Utils.SelectionUtils.SelectionUtils.processInsulationForStackedFamilies(document1, assInst.GetMemberIds(), out List<ElementId> _);
          if (elementIdList.Count == 0)
          {
            values2.Add(assInst.Name);
          }
          else
          {
            List<XYZ> source = new List<XYZ>();
            Dictionary<Element, XYZ> dictionary5 = new Dictionary<Element, XYZ>();
            bool flag2 = false;
            Element element1 = (Element) null;
            List<Family> familyList = new List<Family>();
            bool flag3 = false;
            Element topLevelElement = assInst.GetStructuralFramingElement().GetTopLevelElement();
            foreach (ElementId id in elementIdList)
            {
              Element element2 = document1.GetElement(id);
              FamilyInstance familyInstance = element2 as FamilyInstance;
              string familyName = familyInstance.Symbol.FamilyName;
              if (familyList.Count > 0)
              {
                bool flag4 = false;
                foreach (Element element3 in familyList)
                {
                  if (element3.Name.Equals(familyName))
                  {
                    flag4 = true;
                    break;
                  }
                }
                if (!flag4)
                  familyList.Add(familyInstance.Symbol.Family);
              }
              else
                familyList.Add(familyInstance.Symbol.Family);
              string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element2, "HOST_GUID");
              if (!string.IsNullOrEmpty(parameterAsString) && !topLevelElement.UniqueId.Equals(parameterAsString))
              {
                flag3 = true;
                break;
              }
            }
            if (flag3)
            {
              values1.Add(assInst.Name);
            }
            else
            {
              foreach (Family loadedFamily in familyList)
              {
                Document document2 = document1.EditFamily(loadedFamily);
                FamilyParameter familyParameter = document2.FamilyManager.get_Parameter("INSULATION_MARK");
                if (familyParameter != null && !familyParameter.IsInstance && !stringList.Contains(loadedFamily.Name))
                  stringList.Add(loadedFamily.Name);
                document2.Close(false);
              }
              int num4 = (int) transaction.Start();
              foreach (ElementId id in elementIdList)
              {
                Element element4 = document1.GetElement(id);
                FamilyInstance familyInstance = element4 as FamilyInstance;
                bool flag5 = false;
                foreach (string str in stringList)
                {
                  if (str.Equals(familyInstance.Symbol.FamilyName))
                  {
                    flag5 = true;
                    break;
                  }
                }
                if (!flag5)
                {
                  if (element4.GetElementVolume() <= 0.0)
                  {
                    string str = $"{(element4 as FamilyInstance).Symbol.FamilyName} : {element4.Id?.ToString()}";
                    if (dictionary2.ContainsKey(assInst.Name))
                      dictionary2[assInst.Name].Add(str);
                    else
                      dictionary2.Add(assInst.Name, new List<string>()
                      {
                        str
                      });
                  }
                  else
                  {
                    string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element4, "HOST_GUID");
                    Element element5 = document1.GetElement(parameterAsString);
                    if (element5 == null)
                    {
                      string str = $"{(element4 as FamilyInstance).Symbol.FamilyName} : {id?.ToString()}";
                      if (dictionary3.ContainsKey(assInst.Name))
                        dictionary3[assInst.Name].Add(str);
                      else
                        dictionary3.Add(assInst.Name, new List<string>()
                        {
                          str
                        });
                    }
                    else
                    {
                      element1 = element5;
                      if (Utils.ElementUtils.Parameters.LookupParameter(element4, "INSULATION_MARK") == null)
                      {
                        string str = $"{(element4 as FamilyInstance).Symbol.FamilyName} : {element4.Id?.ToString()}";
                        if (dictionary4.ContainsKey(assInst.Name))
                          dictionary4[assInst.Name].Add(str);
                        else
                          dictionary4.Add(assInst.Name, new List<string>()
                          {
                            str
                          });
                      }
                      else
                      {
                        flag2 = (element1 as FamilyInstance).IsWorkPlaneFlipped;
                        XYZ xyz = InsulationMarkingMain.LeftMostProjectedPoint(element1, element4, document1);
                        dictionary5.Add(element4, xyz);
                        source.Add(xyz);
                      }
                    }
                  }
                }
              }
              if (source.Count > 0)
              {
                List<XYZ> xyzList1 = new List<XYZ>();
                List<XYZ> xyzList2 = flag2 || PlacementUtilities.IsMirrored(element1) ? source.OrderByDescending<XYZ, double>((Func<XYZ, double>) (e => PlacementUtilities.RoundValue(e.Z, bMetric))).ThenBy<XYZ, double>((Func<XYZ, double>) (p => PlacementUtilities.RoundValue(p.X, bMetric))).ToList<XYZ>() : source.OrderByDescending<XYZ, double>((Func<XYZ, double>) (e => PlacementUtilities.RoundValue(e.Z, bMetric))).ThenByDescending<XYZ, double>((Func<XYZ, double>) (p => PlacementUtilities.RoundValue(p.X, bMetric))).ToList<XYZ>();
                int num5 = 0 + 1;
                (element1 as FamilyInstance).GetTransform();
                Dictionary<ElementId, int> dictionary6 = new Dictionary<ElementId, int>();
                foreach (XYZ xyz in xyzList2)
                {
                  bool flag6 = false;
                  for (int index = 0; index < dictionary5.Keys.Count; ++index)
                  {
                    Element element6 = dictionary5.Keys.ElementAt<Element>(index);
                    FamilyInstance familyInstance = element6 as FamilyInstance;
                    if (xyz.IsAlmostEqualTo(dictionary5[element6]))
                    {
                      if (familyInstance != null)
                      {
                        if (familyInstance.SuperComponent != null)
                        {
                          Element superComponent = (element6 as FamilyInstance).SuperComponent;
                          foreach (ElementId elementId in (superComponent as FamilyInstance).GetSubComponentIds().ToList<ElementId>())
                          {
                            ElementId id = elementId;
                            Element element7 = dictionary5.Keys.Where<Element>((Func<Element, bool>) (e => e.Id.Equals((object) id))).FirstOrDefault<Element>();
                            if (element7 != null)
                            {
                              Utils.ElementUtils.Parameters.LookupParameter(element7, "INSULATION_MARK").Set(num5.ToString());
                              dictionary5.Remove(element7);
                              flag6 = true;
                            }
                          }
                          if (Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(superComponent, true))
                            Utils.ElementUtils.Parameters.LookupParameter(superComponent, "INSULATION_MARK").Set(num5.ToString());
                        }
                        else
                        {
                          Utils.ElementUtils.Parameters.LookupParameter(element6, "INSULATION_MARK").Set(num5.ToString());
                          dictionary5.Remove(element6);
                          flag6 = true;
                        }
                      }
                      if (flag6)
                        break;
                    }
                  }
                  if (flag6)
                    ++num5;
                }
              }
              int num6 = (int) transaction.Commit();
            }
          }
        }
        if (values1.Count > 0)
          new TaskDialog("Warning")
          {
            MainContent = "The following Assembly Instance(s) contain insulation pieces where the HOST_GUID parameter is invalid and therefore were not processed. Please check and try again.",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) values1)
          }.Show();
        if (values2.Count > 0)
          new TaskDialog("Warning")
          {
            MainContent = "There were no insulation pieces found in the following Assembly Instance(s).",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) values2)
          }.Show();
        if (stringList.Count > 0)
        {
          string str1 = "";
          foreach (string str2 in stringList)
            str1 = $"{str1}\t{str2}\n";
          new TaskDialog("Warning")
          {
            MainContent = "Please check the following insulation families. The INSULATION_MARK parameter is set as a Type parameter on these families and insulation pieces belonging to them were not processed.",
            ExpandedContent = str1
          }.Show();
        }
        if (dictionary4.Keys.Count > 0)
        {
          string str3 = "";
          foreach (string key in dictionary4.Keys)
          {
            str3 = $"{str3}{key} : \n";
            foreach (string str4 in dictionary4[key])
              str3 = $"{str3}\t{str4}\n";
          }
          new TaskDialog("Warning")
          {
            MainContent = "Please check the following insulation pieces in Assembiles mentioned below. These pieces were missing INSULATION_MARK parameter and therefore were not processed.",
            ExpandedContent = str3
          }.Show();
        }
        if (dictionary2.Keys.Count > 0)
        {
          string str5 = "";
          foreach (string key in dictionary2.Keys)
          {
            str5 = $"{str5}{key} : \n";
            foreach (string str6 in dictionary2[key])
              str5 = $"{str5}\t{str6}\n";
          }
          new TaskDialog("Warning")
          {
            MainContent = "One or more insulation pieces in the following Assembly Instances had zero volume and will not get processed in Insulation Marking.",
            ExpandedContent = str5
          }.Show();
        }
        if (dictionary3.Keys.Count > 0)
        {
          string str7 = "";
          foreach (string key in dictionary3.Keys)
          {
            str7 = $"{str7}{key} : \n";
            foreach (string str8 in dictionary3[key])
              str7 = $"{str7}\t{str8}\n";
          }
          new TaskDialog("Warning")
          {
            MainContent = "The following piece(s) of insulation in Assembly Instance(s) are not hosted to a valid wall panel and therefore were not processed. Please run BOM_PRODUCT_HOST and try again.",
            ExpandedContent = str7
          }.Show();
        }
        if (flag1)
        {
          int num7 = (int) MessageBox.Show("All insulation pieces have been marked successfully.");
        }
      }
      int num8 = (int) transactionGroup.Assimilate();
    }
    return (Result) 0;
  }

  public static bool checkRotatedOrientation(
    Element insulationElement,
    Element sfElement,
    bool noReEntrant)
  {
    Transform transform1 = (insulationElement as FamilyInstance).GetTransform();
    Transform transform2 = (sfElement as FamilyInstance).GetTransform();
    XYZ basisY1 = transform2.BasisY;
    XYZ basisX = transform2.BasisX;
    XYZ basisY2 = transform1.BasisY;
    XYZ source = basisY2;
    double num1 = Math.Round(basisX.AngleTo(source) * 180.0) / Math.PI;
    double num2 = 180.0 - num1;
    double num3 = num1 >= num2 ? num2 : num1;
    double num4 = Math.Round(basisY1.AngleTo(basisY2) * 180.0) / Math.PI;
    double num5 = 180.0 - num4;
    double num6 = num4 >= num5 ? num5 : num4;
    return !noReEntrant ? num3 > num6 : num3 < num6;
  }

  public static XYZ LeftMostProjectedPoint(
    Element wallPanel,
    Element insulation,
    Document revitDoc)
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
}
