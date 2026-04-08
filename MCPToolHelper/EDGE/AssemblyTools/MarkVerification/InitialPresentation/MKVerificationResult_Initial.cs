// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.InitialPresentation.MKVerificationResult_Initial
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
namespace EDGE.AssemblyTools.MarkVerification.InitialPresentation;

public class MKVerificationResult_Initial : Window, IComponentConnector
{
  private UIDocument uiDoc;
  private UIApplication uiApp;
  private ICollection<InitialResult> _results;
  public SortedDictionary<string, MarkGroupResultData> Dictionary;
  private List<string> illegalCharacters;
  private string previousTitle;
  private string previousPath;
  public ExternalEvent AcceptChangesEvent;
  public string userDefinedControlMark = "";
  public InitialResult resultToshow = new InitialResult();
  internal ListBox lstResults;
  internal Border deatilBorder;
  internal System.Windows.Controls.Grid detailPanel;
  internal TextBox MarkNumberBox;
  internal Button btnSave;
  internal ListBox lstTestResults;
  internal Button btnApply;
  internal Button btnExport;
  private bool _contentLoaded;

  public MKVerificationResult_Initial(
    ExternalEvent[] externalEvents,
    SortedDictionary<string, MarkGroupResultData> dictionary,
    UIApplication uiapp,
    IntPtr parentWindowHandler,
    bool bUserIsAllowedToModifyGeometry,
    Document revitDoc)
  {
    this.InitializeComponent();
    this.uiApp = uiapp;
    this.uiDoc = this.uiApp.ActiveUIDocument;
    this.previousTitle = this.uiApp.ActiveUIDocument.Document.Title;
    this.previousPath = this.uiApp.ActiveUIDocument.Document.PathName;
    this.AcceptChangesEvent = externalEvents[0];
    this.btnSave.IsEnabled = bUserIsAllowedToModifyGeometry;
    this.btnApply.IsEnabled = bUserIsAllowedToModifyGeometry;
    this.MarkNumberBox.IsEnabled = bUserIsAllowedToModifyGeometry;
    this.illegalCharacters = new List<string>()
    {
      "<",
      ">",
      ";",
      ":",
      "[",
      "]",
      "{",
      "}",
      "\\",
      "|",
      "'",
      "~",
      "?"
    };
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    ObservableCollection<InitialResult> observableCollection = new ObservableCollection<InitialResult>();
    this.Dictionary = dictionary;
    foreach (KeyValuePair<string, MarkGroupResultData> keyValuePair in dictionary)
    {
      InitialResult result = new InitialResult();
      result.Bucket = keyValuePair.Key;
      result.Count = keyValuePair.Value.GroupMembers.Count;
      result.InitialPlateRotated = keyValuePair.Value.FailedPlateRotationComparison;
      result.UseCountMultiplier = keyValuePair.Value.CountMultiplierExcluded;
      List<InitialResult.DetailResult> detailResultList = new List<InitialResult.DetailResult>();
      foreach (FamilyInstance groupMember in keyValuePair.Value.GroupMembers)
      {
        string parameterAsString = Parameters.GetParameterAsString((Element) groupMember, "CONTROL_NUMBER");
        ElementId id = groupMember.Id;
        if (groupMember.SuperComponent != null)
          id = groupMember.SuperComponent.Id;
        InitialResult.DetailResult detailResult = new InitialResult.DetailResult(id, revitDoc.GetElement(id).Name, parameterAsString, Parameters.GetParameterAsString((Element) groupMember, "CONTROL_MARK"), Parameters.GetParameterAsString((Element) groupMember, "CONSTRUCTION_PRODUCT"));
        detailResultList.Add(detailResult);
      }
      result.DetailedResults = detailResultList;
      observableCollection.Add(new InitialResult(result));
    }
    this._results = (ICollection<InitialResult>) observableCollection;
    this.lstResults.ItemsSource = (IEnumerable) this._results;
  }

  private void App_Activated(object sender, EventArgs e)
  {
    if (this.uiApp == null || this.uiApp.ActiveUIDocument == null)
    {
      App.MarkVerificationInitialWindow.Close();
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
      App.MarkVerificationInitialWindow.Close();
    }
  }

  private void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    bool flag = false;
    foreach (string illegalCharacter in this.illegalCharacters)
    {
      if (this.userDefinedControlMark.Contains(illegalCharacter))
      {
        flag = true;
        int num = (int) MessageBox.Show("Please check your input. The new mark number contains one or more following illegal characters <>;:[]{}\\|'~? ", "Warning");
        break;
      }
    }
    if (flag)
      return;
    foreach (InitialResult.DetailResult detailedResult in this.resultToshow.DetailedResults)
    {
      Element element = this.uiDoc.Document.GetElement(this.uiDoc.Document.GetElement(detailedResult.ElementId).AssemblyInstanceId);
      if (element is AssemblyInstance && Parameters.GetParameterAsString((Element) (element as AssemblyInstance), "ASSEMBLY_MARK_NUMBER") != this.userDefinedControlMark)
      {
        flag = true;
        if (new TaskDialog("Warning")
        {
          MainInstruction = $"You have entered a new Control Mark for a structural framing element that is already part of an assembly. Saving these changes will cause the control mark of the assembly to be out of date with the structural framing element. {Environment.NewLine}To fix this, update the ASSEMBLY_MARK_NUMBER parameter for the affected assembly.{Environment.NewLine}Do you wish to save these changes?",
          CommonButtons = ((TaskDialogCommonButtons) 6)
        }.Show().Equals((object) (TaskDialogResult) 6))
        {
          this.AcceptChangesEvent.Raise();
          break;
        }
        break;
      }
    }
    if (flag)
      return;
    this.AcceptChangesEvent.Raise();
  }

  private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    this.userDefinedControlMark = this.getContent(sender.ToString());
  }

  private string getContent(string txt)
  {
    if (!txt.Contains(":"))
      return "";
    int num = txt.IndexOf(":");
    txt = txt.Substring(num + 2);
    int length = txt.Length;
    txt = txt.Substring(0, length);
    return txt;
  }

  private void lstResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    this.deatilBorder.Visibility = System.Windows.Visibility.Visible;
    this.detailPanel.Visibility = System.Windows.Visibility.Visible;
    this.MarkNumberBox.Text = "";
    IList selectedItems = ((ListBox) sender).SelectedItems;
    if (selectedItems.Count > 0)
      this.resultToshow = (InitialResult) selectedItems[0];
    try
    {
      this.HighlightElements();
    }
    catch (Exception ex)
    {
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
    }
  }

  private void HighlightElements()
  {
    if (this.resultToshow != null)
    {
      if (this.resultToshow.DetailedResults == null)
        return;
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) this.resultToshow.DetailedResults.Select<InitialResult.DetailResult, ElementId>((Func<InitialResult.DetailResult, ElementId>) (s => this.uiDoc.Document.GetElement(s.ElementId).GetTopLevelElement().Id)).ToList<ElementId>());
    }
    else
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
  }

  private void ApplyButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void lstTestResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    List<InitialResult.DetailResult> list = ((ListBox) sender).SelectedItems.Cast<InitialResult.DetailResult>().ToList<InitialResult.DetailResult>();
    if (list.Any<InitialResult.DetailResult>())
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) list.Select<InitialResult.DetailResult, ElementId>((Func<InitialResult.DetailResult, ElementId>) (s => this.uiDoc.Document.GetElement(s.ElementId).GetTopLevelElement().Id)).ToList<ElementId>());
    else
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
  }

  private void lstResults_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    try
    {
      this.HighlightElements();
    }
    catch (Exception ex)
    {
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
    }
  }

  private void btnExport_Click(object sender, RoutedEventArgs e)
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
      List<List<object>> source = new List<List<object>>();
      List<object> objectList1 = new List<object>();
      source.Add(new List<object>()
      {
        (object) "ELEMENT ID",
        (object) "CONTROL MARKS",
        (object) "CONTROL NUMBER"
      });
      foreach (InitialResult result in (IEnumerable<InitialResult>) this._results)
      {
        foreach (InitialResult.DetailResult detailedResult in result.DetailedResults)
          source.Add(new List<object>()
          {
            (object) detailedResult.ElementId,
            (object) result.Bucket,
            (object) detailedResult.ControlNumber
          });
      }
      ExcelDocument excelDocument = new ExcelDocument(fileName);
      if (excelDocument != null)
      {
        int num = ((IEnumerable<object>) ((IEnumerable<object>) source).ToArray<object>()).Count<object>();
        int rowId = 1;
        foreach (List<object> objectList2 in source)
        {
          if (rowId <= num)
          {
            for (int index = 0; index < objectList2.Count; ++index)
            {
              if (objectList2[index] == null)
                objectList2[index] = (object) string.Empty;
              if (objectList2[index] != null)
              {
                if (objectList2[index] is string str1)
                {
                  excelDocument.UpdateCellValue(index + 1, rowId, str1);
                }
                else
                {
                  ElementId elementId = objectList2[index] as ElementId;
                  if ((object) elementId != null)
                  {
                    string str = elementId.IntegerValue.ToString();
                    excelDocument.UpdateCellValue(index + 1, rowId, str);
                  }
                }
              }
            }
            ++rowId;
          }
        }
        excelDocument.SaveAndClose();
      }
      TaskDialog.Show("Message", $"The file has been exported to {fileName}.");
    }
    catch (Exception ex)
    {
      if (ex.ToString().Contains("because it is being used by another process") && ex.ToString().Contains("The process cannot access the file"))
      {
        int num = (int) MessageBox.Show("The file you attempted to save is being used by another process. Please close the file before saving it.", "Warning");
      }
      else
      {
        if (ex == null)
          return;
        TaskDialog.Show("Warning", ex.ToString());
        throw;
      }
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/markverification/initialpresentation/mkverificationresult_initial.xaml", UriKind.Relative));
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
        this.lstResults = (ListBox) target;
        this.lstResults.SelectionChanged += new SelectionChangedEventHandler(this.lstResults_SelectionChanged);
        this.lstResults.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.lstResults_PreviewMouseLeftButtonDown);
        break;
      case 3:
        this.deatilBorder = (Border) target;
        break;
      case 4:
        this.detailPanel = (System.Windows.Controls.Grid) target;
        break;
      case 5:
        this.MarkNumberBox = (TextBox) target;
        this.MarkNumberBox.TextChanged += new TextChangedEventHandler(this.TextBox_TextChanged);
        break;
      case 6:
        this.btnSave = (Button) target;
        this.btnSave.Click += new RoutedEventHandler(this.SaveButton_Click);
        break;
      case 7:
        this.lstTestResults = (ListBox) target;
        this.lstTestResults.SelectionChanged += new SelectionChangedEventHandler(this.lstTestResults_SelectionChanged);
        break;
      case 8:
        this.btnApply = (Button) target;
        this.btnApply.Click += new RoutedEventHandler(this.ApplyButton_Click);
        break;
      case 9:
        this.btnExport = (Button) target;
        this.btnExport.Click += new RoutedEventHandler(this.btnExport_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
