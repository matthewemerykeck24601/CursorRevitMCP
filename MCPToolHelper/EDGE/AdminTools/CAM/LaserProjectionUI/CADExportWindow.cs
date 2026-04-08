// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.LaserProjectionUI.CADExportWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.UserSettingTools.CAD_Export_Settings;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.AdminTools.CAM.LaserProjectionUI;

public class CADExportWindow : Window, IComponentConnector
{
  private string toolName;
  private string manufacturer;
  public bool Cancel = true;
  internal System.Windows.Controls.TextBox ExportFolderPathTextBox;
  internal System.Windows.Controls.Button FolderExplorerButton;
  internal System.Windows.Controls.TextBox FileNameSuffixTextBox;
  internal System.Windows.Controls.CheckBox DWGCheckBox;
  internal System.Windows.Controls.CheckBox DFXChekBox;
  internal TextBlock ExampleFilePathLabel;
  internal System.Windows.Controls.Label FileOverwriteWarningLable;
  internal System.Windows.Controls.Button ResetButton;
  internal System.Windows.Controls.Button ExportButton;
  internal System.Windows.Controls.Button CancelButton;
  private bool _contentLoaded;

  public CADExportWindow(
    IntPtr parentWindowHandler,
    CADExportSettings settings,
    List<string> ElementMarks,
    string projectName,
    string projectNumber,
    string manufacturer,
    bool CADExportTool = true)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.toolName = !CADExportTool ? "Insulation Export" : "CAD Export";
    this.Title = this.toolName;
    this.manufacturer = manufacturer;
    this.DataContext = (object) new CADExportWindowViewModel(settings, CADExportTool, ElementMarks, projectNumber, projectName, manufacturer);
  }

  private void FolderExplorerButton_Click(object sender, RoutedEventArgs e)
  {
    if (!(this.DataContext is CADExportWindowViewModel dataContext))
      return;
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    folderBrowserDialog.Description = $"Please Select {this.toolName} Folder Path:";
    if (Directory.Exists(dataContext.ExportFolderPathString.Trim()))
      folderBrowserDialog.SelectedPath = dataContext.ExportFolderPathString.Trim();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    string selectedPath = folderBrowserDialog.SelectedPath;
    dataContext.ExportFolderPathString = selectedPath;
  }

  public CADExportSettings GetNewSettings()
  {
    return this.DataContext is CADExportWindowViewModel dataContext ? dataContext.GetUpdatedSettings() : (CADExportSettings) null;
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void ResetButton_Click(object sender, RoutedEventArgs e)
  {
    if (!(this.DataContext is CADExportWindowViewModel dataContext))
      return;
    dataContext.SetSettings();
  }

  private void ExportButton_Click(object sender, RoutedEventArgs e)
  {
    if (!(this.DataContext is CADExportWindowViewModel dataContext))
      return;
    List<string> stringList = new List<string>();
    if (Validation.GetHasError((DependencyObject) this.ExportFolderPathTextBox))
      stringList.Add("Export Folder Path");
    if (Validation.GetHasError((DependencyObject) this.FileNameSuffixTextBox))
      stringList.Add("File Name Suffix");
    if (dataContext.BothUnchecked)
      stringList.Add("At least one export file type must be selected.");
    if (stringList.Count > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog(this.toolName);
      taskDialog1.MainInstruction = "One or more fields have invalid settings. Please correct these fields and try to Export again.";
      stringList.Sort();
      foreach (string str in stringList)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str}\n";
      }
      taskDialog1.Show();
    }
    else
    {
      dataContext.UpdateSessionDictionaries(this.manufacturer);
      this.Cancel = false;
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
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/admintools/cam/laserprojectionui/cadexportwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  internal Delegate _CreateDelegate(Type delegateType, string handler)
  {
    return Delegate.CreateDelegate(delegateType, (object) this, handler);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.ExportFolderPathTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 2:
        this.FolderExplorerButton = (System.Windows.Controls.Button) target;
        this.FolderExplorerButton.Click += new RoutedEventHandler(this.FolderExplorerButton_Click);
        break;
      case 3:
        this.FileNameSuffixTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 4:
        this.DWGCheckBox = (System.Windows.Controls.CheckBox) target;
        break;
      case 5:
        this.DFXChekBox = (System.Windows.Controls.CheckBox) target;
        break;
      case 6:
        this.ExampleFilePathLabel = (TextBlock) target;
        break;
      case 7:
        this.FileOverwriteWarningLable = (System.Windows.Controls.Label) target;
        break;
      case 8:
        this.ResetButton = (System.Windows.Controls.Button) target;
        this.ResetButton.Click += new RoutedEventHandler(this.ResetButton_Click);
        break;
      case 9:
        this.ExportButton = (System.Windows.Controls.Button) target;
        this.ExportButton.Click += new RoutedEventHandler(this.ExportButton_Click);
        break;
      case 10:
        this.CancelButton = (System.Windows.Controls.Button) target;
        this.CancelButton.Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
