// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.Views.BFUParameterTaskWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater.Views;

public class BFUParameterTaskWindow : Window, IComponentConnector
{
  private FamilyUpdaterTask task;
  private BulkFamilyUpdatersUtils.taskActions action;
  private IList<FamilyParameter> familyParameters;
  private DefinitionGroups sharedParameters;
  private bool? sharedParameterSelectedBool;
  private Document revitDoc;
  private List<string> usedParams = new List<string>();
  private int normalValueTextBoxLength = 140;
  private int formulaValueTextBoxLength = 235;
  private bool editWindow;
  private List<string> builtInParameters = new List<string>();
  public bool cancelWindow = true;
  internal RowDefinition typeInstanceRow;
  internal RowDefinition groupRow;
  internal RowDefinition valueRow;
  internal Label parameterTypeLabel;
  internal Button familyParameterButton;
  internal Button sharedParameterButton;
  internal Label parameterNameLabel;
  internal TextBox currentParameterNameLabel;
  internal TextBox parameterNameTextBox;
  internal CheckBox typeCheckBox;
  internal CheckBox instanceCheckBox;
  internal Label disciplineLabel;
  internal ComboBox disciplineComboBox;
  internal Label typeLabel;
  internal ComboBox typeComboBox;
  internal Label groupUnderLabel;
  internal ComboBox groupUnderComboBox;
  internal Label formulaEqualLabel;
  internal Label valueLabel;
  internal TextBox valueTextBox;
  internal Label unitLabel;
  internal CheckBox yesValueCheckBox;
  internal CheckBox noValueCheckBox;
  internal CheckBox formulaCheckBox;
  internal CheckBox clearValueCheckBox;
  internal Button oKButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public BFUParameterTaskWindow(
    BulkFamilyUpdatersUtils.taskActions taskAction,
    IntPtr parentWindowHandler,
    Document doc,
    List<string> usedParameters,
    IList<FamilyParameter> familyParameters = null,
    DefinitionGroups sharedParameters = null)
  {
    this.InitializeComponent();
    this.DataContext = (object) new BFUParameterTaskViewModel(taskAction);
    this.action = taskAction;
    this.revitDoc = doc;
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.usedParams = usedParameters;
    BFUParameterTaskViewModel dataContext = this.DataContext as BFUParameterTaskViewModel;
    if (this.action == BulkFamilyUpdatersUtils.taskActions.Add)
    {
      this.Title = "Add Parameter";
      this.familyParameterButton.FontWeight = FontWeights.Bold;
      this.sharedParameterSelectedBool = new bool?(false);
      dataContext.TypeCheckBoxBool = new bool?(true);
      dataContext.SelectedDisciplineItem = "Common";
      dataContext.SelectedTypeItem = "Length";
      dataContext.SelectedGroupUnderItem = "Dimensions";
    }
    else if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
    {
      if (familyParameters != null)
      {
        foreach (FamilyParameter familyParameter in (IEnumerable<FamilyParameter>) familyParameters)
        {
          if (((InternalDefinition) familyParameter.Definition).BuiltInParameter != BuiltInParameter.INVALID)
            this.builtInParameters.Add(familyParameter.Definition.Name);
        }
      }
      this.Title = "Modify Parameter";
      this.parameterNameTextBox.IsEnabled = false;
      this.typeCheckBox.IsEnabled = false;
      this.instanceCheckBox.IsEnabled = false;
      this.disciplineComboBox.IsEnabled = false;
      this.typeComboBox.IsEnabled = false;
      this.groupUnderComboBox.IsEnabled = false;
      this.currentParameterNameLabel.Visibility = System.Windows.Visibility.Visible;
      this.valueTextBox.IsEnabled = false;
      this.formulaCheckBox.IsEnabled = false;
      this.clearValueCheckBox.Visibility = System.Windows.Visibility.Visible;
      this.clearValueCheckBox.IsEnabled = false;
      this.oKButton.IsEnabled = false;
    }
    else if (this.action == BulkFamilyUpdatersUtils.taskActions.Delete)
    {
      this.Title = "Delete Parameter";
      this.parameterNameTextBox.IsEnabled = false;
      this.typeCheckBox.IsEnabled = false;
      this.instanceCheckBox.IsEnabled = false;
      this.disciplineComboBox.IsEnabled = false;
      this.typeComboBox.IsEnabled = false;
      this.groupUnderComboBox.IsEnabled = false;
      this.currentParameterNameLabel.Visibility = System.Windows.Visibility.Visible;
      this.valueTextBox.IsEnabled = false;
      this.formulaCheckBox.IsEnabled = false;
      this.oKButton.IsEnabled = false;
      this.yesValueCheckBox.IsEnabled = false;
      this.noValueCheckBox.IsEnabled = false;
      this.typeInstanceRow.Height = new GridLength(0.0);
      this.groupRow.Height = new GridLength(0.0);
      this.valueRow.Height = new GridLength(0.0);
      this.Height = 320.0;
    }
    this.familyParameters = familyParameters;
    this.sharedParameters = sharedParameters;
    if (familyParameters != null && familyParameters.Count > 0 || this.action == BulkFamilyUpdatersUtils.taskActions.Add)
      this.familyParameterButton.IsEnabled = true;
    if (sharedParameters == null)
      return;
    this.sharedParameterButton.IsEnabled = true;
  }

  public BFUParameterTaskWindow(
    FamilyUpdaterTask updaterTask,
    IntPtr parentWindowHandler,
    Document doc,
    List<string> usedParameters,
    IList<FamilyParameter> familyParameters = null)
  {
    this.InitializeComponent();
    this.revitDoc = doc;
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.task = updaterTask;
    this.action = this.task.action;
    if (usedParameters.Contains(this.task.parameterName))
      usedParameters.Remove(this.task.parameterName);
    if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify && usedParameters.Contains(this.task.oldParameterName))
      usedParameters.Remove(this.task.oldParameterName);
    this.usedParams = usedParameters;
    if (this.action == BulkFamilyUpdatersUtils.taskActions.Add)
      this.Title = "Edit Add Task";
    else if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
      this.Title = "Edit Modify Task";
    else
      this.Close();
    this.editWindow = true;
    this.DataContext = (object) new BFUParameterTaskViewModel(this.action);
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    if (this.task.isSharedParameter.ToUpper().Contains("SHARED"))
    {
      this.sharedParameterButton.FontWeight = FontWeights.Bold;
      this.sharedParameterSelectedBool = new bool?(true);
    }
    else if (this.task.isSharedParameter.ToUpper().Contains("FAMILY"))
    {
      this.familyParameterButton.FontWeight = FontWeights.Bold;
      this.sharedParameterSelectedBool = new bool?(false);
      if (familyParameters != null)
      {
        foreach (FamilyParameter familyParameter in (IEnumerable<FamilyParameter>) familyParameters)
        {
          if (((InternalDefinition) familyParameter.Definition).BuiltInParameter != BuiltInParameter.INVALID)
            this.builtInParameters.Add(familyParameter.Definition.Name);
        }
      }
    }
    viewModel.ParameterName = this.task.parameterName;
    viewModel.CurrentParmaeterName = this.task.oldParameterName;
    this.currentParameterNameLabel.Visibility = System.Windows.Visibility.Visible;
    if (this.task.isTypeParameter.ToUpper().Contains("TYPE"))
      viewModel.TypeCheckBoxBool = new bool?(true);
    else if (this.task.isTypeParameter.ToUpper().Contains("INSTANCE"))
      viewModel.InstanceCheckBoxBool = new bool?(true);
    viewModel.SelectedDisciplineItem = this.task.discipline;
    viewModel.SelectedTypeItem = this.task.specType;
    viewModel.SelectedGroupUnderItem = !BulkFamilyUpdatersUtils.groupParameterUnderToStringDictionary.ContainsValue(this.task.paramGroup) ? "" : this.task.paramGroup;
    ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem)).Key;
    ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
    if (this.task.isValueFormula.GetValueOrDefault())
    {
      viewModel.FormulaCheckBoxBool = new bool?(true);
      viewModel.ParameterValue = this.task.value;
    }
    else if (BulkFamilyUpdatersUtils.boolSpecTypeIdList.Contains(key))
    {
      if (this.task.value.ToUpper().Contains("YES"))
        viewModel.YesCheckBoxBool = new bool?(true);
      else if (this.task.value.ToUpper().Contains("NO"))
        viewModel.NoCheckBoxBool = new bool?(true);
    }
    else
      viewModel.ParameterValue = this.formatForWindowDisplay();
    if (!string.IsNullOrWhiteSpace(viewModel.ParameterValue) && (viewModel.ParameterValue.Equals("unassigned") || viewModel.ParameterValue.Equals("unchanged")))
      viewModel.ParameterValue = "";
    if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
    {
      this.clearValueCheckBox.Visibility = System.Windows.Visibility.Visible;
      if (this.task.clearValue.GetValueOrDefault())
        viewModel.ClearValueCheckBoxBool = new bool?(true);
    }
    if (this.sharedParameterSelectedBool.GetValueOrDefault())
      this.parameterNameTextBox.IsEnabled = false;
    this.disciplineComboBox.IsEnabled = false;
    this.typeComboBox.IsEnabled = false;
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Modify)
      return;
    bool? parameterSelectedBool = this.sharedParameterSelectedBool;
    bool flag = false;
    if (!(parameterSelectedBool.GetValueOrDefault() == flag & parameterSelectedBool.HasValue) || !this.builtInParameters.Contains(viewModel.CurrentParmaeterName))
      return;
    this.parameterNameTextBox.IsEnabled = false;
    viewModel.ParameterName = viewModel.CurrentParmaeterName;
    this.typeCheckBox.IsEnabled = false;
    this.instanceCheckBox.IsEnabled = false;
    this.disciplineComboBox.IsEnabled = false;
    this.typeComboBox.IsEnabled = false;
    this.groupUnderComboBox.IsEnabled = false;
  }

  private void familyParameterButton_Click(object sender, RoutedEventArgs e)
  {
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    if (this.action == BulkFamilyUpdatersUtils.taskActions.Add)
    {
      if (!this.sharedParameterSelectedBool.GetValueOrDefault())
        return;
      this.familyParameterButton.FontWeight = FontWeights.Bold;
      this.sharedParameterSelectedBool = new bool?(false);
      this.sharedParameterButton.FontWeight = FontWeights.Regular;
      viewModel.ParameterName = "";
      this.parameterNameTextBox.IsEnabled = true;
      this.typeCheckBox.IsChecked = new bool?(true);
      this.instanceCheckBox.IsChecked = new bool?(false);
      this.disciplineComboBox.IsEnabled = true;
      this.typeComboBox.IsEnabled = true;
      this.groupUnderComboBox.IsEnabled = true;
      viewModel.ParameterValue = "";
      this.formulaCheckBox.IsChecked = new bool?(false);
      viewModel.SelectedDisciplineItem = "Common";
      viewModel.SelectedTypeItem = "Length";
      viewModel.SelectedGroupUnderItem = "Dimensions";
    }
    else
    {
      BFUSelectWindow bfuSelectWindow = new BFUSelectWindow(this.familyParameters, this.usedParams, Process.GetCurrentProcess().MainWindowHandle);
      bfuSelectWindow.ShowDialog();
      bool canceledBool;
      string familyParameterName;
      bfuSelectWindow.getResults(out canceledBool, out familyParameterName, out string _, out string _);
      if (canceledBool)
        return;
      this.oKButton.IsEnabled = true;
      FamilyParameter familyParameter1 = (FamilyParameter) null;
      foreach (FamilyParameter familyParameter2 in (IEnumerable<FamilyParameter>) this.familyParameters)
      {
        if (familyParameter2.Definition.Name.Equals(familyParameterName))
        {
          familyParameter1 = familyParameter2;
          break;
        }
      }
      if (familyParameter1 == null)
      {
        int num = (int) MessageBox.Show("An error occured when selecting the family parameter. Please try again. If issue continues please contact EDGE support.");
      }
      else
      {
        this.familyParameterButton.FontWeight = FontWeights.Bold;
        this.sharedParameterSelectedBool = new bool?(false);
        this.sharedParameterButton.FontWeight = FontWeights.Regular;
        if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
        {
          viewModel.CurrentParmaeterName = familyParameter1.Definition.Name;
          viewModel.ParameterName = "";
        }
        else if (this.action == BulkFamilyUpdatersUtils.taskActions.Delete)
          viewModel.ParameterName = familyParameter1.Definition.Name;
        this.updateDisciplineTypeGroup(familyParameter1.Definition);
        ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem)).Key;
        ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
        if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
        {
          this.parameterNameTextBox.IsEnabled = true;
          if (viewModel.SelectedTypeItem != "Image")
          {
            this.instanceCheckBox.IsEnabled = true;
            this.typeCheckBox.IsEnabled = true;
          }
          this.instanceCheckBox.IsChecked = new bool?(false);
          this.typeCheckBox.IsChecked = new bool?(false);
          this.groupUnderComboBox.IsEnabled = true;
          if (viewModel.SelectedTypeItem != "Fill Pattern")
          {
            if (!BulkFamilyUpdatersUtils.boolSpecTypeIdList.Contains(key))
              this.valueTextBox.IsEnabled = true;
            this.formulaCheckBox.IsEnabled = true;
            this.clearValueCheckBox.IsEnabled = true;
          }
        }
        if (this.action != BulkFamilyUpdatersUtils.taskActions.Modify)
          return;
        bool? parameterSelectedBool = this.sharedParameterSelectedBool;
        bool flag = false;
        if (!(parameterSelectedBool.GetValueOrDefault() == flag & parameterSelectedBool.HasValue) || !this.builtInParameters.Contains(viewModel.CurrentParmaeterName))
          return;
        this.parameterNameTextBox.IsEnabled = false;
        viewModel.ParameterName = viewModel.CurrentParmaeterName;
        this.typeCheckBox.IsEnabled = false;
        this.instanceCheckBox.IsEnabled = false;
        this.disciplineComboBox.IsEnabled = false;
        this.typeComboBox.IsEnabled = false;
        this.groupUnderComboBox.IsEnabled = false;
      }
    }
  }

  private void sharedParameterButton_Click(object sender, RoutedEventArgs e)
  {
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    BFUSelectWindow bfuSelectWindow = new BFUSelectWindow(this.sharedParameters, this.usedParams, Process.GetCurrentProcess().MainWindowHandle);
    bfuSelectWindow.ShowDialog();
    bool canceledBool;
    string sharedParameterName;
    string sharedParameterGroup;
    bfuSelectWindow.getResults(out canceledBool, out string _, out sharedParameterName, out sharedParameterGroup);
    if (canceledBool)
      return;
    this.oKButton.IsEnabled = true;
    ExternalDefinition parameter = (ExternalDefinition) null;
    foreach (DefinitionGroup sharedParameter in this.sharedParameters)
    {
      if (sharedParameter.Name.Equals(sharedParameterGroup))
      {
        using (IEnumerator<Definition> enumerator = sharedParameter.Definitions.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            ExternalDefinition current = (ExternalDefinition) enumerator.Current;
            if (current.Name.Equals(sharedParameterName))
            {
              parameter = current;
              break;
            }
          }
          break;
        }
      }
    }
    if (parameter == null)
    {
      int num = (int) MessageBox.Show("An error occured when selecting the shared parameter. Please try again. If issue continues please contact EDGE support.");
    }
    else
    {
      this.sharedParameterButton.FontWeight = FontWeights.Bold;
      this.sharedParameterSelectedBool = new bool?(true);
      this.familyParameterButton.FontWeight = FontWeights.Regular;
      if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
        viewModel.CurrentParmaeterName = sharedParameterName;
      viewModel.ParameterName = sharedParameterName;
      if (this.action == BulkFamilyUpdatersUtils.taskActions.Add)
      {
        this.typeCheckBox.IsChecked = new bool?(true);
        this.instanceCheckBox.IsChecked = new bool?(false);
      }
      else
      {
        this.typeCheckBox.IsChecked = new bool?(false);
        this.instanceCheckBox.IsChecked = new bool?(false);
      }
      this.updateDisciplineTypeGroup((Definition) parameter);
      this.formulaCheckBox.IsChecked = new bool?(false);
      this.clearValueCheckBox.IsChecked = new bool?(false);
      viewModel.ParameterValue = "";
      this.formulaCheckBox.IsChecked = new bool?(false);
      if (this.action != BulkFamilyUpdatersUtils.taskActions.Delete)
      {
        ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem)).Key;
        ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
        this.parameterNameTextBox.IsEnabled = false;
        if (viewModel.SelectedTypeItem != "Image")
        {
          this.typeCheckBox.IsEnabled = true;
          this.instanceCheckBox.IsEnabled = true;
        }
        this.disciplineComboBox.IsEnabled = false;
        this.typeComboBox.IsEnabled = false;
        this.groupUnderComboBox.IsEnabled = true;
        if (viewModel.SelectedTypeItem != "Fill Pattern")
        {
          if (!BulkFamilyUpdatersUtils.boolSpecTypeIdList.Contains(key))
            this.valueTextBox.IsEnabled = true;
          if (!BulkFamilyUpdatersUtils.referenceSpecTypeIdList.Contains(key))
            this.formulaCheckBox.IsEnabled = true;
          this.clearValueCheckBox.IsEnabled = true;
        }
      }
      if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
        return;
      viewModel.SelectedGroupUnderItem = "General";
    }
  }

  private void typeCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this.instanceCheckBox.IsChecked = new bool?(false);
  }

  private void typeCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
      return;
    this.instanceCheckBox.IsChecked = new bool?(true);
  }

  private void instanceCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this.typeCheckBox.IsChecked = new bool?(false);
  }

  private void instanceCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
      return;
    this.typeCheckBox.IsChecked = new bool?(true);
  }

  private void formulaCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this.clearValueCheckBox.IsChecked = new bool?(false);
    this.formulaEqualLabel.Visibility = System.Windows.Visibility.Visible;
    this.valueTextBox.Width = (double) this.formulaValueTextBoxLength;
    this.valueTextBox.Visibility = System.Windows.Visibility.Visible;
    this.unitLabel.Visibility = System.Windows.Visibility.Collapsed;
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
    {
      this.yesValueCheckBox.IsChecked = new bool?(false);
      this.noValueCheckBox.IsChecked = new bool?(false);
    }
    this.yesValueCheckBox.Visibility = System.Windows.Visibility.Collapsed;
    this.noValueCheckBox.Visibility = System.Windows.Visibility.Collapsed;
    this.valueTextBox.IsEnabled = true;
    BFUParameterTaskViewModel dataContext = this.DataContext as BFUParameterTaskViewModel;
    if (this.task != null && !this.task.isValueFormula.GetValueOrDefault() && !this.task.value.Equals(dataContext.ParameterValue))
      dataContext.ParameterValue = "";
    dataContext.ValueTextBoxHint = "";
  }

  private void formulaCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    this.formulaEqualLabel.Visibility = System.Windows.Visibility.Hidden;
    this.valueTextBox.Width = (double) this.normalValueTextBoxLength;
    this.unitLabel.Visibility = System.Windows.Visibility.Visible;
    ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem)).Key;
    ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
    if (BulkFamilyUpdatersUtils.boolSpecTypeIdList.Contains(key))
    {
      this.yesValueCheckBox.Visibility = System.Windows.Visibility.Visible;
      this.noValueCheckBox.Visibility = System.Windows.Visibility.Visible;
      this.valueTextBox.Visibility = System.Windows.Visibility.Collapsed;
    }
    viewModel.ParameterValue = "";
    if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
      viewModel.ValueTextBoxHint = "Unchanged";
    else if (this.action == BulkFamilyUpdatersUtils.taskActions.Add)
      viewModel.ValueTextBoxHint = "Unassigned";
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
      return;
    viewModel.YesCheckBoxBool = new bool?(true);
  }

  private void clearValueCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this.formulaCheckBox.IsChecked = new bool?(false);
    this.valueTextBox.IsEnabled = false;
    (this.DataContext as BFUParameterTaskViewModel).ParameterValue = "";
  }

  private void clearValueCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this.valueTextBox.IsEnabled = true;
  }

  private void oKButton_Click(object sender, RoutedEventArgs e)
  {
    FamilyUpdaterTask original = new FamilyUpdaterTask(this.action);
    if (this.task == null)
      this.task = new FamilyUpdaterTask(this.action);
    else
      original = new FamilyUpdaterTask(this.task);
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    bool flag1 = false;
    List<string> stringList = new List<string>();
    ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem)).Key;
    ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
    if (this.task.action == BulkFamilyUpdatersUtils.taskActions.Add)
    {
      if (string.IsNullOrWhiteSpace(viewModel.ParameterName))
      {
        flag1 = true;
        stringList.Add("Parameter Name");
      }
      if (!viewModel.TypeCheckBoxBool.GetValueOrDefault() && !viewModel.InstanceCheckBoxBool.GetValueOrDefault())
      {
        flag1 = true;
        stringList.Add("Type/Instance");
      }
      if (string.IsNullOrWhiteSpace(viewModel.SelectedDisciplineItem))
      {
        flag1 = true;
        stringList.Add("Discipline");
      }
      if (string.IsNullOrWhiteSpace(viewModel.SelectedTypeItem))
      {
        flag1 = true;
        stringList.Add("Parameter Type");
      }
      if (string.IsNullOrWhiteSpace(viewModel.SelectedGroupUnderItem))
      {
        flag1 = true;
        stringList.Add("Group Parameter Under");
      }
      if (viewModel.FormulaCheckBoxBool.GetValueOrDefault() && string.IsNullOrWhiteSpace(viewModel.ParameterValue))
      {
        flag1 = true;
        stringList.Add("Formula");
      }
    }
    else if (this.task.action == BulkFamilyUpdatersUtils.taskActions.Delete)
    {
      if (string.IsNullOrWhiteSpace(viewModel.SelectedDisciplineItem))
      {
        flag1 = true;
        stringList.Add("Discipline");
      }
      if (string.IsNullOrWhiteSpace(viewModel.SelectedTypeItem))
      {
        flag1 = true;
        stringList.Add("Parameter Type");
      }
    }
    else if (this.task.action == BulkFamilyUpdatersUtils.taskActions.Modify)
    {
      if (string.IsNullOrWhiteSpace(viewModel.SelectedDisciplineItem))
      {
        flag1 = true;
        stringList.Add("Discipline");
      }
      if (string.IsNullOrWhiteSpace(viewModel.SelectedTypeItem))
      {
        flag1 = true;
        stringList.Add("Parameter Type");
      }
      if (viewModel.FormulaCheckBoxBool.GetValueOrDefault() && string.IsNullOrWhiteSpace(viewModel.ParameterValue))
      {
        flag1 = true;
        stringList.Add("Formula");
      }
      bool flag2 = false;
      if (BulkFamilyUpdatersUtils.boolSpecTypeIdList.Contains(key) && (viewModel.YesCheckBoxBool.GetValueOrDefault() || viewModel.NoCheckBoxBool.GetValueOrDefault() || viewModel.FormulaCheckBoxBool.GetValueOrDefault()))
        flag2 = true;
      if (!string.IsNullOrWhiteSpace(viewModel.ParameterName) && !viewModel.ParameterName.Trim().Equals(viewModel.CurrentParmaeterName.Trim()))
        flag2 = true;
      if (viewModel.TypeCheckBoxBool.GetValueOrDefault() || viewModel.InstanceCheckBoxBool.GetValueOrDefault())
        flag2 = true;
      if (!string.IsNullOrWhiteSpace(viewModel.SelectedGroupUnderItem))
        flag2 = true;
      if (!string.IsNullOrWhiteSpace(viewModel.ParameterValue) || viewModel.ClearValueCheckBoxBool.GetValueOrDefault())
        flag2 = true;
      if (!flag2)
      {
        new TaskDialog("Bulk Family Updater")
        {
          MainContent = "Unable to create task. You have not made any changes to the Parameter. Please make desired changes and try again."
        }.Show();
        this.task = new FamilyUpdaterTask(original);
        return;
      }
    }
    if (flag1)
    {
      TaskDialog taskDialog1 = new TaskDialog("Bulk Family Updater");
      taskDialog1.MainContent = "Unable to create task. Please check the following fields and try again: \n";
      foreach (string str in stringList)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.MainContent = $"{taskDialog2.MainContent}{str}\n";
      }
      taskDialog1.Show();
      this.task = new FamilyUpdaterTask(original);
    }
    else if (this.usedParams != null && (viewModel.ParameterName != null && this.usedParams.Contains(viewModel.ParameterName.Trim()) || viewModel.CurrentParmaeterName != null && this.usedParams.Contains(viewModel.CurrentParmaeterName.Trim())))
    {
      new TaskDialog("Bulk Family Updater")
      {
        MainContent = "You already have a task created using a parameter with the same name. Change the name of the parameter you are using and try again."
      }.Show();
      this.task = new FamilyUpdaterTask(original);
    }
    else
    {
      this.task.parameterName = viewModel.ParameterName.Trim();
      if (string.IsNullOrWhiteSpace(this.task.parameterName))
        this.task.parameterName = viewModel.CurrentParmaeterName;
      this.task.oldParameterName = viewModel.CurrentParmaeterName;
      if (!string.IsNullOrWhiteSpace(this.task.parameterName))
      {
        char[] charArray1 = ":{}[]\\|;<>?`~".ToCharArray();
        char[] charArray2 = this.task.parameterName.ToCharArray();
        foreach (char ch in charArray1)
        {
          if (((IEnumerable<char>) charArray2).Contains<char>(ch))
          {
            new TaskDialog("Bulk Family Updater")
            {
              MainContent = "Name cannot contain any of the following characters: \n \\ : {} [] | ; < > ? ` ~ \nor any of the non-printable characters. "
            }.Show();
            this.task = new FamilyUpdaterTask(original);
            return;
          }
        }
      }
      bool? nullable;
      if (this.sharedParameterSelectedBool.GetValueOrDefault())
      {
        this.task.isSharedParameter = "Shared";
      }
      else
      {
        nullable = this.sharedParameterSelectedBool;
        bool flag3 = false;
        this.task.isSharedParameter = !(nullable.GetValueOrDefault() == flag3 & nullable.HasValue) ? (this.task.action != BulkFamilyUpdatersUtils.taskActions.Delete ? "unchanged" : "") : "Family";
      }
      nullable = viewModel.TypeCheckBoxBool;
      if (nullable.GetValueOrDefault())
      {
        this.task.isTypeParameter = "Type";
      }
      else
      {
        nullable = viewModel.InstanceCheckBoxBool;
        this.task.isTypeParameter = !nullable.GetValueOrDefault() ? (this.task.action != BulkFamilyUpdatersUtils.taskActions.Delete ? "unchanged" : "") : "Instance";
      }
      this.task.discipline = viewModel.SelectedDisciplineItem.Trim();
      this.task.specType = viewModel.SelectedTypeItem.Trim();
      this.task.paramGroup = !string.IsNullOrWhiteSpace(viewModel.SelectedGroupUnderItem) ? viewModel.SelectedGroupUnderItem.Trim() : (this.task.action != BulkFamilyUpdatersUtils.taskActions.Delete ? "unchanged" : "");
      this.task.isValueFormula = viewModel.FormulaCheckBoxBool;
      this.task.clearValue = viewModel.ClearValueCheckBoxBool;
      if (this.task.clearValue.GetValueOrDefault())
      {
        this.task.value = "cleared";
      }
      else
      {
        if (BulkFamilyUpdatersUtils.boolSpecTypeIdList.Contains(key))
        {
          nullable = viewModel.FormulaCheckBoxBool;
          if (!nullable.GetValueOrDefault() && this.action != BulkFamilyUpdatersUtils.taskActions.Delete)
          {
            nullable = viewModel.YesCheckBoxBool;
            if (nullable.GetValueOrDefault())
            {
              this.task.value = "Yes";
              goto label_77;
            }
            nullable = viewModel.NoCheckBoxBool;
            this.task.value = !nullable.GetValueOrDefault() ? "unchanged" : "No";
            goto label_77;
          }
        }
        if (!string.IsNullOrWhiteSpace(viewModel.ParameterValue))
        {
          nullable = viewModel.FormulaCheckBoxBool;
          if (nullable.GetValueOrDefault())
          {
            this.task.value = viewModel.ParameterValue;
          }
          else
          {
            string outputString;
            if (this.formatForFamilyUpdaterTask(out outputString))
            {
              this.task.value = outputString;
            }
            else
            {
              this.task = new FamilyUpdaterTask(original);
              return;
            }
          }
        }
        else
          this.task.value = this.task.action != BulkFamilyUpdatersUtils.taskActions.Delete ? (this.task.action != BulkFamilyUpdatersUtils.taskActions.Add ? "unchanged" : "unassigned") : "";
      }
label_77:
      if (!string.IsNullOrWhiteSpace(viewModel.SelectedGroupUnderItem) && viewModel.SelectedTypeItem == "Fill Pattern" && this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
      {
        if (new TaskDialog("Bulk Family Updater")
        {
          MainContent = "Changing the group in which a fill pattern parameter type is group under with this tool will result in its value beign removed. Are you sure you want to continue?",
          CommonButtons = ((TaskDialogCommonButtons) 10)
        }.Show() != 6)
        {
          this.task = new FamilyUpdaterTask(original);
          return;
        }
      }
      this.cancelWindow = false;
      this.Close();
    }
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void disciplineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    this.UpdateTypeComboBox();
  }

  private void UpdateTypeComboBox()
  {
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    viewModel.TypeItems.Clear();
    viewModel.SelectedTypeItem = (string) null;
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
      viewModel.TypeItems.Add(new ComboBoxItemString(""));
    if (!BulkFamilyUpdatersUtils.disciplineToString2223.ContainsValue(viewModel.SelectedDisciplineItem))
      return;
    KeyValuePair<ForgeTypeId, string> keyValuePair = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem));
    ForgeTypeId disc = keyValuePair.Key;
    if (BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary.ContainsKey(disc))
    {
      List<string> stringList = new List<string>();
      foreach (ForgeTypeId specTypeId in BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc])
        stringList.Add(LabelUtils.GetLabelForSpec(specTypeId));
      stringList.Sort();
      foreach (string str in stringList)
        viewModel.TypeItems.Add(new ComboBoxItemString(str));
    }
    if (this.task != null && !string.IsNullOrWhiteSpace(this.task.specType))
    {
      keyValuePair = BulkFamilyUpdatersUtils.specTypeIdToString2223.First<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == this.task.specType && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key)));
      if (!(keyValuePair.Key != (ForgeTypeId) null))
        return;
      viewModel.SelectedTypeItem = this.task.specType;
    }
    else
    {
      string valueString = viewModel.TypeItems.FirstOrDefault<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (x => !string.IsNullOrWhiteSpace(x.ValueString))).ValueString;
      viewModel.SelectedTypeItem = valueString;
    }
  }

  private void updateDisciplineTypeGroup(Definition parameter)
  {
    BFUParameterTaskViewModel dataContext = this.DataContext as BFUParameterTaskViewModel;
    ForgeTypeId dataType = parameter.GetDataType();
    if (BulkFamilyUpdatersUtils.doubleSpecTypeIdList.Contains(dataType))
    {
      dataContext.SelectedDisciplineItem = LabelUtils.GetLabelForDiscipline(UnitUtils.GetDiscipline(dataType));
      dataContext.SelectedTypeItem = LabelUtils.GetLabelForSpec(dataType);
    }
    else if (BulkFamilyUpdatersUtils.typeOfParameterToForgeTypeId2223.ContainsKey(dataType))
    {
      dataContext.SelectedDisciplineItem = LabelUtils.GetLabelForDiscipline(BulkFamilyUpdatersUtils.typeOfParameterToForgeTypeId2223[dataType]);
      dataContext.SelectedTypeItem = LabelUtils.GetLabelForSpec(dataType);
    }
    else
    {
      dataContext.SelectedDisciplineItem = LabelUtils.GetLabelForDiscipline(DisciplineTypeId.Common);
      dataContext.SelectedTypeItem = LabelUtils.GetLabelForSpec(dataType);
    }
  }

  public FamilyUpdaterTask getUpdaterTask() => this.task;

  private void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem)).Key;
    ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
    viewModel.ParameterValueUnits = !(key != (ForgeTypeId) null) ? "" : BulkFamilyUpdatersUtils.getUnitsSymboString(this.revitDoc, key);
    if (BulkFamilyUpdatersUtils.boolSpecTypeIdList.Contains(key))
    {
      this.valueTextBox.IsEnabled = false;
      this.yesValueCheckBox.Visibility = System.Windows.Visibility.Visible;
      this.noValueCheckBox.Visibility = System.Windows.Visibility.Visible;
      if (this.action != BulkFamilyUpdatersUtils.taskActions.Delete)
      {
        this.yesValueCheckBox.IsEnabled = true;
        this.noValueCheckBox.IsEnabled = true;
      }
      if (this.action == BulkFamilyUpdatersUtils.taskActions.Add && !this.editWindow)
        viewModel.YesCheckBoxBool = new bool?(true);
      if (!viewModel.FormulaCheckBoxBool.GetValueOrDefault())
      {
        viewModel.ParameterValue = "";
        this.valueTextBox.Visibility = System.Windows.Visibility.Collapsed;
      }
      this.clearValueCheckBox.Visibility = System.Windows.Visibility.Collapsed;
      viewModel.ClearValueCheckBoxBool = new bool?(false);
    }
    else
    {
      if (this.action != BulkFamilyUpdatersUtils.taskActions.Delete)
        this.valueTextBox.IsEnabled = true;
      this.yesValueCheckBox.Visibility = System.Windows.Visibility.Collapsed;
      this.noValueCheckBox.Visibility = System.Windows.Visibility.Collapsed;
      this.valueTextBox.Visibility = System.Windows.Visibility.Visible;
      this.yesValueCheckBox.IsEnabled = false;
      this.noValueCheckBox.IsEnabled = false;
      if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
      {
        viewModel.YesCheckBoxBool = new bool?(false);
        viewModel.NoCheckBoxBool = new bool?(false);
      }
      if (this.action == BulkFamilyUpdatersUtils.taskActions.Modify)
        this.clearValueCheckBox.Visibility = System.Windows.Visibility.Visible;
    }
    if (key == SpecTypeId.Reference.Image)
    {
      this.instanceCheckBox.IsEnabled = false;
      viewModel.InstanceCheckBoxBool = new bool?(false);
      this.typeCheckBox.IsEnabled = false;
      if (this.action == BulkFamilyUpdatersUtils.taskActions.Add)
        viewModel.TypeCheckBoxBool = new bool?(true);
    }
    if (key == SpecTypeId.Reference.FillPattern)
    {
      viewModel.ParameterValue = "";
      viewModel.FormulaCheckBoxBool = new bool?(false);
      viewModel.ClearValueCheckBoxBool = new bool?(false);
      this.valueTextBox.IsEnabled = false;
      this.formulaCheckBox.IsEnabled = false;
      this.clearValueCheckBox.IsEnabled = false;
    }
    if (BulkFamilyUpdatersUtils.referenceSpecTypeIdList.Contains(key))
    {
      this.formulaCheckBox.IsEnabled = false;
      viewModel.FormulaCheckBoxBool = new bool?(false);
    }
    else
    {
      if (this.action == BulkFamilyUpdatersUtils.taskActions.Delete)
        return;
      this.formulaCheckBox.IsEnabled = true;
    }
  }

  private bool formatForFamilyUpdaterTask(out string outputString)
  {
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    outputString = viewModel.ParameterValue;
    if (string.IsNullOrWhiteSpace(outputString))
      return true;
    Units units = this.revitDoc.GetUnits();
    KeyValuePair<ForgeTypeId, string> keyValuePair = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem));
    ForgeTypeId disc = keyValuePair.Key;
    keyValuePair = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key)));
    ForgeTypeId key = keyValuePair.Key;
    if (BulkFamilyUpdatersUtils.intSpecTypeIdList.Contains(key))
    {
      bool flag = int.TryParse(outputString, out int _);
      if (flag)
        return flag;
      double result;
      if (double.TryParse(outputString, out result) && result % 1.0 == 0.0)
      {
        outputString = result.ToString();
        return true;
      }
      new TaskDialog("Bulk Family Updater")
      {
        MainContent = "The value input is invalid. This parameter type requires a integer value. Please change the value input and try again"
      }.Show();
      return false;
    }
    if (BulkFamilyUpdatersUtils.referenceSpecTypeIdList.Contains(key) || BulkFamilyUpdatersUtils.stringSpecTypeIdList.Contains(key))
      return true;
    FormatOptions formatOptions = units.GetFormatOptions(key);
    ForgeTypeId unitTypeId = formatOptions.GetUnitTypeId();
    List<ForgeTypeId> forgeTypeIdList1 = new List<ForgeTypeId>();
    forgeTypeIdList1.Add(UnitTypeId.FeetFractionalInches);
    forgeTypeIdList1.Add(UnitTypeId.RiseDividedBy12Inches);
    forgeTypeIdList1.Add(UnitTypeId.RiseDividedBy120Inches);
    forgeTypeIdList1.Add(UnitTypeId.FractionalInches);
    forgeTypeIdList1.Add(UnitTypeId.RiseDividedBy1Foot);
    forgeTypeIdList1.Add(UnitTypeId.RiseDividedBy10Feet);
    List<ForgeTypeId> forgeTypeIdList2 = new List<ForgeTypeId>()
    {
      UnitTypeId.FeetFractionalInches,
      UnitTypeId.RiseDividedBy1Foot,
      UnitTypeId.RiseDividedBy10Feet
    };
    if (!forgeTypeIdList1.Contains(unitTypeId))
    {
      List<char> charList = new List<char>()
      {
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        '0'
      };
      double num = 0.0;
      bool flag = false;
      foreach (char ch in outputString)
      {
        if (flag)
        {
          if (charList.Contains(ch))
            ++num;
          else
            break;
        }
        if (ch == '.')
          flag = true;
      }
      string str = "0.";
      for (int index = 0; (double) index < num - 1.0; ++index)
        str += "0";
      string s = str + "1";
      double result = 0.0;
      if (double.TryParse(s, out result) && result < formatOptions.Accuracy)
      {
        formatOptions.Accuracy = result <= 1E-12 ? 1E-12 : result;
        units.SetFormatOptions(key, formatOptions);
      }
    }
    else
    {
      formatOptions.Accuracy = !forgeTypeIdList2.Contains(unitTypeId) ? 1.0 / 256.0 : 0.00032552083333333332;
      units.SetFormatOptions(key, formatOptions);
    }
    double num1;
    if (UnitFormatUtils.TryParse(units, key, outputString, out num1))
    {
      outputString = UnitFormatUtils.Format(units, key, num1, true);
      return true;
    }
    new TaskDialog("Bulk Family Updater")
    {
      MainContent = "The value input is invalid. This parameter type requires a number. If you included any units, they maybe invalid. Please change the value input and try again"
    }.Show();
    return false;
  }

  private string formatForWindowDisplay()
  {
    BFUParameterTaskViewModel viewModel = this.DataContext as BFUParameterTaskViewModel;
    if (!BulkFamilyUpdatersUtils.disciplineToStringDictionary.ContainsValue(this.task.discipline) || !BulkFamilyUpdatersUtils.masterTypeOfParameterToStringDictionary.ContainsValue(this.task.specType))
      return this.task.value;
    ForgeTypeId disc = BulkFamilyUpdatersUtils.disciplineToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedDisciplineItem)).Key;
    ForgeTypeId key = BulkFamilyUpdatersUtils.specTypeIdToString2223.FirstOrDefault<KeyValuePair<ForgeTypeId, string>>((Func<KeyValuePair<ForgeTypeId, string>, bool>) (x => x.Value == viewModel.SelectedTypeItem && BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary[disc].Contains(x.Key))).Key;
    string str = this.task.value;
    if (string.IsNullOrWhiteSpace(str))
      return "";
    string unitsSymboString = BulkFamilyUpdatersUtils.getUnitsSymboString(this.revitDoc, key);
    if (!string.IsNullOrWhiteSpace(unitsSymboString))
      str = str.Replace(unitsSymboString, string.Empty);
    return str;
  }

  private void yesValueCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    (this.DataContext as BFUParameterTaskViewModel).NoCheckBoxBool = new bool?(false);
  }

  private void noCheckBox_Checked(object sender, RoutedEventArgs e)
  {
    (this.DataContext as BFUParameterTaskViewModel).YesCheckBoxBool = new bool?(false);
  }

  private void yesValueCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
      return;
    (this.DataContext as BFUParameterTaskViewModel).NoCheckBoxBool = new bool?(true);
  }

  private void noValueCheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    if (this.action != BulkFamilyUpdatersUtils.taskActions.Add)
      return;
    (this.DataContext as BFUParameterTaskViewModel).YesCheckBoxBool = new bool?(true);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/__testing/bulkfamilyupdater/views/bfuparametertaskwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.typeInstanceRow = (RowDefinition) target;
        break;
      case 2:
        this.groupRow = (RowDefinition) target;
        break;
      case 3:
        this.valueRow = (RowDefinition) target;
        break;
      case 4:
        this.parameterTypeLabel = (Label) target;
        break;
      case 5:
        this.familyParameterButton = (Button) target;
        this.familyParameterButton.Click += new RoutedEventHandler(this.familyParameterButton_Click);
        break;
      case 6:
        this.sharedParameterButton = (Button) target;
        this.sharedParameterButton.Click += new RoutedEventHandler(this.sharedParameterButton_Click);
        break;
      case 7:
        this.parameterNameLabel = (Label) target;
        break;
      case 8:
        this.currentParameterNameLabel = (TextBox) target;
        break;
      case 9:
        this.parameterNameTextBox = (TextBox) target;
        break;
      case 10:
        this.typeCheckBox = (CheckBox) target;
        this.typeCheckBox.Checked += new RoutedEventHandler(this.typeCheckBox_Checked);
        this.typeCheckBox.Unchecked += new RoutedEventHandler(this.typeCheckBox_Unchecked);
        break;
      case 11:
        this.instanceCheckBox = (CheckBox) target;
        this.instanceCheckBox.Checked += new RoutedEventHandler(this.instanceCheckBox_Checked);
        this.instanceCheckBox.Unchecked += new RoutedEventHandler(this.instanceCheckBox_Unchecked);
        break;
      case 12:
        this.disciplineLabel = (Label) target;
        break;
      case 13:
        this.disciplineComboBox = (ComboBox) target;
        this.disciplineComboBox.SelectionChanged += new SelectionChangedEventHandler(this.disciplineComboBox_SelectionChanged);
        break;
      case 14:
        this.typeLabel = (Label) target;
        break;
      case 15:
        this.typeComboBox = (ComboBox) target;
        this.typeComboBox.SelectionChanged += new SelectionChangedEventHandler(this.typeComboBox_SelectionChanged);
        break;
      case 16 /*0x10*/:
        this.groupUnderLabel = (Label) target;
        break;
      case 17:
        this.groupUnderComboBox = (ComboBox) target;
        break;
      case 18:
        this.formulaEqualLabel = (Label) target;
        break;
      case 19:
        this.valueLabel = (Label) target;
        break;
      case 20:
        this.valueTextBox = (TextBox) target;
        break;
      case 21:
        this.unitLabel = (Label) target;
        break;
      case 22:
        this.yesValueCheckBox = (CheckBox) target;
        this.yesValueCheckBox.Checked += new RoutedEventHandler(this.yesValueCheckBox_Checked);
        this.yesValueCheckBox.Unchecked += new RoutedEventHandler(this.yesValueCheckBox_Unchecked);
        break;
      case 23:
        this.noValueCheckBox = (CheckBox) target;
        this.noValueCheckBox.Checked += new RoutedEventHandler(this.noCheckBox_Checked);
        this.noValueCheckBox.Unchecked += new RoutedEventHandler(this.noValueCheckBox_Unchecked);
        break;
      case 24:
        this.formulaCheckBox = (CheckBox) target;
        this.formulaCheckBox.Checked += new RoutedEventHandler(this.formulaCheckBox_Checked);
        this.formulaCheckBox.Unchecked += new RoutedEventHandler(this.formulaCheckBox_Unchecked);
        break;
      case 25:
        this.clearValueCheckBox = (CheckBox) target;
        this.clearValueCheckBox.Checked += new RoutedEventHandler(this.clearValueCheckBox_Checked);
        this.clearValueCheckBox.Unchecked += new RoutedEventHandler(this.clearValueCheckBox_Unchecked);
        break;
      case 26:
        this.oKButton = (Button) target;
        this.oKButton.Click += new RoutedEventHandler(this.oKButton_Click);
        break;
      case 27:
        this.cancelButton = (Button) target;
        this.cancelButton.Click += new RoutedEventHandler(this.cancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
