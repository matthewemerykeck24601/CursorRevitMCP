// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.ExcelExport
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DocumentFormat.OpenXml.Spreadsheet;
using EDGE.AssemblyTools.MarkVerification.Tools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.ExcelUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ExcelExport : IExternalCommand
{
  private bool isMetromont;
  private Dictionary<PrecastMaterialType, ElementId> mappedMaterialIndex;
  private List<ElementId> materialIdIndex;
  private IEnumerable<FamilyInstance> allProjectAddons;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    View activeView = document.ActiveView;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Estimating Excel Export must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Estimating Excel Export must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    this.mappedMaterialIndex = Materials.GetPrecastMaterialIds(document);
    if (this.mappedMaterialIndex != null)
      this.materialIdIndex = this.mappedMaterialIndex.Values.ToList<ElementId>();
    this.allProjectAddons = Components.GetAllProjectAddons(document);
    string str1 = "";
    Autodesk.Revit.DB.Parameter parameter = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
      str1 = parameter.AsString();
    if (str1.ToUpper().Contains("METROMONT"))
      this.isMetromont = true;
    string[] strArray = new string[10]
    {
      "Location",
      "Control Mark",
      "Overhead Door Average Width",
      "Overhead Door Average Height",
      "Personal Door Average Width",
      "Personal Door Average Height",
      "Windows and Other Average Width",
      "Windows and Other Average Height",
      "Haunches Form",
      "Haunches Up"
    };
    try
    {
      Dictionary<string, Dictionary<string, List<string>>> dictionary1 = new Dictionary<string, Dictionary<string, List<string>>>();
      if (App.DialogSwitches.MarkVerificationWarning)
      {
        TaskDialog taskDialog = new TaskDialog("PE Export");
        taskDialog.Title = "PE Export";
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 6;
        taskDialog.MainInstruction = "For a more accurate representation of different cutout or haunch configurations, run Mark Verification before running PE Export. Continue with Export ?";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.VerificationText = "Don't show this again?";
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialog.WasVerificationChecked())
          App.DialogSwitches.MarkVerificationWarning = false;
        if (taskDialogResult == 7 || taskDialogResult == 2)
          return (Result) 1;
      }
      List<Element> elementList1 = new List<Element>();
      List<Element> elementList2 = new List<Element>();
      List<Element> list1 = new FilteredElementCollector(document).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).ToList<Element>();
      SortedDictionary<string, Dictionary<string, Dictionary<string, List<Element>>>> sortedDictionary = new SortedDictionary<string, Dictionary<string, Dictionary<string, List<Element>>>>();
      List<Element> source = new List<Element>();
      foreach (Element structFramingElem in list1)
      {
        if (structFramingElem is FamilyInstance)
        {
          Element p = (Element) AssemblyInstances.GetFlatElement(document, structFramingElem as FamilyInstance);
          if (source.Where<Element>((Func<Element, bool>) (e => e.Id.IntegerValue == p.Id.IntegerValue)).ToList<Element>().Count < 1)
            source.Add(p);
        }
      }
      source.Distinct<Element>();
      foreach (Element element in source)
      {
        if (element is FamilyInstance)
        {
          FamilyInstance flatElement = AssemblyInstances.GetFlatElement(document, element as FamilyInstance);
          if (!(flatElement.GetSuperComponent() is FamilyInstance familyInstance))
            familyInstance = flatElement;
          if (Utils.ElementUtils.Parameters.LookupParameter((Element) flatElement, "CONSTRUCTION_PRODUCT") != null)
          {
            string constPro = Utils.ElementUtils.Parameters.LookupParameter((Element) flatElement, "CONSTRUCTION_PRODUCT").AsString();
            if (constPro == null || constPro == "")
            {
              elementList1.Add(element);
            }
            else
            {
              string upper1 = constPro.ToUpper();
              string fullName = this.GetFullName(constPro, (Element) flatElement, false);
              string str2 = fullName.Length > 20 ? fullName.Substring(0, 20).ToUpper() : fullName.ToUpper();
              if (!dictionary1.ContainsKey(str2))
                dictionary1.Add(str2, new Dictionary<string, List<string>>()
                {
                  {
                    upper1,
                    new List<string>()
                    {
                      this.GetFullName(str2, (Element) flatElement, true)
                    }
                  }
                });
              else if (dictionary1[str2].ContainsKey(upper1) && !dictionary1[str2][upper1].Contains(this.GetFullName(str2, (Element) flatElement, true)))
                dictionary1[str2][upper1].Add(this.GetFullName(str2, (Element) flatElement, true));
              string upper2 = this.GetFullName(str2, (Element) flatElement, true).ToUpper();
              string oldValue = upper2;
              while (!string.IsNullOrEmpty(oldValue))
              {
                if (str2.Contains(oldValue))
                {
                  str2 = str2.Replace(oldValue, "");
                  break;
                }
                oldValue = oldValue.Remove(oldValue.Length - 1, 1);
                if (oldValue == " ")
                  oldValue = "";
              }
              if (!str2.Contains(upper2))
                str2 += upper2;
              if (familyInstance.LookupParameter("CONTROL_MARK") != null && familyInstance.LookupParameter("PROD_ERECT_SEQUENCE") != null)
              {
                if (!this.CheckMaterials(element))
                  elementList2.Add(element);
                string key1 = familyInstance.LookupParameter("CONTROL_MARK").AsString();
                string key2 = familyInstance.LookupParameter("PROD_ERECT_SEQUENCE").AsString();
                Dictionary<string, Dictionary<string, List<Element>>> dictionary2 = new Dictionary<string, Dictionary<string, List<Element>>>();
                if (sortedDictionary.ContainsKey(str2.ToUpper()))
                  dictionary2 = sortedDictionary[str2.ToUpper()];
                if (key2 == null)
                  key2 = "";
                if (!dictionary2.Keys.Contains<string>(key1))
                  dictionary2.Add(key1, new Dictionary<string, List<Element>>());
                if (!dictionary2[key1].Keys.Contains<string>(key2))
                  dictionary2[key1].Add(key2, new List<Element>()
                  {
                    (Element) flatElement
                  });
                else
                  dictionary2[key1][key2].Add((Element) flatElement);
                if (sortedDictionary.ContainsKey(str2.ToUpper()))
                  sortedDictionary[str2.ToUpper()] = dictionary2;
                else
                  sortedDictionary.Add(str2.ToUpper(), dictionary2);
                key1.Equals((string) null);
              }
            }
          }
        }
      }
      if (elementList1.Count > 0)
      {
        StringBuilder stringBuilder = new StringBuilder("");
        foreach (Element element in elementList1)
          stringBuilder = stringBuilder.Append($"{element.Name.ToString()} {element.Id.IntegerValue.ToString()}\n");
        string str3 = stringBuilder.ToString();
        new TaskDialog("No Construction Products")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = false,
          MainInstruction = "No Construction Products",
          MainContent = ("The following element(s) will not be exported because no value is assigned to their CONSTRUCTION_PRODUCT parameter: \n" + str3)
        }.Show();
      }
      if (elementList2.Count > 0)
      {
        StringBuilder stringBuilder = new StringBuilder("");
        foreach (Element element in elementList2)
        {
          string str4 = (element as FamilyInstance).Symbol.FamilyName.ToString();
          stringBuilder = stringBuilder.Append($"{str4} {element.Name.ToString()} {element.Id.IntegerValue.ToString()}\n");
        }
        string str5 = stringBuilder.ToString();
        new TaskDialog("No Materials")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = false,
          MainInstruction = "No Materials",
          MainContent = ("The volumes and/or weights for the following element(s) may not be accurate because they do not have a material assigned to them: \n" + str5)
        }.Show();
      }
      if (sortedDictionary.Count == 0)
        throw new Exception("NO CONST");
      StringBuilder stringBuilder1 = new StringBuilder();
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      saveFileDialog1.AddExtension = true;
      saveFileDialog1.DefaultExt = ".xlsx";
      saveFileDialog1.Filter = "Excel Files (*.xlsx)|*.xlsx";
      SaveFileDialog saveFileDialog2 = saveFileDialog1;
      string str6 = string.Empty;
      if (saveFileDialog2.ShowDialog().GetValueOrDefault())
        str6 = saveFileDialog2.FileName;
      ExcelDocument excelDocument = new ExcelDocument(str6);
      if (excelDocument.ExcelFileOpen)
      {
        List<string> stringList1 = new List<string>();
        List<ElementId> list2 = Components.GetVoidIds(document).ToList<ElementId>();
        foreach (string key3 in sortedDictionary.Keys)
        {
          Dictionary<string, Dictionary<string, List<Element>>> dictionary3 = sortedDictionary[key3];
          List<string> stringList2 = new List<string>();
          bool flag = true;
          SheetData sheet = excelDocument.GetSheet(key3);
          if (sheet != null)
          {
            List<ElementId> elementIdList = new List<ElementId>();
            List<Element> elementList3 = new List<Element>();
            List<Element> elementList4 = new List<Element>();
            List<Element> elementList5 = new List<Element>();
            foreach (ElementId id in list2)
            {
              Element element = document.GetElement(id);
              elementList3.Add(element);
            }
            List<string>[] productData = this.GetProductData(key3);
            foreach (string key4 in dictionary3.Keys)
            {
              foreach (string key5 in dictionary3[key4].Keys)
              {
                Element element1 = dictionary3[key4][key5].First<Element>();
                XYZ min = element1.get_BoundingBox((View) null).Min;
                XYZ max = element1.get_BoundingBox((View) null).Max;
                List<Element> elementList6 = new List<Element>();
                List<Element> elementList7 = new List<Element>();
                List<Element> elementList8 = new List<Element>();
                List<Element> elementList9 = new List<Element>();
                List<Element> elementList10 = new List<Element>();
                List<Element> elementList11 = new List<Element>();
                List<Element> elementList12 = new List<Element>();
                int num1 = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                int num5 = 0;
                int num6 = 0;
                int num7 = dictionary3[key4][key5].Count<Element>();
                int num8 = 0;
                int num9 = 0;
                foreach (Element elem in dictionary3[key4][key5])
                {
                  int parameterAsInt1 = Utils.ElementUtils.Parameters.GetParameterAsInt(elem, "UP FACE HAUNCH");
                  if (parameterAsInt1 > -1)
                    num8 += parameterAsInt1;
                  int parameterAsInt2 = Utils.ElementUtils.Parameters.GetParameterAsInt(elem, "FORM FACE HAUNCH");
                  if (parameterAsInt2 > -1)
                    num9 += parameterAsInt2;
                }
                Autodesk.Revit.DB.Outline outline = new Autodesk.Revit.DB.Outline(min, max);
                BoundingBoxIntersectsFilter intersectsFilter1 = new BoundingBoxIntersectsFilter(outline);
                FilteredElementCollector elementCollector = new FilteredElementCollector(document);
                BoundingBoxIntersectsFilter intersectsFilter2 = new BoundingBoxIntersectsFilter(outline, true);
                string uniqueId = element1.UniqueId;
                List<Element> elementList13 = new List<Element>();
                foreach (Element elem in elementList3)
                {
                  if (elem.GetHostGuid().Equals(uniqueId))
                    elementList13.Add(elem);
                }
                foreach (Element elem in elementList13)
                {
                  if (elem.GetManufactureComponent() != null)
                  {
                    if (elem.Name.ToUpper().Contains("OVERHEAD"))
                    {
                      elementList6.Add(elem);
                      ++num1;
                    }
                    else if (elem.Name.ToUpper().Contains("PERSONAL"))
                    {
                      elementList7.Add(elem);
                      ++num2;
                    }
                    else if (elem.Name.ToUpper().Contains("POCKETS"))
                    {
                      elementList10.Add(elem);
                      ++num4;
                      Element element2 = elem.Document.GetElement(elem.GetHostGuid());
                      if (element2 != null)
                      {
                        Transform transform = (element2 as FamilyInstance).GetTransform();
                        XYZ basisX = transform.BasisX;
                        if (transform.Inverse.OfPoint((elem.Location as LocationPoint).Point).X >= 0.0)
                        {
                          elementList12.Add(elem);
                          ++num5;
                        }
                        else
                        {
                          elementList11.Add(elem);
                          ++num6;
                        }
                      }
                    }
                    else if (elem.Name.Equals("VOID_RECTANGULAR"))
                    {
                      elementList8.Add(elem);
                      ++num3;
                    }
                  }
                }
                string archVolume = this.GetArchVolume(element1);
                string grayVolume = this.GetGrayVolume(element1);
                double result1 = 0.0;
                double result2 = 0.0;
                double.TryParse(archVolume, out result1);
                double.TryParse(grayVolume, out result2);
                string str7 = (result1 + result2).ToString();
                if (key3.ToUpper().Contains("PRECAST"))
                {
                  productData[0].Add(key5);
                  productData[1].Add(key4);
                  productData[2].Add(num7.ToString());
                  productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                  productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                  productData[5].Add("");
                  productData[6].Add("");
                  productData[7].Add("");
                  productData[8].Add("");
                  productData[9].Add("");
                  productData[10].Add("");
                  productData[11].Add((num3 * num7).ToString());
                  productData[12].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                  productData[13].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  productData[14].Add(num9.ToString());
                  productData[15].Add(num8.ToString());
                }
                else if (key3.ToUpper().Contains("WALL") && !key3.ToUpper().Contains("INSUL"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add("");
                    productData[3].Add(archVolume);
                    productData[4].Add(grayVolume);
                    productData[5].Add(str7);
                    productData[6].Add(num7.ToString());
                    productData[7].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[8].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add((num1 * num7).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    productData[13].Add((num2 * num7).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[15].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    productData[16 /*0x10*/].Add((num3 * num7).ToString());
                    productData[17].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[18].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[19].Add(num9.ToString());
                    productData[20].Add(num8.ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add("");
                    productData[3].Add(num7.ToString());
                    productData[4].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[5].Add(this.AvgWallLengthWidth(element1, false, document));
                    List<string> stringList3 = productData[6];
                    int num10 = num1 * num7;
                    string str8 = num10.ToString();
                    stringList3.Add(str8);
                    productData[7].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    List<string> stringList4 = productData[9];
                    num10 = num2 * num7;
                    string str9 = num10.ToString();
                    stringList4.Add(str9);
                    productData[10].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    List<string> stringList5 = productData[12];
                    num10 = num3 * num7;
                    string str10 = num10.ToString();
                    stringList5.Add(str10);
                    productData[13].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[15].Add(num9.ToString());
                    productData[16 /*0x10*/].Add(num8.ToString());
                  }
                }
                else if (key3.ToUpper().Contains("INSUL") && key3.ToUpper().Contains("WALL"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add("");
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add((num1 * num7).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    productData[13].Add((num2 * num7).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[15].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    productData[16 /*0x10*/].Add((num3 * num7).ToString());
                    productData[17].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[18].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[19].Add(num9.ToString());
                    productData[20].Add(num8.ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add("");
                    List<string> stringList6 = productData[6];
                    int num11 = num1 * num7;
                    string str11 = num11.ToString();
                    stringList6.Add(str11);
                    productData[7].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    List<string> stringList7 = productData[9];
                    num11 = num2 * num7;
                    string str12 = num11.ToString();
                    stringList7.Add(str12);
                    productData[10].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    List<string> stringList8 = productData[12];
                    num11 = num3 * num7;
                    string str13 = num11.ToString();
                    stringList8.Add(str13);
                    productData[13].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[15].Add(num9.ToString());
                    productData[16 /*0x10*/].Add(num8.ToString());
                  }
                }
                else if (key3.ToUpper().Contains("METRODECK"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add("");
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add("");
                    productData[11].Add((num3 * num7).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[13].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add("");
                    productData[6].Add("");
                    productData[7].Add((num3 * num7).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[9].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("FLAT") && key3.ToUpper().Contains("SLAB"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add(this.WallWeight(element1));
                    productData[9].Add((num3 * num7).ToString());
                    productData[10].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add((num3 * num7).ToString());
                    productData[6].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[7].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("LGIRDER") || key3.ToUpper().Contains("RBEAM") || key3.ToUpper().Contains("L-GIRDER") || key3.ToUpper().Contains("R-BEAM"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[7].Add(this.WallWeight(element1));
                    productData[8].Add((num5 * num7).ToString());
                    productData[9].Add((num6 * num7).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[4].Add((num5 * num7).ToString());
                    productData[5].Add((num6 * num7).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("TGIRDER") || key3.ToUpper().Contains("T-BEAM") || key3.ToUpper().Contains("T-GIRDER"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[7].Add(this.WallWeight(element1));
                    productData[8].Add((num5 * num7).ToString());
                    productData[9].Add((num6 * num7).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[4].Add((num5 * num7).ToString());
                    productData[5].Add((num6 * num7).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("DOUBLE") || key3.ToUpper().Contains("DOUB"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add("2");
                    productData[6].Add(num7.ToString());
                    productData[7].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[8].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add((num3 * num7).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[13].Add("0");
                    productData[14].Add("0");
                    productData[15].Add("0");
                    productData[16 /*0x10*/].Add("0");
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add("2");
                    productData[3].Add(num7.ToString());
                    productData[4].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[5].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[6].Add((num3 * num7).ToString());
                    productData[7].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[9].Add("0");
                    productData[10].Add("0");
                    productData[11].Add("0");
                    productData[12].Add("0");
                  }
                }
                else if (key3.ToUpper().Contains("COLUMN"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[7].Add(this.WallWeight(element1));
                    productData[8].Add(num8.ToString());
                    productData[9].Add(num9.ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[4].Add(num8.ToString());
                    productData[5].Add(num9.ToString());
                  }
                }
                else if (key3.ToUpper().Contains("STAIR") && key3.ToUpper().Contains("LANDINGS"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add(this.WallWeight(element1));
                    productData[9].Add((num3 * num7).ToString());
                    productData[10].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add((num3 * num7).ToString());
                    productData[6].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[7].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("STAIR"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.GetOverallHeight(element1));
                    productData[8].Add(this.GetRiserCount(element1));
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add(this.GetStructuralThickness(element1));
                    productData[11].Add(this.GetOneFootLanding(element1));
                    productData[12].Add(this.GetTwoFeetLanding(element1));
                    productData[13].Add("0");
                    productData[14].Add("0");
                    productData[15].Add("0");
                    productData[16 /*0x10*/].Add("0");
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.GetOverallHeight(element1));
                    productData[5].Add(this.GetRiserCount(element1));
                    productData[6].Add(this.GetStructuralThickness(element1));
                    productData[7].Add(this.GetOneFootLanding(element1));
                    productData[8].Add(this.GetTwoFeetLanding(element1));
                    productData[9].Add("0");
                    productData[10].Add("0");
                    productData[11].Add("0");
                    productData[12].Add("0");
                  }
                }
                else if (key3.ToUpper().Contains("SPANDREL") || key3.ToUpper().Contains("FASCIA"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add(this.WallWeight(element1));
                    List<string> stringList9 = productData[9];
                    int num12 = num4 * num7;
                    string str14 = num12.ToString();
                    stringList9.Add(str14);
                    productData[10].Add(num9.ToString());
                    productData[11].Add(num8.ToString());
                    List<string> stringList10 = productData[12];
                    num12 = num3 * num7;
                    string str15 = num12.ToString();
                    stringList10.Add(str15);
                    productData[13].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add((num4 * num7).ToString());
                    productData[6].Add(num9.ToString());
                    productData[7].Add(num8.ToString());
                    productData[8].Add((num3 * num7).ToString());
                    productData[9].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[10].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("PC"))
                {
                  productData[0].Add(key5);
                  productData[1].Add(key4);
                  productData[2].Add(num7.ToString());
                  productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                  productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                  productData[5].Add("");
                  productData[6].Add("");
                  productData[7].Add("");
                  productData[8].Add("");
                  productData[9].Add("");
                  productData[10].Add("");
                  productData[11].Add((num3 * num7).ToString());
                  productData[12].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                  productData[13].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  productData[14].Add(num9.ToString());
                  productData[15].Add(num8.ToString());
                }
                else if (key3.ToUpper().Contains("MW") || key3.ToUpper().Contains("SWA") || key3.ToUpper().Contains("WPA"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add("");
                    productData[3].Add(archVolume);
                    productData[4].Add(grayVolume);
                    productData[5].Add(str7);
                    productData[6].Add(num7.ToString());
                    productData[7].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[8].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add((num1 * num7).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    productData[13].Add((num2 * num7).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[15].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    productData[16 /*0x10*/].Add((num3 * num7).ToString());
                    productData[17].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[18].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[15].Add(num9.ToString());
                    productData[16 /*0x10*/].Add(num8.ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add("");
                    productData[3].Add(num7.ToString());
                    productData[4].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[5].Add(this.AvgWallLengthWidth(element1, false, document));
                    List<string> stringList11 = productData[6];
                    int num13 = num1 * num7;
                    string str16 = num13.ToString();
                    stringList11.Add(str16);
                    productData[7].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    List<string> stringList12 = productData[9];
                    num13 = num2 * num7;
                    string str17 = num13.ToString();
                    stringList12.Add(str17);
                    productData[10].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    List<string> stringList13 = productData[12];
                    num13 = num3 * num7;
                    string str18 = num13.ToString();
                    stringList13.Add(str18);
                    productData[13].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[15].Add(num9.ToString());
                    productData[16 /*0x10*/].Add(num8.ToString());
                  }
                }
                else if (key3.ToUpper().Contains("WPB"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add("");
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add((num1 * num7).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    productData[13].Add((num2 * num7).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[15].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    productData[16 /*0x10*/].Add((num3 * num7).ToString());
                    productData[17].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[18].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[19].Add(num9.ToString());
                    productData[20].Add(num8.ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add("");
                    List<string> stringList14 = productData[6];
                    int num14 = num1 * num7;
                    string str19 = num14.ToString();
                    stringList14.Add(str19);
                    productData[7].Add(this.AvgHeightWidth(elementList6, true, document).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList6, false, document).ToString());
                    List<string> stringList15 = productData[9];
                    num14 = num2 * num7;
                    string str20 = num14.ToString();
                    stringList15.Add(str20);
                    productData[10].Add(this.AvgHeightWidth(elementList7, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList7, false, document).ToString());
                    List<string> stringList16 = productData[12];
                    num14 = num3 * num7;
                    string str21 = num14.ToString();
                    stringList16.Add(str21);
                    productData[13].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[15].Add(num9.ToString());
                    productData[16 /*0x10*/].Add(num8.ToString());
                  }
                }
                else if (key3.ToUpper().Contains("FMD") || key3.ToUpper().Contains("MDK"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add("");
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add("");
                    productData[11].Add((num3 * num7).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[13].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add("");
                    productData[6].Add("");
                    productData[7].Add((num3 * num7).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[9].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("FSA"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add(this.WallWeight(element1));
                    productData[9].Add((num3 * num7).ToString());
                    productData[10].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add((num3 * num7).ToString());
                    productData[6].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[7].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("LGA") || key3.ToUpper().Contains("RBA"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[7].Add(this.WallWeight(element1));
                    productData[8].Add((num5 * num7).ToString());
                    productData[9].Add((num6 * num7).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[4].Add((num5 * num7).ToString());
                    productData[5].Add((num6 * num7).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("TGA"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[7].Add(this.WallWeight(element1));
                    productData[8].Add((num5 * num7).ToString());
                    productData[9].Add((num6 * num7).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[4].Add((num5 * num7).ToString());
                    productData[5].Add((num6 * num7).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("DT"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add("2");
                    productData[6].Add(num7.ToString());
                    productData[7].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[8].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add((num3 * num7).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[12].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[13].Add("0");
                    productData[14].Add("0");
                    productData[15].Add("0");
                    productData[16 /*0x10*/].Add("0");
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add("2");
                    productData[3].Add(num7.ToString());
                    productData[4].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[5].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[6].Add((num3 * num7).ToString());
                    productData[7].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[8].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                    productData[9].Add("0");
                    productData[10].Add("0");
                    productData[11].Add("0");
                    productData[12].Add("0");
                  }
                }
                else if (key3.ToUpper().Contains("CLA"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[7].Add(this.WallWeight(element1));
                    productData[8].Add(num8.ToString());
                    productData[9].Add(num9.ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[4].Add(num8.ToString());
                    productData[5].Add(num9.ToString());
                  }
                }
                else if (key3.ToUpper().Contains("STF"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add(this.WallWeight(element1));
                    productData[9].Add((num3 * num7).ToString());
                    productData[10].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[11].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add((num3 * num7).ToString());
                    productData[6].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[7].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (key3.ToUpper().Contains("STA") || key3.ToUpper().Contains("STZ"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.GetOverallHeight(element1));
                    productData[8].Add(this.GetRiserCount(element1));
                    productData[9].Add(this.WallWeight(element1));
                    productData[10].Add(this.GetStructuralThickness(element1));
                    productData[11].Add(this.GetOneFootLanding(element1));
                    productData[12].Add(this.GetTwoFeetLanding(element1));
                    productData[13].Add("0");
                    productData[14].Add("0");
                    productData[15].Add("0");
                    productData[16 /*0x10*/].Add("0");
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.GetOverallHeight(element1));
                    productData[5].Add(this.GetRiserCount(element1));
                    productData[6].Add(this.GetStructuralThickness(element1));
                    productData[7].Add(this.GetOneFootLanding(element1));
                    productData[8].Add(this.GetTwoFeetLanding(element1));
                    productData[9].Add("0");
                    productData[10].Add("0");
                    productData[11].Add("0");
                    productData[12].Add("0");
                  }
                }
                else if (key3.ToUpper().Contains("SPA") || key3.ToUpper().Contains("FCA"))
                {
                  if (!this.isMetromont)
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(archVolume);
                    productData[3].Add(grayVolume);
                    productData[4].Add(str7);
                    productData[5].Add(num7.ToString());
                    productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[8].Add(this.WallWeight(element1));
                    List<string> stringList17 = productData[9];
                    int num15 = num4 * num7;
                    string str22 = num15.ToString();
                    stringList17.Add(str22);
                    productData[10].Add(num9.ToString());
                    productData[11].Add(num8.ToString());
                    List<string> stringList18 = productData[12];
                    num15 = num3 * num7;
                    string str23 = num15.ToString();
                    stringList18.Add(str23);
                    productData[13].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[14].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                  else
                  {
                    productData[0].Add(key5);
                    productData[1].Add(key4);
                    productData[2].Add(num7.ToString());
                    productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                    productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                    productData[5].Add((num4 * num7).ToString());
                    productData[6].Add(num9.ToString());
                    productData[7].Add(num8.ToString());
                    productData[8].Add((num3 * num7).ToString());
                    productData[9].Add(this.AvgHeightWidth(elementList8, true, document).ToString());
                    productData[10].Add(this.AvgHeightWidth(elementList8, false, document).ToString());
                  }
                }
                else if (!this.isMetromont)
                {
                  productData[0].Add(key5);
                  productData[1].Add(key4);
                  productData[2].Add(archVolume);
                  productData[3].Add(grayVolume);
                  productData[4].Add(str7);
                  productData[5].Add(num7.ToString());
                  productData[6].Add(this.AvgWallLengthWidth(element1, true, document));
                  productData[7].Add(this.AvgWallLengthWidth(element1, false, document));
                  productData[8].Add(this.WallWeight(element1));
                }
                else
                {
                  productData[0].Add(key5);
                  productData[1].Add(key4);
                  productData[2].Add(num7.ToString());
                  productData[3].Add(this.AvgWallLengthWidth(element1, true, document));
                  productData[4].Add(this.AvgWallLengthWidth(element1, false, document));
                }
              }
            }
            object[,] objArray = new object[productData[0].Count, 23];
            if (key3.ToUpper().Contains("PRECAST"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                objArray[index, 0] = (object) productData[0][index];
                objArray[index, 1] = (object) productData[1][index];
                objArray[index, 2] = (object) productData[2][index];
                objArray[index, 3] = (object) productData[3][index];
                objArray[index, 4] = (object) productData[4][index];
                objArray[index, 5] = (object) productData[5][index];
                objArray[index, 6] = (object) productData[6][index];
                objArray[index, 7] = (object) productData[7][index];
                objArray[index, 8] = (object) productData[8][index];
                objArray[index, 9] = (object) productData[9][index];
                objArray[index, 10] = (object) productData[10][index];
                objArray[index, 11] = (object) productData[11][index];
                objArray[index, 12] = (object) productData[12][index];
                objArray[index, 13] = (object) productData[13][index];
                objArray[index, 14] = (object) productData[14][index];
                objArray[index, 15] = (object) productData[15][index];
              }
            }
            else if (key3.ToUpper().Contains("WALL") && !key3.ToUpper().Contains("INSUL"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                  objArray[index, 17] = (object) productData[17][index];
                  objArray[index, 18] = (object) productData[18][index];
                  objArray[index, 19] = (object) productData[19][index];
                  objArray[index, 20] = (object) productData[20][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("INSUL") && key3.ToUpper().Contains("WALL"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                  objArray[index, 17] = (object) productData[17][index];
                  objArray[index, 18] = (object) productData[18][index];
                  objArray[index, 19] = (object) productData[19][index];
                  objArray[index, 20] = (object) productData[20][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("METRODECK"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("FLAT") && key3.ToUpper().Contains("SLAB"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("LGIRDER") || key3.ToUpper().Contains("RBEAM") || key3.ToUpper().Contains("L-GIRDER") || key3.ToUpper().Contains("R-BEAM"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("TGIRDER") || key3.ToUpper().Contains("T-GIRDER") || key3.ToUpper().Contains("T-BEAM"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("DOUBLE") || key3.ToUpper().Contains("DOUB"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("COLUMN"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("STAIR") && key3.ToUpper().Contains("LANDINGS"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("STAIR"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("SPANDREL") || key3.ToUpper().Contains("FASCIA"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("PC"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                objArray[index, 0] = (object) productData[0][index];
                objArray[index, 1] = (object) productData[1][index];
                objArray[index, 2] = (object) productData[2][index];
                objArray[index, 3] = (object) productData[3][index];
                objArray[index, 4] = (object) productData[4][index];
                objArray[index, 5] = (object) productData[5][index];
                objArray[index, 6] = (object) productData[6][index];
                objArray[index, 7] = (object) productData[7][index];
                objArray[index, 8] = (object) productData[8][index];
                objArray[index, 9] = (object) productData[9][index];
                objArray[index, 10] = (object) productData[10][index];
                objArray[index, 11] = (object) productData[11][index];
                objArray[index, 12] = (object) productData[12][index];
                objArray[index, 13] = (object) productData[13][index];
                objArray[index, 14] = (object) productData[14][index];
                objArray[index, 15] = (object) productData[15][index];
              }
            }
            else if (key3.ToUpper().Contains("MW") || key3.ToUpper().Contains("SWA") || key3.ToUpper().Contains("WPA"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                  objArray[index, 17] = (object) productData[17][index];
                  objArray[index, 18] = (object) productData[18][index];
                  objArray[index, 19] = (object) productData[19][index];
                  objArray[index, 20] = (object) productData[20][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("WPB"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                  objArray[index, 17] = (object) productData[17][index];
                  objArray[index, 18] = (object) productData[18][index];
                  objArray[index, 19] = (object) productData[19][index];
                  objArray[index, 20] = (object) productData[20][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("FMD") || key3.ToUpper().Contains("MDK"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("FSA"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("LGA") || key3.ToUpper().Contains("RBA"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("TGA"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("DT"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("CLA"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("STF"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("STA") || key3.ToUpper().Contains("STZ"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                  objArray[index, 15] = (object) productData[15][index];
                  objArray[index, 16 /*0x10*/] = (object) productData[16 /*0x10*/][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                }
              }
            }
            else if (key3.ToUpper().Contains("SPA") || key3.ToUpper().Contains("FCA"))
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                  objArray[index, 11] = (object) productData[11][index];
                  objArray[index, 12] = (object) productData[12][index];
                  objArray[index, 13] = (object) productData[13][index];
                  objArray[index, 14] = (object) productData[14][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                  objArray[index, 9] = (object) productData[9][index];
                  objArray[index, 10] = (object) productData[10][index];
                }
              }
            }
            else
            {
              for (int index = 0; index < productData[0].Count; ++index)
              {
                if (!this.isMetromont)
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                  objArray[index, 5] = (object) productData[5][index];
                  objArray[index, 6] = (object) productData[6][index];
                  objArray[index, 7] = (object) productData[7][index];
                  objArray[index, 8] = (object) productData[8][index];
                }
                else
                {
                  objArray[index, 0] = (object) productData[0][index];
                  objArray[index, 1] = (object) productData[1][index];
                  objArray[index, 2] = (object) productData[2][index];
                  objArray[index, 3] = (object) productData[3][index];
                  objArray[index, 4] = (object) productData[4][index];
                }
              }
            }
            for (int index1 = 0; index1 < objArray.GetLength(0); ++index1)
            {
              for (int index2 = 0; index2 < objArray.GetLength(1); ++index2)
              {
                if (objArray[index1, index2] is string str24 && !string.IsNullOrWhiteSpace(str24) && !excelDocument.UpdateCellValue(index2 + 1, index1 + 1, str24, ExcelEnums.ExcelCellFormat.General, sheet))
                  flag = false;
              }
            }
          }
          if (!flag)
            stringList1.Add(key3);
        }
        if (stringList1.Count > 0)
        {
          TaskDialog taskDialog1 = new TaskDialog("Warning");
          taskDialog1.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog1.AllowCancellation = false;
          taskDialog1.MainInstruction = "Unable to Update Cell Values";
          taskDialog1.MainContent = "Unable to update one or more cell values in one or more work sheets. Please check the file for accuracy.";
          taskDialog1.ExpandedContent = "Work Sheets:\n";
          foreach (string str25 in stringList1)
          {
            TaskDialog taskDialog2 = taskDialog1;
            taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str25}\n";
          }
          taskDialog1.Show();
        }
        excelDocument.GetSheet("Sheet1");
        excelDocument.SaveAndClose();
        try
        {
          if (new FileInfo(str6).Exists)
            new Process()
            {
              StartInfo = new ProcessStartInfo(str6)
              {
                UseShellExecute = true
              }
            }.Start();
        }
        catch (Exception ex)
        {
          new TaskDialog("PE Export")
          {
            MainInstruction = "Export Successful",
            MainContent = ("Export file created: " + str6),
            AllowCancellation = false
          }.Show();
        }
      }
      return (Result) 0;
    }
    catch (Exception ex)
    {
      if (ex.ToString().ToUpper().Contains("NO CONST"))
      {
        new TaskDialog("EDGE Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          AllowCancellation = false,
          MainInstruction = "No Construction Products",
          MainContent = "No Construction Products found to export. Cancelling."
        }.Show();
        return (Result) 1;
      }
      message = ex.Message;
      return (Result) -1;
    }
  }

  public string AvgHeightWidth(List<Element> elementList, bool isWidth, Document doc)
  {
    double num1 = 0.0;
    double num2 = 0.0;
    double num3 = 0.0;
    new MKTolerances(doc).GetTolerance(MKToleranceAspect.Geometry);
    doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
    foreach (Element element in elementList)
    {
      double num4 = element.LookupParameter("Length") != null ? element.LookupParameter("Length").AsDouble() : 0.0;
      double num5 = element.LookupParameter("Width") != null ? element.LookupParameter("Width").AsDouble() : 0.0;
      num2 += num4;
      num3 += num5;
      ++num1;
    }
    if (num1 == 0.0)
      return "0";
    double num6 = num2 / num1;
    double num7 = num3 / num1;
    return isWidth ? Math.Round(num7, 2).ToString() : Math.Round(num6, 2).ToString();
  }

  public string AvgWallLengthWidth(Element element, bool isWidth, Document doc)
  {
    new MKTolerances(doc).GetTolerance(MKToleranceAspect.Geometry);
    doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
    double num1 = element.LookupParameter("DIM_LENGTH") != null ? element.LookupParameter("DIM_LENGTH").AsDouble() : 0.0;
    double num2 = element.LookupParameter("DIM_WIDTH") != null ? element.LookupParameter("DIM_WIDTH").AsDouble() : (Utils.ElementUtils.Parameters.GetParameterAsDouble(element, "DIM_HEIGHT") == 0.0 ? 0.0 : Utils.ElementUtils.Parameters.GetParameterAsDouble(element, "DIM_HEIGHT"));
    return isWidth ? Math.Round(num2, 2).ToString() : Math.Round(num1, 2).ToString();
  }

  private string DecimalToFraction(double decValue)
  {
    double d = Math.Abs(decValue);
    Math.Truncate(d);
    string str = d.ToString().Replace(Math.Truncate(d).ToString() + ".", "");
    double num1 = Convert.ToDouble(str);
    double num2 = Math.Pow(10.0, (double) str.Length);
    double num3 = 0.0;
    for (int index = 1; (double) index <= num2; ++index)
    {
      if (num1 % (double) index == 0.0 && num2 % (double) index == 0.0)
        num3 = (double) index;
    }
    return $"{num1 / num3}/{num2 / num3}";
  }

  private static int GCD(int x, int y) => y == 0 ? x : ExcelExport.GCD(y, x % y);

  public List<string>[] GetProductData(string product)
  {
    if (product.ToUpper().Contains("PRECAST"))
      return new List<string>[16 /*0x10*/]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Shipping Width" },
        new List<string>() { "Shipping Length" },
        new List<string>() { "Average Thickness" },
        new List<string>() { "Finish Width" },
        new List<string>() { "Finish Length" },
        new List<string>() { "Cold Jt Total LF" },
        new List<string>() { "Mono Jt Total LF" },
        new List<string>() { "Projection Total LF" },
        new List<string>() { "Openings" },
        new List<string>() { "Openings Width" },
        new List<string>() { "Openings Height" },
        new List<string>() { "Form Haunches" },
        new List<string>() { "Up Haunches" }
      };
    if (product.ToUpper().Contains("WALL") && !product.ToUpper().Contains("INSULATED"))
    {
      if (!this.isMetromont)
        return new List<string>[21]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "+/- CY" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Overhead Doors Openings" },
          new List<string>() { "Overhead Doors Width" },
          new List<string>() { "Overhead Doors Height" },
          new List<string>() { "Personal Doors Openings" },
          new List<string>() { "Personal Doors Width" },
          new List<string>() { "Personal Doors Height" },
          new List<string>() { "Windows Openings" },
          new List<string>() { "Windows Width" },
          new List<string>() { "Windows Height" },
          new List<string>() { "Form Haunches" },
          new List<string>() { "Up Haunches" }
        };
      return new List<string>[17]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "+/- CY" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Overhead Doors Openings" },
        new List<string>() { "Overhead Doors Width" },
        new List<string>() { "Overhead Doors Height" },
        new List<string>() { "Personal Doors Openings" },
        new List<string>() { "Personal Doors Width" },
        new List<string>() { "Personal Doors Height" },
        new List<string>() { "Windows Openings" },
        new List<string>() { "Windows Width" },
        new List<string>() { "Windows Height" },
        new List<string>() { "Form Haunches" },
        new List<string>() { "Up Haunches" }
      };
    }
    if (product.ToUpper().Contains("INSULATED") && product.ToUpper().Contains("WALL"))
    {
      if (!this.isMetromont)
        return new List<string>[21]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Insul Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Overhead Doors Openings" },
          new List<string>() { "Overhead Doors Width" },
          new List<string>() { "Overhead Doors Height" },
          new List<string>() { "Personal Doors Openings" },
          new List<string>() { "Personal Doors Width" },
          new List<string>() { "Personal Doors Height" },
          new List<string>() { "Windows Openings" },
          new List<string>() { "Windows Width" },
          new List<string>() { "Windows Height" },
          new List<string>() { "Form Haunches" },
          new List<string>() { "Up Haunches" }
        };
      return new List<string>[17]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Insul Length" },
        new List<string>() { "Overhead Doors Openings" },
        new List<string>() { "Overhead Doors Width" },
        new List<string>() { "Overhead Doors Height" },
        new List<string>() { "Personal Doors Openings" },
        new List<string>() { "Personal Doors Width" },
        new List<string>() { "Personal Doors Height" },
        new List<string>() { "Windows Openings" },
        new List<string>() { "Windows Width" },
        new List<string>() { "Windows Height" },
        new List<string>() { "Form Haunches" },
        new List<string>() { "Up Haunches" }
      };
    }
    if (product.ToUpper().Contains("METRODECK") && product.ToUpper().Contains("FACT"))
    {
      if (!this.isMetromont)
        return new List<string>[14]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Insul Length" },
          new List<string>() { "Weight" },
          new List<string>() { "End Core Width" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Length" }
        };
      return new List<string>[10]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Insul Length" },
        new List<string>() { "End Core Width" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Length" }
      };
    }
    if (product.ToUpper().Contains("METRODECK"))
    {
      if (!this.isMetromont)
        return new List<string>[14]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Insul Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Number Cores" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Length" }
        };
      return new List<string>[10]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Insul Length" },
        new List<string>() { "Number Cores" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Length" }
      };
    }
    if (product.ToUpper().Contains("FLAT") && product.ToUpper().Contains("SLAB"))
    {
      if (!this.isMetromont)
        return new List<string>[12]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Height" }
        };
      return new List<string>[8]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Height" }
      };
    }
    if (product.ToUpper().Contains("LGIRDER") || product.ToUpper().Contains("RBEAM") || product.ToUpper().Contains("L-GIRDER") || product.ToUpper().Contains("R-BEAM"))
    {
      if (!this.isMetromont)
        return new List<string>[10]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Left Pockets" },
          new List<string>() { "Right Pockets" }
        };
      return new List<string>[6]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Length" },
        new List<string>() { "Left Pockets" },
        new List<string>() { "Right Pockets" }
      };
    }
    if (product.ToUpper().Contains("TGIRDER") || product.ToUpper().Contains("T-BEAM") || product.ToUpper().Contains("T-GIRDER"))
    {
      if (!this.isMetromont)
        return new List<string>[10]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Left Pockets" },
          new List<string>() { "Right Pockets" }
        };
      return new List<string>[6]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Length" },
        new List<string>() { "Left Pockets" },
        new List<string>() { "Right Pockets" }
      };
    }
    if (product.ToUpper().Contains("DOUBLE") || product.ToUpper().Contains("DOUB"))
    {
      if (!this.isMetromont)
        return new List<string>[17]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Stems" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Length" },
          new List<string>() { "End 1 Depth" },
          new List<string>() { "End 1 Width" },
          new List<string>() { "End 2 Depth" },
          new List<string>() { "End 2 Width" }
        };
      return new List<string>[13]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Stems" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Length" },
        new List<string>() { "End 1 Depth" },
        new List<string>() { "End 1 Width" },
        new List<string>() { "End 2 Depth" },
        new List<string>() { "End 2 Width" }
      };
    }
    if (product.ToUpper().Contains("COLUMN"))
    {
      if (!this.isMetromont)
        return new List<string>[10]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Haunches" },
          new List<string>() { "Pockets" }
        };
      return new List<string>[6]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Length" },
        new List<string>() { "Haunches" },
        new List<string>() { "Pockets" }
      };
    }
    if (product.ToUpper().Contains("STAIR") && product.ToUpper().Contains("LANDINGS"))
    {
      if (!this.isMetromont)
        return new List<string>[12]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Height" }
        };
      return new List<string>[8]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Height" }
      };
    }
    if (product.ToUpper().Contains("STAIR"))
    {
      if (!this.isMetromont)
        return new List<string>[17]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Overall Height" },
          new List<string>() { "Risers" },
          new List<string>() { "Weight" },
          new List<string>() { "Structural Thickness" },
          new List<string>() { "Landing 1 Ft" },
          new List<string>() { "Landing 2 Ft" },
          new List<string>() { "Dim A" },
          new List<string>() { "Dim B" },
          new List<string>() { "Dim C" },
          new List<string>() { "Dim D" }
        };
      return new List<string>[13]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Overall Height" },
        new List<string>() { "Risers" },
        new List<string>() { "Structural Thickness" },
        new List<string>() { "Landing 1 Ft" },
        new List<string>() { "Landing 2 Ft" },
        new List<string>() { "Dim A" },
        new List<string>() { "Dim B" },
        new List<string>() { "Dim C" },
        new List<string>() { "Dim D" }
      };
    }
    if (product.ToUpper().Contains("SPANDREL") || product.ToUpper().Contains("FASCIA"))
    {
      if (!this.isMetromont)
        return new List<string>[15]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Pockets" },
          new List<string>() { "Form Faces" },
          new List<string>() { "Up Faces" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Height" }
        };
      return new List<string>[11]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Pockets" },
        new List<string>() { "Form Faces" },
        new List<string>() { "Up Faces" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Height" }
      };
    }
    if (product.ToUpper().Contains("PC"))
      return new List<string>[16 /*0x10*/]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Shipping Width" },
        new List<string>() { "Shipping Length" },
        new List<string>() { "Average Thickness" },
        new List<string>() { "Finish Width" },
        new List<string>() { "Finish Length" },
        new List<string>() { "Cold Jt Total LF" },
        new List<string>() { "Mono Jt Total LF" },
        new List<string>() { "Projection Total LF" },
        new List<string>() { "Openings" },
        new List<string>() { "Openings Width" },
        new List<string>() { "Openings Height" },
        new List<string>() { "Form Haunches" },
        new List<string>() { "Up Haunches" }
      };
    if (product.ToUpper().Contains("MW") || product.ToUpper().Contains("SWA") || product.ToUpper().Contains("WPA"))
    {
      if (!this.isMetromont)
        return new List<string>[21]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "+/- CY" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Overhead Doors Openings" },
          new List<string>() { "Overhead Doors Width" },
          new List<string>() { "Overhead Doors Height" },
          new List<string>() { "Personal Doors Openings" },
          new List<string>() { "Personal Doors Width" },
          new List<string>() { "Personal Doors Height" },
          new List<string>() { "Windows Openings" },
          new List<string>() { "Windows Width" },
          new List<string>() { "Windows Height" },
          new List<string>() { "Form Haunches" },
          new List<string>() { "Up Haunches" }
        };
      return new List<string>[17]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "+/- CY" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Overhead Doors Openings" },
        new List<string>() { "Overhead Doors Width" },
        new List<string>() { "Overhead Doors Height" },
        new List<string>() { "Personal Doors Openings" },
        new List<string>() { "Personal Doors Width" },
        new List<string>() { "Personal Doors Height" },
        new List<string>() { "Windows Openings" },
        new List<string>() { "Windows Width" },
        new List<string>() { "Windows Height" },
        new List<string>() { "Form Haunches" },
        new List<string>() { "Up Haunches" }
      };
    }
    if (product.ToUpper().Contains("WPB"))
    {
      if (!this.isMetromont)
        return new List<string>[21]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Insul Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Overhead Doors Openings" },
          new List<string>() { "Overhead Doors Width" },
          new List<string>() { "Overhead Doors Height" },
          new List<string>() { "Personal Doors Openings" },
          new List<string>() { "Personal Doors Width" },
          new List<string>() { "Personal Doors Height" },
          new List<string>() { "Windows Openings" },
          new List<string>() { "Windows Width" },
          new List<string>() { "Windows Height" },
          new List<string>() { "Form Haunches" },
          new List<string>() { "Up Haunches" }
        };
      return new List<string>[17]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Insul Length" },
        new List<string>() { "Overhead Doors Openings" },
        new List<string>() { "Overhead Doors Width" },
        new List<string>() { "Overhead Doors Height" },
        new List<string>() { "Personal Doors Openings" },
        new List<string>() { "Personal Doors Width" },
        new List<string>() { "Personal Doors Height" },
        new List<string>() { "Windows Openings" },
        new List<string>() { "Windows Width" },
        new List<string>() { "Windows Height" },
        new List<string>() { "Form Haunches" },
        new List<string>() { "Up Haunches" }
      };
    }
    if (product.ToUpper().Contains("FMD"))
    {
      if (!this.isMetromont)
        return new List<string>[14]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Insul Length" },
          new List<string>() { "Weight" },
          new List<string>() { "End Core Width" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Length" }
        };
      return new List<string>[10]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Insul Length" },
        new List<string>() { "End Core Width" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Length" }
      };
    }
    if (product.ToUpper().Contains("MDK"))
    {
      if (!this.isMetromont)
        return new List<string>[14]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Insul Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Number Cores" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Length" }
        };
      return new List<string>[10]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Insul Length" },
        new List<string>() { "Number Cores" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Length" }
      };
    }
    if (product.ToUpper().Contains("FSA"))
    {
      if (!this.isMetromont)
        return new List<string>[12]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Height" }
        };
      return new List<string>[8]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Height" }
      };
    }
    if (product.ToUpper().Contains("LGA") || product.ToUpper().Contains("RBA"))
    {
      if (!this.isMetromont)
        return new List<string>[10]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Left Pockets" },
          new List<string>() { "Right Pockets" }
        };
      return new List<string>[6]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Length" },
        new List<string>() { "Left Pockets" },
        new List<string>() { "Right Pockets" }
      };
    }
    if (product.ToUpper().Contains("TGA"))
    {
      if (!this.isMetromont)
        return new List<string>[10]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Left Pockets" },
          new List<string>() { "Right Pockets" }
        };
      return new List<string>[6]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Length" },
        new List<string>() { "Left Pockets" },
        new List<string>() { "Right Pockets" }
      };
    }
    if (product.ToUpper().Contains("DT"))
    {
      if (!this.isMetromont)
        return new List<string>[17]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Stems" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Length" },
          new List<string>() { "End 1 Depth" },
          new List<string>() { "End 1 Width" },
          new List<string>() { "End 2 Depth" },
          new List<string>() { "End 2 Width" }
        };
      return new List<string>[13]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Stems" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Length" },
        new List<string>() { "End 1 Depth" },
        new List<string>() { "End 1 Width" },
        new List<string>() { "End 2 Depth" },
        new List<string>() { "End 2 Width" }
      };
    }
    if (product.ToUpper().Contains("CLA"))
    {
      if (!this.isMetromont)
        return new List<string>[10]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Haunches" },
          new List<string>() { "Pockets" }
        };
      return new List<string>[6]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Length" },
        new List<string>() { "Haunches" },
        new List<string>() { "Pockets" }
      };
    }
    if (product.ToUpper().Contains("STF"))
    {
      if (!this.isMetromont)
        return new List<string>[12]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Height" }
        };
      return new List<string>[8]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Height" }
      };
    }
    if (product.ToUpper().Contains("STA") || product.ToUpper().Contains("STZ"))
    {
      if (!this.isMetromont)
        return new List<string>[17]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Overall Height" },
          new List<string>() { "Risers" },
          new List<string>() { "Weight" },
          new List<string>() { "Structural Thickness" },
          new List<string>() { "Landing 1 Ft" },
          new List<string>() { "Landing 2 Ft" },
          new List<string>() { "Dim A" },
          new List<string>() { "Dim B" },
          new List<string>() { "Dim C" },
          new List<string>() { "Dim D" }
        };
      return new List<string>[13]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Overall Height" },
        new List<string>() { "Risers" },
        new List<string>() { "Structural Thickness" },
        new List<string>() { "Landing 1 Ft" },
        new List<string>() { "Landing 2 Ft" },
        new List<string>() { "Dim A" },
        new List<string>() { "Dim B" },
        new List<string>() { "Dim C" },
        new List<string>() { "Dim D" }
      };
    }
    if (product.ToUpper().Contains("SPA") || product.ToUpper().Contains("FCA"))
    {
      if (!this.isMetromont)
        return new List<string>[15]
        {
          new List<string>() { "Location" },
          new List<string>() { "Mark" },
          new List<string>() { "Arch Volume" },
          new List<string>() { "Gray Volume" },
          new List<string>() { "Total Volume" },
          new List<string>() { "Pieces" },
          new List<string>() { "Width" },
          new List<string>() { "Length" },
          new List<string>() { "Weight" },
          new List<string>() { "Pockets" },
          new List<string>() { "Form Faces" },
          new List<string>() { "Up Faces" },
          new List<string>() { "Blockout Openings" },
          new List<string>() { "Blockout Width" },
          new List<string>() { "Blockout Height" }
        };
      return new List<string>[11]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Pockets" },
        new List<string>() { "Form Faces" },
        new List<string>() { "Up Faces" },
        new List<string>() { "Blockout Openings" },
        new List<string>() { "Blockout Width" },
        new List<string>() { "Blockout Height" }
      };
    }
    if (!this.isMetromont)
      return new List<string>[9]
      {
        new List<string>() { "Location" },
        new List<string>() { "Mark" },
        new List<string>() { "Arch Volume" },
        new List<string>() { "Gray Volume" },
        new List<string>() { "Total Volume" },
        new List<string>() { "Pieces" },
        new List<string>() { "Width" },
        new List<string>() { "Length" },
        new List<string>() { "Weight" }
      };
    return new List<string>[5]
    {
      new List<string>() { "Location" },
      new List<string>() { "Mark" },
      new List<string>() { "Pieces" },
      new List<string>() { "Width" },
      new List<string>() { "Length" }
    };
  }

  private string WallWeight(Element e)
  {
    Element superComponent = e.GetSuperComponent();
    return Math.Round(UnitUtils.ConvertFromInternalUnits(superComponent == null ? (e.LookupParameter("MEMBER_WEIGHT_CAST") != null ? e.LookupParameter("MEMBER_WEIGHT_CAST").AsDouble() : 0.0) : (superComponent.LookupParameter("MEMBER_WEIGHT_CAST") != null ? superComponent.LookupParameter("MEMBER_WEIGHT_CAST").AsDouble() : 0.0), UnitTypeId.PoundsForce), 2).ToString();
  }

  private string GetArchVolume(Element e)
  {
    return Math.Round(UnitUtils.ConvertFromInternalUnits(this.GetVolumes(e, false), UnitTypeId.CubicYards), 2).ToString();
  }

  private string GetGrayVolume(Element e)
  {
    return Math.Round(UnitUtils.ConvertFromInternalUnits(this.GetVolumes(e, true), UnitTypeId.CubicYards), 2).ToString();
  }

  private double GetVolumes(Element e, bool isStruct)
  {
    if (this.materialIdIndex == null)
      return 0.0;
    Dictionary<ElementId, double> volumesByMaterial = StructuralFraming.GetIndexedPieceVolumesByMaterial(e, this.allProjectAddons, this.materialIdIndex);
    double num1 = 0.0;
    double num2 = 0.0;
    if (this.mappedMaterialIndex.ContainsKey(PrecastMaterialType.Structural) && volumesByMaterial.ContainsKey(this.mappedMaterialIndex[PrecastMaterialType.Structural]))
      num1 = volumesByMaterial[this.mappedMaterialIndex[PrecastMaterialType.Structural]];
    if (this.mappedMaterialIndex.ContainsKey(PrecastMaterialType.Arch1) && volumesByMaterial.ContainsKey(this.mappedMaterialIndex[PrecastMaterialType.Arch1]))
      num2 = volumesByMaterial[this.mappedMaterialIndex[PrecastMaterialType.Arch1]];
    if (this.mappedMaterialIndex.ContainsKey(PrecastMaterialType.Arch2) && volumesByMaterial.ContainsKey(this.mappedMaterialIndex[PrecastMaterialType.Arch2]))
      num2 += volumesByMaterial[this.mappedMaterialIndex[PrecastMaterialType.Arch2]];
    if (this.mappedMaterialIndex.ContainsKey(PrecastMaterialType.Arch3) && volumesByMaterial.ContainsKey(this.mappedMaterialIndex[PrecastMaterialType.Arch3]))
      num2 += volumesByMaterial[this.mappedMaterialIndex[PrecastMaterialType.Arch3]];
    if (this.mappedMaterialIndex.ContainsKey(PrecastMaterialType.Arch4) && volumesByMaterial.ContainsKey(this.mappedMaterialIndex[PrecastMaterialType.Arch4]))
      num2 += volumesByMaterial[this.mappedMaterialIndex[PrecastMaterialType.Arch4]];
    return isStruct ? num1 : num2;
  }

  private string GetSurfaceArea(Element e)
  {
    double num1 = 0.0;
    double num2 = 0.0;
    double num3 = 0.0;
    double num4 = 0.0;
    double num5 = 0.0;
    Autodesk.Revit.DB.Parameter parameter1 = e.LookupParameter("ARCH_SF_1");
    Autodesk.Revit.DB.Parameter parameter2 = e.LookupParameter("ARCH_SF_2");
    Autodesk.Revit.DB.Parameter parameter3 = e.LookupParameter("ARCH_SF_3");
    Autodesk.Revit.DB.Parameter parameter4 = e.LookupParameter("ARCH_SF_4");
    if (parameter1 != null)
    {
      num2 = parameter1.AsDouble();
      ++num1;
    }
    if (parameter2 != null)
    {
      num3 = parameter2.AsDouble();
      ++num1;
    }
    if (parameter3 != null)
    {
      num4 = parameter3.AsDouble();
      ++num1;
    }
    if (parameter4 != null)
    {
      num5 = parameter4.AsDouble();
      ++num1;
    }
    double num6 = 0.0;
    if (num1 != 0.0)
      num6 = (num2 + num3 + num4 + num5) / num1;
    return Math.Round(num6, 2).ToString();
  }

  private string GetInsulLength(Element e)
  {
    double num = 0.0;
    foreach (ElementId subComponentId in e.GetSubComponentIds())
    {
      Element element = e.Document.GetElement(subComponentId);
      if (element != null && element.GetManufactureComponent().ToUpper().Contains("INSULATION"))
      {
        Autodesk.Revit.DB.Parameter parameter = element.LookupParameter("DIM_LENGTH");
        if (parameter != null)
        {
          num = parameter.AsDouble();
          break;
        }
      }
    }
    return Math.Round(num, 2).ToString();
  }

  private string GetInsulationLength(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("insulation_length");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(num, 2).ToString();
  }

  private string GetNumCores(Element e)
  {
    int num = 0;
    Autodesk.Revit.DB.Parameter parameter = e.Document.GetElement(e.GetTypeId()).LookupParameter("Number_Interior_Ribs");
    if (parameter != null)
      num = parameter.AsInteger() + 1;
    return num.ToString();
  }

  private string GetOverallHeight(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("DIM_HEIGHT");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(num, 2).ToString();
  }

  private string GetRiserCount(Element e)
  {
    string riserCount = "";
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("Risers");
    if (parameter != null)
      riserCount = parameter.AsValueString();
    return riserCount;
  }

  private string GetRiserHeight(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("Stair_Riser_Height");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(num, 2).ToString();
  }

  private string GetStructuralThickness(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("Stair_Throat_Dimension");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Inches), 2).ToString();
  }

  private string GetTreadOnlyLength(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("Stair_Tread_Depth");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(num, 2).ToString();
  }

  private string GetOverallStairLength(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("DIM_LENGTH");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(num, 2).ToString();
  }

  private string GetOneFootLanding(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("Landing_Lower_Length");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(num, 2).ToString();
  }

  private string GetTwoFeetLanding(Element e)
  {
    double num = 0.0;
    Autodesk.Revit.DB.Parameter parameter = e.LookupParameter("Landing_Upper_Length");
    if (parameter != null)
      num = parameter.AsDouble();
    return Math.Round(num, 2).ToString();
  }

  private string GetFullName(string constPro, Element e, bool onlyDimensions)
  {
    double num1 = Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "DIM_WIDTH");
    double num2 = Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "DIM_THICKNESS");
    double num3 = Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "DIM_DEPTH");
    double num4 = Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "DIM_HEIGHT");
    if (num2 == -1.0 && num3 > -1.0)
      num2 = num3;
    else if (num2 > -1.0 && num3 == -1.0)
      num3 = num2;
    if (num1 == -1.0 && num4 > -1.0)
      num1 = num4;
    else if (num1 > -1.0 && num4 == -1.0)
      num4 = num1;
    double num5 = Math.Round(UnitUtils.ConvertFromInternalUnits(num1, UnitTypeId.Inches), 2);
    double num6 = Math.Round(UnitUtils.ConvertFromInternalUnits(num2, UnitTypeId.Inches), 2);
    double num7 = Math.Round(UnitUtils.ConvertFromInternalUnits(num3, UnitTypeId.Inches), 2);
    double num8 = Math.Round(UnitUtils.ConvertFromInternalUnits(num4, UnitTypeId.Inches), 2);
    if (constPro.ToUpper().Contains("PRECAST"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("LGIRDER") || constPro.ToUpper().Contains("RBEAM") || constPro.ToUpper().Contains("L-GIRDER") || constPro.ToUpper().Contains("TBEAM") || constPro.ToUpper().Contains("T-BEAM") || constPro.ToUpper().Contains("R-BEAM") || constPro.ToUpper().Contains("T-GIRDER") || constPro.ToUpper().Contains("TGIRDER"))
    {
      if (num5 != -12.0 && num7 != -12.0)
      {
        if (onlyDimensions)
          return $" {num5.ToString()}x{num7.ToString()}";
        return $"{constPro} {num5.ToString()}x{num7.ToString()}";
      }
      return onlyDimensions ? "" : constPro;
    }
    if (constPro.ToUpper().Contains("COLUMN"))
    {
      if (num5 != -12.0 && num7 != -12.0)
      {
        if (onlyDimensions)
          return $" {num5.ToString()}x{num7.ToString()}";
        return $"{constPro} {num5.ToString()}x{num7.ToString()}";
      }
      return onlyDimensions ? "" : constPro;
    }
    if (constPro.ToUpper().Contains("DOUBLE") || constPro.ToUpper().Contains("DOUB"))
      return num7 != -12.0 ? (onlyDimensions ? " " + num7.ToString() : $"{constPro} {num7.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("SPANDREL") || constPro.ToUpper().Contains("FASCIA"))
    {
      if (num6 != -12.0 && num8 != -12.0)
      {
        if (onlyDimensions)
          return $" {num6.ToString()}x{num8.ToString()}";
        return $"{constPro} {num6.ToString()}x{num8.ToString()}";
      }
      return onlyDimensions ? "" : constPro;
    }
    if (constPro.ToUpper().Contains("METRODECK"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("WALL"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("SLAB"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("STAIR") && constPro.ToUpper().Contains("LANDINGS"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("STAIR") && !constPro.ToUpper().Contains("Z"))
    {
      double num9 = Math.Round(UnitUtils.ConvertFromInternalUnits(Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "Stair_Throat_Dimension"), UnitTypeId.Inches), 0);
      return onlyDimensions ? " STA " + num9.ToString() : $"{constPro} STA {num9.ToString()}";
    }
    if (constPro.ToUpper().Contains("STAIR") && constPro.ToUpper().Contains("Z"))
    {
      double num10 = Math.Round(UnitUtils.ConvertFromInternalUnits(Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "Stair_Throat_Dimension"), UnitTypeId.Inches), 0);
      return onlyDimensions ? " STZ " + num10.ToString() : $"{constPro} STZ {num10.ToString()}";
    }
    if (constPro.ToUpper().Contains("PC"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("LGA") || constPro.ToUpper().Contains("RBA") || constPro.ToUpper().Contains("TGA"))
    {
      if (num5 != -12.0 && num7 != -12.0)
      {
        if (onlyDimensions)
          return $" {num5.ToString()}x{num7.ToString()}";
        return $"{constPro} {num5.ToString()}x{num7.ToString()}";
      }
      return onlyDimensions ? "" : constPro;
    }
    if (constPro.ToUpper().Contains("CLA"))
    {
      if (num5 != -12.0 && num7 != -12.0)
      {
        if (onlyDimensions)
          return $" {num5.ToString()}x{num7.ToString()}";
        return $"{constPro} {num5.ToString()}x{num7.ToString()}";
      }
      return onlyDimensions ? "" : constPro;
    }
    if (constPro.ToUpper().Contains("DT"))
      return num7 != -12.0 ? (onlyDimensions ? " " + num7.ToString() : $"{constPro} {num7.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("SPA") || constPro.ToUpper().Contains("FCA"))
    {
      if (num6 != -12.0 && num8 != -12.0)
      {
        if (onlyDimensions)
          return $" {num6.ToString()}x{num8.ToString()}";
        return $"{constPro} {num6.ToString()}x{num8.ToString()}";
      }
      return onlyDimensions ? "" : constPro;
    }
    if (constPro.ToUpper().Contains("FMD") || constPro.ToUpper().Contains("MDK"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("SWA") || constPro.ToUpper().Contains("WPB") || constPro.ToUpper().Contains("MW") || constPro.ToUpper().Contains("WPA"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("FSA"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("STF"))
      return num6 != -12.0 ? (onlyDimensions ? " " + num6.ToString() : $"{constPro} {num6.ToString()}") : (onlyDimensions ? "" : constPro);
    if (constPro.ToUpper().Contains("STA"))
    {
      double num11 = Math.Round(UnitUtils.ConvertFromInternalUnits(Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "Stair_Throat_Dimension"), UnitTypeId.Inches), 0);
      return onlyDimensions ? " " + num11.ToString() : $"{constPro} {num11.ToString()}";
    }
    if (constPro.ToUpper().Contains("STZ"))
    {
      double num12 = Math.Round(UnitUtils.ConvertFromInternalUnits(Utils.ElementUtils.Parameters.GetParameterAsDouble(e, "Stair_Throat_Dimension"), UnitTypeId.Inches), 0);
      return onlyDimensions ? " " + num12.ToString() : $"{constPro} {num12.ToString()}";
    }
    return onlyDimensions ? "" : constPro;
  }

  public bool CheckMaterials(Element e) => e.GetMaterialIds(false).ToList<ElementId>().Count != 0;
}
