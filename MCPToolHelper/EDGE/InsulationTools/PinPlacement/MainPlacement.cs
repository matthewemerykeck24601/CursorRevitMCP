// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.PinPlacement.MainPlacement
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.InsulationTools.DirectionalMarker;
using EDGE.InsulationTools.InsulationMarking;
using EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.InsulationTools.PinPlacement;

[Transaction(TransactionMode.Manual)]
public class MainPlacement : IExternalCommand
{
  private const double defaultValue = 0.5;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Autodesk.Revit.UI.Selection.Selection selection = application.ActiveUIDocument.Selection;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Automatic Pin Placement must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Automatic Pin Placement must be run in the project environment.  Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document, "Automatic Pin Placement"))
    {
      int num1 = (int) transactionGroup.Start();
      TaskDialog taskDialog = new TaskDialog("Automatic Pin Placement");
      taskDialog.Title = "Automatic Pin Placement";
      taskDialog.AllowCancellation = true;
      taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
      taskDialog.MainInstruction = "Automatic Pin Placement";
      taskDialog.MainContent = "Select the scope for automatic pin placement.";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Peform Automatic Pin Placement for the Whole Project.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Peform Automatic Pin Placement for the Active View.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Peform Automatic Pin Placement for selected insulations.");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      Result result = (Result) 0;
      List<Element> elementList1 = new List<Element>();
      ICollection<ElementId> elementIds = application.ActiveUIDocument.Selection.GetElementIds();
      List<Element> source1;
      if (taskDialogResult == 1001)
        source1 = PinPlacementChoice.WholeModel(document);
      else if (taskDialogResult == 1002)
        source1 = PinPlacementChoice.ActiveView(document, activeUiDocument);
      else if (taskDialogResult == 1003)
      {
        source1 = PinPlacementChoice.SelectedElements(document, elementIds, selection, false);
        if (source1 == null)
        {
          int num2 = (int) transactionGroup.RollBack();
          return (Result) 1;
        }
      }
      else
      {
        int num3 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      if (result == 1 || result == -1)
      {
        int num4 = (int) transactionGroup.RollBack();
        return result;
      }
      Dictionary<FamilySymbol, List<Element>> dictionary1 = new Dictionary<FamilySymbol, List<Element>>();
      List<string> values1 = new List<string>();
      List<string> values2 = new List<string>();
      List<string> source2 = new List<string>();
      List<string> source3 = new List<string>();
      if (source1.Count<Element>() == 0)
      {
        new TaskDialog("Automatic Pin Placement")
        {
          Title = "Automatic Pin Placement",
          MainContent = "There were no valid insulation elements in the selected scope. Valid insulation elements contain \"EDGE\" and \"INSULATION\" in their MANUFACTURE_COMPONENT."
        }.Show();
        int num5 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      if (document.IsWorkshared && !CheckElementsOwnership.CheckOwnership("Pin Placement", source1.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>(), document, activeUiDocument, out List<ElementId> _))
        return (Result) 1;
      bool flag1 = false;
      ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
      {
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_SpecialityEquipment
      });
      Dictionary<FamilySymbol, List<Element>> dictionary2 = new Dictionary<FamilySymbol, List<Element>>();
      List<ElementId> elementIdList = new List<ElementId>();
      List<Element> elementsToRemove = new List<Element>();
      foreach (Element elem in source1)
      {
        double num6 = 0.0;
        foreach (Solid solid in Solids.GetSolids(elem))
          num6 += solid.Volume;
        if (num6 <= 0.0)
          elementsToRemove.Add(elem);
      }
      if (elementsToRemove.Count > 0)
        source1.RemoveAll((Predicate<Element>) (e => elementsToRemove.Contains(e)));
      if (source1.Count == 0)
      {
        TaskDialog.Show("Automatic Pin Placement", "No pins could be placed for selected scope.");
        int num7 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      foreach (Element element in source1)
      {
        string str1 = $"{(element as FamilyInstance).Symbol.FamilyName} - {element.Id?.ToString()}";
        Parameter parameter1 = Parameters.LookupParameter(element, "HOST_GUID");
        if (parameter1 == null)
        {
          values1.Add(str1);
        }
        else
        {
          string str2 = parameter1.AsString();
          if (str2 != null)
          {
            if (document.GetElement(parameter1.AsString()) == null)
            {
              values1.Add(str1);
              continue;
            }
          }
          else if (string.IsNullOrEmpty(str2))
          {
            values1.Add(str1);
            continue;
          }
          ElementId id = (element as FamilyInstance).Symbol.Id;
          bool flag2 = false;
          Parameter parameter2 = Parameters.LookupParameter(element, "BOM_PRODUCT_HOST");
          if (parameter2 == null || !parameter2.HasValue || string.IsNullOrEmpty(parameter2.AsString()))
          {
            values2.Add(str1);
          }
          else
          {
            Parameter parameter3 = Parameters.LookupParameter(element, "DIM_LENGTH");
            if (parameter3 == null || !parameter3.Definition.GetDataType().Equals((object) SpecTypeId.Length))
            {
              string familyName = (element as FamilyInstance).Symbol.FamilyName;
              source2.Add(familyName);
            }
            else
            {
              Parameter parameter4 = Parameters.LookupParameter(element, "DIM_WIDTH");
              if (parameter4 == null || !parameter4.Definition.GetDataType().Equals((object) SpecTypeId.Length))
              {
                string familyName = (element as FamilyInstance).Symbol.FamilyName;
                source3.Add(familyName);
              }
              else
              {
                List<ElementId> list1 = InstanceVoidCutUtils.GetCuttingVoidInstances(element).ToList<ElementId>();
                if (list1.Count > 0)
                {
                  List<Element> list2 = new FilteredElementCollector(document, (ICollection<ElementId>) list1).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Parameters.GetParameterAsString((Element) (e as FamilyInstance).Symbol, "MANUFACTURE_COMPONENT").ToUpper().Contains("PIN"))).ToList<Element>();
                  if (list2.Count > 0)
                  {
                    flag1 = true;
                    bool flag3 = false;
                    elementIdList.AddRange((IEnumerable<ElementId>) list2.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>());
                    foreach (FamilySymbol key in dictionary2.Keys)
                    {
                      if (key.Id.Equals((object) id))
                      {
                        dictionary2[key].Add(element);
                        flag3 = true;
                      }
                    }
                    if (!flag3)
                      dictionary2.Add((element as FamilyInstance).Symbol, new List<Element>()
                      {
                        element
                      });
                  }
                }
                foreach (FamilySymbol key in dictionary1.Keys)
                {
                  if (key.Id.Equals((object) id))
                  {
                    dictionary1[key].Add(element);
                    flag2 = true;
                  }
                }
                if (!flag2)
                  dictionary1.Add((element as FamilyInstance).Symbol, new List<Element>()
                  {
                    element
                  });
              }
            }
          }
        }
      }
      Dictionary<FamilySymbol, List<Element>> eachFamilyAndBelongingElements = new Dictionary<FamilySymbol, List<Element>>();
      if (flag1)
      {
        string str = "";
        foreach (FamilySymbol key in dictionary2.Keys)
        {
          str = $"{str}{key.Name}\n";
          foreach (Element element in dictionary2[key])
            str = $"{str}   {element.Name} - {element.Id?.ToString()}\n";
          str += "\n";
        }
        if (new TaskDialog("Pin Placement")
        {
          MainContent = "The following pieces of insulation contain pins already placed in them. Would you like to delete these pins and proceed with Automatic Pin Placement?",
          ExpandedContent = string.Join("\n", str),
          CommonButtons = ((TaskDialogCommonButtons) 6)
        }.Show() == 6)
        {
          using (Transaction transaction = new Transaction(document, "Delete Previously Placed Pins"))
          {
            int num8 = (int) transaction.Start();
            document.Delete((ICollection<ElementId>) elementIdList);
            int num9 = (int) transaction.Commit();
          }
          eachFamilyAndBelongingElements = dictionary1;
        }
        else
        {
          foreach (FamilySymbol key1 in dictionary1.Keys)
          {
            bool flag4 = false;
            FamilySymbol key2 = (FamilySymbol) null;
            foreach (FamilySymbol key3 in dictionary2.Keys)
            {
              if (key3.Name.Equals(key1.Name))
              {
                flag4 = true;
                key2 = key3;
                break;
              }
            }
            if (flag4)
            {
              List<Element> elementList2 = new List<Element>();
              List<Element> elementList3 = dictionary2[key2];
              foreach (Element element1 in dictionary1[key1])
              {
                bool flag5 = false;
                foreach (Element element2 in elementList3)
                {
                  if (element2.Id.IntegerValue.Equals(element1.Id.IntegerValue))
                  {
                    flag5 = true;
                    break;
                  }
                }
                if (!flag5)
                  elementList2.Add(element1);
              }
              if (elementList2.Count > 0)
                eachFamilyAndBelongingElements.Add(key1, elementList2);
            }
            else
              break;
          }
        }
      }
      else
        eachFamilyAndBelongingElements = dictionary1;
      bool flag6 = false;
      bool dupeErr = false;
      if (eachFamilyAndBelongingElements.Keys.Count > 0)
        flag6 = MainPlacement.pinPlacementEDGE(eachFamilyAndBelongingElements, out dupeErr);
      if (!dupeErr)
      {
        if (values1.Count > 0)
          new TaskDialog("Pin Placement")
          {
            MainContent = "The following insulation pieces are not hosted to a wall panel and therefore cannot be processed. Please run the BOM Product Hosting tool and try again.",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) values1)
          }.Show();
        if (values2.Count > 0)
          new TaskDialog("Pin Placement")
          {
            MainContent = "The BOM_PRODUCT_HOST parameter on the following pieces of insulation either does not exist or does not have a valid value; therefore, the insulation could not be processed. It is recommended to run the BOM Product Hosting tool on the model before running Pin Placement.",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) values2)
          }.Show();
        if (source2.Count > 0)
        {
          List<string> list = source2.Distinct<string>().ToList<string>();
          new TaskDialog("Pin Placement")
          {
            MainContent = "The DIM_LENGTH parameter on the following pieces of insulation either does not exist or is not set up as a Length type parameter.",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) list)
          }.Show();
        }
        if (source3.Count > 0)
        {
          List<string> list = source3.Distinct<string>().ToList<string>();
          new TaskDialog("Pin Placement")
          {
            MainContent = "The DIM_WIDTH parameter on the following pieces of insulation either does not exist or is not set up as a Length type parameter.",
            ExpandedContent = string.Join("\n", (IEnumerable<string>) list)
          }.Show();
        }
      }
      if (!flag6)
      {
        int num10 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      int num11 = (int) transactionGroup.Commit();
      return result;
    }
  }

  public static bool pinPlacementEDGE(
    Dictionary<FamilySymbol, List<Element>> eachFamilyAndBelongingElements,
    out bool dupeErr)
  {
    dupeErr = false;
    double num1 = 1.0 / 3.0;
    double num2 = 1.0 / 3.0;
    double num3 = 4.0 / 3.0;
    double num4 = 4.0 / 3.0;
    double num5 = 1.0 / 3.0;
    double num6 = 1.0 / 3.0;
    Document document = eachFamilyAndBelongingElements.First<KeyValuePair<FamilySymbol, List<Element>>>().Key.Document;
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> elementList1 = new List<Element>();
    List<Element> list = new FilteredElementCollector(document).OfClass(typeof (FamilySymbol)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("PIN"))).ToList<Element>();
    Dictionary<FamilySymbol, List<Element>> dictionary1 = new Dictionary<FamilySymbol, List<Element>>();
    Dictionary<ElementId, bool> dictionary2 = new Dictionary<ElementId, bool>();
    Dictionary<FamilySymbol, List<Element>> dictionary3 = new Dictionary<FamilySymbol, List<Element>>();
    Dictionary<FamilySymbol, Element> dictionary4 = new Dictionary<FamilySymbol, Element>();
    Dictionary<FamilySymbol, bool> dictionary5 = new Dictionary<FamilySymbol, bool>();
    List<string> stringList = new List<string>();
    foreach (FamilySymbol key in eachFamilyAndBelongingElements.Keys)
    {
      List<Element> source = new List<Element>();
      string str1 = "";
      string str2 = "";
      Parameter parameter1 = Parameters.LookupParameter((Element) key, "PIN_FAMILY");
      if (parameter1 != null)
        str1 = parameter1.AsString().Trim();
      Parameter parameter2 = Parameters.LookupParameter((Element) key, "PIN_TYPE");
      if (parameter2 != null)
        str2 = parameter2.AsString().Trim();
      bool flag1 = false;
      if (!string.IsNullOrEmpty(str1))
      {
        if (string.IsNullOrEmpty(str2))
        {
          flag1 = true;
        }
        else
        {
          bool flag2 = false;
          foreach (Element element in list)
          {
            if ((element as FamilySymbol).FamilyName.Equals(str1) && (element as FamilySymbol).Name.Equals(str2))
            {
              flag2 = true;
              dictionary4.Add(key, element);
              break;
            }
          }
          if (!flag2)
            flag1 = true;
        }
        if (flag1)
        {
          foreach (Element element in list)
          {
            if ((element as FamilySymbol).FamilyName.Equals(str1))
              source.Add(element);
          }
        }
        else
          source = list;
      }
      else
        source = list;
      if (source.Count<Element>() == 0)
      {
        string str3 = $"Insulation Type: {key.FamilyName} : {key.Name}\n";
        string str4 = (!string.IsNullOrEmpty(str2) ? $"Pin Type: {str1} : {str2}" : "Pin Family: " + str1) + "\n";
        stringList.Add(str3 + str4);
      }
      else
      {
        if (dictionary3.ContainsKey(key))
          dictionary3[key] = source;
        else
          dictionary3.Add(key, source);
        dictionary5.Add(key, flag1);
      }
    }
    if (stringList.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("Automatic Pin Placement");
      taskDialog.Title = "Automatic Pin Placement";
      taskDialog.MainContent = "One or more specified pin types were either not loaded into the current project or no instances have been placed. Please ensure the specified pin families/types are loaded in and active in the current project in order to run the Automatic Pin Placement tool. Expand for details";
      foreach (string str in stringList)
        taskDialog.ExpandedContent += str;
      taskDialog.Show();
      return false;
    }
    foreach (FamilySymbol key in eachFamilyAndBelongingElements.Keys)
    {
      string str5 = "";
      IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
      bool OnlyTypesShown = dictionary5[key];
      Element element1 = dictionary4.ContainsKey(key) ? dictionary4[key] : (Element) null;
      List<Element> values = dictionary3.ContainsKey(key) ? dictionary3[key] : new List<Element>();
      if (element1 == null)
      {
        values.Sort((Comparison<Element>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings($"{(p as FamilySymbol).FamilyName}:{p.Name}", $"{(q as FamilySymbol).FamilyName}:{q.Name}")));
        PinFamilySelection pinFamilySelection = new PinFamilySelection(key, values, mainWindowHandle, OnlyTypesShown);
        if (pinFamilySelection.pins.Items.Count == 1)
        {
          str5 = (pinFamilySelection.pins.Items[0] as ListViewItem).Content.ToString();
        }
        else
        {
          pinFamilySelection.ShowDialog();
          if (pinFamilySelection.pins.SelectedItem is ListViewItem)
            str5 = (pinFamilySelection.pins.SelectedItem as ListViewItem).Content.ToString();
        }
        if (string.IsNullOrEmpty(str5))
          return false;
        if (OnlyTypesShown)
        {
          foreach (Element element2 in values)
          {
            if ((element2 as FamilySymbol).Name.Equals(str5))
            {
              element1 = element2;
              break;
            }
          }
        }
        else
        {
          foreach (Element element3 in values)
          {
            string[] source = str5.Split(':');
            string str6 = ((IEnumerable<string>) source).ElementAt<string>(1);
            string str7 = ((IEnumerable<string>) source).ElementAt<string>(0);
            if ((element3 as FamilySymbol).FamilyName.Equals(str7) && (element3 as FamilySymbol).Name.Equals(str6))
            {
              element1 = element3;
              break;
            }
          }
        }
      }
      Parameter parameter3 = Parameters.LookupParameter(element1, "MANUFACTURE_COMPONENT");
      if (parameter3 != null && parameter3.AsString().Contains("COMPOSITE"))
      {
        bool flag = false;
        App.ifCompositePinsExist = true;
        foreach (Element elem in eachFamilyAndBelongingElements[key])
        {
          Parameter parameter4 = Parameters.LookupParameter(elem, "BOM_PRODUCT_HOST");
          if (parameter4 == null || !parameter4.HasValue || string.IsNullOrEmpty(parameter4.AsString()))
            flag = true;
        }
        if (!flag)
        {
          TaskDialog taskDialog = new TaskDialog("Pin Placement");
          string friendlyFamilyTypeName1 = element1.GetFriendlyFamilyTypeName();
          string friendlyFamilyTypeName2 = key.GetFriendlyFamilyTypeName();
          taskDialog.MainInstruction = $"Please pick the orientation you would like to place the {friendlyFamilyTypeName1} in the {friendlyFamilyTypeName2} insulation.";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Horizontal Orientation");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Vertical Orientation");
          TaskDialogResult taskDialogResult = taskDialog.Show();
          if (taskDialogResult == 2)
            return false;
          foreach (Element element4 in eachFamilyAndBelongingElements[key])
            dictionary2.Add(element4.Id, taskDialogResult == 1002);
        }
        else
          continue;
      }
      if (dictionary1.ContainsKey(element1 as FamilySymbol))
        dictionary1[element1 as FamilySymbol].AddRange((IEnumerable<Element>) eachFamilyAndBelongingElements[key]);
      else
        dictionary1.Add(element1 as FamilySymbol, eachFamilyAndBelongingElements[key]);
    }
    using (Transaction transaction = new Transaction(document, "Pin Placement"))
    {
      new errorMessageInfo().ListOfElements = new Dictionary<string, List<Element>>();
      new errorMessageInfo().ListOfElements = new Dictionary<string, List<Element>>();
      new errorMessageInfo().ListOfElements = new Dictionary<string, List<Element>>();
      List<Element> elementList2 = new List<Element>();
      List<Element> elementList3 = new List<Element>();
      int num7 = 0;
      FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
      WarningSwallower preprocessor = new WarningSwallower();
      failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) preprocessor);
      transaction.SetFailureHandlingOptions(failureHandlingOptions);
      int num8 = (int) transaction.Start();
      List<Element> elementList4 = new List<Element>();
      foreach (FamilySymbol key in dictionary1.Keys)
      {
        Parameter parameter5 = Parameters.LookupParameter((Element) key, "MANUFACTURE_COMPONENT");
        bool composite = false;
        if (parameter5 != null && parameter5.HasValue && parameter5.AsString().ToUpper().Contains("COMPOSITE"))
          composite = true;
        foreach (Element element5 in dictionary1[key])
        {
          Transform transform1 = (element5 as FamilyInstance).GetTransform();
          bool flag3 = false;
          bool flag4 = false;
          Parameter parameter6 = Parameters.LookupParameter(element5, "HOST_GUID");
          string str = parameter6.AsString();
          Element element6 = (Element) null;
          if (str != null)
            element6 = document.GetElement(parameter6.AsString());
          Transform transform2 = (element6 as FamilyInstance).GetTransform();
          bool noReEntrant = DirectionalMarkerMain.checkWallandInsulationOrienatation(element5, element6);
          int num9 = InsulationMarkingMain.checkRotatedOrientation(element5, element6, noReEntrant) ? 1 : 0;
          FamilySymbol familySymbol = key;
          Parameter parameter7 = Parameters.LookupParameter(element5, "DIM_LENGTH");
          Parameter parameter8 = Parameters.LookupParameter(element5, "DIM_WIDTH");
          double num10;
          double num11;
          if (num9 != 0)
          {
            num10 = parameter7.AsDouble();
            num11 = parameter8.AsDouble();
          }
          else
          {
            num11 = parameter7.AsDouble();
            num10 = parameter8.AsDouble();
          }
          Parameter parameter9 = Parameters.LookupParameter(element5, "PIN_START_X");
          Parameter parameter10 = Parameters.LookupParameter(element5, "PIN_START_Y");
          if (parameter9 != null && parameter9.HasValue && parameter9.AsDouble() > 0.0)
            num1 = parameter9.AsDouble();
          if (parameter10 != null && parameter10.HasValue && parameter10.AsDouble() > 0.0)
            num2 = parameter10.AsDouble();
          Parameter parameter11 = Parameters.LookupParameter(element5, "PIN_END_BUFFER_X");
          Parameter parameter12 = Parameters.LookupParameter(element5, "PIN_END_BUFFER_Y");
          if (parameter11 != null && parameter11.HasValue && parameter11.AsDouble() > 0.0)
            num5 = parameter11.AsDouble();
          if (parameter12 != null && parameter12.HasValue && parameter12.AsDouble() > 0.0)
            num6 = parameter12.AsDouble();
          Parameter parameter13 = Parameters.LookupParameter(element5, "PIN_HORIZ_SPACING");
          Parameter parameter14 = Parameters.LookupParameter(element5, "PIN_VERT_SPACING");
          if (parameter13 != null && parameter13.HasValue && parameter13.AsDouble() > 0.0)
            num3 = parameter13.AsDouble();
          if (parameter14 != null && parameter14.HasValue && parameter14.AsDouble() > 0.0)
            num4 = parameter14.AsDouble();
          double num12 = num2;
          double num13 = num1;
          double num14 = num10 - num1 - num5;
          double num15 = num11 - num2 - num6;
          int int32_1 = Convert.ToInt32(Math.Floor(Math.Round(num14 / num3, 7)) + 1.0);
          int int32_2 = Convert.ToInt32(Math.Floor(Math.Round(num15 / num4, 7)) + 1.0);
          if (!noReEntrant)
          {
            Transform identity = Transform.Identity;
            identity.BasisX = transform2.BasisZ.CrossProduct(transform1.BasisZ);
            identity.BasisY = transform1.BasisZ;
            identity.BasisZ = transform2.BasisZ;
            if (!identity.IsConformal)
            {
              elementList4.Add(element5);
            }
            else
            {
              bool bSymbol;
              PlanarFace topFaceReturn;
              List<PlanarFace> upFaces;
              XYZ xyz1 = DirectionalMarkerMain.locationForOrienatation(element5, element6, document, identity, out bSymbol, out topFaceReturn, out upFaces);
              double x = xyz1.X + num13;
              double num16 = xyz1.Z + num12;
              double y = xyz1.Y;
              double z1 = num16;
              XYZ xyz2 = new XYZ(x, y, z1);
              bool flag5 = PlacementUtilities.IsMirrored(element5);
              if ((GeometryObject) topFaceReturn == (GeometryObject) null)
              {
                elementList3.Add(element5);
                flag4 = true;
              }
              if (flag4)
              {
                elementList4.Add(element5);
              }
              else
              {
                XYZ point = new XYZ(xyz2.X, xyz2.Y, xyz2.Z);
                ICollection<Element> source = (ICollection<Element>) new List<Element>();
                XYZ basisY = transform1.BasisY;
                XYZ referenceDirection = !composite ? identity.BasisZ : (dictionary2.ContainsKey(element5.Id) ? (!dictionary2[element5.Id] ? (!flag5 ? identity.BasisZ : -identity.BasisZ) : identity.BasisX) : identity.BasisZ);
                int num17 = 0;
                for (int index1 = 0; index1 < int32_1; ++index1)
                {
                  for (int index2 = 0; index2 < int32_2; ++index2)
                  {
                    familySymbol.Activate();
                    if (MainPlacement.processProjectionAndEdgesForRentrantCorner(upFaces, bSymbol, topFaceReturn, point, composite, familySymbol, transform2, transform1, identity, document))
                    {
                      XYZ location = identity.OfPoint(point);
                      FamilyInstance elem = document.Create.NewFamilyInstance(topFaceReturn.Reference, location, referenceDirection, familySymbol);
                      Parameters.LookupParameter((Element) elem, "Offset from Host").Set(0);
                      ++num7;
                      source.Add((Element) elem);
                    }
                    double z2 = point.Z + num4;
                    point = new XYZ(point.X, point.Y, z2);
                    ++num17;
                  }
                  XYZ xyz3 = new XYZ(xyz2.X, xyz2.Y, xyz2.Z);
                  point = new XYZ(xyz3.X + num3 * (double) (index1 + 1), xyz3.Y, xyz3.Z);
                }
                if (num17 == 0)
                  elementList4.Add(element5);
                document.Regenerate();
                MainPlacement.cutVoidsAndPins(source.ToList<Element>(), document, element5);
              }
            }
          }
          else
          {
            XYZ xyz4 = InsulationMarkingMain.LeftMostProjectedPoint(element6, element5, document);
            PlanarFace topFace;
            bool bSymbol;
            List<PlanarFace> allTopFaces;
            PlacementUtilities.GetAllTopFacesFromInsulation(document, element5, out topFace, out PlanarFace _, out bSymbol, out allTopFaces);
            if ((GeometryObject) topFace == (GeometryObject) null)
            {
              elementList3.Add(element5);
              flag4 = true;
            }
            if (!flag4)
            {
              XYZ xyz5;
              if (PlacementUtilities.IsMirrored(element6))
              {
                XYZ xyz6 = xyz4 + XYZ.BasisX * num13;
                flag3 = true;
                xyz5 = xyz6 + XYZ.BasisZ * num12;
              }
              else
                xyz5 = xyz4 - XYZ.BasisX * num13 + XYZ.BasisZ * num12;
              XYZ point = xyz5;
              ICollection<Element> source = (ICollection<Element>) new List<Element>();
              XYZ basisY = transform1.BasisY;
              XYZ vec = !composite ? XYZ.BasisZ : (dictionary2.ContainsKey(element5.Id) ? (!dictionary2[element5.Id] ? (!flag3 ? XYZ.BasisZ : -XYZ.BasisZ) : XYZ.BasisX) : XYZ.BasisZ);
              int num18 = 0;
              for (int index3 = 0; index3 < int32_1; ++index3)
              {
                for (int index4 = 0; index4 < int32_2; ++index4)
                {
                  familySymbol.Activate();
                  XYZ xyz7 = transform2.OfPoint(point);
                  if (MainPlacement.processProjectionAndEdges(allTopFaces, bSymbol, topFace, xyz7, composite, familySymbol, transform2, transform1, document))
                  {
                    XYZ referenceDirection = transform2.OfVector(vec);
                    FamilyInstance elem = document.Create.NewFamilyInstance(topFace.Reference, xyz7, referenceDirection, familySymbol);
                    Parameters.LookupParameter((Element) elem, "Offset from Host").Set(0);
                    ++num7;
                    source.Add((Element) elem);
                  }
                  point += XYZ.BasisZ * num4;
                  ++num18;
                }
                XYZ xyz8 = new XYZ(xyz5.X, xyz5.Y, xyz5.Z);
                point = !flag3 ? xyz8 - XYZ.BasisX * num3 * (double) (index3 + 1) : xyz8 + XYZ.BasisX * num3 * (double) (index3 + 1);
              }
              if (num18 == 0)
                elementList4.Add(element5);
              document.Regenerate();
              MainPlacement.cutVoidsAndPins(source.ToList<Element>(), document, element5);
            }
          }
        }
      }
      if (elementList3.Count > 0)
      {
        string str = "";
        foreach (Element element in elementList3)
        {
          FamilyInstance familyInstance = element as FamilyInstance;
          str = $"{str}\n{familyInstance.Symbol.FamilyName} : {element.Name} - {element.Id?.ToString()}";
        }
        new TaskDialog("Automatic Pin Placement")
        {
          Title = "Automatic Pin Placement",
          MainContent = "Please check the solids for family instances on the following insulation pieces and try again.",
          ExpandedContent = str
        }.Show();
      }
      int num19 = (int) transaction.Commit();
      if ((preprocessor.FailType & WarningSwallower.FailureType.DuplicateInstances) == WarningSwallower.FailureType.DuplicateInstances)
      {
        if (new TaskDialog("Pin Placement")
        {
          MainContent = "Pin placement would create duplicate pin instances in one or more locations in the model. Continue with pin placement?",
          CommonButtons = ((TaskDialogCommonButtons) 6)
        }.Show() != 6)
        {
          dupeErr = true;
          return false;
        }
      }
      if (elementList4.Count > 0)
      {
        TaskDialog taskDialog = new TaskDialog("Automatic Pin Placement");
        taskDialog.MainContent = "Pins could not be placed for the following insulation pieces.";
        string str = "Problem Elements:\n";
        foreach (Element element in elementList4)
        {
          if (element is FamilyInstance familyInstance)
            str = $"{str}{familyInstance.Symbol.FamilyName} : {familyInstance.Name} : {familyInstance.Id?.ToString()}\n";
        }
        taskDialog.ExpandedContent = str;
        taskDialog.Show();
      }
      else if (num7 > 0)
        new TaskDialog("Automatic Pin Placement")
        {
          MainContent = "All pins have been placed succesfully."
        }.Show();
      else
        new TaskDialog("Automatic Pin Placement")
        {
          MainContent = "No pins could be placed for selected scope."
        }.Show();
    }
    return true;
  }

  public static bool placingPin(
    Dictionary<FamilySymbol, List<Element>> eachFamilyAndBelongingElements)
  {
    double pinBufferMin = 1.0 / 3.0;
    double pinMaxOffset = 4.0 / 3.0;
    Document document = eachFamilyAndBelongingElements.First<KeyValuePair<FamilySymbol, List<Element>>>().Key.Document;
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> elementList1 = new List<Element>();
    List<Element> list = new FilteredElementCollector(document).OfClass(typeof (FamilySymbol)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("PIN"))).ToList<Element>();
    Dictionary<FamilySymbol, List<Element>> dictionary1 = new Dictionary<FamilySymbol, List<Element>>();
    Dictionary<ElementId, bool> dictionary2 = new Dictionary<ElementId, bool>();
    foreach (FamilySymbol key in eachFamilyAndBelongingElements.Keys)
    {
      List<Element> elementList2 = new List<Element>();
      Element element1 = (Element) null;
      string str1 = "";
      string str2 = "";
      Parameter parameter1 = Parameters.LookupParameter((Element) key, "PIN_FAMILY");
      if (parameter1 != null)
        str1 = parameter1.AsString().Trim();
      Parameter parameter2 = Parameters.LookupParameter((Element) key, "PIN_TYPE");
      if (parameter2 != null)
        str2 = parameter2.AsString().Trim();
      bool OnlyTypesShown = false;
      if (!string.IsNullOrEmpty(str1))
      {
        if (string.IsNullOrEmpty(str2))
        {
          OnlyTypesShown = true;
        }
        else
        {
          bool flag = false;
          foreach (Element element2 in list)
          {
            if ((element2 as FamilySymbol).FamilyName.Equals(str1) && (element2 as FamilySymbol).Name.Equals(str2))
            {
              flag = true;
              element1 = element2;
              break;
            }
          }
          if (!flag)
            OnlyTypesShown = true;
        }
        if (OnlyTypesShown)
        {
          foreach (Element element3 in list)
          {
            if ((element3 as FamilySymbol).FamilyName.Equals(str1))
              elementList2.Add(element3);
          }
        }
        else
          elementList2 = list;
      }
      else
        elementList2 = list;
      IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
      string str3 = "";
      if (elementList2.Count<Element>() == 0)
      {
        new TaskDialog("Automatic Pin Placement")
        {
          Title = "Automatic Pin Placement",
          MainContent = "The pin family is either not loaded into the current project or no instances have been placed. Please ensure the family is loaded in and active in the current project in order to run the Automatic Pin Placement tool."
        }.Show();
        return false;
      }
      if (element1 == null)
      {
        elementList2.Sort((Comparison<Element>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings($"{(p as FamilySymbol).FamilyName}:{p.Name}", $"{(q as FamilySymbol).FamilyName}:{q.Name}")));
        PinFamilySelection pinFamilySelection = new PinFamilySelection(key, elementList2, mainWindowHandle, OnlyTypesShown);
        if (pinFamilySelection.pins.Items.Count == 1)
        {
          str3 = (pinFamilySelection.pins.Items[0] as ListViewItem).Content.ToString();
        }
        else
        {
          pinFamilySelection.ShowDialog();
          if (pinFamilySelection.pins.SelectedItem is ListViewItem)
            str3 = (pinFamilySelection.pins.SelectedItem as ListViewItem).Content.ToString();
        }
        if (string.IsNullOrEmpty(str3))
          return false;
        if (OnlyTypesShown)
        {
          foreach (Element element4 in elementList2)
          {
            if ((element4 as FamilySymbol).Name.Equals(str3))
            {
              element1 = element4;
              break;
            }
          }
        }
        else
        {
          foreach (Element element5 in elementList2)
          {
            string[] source = str3.Split(':');
            string str4 = ((IEnumerable<string>) source).ElementAt<string>(1);
            string str5 = ((IEnumerable<string>) source).ElementAt<string>(0);
            if ((element5 as FamilySymbol).FamilyName.Equals(str5) && (element5 as FamilySymbol).Name.Equals(str4))
            {
              element1 = element5;
              break;
            }
          }
        }
      }
      Parameter parameter3 = Parameters.LookupParameter(element1, "MANUFACTURE_COMPONENT");
      if (parameter3 != null && parameter3.AsString().Contains("COMPOSITE"))
      {
        bool flag = false;
        App.ifCompositePinsExist = true;
        foreach (Element elem in eachFamilyAndBelongingElements[key])
        {
          Parameter parameter4 = Parameters.LookupParameter(elem, "BOM_PRODUCT_HOST");
          if (parameter4 == null || !parameter4.HasValue || string.IsNullOrEmpty(parameter4.AsString()))
            flag = true;
        }
        if (!flag)
        {
          TaskDialog taskDialog = new TaskDialog("Pin Placement");
          string friendlyFamilyTypeName1 = element1.GetFriendlyFamilyTypeName();
          string friendlyFamilyTypeName2 = key.GetFriendlyFamilyTypeName();
          taskDialog.MainInstruction = $"Please pick the orientation you would like to place the {friendlyFamilyTypeName1} in the {friendlyFamilyTypeName2} insulation.";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Horizontal Orientation");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Vertical Orientation");
          TaskDialogResult taskDialogResult = taskDialog.Show();
          if (taskDialogResult == 2)
            return false;
          foreach (Element element6 in eachFamilyAndBelongingElements[key])
            dictionary2.Add(element6.Id, taskDialogResult == 1002);
        }
        else
          continue;
      }
      if (dictionary1.ContainsKey(element1 as FamilySymbol))
        dictionary1[element1 as FamilySymbol].AddRange((IEnumerable<Element>) eachFamilyAndBelongingElements[key]);
      else
        dictionary1.Add(element1 as FamilySymbol, eachFamilyAndBelongingElements[key]);
    }
    using (Transaction transaction = new Transaction(document, "Pin Placement"))
    {
      new errorMessageInfo().ListOfElements = new Dictionary<string, List<Element>>();
      new errorMessageInfo().ListOfElements = new Dictionary<string, List<Element>>();
      new errorMessageInfo().ListOfElements = new Dictionary<string, List<Element>>();
      List<Element> elementList3 = new List<Element>();
      List<Element> elementList4 = new List<Element>();
      int num1 = 0;
      int num2 = (int) transaction.Start();
      foreach (FamilySymbol key in dictionary1.Keys)
      {
        Parameter parameter5 = Parameters.LookupParameter((Element) key, "MANUFACTURE_COMPONENT");
        bool composite = false;
        if (parameter5 != null && parameter5.HasValue && parameter5.AsString().ToUpper().Contains("COMPOSITE"))
          composite = true;
        foreach (Element element7 in dictionary1[key])
        {
          Transform transform1 = (element7 as FamilyInstance).GetTransform();
          bool flag1 = false;
          bool flag2 = false;
          Parameter parameter6 = Parameters.LookupParameter(element7, "HOST_GUID");
          string str = parameter6.AsString();
          Element element8 = (Element) null;
          if (str != null)
            element8 = document.GetElement(parameter6.AsString());
          Transform transform2 = (element8 as FamilyInstance).GetTransform();
          bool noReEntrant = DirectionalMarkerMain.checkWallandInsulationOrienatation(element7, element8);
          int num3 = InsulationMarkingMain.checkRotatedOrientation(element7, element8, noReEntrant) ? 1 : 0;
          FamilySymbol familySymbol = key;
          Parameter parameter7 = Parameters.LookupParameter(element7, "DIM_LENGTH");
          Parameter parameter8 = Parameters.LookupParameter(element7, "DIM_WIDTH");
          double totalDimension1;
          double totalDimension2;
          if (num3 != 0)
          {
            totalDimension1 = parameter7.AsDouble();
            totalDimension2 = parameter8.AsDouble();
          }
          else
          {
            totalDimension2 = parameter7.AsDouble();
            totalDimension1 = parameter8.AsDouble();
          }
          Parameter parameter9 = Parameters.LookupParameter(element7, "PIN_BUFFER_MIN");
          Parameter parameter10 = Parameters.LookupParameter(element7, "PIN_MAX_OFFSET");
          if (parameter9 != null && parameter9.HasValue && parameter9.AsDouble() > 0.0)
            pinBufferMin = parameter9.AsDouble();
          if (parameter10 != null && parameter10.HasValue && parameter10.AsDouble() > 0.0)
            pinMaxOffset = parameter10.AsDouble();
          int totalRows1;
          double edgeBuffer1;
          double pinMaxOffsetValue1;
          MainPlacement.AlgorithmForRowsAndOffset(totalDimension2, pinMaxOffset, pinBufferMin, out totalRows1, out edgeBuffer1, out pinMaxOffsetValue1);
          double me = totalDimension1;
          int totalRows2;
          double edgeBuffer2;
          double pinMaxOffsetValue2;
          MainPlacement.AlgorithmForRowsAndOffset(totalDimension1, pinMaxOffset, pinBufferMin, out totalRows2, out edgeBuffer2, out pinMaxOffsetValue2);
          double num4;
          double num5;
          if (!noReEntrant)
          {
            Transform identity = Transform.Identity;
            identity.BasisX = transform2.BasisZ.CrossProduct(transform1.BasisZ);
            identity.BasisY = transform1.BasisZ;
            identity.BasisZ = transform2.BasisZ;
            bool bSymbol;
            PlanarFace topFaceReturn;
            List<PlanarFace> upFaces;
            XYZ xyz1 = DirectionalMarkerMain.locationForOrienatation(element7, element8, document, identity, out bSymbol, out topFaceReturn, out upFaces);
            double x = xyz1.X + edgeBuffer2;
            double num6 = xyz1.Z + edgeBuffer1;
            double y = xyz1.Y;
            double z1 = num6;
            XYZ xyz2 = new XYZ(x, y, z1);
            bool flag3 = PlacementUtilities.IsMirrored(element7);
            if ((GeometryObject) topFaceReturn == (GeometryObject) null)
            {
              elementList4.Add(element7);
              flag2 = true;
            }
            if (!flag2)
            {
              num4 = totalDimension2 - 2.0 * edgeBuffer1;
              num5 = totalDimension1 - 2.0 * edgeBuffer2;
              double num7 = me - edgeBuffer2;
              XYZ point = xyz2;
              ICollection<Element> source = (ICollection<Element>) new List<Element>();
              XYZ referenceDirection = identity.BasisZ;
              if (composite)
                referenceDirection = dictionary2.ContainsKey(element7.Id) ? (!dictionary2[element7.Id] ? (!flag3 ? identity.BasisZ : -identity.BasisZ) : identity.BasisX) : identity.BasisZ;
              int num8 = 0;
              for (int index1 = 0; index1 < totalRows2; ++index1)
              {
                for (int index2 = 0; index2 < totalRows1; ++index2)
                {
                  familySymbol.Activate();
                  if (MainPlacement.processProjectionAndEdgesForRentrantCorner(upFaces, bSymbol, topFaceReturn, point, composite, familySymbol, transform2, transform1, identity, document))
                  {
                    XYZ location = identity.OfPoint(point);
                    FamilyInstance elem = document.Create.NewFamilyInstance(topFaceReturn.Reference, location, referenceDirection, familySymbol);
                    Parameters.LookupParameter((Element) elem, "Offset from Host").Set(0);
                    ++num1;
                    source.Add((Element) elem);
                  }
                  double z2 = point.Z + pinMaxOffsetValue1;
                  point = new XYZ(point.X, point.Y, z2);
                  ++num8;
                }
                me -= pinMaxOffset;
                point = xyz2;
                if (!me.ApproximatelyEquals(0.0) || me > 0.0)
                  point = new XYZ(point.X + pinMaxOffsetValue2 * (double) (index1 + 1), point.Y, point.Z);
              }
              document.Regenerate();
              MainPlacement.cutVoidsAndPins(source.ToList<Element>(), document, element7);
            }
          }
          else
          {
            XYZ xyz3 = InsulationMarkingMain.LeftMostProjectedPoint(element8, element7, document);
            PlanarFace topFace;
            bool bSymbol;
            List<PlanarFace> allTopFaces;
            PlacementUtilities.GetAllTopFacesFromInsulation(document, element7, out topFace, out PlanarFace _, out bSymbol, out allTopFaces);
            if ((GeometryObject) topFace == (GeometryObject) null)
            {
              elementList4.Add(element7);
              flag2 = true;
            }
            if (!flag2)
            {
              XYZ xyz4;
              if (PlacementUtilities.IsMirrored(element8))
              {
                XYZ xyz5 = xyz3 + XYZ.BasisX * edgeBuffer2;
                flag1 = true;
                xyz4 = xyz5 + XYZ.BasisZ * edgeBuffer1;
              }
              else
                xyz4 = xyz3 - XYZ.BasisX * edgeBuffer2 + XYZ.BasisZ * edgeBuffer1;
              num4 = totalDimension2 - 2.0 * edgeBuffer1;
              num5 = totalDimension1 - 2.0 * edgeBuffer2;
              double num9 = me - edgeBuffer2;
              XYZ point = xyz4;
              ICollection<Element> source = (ICollection<Element>) new List<Element>();
              XYZ vec = XYZ.BasisZ;
              if (composite)
                vec = dictionary2.ContainsKey(element7.Id) ? (!dictionary2[element7.Id] ? (!flag1 ? XYZ.BasisZ : -XYZ.BasisZ) : XYZ.BasisX) : XYZ.BasisZ;
              int num10 = 0;
              for (int index3 = 0; index3 < totalRows2; ++index3)
              {
                for (int index4 = 0; index4 < totalRows1; ++index4)
                {
                  familySymbol.Activate();
                  XYZ xyz6 = transform2.OfPoint(point);
                  if (MainPlacement.processProjectionAndEdges(allTopFaces, bSymbol, topFace, xyz6, composite, familySymbol, transform2, transform1, document))
                  {
                    XYZ referenceDirection = transform2.OfVector(vec);
                    FamilyInstance elem = document.Create.NewFamilyInstance(topFace.Reference, xyz6, referenceDirection, familySymbol);
                    Parameters.LookupParameter((Element) elem, "Offset from Host").Set(0);
                    ++num1;
                    source.Add((Element) elem);
                  }
                  point += XYZ.BasisZ * pinMaxOffsetValue1;
                  ++num10;
                }
                me -= pinMaxOffset;
                point = xyz4;
                if (!me.ApproximatelyEquals(0.0) || me > 0.0)
                {
                  if (flag1)
                    point += XYZ.BasisX * pinMaxOffsetValue2 * (double) (index3 + 1);
                  else
                    point -= XYZ.BasisX * pinMaxOffsetValue2 * (double) (index3 + 1);
                }
              }
              document.Regenerate();
              MainPlacement.cutVoidsAndPins(source.ToList<Element>(), document, element7);
            }
          }
        }
      }
      if (elementList4.Count > 0)
      {
        string str = "";
        foreach (Element element in elementList4)
        {
          FamilyInstance familyInstance = element as FamilyInstance;
          str = $"{str}\n{familyInstance.Symbol.FamilyName} : {element.Name} - {element.Id?.ToString()}";
        }
        new TaskDialog("Automatic Pin Placement")
        {
          Title = "Automatic Pin Placement",
          MainContent = "Please check the solids for family instances on the following insulation pieces and try again.",
          ExpandedContent = str
        }.Show();
      }
      int num11 = (int) transaction.Commit();
      if (num1 > 0)
        new TaskDialog("Automatic Pin Placement")
        {
          MainContent = "All pins have been placed succesfully."
        }.Show();
    }
    return true;
  }

  public static void cutVoidsAndPins(List<Element> voidElements, Document revitdoc, Element elem)
  {
    foreach (FamilyInstance voidElement in voidElements)
    {
      foreach (ElementId subComponentId in (IEnumerable<ElementId>) voidElement.GetSubComponentIds())
      {
        Element element = revitdoc.GetElement(subComponentId);
        FamilyInstance familyInstance = element as FamilyInstance;
        Element cuttingInstance = familyInstance == null || familyInstance.SuperComponent == null ? element : revitdoc.GetElement(familyInstance.SuperComponent.Id);
        Parameter parameter = Parameters.LookupParameter(element, "MANUFACTURE_COMPONENT");
        if (parameter != null)
        {
          string str = parameter.AsString();
          if (parameter.HasValue && !string.IsNullOrEmpty(str) && str.Contains("VOID") && !InstanceVoidCutUtils.GetElementsBeingCut(cuttingInstance).Contains(elem.Id))
            InstanceVoidCutUtils.AddInstanceVoidCut(revitdoc, elem, cuttingInstance);
        }
      }
    }
  }

  public static bool processProjectionAndEdgesForRentrantCorner(
    List<PlanarFace> allTopFacingFaces,
    bool bSymbol,
    PlanarFace topFace,
    XYZ point,
    bool composite,
    FamilySymbol familySymbol,
    Transform wallTransform,
    Transform insulationTransform,
    Transform modifiedTransform,
    Document revitDoc)
  {
    bool flag1 = true;
    int num1 = 0;
    foreach (PlanarFace allTopFacingFace in allTopFacingFaces)
    {
      bool flag2 = false;
      if (!bSymbol)
      {
        XYZ point1 = modifiedTransform.OfPoint(point);
        if (allTopFacingFace.Project(point1) != null)
          flag2 = true;
        else
          ++num1;
      }
      else
      {
        point = modifiedTransform.OfPoint(point);
        point = insulationTransform.Inverse.OfPoint(point);
        if (allTopFacingFace.Project(point) != null)
          flag2 = true;
        else
          ++num1;
      }
      if (flag2)
      {
        Parameter parameter1 = Parameters.LookupParameter((Element) familySymbol, "DIM_DIAMETER");
        foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) allTopFacingFace.GetEdgesAsCurveLoops())
        {
          CurveLoop curveLoop1 = edgesAsCurveLoop;
          if (!bSymbol)
            curveLoop1 = CurveLoop.CreateViaTransform(edgesAsCurveLoop, insulationTransform.Inverse);
          CurveLoop curveLoop2 = curveLoop1;
          Plane plane = curveLoop2.GetPlane();
          foreach (Curve curve in curveLoop2)
          {
            XYZ xyz = insulationTransform.Inverse.OfPoint(point);
            XYZ point2 = new XYZ(xyz.X, plane.Origin.Y, xyz.Z);
            if (!composite)
            {
              if (curve.Distance(point2) < parameter1.AsDouble() / 2.0)
                return false;
            }
            else
            {
              Parameter parameter2 = Parameters.LookupParameter((Element) familySymbol, "DIM_WIDTH");
              double num2 = parameter1.AsDouble() / 2.0 + parameter2.AsDouble() / 2.0;
              if (curve.Distance(point2) < num2)
                return false;
            }
          }
        }
        flag1 = flag2;
      }
    }
    return num1 != allTopFacingFaces.Count && flag1;
  }

  public static bool processProjectionAndEdges(
    List<PlanarFace> allTopFacingFaces,
    bool bSymbol,
    PlanarFace topFace,
    XYZ point,
    bool composite,
    FamilySymbol familySymbol,
    Transform wallTransform,
    Transform insulationTransform,
    Document revitDoc)
  {
    bool flag1 = true;
    int num1 = 0;
    foreach (PlanarFace allTopFacingFace in allTopFacingFaces)
    {
      bool flag2 = false;
      if (bSymbol)
      {
        XYZ point1 = insulationTransform.Inverse.OfPoint(point);
        if (allTopFacingFace.Project(point1) != null)
          flag2 = true;
        else
          ++num1;
      }
      else if (allTopFacingFace.Project(point) != null)
        flag2 = true;
      else
        ++num1;
      if (flag2)
      {
        Parameter parameter1 = Parameters.LookupParameter((Element) familySymbol, "DIM_DIAMETER");
        foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) allTopFacingFace.GetEdgesAsCurveLoops())
        {
          CurveLoop curveLoop = edgesAsCurveLoop;
          if (bSymbol)
            curveLoop = CurveLoop.CreateViaTransform(edgesAsCurveLoop, wallTransform);
          CurveLoop viaTransform = CurveLoop.CreateViaTransform(curveLoop, wallTransform.Inverse);
          Plane plane = viaTransform.GetPlane();
          foreach (Curve curve in viaTransform)
          {
            XYZ xyz = wallTransform.Inverse.OfPoint(point);
            XYZ point2 = new XYZ(xyz.X, plane.Origin.Y, xyz.Z);
            if (!composite)
            {
              if (curve.Distance(point2) < parameter1.AsDouble() / 2.0)
                return false;
            }
            else
            {
              Parameter parameter2 = Parameters.LookupParameter((Element) familySymbol, "DIM_WIDTH");
              double num2 = parameter1.AsDouble() / 2.0 + parameter2.AsDouble() / 2.0;
              if (curve.Distance(point2) < num2)
                return false;
            }
          }
        }
        flag1 = flag2;
      }
    }
    return num1 != allTopFacingFaces.Count && flag1;
  }

  public static XYZ LeftMostPointonSolidBBox(
    Element wallPanel,
    Element insulation,
    Document revitDoc)
  {
    Transform transform1 = (wallPanel as FamilyInstance).GetTransform();
    Transform transform2 = (insulation as FamilyInstance).GetTransform();
    XYZ xyz1 = new XYZ();
    PlacementUtilities.IsMirrored(wallPanel);
    bool bSymbol;
    List<Solid> symbolSolids = Solids.GetSymbolSolids(insulation, out bSymbol, options: new Options()
    {
      ComputeReferences = true
    });
    List<XYZ> xyzList = new List<XYZ>();
    foreach (Solid solid in symbolSolids)
    {
      BoundingBoxXYZ boundingBox = solid.GetBoundingBox();
      double num1 = boundingBox.Max.X - boundingBox.Min.X;
      double num2 = boundingBox.Max.Y - boundingBox.Min.Y;
      double z1 = boundingBox.Max.Z;
      double z2 = boundingBox.Min.Z;
      XYZ point1 = boundingBox.Min + num1 * XYZ.BasisX;
      XYZ point2 = point1 + num2 * XYZ.BasisY;
      XYZ point3 = boundingBox.Min + num2 * XYZ.BasisY;
      XYZ min = boundingBox.Min;
      XYZ point4 = boundingBox.Max - num1 * XYZ.BasisX;
      XYZ point5 = point4 - num2 * XYZ.BasisY;
      XYZ point6 = boundingBox.Max - num2 * XYZ.BasisY;
      XYZ max = boundingBox.Max;
      XYZ point7 = boundingBox.Transform.OfPoint(min);
      XYZ point8 = boundingBox.Transform.OfPoint(point1);
      XYZ point9 = boundingBox.Transform.OfPoint(point2);
      XYZ point10 = boundingBox.Transform.OfPoint(point3);
      XYZ point11 = boundingBox.Transform.OfPoint(max);
      XYZ point12 = boundingBox.Transform.OfPoint(point4);
      XYZ point13 = boundingBox.Transform.OfPoint(point5);
      XYZ point14 = boundingBox.Transform.OfPoint(point6);
      if (bSymbol)
      {
        point7 = transform2.OfPoint(point7);
        point8 = transform2.OfPoint(point8);
        point9 = transform2.OfPoint(point9);
        point10 = transform2.OfPoint(point10);
        point11 = transform2.OfPoint(point11);
        point12 = transform2.OfPoint(point12);
        point13 = transform2.OfPoint(point13);
        point14 = transform2.OfPoint(point14);
      }
      XYZ xyz2 = transform1.Inverse.OfPoint(point7);
      XYZ xyz3 = transform1.Inverse.OfPoint(point8);
      XYZ xyz4 = transform1.Inverse.OfPoint(point9);
      XYZ xyz5 = transform1.Inverse.OfPoint(point10);
      XYZ xyz6 = transform1.Inverse.OfPoint(point11);
      XYZ xyz7 = transform1.Inverse.OfPoint(point12);
      XYZ xyz8 = transform1.Inverse.OfPoint(point13);
      XYZ xyz9 = transform1.Inverse.OfPoint(point14);
      xyzList.Add(xyz2);
      xyzList.Add(xyz3);
      xyzList.Add(xyz4);
      xyzList.Add(xyz5);
      xyzList.Add(xyz6);
      xyzList.Add(xyz7);
      xyzList.Add(xyz8);
      xyzList.Add(xyz9);
    }
    double x = double.MinValue;
    double y = double.MaxValue;
    double z = double.MaxValue;
    foreach (XYZ xyz10 in xyzList)
    {
      if (xyz10.X > x)
        x = xyz10.X;
      if (xyz10.Y < y)
        y = xyz10.Y;
      if (xyz10.Z < z)
        z = xyz10.Z;
    }
    return new XYZ(x, y, z);
  }

  public static void AlgorithmForRowsAndOffset(
    double totalDimension,
    double pinMaxOffset,
    double pinBufferMin,
    out int totalRows,
    out double edgeBuffer,
    out double pinMaxOffsetValue)
  {
    double num1 = pinMaxOffset;
    int num2;
    if (totalDimension < pinMaxOffset)
    {
      if (totalDimension < pinMaxOffset / 2.0)
      {
        num2 = 0;
      }
      else
      {
        num2 = 1;
        num1 = totalDimension / 2.0;
      }
    }
    else
    {
      num2 = (int) Math.Ceiling(totalDimension / pinMaxOffset);
      num1 = (totalDimension - (double) (num2 - 1) * pinMaxOffset) / 2.0;
      if (num1 < pinBufferMin)
      {
        num1 = pinBufferMin;
        pinMaxOffset = (totalDimension - num1 * 2.0) / (double) (num2 - 1);
      }
    }
    edgeBuffer = num1;
    totalRows = num2;
    pinMaxOffsetValue = pinMaxOffset;
  }

  private static void greaterThan(Dictionary<string, List<Element>> elems = null)
  {
    string str1 = "";
    foreach (string key in elems.Keys)
    {
      str1 = $"{str1}{key}:\n";
      foreach (Element element in elems[key])
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        string str2 = familyInstance.Symbol.FamilyName.Equals(familyInstance.Name) ? familyInstance.Symbol.FamilyName : $"{familyInstance.Symbol.FamilyName} : {familyInstance.Name}";
        str1 = $"{str1}    {str2} - {element.Id?.ToString()}\n";
      }
      str1 += "\n";
    }
    new TaskDialog("Automatic Pin Placement")
    {
      Title = "Automatic Pin Placement",
      MainContent = "Please check the PIN_BUFFER parameter on the family instances listed below. This value cannot be greater than DIM_WIDTH or DIM_LENGTH parameters.",
      ExpandedContent = str1
    }.Show();
  }

  private static void NegativeParameter(Dictionary<string, List<Element>> elems = null)
  {
    string str1 = "";
    foreach (string key in elems.Keys)
    {
      str1 = $"{str1}{key}:\n";
      foreach (Element element in elems[key])
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        string str2 = familyInstance.Symbol.FamilyName.Equals(familyInstance.Name) ? familyInstance.Symbol.FamilyName : $"{familyInstance.Symbol.FamilyName} : {familyInstance.Name}";
        str1 = $"{str1}    {str2} - {element.Id?.ToString()}\n";
      }
      str1 += "\n";
    }
    new TaskDialog("Automatic Pin Placement")
    {
      Title = "Automatic Pin Placement",
      MainContent = "Please check the following parameter values for the family instances listed below. This value cannot be negative.",
      ExpandedContent = str1
    }.Show();
  }

  private static void ZeroParameter(Dictionary<string, List<Element>> elems = null)
  {
    string str1 = "";
    foreach (string key in elems.Keys)
    {
      str1 = $"{str1}{key}:\n";
      foreach (Element element in elems[key])
      {
        FamilyInstance familyInstance = element as FamilyInstance;
        string str2 = familyInstance.Symbol.FamilyName.Equals(familyInstance.Name) ? familyInstance.Symbol.FamilyName : $"{familyInstance.Symbol.FamilyName} : {familyInstance.Name}";
        str1 = $"{str1}    {str2} - {element.Id?.ToString()}\n";
      }
      str1 += "\n";
    }
    new TaskDialog("Automatic Pin Placement")
    {
      Title = "Automatic Pin Placement",
      MainContent = "Please check the following parameter values for the family instances listed below. This value cannot be zero.",
      ExpandedContent = str1
    }.Show();
  }
}
