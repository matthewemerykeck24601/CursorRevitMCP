// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.Views.BFUSelectWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater.Views;

public class BFUSelectWindow : Window, IComponentConnector
{
  private bool canceled = true;
  private bool sharedParameterMode;
  private Dictionary<string, List<string>> sharedParameterGroupDictionary = new Dictionary<string, List<string>>();
  internal RowDefinition parameterGroupRow;
  internal Label headerLabel;
  internal System.Windows.Controls.Grid parameterGroupGrid;
  internal Label parameterGroupLabel;
  internal ComboBox parameterGroupComboBox;
  internal RowDefinition parametersLabelGrid;
  internal Label parametersLabel;
  internal ListBox parametersListBox;
  internal Button oKButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public BFUSelectWindow(
    IList<FamilyParameter> familyParameters,
    List<string> usedParameters,
    IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    this.Title = "Select Family Parameter";
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.parameterGroupRow.Height = new GridLength(0.0);
    this.parameterGroupGrid.Visibility = System.Windows.Visibility.Collapsed;
    this.parametersLabel.Visibility = System.Windows.Visibility.Collapsed;
    this.parametersLabelGrid.Height = new GridLength(0.0);
    this.DataContext = (object) new BFUSelectViewModel(familyParameters, usedParameters);
  }

  public BFUSelectWindow(
    DefinitionGroups sharedParameters,
    List<string> usedParameters,
    IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    this.Title = "Select Shared Parameter";
    this.sharedParameterMode = true;
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    List<string> groupNames = new List<string>();
    foreach (DefinitionGroup sharedParameter in sharedParameters)
    {
      if (!this.sharedParameterGroupDictionary.ContainsKey(sharedParameter.Name))
      {
        this.sharedParameterGroupDictionary.Add(sharedParameter.Name, new List<string>());
        groupNames.Add(sharedParameter.Name);
      }
      foreach (Definition definition in sharedParameter.Definitions)
      {
        if (!usedParameters.Contains(definition.Name.Trim()))
          this.sharedParameterGroupDictionary[sharedParameter.Name].Add(definition.Name);
      }
    }
    groupNames.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    foreach (KeyValuePair<string, List<string>> sharedParameterGroup in this.sharedParameterGroupDictionary)
      sharedParameterGroup.Value.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    this.DataContext = (object) new BFUSelectViewModel(groupNames);
  }

  private void oKButton_Click(object sender, RoutedEventArgs e)
  {
    this.canceled = false;
    this.Close();
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void parameterGroupComboBox_SelectionChanged(object sender, RoutedEventArgs e)
  {
    BFUSelectViewModel dataContext = this.DataContext as BFUSelectViewModel;
    dataContext.ParametersList.Clear();
    foreach (string str in this.sharedParameterGroupDictionary[dataContext.SelectedParameterGroup])
      dataContext.ParametersList.Add(str);
    this.parametersListBox.Items.Refresh();
  }

  public void getResults(
    out bool canceledBool,
    out string familyParameterName,
    out string sharedParameterName,
    out string sharedParameterGroup)
  {
    BFUSelectViewModel dataContext = this.DataContext as BFUSelectViewModel;
    canceledBool = this.canceled;
    if (this.sharedParameterMode)
    {
      familyParameterName = "";
      sharedParameterName = dataContext.SelectedParameter;
      sharedParameterGroup = dataContext.SelectedParameterGroup;
    }
    else
    {
      familyParameterName = dataContext.SelectedParameter;
      sharedParameterName = "";
      sharedParameterGroup = "";
    }
  }

  private void parametersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    BFUSelectViewModel dataContext = this.DataContext as BFUSelectViewModel;
    if (this.sharedParameterMode)
    {
      if (this.sharedParameterGroupDictionary.ContainsKey(dataContext.SelectedParameterGroup) && this.sharedParameterGroupDictionary[dataContext.SelectedParameterGroup].Contains(dataContext.SelectedParameter))
        this.oKButton.IsEnabled = true;
      else
        this.oKButton.IsEnabled = false;
    }
    else if (!string.IsNullOrWhiteSpace(dataContext.SelectedParameter) && dataContext.ParametersList.Contains(dataContext.SelectedParameter))
      this.oKButton.IsEnabled = true;
    else
      this.oKButton.IsEnabled = false;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/__testing/bulkfamilyupdater/views/bfuselectwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.parameterGroupRow = (RowDefinition) target;
        break;
      case 2:
        this.headerLabel = (Label) target;
        break;
      case 3:
        this.parameterGroupGrid = (System.Windows.Controls.Grid) target;
        break;
      case 4:
        this.parameterGroupLabel = (Label) target;
        break;
      case 5:
        this.parameterGroupComboBox = (ComboBox) target;
        this.parameterGroupComboBox.SelectionChanged += new SelectionChangedEventHandler(this.parameterGroupComboBox_SelectionChanged);
        break;
      case 6:
        this.parametersLabelGrid = (RowDefinition) target;
        break;
      case 7:
        this.parametersLabel = (Label) target;
        break;
      case 8:
        this.parametersListBox = (ListBox) target;
        this.parametersListBox.SelectionChanged += new SelectionChangedEventHandler(this.parametersListBox_SelectionChanged);
        break;
      case 9:
        this.oKButton = (Button) target;
        this.oKButton.Click += new RoutedEventHandler(this.oKButton_Click);
        break;
      case 10:
        this.cancelButton = (Button) target;
        this.cancelButton.Click += new RoutedEventHandler(this.cancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
