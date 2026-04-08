// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TitleblockPopCore
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.UserSettingTools.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

internal class TitleblockPopCore
{
  private static ViewSheet workingSheetView;

  public static Result PopulateTicketTitleBlock(
    UIDocument uidoc,
    ref string message,
    ViewSheet sheetView,
    out List<Element> invalidWeightInputsOut,
    out List<Element> weightParametesDoNotExistOut,
    bool bShowErrors = true)
  {
    ActiveModel.GetInformation(uidoc);
    Document document = uidoc.Document;
    TitleblockPopCore.workingSheetView = sheetView;
    int result = 0;
    invalidWeightInputsOut = new List<Element>();
    weightParametesDoNotExistOut = new List<Element>();
    StringBuilder stringBuilder1 = new StringBuilder();
    StringBuilder stringBuilder2 = new StringBuilder();
    List<ElementId> elementIdList = new List<ElementId>();
    ICollection<Element> source = StructuralFraming.RefineNestedFamilies(document);
    using (SubTransaction subTransaction = new SubTransaction(document))
    {
      int num1 = (int) subTransaction.Start();
      Line bound = Line.CreateBound(XYZ.Zero, XYZ.BasisX);
      ElementId id = document.Create.NewDetailCurve((View) TitleblockPopCore.workingSheetView, (Curve) bound).Id;
      document.Delete(id);
      try
      {
        AssemblyInstance element = document.GetElement(TitleblockPopCore.workingSheetView.AssociatedAssemblyInstanceId) as AssemblyInstance;
        App.UpdateSheetCountParameter(element, document);
        if (element.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(element.Document.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320)).Count<Element>() > 1)
        {
          new TaskDialog("Structural Framing Element Error")
          {
            MainContent = $"Please check assembly instance {element.Name}, it contains more than one structural framing element. This operation will be cancelled."
          }.Show();
          return (Result) 1;
        }
        Parameter parameter1 = (Parameter) null;
        if (element != null)
        {
          if (element.LookupParameter("ASSEMBLY_MARK_NUMBER") != null)
            parameter1 = element.LookupParameter("ASSEMBLY_MARK_NUMBER");
          else if (document.GetElement(element.GetTypeId()).LookupParameter("ASSEMBLY_MARK_NUMBER") != null)
            parameter1 = document.GetElement(element.GetTypeId()).LookupParameter("ASSEMBLY_MARK_NUMBER");
        }
        string cmValue = "";
        if (parameter1 != null)
          cmValue = parameter1.AsString();
        List<string> list1 = source.Where<Element>((Func<Element, bool>) (sfElem => sfElem.LookupParameter("CONTROL_MARK") != null && sfElem.LookupParameter("CONTROL_MARK").HasValue && sfElem.LookupParameter("CONTROL_MARK").AsString().Equals(cmValue) && sfElem.LookupParameter("CONTROL_NUMBER") != null && sfElem.LookupParameter("CONTROL_NUMBER").HasValue)).Select<Element, string>((Func<Element, string>) (sfElem => sfElem.LookupParameter("CONTROL_NUMBER").AsString())).Distinct<string>().ToList<string>();
        List<ElementId> list2 = source.Where<Element>((Func<Element, bool>) (sfElem =>
        {
          if (sfElem.LookupParameter("CONTROL_MARK") == null || !sfElem.LookupParameter("CONTROL_MARK").HasValue || !sfElem.LookupParameter("CONTROL_MARK").AsString().Equals(cmValue))
            return false;
          return sfElem.LookupParameter("CONTROL_NUMBER") == null || !sfElem.LookupParameter("CONTROL_NUMBER").HasValue || sfElem.LookupParameter("CONTROL_NUMBER").AsString().Equals("") || sfElem.LookupParameter("CONTROL_NUMBER").AsString().Equals(" ");
        })).Select<Element, ElementId>((Func<Element, ElementId>) (sfElem => sfElem.Id)).ToList<ElementId>();
        List<string> list3 = list1.OrderBy<string, string>((Func<string, string>) (controlNum => controlNum)).ToList<string>();
        list3.Count<string>();
        foreach (string str in list3)
          stringBuilder1.Append(str + ", ");
        foreach (ElementId elementId in list2)
          stringBuilder2.Append(elementId?.ToString() + ", ");
        if (stringBuilder1.Length > 0)
          stringBuilder1.Remove(stringBuilder1.Length - 2, 1);
        else
          new TaskDialog("Control Numbers")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "There are no matching Control Numbers for this Assembly."
          }.Show();
        string[] separator = new string[1]{ ", " };
        string[] strArray = stringBuilder1.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
        foreach (Parameter orderedParameter in (IEnumerable<Parameter>) TitleblockPopCore.workingSheetView.GetOrderedParameters())
        {
          string name = orderedParameter.Definition.Name;
          if (name.Contains("TKT_CONTROL_") && !name.Equals("TKT_CONTROL_REQD_TOTAL"))
          {
            orderedParameter.Set("");
            int.TryParse(name.Substring(12, 2), out result);
          }
        }
        if (result != 0)
        {
          if (strArray.Length <= result)
          {
            for (int index = 0; index < strArray.Length; ++index)
            {
              string name = "TKT_CONTROL_0" + (index + 1).ToString();
              if (index == 9)
                name = "TKT_CONTROL_10";
              if (index > 9)
                name = "TKT_CONTROL_" + (index + 1).ToString();
              TitleblockPopCore.workingSheetView.LookupParameter(name)?.Set(strArray[index]);
            }
          }
          else
          {
            List<string> stringList = new List<string>();
            List<string> second = new List<string>();
            for (int index = 0; index < strArray.Length; ++index)
            {
              if (int.TryParse(strArray[index], out int _))
                stringList.Add(strArray[index]);
              else
                second.Add(strArray[index]);
            }
            string[] array = ((IEnumerable<string>) TitleblockPopCore.FindSeries(stringList.ToArray())).ToList<string>().Union<string>((IEnumerable<string>) second).ToArray<string>();
            for (int index = 0; index < array.Length; ++index)
            {
              string name = "TKT_CONTROL_0" + (index + 1).ToString();
              if (index == 9)
                name = "TKT_CONTROL_10";
              if (index > 9)
                name = "TKT_CONTROL_" + (index + 1).ToString();
              TitleblockPopCore.workingSheetView.LookupParameter(name)?.Set(array[index]);
            }
          }
        }
        else
        {
          string text = $"CONTROL NUMBERS: {Environment.NewLine}{stringBuilder1?.ToString()}";
          XYZ xyz = uidoc.Selection.PickPoint("Pick the top left corner of the TextNote to be created.");
          TextNote.Create(document, document.ActiveView.Id, new XYZ(xyz.X + 0.25, xyz.Y, xyz.Z), 0.25, text, new TextNoteOptions()
          {
            HorizontalAlignment = HorizontalTextAlignment.Right,
            TypeId = document.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType)
          }).get_Parameter(BuiltInParameter.TEXT_ALIGN_HORZ).Set(64 /*0x40*/);
        }
        List<IGrouping<string, Element>> list4 = TitleblockPopCore.GroupByControlMark((IEnumerable<Element>) StructuralFraming.GetFilteredElements(document).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).SuperComponent == null)).ToList<Element>()).ToList<IGrouping<string, Element>>();
        int quantity = TitleblockPopCore.GetQuantity(Parameters.GetParameterAsString((Element) element, "ASSEMBLY_MARK_NUMBER"), (IEnumerable<IGrouping<string, Element>>) list4);
        string str1 = "";
        Parameter parameter2 = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
        if (parameter2 != null && !string.IsNullOrEmpty(parameter2.AsString()))
          str1 = parameter2.AsString();
        string str2 = string.IsNullOrEmpty(App.TBPopFolderPath) ? $"C:/EDGEforREVIT/{str1}_TitleBlock_Mapping.xml" : $"{App.TBPopFolderPath}\\{str1}_TitleBlock_Mapping.xml";
        TBParameterSettings parameterSettings = new TBParameterSettings();
        List<Element> invalidWeightInputs = new List<Element>();
        List<Element> weightParametesDoNotExist = new List<Element>();
        if (File.Exists(str2))
        {
          if (parameterSettings.LoadTicketTemplateSettings(str2))
          {
            List<DatagridItemEntry> tbParameterList = parameterSettings.TBParameterList;
            if (!TitleblockPopCore.SetUserDefinedTitleBlockParameters(document, quantity, tbParameterList, out invalidWeightInputs, out weightParametesDoNotExist, bShowErrors))
              return (Result) 1;
            if (invalidWeightInputs.Count > 0)
              invalidWeightInputsOut.AddRange((IEnumerable<Element>) invalidWeightInputs);
            if (weightParametesDoNotExist.Count > 0)
              weightParametesDoNotExistOut.AddRange((IEnumerable<Element>) weightParametesDoNotExist);
          }
        }
        else if (!TitleblockPopCore.SetTitleBlockParameters(document, quantity, bShowErrors))
          return (Result) 1;
        if (subTransaction.Commit() != TransactionStatus.Committed)
          return (Result) -1;
        if (document.IsWorkshared)
        {
          ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
          elementsToCheckout.Add(TitleblockPopCore.workingSheetView.Id);
          WorksharingUtils.CheckoutElements(document, elementsToCheckout);
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (subTransaction.HasStarted())
        {
          int num2 = (int) subTransaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }

  private static IEnumerable<IGrouping<string, Element>> GroupByControlMark(
    IEnumerable<Element> structFramingElems)
  {
    Document doc = ActiveModel.Document;
    return structFramingElems.Where<Element>((Func<Element, bool>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")) != null)).GroupBy<Element, string>((Func<Element, string>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")).AsString()));
  }

  public static int GetQuantity(
    string assemblyMarkNumber,
    IEnumerable<IGrouping<string, Element>> structFramingGroups)
  {
    using (IEnumerator<IGrouping<string, Element>> enumerator = structFramingGroups.Where<IGrouping<string, Element>>((Func<IGrouping<string, Element>, bool>) (group => group.Key.Equals(assemblyMarkNumber))).GetEnumerator())
    {
      if (enumerator.MoveNext())
        return enumerator.Current.ToList<Element>().Count;
    }
    return 0;
  }

  private static bool SetUserDefinedTitleBlockParameters(
    Document doc,
    int totalRequired,
    List<DatagridItemEntry> userDefinedMappings,
    out List<Element> invalidWeightInputs,
    out List<Element> weightParametesDoNotExist,
    bool bShowErrors = true)
  {
    Element element1 = (Element) null;
    AssemblyInstance element2 = doc.GetElement(TitleblockPopCore.workingSheetView.AssociatedAssemblyInstanceId) as AssemblyInstance;
    ElementId elementId1 = new ElementId(BuiltInCategory.OST_StructuralFraming);
    foreach (ElementId memberId in (IEnumerable<ElementId>) element2.GetMemberIds())
    {
      Element element3 = doc.GetElement(memberId);
      if (element3.Category.Id.Equals((object) elementId1))
      {
        element1 = element3;
        break;
      }
    }
    Dictionary<PrecastMaterialType, ElementId> dictionary1 = Materials.GetPrecastMaterialIds(doc);
    Dictionary<ElementId, double> dictionary2 = new Dictionary<ElementId, double>();
    double num1 = 0.0;
    if (dictionary1 != null)
    {
      foreach (PrecastMaterialType key in dictionary1.Keys)
      {
        ElementId elementId2 = dictionary1[key];
        double materialVolume = element2.GetMaterialVolume(elementId2);
        num1 += materialVolume;
        if (dictionary2.ContainsKey(elementId2))
          dictionary2[elementId2] += materialVolume;
        else
          dictionary2.Add(elementId2, materialVolume);
      }
    }
    else
      dictionary1 = new Dictionary<PrecastMaterialType, ElementId>();
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    invalidWeightInputs = new List<Element>();
    weightParametesDoNotExist = new List<Element>();
    foreach (DatagridItemEntry userDefinedMapping in userDefinedMappings)
    {
      Parameter parameter1 = TitleblockPopCore.workingSheetView.LookupParameter(userDefinedMapping.mappingToParam);
      if (parameter1 != null)
      {
        string lengthValue;
        string widthValue;
        string depthValue;
        TitleblockPopCore.GetLengthAndWidth(doc, element1, out lengthValue, out widthValue, out depthValue);
        if (userDefinedMapping.mappingToParam.Equals("TKT_LENGTH") && userDefinedMapping.mappingFromParam.Equals("DIM_LENGTH"))
          parameter1.Set(lengthValue);
        else if (userDefinedMapping.mappingToParam.Equals("TKT_WIDTH"))
          parameter1.Set(widthValue);
        else if (userDefinedMapping.mappingToParam.Equals("TKT_DEPTH"))
          parameter1.Set(depthValue);
        else if (userDefinedMapping.mappingToParam.Equals("TKT_ARCH_VOL_1"))
        {
          if (dictionary1.ContainsKey(PrecastMaterialType.Arch1) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch1]))
            parameter1.Set(dictionary2[dictionary1[PrecastMaterialType.Arch1]]);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_ARCH_VOL_2"))
        {
          if (dictionary1.ContainsKey(PrecastMaterialType.Arch2) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch2]))
            parameter1.Set(dictionary2[dictionary1[PrecastMaterialType.Arch2]]);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_ARCH_VOL_3"))
        {
          if (dictionary1.ContainsKey(PrecastMaterialType.Arch3) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch3]))
            parameter1.Set(dictionary2[dictionary1[PrecastMaterialType.Arch3]]);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_ARCH_VOL_4"))
        {
          if (dictionary1.ContainsKey(PrecastMaterialType.Arch4) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch4]))
            parameter1.Set(dictionary2[dictionary1[PrecastMaterialType.Arch4]]);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_ARCH_VOL_TOT"))
        {
          double num2 = 0.0;
          foreach (PrecastMaterialType key in new List<PrecastMaterialType>()
          {
            PrecastMaterialType.Arch1,
            PrecastMaterialType.Arch2,
            PrecastMaterialType.Arch3,
            PrecastMaterialType.Arch4
          })
          {
            if (dictionary1.ContainsKey(key) && dictionary2.ContainsKey(dictionary1[key]))
              num2 += dictionary2[dictionary1[key]];
          }
          parameter1.Set(num2);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_STRUCT_CUYDS"))
        {
          if (dictionary1.ContainsKey(PrecastMaterialType.Structural) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Structural]))
            parameter1.Set(dictionary2[dictionary1[PrecastMaterialType.Structural]]);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_TOTAL_REQUIRED"))
        {
          string str = totalRequired.ToString();
          parameter1.Set(str);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_CUYDS"))
        {
          double num3 = UnitUtils.ConvertFromInternalUnits(num1, doc.GetUnits().GetFormatOptions(SpecTypeId.Volume).GetUnitTypeId());
          parameter1.Set(num3.ToString("F"));
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_WT"))
        {
          Element superComponent = (element1 as FamilyInstance).SuperComponent;
          Parameter parameter2 = superComponent != null ? Parameters.LookupParameter(superComponent, "UNIT_WEIGHT") : Parameters.LookupParameter(element1, "UNIT_WEIGHT");
          bool flag = parameter2 != null;
          Parameter parameter3 = parameter2 ?? (superComponent != null ? Parameters.LookupParameter(superComponent, "WEIGHT_PER_UNIT") : Parameters.LookupParameter(element1, "WEIGHT_PER_UNIT"));
          if (parameter3 != null)
          {
            double num4 = parameter3.AsDouble();
            if (flag)
              num4 = UnitUtils.ConvertFromInternalUnits(num4, UnitTypeId.PoundsForcePerCubicFoot);
            if (num4 <= 0.0 && !invalidWeightInputs.Contains(element1))
              invalidWeightInputs.Add(element1);
            double internalUnits = UnitUtils.ConvertToInternalUnits(num4 * num1, UnitTypeId.PoundsForce);
            parameter1.Set(internalUnits);
          }
          else if (!weightParametesDoNotExist.Contains(element1))
            weightParametesDoNotExist.Add(element1);
        }
        else if (userDefinedMapping.mappingToParam.Equals("TKT_WEIGHT"))
        {
          Element superComponent = (element1 as FamilyInstance).SuperComponent;
          Parameter parameter4 = superComponent != null ? Parameters.LookupParameter(superComponent, "UNIT_WEIGHT") : Parameters.LookupParameter(element1, "UNIT_WEIGHT");
          bool flag = parameter4 != null;
          Parameter parameter5 = parameter4 ?? (superComponent != null ? Parameters.LookupParameter(superComponent, "WEIGHT_PER_UNIT") : Parameters.LookupParameter(element1, "WEIGHT_PER_UNIT"));
          if (parameter5 != null)
          {
            double num5 = parameter5.AsDouble();
            if (flag)
              num5 = UnitUtils.ConvertFromInternalUnits(num5, UnitTypeId.PoundsForcePerCubicFoot);
            if (num5 <= 0.0 && !invalidWeightInputs.Contains(element1))
              invalidWeightInputs.Add(element1);
            double a = UnitUtils.ConvertFromInternalUnits(UnitUtils.ConvertToInternalUnits(num5 * num1, UnitTypeId.PoundsForce), doc.GetUnits().GetFormatOptions(SpecTypeId.Weight).GetUnitTypeId());
            parameter1.Set(string.Format("{0:#,###0}", (object) Math.Round(a)));
          }
          else if (!weightParametesDoNotExist.Contains(element1))
            weightParametesDoNotExist.Add(element1);
        }
        else
        {
          Parameter parameter6 = Parameters.LookupParameter((Element) element2, userDefinedMapping.mappingFromParam);
          if (parameter6 == null)
          {
            Element superComponent = (element1 as FamilyInstance).SuperComponent;
            if (superComponent != null)
              parameter6 = Parameters.LookupParameter(superComponent, userDefinedMapping.mappingFromParam);
            if (parameter6 == null)
              parameter6 = Parameters.LookupParameter(element1, userDefinedMapping.mappingFromParam);
          }
          if (parameter6 != null)
          {
            if (parameter6.StorageType == parameter1.StorageType)
            {
              if (parameter1.StorageType == StorageType.String)
              {
                if (userDefinedMapping.mappingFromParam.ToUpper().Contains("DATE"))
                {
                  string str = parameter6.AsString();
                  if (str != null)
                  {
                    if (str.Length > 10)
                      str = str.Substring(0, 10);
                    parameter1.Set(str);
                  }
                  else
                    parameter1.Set("");
                }
                else if (parameter6.AsString() != null)
                  parameter1.Set(parameter6.AsString());
                else
                  parameter1.Set("");
              }
              else if (parameter1.StorageType == StorageType.Double)
                parameter1.Set(parameter6.AsDouble());
              else if (parameter1.StorageType == StorageType.Integer)
                parameter1.Set(parameter6.AsInteger());
            }
            else if (parameter1.StorageType == StorageType.String)
            {
              if (parameter6.StorageType == StorageType.Double)
                parameter1.Set(parameter6.AsDouble().ToString());
              else if (parameter6.StorageType == StorageType.Integer)
                parameter1.Set(parameter6.AsInteger().ToString());
            }
            else if (parameter1.StorageType == StorageType.Double)
            {
              if (parameter6.StorageType == StorageType.Integer)
                parameter1.Set((double) parameter6.AsInteger());
              else
                stringList2.Add(userDefinedMapping.mappingToParam);
            }
            else if (parameter1.StorageType == StorageType.Integer)
              stringList2.Add(userDefinedMapping.mappingToParam);
          }
        }
      }
      else
        stringList1.Add(userDefinedMapping.mappingToParam);
    }
    if (bShowErrors && stringList1.Count > 0)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in stringList1)
        stringBuilder.Append($" {str},");
      string messageBoxText = $"{(stringList1.Count > 1 ? "The following parameters are " : "The following parameter is ")}missing, please run the Project Shared Parameters tool and try again.{Environment.NewLine}{stringBuilder.ToString().Substring(0, stringBuilder.Length - 1)}.{Environment.NewLine}{Environment.NewLine}Do you want to continue?";
      string str1 = "Warning";
      MessageBoxButton messageBoxButton = MessageBoxButton.OKCancel;
      string caption = str1;
      int button = (int) messageBoxButton;
      if (MessageBox.Show(messageBoxText, caption, (MessageBoxButton) button) != MessageBoxResult.OK)
        return false;
    }
    if (bShowErrors && stringList2.Count > 0)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in stringList2)
        stringBuilder.Append($" {str},");
      string messageBoxText = $"{(stringList1.Count > 1 ? "The types of following parameters do not " : "The type of following parameter does not ")}match the{(stringList1.Count > 1 ? " types of parameters " : " type of parameter ")}that you are trying to map to. Please check your Title Block Populator Settings and try again.{Environment.NewLine}{Environment.NewLine}{stringBuilder.ToString().Substring(0, stringBuilder.Length - 1)}.{Environment.NewLine}{Environment.NewLine}Do you want to continue?";
      string str2 = "Warning";
      MessageBoxButton messageBoxButton = MessageBoxButton.OKCancel;
      string caption = str2;
      int button = (int) messageBoxButton;
      if (MessageBox.Show(messageBoxText, caption, (MessageBoxButton) button) != MessageBoxResult.OK)
        return false;
    }
    return true;
  }

  private static bool SetTitleBlockParameters(Document doc, int totalRequired, bool bShowErrors = true)
  {
    Element element1 = (Element) null;
    AssemblyInstance element2 = doc.GetElement(TitleblockPopCore.workingSheetView.AssociatedAssemblyInstanceId) as AssemblyInstance;
    ElementId elementId1 = new ElementId(BuiltInCategory.OST_StructuralFraming);
    foreach (ElementId memberId in (IEnumerable<ElementId>) element2.GetMemberIds())
    {
      Element element3 = doc.GetElement(memberId);
      if (element3.Category.Id.Equals((object) elementId1))
      {
        element1 = element3;
        break;
      }
    }
    List<string> stringList = new List<string>();
    string lengthValue;
    string widthValue;
    string depthValue;
    TitleblockPopCore.GetLengthAndWidth(doc, element1, out lengthValue, out widthValue, out depthValue);
    Parameter parameter1 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_LENGTH");
    if (parameter1 != null)
      parameter1.Set(lengthValue);
    else
      stringList.Add("TKT_LENGTH");
    Parameter parameter2 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_WIDTH");
    if (parameter2 != null)
      parameter2.Set(widthValue);
    else
      stringList.Add("TKT_WIDTH");
    Parameter parameter3 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_DEPTH");
    if (parameter3 != null)
      parameter3.Set(depthValue);
    else
      stringList.Add("TKT_DEPTH");
    Parameter parameter4 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_DIM_DIAGONAL");
    if (parameter4 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter5 = superComponent != null ? superComponent.LookupParameter("DIM_DIAGONAL") : element1.LookupParameter("DIM_DIAGONAL");
      if (parameter5 != null)
      {
        double num = parameter5.AsDouble();
        parameter4.Set(num.ToString());
      }
    }
    else
      stringList.Add("TKT_DIM_DIAGONAL");
    Parameter parameter6 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_SQFT");
    if (parameter6 != null)
      parameter6.Set(Parameters.GetParameterAsString(element1, "DIM_SQFT"));
    else
      stringList.Add("TKT_SQFT");
    Parameter parameter7 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_DESIGN_NUMBER");
    if (parameter7 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter8 = superComponent != null ? superComponent.LookupParameter("DESIGN_NUMBER") : element1.LookupParameter("DESIGN_NUMBER");
      if (parameter8 != null)
      {
        string str = parameter8.AsString();
        parameter7.Set(str);
      }
    }
    else
      stringList.Add("TKT_DESIGN_NUMBER");
    Parameter parameter9 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_PROD_CODE");
    if (parameter9 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter10 = superComponent != null ? superComponent.LookupParameter("PRODUCT_CODE") : element1.LookupParameter("PRODUCT_CODE");
      if (parameter10 != null)
      {
        string str = parameter10.AsString();
        parameter9.Set(str);
      }
    }
    else
      stringList.Add("TKT_PROD_CODE");
    Parameter parameter11 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_EREC_SEQ");
    if (parameter11 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter12 = superComponent != null ? superComponent.LookupParameter("ERECTION_SEQUENCE") : element1.LookupParameter("ERECTION_SEQUENCE");
      if (parameter12 != null)
      {
        string str = parameter12.AsString();
        parameter11.Set(str);
      }
    }
    else
      stringList.Add("TKT_EREC_SEQ");
    Parameter parameter13 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_STRUCT_MIX");
    if (parameter13 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter14 = superComponent != null ? superComponent.LookupParameter("STRUCT_MIX_NUM") : element1.LookupParameter("STRUCT_MIX_NUM");
      if (parameter14 != null)
      {
        string str = parameter14.AsString();
        parameter13.Set(str);
      }
    }
    else
      stringList.Add("TKT_STRUCT_MIX");
    Parameter parameter15 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_MIX_1");
    if (parameter15 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter16 = superComponent != null ? superComponent.LookupParameter("ARCH_MIX_NUM_1") : element1.LookupParameter("ARCH_MIX_NUM_1");
      if (parameter16 != null)
      {
        string str = parameter16.AsString();
        parameter15.Set(str);
      }
    }
    else
      stringList.Add("TKT_ARCH_MIX_1");
    Parameter parameter17 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_MIX_2");
    if (parameter17 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter18 = superComponent != null ? superComponent.LookupParameter("ARCH_MIX_NUM_2") : element1.LookupParameter("ARCH_MIX_NUM_2");
      if (parameter18 != null)
      {
        string str = parameter18.AsString();
        parameter17.Set(str);
      }
    }
    else
      stringList.Add("TKT_ARCH_MIX_2");
    Parameter parameter19 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_MIX_3");
    if (parameter19 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter20 = superComponent != null ? superComponent.LookupParameter("ARCH_MIX_NUM_3") : element1.LookupParameter("ARCH_MIX_NUM_3");
      if (parameter20 != null)
      {
        string str = parameter20.AsString();
        parameter19.Set(str);
      }
    }
    else
      stringList.Add("TKT_ARCH_MIX_3");
    Parameter parameter21 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_MIX_4");
    if (parameter21 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter22 = superComponent != null ? superComponent.LookupParameter("ARCH_MIX_NUM_4") : element1.LookupParameter("ARCH_MIX_NUM_4");
      if (parameter22 != null)
      {
        string str = parameter22.AsString();
        parameter21.Set(str);
      }
    }
    else
      stringList.Add("TKT_ARCH_MIX_4");
    Parameter parameter23 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_TOP_FINISH");
    if (parameter23 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter24 = superComponent != null ? superComponent.LookupParameter("TOP_FINISH") : element1.LookupParameter("TOP_FINISH");
      if (parameter24 != null)
      {
        string str = parameter24.AsString();
        parameter23.Set(str);
      }
    }
    else
      stringList.Add("TKT_TOP_FINISH");
    Parameter parameter25 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_RELEASE_STRENGTH");
    if (parameter25 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter26 = superComponent != null ? superComponent.LookupParameter("RELEASE_STRENGTH") : element1.LookupParameter("RELEASE_STRENGTH");
      if (parameter26 != null)
      {
        double num = parameter26.AsDouble();
        parameter25.Set(num.ToString());
      }
    }
    else
      stringList.Add("TKT_RELEASE_STRENGTH");
    Parameter parameter27 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_FINAL_STRENGTH");
    if (parameter27 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter28 = superComponent != null ? superComponent.LookupParameter("FINAL_STRENGTH") : element1.LookupParameter("FINAL_STRENGTH");
      if (parameter28 != null)
      {
        double num = parameter28.AsDouble();
        parameter27.Set(num.ToString());
      }
    }
    else
      stringList.Add("TKT_FINAL_STRENGTH");
    Parameter parameter29 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_SF_1");
    Parameter parameter30 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_SF_2");
    Parameter parameter31 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_SF_3");
    Parameter parameter32 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_SF_4");
    if (parameter29 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter33 = superComponent != null ? superComponent.LookupParameter("ARCH_SF_1") : element1.LookupParameter("ARCH_SF_1");
      if (parameter33 != null)
      {
        double num = parameter33.AsDouble();
        parameter29.Set(num);
      }
    }
    else
      stringList.Add("TKT_ARCH_SF_1");
    if (parameter30 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter34 = superComponent != null ? superComponent.LookupParameter("ARCH_SF_2") : element1.LookupParameter("ARCH_SF_2");
      if (parameter34 != null)
      {
        double num = parameter34.AsDouble();
        parameter30.Set(num);
      }
    }
    else
      stringList.Add("TKT_ARCH_SF_2");
    if (parameter31 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter35 = superComponent != null ? superComponent.LookupParameter("ARCH_SF_3") : element1.LookupParameter("ARCH_SF_3");
      if (parameter35 != null)
      {
        double num = parameter35.AsDouble();
        parameter31.Set(num);
      }
    }
    else
      stringList.Add("TKT_ARCH_SF_3");
    if (parameter32 != null)
    {
      Element superComponent = (element1 as FamilyInstance).SuperComponent;
      Parameter parameter36 = superComponent != null ? superComponent.LookupParameter("ARCH_SF_4") : element1.LookupParameter("ARCH_SF_4");
      if (parameter36 != null)
      {
        double num = parameter36.AsDouble();
        parameter32.Set(num);
      }
    }
    else
      stringList.Add("TKT_ARCH_SF_4");
    Dictionary<PrecastMaterialType, ElementId> dictionary1 = Materials.GetPrecastMaterialIds(doc);
    Dictionary<ElementId, double> dictionary2 = new Dictionary<ElementId, double>();
    double num1 = 0.0;
    if (dictionary1 != null)
    {
      foreach (PrecastMaterialType key in dictionary1.Keys)
      {
        ElementId elementId2 = dictionary1[key];
        double materialVolume = element2.GetMaterialVolume(elementId2);
        num1 += materialVolume;
        if (dictionary2.ContainsKey(elementId2))
          dictionary2[elementId2] += materialVolume;
        else
          dictionary2.Add(elementId2, materialVolume);
      }
    }
    else
      dictionary1 = new Dictionary<PrecastMaterialType, ElementId>();
    Parameter parameter37 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_VOL_1");
    Parameter parameter38 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_VOL_2");
    Parameter parameter39 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_VOL_3");
    Parameter parameter40 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_VOL_4");
    Parameter parameter41 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_STRUCT_CUYDS");
    Parameter parameter42 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_ARCH_VOL_TOT");
    if (parameter37 != null && dictionary1.ContainsKey(PrecastMaterialType.Arch1) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch1]))
      parameter37.Set(dictionary2[dictionary1[PrecastMaterialType.Arch1]]);
    else if (parameter37 == null)
      stringList.Add("TKT_ARCH_VOL_1");
    if (parameter38 != null && dictionary1.ContainsKey(PrecastMaterialType.Arch2) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch2]))
      parameter38.Set(dictionary2[dictionary1[PrecastMaterialType.Arch2]]);
    else if (parameter38 == null)
      stringList.Add("TKT_ARCH_VOL_2");
    if (parameter39 != null && dictionary1.ContainsKey(PrecastMaterialType.Arch3) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch3]))
      parameter39.Set(dictionary2[dictionary1[PrecastMaterialType.Arch3]]);
    else if (parameter39 == null)
      stringList.Add("TKT_ARCH_VOL_3");
    if (parameter40 != null && dictionary1.ContainsKey(PrecastMaterialType.Arch4) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Arch4]))
      parameter40.Set(dictionary2[dictionary1[PrecastMaterialType.Arch4]]);
    else if (parameter40 == null)
      stringList.Add("TKT_ARCH_VOL_4");
    if (parameter41 != null && dictionary1.ContainsKey(PrecastMaterialType.Structural) && dictionary2.ContainsKey(dictionary1[PrecastMaterialType.Structural]))
      parameter41.Set(dictionary2[dictionary1[PrecastMaterialType.Structural]]);
    else if (parameter41 == null)
      stringList.Add("TKT_STRUCT_CUYDS");
    if (parameter42 != null)
    {
      double num2 = 0.0;
      foreach (PrecastMaterialType key in new List<PrecastMaterialType>()
      {
        PrecastMaterialType.Arch1,
        PrecastMaterialType.Arch2,
        PrecastMaterialType.Arch3,
        PrecastMaterialType.Arch4
      })
      {
        if (dictionary1.ContainsKey(key) && dictionary2.ContainsKey(dictionary1[key]))
          num2 += dictionary2[dictionary1[key]];
      }
      parameter42.Set(num2);
    }
    else if (parameter42 == null)
      stringList.Add("TKT_ARCH_VOL_TOT");
    string str1 = totalRequired.ToString();
    Parameter parameter43 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_TOTAL_REQUIRED");
    if (parameter43 != null)
      parameter43.Set(str1);
    else
      stringList.Add("TKT_TOTAL_REQUIRED");
    double num3 = UnitUtils.ConvertFromInternalUnits(num1, doc.GetUnits().GetFormatOptions(SpecTypeId.Volume).GetUnitTypeId());
    Element superComponent1 = (element1 as FamilyInstance).SuperComponent;
    bool flag = element1.LookupParameter("UNIT_WEIGHT") != null;
    Parameter parameter44 = flag ? (superComponent1 != null ? superComponent1.LookupParameter("UNIT_WEIGHT") : element1.LookupParameter("UNIT_WEIGHT")) : (superComponent1 != null ? superComponent1.LookupParameter("WEIGHT_PER_UNIT") : element1.LookupParameter("WEIGHT_PER_UNIT"));
    if (parameter44 != null)
    {
      double num4 = parameter44.AsDouble();
      if (flag)
        num4 = UnitUtils.ConvertFromInternalUnits(num4, UnitTypeId.PoundsForcePerCubicFoot);
      double internalUnits = UnitUtils.ConvertToInternalUnits(num4 * num1, UnitTypeId.PoundsForce);
      double a = UnitUtils.ConvertFromInternalUnits(internalUnits, doc.GetUnits().GetFormatOptions(SpecTypeId.Weight).GetUnitTypeId());
      Parameter parameter45 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_WEIGHT");
      TitleblockPopCore.workingSheetView.LookupParameter("TKT_WT")?.Set(internalUnits);
      parameter45?.Set(string.Format("{0:#,###0}", (object) Math.Round(a)));
    }
    Parameter parameter46 = TitleblockPopCore.workingSheetView.LookupParameter("TKT_CUYDS");
    if (parameter46 != null)
      parameter46.Set(num3.ToString("F"));
    else
      stringList.Add("TKT_CUYDS");
    if (stringList.Count <= 0)
      return true;
    StringBuilder stringBuilder = new StringBuilder();
    foreach (string str2 in stringList)
      stringBuilder.Append($" {str2},");
    string messageBoxText = $"{(stringList.Count > 1 ? "The following parameters are " : "The following parameter is ")}missing, please run the Project Shared Parameters tool and try again.{Environment.NewLine}{stringBuilder.ToString().Substring(0, stringBuilder.Length - 1)}.{Environment.NewLine}{Environment.NewLine}Do you want to continue?";
    string caption = "Warning";
    MessageBoxButton button = MessageBoxButton.OKCancel;
    MessageBoxResult messageBoxResult = MessageBoxResult.OK;
    if (bShowErrors)
      messageBoxResult = MessageBox.Show(messageBoxText, caption, button);
    return messageBoxResult == MessageBoxResult.OK;
  }

  private static double ConvRvtToCY(double valueToConvert)
  {
    return UnitUtils.ConvertFromInternalUnits(valueToConvert, UnitTypeId.CubicYards);
  }

  public static void GetLengthAndWidth(
    Document doc,
    Element sfElement,
    out string lengthValue,
    out string widthValue,
    out string depthValue)
  {
    lengthValue = "";
    widthValue = "";
    depthValue = "";
    Element superComponent = (sfElement as FamilyInstance).SuperComponent;
    Element element = superComponent == null ? doc.GetElement(sfElement.GetTypeId()) : doc.GetElement(superComponent.GetTypeId());
    if (element == null)
      return;
    Parameter parameter1;
    if (element.LookupParameter("CONSTRUCTION_PRODUCT") != null)
    {
      parameter1 = element.LookupParameter("CONSTRUCTION_PRODUCT");
      parameter1.AsValueString();
    }
    else
    {
      parameter1 = sfElement.LookupParameter("CONSTRUCTION_PRODUCT");
      parameter1?.AsValueString();
    }
    if (element.LookupParameter("DIM_WIDTH") != null)
      widthValue = element.LookupParameter("DIM_WIDTH").AsValueString();
    else if (sfElement.LookupParameter("DIM_WIDTH") != null)
      widthValue = sfElement.LookupParameter("DIM_WIDTH").AsValueString();
    else if (element.LookupParameter("DIM_HEIGHT") != null)
      widthValue = element.LookupParameter("DIM_HEIGHT").AsValueString();
    else if (sfElement.LookupParameter("DIM_HEIGHT") != null)
      widthValue = sfElement.LookupParameter("DIM_HEIGHT").AsValueString();
    if (element.LookupParameter("DIM_DEPTH") != null)
      depthValue = element.LookupParameter("DIM_DEPTH").AsValueString();
    else if (sfElement.LookupParameter("DIM_DEPTH") != null)
      depthValue = sfElement.LookupParameter("DIM_DEPTH").AsValueString();
    else if (element.LookupParameter("DIM_THICKNESS") != null)
      depthValue = element.LookupParameter("DIM_THICKNESS").AsValueString();
    else if (sfElement.LookupParameter("DIM_THICKNESS") != null)
      depthValue = sfElement.LookupParameter("DIM_THICKNESS").AsValueString();
    if (element.LookupParameter("DIM_LENGTH") != null)
      lengthValue = element.LookupParameter("DIM_LENGTH").AsValueString();
    else if (sfElement.LookupParameter("DIM_LENGTH") != null)
      lengthValue = sfElement.LookupParameter("DIM_LENGTH").AsValueString();
    if (parameter1 == null || !parameter1.HasValue || !parameter1.AsString().Equals("FLAT SLAB"))
      return;
    Parameter parameter2 = (Parameter) null;
    Parameter parameter3 = (Parameter) null;
    if (element.LookupParameter("DIM_WIDTH") != null)
      parameter2 = element.LookupParameter("DIM_WIDTH");
    else if (sfElement.LookupParameter("DIM_WIDTH") != null)
      parameter2 = sfElement.LookupParameter("DIM_WIDTH");
    if (element.LookupParameter("DIM_LENGTH") != null)
      parameter3 = element.LookupParameter("DIM_LENGTH");
    else if (sfElement.LookupParameter("DIM_LENGTH") != null)
      parameter3 = sfElement.LookupParameter("DIM_LENGTH");
    if (parameter2 == null || parameter3 == null || parameter2.AsDouble() <= parameter3.AsDouble())
      return;
    string str = lengthValue;
    lengthValue = widthValue;
    widthValue = str;
  }

  private static string[] FindSeries(string[] resultsArray)
  {
    int num1 = 0;
    string str1 = "";
    bool flag1 = false;
    bool flag2 = false;
    StringBuilder stringBuilder = new StringBuilder();
    int[] numArray = new int[resultsArray.Length];
    for (int index = 0; index < resultsArray.Length; ++index)
    {
      int result;
      if (int.TryParse(resultsArray[index], out result))
        numArray[index] = result;
    }
    for (int index1 = 0; index1 < numArray.Length && !flag2; ++index1)
    {
      int num2 = numArray[index1];
      if (index1 + 1 == numArray.Length)
        flag2 = true;
      int num3 = num2;
      for (int index2 = index1; index2 < numArray.Length; ++index2)
      {
        if (index2 + 1 == numArray.Length)
          flag2 = true;
        int num4 = !flag2 ? numArray[index2 + 1] : numArray[index2];
        if (num4 - num2 == 1)
        {
          flag1 = true;
          num2 = num4;
          num1 = num4;
        }
        else
        {
          index1 = index2;
          break;
        }
      }
      string str2 = num3 >= 10 ? (num3 >= 100 ? num3.ToString() : "0" + num3.ToString()) : "00" + num3.ToString();
      if (num1 < 10)
        str1 = "00" + num1.ToString();
      string str3 = num1 >= 100 ? num1.ToString() : "0" + num1.ToString();
      if (flag1)
      {
        flag1 = false;
        stringBuilder.Append($"{str2}-{str3},");
      }
      else
        stringBuilder.Append(str2 + ",");
    }
    char[] separator = new char[1]{ ',' };
    return stringBuilder.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
  }
}
