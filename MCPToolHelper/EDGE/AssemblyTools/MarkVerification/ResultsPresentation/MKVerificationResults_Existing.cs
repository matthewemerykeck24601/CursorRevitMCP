// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.MKVerificationResults_Existing
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DocumentFormat.OpenXml.Spreadsheet;
using EDGE.AssemblyTools.MarkVerification.QA;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public class MKVerificationResults_Existing : Window, IComponentConnector
{
  private ICollection<MarkResult> _results;
  private UIDocument uiDoc;
  private UIApplication uiApp;
  private Document revitDoc;
  private string previousTitle;
  private string previousPath;
  private MarkQA MarkQACopyForResultsExisting = new MarkQA();
  private AddInId addinId;
  private ExternalEvent[] externalEventForMarkV = new ExternalEvent[1];
  private MKExistingDetails detailWindow;
  internal Button Passed;
  internal Button Failed;
  internal Button All;
  internal TextBox SearchBox;
  internal ListBox lstResults;
  internal System.Windows.Controls.Grid gridofresults;
  internal ListBox lstTestResults;
  internal Button DetailButton;
  internal Button WarningsBUtton;
  internal Button ExportButton;
  private bool _contentLoaded;

  public MKVerificationResults_Existing(
    List<MarkResult> results,
    UIApplication uiapp,
    Document Doc,
    IntPtr parentWindowHandler,
    MarkQA markQACopy,
    ExternalEvent[] externalEvent)
  {
    this.uiDoc = uiapp.ActiveUIDocument;
    this.uiApp = uiapp;
    this.revitDoc = Doc;
    this.addinId = uiapp.Application.ActiveAddInId;
    this.previousTitle = this.uiApp.ActiveUIDocument.Document.Title;
    this.previousPath = this.uiApp.ActiveUIDocument.Document.PathName;
    this.InitializeComponent();
    this.Closed += new EventHandler(this.thisWindowClosed);
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    ObservableCollection<MarkResult> source = new ObservableCollection<MarkResult>();
    this.MarkQACopyForResultsExisting = new MarkQA(markQACopy);
    if (results.Count == 0)
      this.ExportButton.IsEnabled = false;
    int num1 = 0;
    int num2 = 0;
    foreach (MarkResult result in results)
    {
      source.Add(new MarkResult(result));
      if (result.Verified)
        ++num2;
      else
        ++num1;
    }
    source.ToList<MarkResult>();
    this._results = (ICollection<MarkResult>) source;
    this.lstResults.ItemsSource = (IEnumerable) this._results;
    this.externalEventForMarkV[0] = externalEvent[0];
    this.Passed.Content = (object) $"{this.Passed.Content?.ToString()} ({num2.ToString()})";
    this.Failed.Content = (object) $"{this.Failed.Content?.ToString()} ({num1.ToString()})";
    this.All.Content = (object) $"{this.All.Content?.ToString()} ({(num2 + num1).ToString()})";
  }

  private void App_Activated(object sender, EventArgs e)
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
      App.MarkVerificationExistingWindow.Close();
    }
  }

  private void lstTestResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    IList selectedItems = ((ListBox) sender).SelectedItems;
    TestResult testResult1 = new TestResult();
    foreach (TestResult testResult2 in (IEnumerable) selectedItems)
      testResult1 = testResult2;
    if (!this.MarkQACopyForResultsExisting.traditionalApproach)
    {
      List<ElementId> elementIdList = new List<ElementId>();
      foreach (MarkResult result in (IEnumerable<MarkResult>) this._results)
      {
        if (result.ControlMark.Equals((this.lstResults.SelectedValue as MarkResult).ControlMark))
        {
          foreach (GroupTestResult groupTestResult in result.GroupTestResults)
          {
            foreach (TestResult groupResult in groupTestResult.GroupResults)
            {
              if (testResult1.TestName.Equals(groupResult.TestName))
              {
                if (!groupResult.Passed)
                {
                  if (!groupResult.NotUsed)
                  {
                    try
                    {
                      if (this.uiDoc.Document.GetElement(groupResult.FailingStructuralFramingId).GetTopLevelElement().IsValidObject)
                      {
                        elementIdList.Add(this.uiDoc.Document.GetElement(groupResult.FailingStructuralFramingId).GetTopLevelElement().Id);
                        break;
                      }
                      break;
                    }
                    catch (Exception ex)
                    {
                      elementIdList = new List<ElementId>();
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
              }
            }
          }
        }
      }
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
    }
    else if (testResult1.FailingStructuralFramingId != (ElementId) null)
    {
      Element element1 = this.uiDoc.Document.GetElement(testResult1.FailingStructuralFramingId);
      Element element2 = this.uiDoc.Document.GetElement(testResult1.StandardStructuralFramingId);
      if (element1 == null || element2 == null)
        return;
      ElementId id = element1.GetTopLevelElement().Id;
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>()
      {
        element2.GetTopLevelElement().Id,
        id
      });
    }
    else
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
  }

  private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    ICollection<MarkResult> markResults = (ICollection<MarkResult>) new List<MarkResult>();
    string str1 = sender.ToString();
    if (str1 == "System.Windows.Controls.TextBox")
    {
      this.lstResults.ItemsSource = (IEnumerable) this._results;
    }
    else
    {
      int num = str1.IndexOf(":");
      string str2 = str1.Substring(num + 2);
      int length = str2.Length;
      string str3 = str2.Substring(0, length).Trim();
      ObservableCollection<MarkResult> observableCollection = new ObservableCollection<MarkResult>();
      foreach (MarkResult result in (IEnumerable<MarkResult>) this._results)
      {
        if (result.ControlMark.ToLower().Contains(str3.ToLower()))
          observableCollection.Add(new MarkResult(result));
      }
      this.lstResults.ItemsSource = (IEnumerable) observableCollection;
    }
  }

  private void btnpass_Click(object sender, RoutedEventArgs e)
  {
    ICollection<MarkResult> markResults = (ICollection<MarkResult>) new List<MarkResult>();
    ObservableCollection<MarkResult> observableCollection = new ObservableCollection<MarkResult>();
    foreach (MarkResult result in (IEnumerable<MarkResult>) this._results)
    {
      if (result.Verified)
        observableCollection.Add(new MarkResult(result));
    }
    this.lstResults.ItemsSource = (IEnumerable) observableCollection;
  }

  private void btnfail_Click(object sender, RoutedEventArgs e)
  {
    ICollection<MarkResult> markResults = (ICollection<MarkResult>) new List<MarkResult>();
    ObservableCollection<MarkResult> observableCollection = new ObservableCollection<MarkResult>();
    foreach (MarkResult result in (IEnumerable<MarkResult>) this._results)
    {
      if (!result.Verified)
        observableCollection.Add(new MarkResult(result));
    }
    this.lstResults.ItemsSource = (IEnumerable) observableCollection;
  }

  private void btnall_Click(object sender, RoutedEventArgs e)
  {
    this.lstResults.ItemsSource = (IEnumerable) this._results;
  }

  private void DetailButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.lstResults.SelectedItem == null)
      return;
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    MarkResult selectedItem = this.lstResults.SelectedItem as MarkResult;
    selectedItem.GroupTestResults = selectedItem.GroupTestResults.OrderByDescending<GroupTestResult, int>((Func<GroupTestResult, int>) (ef => ef.GroupQuantity)).ToList<GroupTestResult>();
    this.detailWindow = new MKExistingDetails(selectedItem, this.uiApp, this.revitDoc, mainWindowHandle, this, this.MarkQACopyForResultsExisting, this.addinId, this.externalEventForMarkV);
    this.detailWindow.Show();
    this.DetailButton.IsEnabled = false;
    this.DetailButton.IsHitTestVisible = false;
    this.detailWindow.Closed += new EventHandler(this.closed);
  }

  private void closed(object sender, EventArgs e)
  {
    this.DetailButton.IsEnabled = true;
    this.DetailButton.IsHitTestVisible = true;
  }

  private void thisWindowClosed(object sender, EventArgs e)
  {
    if (this.detailWindow == null)
      return;
    this.detailWindow.Close();
  }

  private void lstResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (this.lstResults.SelectedItem == null)
      return;
    MarkResult selectedItem = this.lstResults.SelectedItem as MarkResult;
    if (selectedItem.GroupTestResults.Count > 1)
      this.DetailButton.Visibility = System.Windows.Visibility.Visible;
    else
      this.DetailButton.Visibility = System.Windows.Visibility.Collapsed;
    if (!this.MarkQACopyForResultsExisting.traditionalApproach)
    {
      if (selectedItem.FailedRotationList != null && selectedItem.CountMultiplierList != null)
      {
        if (selectedItem.FailedRotationList.Count<Plates>() > 0 || selectedItem.CountMultiplierList.Count<Plates>() > 0)
          this.WarningsBUtton.Visibility = System.Windows.Visibility.Visible;
        else
          this.WarningsBUtton.Visibility = System.Windows.Visibility.Collapsed;
      }
      else if (selectedItem.CountMultiplierList != null)
      {
        if (selectedItem.CountMultiplierList.Count<Plates>() > 0)
          this.WarningsBUtton.Visibility = System.Windows.Visibility.Visible;
        else
          this.WarningsBUtton.Visibility = System.Windows.Visibility.Collapsed;
      }
      else if (selectedItem.FailedRotationList != null)
      {
        if (selectedItem.FailedRotationList.Count<Plates>() > 0)
          this.WarningsBUtton.Visibility = System.Windows.Visibility.Visible;
        else
          this.WarningsBUtton.Visibility = System.Windows.Visibility.Collapsed;
      }
      else
        this.WarningsBUtton.Visibility = System.Windows.Visibility.Collapsed;
    }
    List<ElementId> elementIdList = new List<ElementId>();
    if (selectedItem.GroupTestResults != null)
    {
      foreach (GroupTestResult groupTestResult in selectedItem.GroupTestResults)
      {
        if (groupTestResult.GroupResults != null)
        {
          foreach (MemberDetails groupDetail in groupTestResult.GroupDetails)
          {
            ElementId id = ElementId.Parse(groupDetail.ElementId);
            try
            {
              Element topLevelElement = this.uiDoc.Document.GetElement(id).GetTopLevelElement();
              if (topLevelElement.IsValidObject)
                elementIdList.Add(topLevelElement.Id);
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
    }
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }

  private string familytype(ElementId id)
  {
    FamilyInstance element = this.uiDoc.Document.GetElement(id) as FamilyInstance;
    return $"{element.Symbol.FamilyName} - {element.Symbol.Name}";
  }

  private void ExportButton_Click(object sender, RoutedEventArgs e)
  {
    int num1 = this.MarkQACopyForResultsExisting.traditionalApproach ? 1 : 0;
    List<List<List<object>>> objectListListList = new List<List<List<object>>>();
    if (num1 == 0)
    {
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
        Dictionary<string, List<storingTestTypesAndItsData>> dictionary1 = new Dictionary<string, List<storingTestTypesAndItsData>>();
        foreach (MarkResult markResult in this.lstResults.ItemsSource)
        {
          string key1 = this.removeSpecialCharacters(markResult.ControlMark);
          Dictionary<object, List<object>> dictionary2 = new Dictionary<object, List<object>>();
          int num2 = 0;
          List<List<object>> objectListList = new List<List<object>>();
          Dictionary<string, List<ElementId>> dictionary3 = new Dictionary<string, List<ElementId>>();
          int num3 = markResult.GroupTestResults.Count<GroupTestResult>();
          List<object> objectList1 = new List<object>();
          List<TestResult> groupResults = markResult.GroupTestResults.First<GroupTestResult>().GroupResults;
          if (groupResults.Count<TestResult>() == 0)
          {
            storingTestTypesAndItsData testTypesAndItsData = new storingTestTypesAndItsData();
            dictionary1.Add(key1, new List<storingTestTypesAndItsData>()
            {
              testTypesAndItsData
            });
          }
          else
          {
            bool flag1 = false;
            if (!markResult.PlateRotated)
            {
              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
              {
                if (groupTestResult.GroupResults.Where<TestResult>((Func<TestResult, bool>) (testResult => testResult.Passed)).Count<TestResult>() == groupResults.Count)
                {
                  if (groupTestResult.GroupResults.Where<TestResult>((Func<TestResult, bool>) (test => test.NotUsed)).Count<TestResult>() == 0)
                  {
                    flag1 = true;
                  }
                  else
                  {
                    flag1 = false;
                    break;
                  }
                }
                else
                {
                  flag1 = false;
                  break;
                }
              }
            }
            if (flag1)
            {
              storingTestTypesAndItsData testTypesAndItsData = new storingTestTypesAndItsData();
              dictionary1.Add(key1, new List<storingTestTypesAndItsData>()
              {
                testTypesAndItsData
              });
            }
            else
            {
              storingTestTypesAndItsData testTypesAndItsData1 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData2 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData3 = new storingTestTypesAndItsData();
              objectList1.Add((object) "");
              for (int index = 1; index <= num3; ++index)
                objectList1.Add((object) index);
              testTypesAndItsData1.ListofData = new List<object>()
              {
                (object) objectList1
              };
              testTypesAndItsData1.TestName = "Detail Groups";
              List<object> objectList2 = new List<object>();
              List<object> objectList3 = new List<object>();
              objectList2.Add((object) "");
              objectList3.Add((object) "");
              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
              {
                objectList3.Add((object) groupTestResult.GroupMembers.First<FamilyInstance>().Id);
                string str = "";
                foreach (FamilyInstance groupMember in groupTestResult.GroupMembers)
                  str = !groupTestResult.GroupMembers.Last<FamilyInstance>().Equals((object) groupMember) ? $"{str}{groupMember.Id?.ToString()}; " : str + groupMember.Id?.ToString();
                objectList2.Add((object) str);
                if (groupTestResult.GroupQuantity > num2)
                  num2 = groupTestResult.GroupQuantity;
              }
              testTypesAndItsData2.ListofData = new List<object>()
              {
                (object) objectList3
              };
              testTypesAndItsData2.TestName = "Master Element ID";
              testTypesAndItsData3.ListofData = new List<object>()
              {
                (object) objectList2
              };
              testTypesAndItsData3.TestName = "Structural Framing Elements";
              markResult.GroupTestResults.First<GroupTestResult>().GroupResults.Count<TestResult>();
              storingTestTypesAndItsData testTypesAndItsData4 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData5 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData6 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData7 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData8 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData9 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData10 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData11 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData12 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData13 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData14 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData15 = new storingTestTypesAndItsData();
              storingTestTypesAndItsData testTypesAndItsData16 = new storingTestTypesAndItsData();
              List<object> objectList4 = new List<object>();
              List<object> objectList5 = new List<object>();
              List<object> objectList6 = new List<object>();
              List<object> objectList7 = new List<object>();
              List<object> objectList8 = new List<object>();
              List<object> objectList9 = new List<object>();
              List<object> objectList10 = new List<object>();
              List<object> objectList11 = new List<object>();
              List<object> objectList12 = new List<object>();
              List<object> objectList13 = new List<object>();
              List<object> objectList14 = new List<object>();
              List<object> objectList15 = new List<object>();
              foreach (TestResult testResult1 in groupResults)
              {
                List<object> objectList16 = new List<object>();
                List<object> objectList17 = new List<object>();
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
                            bool flag2 = true;
                            bool flag3 = true;
                            List<AddonLocation> addonLocationList1 = new List<AddonLocation>();
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                                        bool flag4 = false;
                                        foreach (AddonLocation addonLocation in addonLocationList1)
                                        {
                                          if (addonLocation.FamilyName.Equals(current.FamilyName))
                                            flag4 = true;
                                        }
                                        if (!flag4)
                                          addonLocationList1.Add(current);
                                      }
                                      break;
                                    }
                                  }
                                }
                              }
                            }
                            List<AddonLocation> addonLocationList2 = new List<AddonLocation>();
                            List<AddonLocation> source = new List<AddonLocation>();
                            foreach (AddonLocation addonLocation in addonLocationList1)
                            {
                              if (addonLocation.Addons)
                                addonLocationList2.Add(addonLocation);
                              else
                                source.Add(addonLocation);
                            }
                            if (addonLocationList2 != null && addonLocationList2.Count > 0)
                            {
                              flag2 = false;
                              foreach (AddonLocation addonLocation1 in addonLocationList2)
                              {
                                List<object> objectList18 = new List<object>();
                                bool flag5 = false;
                                objectList18.Add((object) addonLocation1.FamilyName);
                                foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                                {
                                  if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                                  {
                                    objectList18.Add((object) "Not Processed");
                                    testTypesAndItsData11.Pass = "Not Processed";
                                  }
                                  else
                                  {
                                    bool flag6 = false;
                                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                                    {
                                      if (groupResult.TestName.Equals(testResult1.TestName))
                                      {
                                        if (addonLocation1.WarningId != null)
                                        {
                                          foreach (ElementId elementId in addonLocation1.WarningId.Distinct<ElementId>())
                                          {
                                            if (dictionary3.ContainsKey(addonLocation1.FamilyName))
                                            {
                                              if (!dictionary3[addonLocation1.FamilyName].Contains(elementId))
                                                dictionary3[addonLocation1.FamilyName].Add(elementId);
                                            }
                                            else
                                              dictionary3.Add(addonLocation1.FamilyName, addonLocation1.WarningId.Distinct<ElementId>().ToList<ElementId>());
                                          }
                                        }
                                        if (groupResult.Passed)
                                        {
                                          if (!flag5)
                                          {
                                            objectList18.Add((object) "BASELINE");
                                            flag5 = true;
                                            break;
                                          }
                                          objectList18.Add((object) "PASS");
                                          break;
                                        }
                                        List<AddonLocation> addonLocationList3 = new List<AddonLocation>();
                                        List<AddonLocation> locations = groupResult.Locations;
                                        if (!groupResult.Passed && groupResult.NotUsed)
                                        {
                                          objectList18.Add((object) "PASS");
                                          break;
                                        }
                                        foreach (AddonLocation addonLocation2 in locations)
                                        {
                                          if (addonLocation1.FamilyName.Equals(addonLocation2.FamilyName))
                                          {
                                            flag6 = true;
                                            if (!groupResult.Passed)
                                            {
                                              testTypesAndItsData11.Pass = "FAIL";
                                              string str = addonLocation2.ElementIds.Count<ElementId>() != 0 ? string.Join<ElementId>("; ", (IEnumerable<ElementId>) addonLocation2.ElementIds) : "Count Mismatch";
                                              objectList18.Add((object) str);
                                              break;
                                            }
                                            objectList18.Add((object) "PASS");
                                            break;
                                          }
                                        }
                                        if (!flag6)
                                          objectList18.Add((object) "PASS");
                                      }
                                    }
                                  }
                                }
                                objectList11.Add((object) objectList18);
                              }
                            }
                            if (source != null && source.Count<AddonLocation>() > 0)
                            {
                              flag3 = false;
                              foreach (AddonLocation addonLocation in source)
                              {
                                List<object> objectList19 = new List<object>();
                                bool flag7 = false;
                                objectList19.Add((object) addonLocation.FamilyName);
                                foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                                {
                                  if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                                  {
                                    objectList19.Add((object) "Not Processed");
                                    testTypesAndItsData14.Pass = "Not Processed";
                                  }
                                  else
                                  {
                                    bool flag8 = false;
                                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                                    {
                                      if (groupResult.TestName.Equals(testResult1.TestName))
                                      {
                                        if (addonLocation.WarningId != null)
                                        {
                                          foreach (ElementId elementId in addonLocation.WarningId.Distinct<ElementId>())
                                          {
                                            if (dictionary3.ContainsKey(addonLocation.FamilyName))
                                            {
                                              if (!dictionary3[addonLocation.FamilyName].Contains(elementId))
                                                dictionary3[addonLocation.FamilyName].Add(elementId);
                                            }
                                            else
                                              dictionary3.Add(addonLocation.FamilyName, addonLocation.WarningId.Distinct<ElementId>().ToList<ElementId>());
                                          }
                                        }
                                        if (groupResult.Passed)
                                        {
                                          if (!flag7)
                                          {
                                            objectList19.Add((object) "BASELINE");
                                            flag7 = true;
                                            break;
                                          }
                                          objectList19.Add((object) "PASS");
                                          break;
                                        }
                                        if (!groupResult.Passed && groupResult.NotUsed)
                                        {
                                          objectList19.Add((object) "PASS");
                                          break;
                                        }
                                        List<AddonLocation> addonLocationList4 = new List<AddonLocation>();
                                        foreach (AddonLocation location in groupResult.Locations)
                                        {
                                          if (addonLocation.FamilyName.Equals(location.FamilyName))
                                          {
                                            flag8 = true;
                                            if (!groupResult.Passed)
                                            {
                                              testTypesAndItsData14.Pass = "FAIL";
                                              string str = location.ElementIds.Count<ElementId>() != 0 ? string.Join<ElementId>("; ", (IEnumerable<ElementId>) location.ElementIds) : "Count Mismatch";
                                              objectList19.Add((object) str);
                                              break;
                                            }
                                            objectList19.Add((object) "PASS");
                                            break;
                                          }
                                        }
                                        if (!flag8)
                                          objectList19.Add((object) "PASS");
                                      }
                                    }
                                  }
                                }
                                objectList13.Add((object) objectList19);
                              }
                            }
                            if (flag2)
                            {
                              List<object> objectList20 = new List<object>();
                              bool flag9 = false;
                              List<object> objectList21 = new List<object>();
                              objectList21.Add((object) "");
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                              {
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    if (!groupResult.Passed && groupResult.NotUsed)
                                    {
                                      testTypesAndItsData11.Pass = "Not Processed";
                                      objectList21.Add((object) "Not Processed");
                                      break;
                                    }
                                    if (!groupResult.Passed && !groupResult.NotUsed)
                                    {
                                      testTypesAndItsData11.Pass = "FAIL";
                                      objectList21.Add((object) "FAIL");
                                      break;
                                    }
                                    objectList21.Add((object) "PASS");
                                    break;
                                  }
                                }
                                if (flag9)
                                  break;
                              }
                              objectList11.Add((object) objectList21);
                            }
                            if (flag3)
                            {
                              bool flag10 = false;
                              List<object> objectList22 = new List<object>();
                              objectList22.Add((object) "");
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                              {
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    if (!groupResult.Passed && groupResult.NotUsed)
                                    {
                                      testTypesAndItsData14.Pass = "Not Processed";
                                      objectList22.Add((object) "Not Processed");
                                      break;
                                    }
                                    objectList22.Add((object) "PASS");
                                    break;
                                  }
                                }
                                if (flag10)
                                  break;
                              }
                              objectList13.Add((object) objectList22);
                            }
                            testTypesAndItsData11.ListofData = objectList11;
                            testTypesAndItsData11.TestName = "Addon Location Comparison";
                            testTypesAndItsData14.ListofData = objectList13;
                            testTypesAndItsData14.TestName = "Finish Location Comparison";
                            continue;
                          }
                          continue;
                        case 'M':
                          if (testName == "Compare Member Geometry")
                          {
                            Dictionary<string, List<ElementId>> source = new Dictionary<string, List<ElementId>>();
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                            {
                              if (groupTestResult.GroupResults.Count<TestResult>() != 1)
                              {
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName) && !groupResult.Passed && !groupResult.NotUsed)
                                  {
                                    string key2 = groupResult.StandardStructuralFramingId.ToString();
                                    if (source.ContainsKey(key2))
                                    {
                                      source[key2].Add(groupResult.FailingStructuralFramingId);
                                      break;
                                    }
                                    source.Add(key2, new List<ElementId>()
                                    {
                                      groupResult.FailingStructuralFramingId
                                    });
                                    break;
                                  }
                                }
                              }
                            }
                            if (source == null || source.Count<KeyValuePair<string, List<ElementId>>>() == 0)
                            {
                              List<object> objectList23 = new List<object>();
                              objectList23.Add((object) "");
                              bool flag11 = false;
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                              {
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    if (groupResult.Passed && !groupResult.NotUsed)
                                    {
                                      objectList23.Add((object) "PASS");
                                      break;
                                    }
                                    if (!groupResult.Passed)
                                    {
                                      if (groupResult.NotUsed)
                                      {
                                        objectList23.Add((object) "Not Processed");
                                        testTypesAndItsData15.Pass = "Not Processed";
                                        break;
                                      }
                                      break;
                                    }
                                    break;
                                  }
                                }
                                if (flag11)
                                  break;
                              }
                              objectList15.Add((object) objectList23);
                            }
                            else
                            {
                              foreach (KeyValuePair<string, List<ElementId>> keyValuePair in source)
                              {
                                List<object> objectList24 = new List<object>();
                                objectList24.Add((object) "");
                                bool flag12 = false;
                                foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                                {
                                  if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                                  {
                                    objectList24.Add((object) "Not Processed");
                                  }
                                  else
                                  {
                                    foreach (TestResult groupResult in groupTestResult.GroupResults)
                                    {
                                      if (groupResult.TestName.Equals(testResult1.TestName))
                                      {
                                        if (groupResult.Passed && groupTestResult.GroupQuantity == num2)
                                        {
                                          if (!flag12)
                                          {
                                            objectList24.Add((object) "BASELINE");
                                            flag12 = true;
                                            break;
                                          }
                                          objectList24.Add((object) "PASS");
                                          break;
                                        }
                                        if (!groupResult.Passed && groupResult.NotUsed)
                                        {
                                          objectList24.Add((object) "PASS");
                                          break;
                                        }
                                        if (groupResult.StandardStructuralFramingId.ToString().Equals(keyValuePair.Key))
                                        {
                                          if (!groupResult.Passed)
                                          {
                                            objectList24.Add((object) "FAIL");
                                            testTypesAndItsData15.Pass = "FAIL";
                                            break;
                                          }
                                          objectList24.Add((object) "PASS");
                                          break;
                                        }
                                        break;
                                      }
                                    }
                                  }
                                }
                                objectList15.Add((object) objectList24);
                              }
                            }
                            testTypesAndItsData15.ListofData = objectList15;
                            testTypesAndItsData15.TestName = "Compare Member Geometry";
                            continue;
                          }
                          continue;
                        case 'P':
                          if (testName == "Compare Plate Locations")
                          {
                            List<AddonLocation> source = new List<AddonLocation>();
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                                        bool flag13 = false;
                                        foreach (AddonLocation addonLocation in source)
                                        {
                                          if (addonLocation.FamilyName.Equals(current.FamilyName))
                                            flag13 = true;
                                        }
                                        if (!flag13)
                                          source.Add(current);
                                      }
                                      break;
                                    }
                                  }
                                }
                              }
                            }
                            if (source.Count<AddonLocation>() == 0)
                            {
                              List<object> objectList25 = new List<object>();
                              objectList25.Add((object) "");
                              bool flag14 = false;
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                                    if (!groupResult.Passed && !groupResult.NotUsed)
                                    {
                                      objectList25.Add((object) "FAIL");
                                      testTypesAndItsData8.Pass = "FAIL";
                                      break;
                                    }
                                    if (!groupResult.Passed)
                                    {
                                      if (groupResult.NotUsed)
                                      {
                                        objectList25.Add((object) "Not Processed");
                                        testTypesAndItsData8.Pass = "Not Processed";
                                        break;
                                      }
                                      break;
                                    }
                                    break;
                                  }
                                }
                                if (flag14)
                                  break;
                              }
                              objectList8.Add((object) objectList25);
                            }
                            else
                            {
                              foreach (AddonLocation addonLocation3 in source)
                              {
                                List<object> objectList26 = new List<object>();
                                objectList26.Add((object) addonLocation3.FamilyName);
                                bool flag15 = false;
                                foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                                {
                                  bool flag16 = false;
                                  foreach (TestResult groupResult in groupTestResult.GroupResults)
                                  {
                                    if (groupResult.TestName.Equals(testResult1.TestName))
                                    {
                                      if (addonLocation3.WarningId != null)
                                      {
                                        foreach (ElementId elementId in addonLocation3.WarningId.Distinct<ElementId>())
                                        {
                                          if (dictionary3.ContainsKey(addonLocation3.FamilyName))
                                          {
                                            if (!dictionary3[addonLocation3.FamilyName].Contains(elementId))
                                              dictionary3[addonLocation3.FamilyName].Add(elementId);
                                          }
                                          else
                                            dictionary3.Add(addonLocation3.FamilyName, addonLocation3.WarningId.Distinct<ElementId>().ToList<ElementId>());
                                        }
                                      }
                                      if (groupResult.Passed)
                                      {
                                        if (!flag15)
                                        {
                                          objectList26.Add((object) "BASELINE");
                                          flag15 = true;
                                          break;
                                        }
                                        objectList26.Add((object) "PASS");
                                        break;
                                      }
                                      List<AddonLocation> addonLocationList = new List<AddonLocation>();
                                      List<AddonLocation> locations = groupResult.Locations;
                                      if (!groupResult.Passed && groupResult.NotUsed)
                                      {
                                        objectList26.Add((object) "Not Processed");
                                        testTypesAndItsData8.Pass = "Not Processed";
                                        break;
                                      }
                                      if (!groupResult.Passed && !groupResult.NotUsed)
                                      {
                                        objectList26.Add((object) "FAIL");
                                        testTypesAndItsData8.Pass = "FAIL";
                                        break;
                                      }
                                      foreach (AddonLocation addonLocation4 in locations)
                                      {
                                        if (addonLocation3.FamilyName.Equals(addonLocation4.FamilyName))
                                        {
                                          flag16 = true;
                                          if (!groupResult.Passed)
                                          {
                                            string str;
                                            if (addonLocation4.ElementIds.Count<ElementId>() == 0)
                                            {
                                              str = "Count Mismatch";
                                              testTypesAndItsData8.Pass = "FAIL";
                                            }
                                            else
                                              str = string.Join<ElementId>("; ", (IEnumerable<ElementId>) addonLocation4.ElementIds);
                                            objectList26.Add((object) str);
                                            break;
                                          }
                                          objectList26.Add((object) "PASS");
                                          break;
                                        }
                                      }
                                      if (!flag16)
                                        objectList26.Add((object) "PASS");
                                    }
                                  }
                                }
                                objectList8.Add((object) objectList26);
                              }
                            }
                            testTypesAndItsData8.ListofData = objectList8;
                            testTypesAndItsData8.TestName = "Plate Location Comparison (results provides Element ID for offending items)";
                            continue;
                          }
                          continue;
                        case 'y':
                          if (testName == "Family Types Comparison")
                          {
                            List<object> objectList27 = new List<object>();
                            objectList27.Add((object) "");
                            bool flag17 = false;
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                                      flag17 = true;
                                      break;
                                    }
                                    break;
                                  }
                                  break;
                                }
                              }
                            }
                            bool flag18 = false;
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                            {
                              string str = "";
                              foreach (TestResult groupResult in groupTestResult.GroupResults)
                              {
                                if (groupResult.TestName.Equals(testResult1.TestName))
                                {
                                  if (groupResult.Passed)
                                  {
                                    if (flag17)
                                    {
                                      if (groupTestResult.GroupQuantity == num2)
                                      {
                                        if (!flag18)
                                        {
                                          str = this.familytype(groupResult.StandardStructuralFramingId);
                                          flag18 = true;
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
                                  {
                                    testTypesAndItsData4.Pass = "FAIL";
                                    str = groupResult.ActualResult;
                                  }
                                  objectList27.Add((object) str);
                                  break;
                                }
                              }
                            }
                            objectList4.Add((object) objectList27);
                            testTypesAndItsData4.ListofData = objectList4;
                            testTypesAndItsData4.TestName = "Family Types Comparison";
                            continue;
                          }
                          continue;
                        default:
                          continue;
                      }
                    case 25:
                      if (testName == "Compare Family Parameters")
                      {
                        List<MVEEnhancedDetails> mveEnhancedDetailsList = new List<MVEEnhancedDetails>();
                        foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                        Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
                        foreach (MVEEnhancedDetails mveEnhancedDetails in mveEnhancedDetailsList)
                        {
                          foreach (KeyValuePair<string, List<string>> parametersAndValue in mveEnhancedDetails.ParametersAndValues)
                          {
                            if (dictionary4.ContainsKey(parametersAndValue.Key))
                              dictionary4[parametersAndValue.Key] = parametersAndValue.Value.ElementAt<string>(0);
                            else
                              dictionary4.Add(parametersAndValue.Key, parametersAndValue.Value.ElementAt<string>(0));
                          }
                        }
                        if (dictionary4.Keys.Count<string>() == 0)
                        {
                          List<object> objectList28 = new List<object>();
                          objectList28.Add((object) "");
                          bool flag19 = false;
                          foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                          {
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (groupResult.Passed && !groupResult.NotUsed)
                                  objectList28.Add((object) "PASS");
                                else if (!groupResult.Passed && groupResult.NotUsed)
                                {
                                  objectList28.Add((object) "Not Processed");
                                  testTypesAndItsData5.Pass = "Not Processed";
                                  break;
                                }
                              }
                            }
                            if (flag19)
                              break;
                          }
                          objectList5.Add((object) objectList28);
                        }
                        else
                        {
                          foreach (KeyValuePair<string, string> keyValuePair in dictionary4)
                          {
                            List<object> objectList29 = new List<object>();
                            bool flag20 = false;
                            objectList29.Add((object) keyValuePair.Key);
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                            {
                              foreach (TestResult groupResult in groupTestResult.GroupResults)
                              {
                                if (groupResult.TestName.Equals(testResult1.TestName))
                                {
                                  if (groupResult.Passed && groupTestResult.GroupQuantity == num2)
                                  {
                                    if (!flag20)
                                    {
                                      objectList29.Add((object) keyValuePair.Value);
                                      flag20 = true;
                                      break;
                                    }
                                    objectList29.Add((object) keyValuePair.Value);
                                    break;
                                  }
                                  if (!groupResult.Passed && groupResult.NotUsed)
                                  {
                                    objectList29.Add((object) "Not Processed");
                                    testTypesAndItsData5.Pass = "Not Processed";
                                    break;
                                  }
                                  if (!groupResult.Passed && groupResult.FailingStructuralFramingId.Equals((object) groupResult.MVE.IdOfElement))
                                  {
                                    if (groupResult.MVE.ParametersAndValues.ContainsKey(keyValuePair.Key))
                                    {
                                      objectList29.Add((object) groupResult.MVE.ParametersAndValues[keyValuePair.Key].ElementAt<string>(1));
                                      testTypesAndItsData5.Pass = "FAIL";
                                      break;
                                    }
                                    objectList29.Add((object) keyValuePair.Value);
                                    break;
                                  }
                                  objectList29.Add((object) keyValuePair.Value);
                                  break;
                                }
                              }
                            }
                            objectList5.Add((object) objectList29);
                          }
                        }
                        testTypesAndItsData5.ListofData = objectList5;
                        testTypesAndItsData5.TestName = "Compare Family Parameters";
                        continue;
                      }
                      continue;
                    case 29:
                      if (testName == "Compare Main Material Volumes")
                      {
                        List<CompareVolume> compareVolumeList = new List<CompareVolume>();
                        foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                        Dictionary<string, string> dictionary5 = new Dictionary<string, string>();
                        foreach (CompareVolume compareVolume in compareVolumeList)
                        {
                          foreach (KeyValuePair<string, List<string>> parametersAndValue in compareVolume.ParametersAndValues)
                          {
                            if (dictionary5.ContainsKey(parametersAndValue.Key))
                              dictionary5[parametersAndValue.Key] = parametersAndValue.Value.ElementAt<string>(0);
                            else
                              dictionary5.Add(parametersAndValue.Key, parametersAndValue.Value.ElementAt<string>(0));
                          }
                        }
                        if (dictionary5.Keys.Count == 0)
                        {
                          List<object> objectList30 = new List<object>();
                          objectList30.Add((object) "");
                          bool flag21 = false;
                          foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                          {
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (groupResult.Passed && !groupResult.NotUsed)
                                {
                                  objectList30.Add((object) "PASS");
                                  break;
                                }
                                if (!groupResult.Passed)
                                {
                                  if (groupResult.NotUsed)
                                  {
                                    objectList30.Add((object) "Not Processed");
                                    testTypesAndItsData6.Pass = "Not Processed";
                                    break;
                                  }
                                  break;
                                }
                                break;
                              }
                            }
                            if (flag21)
                              break;
                          }
                          objectList6.Add((object) objectList30);
                        }
                        else
                        {
                          foreach (KeyValuePair<string, string> keyValuePair in dictionary5)
                          {
                            List<object> objectList31 = new List<object>();
                            objectList31.Add((object) keyValuePair.Key);
                            bool flag22 = false;
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                            {
                              foreach (TestResult groupResult in groupTestResult.GroupResults)
                              {
                                if (groupResult.TestName.Equals(testResult1.TestName))
                                {
                                  if (groupResult.Passed && groupTestResult.GroupQuantity == num2)
                                  {
                                    if (!flag22)
                                    {
                                      objectList31.Add((object) keyValuePair.Value);
                                      flag22 = true;
                                      break;
                                    }
                                    objectList31.Add((object) keyValuePair.Value);
                                    break;
                                  }
                                  if (!groupResult.Passed && groupResult.NotUsed)
                                  {
                                    objectList31.Add((object) "Not Processed");
                                    testTypesAndItsData6.Pass = "Not Processed";
                                  }
                                  else if (!groupResult.Passed && groupResult.FailingStructuralFramingId.Equals((object) groupResult.CV.IdOfElement))
                                  {
                                    if (groupResult.CV.ParametersAndValues.ContainsKey(keyValuePair.Key))
                                    {
                                      objectList31.Add((object) groupResult.CV.ParametersAndValues[keyValuePair.Key].ElementAt<string>(1));
                                      testTypesAndItsData6.Pass = "FAIL";
                                    }
                                  }
                                  else
                                    objectList31.Add((object) keyValuePair.Value);
                                }
                              }
                            }
                            objectList6.Add((object) objectList31);
                          }
                        }
                        testTypesAndItsData6.ListofData = objectList6;
                        testTypesAndItsData6.TestName = "Compare Main Material Volumes";
                        continue;
                      }
                      continue;
                    case 37:
                      if (testName == "Compare Plate Family Types and Counts")
                      {
                        List<PlateTypes> plateTypesList1 = new List<PlateTypes>();
                        foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                                    bool flag23 = false;
                                    foreach (PlateTypes plateTypes in plateTypesList1)
                                    {
                                      if (plateTypes.FamilyName.Equals(current.FamilyName))
                                        flag23 = true;
                                    }
                                    if (!flag23)
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
                          List<object> objectList32 = new List<object>();
                          objectList32.Add((object) "");
                          bool flag24 = false;
                          foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                          {
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (groupResult.Passed && !groupResult.NotUsed)
                                {
                                  objectList32.Add((object) "PASS");
                                  break;
                                }
                                if (!groupResult.Passed)
                                {
                                  if (groupResult.NotUsed)
                                  {
                                    objectList32.Add((object) "Not Processed");
                                    testTypesAndItsData7.Pass = "Not Processed";
                                    break;
                                  }
                                  break;
                                }
                                break;
                              }
                            }
                            if (flag24)
                              break;
                          }
                          objectList7.Add((object) objectList32);
                        }
                        else
                        {
                          foreach (PlateTypes plateTypes1 in plateTypesList1)
                          {
                            bool flag25 = false;
                            List<object> objectList33 = new List<object>();
                            objectList33.Add((object) plateTypes1.FamilyName);
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                            {
                              if (groupTestResult.GroupResults.Count<TestResult>() == 1)
                              {
                                objectList33.Add((object) "Not Processed");
                                testTypesAndItsData7.Pass = "Not Processed";
                              }
                              else
                              {
                                bool flag26 = false;
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    if (groupResult.Passed)
                                    {
                                      if (!flag25)
                                      {
                                        objectList33.Add((object) plateTypes1.FamilyCount);
                                        flag25 = true;
                                        break;
                                      }
                                      objectList33.Add((object) plateTypes1.FamilyCount);
                                      break;
                                    }
                                    if (!groupResult.Passed && groupResult.NotUsed)
                                    {
                                      objectList33.Add((object) "Not Processed");
                                      testTypesAndItsData7.Pass = "Not Processed";
                                      break;
                                    }
                                    List<PlateTypes> plateTypesList2 = new List<PlateTypes>();
                                    List<PlateTypes> plateTypes2 = groupResult.PlateTypes;
                                    if (groupResult.Passed && !groupResult.NotUsed && plateTypes1.FamilyCount == 0)
                                    {
                                      objectList33.Add((object) 0);
                                      break;
                                    }
                                    foreach (PlateTypes plateTypes3 in plateTypes2)
                                    {
                                      if (plateTypes1.FamilyName.Equals(plateTypes3.FamilyName))
                                      {
                                        flag26 = true;
                                        if (!groupResult.Passed)
                                        {
                                          objectList33.Add((object) plateTypes3.ActualCount);
                                          testTypesAndItsData7.Pass = "FAIL";
                                          break;
                                        }
                                        objectList33.Add((object) plateTypes1.FamilyCount);
                                        break;
                                      }
                                    }
                                    if (!flag26)
                                    {
                                      objectList33.Add((object) plateTypes1.FamilyCount);
                                      break;
                                    }
                                    break;
                                  }
                                }
                              }
                            }
                            objectList7.Add((object) objectList33);
                          }
                        }
                        testTypesAndItsData7.ListofData = objectList7;
                        testTypesAndItsData7.TestName = "Compare Plate Family Types and Counts";
                        continue;
                      }
                      continue;
                    case 48 /*0x30*/:
                      if (testName == "Addon Family Types, Counts, and Material Volumes")
                      {
                        List<dataForAddonTest> dataForAddonTestList1 = new List<dataForAddonTest>();
                        foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
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
                                    bool flag27 = false;
                                    foreach (dataForAddonTest dataForAddonTest in dataForAddonTestList1)
                                    {
                                      if (dataForAddonTest.FamilyTypeName.Equals(current.FamilyTypeName))
                                        flag27 = true;
                                    }
                                    if (!flag27)
                                      dataForAddonTestList1.Add(current);
                                  }
                                  break;
                                }
                              }
                            }
                          }
                        }
                        List<object> objectList34 = new List<object>();
                        List<object> objectList35 = new List<object>();
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
                              List<object> objectList36 = new List<object>();
                              objectList36.Add((object) "");
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                              {
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    objectList36.Add((object) "PASS");
                                    break;
                                  }
                                }
                              }
                              objectList9.Add((object) objectList36);
                            }
                            else
                            {
                              string str = dataForAddonTest1.FamilyTypeName;
                              List<object> objectList37 = new List<object>();
                              if (str.Equals("No Matches Found"))
                                str = dataForAddonTest1.Message;
                              objectList37.Add((object) str);
                              bool flag28 = false;
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                              {
                                bool flag29 = false;
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    bool flag30;
                                    if (groupResult.Passed && groupTestResult.GroupQuantity == num2)
                                    {
                                      if (!flag28)
                                      {
                                        objectList37.Add((object) dataForAddonTest1.FamilyTypeCount);
                                        flag28 = true;
                                      }
                                      else
                                        objectList37.Add((object) dataForAddonTest1.FamilyTypeCount);
                                      flag30 = true;
                                      break;
                                    }
                                    List<dataForAddonTest> dataForAddonTestList4 = new List<dataForAddonTest>();
                                    List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                                    if (!groupResult.Passed && groupResult.NotUsed)
                                    {
                                      flag30 = true;
                                      objectList37.Add((object) "Not Processed");
                                      testTypesAndItsData9.Pass = "Not Processed";
                                      break;
                                    }
                                    if (groupResult.Passed && !groupResult.NotUsed && dataForAddonTest1.FamilyTypeCount == 0)
                                    {
                                      objectList37.Add((object) 0);
                                      break;
                                    }
                                    foreach (dataForAddonTest dataForAddonTest2 in dataForAddOn)
                                    {
                                      if (dataForAddonTest2.AddonPass && dataForAddonTest1.FamilyTypeName.Equals(dataForAddonTest2.FamilyTypeName) && !groupResult.Passed)
                                      {
                                        flag29 = true;
                                        objectList37.Add((object) dataForAddonTest2.ActualCount);
                                        testTypesAndItsData9.Pass = "FAIL";
                                        break;
                                      }
                                    }
                                    if (!flag29)
                                    {
                                      objectList37.Add((object) dataForAddonTest1.FamilyTypeCount);
                                      break;
                                    }
                                    break;
                                  }
                                }
                              }
                              objectList9.Add((object) objectList37);
                            }
                          }
                          foreach (dataForAddonTest dataForAddonTest3 in dataForAddonTestList2)
                          {
                            string str1 = dataForAddonTest3.FamilyTypeName;
                            List<object> objectList38 = new List<object>();
                            bool flag31 = false;
                            if (str1.Equals("No Matches Found"))
                              str1 = dataForAddonTest3.Message;
                            objectList38.Add((object) str1);
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                            {
                              bool flag32 = false;
                              foreach (TestResult groupResult in groupTestResult.GroupResults)
                              {
                                if (groupResult.TestName.Equals(testResult1.TestName))
                                {
                                  if (groupResult.Passed && groupTestResult.GroupQuantity == num2)
                                  {
                                    if (!flag31)
                                    {
                                      objectList38.Add((object) "BASELINE");
                                      flag31 = true;
                                      break;
                                    }
                                    objectList38.Add((object) "PASS");
                                    break;
                                  }
                                  List<dataForAddonTest> dataForAddonTestList5 = new List<dataForAddonTest>();
                                  List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                                  if (!groupResult.Passed && groupResult.NotUsed)
                                  {
                                    objectList38.Add((object) "Not Processed");
                                    testTypesAndItsData10.Pass = "Not Processed";
                                    break;
                                  }
                                  if (dataForAddOn != null)
                                  {
                                    foreach (dataForAddonTest dataForAddonTest4 in dataForAddOn)
                                    {
                                      if (dataForAddonTest4.AddonPass && dataForAddonTest3.FamilyTypeName.Equals(dataForAddonTest4.FamilyTypeName))
                                      {
                                        string str2 = "";
                                        flag32 = true;
                                        if (!groupResult.Passed)
                                        {
                                          testTypesAndItsData10.Pass = "FAIL";
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
                                          objectList38.Add((object) str2);
                                          break;
                                        }
                                        objectList38.Add((object) "PASS");
                                        break;
                                      }
                                    }
                                  }
                                  if (!flag32)
                                  {
                                    objectList38.Add((object) "PASS");
                                    break;
                                  }
                                  break;
                                }
                              }
                            }
                            objectList10.Add((object) objectList38);
                          }
                        }
                        else
                        {
                          List<object> objectList39 = new List<object>();
                          objectList39.Add((object) "");
                          bool flag33 = false;
                          List<object> objectList40 = new List<object>();
                          objectList40.Add((object) "");
                          foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                          {
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (!groupResult.Passed && groupResult.NotUsed)
                                {
                                  testTypesAndItsData9.Pass = "Not Processed";
                                  testTypesAndItsData10.Pass = "Not Processed";
                                  objectList39.Add((object) "Not Processed");
                                  objectList40.Add((object) "Not Processed");
                                  break;
                                }
                                objectList39.Add((object) "PASS");
                                objectList40.Add((object) "PASS");
                                break;
                              }
                            }
                            if (flag33)
                              break;
                          }
                          objectList9.Add((object) objectList39);
                          objectList10.Add((object) objectList40);
                        }
                        if (dataForAddonTestList3.Count > 0)
                        {
                          foreach (dataForAddonTest dataForAddonTest5 in dataForAddonTestList3)
                          {
                            if (!dataForAddonTest5.VolumePass && dataForAddonTest5.ActualCount == dataForAddonTest5.FamilyTypeCount)
                            {
                              List<object> objectList41 = new List<object>();
                              objectList41.Add((object) "");
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                              {
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    objectList41.Add((object) "PASS");
                                    break;
                                  }
                                }
                              }
                              objectList12.Add((object) objectList41);
                            }
                            else
                            {
                              string str = dataForAddonTest5.FamilyTypeName;
                              List<object> objectList42 = new List<object>();
                              bool flag34 = false;
                              if (str.Equals("No Matches Found"))
                                str = dataForAddonTest5.Message;
                              objectList42.Add((object) str);
                              foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                              {
                                bool flag35 = false;
                                foreach (TestResult groupResult in groupTestResult.GroupResults)
                                {
                                  if (groupResult.TestName.Equals(testResult1.TestName))
                                  {
                                    if (groupResult.Passed && groupTestResult.GroupQuantity == num2)
                                    {
                                      if (!flag34)
                                      {
                                        objectList42.Add((object) dataForAddonTest5.FamilyTypeCount);
                                        flag34 = true;
                                        break;
                                      }
                                      objectList42.Add((object) dataForAddonTest5.FamilyTypeCount);
                                      break;
                                    }
                                    List<dataForAddonTest> dataForAddonTestList6 = new List<dataForAddonTest>();
                                    List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                                    if (!groupResult.Passed && groupResult.NotUsed)
                                    {
                                      objectList42.Add((object) "Not Processed");
                                      testTypesAndItsData12.Pass = "Not Processed";
                                      break;
                                    }
                                    if (dataForAddOn != null)
                                    {
                                      foreach (dataForAddonTest dataForAddonTest6 in dataForAddOn)
                                      {
                                        if (!dataForAddonTest6.AddonPass && dataForAddonTest5.FamilyTypeName.Equals(dataForAddonTest6.FamilyTypeName) && !groupResult.Passed)
                                        {
                                          objectList42.Add((object) dataForAddonTest6.ActualCount);
                                          flag35 = true;
                                          testTypesAndItsData12.Pass = "FAIL";
                                          break;
                                        }
                                      }
                                    }
                                    if (!flag35)
                                    {
                                      objectList42.Add((object) dataForAddonTest5.FamilyTypeCount);
                                      break;
                                    }
                                    break;
                                  }
                                }
                              }
                              objectList12.Add((object) objectList42);
                            }
                          }
                          foreach (dataForAddonTest dataForAddonTest7 in dataForAddonTestList3)
                          {
                            string str3 = dataForAddonTest7.FamilyTypeName;
                            bool flag36 = false;
                            List<object> objectList43 = new List<object>();
                            if (str3.Equals("No Matches Found"))
                              str3 = dataForAddonTest7.Message;
                            objectList43.Add((object) str3);
                            foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                            {
                              bool flag37 = false;
                              foreach (TestResult groupResult in groupTestResult.GroupResults)
                              {
                                if (groupResult.TestName.Equals(testResult1.TestName))
                                {
                                  if (groupResult.Passed && groupTestResult.GroupQuantity == num2)
                                  {
                                    if (!flag36)
                                    {
                                      objectList43.Add((object) "BASELINE");
                                      flag36 = true;
                                      break;
                                    }
                                    objectList43.Add((object) "PASS");
                                    break;
                                  }
                                  List<dataForAddonTest> dataForAddonTestList7 = new List<dataForAddonTest>();
                                  List<dataForAddonTest> dataForAddOn = groupResult.DataForAddOn;
                                  if (!groupResult.Passed && groupResult.NotUsed)
                                  {
                                    objectList43.Add((object) "Not Processed");
                                    testTypesAndItsData13.Pass = "Not Processed";
                                    break;
                                  }
                                  if (dataForAddOn != null)
                                  {
                                    foreach (dataForAddonTest dataForAddonTest8 in dataForAddOn)
                                    {
                                      if (!dataForAddonTest8.AddonPass && dataForAddonTest7.FamilyTypeName.Equals(dataForAddonTest8.FamilyTypeName))
                                      {
                                        string str4 = "";
                                        flag37 = true;
                                        if (!groupResult.Passed)
                                        {
                                          testTypesAndItsData13.Pass = "FAIL";
                                          if (dataForAddonTest8.MismatchedId.Count<ElementId>() == 0 && dataForAddonTest8.ListforActual.Count<ElementId>() == 0)
                                            str4 = "No Match";
                                          else if (dataForAddonTest8.MismatchedId.Count<ElementId>() > 0)
                                            str4 = string.Join<ElementId>("; ", (IEnumerable<ElementId>) dataForAddonTest8.MismatchedId);
                                          else if (dataForAddonTest8.FamilyTypeCount != dataForAddonTest8.ActualCount)
                                          {
                                            if (dataForAddonTest8.VolumePass)
                                            {
                                              str4 = "Count Mismatch";
                                            }
                                            else
                                            {
                                              str4 += "Count Mismatch: ";
                                              foreach (ElementId elementId in dataForAddonTest8.ListforExpected)
                                                str4 = !dataForAddonTest8.ListforExpected.Last<ElementId>().Equals((object) elementId) ? $"{str4}{elementId.ToString()}; " : str4 + elementId.ToString();
                                            }
                                          }
                                          objectList43.Add((object) str4);
                                          break;
                                        }
                                        objectList43.Add((object) "PASS");
                                        break;
                                      }
                                    }
                                  }
                                  if (!flag37)
                                  {
                                    objectList43.Add((object) "PASS");
                                    break;
                                  }
                                  break;
                                }
                              }
                            }
                            objectList14.Add((object) objectList43);
                          }
                        }
                        else
                        {
                          List<object> objectList44 = new List<object>();
                          objectList44.Add((object) "");
                          bool flag38 = false;
                          List<object> objectList45 = new List<object>();
                          objectList45.Add((object) "");
                          foreach (GroupTestResult groupTestResult in markResult.GroupTestResults)
                          {
                            foreach (TestResult groupResult in groupTestResult.GroupResults)
                            {
                              if (groupResult.TestName.Equals(testResult1.TestName))
                              {
                                if (!groupResult.Passed && groupResult.NotUsed)
                                {
                                  objectList44.Add((object) "Not Processed");
                                  objectList45.Add((object) "Not Processed");
                                  testTypesAndItsData12.Pass = "Not Processed";
                                  testTypesAndItsData13.Pass = "Not Processed";
                                  break;
                                }
                                objectList44.Add((object) "PASS");
                                objectList45.Add((object) "PASS");
                                break;
                              }
                            }
                            if (flag38)
                              break;
                          }
                          objectList12.Add((object) objectList44);
                          objectList14.Add((object) objectList45);
                        }
                        testTypesAndItsData9.ListofData = objectList9;
                        testTypesAndItsData9.TestName = "Addon Family Count Comparison";
                        testTypesAndItsData10.ListofData = objectList10;
                        testTypesAndItsData10.TestName = "Addon Volume Comparison";
                        testTypesAndItsData12.ListofData = objectList12;
                        testTypesAndItsData12.TestName = "Finish Family Count Comparison";
                        testTypesAndItsData13.ListofData = objectList14;
                        testTypesAndItsData13.TestName = "Finish Volume Comparison";
                        continue;
                      }
                      continue;
                    default:
                      continue;
                  }
                }
              }
              if (string.IsNullOrEmpty(key1))
                key1 = "empty";
              dictionary1.Add(key1, new List<storingTestTypesAndItsData>()
              {
                testTypesAndItsData1
              });
              dictionary1[key1].Add(testTypesAndItsData2);
              dictionary1[key1].Add(testTypesAndItsData3);
              dictionary1[key1].Add(testTypesAndItsData4);
              dictionary1[key1].Add(testTypesAndItsData5);
              dictionary1[key1].Add(testTypesAndItsData6);
              dictionary1[key1].Add(testTypesAndItsData7);
              dictionary1[key1].Add(testTypesAndItsData8);
              dictionary1[key1].Add(testTypesAndItsData9);
              dictionary1[key1].Add(testTypesAndItsData10);
              dictionary1[key1].Add(testTypesAndItsData11);
              dictionary1[key1].Add(testTypesAndItsData12);
              dictionary1[key1].Add(testTypesAndItsData13);
              dictionary1[key1].Add(testTypesAndItsData14);
              dictionary1[key1].Add(testTypesAndItsData15);
              if (markResult.FailedRotationList != null && markResult.FailedRotationList.Count<Plates>() > 0)
              {
                testTypesAndItsData16.Pass = "TRUE";
                testTypesAndItsData16.TestName = "Rotation Warnings (Warnings are per control mark not detail group.)";
                foreach (Plates failedRotation in markResult.FailedRotationList)
                {
                  List<object> objectList46 = new List<object>();
                  objectList46.Add((object) failedRotation.Names);
                  string str = "";
                  foreach (ElementId id in failedRotation.Ids)
                    str = !id.Equals((object) failedRotation.Ids.Last<ElementId>()) ? $"{str}{id.ToString()}; " : str + id.ToString();
                  objectList46.Add((object) str);
                  if (testTypesAndItsData16.ListofData == null)
                  {
                    testTypesAndItsData16.ListofData = new List<object>();
                    testTypesAndItsData16.ListofData.Add((object) objectList46);
                  }
                  else
                    testTypesAndItsData16.ListofData.Add((object) objectList46);
                }
                dictionary1[key1].Add(testTypesAndItsData16);
              }
            }
          }
        }
        bool flag = false;
        List<Dictionary<string, List<storingTestTypesAndItsData>>> dictionaryList = new List<Dictionary<string, List<storingTestTypesAndItsData>>>();
        Dictionary<string, List<storingTestTypesAndItsData>> source1 = new Dictionary<string, List<storingTestTypesAndItsData>>();
        foreach (string key in dictionary1.Keys)
        {
          if (dictionary1[key].Count > 1)
            source1.Add(key, dictionary1[key]);
        }
        if (source1.Keys.Count > (int) byte.MaxValue)
        {
          if (!new TaskDialog("Mark Verification Existing")
          {
            MainInstruction = "There were more than 255 failing control marks found in this model to be exported. The exporter would like to create separate files for all marks exceeding this limit. Do you wish to continue ?",
            CommonButtons = ((TaskDialogCommonButtons) 6)
          }.Show().Equals((object) (TaskDialogResult) 6))
            return;
          flag = true;
          int counter = 0;
          foreach (Dictionary<string, List<storingTestTypesAndItsData>> dictionary6 in source1.GroupBy<KeyValuePair<string, List<storingTestTypesAndItsData>>, int>((Func<KeyValuePair<string, List<storingTestTypesAndItsData>>, int>) (x => counter++ / 254)).Select<IGrouping<int, KeyValuePair<string, List<storingTestTypesAndItsData>>>, Dictionary<string, List<storingTestTypesAndItsData>>>((Func<IGrouping<int, KeyValuePair<string, List<storingTestTypesAndItsData>>>, Dictionary<string, List<storingTestTypesAndItsData>>>) (g => g.ToDictionary<KeyValuePair<string, List<storingTestTypesAndItsData>>, string, List<storingTestTypesAndItsData>>((Func<KeyValuePair<string, List<storingTestTypesAndItsData>>, string>) (h => h.Key), (Func<KeyValuePair<string, List<storingTestTypesAndItsData>>, List<storingTestTypesAndItsData>>) (h => h.Value)))))
            dictionaryList.Add(dictionary6);
        }
        else
          dictionaryList.Add(dictionary1);
        string str5 = "";
        int num4 = 1;
        foreach (Dictionary<string, List<storingTestTypesAndItsData>> dictionary7 in dictionaryList)
        {
          if (flag)
          {
            string source2 = ((IEnumerable<string>) saveFileDialog2.SafeFileName.Split('.')).ElementAt<string>(0);
            if (char.IsNumber(source2.Last<char>()))
            {
              string newName = source2.Remove(source2.Length - 1) + num4.ToString();
              saveFileDialog2.FileName = this.fileName(saveFileDialog2.FileName, newName);
              str5 = dictionaryList.Count != num4 ? $"{str5}{saveFileDialog2.FileName}; \n" : str5 + saveFileDialog2.FileName;
            }
            else
            {
              string newName = source2 + num4.ToString();
              saveFileDialog2.FileName = this.fileName(saveFileDialog2.FileName, newName);
              str5 = $"{str5}{saveFileDialog2.FileName}; \n";
            }
            fileName = saveFileDialog2.FileName;
            ++num4;
          }
          else
            str5 = saveFileDialog2.FileName;
          List<object[,]> objArrayList = new List<object[,]>();
          object[,] objArray1 = new object[dictionary1.Count + 1, 14];
          objArray1[0, 0] = (object) "Control Marks";
          objArray1[0, 1] = (object) "Family Types";
          objArray1[0, 2] = (object) "Family Parameters";
          objArray1[0, 3] = (object) "Main Material Volumes";
          objArray1[0, 4] = (object) "Plate Family Types and Counts";
          objArray1[0, 5] = (object) "Plate Location Comparison (results provides Element ID for offending items)";
          objArray1[0, 6] = (object) "Addon Family Count";
          objArray1[0, 7] = (object) "Addon Volume";
          objArray1[0, 8] = (object) "Addon Location";
          objArray1[0, 9] = (object) "Finish Family Count";
          objArray1[0, 10] = (object) "Finish Volume";
          objArray1[0, 11] = (object) "Finish Location";
          objArray1[0, 12] = (object) "Geometry";
          objArray1[0, 13] = (object) "Rotation Warning";
          int index1 = 1;
          foreach (string key in dictionary1.Keys)
          {
            List<storingTestTypesAndItsData> testTypesAndItsDataList = new List<storingTestTypesAndItsData>();
            List<storingTestTypesAndItsData> source3 = !dictionary7.ContainsKey(key) ? dictionary1[key] : dictionary7[key];
            string str6;
            if (source3.Count == 1)
              str6 = !key.Contains("empty") ? key : "";
            else if (dictionary7.ContainsKey(key))
            {
              if (key.Contains("empty"))
                str6 = "";
              else
                str6 = $"=HYPERLINK(\"[{saveFileDialog2.SafeFileName}]{key}!A1\",\"{key}\")";
            }
            else
              str6 = key;
            objArray1[index1, 0] = (object) str6;
            for (int index2 = 0; index2 < 13; ++index2)
            {
              string str7 = source3.Count != 1 ? (index2 != 12 || source3.Count != 15 ? source3.ElementAt<storingTestTypesAndItsData>(index2 + 3).Pass.ToString() : "FALSE") : (index2 != 12 ? "PASS" : "FALSE");
              objArray1[index1, index2 + 1] = (object) str7;
            }
            ++index1;
          }
          objArrayList.Add(objArray1);
          Dictionary<string, List<int>> dictionary8 = new Dictionary<string, List<int>>();
          foreach (string key in dictionary7.Keys)
          {
            List<storingTestTypesAndItsData> source4 = dictionary7[key];
            if (source4.Count > 1)
            {
              List<int> intList = new List<int>();
              int length = 0;
              int index3 = 0;
              foreach (storingTestTypesAndItsData testTypesAndItsData in source4)
                length += testTypesAndItsData.ListofData.Count + 1;
              int count = (source4.First<storingTestTypesAndItsData>().ListofData.First<object>() as List<object>).Count;
              object[,] objArray2 = new object[length, count];
              for (int index4 = 0; index4 < source4.Count; ++index4)
              {
                List<object> listofData = source4[index4].ListofData;
                objArray2[index3, 0] = (object) source4[index4].TestName;
                intList.Add(index3 + 1);
                ++index3;
                foreach (List<object> source5 in listofData)
                {
                  for (int index5 = 0; index5 < source5.Count; ++index5)
                    objArray2[index3, index5] = source5.ElementAt<object>(index5);
                  ++index3;
                }
              }
              dictionary8.Add(key, intList);
              objArrayList.Add(objArray2);
            }
            ++index1;
          }
          ExcelDocument excelDocument = new ExcelDocument(fileName);
          ExcelFont newExcelFont = new ExcelFont();
          newExcelFont.bold = true;
          ExcelFill newExcelFill = new ExcelFill();
          newExcelFill.Pattern = PatternValues.Solid;
          newExcelFill.SetBackgroundColor(211, 211, 211);
          int num5 = 1;
          foreach (object[,] objArray3 in objArrayList)
          {
            List<int> intList = new List<int>();
            SheetData sheetData = (SheetData) null;
            if (num5 == 1)
            {
              sheetData = excelDocument.GetSheet("CoverSheet");
              for (int columnId = 1; columnId < 15; ++columnId)
              {
                excelDocument.UpdateCellFont(columnId, 1, newExcelFont, sheetData);
                excelDocument.UpdateCellFill(columnId, 1, newExcelFill, sheetData);
              }
            }
            else
            {
              string str8 = dictionary8.Keys.ElementAt<string>(num5 - 2);
              intList = dictionary8[str8];
              string str9 = this.removeSpecialCharacters(str8);
              if (!string.IsNullOrEmpty(str9))
                sheetData = str9.Contains("empty") ? excelDocument.GetSheet("Sheet" + num5.ToString()) : excelDocument.GetSheet(str8);
            }
            if (sheetData == null)
            {
              ++num5;
            }
            else
            {
              for (int index6 = 0; index6 < objArray3.GetLength(0); ++index6)
              {
                for (int index7 = 0; index7 < objArray3.GetLength(1); ++index7)
                {
                  object obj = objArray3[index6, index7];
                  switch (obj)
                  {
                    case string str11:
                      if (!string.IsNullOrWhiteSpace(str11))
                      {
                        excelDocument.UpdateCellValue(index7 + 1, index6 + 1, str11, ExcelEnums.ExcelCellFormat.General, sheetData);
                        if (str11.Contains("=HYPERLINK"))
                          excelDocument.UpdateCellStyle(index7 + 1, index6 + 1, ExcelEnums.ExcelCellStyles.Hyperlink, sheetData);
                        excelDocument.UpdateCellAlignment(index7 + 1, index6 + 1, HorizontalAlignmentValues.Center, sheetData);
                        break;
                      }
                      break;
                    case int num6:
                      excelDocument.UpdateCellValue(index7 + 1, index6 + 1, (double) num6, ExcelEnums.ExcelCellFormat.General, sheetData);
                      excelDocument.UpdateCellAlignment(index7 + 1, index6 + 1, HorizontalAlignmentValues.Center, sheetData);
                      break;
                    case double num7:
                      excelDocument.UpdateCellValue(index7 + 1, index6 + 1, num7, ExcelEnums.ExcelCellFormat.General, sheetData);
                      excelDocument.UpdateCellAlignment(index7 + 1, index6 + 1, HorizontalAlignmentValues.Center, sheetData);
                      break;
                    default:
                      ElementId elementId = obj as ElementId;
                      if ((object) elementId != null)
                      {
                        string str10 = elementId.ToString();
                        excelDocument.UpdateCellValue(index7 + 1, index6 + 1, str10, ExcelEnums.ExcelCellFormat.General, sheetData);
                        excelDocument.UpdateCellAlignment(index7 + 1, index6 + 1, HorizontalAlignmentValues.Center, sheetData);
                        break;
                      }
                      break;
                  }
                }
              }
              if (intList.Count > 0)
              {
                int length = objArray3.GetLength(1);
                foreach (int RowId in intList)
                {
                  excelDocument.UpdateRowFont(RowId, newExcelFont, sheetData);
                  excelDocument.UpdateRowFill(RowId, newExcelFill, sheetData);
                  string columnId1 = excelDocument.getColumnId(1);
                  if (!string.IsNullOrWhiteSpace(columnId1))
                  {
                    string columnId2 = excelDocument.getColumnId(length);
                    if (!string.IsNullOrEmpty(columnId2))
                    {
                      string firstCellReference = columnId1 + RowId.ToString();
                      string secondCellReference = columnId2 + RowId.ToString();
                      excelDocument.MergeCells(firstCellReference, secondCellReference, sheetData);
                    }
                    else
                      break;
                  }
                  else
                    break;
                }
              }
              excelDocument.UpdateAllColumnsWidthBestFit(sheetData);
              ++num5;
            }
          }
          excelDocument.SaveAndClose();
        }
        new TaskDialog("Mark Verification Existing")
        {
          MainInstruction = "The file has been successfully exported",
          ExpandedContent = str5
        }.Show();
      }
      catch (Exception ex)
      {
        if (ex == null)
          return;
        if (ex.ToString().Contains("because it is being used by another process") && ex.ToString().Contains("The process cannot access the file"))
        {
          int num8 = (int) MessageBox.Show("The file you attempted to save is being used by another process. Please close the file before saving it.", "Warning");
        }
        else
        {
          TaskDialog.Show("Warning", ex.ToString());
          throw;
        }
      }
    }
    else
    {
      try
      {
        SaveFileDialog saveFileDialog3 = new SaveFileDialog();
        saveFileDialog3.AddExtension = true;
        saveFileDialog3.DefaultExt = ".xlsx";
        saveFileDialog3.Filter = "Excel Files (*.xlsx)|*.xlsx";
        SaveFileDialog saveFileDialog4 = saveFileDialog3;
        if (!saveFileDialog4.ShowDialog().GetValueOrDefault())
          return;
        string fileName = saveFileDialog4.FileName;
        List<List<object>> source = new List<List<object>>();
        List<object> objectList47 = new List<object>();
        List<bool> boolList = new List<bool>();
        source.Add(new List<object>()
        {
          (object) "ELEMENT ID",
          (object) "CONTROL MARKS",
          (object) "CONTROL NUMBER",
          (object) "RESULTS",
          (object) "FAILED TEST",
          (object) "GROUP ID"
        });
        boolList.Add(false);
        int num9 = 0;
        foreach (MarkResult result in (IEnumerable<MarkResult>) this._results)
        {
          foreach (GroupTestResult groupTestResult in result.GroupTestResults)
          {
            foreach (FamilyInstance groupMember in groupTestResult.GroupMembers)
            {
              List<object> objectList48 = new List<object>();
              objectList48.Add((object) groupMember.Id.ToString());
              objectList48.Add((object) Utils.ElementUtils.Parameters.GetParameterAsString((Element) groupMember, "CONTROL_MARK"));
              objectList48.Add((object) Utils.ElementUtils.Parameters.GetParameterAsString((Element) groupMember, "CONTROL_NUMBER"));
              if (groupTestResult.GroupResults.Count > 0)
              {
                if (groupTestResult.GroupResults.Last<TestResult>().Passed)
                  objectList48.Add((object) "PASS");
                else
                  objectList48.Add((object) "FAIL");
                boolList.Add(!groupTestResult.GroupResults.Last<TestResult>().Passed);
              }
              else
              {
                objectList48.Add((object) "");
                boolList.Add(false);
              }
              source.Add(objectList48);
              if (groupTestResult.GroupResults.Count > 0 && !groupTestResult.GroupResults.Last<TestResult>().Passed)
                objectList48.Add((object) groupTestResult.GroupResults.Last<TestResult>().TestName);
              else
                objectList48.Add((object) "");
              objectList48.Add((object) num9);
            }
            ++num9;
          }
        }
        ExcelDocument excelDocument = new ExcelDocument(fileName);
        if (excelDocument != null)
        {
          int num10 = ((IEnumerable<object>) ((IEnumerable<object>) source).ToArray<object>()).Count<object>();
          int num11 = 1;
          foreach (List<object> objectList49 in source)
          {
            if (num11 <= num10)
            {
              for (int index = 0; index < objectList49.Count; ++index)
              {
                if (objectList49[index] is string str)
                  excelDocument.UpdateCellValue(index + 1, num11, str);
                else if (objectList49[index] is int num12)
                  excelDocument.UpdateCellValue(index + 1, num11, (double) num12);
              }
              if (boolList[num11 - 1])
              {
                ExcelFill newExcelFill = new ExcelFill();
                newExcelFill.Pattern = PatternValues.Solid;
                newExcelFill.SetBackgroundColor((int) byte.MaxValue, 0, 0);
                excelDocument.UpdateRowFill(num11, newExcelFill);
              }
              ++num11;
            }
          }
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
  }

  private string fileName(string originalName, string newName)
  {
    string[] source = originalName.Split('\\');
    string str1 = "";
    foreach (string str2 in source)
      str1 = !((IEnumerable<string>) source).Last<string>().Equals(str2) ? $"{str1}{str2}\\" : $"{str1}{newName}.xlsx";
    return str1;
  }

  private string removeSpecialCharacters(string value)
  {
    string str1 = "";
    for (int index = 0; index < value.Length; ++index)
    {
      string str2 = value[index].ToString().Replace("[", "").Replace("]", "").Replace("/", "").Replace("\\", "").Replace(":", "").Replace("?", "").Replace("*", "");
      str1 += str2;
    }
    return str1.Length > 31 /*0x1F*/ ? str1.Substring(0, 30) : str1;
  }

  private void WarningsBUtton_Click(object sender, RoutedEventArgs e)
  {
    if (this.lstResults.SelectedItem == null)
      return;
    new PlateWarnings(this.lstResults.SelectedItem as MarkResult, this.uiApp, Process.GetCurrentProcess().MainWindowHandle, this).Show();
    this.WarningsBUtton.IsEnabled = false;
  }

  private void lstResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    if (this.lstResults.SelectedItem == null)
      return;
    MarkResult selectedItem = this.lstResults.SelectedItem as MarkResult;
    List<ElementId> elementIdList = new List<ElementId>();
    if (selectedItem.GroupTestResults != null)
    {
      foreach (GroupTestResult groupTestResult in selectedItem.GroupTestResults)
      {
        if (groupTestResult.GroupResults != null)
        {
          foreach (MemberDetails groupDetail in groupTestResult.GroupDetails)
          {
            ElementId id = ElementId.Parse(groupDetail.ElementId);
            try
            {
              if (this.uiDoc.Document.GetElement(id).GetTopLevelElement().IsValidObject)
                elementIdList.Add(id);
            }
            catch (Exception ex)
            {
            }
          }
        }
      }
    }
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/markverification/resultspresentation/mkverificationresults_existing.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Window) target).Activated += new EventHandler(this.App_Activated);
        break;
      case 2:
        this.Passed = (Button) target;
        this.Passed.Click += new RoutedEventHandler(this.btnpass_Click);
        break;
      case 3:
        this.Failed = (Button) target;
        this.Failed.Click += new RoutedEventHandler(this.btnfail_Click);
        break;
      case 4:
        this.All = (Button) target;
        this.All.Click += new RoutedEventHandler(this.btnall_Click);
        break;
      case 5:
        this.SearchBox = (TextBox) target;
        this.SearchBox.TextChanged += new TextChangedEventHandler(this.SearchBox_TextChanged);
        break;
      case 6:
        this.lstResults = (ListBox) target;
        this.lstResults.SelectionChanged += new SelectionChangedEventHandler(this.lstResults_SelectionChanged);
        this.lstResults.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.lstResults_PreviewMouseLeftButtonDown);
        break;
      case 7:
        this.gridofresults = (System.Windows.Controls.Grid) target;
        break;
      case 8:
        this.lstTestResults = (ListBox) target;
        this.lstTestResults.SelectionChanged += new SelectionChangedEventHandler(this.lstTestResults_SelectionChanged);
        break;
      case 9:
        this.DetailButton = (Button) target;
        this.DetailButton.Click += new RoutedEventHandler(this.DetailButton_Click);
        break;
      case 10:
        this.WarningsBUtton = (Button) target;
        this.WarningsBUtton.Click += new RoutedEventHandler(this.WarningsBUtton_Click);
        break;
      case 11:
        this.ExportButton = (Button) target;
        this.ExportButton.Click += new RoutedEventHandler(this.ExportButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
