// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.CADExportSettingsWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class CADExportSettingsWindow : Window, IComponentConnector, IStyleConnector
{
  private string manufacturer = string.Empty;
  private Units units;
  private bool imperialUnits;
  private CADExportSettings cadExportSettings;
  private bool Canceled = true;
  private bool defaults;
  private bool CADLayerSetupTabSelected;
  private bool InsulationLayerSetupTabSelected;
  internal System.Windows.Controls.TabControl ToolTabMenu;
  internal TabItem CADExportSettingsTab;
  internal System.Windows.Controls.TabControl CADExportTabMenu;
  internal System.Windows.Controls.TextBox CADExportFolderPathTextBox;
  internal System.Windows.Controls.Button CADExportFilePathButton;
  internal System.Windows.Controls.Label CADExportFileNameSuffixLabel;
  internal System.Windows.Controls.TextBox CADExportFileNameSuffixTextBox;
  internal System.Windows.Controls.Label CADExportFileTypeLabel;
  internal System.Windows.Controls.CheckBox CADExportDWGFileTypeCheckBox;
  internal System.Windows.Controls.CheckBox CADExportDXFFileTypeCheckBox;
  internal System.Windows.Controls.DataGrid CADExportLayerDataGrid;
  internal System.Windows.Controls.ComboBox TIFLayerComboBox;
  internal System.Windows.Controls.RadioButton TIFExportBoundingBoxRadioButton;
  internal System.Windows.Controls.RadioButton TIFExportFixedCircle;
  internal System.Windows.Controls.ComboBox BIFLayerComboBox;
  internal System.Windows.Controls.RadioButton BIFExportBoundingBoxRadioButton;
  internal System.Windows.Controls.RadioButton BIFExportFixedCircle;
  internal System.Windows.Controls.ComboBox SIFLayerComboBox;
  internal System.Windows.Controls.TextBox FixedCircleRadiusTextBox;
  internal System.Windows.Controls.TextBox TickMarkLengthTextBox;
  internal System.Windows.Controls.CheckBox EdgeCadLinesCheckBox;
  internal System.Windows.Controls.ComboBox FormLayerComboBox;
  internal System.Windows.Controls.ComboBox CorbelLayerComboBox;
  internal System.Windows.Controls.ComboBox ArchLayerComboBox;
  internal System.Windows.Controls.ComboBox StrandLayerComboBox;
  internal TabItem InsulationExportSettingsTab;
  internal System.Windows.Controls.TabControl InsulationExportTabMenu;
  internal System.Windows.Controls.TextBox InsulationExportFolderPathTextBox;
  internal System.Windows.Controls.Button InsulationExportFilePathButton;
  internal System.Windows.Controls.Label InsulationExportFileNameSuffixLabel;
  internal System.Windows.Controls.TextBox InsulationExportFileNameSuffixTextBox;
  internal System.Windows.Controls.Label InsulationExportFileTypeLabel;
  internal System.Windows.Controls.CheckBox InsulationExportDWGFileTypeCheckBox;
  internal System.Windows.Controls.CheckBox InsulationExportDXFFileTypeCheckBox;
  internal System.Windows.Controls.DataGrid InsulationExportLayerDataGrid;
  internal System.Windows.Controls.ComboBox InsulationLayerComboBox;
  internal System.Windows.Controls.ComboBox CutoutsLayerComboBox;
  internal System.Windows.Controls.ComboBox PinsLayerComboBox;
  internal System.Windows.Controls.ComboBox SlotLayerComboBox;
  internal System.Windows.Controls.ComboBox ExtraTiesLayerComboBox;
  internal System.Windows.Controls.ComboBox MarkSymbolLayerComboBox;
  internal System.Windows.Controls.ComboBox InsulationMarkLayerComboBox;
  internal System.Windows.Controls.Button ResetButton;
  internal System.Windows.Controls.Button SaveButton;
  internal System.Windows.Controls.Button CancelButton;
  private bool _contentLoaded;

  public CADExportSettingsWindow(
    IntPtr parentWindowHandler,
    Units units,
    string manufacturer = null,
    bool imperialUnits = true)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.manufacturer = manufacturer;
    this.units = units;
    this.imperialUnits = imperialUnits;
    UnitValidationParameters.units = units;
    UnitValidationParameters.imperialUnits = imperialUnits;
    CADExportSettings settings = new CADExportSettings(true);
    if (!settings.GetManufacturerSettings(manufacturer))
    {
      new TaskDialog("CAD Export Settings")
      {
        MainInstruction = "Unable to read CAD Export Settings file. The CAD Export Settings window will display default settings and create a new file when saved.",
        CommonButtons = ((TaskDialogCommonButtons) 1)
      }.Show();
      this.defaults = true;
    }
    this.cadExportSettings = settings.Clone();
    this.DataContext = (object) new CADExportSettingsDataViewModel(settings, this.imperialUnits);
    this.Title = $"{this.Title} - {manufacturer}";
  }

  private void Delete_Click(object sender, RoutedEventArgs e)
  {
    if (!(sender is System.Windows.Controls.Button button) || !(button.DataContext is CADExportLayer dataContext1) || !(this.DataContext is CADExportSettingsDataViewModel dataContext2))
      return;
    if (this.CADExportSettingsTab.IsSelected)
    {
      dataContext2.CADExportLayersDataGridList.Remove(dataContext1);
      dataContext2.CADExportLayersDataGridList = dataContext2.CADExportLayersDataGridList;
      if (dataContext2.TIFLayer != null && dataContext2.TIFLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.TIFLayer = (CADExportLayer) null;
      if (dataContext2.BIFLayer != null && dataContext2.BIFLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.BIFLayer = (CADExportLayer) null;
      if (dataContext2.SIFLayer != null && dataContext2.SIFLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.SIFLayer = (CADExportLayer) null;
      if (dataContext2.FormLayer != null && dataContext2.FormLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.FormLayer = (CADExportLayer) null;
      if (dataContext2.CorbelLayer != null && dataContext2.CorbelLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.CorbelLayer = (CADExportLayer) null;
      if (dataContext2.ArchLayer != null && dataContext2.ArchLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.ArchLayer = (CADExportLayer) null;
      if (dataContext2.StrandLayer == null || !dataContext2.StrandLayer.LayerName.Equals(dataContext1.LayerName))
        return;
      dataContext2.StrandLayer = (CADExportLayer) null;
    }
    else
    {
      dataContext2.InsulationExportLayersDataGridList.Remove(dataContext1);
      dataContext2.InsulationExportLayersDataGridList = dataContext2.InsulationExportLayersDataGridList;
      if (dataContext2.InsulationLayer != null && dataContext2.InsulationLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.InsulationLayer = (CADExportLayer) null;
      if (dataContext2.CutoutsLayer != null && dataContext2.CutoutsLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.CutoutsLayer = (CADExportLayer) null;
      if (dataContext2.PinsLayer != null && dataContext2.PinsLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.PinsLayer = (CADExportLayer) null;
      if (dataContext2.SlotLayer != null && dataContext2.SlotLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.SlotLayer = (CADExportLayer) null;
      if (dataContext2.ExtraTiesLayer != null && dataContext2.ExtraTiesLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.ExtraTiesLayer = (CADExportLayer) null;
      if (dataContext2.MarkSymbolLayer != null && dataContext2.MarkSymbolLayer.LayerName.Equals(dataContext1.LayerName))
        dataContext2.MarkSymbolLayer = (CADExportLayer) null;
      if (dataContext2.InsulationMarkLayer == null || !dataContext2.InsulationMarkLayer.LayerName.Equals(dataContext1.LayerName))
        return;
      dataContext2.InsulationMarkLayer = (CADExportLayer) null;
    }
  }

  private void TabMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (!(sender is System.Windows.Controls.TabControl tabControl) || tabControl.SelectedItem == null)
      return;
    TabItem selectedItem = tabControl.SelectedItem as TabItem;
    if (!(selectedItem.Header is string header))
      return;
    if (selectedItem != null && header == "Layer Setup")
    {
      if (tabControl.Name == "CADExportTabMenu")
      {
        this.CADLayerSetupTabSelected = true;
      }
      else
      {
        if (!(tabControl.Name == "InsulationExportTabMenu"))
          return;
        this.InsulationLayerSetupTabSelected = true;
      }
    }
    else
    {
      if (!header.Contains("Layers"))
        return;
      if (tabControl.Name == "CADExportTabMenu")
      {
        if (!this.CADLayerSetupTabSelected || !(this.DataContext is CADExportSettingsDataViewModel dataContext))
          return;
        this.CADLayerSetupTabSelected = false;
        dataContext.CADExportLayerComboBoxesList = dataContext.getComboBoxList(dataContext.CADExportLayersDataGridList);
        this.TIFLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.TIFLayerComboBox, dataContext.TIFLayer);
        this.BIFLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.BIFLayerComboBox, dataContext.BIFLayer);
        this.SIFLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.SIFLayerComboBox, dataContext.SIFLayer);
        this.FormLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.FormLayerComboBox, dataContext.FormLayer);
        this.CorbelLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.CorbelLayerComboBox, dataContext.CorbelLayer);
        this.ArchLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.ArchLayerComboBox, dataContext.ArchLayer);
        this.StrandLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.StrandLayerComboBox, dataContext.StrandLayer);
      }
      else
      {
        if (!(tabControl.Name == "InsulationExportTabMenu") || !this.InsulationLayerSetupTabSelected || !(this.DataContext is CADExportSettingsDataViewModel dataContext))
          return;
        this.InsulationLayerSetupTabSelected = false;
        dataContext.InsulationExportLayerComboBoxesList = dataContext.getComboBoxList(dataContext.InsulationExportLayersDataGridList);
        this.InsulationLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.InsulationLayerComboBox, dataContext.InsulationLayer);
        this.CutoutsLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.CutoutsLayerComboBox, dataContext.CutoutsLayer);
        this.PinsLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.PinsLayerComboBox, dataContext.PinsLayer);
        this.SlotLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.SlotLayerComboBox, dataContext.SlotLayer);
        this.ExtraTiesLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.ExtraTiesLayerComboBox, dataContext.ExtraTiesLayer);
        this.MarkSymbolLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.MarkSymbolLayerComboBox, dataContext.MarkSymbolLayer);
        this.InsulationMarkLayerComboBox.SelectedIndex = this.GetSelectedIndexValue(this.InsulationMarkLayerComboBox, dataContext.InsulationMarkLayer);
      }
    }
  }

  private int GetSelectedIndexValue(System.Windows.Controls.ComboBox comboBox, CADExportLayer layer)
  {
    if (layer == null)
      return 0;
    ItemCollection items = comboBox.Items;
    if (items == null)
      return 0;
    int count = items.Count;
    for (int index = 0; index < count; ++index)
    {
      if (items.GetItemAt(index) is CADExportLayer itemAt && layer.Equals(itemAt))
        return index;
    }
    return 0;
  }

  private void ExportFilePathButton_Click(object sender, RoutedEventArgs e)
  {
    if (!(sender is System.Windows.Controls.Button button) || !(this.DataContext is CADExportSettingsDataViewModel dataContext))
      return;
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (button.Name == "CADExportFilePathButton")
    {
      folderBrowserDialog.Description = "Please Select CAD Export Folder Path:";
      if (Directory.Exists(dataContext.CADExportFolderPathString))
        folderBrowserDialog.SelectedPath = dataContext.CADExportFolderPathString;
    }
    else if (button.Name == "InsulationExportFilePathButton")
    {
      folderBrowserDialog.Description = "Please Select Insulation Export Folder Path:";
      if (Directory.Exists(dataContext.InsulationExportFolderPathString))
        folderBrowserDialog.SelectedPath = dataContext.InsulationExportFolderPathString;
    }
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    string selectedPath = folderBrowserDialog.SelectedPath;
    if (button.Name == "CADExportFilePathButton")
    {
      dataContext.CADExportFolderPathString = selectedPath;
    }
    else
    {
      if (!(button.Name == "InsulationExportFilePathButton"))
        return;
      dataContext.InsulationExportFolderPathString = selectedPath;
    }
  }

  private void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.DataContext is CADExportSettingsDataViewModel dataContext)
    {
      List<string> stringList = new List<string>();
      if (Validation.GetHasError((DependencyObject) this.CADExportFolderPathTextBox))
        stringList.Add("CAD Export Settings - Export Folder Path");
      if (Validation.GetHasError((DependencyObject) this.CADExportFileNameSuffixTextBox))
        stringList.Add("CAD Export Settings - File Name Suffix");
      if (dataContext.CADFileExportTypeError)
        stringList.Add("CAD Export Settings - File Type Exported must have at least one type selected.");
      if (Validation.GetHasError((DependencyObject) this.InsulationExportFolderPathTextBox))
        stringList.Add("Insulation Export Settings - Export Folder Path");
      if (Validation.GetHasError((DependencyObject) this.InsulationExportFileNameSuffixTextBox))
        stringList.Add("Insulation Export Settings - File Name Suffix");
      if (dataContext.InsulationFileExportTypeLayer)
        stringList.Add("Insulation Export Settings - File Type Exported must have at least one type selected.");
      if (Validation.GetHasError((DependencyObject) this.FixedCircleRadiusTextBox))
        stringList.Add("CAD Export Settings - Fixed Circle Radius");
      if (Validation.GetHasError((DependencyObject) this.TickMarkLengthTextBox))
        stringList.Add("CAD Export Settings - Tick Mark Length");
      if (dataContext.CADExportLayersDataGridList.Where<CADExportLayer>((Func<CADExportLayer, bool>) (x => x.LayerError)).Any<CADExportLayer>())
        stringList.Add("CAD Export Settings - Layer Setup");
      if (dataContext.InsulationExportLayersDataGridList.Where<CADExportLayer>((Func<CADExportLayer, bool>) (x => x.LayerError)).Any<CADExportLayer>())
        stringList.Add("Insulation Export Settings - Layer Setup");
      if (stringList.Count > 0)
      {
        TaskDialog taskDialog1 = new TaskDialog("CAD Export Settings");
        taskDialog1.MainInstruction = "One or more fields have invalid settings. Please correct these fields and try to save again.";
        stringList.Sort();
        foreach (string str in stringList)
        {
          TaskDialog taskDialog2 = taskDialog1;
          taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str}\n";
        }
        taskDialog1.Show();
        return;
      }
      CADExportSettings settings = new CADExportSettings(true);
      CADExportSettings cadExportSettings = dataContext.updateSettings(settings, this.units, this.imperialUnits);
      if (!cadExportSettings.Equals(this.cadExportSettings) || this.defaults)
      {
        this.cadExportSettings = cadExportSettings.Clone();
        this.defaults = false;
        if (this.cadExportSettings.SaveCADExportSettings(this.manufacturer))
        {
          TaskDialog.Show("CAD Export Settings", "CAD Export Settings File successfully saved.");
          return;
        }
      }
      else
      {
        TaskDialog.Show("CAD Export Settings", "Current settings already match CAD Export Settings File.");
        return;
      }
    }
    TaskDialog.Show("CAD Export Settings", "Unable to save CAD Export Settings File.");
  }

  private void ResetButton_Click(object sender, RoutedEventArgs e)
  {
    if (new TaskDialog("CAD Export Settings")
    {
      MainInstruction = "Would you like to restore both CAD Export and Insulation Export settings to defaults? Any unsaved custom settings will be lost.",
      CommonButtons = ((TaskDialogCommonButtons) 6)
    }.Show() != 6)
      return;
    CADExportSettings settings = new CADExportSettings(true);
    if (!(this.DataContext is CADExportSettingsDataViewModel dataContext))
      return;
    dataContext.UpdateSettings(settings, this.imperialUnits);
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void Window_Closing(object sender, CancelEventArgs e)
  {
    if (!this.Canceled || this.defaults || !(this.DataContext is CADExportSettingsDataViewModel dataContext))
      return;
    if (this.cadExportSettings == null)
      return;
    CADExportSettings settings = this.cadExportSettings.Clone();
    List<string> fieldsChanged;
    if (dataContext.updateSettings(settings, this.units, this.imperialUnits).Equals(this.cadExportSettings, out fieldsChanged, true))
      return;
    TaskDialog taskDialog1 = new TaskDialog("CAD Export Settings");
    taskDialog1.MainContent = "Do you wish to close the CAD Export Settings without saving? There are currently unsaved changes that will be lost. To save these changes, select No and click the Save button.";
    if (fieldsChanged.Count > 0)
    {
      fieldsChanged.Sort();
      foreach (string str in fieldsChanged)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str}\n";
      }
    }
    taskDialog1.CommonButtons = (TaskDialogCommonButtons) 6;
    if (taskDialog1.Show() == 6)
      return;
    e.Cancel = true;
  }

  private void CADExportDataGridValidateRows(object sender, RoutedEventArgs e)
  {
    this.ValidateDataGridNames(this.CADExportLayerDataGrid);
  }

  private void InsulationExportDataGridValidateRows(object sender, RoutedEventArgs e)
  {
    this.ValidateDataGridNames(this.InsulationExportLayerDataGrid);
  }

  private void ValidateDataGridNames(System.Windows.Controls.DataGrid dataGrid)
  {
    Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
    int num = 0;
    foreach (object obj in (IEnumerable) dataGrid.Items)
    {
      if (obj is CADExportLayer cadExportLayer)
      {
        cadExportLayer.LayerError = false;
        if (string.IsNullOrWhiteSpace(cadExportLayer.LayerName))
          cadExportLayer.LayerError = true;
        else if (cadExportLayer.LayerName.ToUpper() == "(DISABLED)")
          cadExportLayer.LayerError = true;
        else if (cadExportLayer.LayerName.Length > (int) byte.MaxValue)
          cadExportLayer.LayerError = true;
        else if (cadExportLayer.LayerColor == null)
        {
          cadExportLayer.LayerError = true;
        }
        else
        {
          foreach (char ch in "<>/\\”:;?*|,=’".ToCharArray())
          {
            if (cadExportLayer.LayerName.Contains<char>(ch))
            {
              cadExportLayer.LayerError = true;
              break;
            }
          }
        }
        if (dictionary.ContainsKey(cadExportLayer.LayerName.ToUpper().Trim()))
          dictionary[cadExportLayer.LayerName.ToUpper().Trim()].Add(num);
        else
          dictionary.Add(cadExportLayer.LayerName.ToUpper().Trim(), new List<int>()
          {
            num
          });
      }
      ++num;
    }
    foreach (string key in dictionary.Keys)
    {
      if (dictionary[key].Count > 1)
      {
        foreach (int index in dictionary[key])
        {
          if (dataGrid.Items[index] is CADExportLayer cadExportLayer)
            cadExportLayer.LayerError = true;
        }
      }
    }
  }

  private void LayerDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
  {
    if (!(sender is System.Windows.Controls.DataGrid dataGrid))
      return;
    CADExportSettingsDataViewModel dataContext = this.DataContext as CADExportSettingsDataViewModel;
    if (dataGrid == null)
      return;
    if (dataGrid.Name.Contains("CADExport"))
    {
      List<CADExportLayer> list = new List<CADExportLayer>();
      foreach (CADExportLayer exportLayersDataGrid in (Collection<CADExportLayer>) dataContext.CADExportLayersDataGridList)
      {
        if (!string.IsNullOrWhiteSpace(exportLayersDataGrid.LayerName) || exportLayersDataGrid.LayerColor != null)
        {
          exportLayersDataGrid.DeleteEnabled = true;
          list.Add(exportLayersDataGrid);
        }
      }
      if (list.Count == dataContext.CADExportLayersDataGridList.Count)
        return;
      dataContext.CADExportLayersDataGridList = new ObservableCollection<CADExportLayer>(list);
    }
    else
    {
      if (!dataGrid.Name.Contains("Insulation"))
        return;
      List<CADExportLayer> list = new List<CADExportLayer>();
      foreach (CADExportLayer exportLayersDataGrid in (Collection<CADExportLayer>) dataContext.InsulationExportLayersDataGridList)
      {
        if (!string.IsNullOrWhiteSpace(exportLayersDataGrid.LayerName) || exportLayersDataGrid.LayerColor != null)
        {
          exportLayersDataGrid.DeleteEnabled = true;
          list.Add(exportLayersDataGrid);
        }
      }
      if (list.Count == dataContext.InsulationExportLayersDataGridList.Count)
        return;
      dataContext.InsulationExportLayersDataGridList = new ObservableCollection<CADExportLayer>(list);
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/cad%20export%20settings/cadexportsettingswindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Window) target).Closing += new CancelEventHandler(this.Window_Closing);
        break;
      case 2:
        this.ToolTabMenu = (System.Windows.Controls.TabControl) target;
        break;
      case 3:
        this.CADExportSettingsTab = (TabItem) target;
        break;
      case 4:
        this.CADExportTabMenu = (System.Windows.Controls.TabControl) target;
        this.CADExportTabMenu.SelectionChanged += new SelectionChangedEventHandler(this.TabMenu_SelectionChanged);
        break;
      case 5:
        this.CADExportFolderPathTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 6:
        this.CADExportFilePathButton = (System.Windows.Controls.Button) target;
        this.CADExportFilePathButton.Click += new RoutedEventHandler(this.ExportFilePathButton_Click);
        break;
      case 7:
        this.CADExportFileNameSuffixLabel = (System.Windows.Controls.Label) target;
        break;
      case 8:
        this.CADExportFileNameSuffixTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 9:
        this.CADExportFileTypeLabel = (System.Windows.Controls.Label) target;
        break;
      case 10:
        this.CADExportDWGFileTypeCheckBox = (System.Windows.Controls.CheckBox) target;
        break;
      case 11:
        this.CADExportDXFFileTypeCheckBox = (System.Windows.Controls.CheckBox) target;
        break;
      case 12:
        this.CADExportLayerDataGrid = (System.Windows.Controls.DataGrid) target;
        this.CADExportLayerDataGrid.RowEditEnding += new EventHandler<DataGridRowEditEndingEventArgs>(this.LayerDataGrid_RowEditEnding);
        break;
      case 16 /*0x10*/:
        this.TIFLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 17:
        this.TIFExportBoundingBoxRadioButton = (System.Windows.Controls.RadioButton) target;
        break;
      case 18:
        this.TIFExportFixedCircle = (System.Windows.Controls.RadioButton) target;
        break;
      case 19:
        this.BIFLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 20:
        this.BIFExportBoundingBoxRadioButton = (System.Windows.Controls.RadioButton) target;
        break;
      case 21:
        this.BIFExportFixedCircle = (System.Windows.Controls.RadioButton) target;
        break;
      case 22:
        this.SIFLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 23:
        this.FixedCircleRadiusTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 24:
        this.TickMarkLengthTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 25:
        this.EdgeCadLinesCheckBox = (System.Windows.Controls.CheckBox) target;
        break;
      case 26:
        this.FormLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 27:
        this.CorbelLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 28:
        this.ArchLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 29:
        this.StrandLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 30:
        this.InsulationExportSettingsTab = (TabItem) target;
        break;
      case 31 /*0x1F*/:
        this.InsulationExportTabMenu = (System.Windows.Controls.TabControl) target;
        this.InsulationExportTabMenu.SelectionChanged += new SelectionChangedEventHandler(this.TabMenu_SelectionChanged);
        break;
      case 32 /*0x20*/:
        this.InsulationExportFolderPathTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 33:
        this.InsulationExportFilePathButton = (System.Windows.Controls.Button) target;
        this.InsulationExportFilePathButton.Click += new RoutedEventHandler(this.ExportFilePathButton_Click);
        break;
      case 34:
        this.InsulationExportFileNameSuffixLabel = (System.Windows.Controls.Label) target;
        break;
      case 35:
        this.InsulationExportFileNameSuffixTextBox = (System.Windows.Controls.TextBox) target;
        break;
      case 36:
        this.InsulationExportFileTypeLabel = (System.Windows.Controls.Label) target;
        break;
      case 37:
        this.InsulationExportDWGFileTypeCheckBox = (System.Windows.Controls.CheckBox) target;
        break;
      case 38:
        this.InsulationExportDXFFileTypeCheckBox = (System.Windows.Controls.CheckBox) target;
        break;
      case 39:
        this.InsulationExportLayerDataGrid = (System.Windows.Controls.DataGrid) target;
        this.InsulationExportLayerDataGrid.RowEditEnding += new EventHandler<DataGridRowEditEndingEventArgs>(this.LayerDataGrid_RowEditEnding);
        break;
      case 43:
        this.InsulationLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 44:
        this.CutoutsLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 45:
        this.PinsLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 46:
        this.SlotLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 47:
        this.ExtraTiesLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 48 /*0x30*/:
        this.MarkSymbolLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 49:
        this.InsulationMarkLayerComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 50:
        this.ResetButton = (System.Windows.Controls.Button) target;
        this.ResetButton.Click += new RoutedEventHandler(this.ResetButton_Click);
        break;
      case 51:
        this.SaveButton = (System.Windows.Controls.Button) target;
        this.SaveButton.Click += new RoutedEventHandler(this.SaveButton_Click);
        break;
      case 52:
        this.CancelButton = (System.Windows.Controls.Button) target;
        this.CancelButton.Click += new RoutedEventHandler(this.CancelButton_Click);
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
    switch (connectionId)
    {
      case 13:
        ((UIElement) target).LostFocus += new RoutedEventHandler(this.CADExportDataGridValidateRows);
        break;
      case 14:
        ((UIElement) target).LostFocus += new RoutedEventHandler(this.CADExportDataGridValidateRows);
        break;
      case 15:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.Delete_Click);
        break;
      case 40:
        ((UIElement) target).LostFocus += new RoutedEventHandler(this.InsulationExportDataGridValidateRows);
        break;
      case 41:
        ((UIElement) target).LostFocus += new RoutedEventHandler(this.InsulationExportDataGridValidateRows);
        break;
      case 42:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.Delete_Click);
        break;
    }
  }
}
