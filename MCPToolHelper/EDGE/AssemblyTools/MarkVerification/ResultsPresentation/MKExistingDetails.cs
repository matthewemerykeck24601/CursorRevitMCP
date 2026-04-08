// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.MKExistingDetails
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DocumentFormat.OpenXml.Spreadsheet;
using EDGE.AssemblyTools.MarkVerification.GeometryComparer;
using EDGE.AssemblyTools.MarkVerification.QA;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.ElementUtils;
using Utils.ExcelUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class MKExistingDetails : Window, IComponentConnector, IStyleConnector
{
  private UIDocument uiDoc;
  private UIApplication uiApp;
  private Document rDoc;
  private MKVerificationResults_Existing SourceForm;
  private MarkResult selectionForMarkResult;
  private MarkQA MarkQACopy = new MarkQA();
  private bool traditionalcheck;
  private string previousTitle;
  private string previousPath;
  private AddInId addin;
  private ExternalEvent[] highlight;
  private HighlightEvent mainEvent;
  private List<ElementId> listOfHighlighting = new List<ElementId>();
  private string childWasSelected = "";
  internal TextBlock Baseline;
  internal ListBox GroupTestResults;
  internal TextBlock NextControlMark;
  internal Button Export;
  internal Button Detail;
  internal System.Windows.Controls.Grid evenBiggerGrid;
  internal ListBox DetailResults;
  internal TextBlock ElementDisplay;
  internal ScrollViewer ScrollViewerResults;
  internal ListBox Results;
  private bool _contentLoaded;

  public MKExistingDetails(
    MarkResult selectedMarkResult,
    UIApplication uiapp,
    Document revitDoc,
    IntPtr parentWindowHandler,
    MKVerificationResults_Existing source_form,
    MarkQA MarkQAforMKEXisting,
    AddInId addIn,
    ExternalEvent[] eventforHighlight)
  {
    this.uiApp = uiapp;
    this.addin = addIn;
    this.uiDoc = uiapp.ActiveUIDocument;
    this.rDoc = revitDoc;
    this.SourceForm = source_form;
    this.InitializeComponent();
    this.MarkQACopy = new MarkQA(MarkQAforMKEXisting);
    this.previousTitle = this.uiApp.ActiveUIDocument.Document.Title;
    this.previousPath = this.uiApp.ActiveUIDocument.Document.PathName;
    if (this.MarkQACopy.traditionalApproach)
      this.traditionalcheck = true;
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    foreach (GroupTestResult groupTestResult in selectedMarkResult.GroupTestResults)
    {
      foreach (MemberDetails groupDetail in groupTestResult.GroupDetails)
      {
        int result = 0;
        int.TryParse(groupDetail.ElementId, out result);
        Element element = revitDoc.GetElement(new ElementId(result));
        if (element.HasSuperComponent())
        {
          string str = element.GetSuperComponent().Id.ToString();
          groupDetail.ElementId = str;
        }
      }
    }
    this.GroupTestResults.ItemsSource = (IEnumerable) selectedMarkResult.GroupTestResults;
    HashSet<string> stringSet = new HashSet<string>();
    List<string> list = new FilteredElementCollector(this.uiDoc.Document).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (s => !s.IsWarpableProduct() || s.IsFlatFamily())).Select<FamilyInstance, string>((Func<FamilyInstance, string>) (s => s.GetControlMark())).Distinct<string>().ToList<string>();
    foreach (string str in list)
      stringSet.Add(str);
    this.selectionForMarkResult = selectedMarkResult;
    string incrementor;
    Dictionary<string, string> settingsDictionary = MKComparisonEngine.GetUserMarkPrefixSettingsDictionary(this.uiDoc.Document, out incrementor);
    string constructionProduct = MKExistingDetails.GetMarkPrefixGivenConstructionProduct(selectedMarkResult.ConstructionProduct, settingsDictionary);
    string leadingZeroes;
    int incrementorDetails = MKComparisonEngine.GetIncrementorDetails(out leadingZeroes, incrementor);
    string str1;
    do
    {
      str1 = string.Format($"{{0}}{{1:{leadingZeroes}}}", (object) constructionProduct, (object) incrementorDetails);
      ++incrementorDetails;
    }
    while (list.Contains(str1));
    this.NextControlMark.Text = str1;
    if (this.traditionalcheck)
      this.Export.Visibility = System.Windows.Visibility.Collapsed;
    else
      this.Export.Visibility = System.Windows.Visibility.Visible;
    this.mainEvent = new HighlightEvent(this.uiDoc)
    {
      HighlightEvents = eventforHighlight[0]
    };
    this.highlight = eventforHighlight;
  }

  private static string GetMarkPrefixGivenConstructionProduct(
    string constructionProduct,
    Dictionary<string, string> _userSettingDictionary)
  {
    string constructionProduct1 = constructionProduct;
    if (_userSettingDictionary.ContainsKey(constructionProduct) && _userSettingDictionary[constructionProduct] != "")
      constructionProduct1 = _userSettingDictionary[constructionProduct];
    return constructionProduct1;
  }

  private void Results_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    IList selectedItems = ((ListBox) sender).SelectedItems;
    TestResult testResult1 = new TestResult();
    foreach (TestResult testResult2 in (IEnumerable) selectedItems)
      testResult1 = testResult2;
    if (testResult1.FailingStructuralFramingId != (ElementId) null)
    {
      Element element1 = this.uiDoc.Document.GetElement(testResult1.FailingStructuralFramingId);
      Element element2 = this.uiDoc.Document.GetElement(testResult1.StandardStructuralFramingId);
      if (element1 == null || element2 == null)
        return;
      List<ElementId> elementIdList = new List<ElementId>();
      if (element1.GetTopLevelElement().IsValidObject)
      {
        ElementId id = element1.GetTopLevelElement().Id;
        elementIdList.Add(id);
      }
      if (element2.GetTopLevelElement().IsValidObject)
      {
        ElementId id = element2.GetTopLevelElement().Id;
        elementIdList.Add(id);
      }
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
    }
    else
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
  }

  private void GroupTestResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    IList selectedItems = ((ListBox) sender).SelectedItems;
    List<ElementId> elementIdList = new List<ElementId>();
    string str = "";
    foreach (GroupTestResult groupTestResult in (IEnumerable) selectedItems)
    {
      if (groupTestResult.GroupResults.Where<TestResult>((Func<TestResult, bool>) (ed => ed.Passed)).Count<TestResult>() + groupTestResult.GroupResults.Where<TestResult>((Func<TestResult, bool>) (ed => ed.NotUsed)).Count<TestResult>() == groupTestResult.GroupResults.Count)
      {
        if (this.selectionForMarkResult.GroupTestResults.First<GroupTestResult>().GroupQuantity == groupTestResult.GroupQuantity)
          this.Baseline.Visibility = System.Windows.Visibility.Visible;
      }
      else
        this.Baseline.Visibility = System.Windows.Visibility.Collapsed;
      List<FamilyInstance> groupMembers = groupTestResult.GroupMembers;
      if (groupMembers.First<FamilyInstance>().IsValidObject)
        str = groupMembers.First<FamilyInstance>().GetTopLevelElement().Id.ToString();
      foreach (Element elem in groupMembers)
      {
        if (elem.IsValidObject)
          elementIdList.Add(elem.GetTopLevelElement().Id);
      }
    }
    if (elementIdList.Count == 0)
    {
      int num = (int) MessageBox.Show("This group does not exist in the current model anymore.", "ERROR");
    }
    else
    {
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
    }
    this.ScrollViewerResults.ScrollToTop();
    if (this.MarkQACopy.traditionalApproach)
      return;
    this.ElementDisplay.Text = "BASELINE - " + str;
  }

  private void DetailResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    IList selectedItems = ((ListBox) sender).SelectedItems;
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (MemberDetails memberDetails in (IEnumerable) selectedItems)
    {
      Element element = this.uiDoc.Document.GetElement(new ElementId(int.Parse(memberDetails.ElementId)));
      if (element != null && element.IsValidObject)
        elementIdList.Add(element.GetTopLevelElement().Id);
    }
    if (elementIdList.Count <= 0)
      return;
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }

  private void Window_Closed(object sender, EventArgs e)
  {
    this.SourceForm.DetailButton.IsEnabled = true;
  }

  private string familytype(ElementId id)
  {
    FamilyInstance element = this.uiDoc.Document.GetElement(id) as FamilyInstance;
    return $"{element.Symbol.FamilyName} - {element.Symbol.Name}";
  }

  private void Export_Click(object sender, RoutedEventArgs e)
  {
    int num1 = 0;
    try
    {
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      saveFileDialog1.AddExtension = true;
      saveFileDialog1.DefaultExt = ".xlsx";
      saveFileDialog1.Filter = "Excel Files (*.xlsx)|*.xlsx";
      SaveFileDialog saveFileDialog2 = saveFileDialog1;
      if (!saveFileDialog2.ShowDialog().GetValueOrDefault())
        return;
      string fileName = saveFileDialog2.FileName;
      List<List<object>> objectListList = new List<List<object>>();
      Dictionary<object, List<object>> source1 = new Dictionary<object, List<object>>();
      MarkResult selectionForMarkResult = this.selectionForMarkResult;
      string controlMark = selectionForMarkResult.ControlMark;
      int num2 = selectionForMarkResult.GroupTestResults.Count<GroupTestResult>();
      Dictionary<string, List<ElementId>> dictionary1 = new Dictionary<string, List<ElementId>>();
      List<object> objectList1 = new List<object>();
      List<object> objectList2 = new List<object>();
      objectList1.Add((object) "Detail Groups");
      objectList2.Add((object) "");
      for (int index = 1; index <= num2; ++index)
        objectList2.Add((object) index);
      List<object> objectList3 = new List<object>();
      List<object> objectList4 = new List<object>();
      List<object> objectList5 = new List<object>();
      List<object> objectList6 = new List<object>();
      objectList3.Add((object) "Structural Framing Elements");
      objectList4.Add((object) "");
      objectList6.Add((object) "Master Element ID");
      objectList5.Add((object) "");
      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
      {
        objectList5.Add((object) groupTestResult.GroupMembers.First<FamilyInstance>().Id);
        string str = "";
        foreach (FamilyInstance groupMember in groupTestResult.GroupMembers)
          str = !groupTestResult.GroupMembers.Last<FamilyInstance>().Equals((object) groupMember) ? $"{str}{groupMember.Id?.ToString()}; " : str + groupMember.Id?.ToString();
        objectList4.Add((object) str);
        if (groupTestResult.GroupQuantity > num1)
          num1 = groupTestResult.GroupQuantity;
      }
      selectionForMarkResult.GroupTestResults.First<GroupTestResult>().GroupResults.Count<TestResult>();
      List<TestResult> groupResults = selectionForMarkResult.GroupTestResults.First<GroupTestResult>().GroupResults;
      List<ElementId> elementIdList = new List<ElementId>();
      List<object> objectList7 = new List<object>();
      List<object> objectList8 = new List<object>();
      List<object> objectList9 = new List<object>();
      List<object> objectList10 = new List<object>();
      List<object> objectList11 = new List<object>();
      List<object> objectList12 = new List<object>();
      List<object> objectList13 = new List<object>();
      List<object> objectList14 = new List<object>();
      List<object> objectList15 = new List<object>();
      List<object> objectList16 = new List<object>();
      List<object> objectList17 = new List<object>();
      List<object> objectList18 = new List<object>();
      foreach (TestResult testResult1 in groupResults)
      {
        string testName = testResult1.TestName;
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
                    bool flag1 = true;
                    bool flag2 = true;
                    List<AddonLocation> addonLocationList1 = new List<AddonLocation>();
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName) && !groupResult.Passed && !groupResult.NotUsed)
                          {
                            using (List<AddonLocation>.Enumerator enumerator = groupResult.Locations.GetEnumerator())
                            {
                              while (enumerator.MoveNext())
                              {
                                AddonLocation current = enumerator.Current;
                                bool flag3 = false;
                                foreach (AddonLocation addonLocation in addonLocationList1)
                                {
                                  if (addonLocation.FamilyName.Equals(current.FamilyName))
                                    flag3 = true;
                                }
                                if (!flag3)
                                  addonLocationList1.Add(current);
                              }
                              break;
                            }
                          }
                        }
                      }
                    }
                    List<AddonLocation> addonLocationList2 = new List<AddonLocation>();
                    List<AddonLocation> source2 = new List<AddonLocation>();
                    foreach (AddonLocation addonLocation in addonLocationList1)
                    {
                      if (addonLocation.Addons)
                        addonLocationList2.Add(addonLocation);
                      else
                        source2.Add(addonLocation);
                    }
                    if (addonLocationList2 != null && addonLocationList2.Count > 0)
                    {
                      flag1 = false;
                      foreach (AddonLocation addonLocation1 in addonLocationList2)
                      {
                        List<object> objectList19 = new List<object>();
                        bool flag4 = false;
                        objectList19.Add((object) addonLocation1.FamilyName);
                        foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                        {
                          if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                          {
                            objectList19.Add((object) "Not Processed");
                          }
                          else
                          {
                            bool flag5 = false;
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (addonLocation1.WarningId != null)
                                {
                                  foreach (ElementId elementId in addonLocation1.WarningId.Distinct<ElementId>())
                                  {
                                    if (dictionary1.ContainsKey(addonLocation1.FamilyName))
                                    {
                                      if (!dictionary1[addonLocation1.FamilyName].Contains(elementId))
                                        dictionary1[addonLocation1.FamilyName].Add(elementId);
                                    }
                                    else
                                      dictionary1.Add(addonLocation1.FamilyName, addonLocation1.WarningId.Distinct<ElementId>().ToList<ElementId>());
                                  }
                                }
                                if (groupResult.Passed)
                                {
                                  if (!flag4)
                                  {
                                    objectList19.Add((object) "BASELINE");
                                    flag4 = true;
                                    break;
                                  }
                                  objectList19.Add((object) "PASS");
                                  break;
                                }
                                List<AddonLocation> addonLocationList3 = new List<AddonLocation>();
                                List<AddonLocation> locations = groupResult.Locations;
                                if (!groupResult.Passed && groupResult.NotUsed)
                                {
                                  objectList19.Add((object) "PASS");
                                  break;
                                }
                                foreach (AddonLocation addonLocation2 in locations)
                                {
                                  if (addonLocation1.FamilyName.Equals(addonLocation2.FamilyName))
                                  {
                                    flag5 = true;
                                    if (!groupResult.Passed)
                                    {
                                      string str = addonLocation2.ElementIds.Count<ElementId>() != 0 ? string.Join<ElementId>("; ", (IEnumerable<ElementId>) addonLocation2.ElementIds) : "Count Mismatch";
                                      objectList19.Add((object) str);
                                      break;
                                    }
                                    objectList19.Add((object) "PASS");
                                    break;
                                  }
                                }
                                if (!flag5)
                                  objectList19.Add((object) "PASS");
                              }
                            }
                          }
                        }
                        objectList14.Add((object) objectList19);
                      }
                    }
                    if (source2 != null && source2.Count<AddonLocation>() > 0)
                    {
                      flag2 = false;
                      foreach (AddonLocation addonLocation in source2)
                      {
                        List<object> objectList20 = new List<object>();
                        bool flag6 = false;
                        objectList20.Add((object) addonLocation.FamilyName);
                        foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                        {
                          if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                          {
                            objectList20.Add((object) "Not Processed");
                          }
                          else
                          {
                            bool flag7 = false;
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (addonLocation.WarningId != null)
                                {
                                  foreach (ElementId elementId in addonLocation.WarningId.Distinct<ElementId>())
                                  {
                                    if (dictionary1.ContainsKey(addonLocation.FamilyName))
                                    {
                                      if (!dictionary1[addonLocation.FamilyName].Contains(elementId))
                                        dictionary1[addonLocation.FamilyName].Add(elementId);
                                    }
                                    else
                                      dictionary1.Add(addonLocation.FamilyName, addonLocation.WarningId.Distinct<ElementId>().ToList<ElementId>());
                                  }
                                }
                                if (groupResult.Passed)
                                {
                                  if (!flag6)
                                  {
                                    objectList20.Add((object) "BASELINE");
                                    flag6 = true;
                                    break;
                                  }
                                  objectList20.Add((object) "PASS");
                                  break;
                                }
                                if (!groupResult.Passed && groupResult.NotUsed)
                                {
                                  objectList20.Add((object) "PASS");
                                  break;
                                }
                                List<AddonLocation> addonLocationList4 = new List<AddonLocation>();
                                foreach (AddonLocation location in groupResult.Locations)
                                {
                                  if (addonLocation.FamilyName.Equals(location.FamilyName))
                                  {
                                    flag7 = true;
                                    if (!groupResult.Passed)
                                    {
                                      string str = location.ElementIds.Count<ElementId>() != 0 ? string.Join<ElementId>("; ", (IEnumerable<ElementId>) location.ElementIds) : "Count Mismatch";
                                      objectList20.Add((object) str);
                                      break;
                                    }
                                    objectList20.Add((object) "PASS");
                                    break;
                                  }
                                }
                                if (!flag7)
                                  objectList20.Add((object) "PASS");
                              }
                            }
                          }
                        }
                        objectList17.Add((object) objectList20);
                      }
                    }
                    if (flag1)
                    {
                      List<object> objectList21 = new List<object>();
                      bool flag8 = false;
                      List<object> objectList22 = new List<object>();
                      objectList22.Add((object) "");
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (!groupResult.Passed && groupResult.NotUsed)
                            {
                              objectList22.Add((object) "Not Processed");
                              break;
                            }
                            objectList22.Add((object) "PASS");
                            break;
                          }
                        }
                        if (flag8)
                          break;
                      }
                      objectList14.Add((object) objectList22);
                    }
                    if (flag2)
                    {
                      List<object> objectList23 = new List<object>();
                      bool flag9 = false;
                      List<object> objectList24 = new List<object>();
                      objectList24.Add((object) "");
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (!groupResult.Passed && groupResult.NotUsed)
                            {
                              objectList24.Add((object) "Not Processed");
                              break;
                            }
                            objectList24.Add((object) "PASS");
                            break;
                          }
                        }
                        if (flag9)
                          break;
                      }
                      objectList17.Add((object) objectList24);
                      continue;
                    }
                    continue;
                  }
                  continue;
                case 'M':
                  if (testName == "Compare Member Geometry")
                  {
                    Dictionary<string, List<ElementId>> source3 = new Dictionary<string, List<ElementId>>();
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName) && !groupResult.Passed && !groupResult.NotUsed)
                          {
                            string key = groupResult.StandardStructuralFramingId.ToString();
                            if (source3.ContainsKey(key))
                            {
                              source3[key].Add(groupResult.FailingStructuralFramingId);
                              break;
                            }
                            source3.Add(key, new List<ElementId>()
                            {
                              groupResult.FailingStructuralFramingId
                            });
                            break;
                          }
                        }
                      }
                    }
                    if (source3 == null || source3.Count<KeyValuePair<string, List<ElementId>>>() == 0)
                    {
                      List<object> objectList25 = new List<object>();
                      objectList25.Add((object) "");
                      bool flag = false;
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (groupResult.Passed && !groupResult.NotUsed)
                            {
                              objectList25.Add((object) "PASS");
                              break;
                            }
                            if (!groupResult.Passed)
                            {
                              if (groupResult.NotUsed)
                              {
                                objectList25.Add((object) "Not Processed");
                                break;
                              }
                              break;
                            }
                            break;
                          }
                        }
                        if (flag)
                          break;
                      }
                      objectList18.Add((object) objectList25);
                      continue;
                    }
                    using (Dictionary<string, List<ElementId>>.Enumerator enumerator = source3.GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        KeyValuePair<string, List<ElementId>> current = enumerator.Current;
                        List<object> objectList26 = new List<object>();
                        objectList26.Add((object) "");
                        bool flag = false;
                        foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                        {
                          if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                          {
                            objectList26.Add((object) "Not Processed");
                          }
                          else
                          {
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (groupResult.Passed && groupTestResult.GroupQuantity == num1)
                                {
                                  if (!flag)
                                  {
                                    objectList26.Add((object) "BASELINE");
                                    flag = true;
                                    break;
                                  }
                                  objectList26.Add((object) "PASS");
                                  break;
                                }
                                if (!groupResult.Passed && groupResult.NotUsed)
                                {
                                  objectList26.Add((object) "PASS");
                                  break;
                                }
                                if (groupResult.StandardStructuralFramingId.ToString().Equals(current.Key))
                                {
                                  if (!groupResult.Passed)
                                  {
                                    objectList26.Add((object) "FAIL");
                                    break;
                                  }
                                  objectList26.Add((object) "PASS");
                                  break;
                                }
                                break;
                              }
                            }
                          }
                        }
                        objectList18.Add((object) objectList26);
                      }
                      continue;
                    }
                  }
                  continue;
                case 'P':
                  if (testName == "Compare Plate Locations")
                  {
                    List<AddonLocation> source4 = new List<AddonLocation>();
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName) && (groupResult.Passed && groupResult.FailedPlateRotationTest || !groupResult.Passed && !groupResult.NotUsed))
                          {
                            using (List<AddonLocation>.Enumerator enumerator = groupResult.Locations.GetEnumerator())
                            {
                              while (enumerator.MoveNext())
                              {
                                AddonLocation current = enumerator.Current;
                                bool flag = false;
                                foreach (AddonLocation addonLocation in source4)
                                {
                                  if (addonLocation.FamilyName.Equals(current.FamilyName))
                                    flag = true;
                                }
                                if (!flag)
                                  source4.Add(current);
                              }
                              break;
                            }
                          }
                        }
                      }
                    }
                    if (source4.Count<AddonLocation>() == 0)
                    {
                      List<object> objectList27 = new List<object>();
                      objectList27.Add((object) "");
                      bool flag = false;
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (groupResult.Passed && !groupResult.NotUsed)
                            {
                              objectList27.Add((object) "PASS");
                              break;
                            }
                            if (!groupResult.Passed)
                            {
                              if (groupResult.NotUsed)
                              {
                                objectList27.Add((object) "Not Processed");
                                break;
                              }
                              break;
                            }
                            break;
                          }
                        }
                        if (flag)
                          break;
                      }
                      objectList11.Add((object) objectList27);
                      continue;
                    }
                    using (List<AddonLocation>.Enumerator enumerator = source4.GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        AddonLocation current = enumerator.Current;
                        List<object> objectList28 = new List<object>();
                        objectList28.Add((object) current.FamilyName);
                        bool flag10 = false;
                        foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                        {
                          bool flag11 = false;
                          foreach (TestResult groupResult in groupTestResult.GroupResults)
                          {
                            if (groupResult.TestName.Equals(testResult1.TestName))
                            {
                              if (current.WarningId != null)
                              {
                                foreach (ElementId elementId in current.WarningId.Distinct<ElementId>())
                                {
                                  if (dictionary1.ContainsKey(current.FamilyName))
                                  {
                                    if (!dictionary1[current.FamilyName].Contains(elementId))
                                      dictionary1[current.FamilyName].Add(elementId);
                                  }
                                  else
                                    dictionary1.Add(current.FamilyName, current.WarningId.Distinct<ElementId>().ToList<ElementId>());
                                }
                              }
                              if (groupResult.Passed)
                              {
                                if (!flag10)
                                {
                                  objectList28.Add((object) "BASELINE");
                                  flag10 = true;
                                  break;
                                }
                                objectList28.Add((object) "PASS");
                                break;
                              }
                              List<AddonLocation> addonLocationList = new List<AddonLocation>();
                              List<AddonLocation> locations = groupResult.Locations;
                              if (!groupResult.Passed && groupResult.NotUsed)
                              {
                                objectList28.Add((object) "Not Processed");
                                break;
                              }
                              foreach (AddonLocation addonLocation in locations)
                              {
                                if (current.FamilyName.Equals(addonLocation.FamilyName))
                                {
                                  flag11 = true;
                                  if (!groupResult.Passed)
                                  {
                                    string str = addonLocation.ElementIds.Count<ElementId>() != 0 ? string.Join<ElementId>("; ", (IEnumerable<ElementId>) addonLocation.ElementIds) : "Count Mismatch";
                                    objectList28.Add((object) str);
                                    break;
                                  }
                                  objectList28.Add((object) "PASS");
                                  break;
                                }
                              }
                              if (!flag11)
                                objectList28.Add((object) "PASS");
                            }
                          }
                        }
                        objectList11.Add((object) objectList28);
                      }
                      continue;
                    }
                  }
                  continue;
                case 'y':
                  if (testName == "Family Types Comparison")
                  {
                    objectList7.Add((object) "");
                    bool flag12 = false;
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      TestResult testResult2 = new TestResult();
                      foreach (TestResult groupResult in groupTestResult.GroupResults)
                      {
                        if (testResult1.TestName.Equals(groupResult.TestName))
                        {
                          if (!groupResult.Passed)
                          {
                            if (!groupResult.NotUsed)
                            {
                              flag12 = true;
                              break;
                            }
                            break;
                          }
                          break;
                        }
                      }
                    }
                    bool flag13 = false;
                    using (List<GroupTestResult>.Enumerator enumerator = selectionForMarkResult.GroupTestResults.GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        GroupTestResult current = enumerator.Current;
                        string str = "";
                        foreach (TestResult groupResult in current.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (groupResult.Passed)
                            {
                              if (flag12)
                              {
                                if (current.GroupQuantity == num1)
                                {
                                  if (!flag13)
                                  {
                                    str = this.familytype(groupResult.StandardStructuralFramingId);
                                    flag13 = true;
                                  }
                                  else
                                    str = "PASS";
                                }
                                else
                                  str = this.familytype(groupResult.StandardStructuralFramingId);
                              }
                              else
                                str = "PASS";
                            }
                            else if (!groupResult.Passed)
                              str = groupResult.ActualResult;
                            objectList7.Add((object) str);
                            break;
                          }
                        }
                      }
                      continue;
                    }
                  }
                  continue;
                default:
                  continue;
              }
            case 25:
              if (testName == "Compare Family Parameters")
              {
                List<MVEEnhancedDetails> mveEnhancedDetailsList = new List<MVEEnhancedDetails>();
                foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                {
                  if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                  {
                    TestResult testResult3 = new TestResult();
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (testResult1.TestName.Equals(groupResult.TestName) && !groupResult.Passed && !groupResult.NotUsed)
                      {
                        TestResult testResult4 = groupResult;
                        mveEnhancedDetailsList.Add(testResult4.MVE);
                        break;
                      }
                    }
                  }
                }
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                foreach (MVEEnhancedDetails mveEnhancedDetails in mveEnhancedDetailsList)
                {
                  foreach (KeyValuePair<string, List<string>> parametersAndValue in mveEnhancedDetails.ParametersAndValues)
                  {
                    if (dictionary2.ContainsKey(parametersAndValue.Key))
                      dictionary2[parametersAndValue.Key] = parametersAndValue.Value.ElementAt<string>(0);
                    else
                      dictionary2.Add(parametersAndValue.Key, parametersAndValue.Value.ElementAt<string>(0));
                  }
                }
                if (dictionary2.Keys.Count<string>() == 0)
                {
                  List<object> objectList29 = new List<object>();
                  objectList29.Add((object) "");
                  bool flag = false;
                  foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                  {
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (groupResult.TestName.Equals(testResult1.TestName))
                      {
                        if (groupResult.Passed && !groupResult.NotUsed)
                        {
                          objectList29.Add((object) "PASS");
                          break;
                        }
                        if (!groupResult.Passed)
                        {
                          if (groupResult.NotUsed)
                          {
                            objectList29.Add((object) "Not Processed");
                            break;
                          }
                          break;
                        }
                        break;
                      }
                    }
                    if (flag)
                      break;
                  }
                  objectList8.Add((object) objectList29);
                  continue;
                }
                using (Dictionary<string, string>.Enumerator enumerator = dictionary2.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    KeyValuePair<string, string> current = enumerator.Current;
                    List<object> objectList30 = new List<object>();
                    bool flag = false;
                    objectList30.Add((object) current.Key);
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      foreach (TestResult groupResult in groupTestResult.GroupResults)
                      {
                        if (groupResult.TestName.Equals(testResult1.TestName))
                        {
                          if (groupResult.Passed && groupTestResult.GroupQuantity == num1)
                          {
                            if (!flag)
                            {
                              objectList30.Add((object) current.Value);
                              flag = true;
                              break;
                            }
                            objectList30.Add((object) current.Value);
                            break;
                          }
                          if (!groupResult.Passed && groupResult.NotUsed)
                          {
                            objectList30.Add((object) "Not Processed");
                            break;
                          }
                          if (!groupResult.Passed && groupResult.FailingStructuralFramingId.Equals((object) groupResult.MVE.IdOfElement))
                          {
                            if (groupResult.MVE.ParametersAndValues.ContainsKey(current.Key))
                            {
                              objectList30.Add((object) groupResult.MVE.ParametersAndValues[current.Key].ElementAt<string>(1));
                              break;
                            }
                            objectList30.Add((object) current.Value);
                            break;
                          }
                          objectList30.Add((object) current.Value);
                          break;
                        }
                      }
                    }
                    objectList8.Add((object) objectList30);
                  }
                  continue;
                }
              }
              continue;
            case 29:
              if (testName == "Compare Main Material Volumes")
              {
                List<CompareVolume> compareVolumeList = new List<CompareVolume>();
                foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                {
                  if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                  {
                    TestResult testResult5 = new TestResult();
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (testResult1.TestName.Equals(groupResult.TestName) && !groupResult.Passed && !groupResult.NotUsed)
                      {
                        TestResult testResult6 = groupResult;
                        compareVolumeList.Add(testResult6.CV);
                        break;
                      }
                    }
                  }
                }
                Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
                foreach (CompareVolume compareVolume in compareVolumeList)
                {
                  foreach (KeyValuePair<string, List<string>> parametersAndValue in compareVolume.ParametersAndValues)
                  {
                    if (dictionary3.ContainsKey(parametersAndValue.Key))
                      dictionary3[parametersAndValue.Key] = parametersAndValue.Value.ElementAt<string>(0);
                    else
                      dictionary3.Add(parametersAndValue.Key, parametersAndValue.Value.ElementAt<string>(0));
                  }
                }
                if (dictionary3.Keys.Count == 0)
                {
                  List<object> objectList31 = new List<object>();
                  objectList31.Add((object) "");
                  bool flag = false;
                  foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                  {
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (groupResult.TestName.Equals(testResult1.TestName))
                      {
                        if (groupResult.Passed && !groupResult.NotUsed)
                        {
                          objectList31.Add((object) "PASS");
                          break;
                        }
                        if (!groupResult.Passed)
                        {
                          if (groupResult.NotUsed)
                          {
                            objectList31.Add((object) "Not Processed");
                            break;
                          }
                          break;
                        }
                        break;
                      }
                    }
                    if (flag)
                      break;
                  }
                  objectList9.Add((object) objectList31);
                  continue;
                }
                using (Dictionary<string, string>.Enumerator enumerator = dictionary3.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    KeyValuePair<string, string> current = enumerator.Current;
                    List<object> objectList32 = new List<object>();
                    objectList32.Add((object) current.Key);
                    bool flag = false;
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      foreach (TestResult groupResult in groupTestResult.GroupResults)
                      {
                        if (groupResult.TestName.Equals(testResult1.TestName))
                        {
                          if (groupResult.Passed && groupTestResult.GroupQuantity == num1)
                          {
                            if (!flag)
                            {
                              objectList32.Add((object) current.Value);
                              flag = true;
                              break;
                            }
                            objectList32.Add((object) current.Value);
                            break;
                          }
                          if (!groupResult.Passed && groupResult.NotUsed)
                            objectList32.Add((object) "Not Processed");
                          else if (!groupResult.Passed && groupResult.FailingStructuralFramingId.Equals((object) groupResult.CV.IdOfElement))
                          {
                            if (groupResult.CV.ParametersAndValues.ContainsKey(current.Key))
                              objectList32.Add((object) groupResult.CV.ParametersAndValues[current.Key].ElementAt<string>(1));
                          }
                          else
                            objectList32.Add((object) current.Value);
                        }
                      }
                    }
                    objectList9.Add((object) objectList32);
                  }
                  continue;
                }
              }
              continue;
            case 37:
              if (testName == "Compare Plate Family Types and Counts")
              {
                List<PlateTypes> plateTypesList1 = new List<PlateTypes>();
                foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                {
                  if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                  {
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (testResult1.TestName.Equals(groupResult.TestName) && !groupResult.Passed && !groupResult.NotUsed)
                      {
                        using (List<PlateTypes>.Enumerator enumerator = groupResult.PlateTypes.GetEnumerator())
                        {
                          while (enumerator.MoveNext())
                          {
                            PlateTypes current = enumerator.Current;
                            bool flag = false;
                            foreach (PlateTypes plateTypes in plateTypesList1)
                            {
                              if (plateTypes.FamilyName.Equals(current.FamilyName))
                                flag = true;
                            }
                            if (!flag)
                              plateTypesList1.Add(current);
                          }
                          break;
                        }
                      }
                    }
                  }
                }
                if (plateTypesList1.Count == 0)
                {
                  List<object> objectList33 = new List<object>();
                  objectList33.Add((object) "");
                  bool flag = false;
                  foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                  {
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (groupResult.TestName.Equals(testResult1.TestName))
                      {
                        if (groupResult.Passed && !groupResult.NotUsed)
                        {
                          objectList33.Add((object) "PASS");
                          break;
                        }
                        if (!groupResult.Passed)
                        {
                          if (groupResult.NotUsed)
                          {
                            objectList33.Add((object) "Not Processed");
                            break;
                          }
                          break;
                        }
                        break;
                      }
                    }
                    if (flag)
                      break;
                  }
                  objectList10.Add((object) objectList33);
                  continue;
                }
                using (List<PlateTypes>.Enumerator enumerator = plateTypesList1.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    PlateTypes current = enumerator.Current;
                    bool flag14 = false;
                    List<object> objectList34 = new List<object>();
                    objectList34.Add((object) current.FamilyName);
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                      {
                        objectList34.Add((object) "Not Processed");
                      }
                      else
                      {
                        bool flag15 = false;
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (groupResult.Passed)
                            {
                              if (!flag14)
                              {
                                objectList34.Add((object) current.FamilyCount);
                                flag14 = true;
                                break;
                              }
                              objectList34.Add((object) current.FamilyCount);
                              break;
                            }
                            if (!groupResult.Passed && groupResult.NotUsed)
                            {
                              objectList34.Add((object) "Not Processed");
                              break;
                            }
                            List<PlateTypes> plateTypesList2 = new List<PlateTypes>();
                            List<PlateTypes> plateTypes1 = groupResult.PlateTypes;
                            if (groupResult.Passed && !groupResult.NotUsed && current.FamilyCount == 0)
                            {
                              objectList34.Add((object) 0);
                              break;
                            }
                            foreach (PlateTypes plateTypes2 in plateTypes1)
                            {
                              if (current.FamilyName.Equals(plateTypes2.FamilyName))
                              {
                                flag15 = true;
                                if (!groupResult.Passed)
                                {
                                  objectList34.Add((object) plateTypes2.ActualCount);
                                  break;
                                }
                                objectList34.Add((object) current.FamilyCount);
                                break;
                              }
                            }
                            if (!flag15)
                            {
                              objectList34.Add((object) current.FamilyCount);
                              break;
                            }
                            break;
                          }
                        }
                      }
                    }
                    objectList10.Add((object) objectList34);
                  }
                  continue;
                }
              }
              continue;
            case 48 /*0x30*/:
              if (testName == "Addon Family Types, Counts, and Material Volumes")
              {
                List<dataForAddonTest> dataForAddonTestList1 = new List<dataForAddonTest>();
                foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                {
                  if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                  {
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (groupResult.TestName.Equals(testResult1.TestName) && !groupResult.Passed && !groupResult.NotUsed)
                      {
                        using (List<dataForAddonTest>.Enumerator enumerator = groupResult.DataForAddOn.GetEnumerator())
                        {
                          while (enumerator.MoveNext())
                          {
                            dataForAddonTest current = enumerator.Current;
                            bool flag = false;
                            foreach (dataForAddonTest dataForAddonTest in dataForAddonTestList1)
                            {
                              if (dataForAddonTest.FamilyTypeName.Equals(current.FamilyTypeName))
                                flag = true;
                            }
                            if (!flag)
                              dataForAddonTestList1.Add(current);
                          }
                          break;
                        }
                      }
                    }
                  }
                }
                List<object> objectList35 = new List<object>();
                List<object> objectList36 = new List<object>();
                List<dataForAddonTest> dataForAddonTestList2 = new List<dataForAddonTest>();
                List<dataForAddonTest> dataForAddonTestList3 = new List<dataForAddonTest>();
                foreach (dataForAddonTest dataForAddonTest in dataForAddonTestList1)
                {
                  if (dataForAddonTest.AddonPass)
                    dataForAddonTestList2.Add(dataForAddonTest);
                  else
                    dataForAddonTestList3.Add(dataForAddonTest);
                }
                if (dataForAddonTestList2.Count > 0)
                {
                  foreach (dataForAddonTest dataForAddonTest1 in dataForAddonTestList2)
                  {
                    if (!dataForAddonTest1.VolumePass && dataForAddonTest1.ActualCount == dataForAddonTest1.FamilyTypeCount)
                    {
                      List<object> objectList37 = new List<object>();
                      objectList37.Add((object) "");
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            objectList37.Add((object) "PASS");
                            break;
                          }
                        }
                      }
                      objectList12.Add((object) objectList37);
                    }
                    else
                    {
                      string str = dataForAddonTest1.FamilyTypeName;
                      List<object> objectList38 = new List<object>();
                      if (str.Equals("No Matches Found"))
                        str = dataForAddonTest1.Message;
                      objectList38.Add((object) str);
                      bool flag16 = false;
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        bool flag17 = false;
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            bool flag18;
                            if (groupResult.Passed && groupTestResult.GroupQuantity == num1)
                            {
                              if (!flag16)
                              {
                                objectList38.Add((object) dataForAddonTest1.FamilyTypeCount);
                                flag16 = true;
                              }
                              else
                                objectList38.Add((object) dataForAddonTest1.FamilyTypeCount);
                              flag18 = true;
                              break;
                            }
                            List<dataForAddonTest> dataForAddonTestList4 = new List<dataForAddonTest>();
                            List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                            if (!groupResult.Passed && groupResult.NotUsed)
                            {
                              flag18 = true;
                              objectList38.Add((object) "Not Processed");
                              break;
                            }
                            if (groupResult.Passed && !groupResult.NotUsed && dataForAddonTest1.FamilyTypeCount == 0)
                            {
                              objectList38.Add((object) 0);
                              break;
                            }
                            foreach (dataForAddonTest dataForAddonTest2 in dataForAddOn)
                            {
                              if (dataForAddonTest2.AddonPass && dataForAddonTest1.FamilyTypeName.Equals(dataForAddonTest2.FamilyTypeName) && !groupResult.Passed)
                              {
                                flag17 = true;
                                objectList38.Add((object) dataForAddonTest2.ActualCount);
                                break;
                              }
                            }
                            if (!flag17)
                            {
                              objectList38.Add((object) dataForAddonTest1.FamilyTypeCount);
                              break;
                            }
                            break;
                          }
                        }
                      }
                      objectList12.Add((object) objectList38);
                    }
                  }
                  foreach (dataForAddonTest dataForAddonTest3 in dataForAddonTestList2)
                  {
                    string str1 = dataForAddonTest3.FamilyTypeName;
                    List<object> objectList39 = new List<object>();
                    bool flag19 = false;
                    if (str1.Equals("No Matches Found"))
                      str1 = dataForAddonTest3.Message;
                    objectList39.Add((object) str1);
                    foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                    {
                      bool flag20 = false;
                      foreach (TestResult groupResult in groupTestResult.GroupResults)
                      {
                        if (groupResult.TestName.Equals(testResult1.TestName))
                        {
                          if (groupResult.Passed && groupTestResult.GroupQuantity == num1)
                          {
                            if (!flag19)
                            {
                              objectList39.Add((object) "BASELINE");
                              flag19 = true;
                              break;
                            }
                            objectList39.Add((object) "PASS");
                            break;
                          }
                          List<dataForAddonTest> dataForAddonTestList5 = new List<dataForAddonTest>();
                          List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                          if (!groupResult.Passed && groupResult.NotUsed)
                          {
                            objectList39.Add((object) "Not Processed");
                            break;
                          }
                          foreach (dataForAddonTest dataForAddonTest4 in dataForAddOn)
                          {
                            if (dataForAddonTest4.AddonPass && dataForAddonTest3.FamilyTypeName.Equals(dataForAddonTest4.FamilyTypeName))
                            {
                              string str2 = "";
                              flag20 = true;
                              if (!groupResult.Passed)
                              {
                                if (dataForAddonTest4.MismatchedId.Count<ElementId>() == 0 && dataForAddonTest4.ListforActual.Count<ElementId>() == 0)
                                  str2 = "No Match";
                                else if (dataForAddonTest4.MismatchedId.Count<ElementId>() > 0)
                                  str2 = string.Join<ElementId>("; ", (IEnumerable<ElementId>) dataForAddonTest4.MismatchedId);
                                else if (dataForAddonTest4.FamilyTypeCount != dataForAddonTest4.ActualCount)
                                {
                                  if (dataForAddonTest4.VolumePass)
                                    str2 = "Count Mismatch";
                                  else if (dataForAddonTest4.ListforExpected.Count<ElementId>() == 0)
                                  {
                                    str2 = "Count Mismatch";
                                  }
                                  else
                                  {
                                    str2 += "Count Mismatch: ";
                                    foreach (ElementId elementId in dataForAddonTest4.ListforExpected)
                                      str2 = !dataForAddonTest4.ListforExpected.Last<ElementId>().Equals((object) elementId) ? $"{str2}{elementId.ToString()}; " : str2 + elementId.ToString();
                                  }
                                }
                                objectList39.Add((object) str2);
                                break;
                              }
                              objectList39.Add((object) "PASS");
                              break;
                            }
                          }
                          if (!flag20)
                          {
                            objectList39.Add((object) "PASS");
                            break;
                          }
                          break;
                        }
                      }
                    }
                    objectList13.Add((object) objectList39);
                  }
                }
                else
                {
                  List<object> objectList40 = new List<object>();
                  objectList40.Add((object) "");
                  bool flag = false;
                  List<object> objectList41 = new List<object>();
                  objectList41.Add((object) "");
                  foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                  {
                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                    {
                      if (groupResult.TestName.Equals(testResult1.TestName))
                      {
                        if (!groupResult.Passed && groupResult.NotUsed)
                        {
                          objectList40.Add((object) "Not Processed");
                          objectList41.Add((object) "Not Processed");
                          break;
                        }
                        objectList40.Add((object) "PASS");
                        objectList41.Add((object) "PASS");
                        break;
                      }
                    }
                    if (flag)
                      break;
                  }
                  objectList12.Add((object) objectList40);
                  objectList13.Add((object) objectList41);
                }
                if (dataForAddonTestList3.Count > 0)
                {
                  foreach (dataForAddonTest dataForAddonTest5 in dataForAddonTestList3)
                  {
                    if (!dataForAddonTest5.VolumePass && dataForAddonTest5.ActualCount == dataForAddonTest5.FamilyTypeCount)
                    {
                      List<object> objectList42 = new List<object>();
                      objectList42.Add((object) "");
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            objectList42.Add((object) "PASS");
                            break;
                          }
                        }
                      }
                      objectList15.Add((object) objectList42);
                    }
                    else
                    {
                      string str = dataForAddonTest5.FamilyTypeName;
                      List<object> objectList43 = new List<object>();
                      bool flag21 = false;
                      if (str.Equals("No Matches Found"))
                        str = dataForAddonTest5.Message;
                      objectList43.Add((object) str);
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        bool flag22 = false;
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (groupResult.Passed && groupTestResult.GroupQuantity == num1)
                            {
                              if (!flag21)
                              {
                                objectList43.Add((object) dataForAddonTest5.FamilyTypeCount);
                                flag21 = true;
                                break;
                              }
                              objectList43.Add((object) dataForAddonTest5.FamilyTypeCount);
                              break;
                            }
                            List<dataForAddonTest> dataForAddonTestList6 = new List<dataForAddonTest>();
                            List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                            if (!groupResult.Passed && groupResult.NotUsed)
                            {
                              objectList43.Add((object) "Not Processed");
                              break;
                            }
                            foreach (dataForAddonTest dataForAddonTest6 in dataForAddOn)
                            {
                              if (!dataForAddonTest6.AddonPass && dataForAddonTest5.FamilyTypeName.Equals(dataForAddonTest6.FamilyTypeName) && !groupResult.Passed)
                              {
                                objectList43.Add((object) dataForAddonTest6.ActualCount);
                                flag22 = true;
                                break;
                              }
                            }
                            if (!flag22)
                            {
                              objectList43.Add((object) dataForAddonTest5.FamilyTypeCount);
                              break;
                            }
                            break;
                          }
                        }
                      }
                      objectList15.Add((object) objectList43);
                    }
                  }
                  using (List<dataForAddonTest>.Enumerator enumerator = dataForAddonTestList3.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      dataForAddonTest current = enumerator.Current;
                      string str3 = current.FamilyTypeName;
                      bool flag23 = false;
                      List<object> objectList44 = new List<object>();
                      if (str3.Equals("No Matches Found"))
                        str3 = current.Message;
                      objectList44.Add((object) str3);
                      foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                      {
                        bool flag24 = false;
                        foreach (TestResult groupResult in groupTestResult.GroupResults)
                        {
                          if (groupResult.TestName.Equals(testResult1.TestName))
                          {
                            if (groupResult.Passed && groupTestResult.GroupQuantity == num1)
                            {
                              if (!flag23)
                              {
                                objectList44.Add((object) "BASELINE");
                                flag23 = true;
                                break;
                              }
                              objectList44.Add((object) "PASS");
                              break;
                            }
                            List<dataForAddonTest> dataForAddonTestList7 = new List<dataForAddonTest>();
                            List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                            if (!groupResult.Passed && groupResult.NotUsed)
                            {
                              objectList44.Add((object) "Not Processed");
                              break;
                            }
                            foreach (dataForAddonTest dataForAddonTest in dataForAddOn)
                            {
                              if (!dataForAddonTest.AddonPass && current.FamilyTypeName.Equals(dataForAddonTest.FamilyTypeName))
                              {
                                string str4 = "";
                                flag24 = true;
                                if (!groupResult.Passed)
                                {
                                  if (dataForAddonTest.MismatchedId.Count<ElementId>() == 0 && dataForAddonTest.ListforActual.Count<ElementId>() == 0)
                                    str4 = "No Match";
                                  else if (dataForAddonTest.MismatchedId.Count<ElementId>() > 0)
                                    str4 = string.Join<ElementId>("; ", (IEnumerable<ElementId>) dataForAddonTest.MismatchedId);
                                  else if (dataForAddonTest.FamilyTypeCount != dataForAddonTest.ActualCount)
                                  {
                                    if (dataForAddonTest.VolumePass)
                                    {
                                      str4 = "Count Mismatch";
                                    }
                                    else
                                    {
                                      str4 += "Count Mismatch: ";
                                      foreach (ElementId elementId in dataForAddonTest.ListforExpected)
                                        str4 = !dataForAddonTest.ListforExpected.Last<ElementId>().Equals((object) elementId) ? $"{str4}{elementId.ToString()}; " : str4 + elementId.ToString();
                                    }
                                  }
                                  objectList44.Add((object) str4);
                                  break;
                                }
                                objectList44.Add((object) "PASS");
                                break;
                              }
                            }
                            if (!flag24)
                            {
                              objectList44.Add((object) "PASS");
                              break;
                            }
                            break;
                          }
                        }
                      }
                      objectList16.Add((object) objectList44);
                    }
                    continue;
                  }
                }
                List<object> objectList45 = new List<object>();
                objectList45.Add((object) "");
                bool flag25 = false;
                List<object> objectList46 = new List<object>();
                objectList46.Add((object) "");
                foreach (GroupTestResult groupTestResult in selectionForMarkResult.GroupTestResults)
                {
                  foreach (TestResult groupResult in groupTestResult.GroupResults)
                  {
                    if (groupResult.TestName.Equals(testResult1.TestName))
                    {
                      if (!groupResult.Passed && groupResult.NotUsed)
                      {
                        objectList45.Add((object) "Not Processed");
                        objectList46.Add((object) "Not Processed");
                        break;
                      }
                      objectList45.Add((object) "PASS");
                      objectList46.Add((object) "PASS");
                      break;
                    }
                  }
                  if (flag25)
                    break;
                }
                objectList15.Add((object) objectList45);
                objectList16.Add((object) objectList46);
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      source1.Add((object) "Detail Groups", new List<object>()
      {
        (object) objectList2
      });
      source1.Add((object) "Master Element ID", new List<object>()
      {
        (object) objectList5
      });
      source1.Add((object) "Structural Framing Elements", new List<object>()
      {
        (object) objectList4
      });
      source1.Add((object) "Family Types Comparison", new List<object>()
      {
        (object) objectList7
      });
      source1.Add((object) "Compare Family Parameters", objectList8);
      source1.Add((object) "Compare Main Material Volumes", objectList9);
      source1.Add((object) "Compare Plate Family Types and Counts", objectList10);
      source1.Add((object) "Plate Location Comparison (results provides Element ID for offending items)", objectList11);
      source1.Add((object) "Addon Family Count Comparison", objectList12);
      source1.Add((object) "Addon Volume Comparison", objectList13);
      source1.Add((object) "Addon Location Comparison", objectList14);
      source1.Add((object) "Finish Family Count Comparison", objectList15);
      source1.Add((object) "Finish Volume Comparison", objectList16);
      source1.Add((object) "Finish Location Comparison", objectList17);
      source1.Add((object) "Geometry Comparison", objectList18);
      if (selectionForMarkResult.FailedRotationList != null && selectionForMarkResult.FailedRotationList.Count<Plates>() > 0)
      {
        foreach (Plates failedRotation in selectionForMarkResult.FailedRotationList)
        {
          List<object> objectList47 = new List<object>();
          objectList47.Add((object) failedRotation.Names);
          string str = "";
          foreach (ElementId id in failedRotation.Ids)
            str = !failedRotation.Ids.Last<ElementId>().Equals((object) id) ? $"{str}{id.ToString()}; " : str + id.ToString();
          objectList47.Add((object) str);
          if (source1.ContainsKey((object) "Rotation Warnings (Warnings are per control mark not detail group. Element ID's for each family type with a warning are listed below)"))
            source1[(object) "Rotation Warnings (Warnings are per control mark not detail group. Element ID's for each family type with a warning are listed below)"].Add((object) objectList47);
          else
            source1.Add((object) "Rotation Warnings (Warnings are per control mark not detail group. Element ID's for each family type with a warning are listed below)", new List<object>()
            {
              (object) objectList47
            });
        }
      }
      List<int> intList = new List<int>();
      int num3 = (source1.Values.First<List<object>>().First<object>() as List<object>).Count<object>();
      int length1 = 0;
      foreach (object key in source1.Keys)
        length1 += source1[key].Count + 1;
      int length2 = num3 + 1;
      object[,] objArray = new object[length1, length2];
      int index1 = 0;
      for (int index2 = 0; index2 < source1.Keys.Count; ++index2)
      {
        objArray[index1, 0] = source1.Keys.ElementAt<object>(index2);
        intList.Add(index1 + 1);
        KeyValuePair<object, List<object>> keyValuePair = source1.ElementAt<KeyValuePair<object, List<object>>>(index2);
        int num4 = keyValuePair.Value.Count<object>();
        ++index1;
        for (int index3 = 0; index3 < num4; ++index3)
        {
          keyValuePair = source1.ElementAt<KeyValuePair<object, List<object>>>(index2);
          List<object> source5 = keyValuePair.Value.ElementAt<object>(index3) as List<object>;
          for (int index4 = 0; index4 < source5.Count; ++index4)
            objArray[index1, index4] = source5.ElementAt<object>(index4);
          ++index1;
        }
      }
      ExcelDocument excelDocument = new ExcelDocument(fileName);
      if (excelDocument != null)
      {
        ExcelFont newExcelFont = new ExcelFont();
        newExcelFont.bold = true;
        ExcelFill newExcelFill = new ExcelFill();
        newExcelFill.Pattern = PatternValues.Solid;
        newExcelFill.SetBackgroundColor(211, 211, 211);
        string sheetName = this.removeSpecialCharacters(controlMark);
        if (!string.IsNullOrEmpty(sheetName))
          excelDocument.GetSheet(sheetName);
        for (int index5 = 0; index5 < objArray.GetLength(0); ++index5)
        {
          for (int index6 = 0; index6 < objArray.GetLength(1); ++index6)
          {
            object obj = objArray[index5, index6];
            switch (obj)
            {
              case string str5:
                if (!string.IsNullOrWhiteSpace(str5))
                {
                  excelDocument.UpdateCellValue(index6 + 1, index5 + 1, str5, ExcelEnums.ExcelCellFormat.General);
                  excelDocument.UpdateCellAlignment(index6 + 1, index5 + 1, HorizontalAlignmentValues.Center);
                  break;
                }
                break;
              case int num5:
                excelDocument.UpdateCellValue(index6 + 1, index5 + 1, (double) num5, ExcelEnums.ExcelCellFormat.General);
                excelDocument.UpdateCellAlignment(index6 + 1, index5 + 1, HorizontalAlignmentValues.Center);
                break;
              default:
                ElementId elementId = obj as ElementId;
                if ((object) elementId != null)
                {
                  string str = elementId.IntegerValue.ToString();
                  excelDocument.UpdateCellValue(index6 + 1, index5 + 1, str, ExcelEnums.ExcelCellFormat.General);
                  excelDocument.UpdateCellAlignment(index6 + 1, index5 + 1, HorizontalAlignmentValues.Center);
                  break;
                }
                break;
            }
          }
        }
        foreach (int RowId in intList)
        {
          excelDocument.UpdateRowFont(RowId, newExcelFont);
          excelDocument.UpdateRowFill(RowId, newExcelFill);
          string columnId1 = excelDocument.getColumnId(1);
          if (!string.IsNullOrWhiteSpace(columnId1))
          {
            string columnId2 = excelDocument.getColumnId(length2 - 1);
            if (!string.IsNullOrEmpty(columnId2))
            {
              string firstCellReference = columnId1 + RowId.ToString();
              string secondCellReference = columnId2 + RowId.ToString();
              excelDocument.MergeCells(firstCellReference, secondCellReference);
            }
            else
              break;
          }
          else
            break;
        }
        excelDocument.UpdateAllColumnsWidthBestFit();
        excelDocument.SaveAndClose();
      }
      TaskDialog.Show("Message", $"The file has been exported to {fileName}.");
    }
    catch (Exception ex)
    {
      if (ex == null)
        return;
      TaskDialog.Show("Warning", ex.ToString());
      throw;
    }
  }

  private string removeSpecialCharacters(string value)
  {
    string str1 = "";
    for (int index = 0; index < value.Length; ++index)
    {
      string str2 = value[index].ToString().Replace("[", "").Replace("]", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace("?", "").Replace("*", "");
      str1 += str2;
    }
    if (string.IsNullOrEmpty(str1))
      return "";
    return str1.Length > 31 /*0x1F*/ ? str1.Substring(0, 30) : str1;
  }

  private void Detail_Click(object sender, RoutedEventArgs e)
  {
    List<FamilyInstance> source1 = new List<FamilyInstance>();
    foreach (GroupTestResult groupTestResult in this.selectionForMarkResult.GroupTestResults)
    {
      foreach (FamilyInstance groupMember in groupTestResult.GroupMembers)
        source1.Add(groupMember);
    }
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    MarkQA markQa1 = new MarkQA();
    CompareEngine_Selection_Window engineSelectionWindow = new CompareEngine_Selection_Window(mainWindowHandle, "existing");
    engineSelectionWindow.traditionalBox.IsEnabled = false;
    engineSelectionWindow.ShowDialog();
    if (!engineSelectionWindow.isContinue)
      return;
    markQa1.bFamilyTypeTest = true;
    markQa1.bCompareAllParameters = engineSelectionWindow.familyParametercheckBox.IsChecked.Value;
    MarkQA markQa2 = markQa1;
    bool? isChecked = engineSelectionWindow.mainMaterialVolumncheckBox.IsChecked;
    int num1 = isChecked.Value ? 1 : 0;
    markQa2.bCompareMaterialVolumes = num1 != 0;
    MarkQA markQa3 = markQa1;
    isChecked = engineSelectionWindow.addonFamilycheckBox.IsChecked;
    int num2 = isChecked.Value ? 1 : 0;
    markQa3.bCompareAddons_VolMatCountFamily = num2 != 0;
    MarkQA markQa4 = markQa1;
    isChecked = engineSelectionWindow.memberGeometry_AddonLocation_checkBox.IsChecked;
    int num3 = isChecked.Value ? 1 : 0;
    markQa4.bCompareAddons_LocationAndOrientation = num3 != 0;
    MarkQA markQa5 = markQa1;
    isChecked = engineSelectionWindow.plateFamilycheckBox.IsChecked;
    int num4 = isChecked.Value ? 1 : 0;
    markQa5.bComparePlates_NamesAndCounts = num4 != 0;
    MarkQA markQa6 = markQa1;
    isChecked = engineSelectionWindow.memberGeometry_EmbededLocation_checkBox.IsChecked;
    int num5 = isChecked.Value ? 1 : 0;
    markQa6.bComparePlate_LocationAndOrientation = num5 != 0;
    MarkQA markQa7 = markQa1;
    isChecked = engineSelectionWindow.memberGeometry_Solid_checkBox.IsChecked;
    int num6 = isChecked.Value ? 1 : 0;
    markQa7.bCompareSolidsFaces = num6 != 0;
    MarkQA markQa8 = markQa1;
    isChecked = engineSelectionWindow.traditionalBox.IsChecked;
    int num7 = isChecked.Value ? 1 : 0;
    markQa8.traditionalApproach = num7 != 0;
    mkExistingWarning mkExistingWarning = new mkExistingWarning(mainWindowHandle);
    mkExistingWarning.ShowDialog();
    if (!mkExistingWarning.ifContinue)
      return;
    List<MarkResult> source2 = MKComparisonEngine.RunMarkVerifictionOnExisting(this.uiDoc, source1.GroupBy<FamilyInstance, string>((Func<FamilyInstance, string>) (s => Utils.ElementUtils.Parameters.GetParameterAsString((Element) s, "CONTROL_MARK"))), markQa1);
    if (source2 == null)
      return;
    source2?.Sort((Comparison<MarkResult>) ((p, q) => p.CompareTo((object) q)));
    this.Detail.IsEnabled = false;
    this.Detail.Visibility = System.Windows.Visibility.Hidden;
    this.Export.IsEnabled = true;
    this.Export.Visibility = System.Windows.Visibility.Visible;
    MarkResult selectedMarkResult = source2.First<MarkResult>();
    selectedMarkResult.GroupTestResults = selectedMarkResult.GroupTestResults.OrderByDescending<GroupTestResult, int>((Func<GroupTestResult, int>) (ef => ef.GroupQuantity)).ToList<GroupTestResult>();
    MKExistingDetails mkExistingDetails = new MKExistingDetails(selectedMarkResult, this.uiApp, this.rDoc, mainWindowHandle, this.SourceForm, markQa1, this.uiDoc.Application.ActiveAddInId, this.highlight);
    mkExistingDetails.Show();
    this.Close();
    this.SourceForm.DetailButton.IsEnabled = false;
    this.SourceForm.DetailButton.IsHitTestVisible = false;
    mkExistingDetails.Closed += new EventHandler(this.closed);
  }

  private void closed(object sender, EventArgs e)
  {
    this.SourceForm.DetailButton.IsEnabled = true;
    this.SourceForm.DetailButton.IsHitTestVisible = true;
  }

  private void expand(TreeViewItem tvi, TestResult tr)
  {
    if (this.traditionalcheck)
      return;
    tvi.Items.Clear();
    if (tr.Passed)
      return;
    TreeViewItem treeViewItem = new TreeViewItem();
    if (!tr.NotUsed)
    {
      string str1 = "";
      if (tr.TestName.Equals("Family Types Comparison"))
      {
        TreeViewItem newItem1 = new TreeViewItem();
        TreeViewItem newItem2 = new TreeViewItem();
        newItem2.Header = (object) ("Actual Result: " + tr.ActualResult);
        newItem2.Tag = (object) tr.FailingStructuralFramingId;
        newItem2.MouseUp += new MouseButtonEventHandler(this.selection);
        newItem1.Header = (object) ("Expected Result: " + tr.Expectedfailingreason);
        newItem1.Tag = (object) tr.StandardStructuralFramingId;
        newItem1.MouseUp += new MouseButtonEventHandler(this.selection);
        newItem2.Focusable = true;
        newItem1.Focusable = true;
        tvi.Items.Add((object) newItem1);
        tvi.Items.Add((object) newItem2);
      }
      else if (tr.TestName.Equals("Compare Family Parameters"))
      {
        foreach (string key in tr.MVE.ParametersAndValues.Keys)
        {
          TreeViewItem newItem3 = new TreeViewItem();
          TreeViewItem newItem4 = new TreeViewItem();
          TreeViewItem newItem5 = new TreeViewItem();
          newItem3.Header = (object) key;
          newItem4.Header = (object) ("Expected Result: " + tr.MVE.ParametersAndValues[key].ElementAt<string>(0));
          newItem5.Header = (object) ("Actual Result: " + tr.MVE.ParametersAndValues[key].ElementAt<string>(1));
          newItem3.Tag = (object) $"{tr.MVE.IdOfElement?.ToString()};{tr.MVE.StandardId?.ToString()}";
          newItem3.MouseUp += new MouseButtonEventHandler(this.selection);
          newItem4.Tag = (object) tr.MVE.StandardId;
          newItem4.MouseUp += new MouseButtonEventHandler(this.selection);
          newItem5.Tag = (object) tr.MVE.IdOfElement;
          newItem5.MouseUp += new MouseButtonEventHandler(this.selection);
          treeViewItem.Focusable = true;
          treeViewItem.IsExpanded = true;
          treeViewItem.MouseDoubleClick += new MouseButtonEventHandler(this.handle);
          newItem3.Items.Add((object) newItem4);
          newItem3.Items.Add((object) newItem5);
          newItem3.Focusable = true;
          newItem3.IsExpanded = true;
          tvi.Items.Add((object) newItem3);
        }
      }
      else if (tr.TestName.Equals("Compare Main Material Volumes"))
      {
        foreach (string key in tr.CV.ParametersAndValues.Keys)
        {
          TreeViewItem newItem6 = new TreeViewItem();
          TreeViewItem newItem7 = new TreeViewItem();
          TreeViewItem newItem8 = new TreeViewItem();
          newItem6.Header = (object) key;
          newItem7.Header = (object) ("Expected Result: " + tr.CV.ParametersAndValues[key].ElementAt<string>(0));
          newItem8.Header = (object) ("Actual Result: " + tr.CV.ParametersAndValues[key].ElementAt<string>(1));
          newItem6.Tag = (object) $"{tr.CV.IdOfElement?.ToString()};{tr.CV.StandardId?.ToString()}";
          newItem6.MouseUp += new MouseButtonEventHandler(this.selection);
          newItem7.Tag = (object) tr.CV.StandardId;
          newItem7.MouseUp += new MouseButtonEventHandler(this.selection);
          newItem7.Focusable = true;
          newItem8.Tag = (object) tr.CV.IdOfElement;
          newItem8.MouseUp += new MouseButtonEventHandler(this.selection);
          newItem8.Focusable = true;
          newItem6.Focusable = true;
          newItem6.IsExpanded = true;
          treeViewItem.Focusable = true;
          treeViewItem.IsExpanded = true;
          newItem6.Items.Add((object) newItem7);
          newItem6.Items.Add((object) newItem8);
          tvi.Items.Add((object) newItem6);
        }
      }
      else if (tr.TestName.Equals("Addon Family Types, Counts, and Material Volumes"))
      {
        if (tr.DataForAddOn.Count<dataForAddonTest>() > 0)
        {
          foreach (dataForAddonTest dataForAddonTest in tr.DataForAddOn)
          {
            string str2 = "";
            string str3 = "";
            TreeViewItem newItem9 = new TreeViewItem();
            newItem9.Header = (object) dataForAddonTest.FamilyTypeName;
            TreeViewItem newItem10 = new TreeViewItem();
            foreach (ElementId elementId in dataForAddonTest.ListforExpected)
              str2 = $"{str2}{elementId.ToString()};";
            foreach (ElementId elementId in dataForAddonTest.ListforActual)
              str3 = $"{str3}{elementId.ToString()};";
            str1 = $"{str1}{str3};";
            string str4 = str2 + str3;
            newItem9.Tag = (object) str4;
            newItem9.MouseUp += new MouseButtonEventHandler(this.selection);
            if (dataForAddonTest.FamilyTypeCount != dataForAddonTest.ActualCount)
            {
              TreeViewItem newItem11 = new TreeViewItem();
              newItem11.Header = (object) "Count";
              newItem11.Tag = (object) str4;
              newItem11.MouseUp += new MouseButtonEventHandler(this.selection);
              newItem10.Header = (object) ("Expected Count : " + dataForAddonTest.FamilyTypeCount.ToString());
              newItem10.Tag = (object) str2;
              newItem10.MouseUp += new MouseButtonEventHandler(this.selection);
              TreeViewItem newItem12 = new TreeViewItem();
              newItem12.Header = (object) ("Actual Count: " + dataForAddonTest.ActualCount.ToString());
              newItem12.Tag = (object) str3;
              newItem12.MouseUp += new MouseButtonEventHandler(this.selection);
              newItem11.Items.Add((object) newItem10);
              newItem11.Items.Add((object) newItem12);
              newItem11.IsExpanded = true;
              newItem11.Focusable = true;
              newItem9.Items.Add((object) newItem11);
            }
            if (!dataForAddonTest.VolumePass || !dataForAddonTest.VolumePass && dataForAddonTest.FamilyTypeCount != dataForAddonTest.ActualCount)
            {
              TreeViewItem newItem13 = new TreeViewItem();
              newItem13.Header = (object) "Volume (Failing Element ID's)";
              newItem13.MouseUp += new MouseButtonEventHandler(this.selection);
              if (dataForAddonTest.ListforActual.Count<ElementId>() == 0 && dataForAddonTest.ListforExpected.Count<ElementId>() > 0)
              {
                foreach (ElementId elementId in dataForAddonTest.ListforExpected)
                {
                  TreeViewItem newItem14 = new TreeViewItem();
                  newItem14.Header = (object) elementId.ToString();
                  newItem14.Tag = (object) elementId.ToString();
                  newItem14.MouseUp += new MouseButtonEventHandler(this.selection);
                  newItem13.Items.Add((object) newItem14);
                  str2 = elementId.ToString() + ";";
                }
              }
              else if (dataForAddonTest.ListforExpected.Count<ElementId>() == 0 && dataForAddonTest.ListforActual.Count<ElementId>() > 0)
              {
                foreach (ElementId elementId in dataForAddonTest.ListforActual)
                {
                  TreeViewItem newItem15 = new TreeViewItem();
                  newItem15.Header = (object) elementId.ToString();
                  newItem15.Tag = (object) elementId.ToString();
                  newItem15.MouseUp += new MouseButtonEventHandler(this.selection);
                  newItem13.Items.Add((object) newItem15);
                  str2 = elementId.ToString() + ";";
                }
              }
              else if (dataForAddonTest.MismatchedId.Count<ElementId>() > 0)
              {
                foreach (ElementId elementId in dataForAddonTest.MismatchedId)
                {
                  TreeViewItem newItem16 = new TreeViewItem();
                  newItem16.Header = (object) elementId.ToString();
                  newItem16.Tag = (object) elementId.ToString();
                  newItem16.MouseUp += new MouseButtonEventHandler(this.selection);
                  newItem13.Items.Add((object) newItem16);
                  str2 = elementId.ToString() + ";";
                }
              }
              else
              {
                TreeViewItem newItem17 = new TreeViewItem();
                newItem17.MouseDoubleClick += new MouseButtonEventHandler(this.handle);
                newItem17.Header = (object) "Count Mismatch";
                newItem13.Items.Add((object) newItem17);
              }
              newItem13.IsExpanded = true;
              newItem13.Tag = (object) str2;
              newItem9.IsExpanded = true;
              newItem9.Focusable = true;
              newItem9.Items.Add((object) newItem13);
              tvi.Items.Add((object) newItem9);
            }
            else
            {
              newItem9.IsExpanded = true;
              tvi.Items.Add((object) newItem9);
            }
          }
          tvi.Tag = (object) str1;
          tvi.MouseUp += new MouseButtonEventHandler(this.selection);
        }
      }
      else if (tr.TestName.Equals("Compare Plate Family Types and Counts"))
      {
        if (tr.PlateTypes.Count<PlateTypes>() != 0)
        {
          foreach (PlateTypes plateType in tr.PlateTypes)
          {
            string str5 = "";
            string str6 = "";
            foreach (ElementId expected in plateType.ExpectedList)
              str5 = $"{str5}{expected.ToString()};";
            foreach (ElementId actual in plateType.ActualList)
              str6 = $"{str6}{actual.ToString()};";
            str1 = $"{str1}{str5};";
            TreeViewItem newItem18 = new TreeViewItem();
            newItem18.Tag = (object) (str5 + str6);
            newItem18.Header = (object) plateType.FamilyName;
            newItem18.MouseUp += new MouseButtonEventHandler(this.selection);
            TreeViewItem newItem19 = new TreeViewItem();
            newItem19.Header = (object) ("Expected Count : " + plateType.FamilyCount.ToString());
            newItem19.Tag = (object) str6;
            newItem19.MouseUp += new MouseButtonEventHandler(this.selection);
            TreeViewItem newItem20 = new TreeViewItem();
            newItem20.Header = (object) ("Actual Count: " + plateType.ActualCount.ToString());
            newItem20.Tag = (object) str5;
            newItem20.MouseUp += new MouseButtonEventHandler(this.selection);
            newItem18.Items.Add((object) newItem19);
            newItem18.Items.Add((object) newItem20);
            newItem18.IsExpanded = true;
            newItem18.Focusable = true;
            tvi.Items.Add((object) newItem18);
          }
          tvi.Tag = (object) str1;
          tvi.MouseUp += new MouseButtonEventHandler(this.selection);
        }
      }
      else if (tr.TestName.Equals("Compare Addon Locations"))
      {
        if (tr.Locations.Count<AddonLocation>() > 0)
        {
          foreach (AddonLocation location in tr.Locations)
          {
            string str7 = "";
            TreeViewItem newItem21 = new TreeViewItem();
            newItem21.Header = (object) location.FamilyName;
            if (location.ElementIds.Count<ElementId>() == 0)
            {
              TreeViewItem newItem22 = new TreeViewItem();
              newItem22.Header = (object) "Count Mismatch";
              newItem21.Items.Add((object) newItem22);
            }
            else
            {
              foreach (ElementId elementId in location.ElementIds)
              {
                str7 = $"{str7}{elementId.ToString()};";
                TreeViewItem newItem23 = new TreeViewItem();
                newItem23.Header = (object) elementId;
                newItem23.Tag = (object) elementId;
                newItem23.MouseUp += new MouseButtonEventHandler(this.selection);
                newItem21.Items.Add((object) newItem23);
                newItem23.Focusable = true;
                newItem23.MouseUp += new MouseButtonEventHandler(this.selection);
              }
            }
            str1 = $"{str1}{str7};";
            newItem21.Tag = (object) str7;
            newItem21.MouseUp += new MouseButtonEventHandler(this.selection);
            newItem21.IsExpanded = true;
            newItem21.Focusable = true;
            tvi.Items.Add((object) newItem21);
          }
          tvi.Tag = (object) str1;
          tvi.MouseUp += new MouseButtonEventHandler(this.selection);
        }
      }
      else if (tr.TestName.Equals("Compare Plate Locations"))
      {
        if (tr.Locations.Count<AddonLocation>() > 0)
        {
          foreach (AddonLocation location in tr.Locations)
          {
            string str8 = "";
            TreeViewItem newItem24 = new TreeViewItem();
            FamilyInstance element1 = this.rDoc.GetElement(location.ElementIds[0]) as FamilyInstance;
            if (element1.HasSuperComponent())
              newItem24.Header = (object) element1.SuperComponent.Name;
            else
              newItem24.Header = (object) element1.Name;
            if (location.ElementIds.Count<ElementId>() == 0)
            {
              TreeViewItem newItem25 = new TreeViewItem();
              newItem25.Header = (object) "Count Mismatch";
              newItem24.Items.Add((object) newItem25);
            }
            else
            {
              foreach (ElementId elementId in location.ElementIds)
              {
                str8 = $"{str8}{elementId?.ToString()};";
                TreeViewItem newItem26 = new TreeViewItem();
                FamilyInstance element2 = this.rDoc.GetElement(elementId) as FamilyInstance;
                if (element2.HasSuperComponent())
                {
                  newItem26.Header = (object) element2.SuperComponent.Id;
                  newItem26.Tag = (object) element2.SuperComponent.Id;
                }
                else
                {
                  newItem26.Header = (object) elementId;
                  newItem26.Tag = (object) elementId;
                }
                newItem26.MouseUp += new MouseButtonEventHandler(this.selection);
                bool flag = true;
                foreach (HeaderedItemsControl headeredItemsControl in (IEnumerable) newItem24.Items)
                {
                  if (headeredItemsControl.Header.Equals(newItem26.Header))
                    flag = false;
                }
                if (flag)
                  newItem24.Items.Add((object) newItem26);
                newItem26.Focusable = true;
              }
            }
            str1 = $"{str1}{str8};";
            newItem24.Tag = (object) str8;
            newItem24.MouseUp += new MouseButtonEventHandler(this.selection);
            newItem24.IsExpanded = true;
            newItem24.Focusable = true;
            tvi.Items.Add((object) newItem24);
          }
          tvi.Tag = (object) str1;
          tvi.MouseUp += new MouseButtonEventHandler(this.selection);
        }
      }
      else if (tr.TestName.Equals("Compare Member Geometry"))
      {
        TreeViewItem newItem27 = new TreeViewItem();
        TreeViewItem newItem28 = new TreeViewItem();
        TreeViewItem newItem29 = new TreeViewItem();
        newItem27.Header = (object) "Geometry Mismatch";
        newItem28.Header = (object) ("Actual: " + tr.StandardStructuralFramingId?.ToString());
        newItem29.Header = (object) ("Failing Id: " + tr.FailingStructuralFramingId?.ToString());
        string str9 = $"{tr.StandardStructuralFramingId?.ToString()}; {tr.FailingStructuralFramingId?.ToString()}";
        newItem27.Tag = (object) str9;
        newItem27.MouseUp += new MouseButtonEventHandler(this.selectionForSolidParents);
        newItem28.Tag = (object) str9;
        newItem28.MouseUp += new MouseButtonEventHandler(this.selectionForSolidParents);
        newItem29.Tag = (object) str9;
        newItem29.MouseUp += new MouseButtonEventHandler(this.selectionForSolidParents);
        newItem27.Items.Add((object) newItem28);
        newItem27.Items.Add((object) newItem29);
        newItem27.IsExpanded = true;
        tvi.Items.Add((object) newItem27);
        tvi.Tag = (object) str9;
        tvi.MouseUp += new MouseButtonEventHandler(this.selectionForSolidParents);
      }
      tvi.IsExpanded = true;
    }
    treeViewItem.IsExpanded = true;
  }

  private void handle(object sender, MouseButtonEventArgs e) => e.Handled = true;

  public void selectionForSolid(TreeViewItem tvi, RoutedEventArgs e)
  {
    tvi.IsHitTestVisible = false;
    string[] strArray = tvi.Tag.ToString().Split(';');
    int result1;
    int.TryParse(strArray[0], out result1);
    int result2;
    int.TryParse(strArray[1], out result2);
    ElementId id1 = new ElementId(result1);
    ElementId id2 = new ElementId(result2);
    List<ElementId> list = new List<ElementId>();
    list.Add(id2);
    if (this.uiDoc.Document.GetElement(id1) != null)
    {
      if (this.uiDoc.Document.GetElement(id2) != null)
      {
        try
        {
          this.IsHitTestVisible = false;
          this.SourceForm.IsHitTestVisible = false;
          TaskDialogResult taskDialogResult = new TaskDialog("Isolate Elements")
          {
            MainContent = "Would you like to isolate the failing structural framing element and generate a temporary solid for analysis?",
            CommonButtons = ((TaskDialogCommonButtons) 6),
            TitleAutoPrefix = false
          }.Show();
          if (taskDialogResult == 2)
          {
            this.IsHitTestVisible = true;
            this.SourceForm.IsHitTestVisible = true;
            return;
          }
          try
          {
            ElementId elementId = GeometryVerificationFamily.solidComparison(this.uiDoc.Document.GetElement(id1) as FamilyInstance, this.uiDoc.Document.GetElement(id2) as FamilyInstance, this.uiDoc, this.addin);
            if (elementId == (ElementId) null)
            {
              this.IsHitTestVisible = true;
              this.SourceForm.IsHitTestVisible = true;
              return;
            }
            this.mainEvent.isolate = taskDialogResult != 7;
            IList<UIView> openUiViews = this.uiApp.ActiveUIDocument.GetOpenUIViews();
            if (new FilteredElementCollector(this.uiDoc.Document, this.uiDoc.ActiveView.Id).ToList<Element>().Where<Element>((Func<Element, bool>) (eId => list.Contains(eId.Id))).Select<Element, ElementId>((Func<Element, ElementId>) (idsConv => idsConv.Id)).ToList<ElementId>().Count != list.Count)
            {
              bool flag = false;
              foreach (UIView uiView in (IEnumerable<UIView>) openUiViews)
              {
                Element element = this.uiDoc.Document.GetElement(uiView.ViewId);
                if ((element as View).ViewType == ViewType.ThreeD)
                {
                  this.uiApp.ActiveUIDocument.ActiveView = element as View;
                  flag = true;
                  break;
                }
              }
              if (!flag)
              {
                FilteredElementCollector elementCollector = new FilteredElementCollector(this.uiDoc.Document);
                ElementClassFilter filter = new ElementClassFilter(typeof (View3D));
                elementCollector.WherePasses((ElementFilter) filter);
                try
                {
                  foreach (View3D view3D in elementCollector)
                  {
                    if (view3D.IsValidObject && !view3D.IsTemplate)
                    {
                      this.uiApp.ActiveUIDocument.ActiveView = (View) view3D;
                      break;
                    }
                  }
                }
                catch (Exception ex)
                {
                  throw ex;
                }
              }
            }
            this.mainEvent.directShape = elementId;
            this.mainEvent.ListOfIds = list;
            this.mainEvent.detailWindow = this;
            this.mainEvent.overallResults = this.SourceForm;
            this.mainEvent.ExecuteHighlightEvent((object) null);
            App.MarkVerificationSolidToBeHighlighted = this.mainEvent;
          }
          catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
          {
            App.DialogSwitches.SuspendModelLockingforOperation = false;
            return;
          }
          tvi.IsHitTestVisible = true;
          e.Handled = true;
          return;
        }
        catch (Exception ex)
        {
          int num = (int) MessageBox.Show("The solid geometry for analysis could not be generated.");
          this.IsHitTestVisible = true;
          this.SourceForm.IsHitTestVisible = true;
          return;
        }
      }
    }
    int num1 = (int) MessageBox.Show("Solid Analysis cannot be generated since one of the selected elements do not exist in the model.");
  }

  public void selectionForSolidParents(object sender, RoutedEventArgs e)
  {
    if (sender is TreeViewItem)
    {
      TreeViewItem tvi = sender as TreeViewItem;
      tvi.IsHitTestVisible = false;
      this.selectionForSolid(tvi, e);
      e.Handled = true;
      (sender as TreeViewItem).IsSelected = false;
    }
    (sender as TreeViewItem).IsHitTestVisible = true;
  }

  private void highlightSelection(TreeViewItem tvi, MouseButtonEventArgs e)
  {
    List<ElementId> elementIdList = new List<ElementId>();
    if (!(e.OriginalSource is TextBlock originalSource) || !tvi.Header.ToString().Equals(originalSource.Text))
      return;
    string str = tvi.Tag.ToString();
    char[] chArray = new char[1]{ ';' };
    foreach (string s in str.Split(chArray))
    {
      if (!string.IsNullOrEmpty(s))
      {
        int result;
        int.TryParse(s, out result);
        Element element = this.uiDoc.Document.GetElement(new ElementId(result));
        if (element != null)
        {
          if (element.Name.ToUpper().Contains("FLAT (DO NOT USE)"))
          {
            if (element.GetTopLevelElement().Id != (ElementId) null)
              elementIdList.Add(element.GetTopLevelElement().Id);
            else
              elementIdList.Add(element.Id);
          }
          else
            elementIdList.Add(element.Id);
        }
      }
    }
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }

  public void selection(object sender, MouseButtonEventArgs e)
  {
    TreeViewItem tvi = sender as TreeViewItem;
    this.highlightSelection(tvi, e);
    e.Handled = true;
    tvi.IsSelected = false;
  }

  private void Treeview_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
  {
    TreeView treeView = sender as TreeView;
    if (e.NewValue == null || !(treeView.SelectedItem is TreeViewItem selectedItem) || selectedItem.IsExpanded)
      return;
    foreach (TestResult tr in (IEnumerable) this.Results.Items)
    {
      List<ElementId> elementIdList = new List<ElementId>();
      if (tr.TestName.Equals(selectedItem.Header.ToString()))
      {
        this.expand(selectedItem, tr);
        break;
      }
      selectedItem.IsExpanded = true;
      selectedItem.IsSelected = false;
    }
  }

  private void GroupTestResults_GotFocus(object sender, RoutedEventArgs e)
  {
    IList selectedItems = ((ListBox) sender).SelectedItems;
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (GroupTestResult groupTestResult in (IEnumerable) selectedItems)
    {
      foreach (Element groupMember in groupTestResult.GroupMembers)
      {
        if (groupMember != null && groupMember.IsValidObject)
          elementIdList.Add(groupMember.GetTopLevelElement().Id);
      }
    }
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }

  private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
  {
    this.ScrollViewerResults.ScrollToVerticalOffset(this.ScrollViewerResults.VerticalOffset - (double) e.Delta);
    e.Handled = true;
  }

  private void Window_Activated(object sender, EventArgs e)
  {
    if (this.uiApp == null || this.uiApp.ActiveUIDocument == null)
    {
      App.MarkVerificationExistingWindow.Close();
    }
    else
    {
      UIDocument activeUiDocument = this.uiApp.ActiveUIDocument;
      string title = activeUiDocument.Document.Title;
      string pathName = activeUiDocument.Document.PathName;
      if (pathName.Equals(this.previousPath) && title.Equals(this.previousTitle))
        return;
      this.previousTitle = title;
      this.previousPath = pathName;
      this.Close();
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/markverification/resultspresentation/mkexistingdetails.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Window) target).Activated += new EventHandler(this.Window_Activated);
        ((Window) target).Closed += new EventHandler(this.Window_Closed);
        break;
      case 2:
        this.Baseline = (TextBlock) target;
        break;
      case 3:
        this.GroupTestResults = (ListBox) target;
        this.GroupTestResults.GotFocus += new RoutedEventHandler(this.GroupTestResults_GotFocus);
        this.GroupTestResults.SelectionChanged += new SelectionChangedEventHandler(this.GroupTestResults_SelectionChanged);
        break;
      case 4:
        this.NextControlMark = (TextBlock) target;
        break;
      case 5:
        this.Export = (Button) target;
        this.Export.Click += new RoutedEventHandler(this.Export_Click);
        break;
      case 6:
        this.Detail = (Button) target;
        this.Detail.Click += new RoutedEventHandler(this.Detail_Click);
        break;
      case 7:
        this.evenBiggerGrid = (System.Windows.Controls.Grid) target;
        break;
      case 8:
        this.DetailResults = (ListBox) target;
        this.DetailResults.SelectionChanged += new SelectionChangedEventHandler(this.DetailResults_SelectionChanged);
        break;
      case 9:
        this.ElementDisplay = (TextBlock) target;
        break;
      case 10:
        this.ScrollViewerResults = (ScrollViewer) target;
        this.ScrollViewerResults.PreviewMouseWheel += new MouseWheelEventHandler(this.ScrollViewer_PreviewMouseWheel);
        break;
      case 11:
        this.Results = (ListBox) target;
        this.Results.SelectionChanged += new SelectionChangedEventHandler(this.Results_SelectionChanged);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IStyleConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 12)
      return;
    ((TreeView) target).SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(this.Treeview_SelectedItemChanged);
  }
}
