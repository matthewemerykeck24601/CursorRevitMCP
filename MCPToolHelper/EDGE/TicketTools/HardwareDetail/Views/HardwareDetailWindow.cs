// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.HardwareDetailWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.ElementUtils;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class HardwareDetailWindow : Window, IComponentConnector
{
  public bool canceled;
  public bool error;
  public string errorMessage = string.Empty;
  public List<string> selectedTemplates = new List<string>();
  private HardwareDetailTemplateSettings templateSettings;
  private string filePath = App.HardwareDetailTemplateSettingsPath;
  private Document revitDoc;
  private string selectedManufacturerString = HardwareDetailWindowViewModel.manufacturerDefaultString;
  private List<string> titleBlockNames = new List<string>();
  private string multipleTemplates = "<multiple templates selected>";
  private ScaleUnits ScaleUnits;
  private bool populate;
  internal System.Windows.Controls.Label templateManufactureLabel;
  internal System.Windows.Controls.ComboBox manufactureListComboBox;
  internal System.Windows.Controls.Label templateLabel;
  internal System.Windows.Controls.ComboBox templateListComboBox;
  internal System.Windows.Controls.Button selectMultipleButton;
  internal System.Windows.Controls.Label scaleLabel;
  internal System.Windows.Controls.ComboBox scaleComboBox;
  internal System.Windows.Controls.Label titleBlockLabel;
  internal System.Windows.Controls.ComboBox titleBlockComboBox;
  internal System.Windows.Controls.Label copyLabel;
  internal System.Windows.Controls.ComboBox copySourceComboBox;
  internal System.Windows.Controls.Button clearButton;
  internal System.Windows.Controls.Button populateButton;
  internal System.Windows.Controls.Button cancelButton;
  private bool _contentLoaded;

  public HardwareDetailWindow(Document revitDoc, IntPtr parentWindowHandler, List<string> selected)
  {
    this.InitializeComponent();
    this.DataContext = (object) new HardwareDetailWindowViewModel();
    this.templateSettings = new HardwareDetailTemplateSettings();
    this.revitDoc = revitDoc;
    this.populateForm(selected);
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
  }

  private void populateForm(List<string> selected)
  {
    while (!this.LoadSettings())
    {
      string empty = string.Empty;
      string str1 = !string.IsNullOrWhiteSpace(this.filePath) ? $"Application Hardware Detail Template Settings have NOT been found in the file:{Environment.NewLine}{Environment.NewLine}{this.filePath}" : "Application Hardware Detail Template Settings File has NOT been defined";
      string str2 = $"Hardware Detail Template Settings Filename is stored in the file:{Environment.NewLine}{Environment.NewLine}     C:\\EDGEforREVIT\\EDGE_Preferences.txt";
      TaskDialog taskDialog = new TaskDialog("Application Settings not found.");
      taskDialog.MainInstruction = str1;
      taskDialog.FooterText = str2;
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Choose an alternate template settings file.", "Browse to a file on disk which contains Hardware Detail Template Settings");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      if (taskDialog.Show() == 2)
      {
        this.canceled = true;
        return;
      }
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.FileName = "openFileDialog1";
      openFileDialog.Title = "Select Hardware Detail Settings File";
      if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
      {
        this.canceled = true;
        return;
      }
      this.filePath = openFileDialog.FileName;
    }
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
    IOrderedEnumerable<string> orderedEnumerable = this.templateSettings.Templates.Where<HWDetailTemplate>((Func<HWDetailTemplate, bool>) (s => s.TemplateManufacturerName != null && s.TemplateManufacturerName != "<TemplateManufacturer>" && !string.IsNullOrWhiteSpace(s.TemplateManufacturerName))).Select<HWDetailTemplate, string>((Func<HWDetailTemplate, string>) (s => s.TemplateManufacturerName)).Distinct<string>().OrderBy<string, string>((Func<string, string>) (str => str));
    Parameter parameter1 = this.revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    string str3 = string.Empty;
    if (parameter1 != null && !string.IsNullOrEmpty(parameter1.AsString()))
      str3 = parameter1.AsString();
    string str4 = HardwareDetailWindowViewModel.manufacturerDefaultString;
    foreach (string str5 in (IEnumerable<string>) orderedEnumerable)
    {
      observableCollection.Add(new ComboBoxItemString(str5));
      if (str5.Equals(str3))
        str4 = str5;
    }
    observableCollection.Add(new ComboBoxItemString(HardwareDetailWindowViewModel.manufacturerDefaultString));
    if (string.IsNullOrWhiteSpace(dataContext.ManufacturerString))
      dataContext.ManufacturerString = HardwareDetailWindowViewModel.manufacturerDefaultString;
    dataContext.ManufacturerList = observableCollection;
    dataContext.ManufacturerString = str4;
    if (dataContext.ManufacturerList.Count > 1)
      this.manufactureListComboBox.IsEnabled = true;
    this.titleBlockNames = new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).Cast<FamilySymbol>().Select<FamilySymbol, string>((Func<FamilySymbol, string>) (p => p.Name)).ToList<string>();
    this.manufactureListComboBox.Text = dataContext.ManufacturerString;
    this.manufactureListComboBox.Items.Refresh();
    List<ViewSheet> list = new FilteredElementCollector(this.revitDoc).OfClass(typeof (ViewSheet)).Where<Element>((Func<Element, bool>) (view => (view as ViewSheet).AssociatedAssemblyInstanceId != ElementId.InvalidElementId)).Cast<ViewSheet>().ToList<ViewSheet>();
    Dictionary<ElementId, string> dictionary = new Dictionary<ElementId, string>();
    foreach (Autodesk.Revit.DB.View view in list)
    {
      AssemblyInstance element = this.revitDoc.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance;
      if (!dictionary.ContainsKey(element.Id))
      {
        Parameter parameter2 = Parameters.LookupParameter((Element) element, "HARDWARE_DETAIL");
        if (parameter2 != null && parameter2.AsInteger() == 1 && !selected.Contains(element.Name))
        {
          bool flag = false;
          foreach (string str6 in selected)
          {
            if (("_HWD-" + str6).Equals(element.Name))
            {
              flag = true;
              break;
            }
          }
          if (!flag)
            dictionary.Add(element.Id, element.Name);
        }
      }
    }
    List<ComboBoxItemString> comboBoxItemStringList = new List<ComboBoxItemString>();
    foreach (string str7 in dictionary.Values)
      comboBoxItemStringList.Add(new ComboBoxItemString(str7));
    comboBoxItemStringList.Sort((Comparison<ComboBoxItemString>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.ValueString, q.ValueString)));
    dataContext.CopyList = (ObservableCollection<ComboBoxItemString>) null;
    dataContext.CopyString = HardwareDetailWindowViewModel.copyDefaultString;
    foreach (ComboBoxItemString comboBoxItemString in comboBoxItemStringList)
      dataContext.CopyList.Add(comboBoxItemString);
    if (dataContext.CopyList.Count <= 1)
      return;
    this.copySourceComboBox.IsEnabled = true;
    this.clearButton.IsEnabled = true;
  }

  private bool LoadSettings()
  {
    return this.templateSettings.LoadHardwareDetailTemplateSettings(this.filePath);
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e)
  {
    this.canceled = true;
    this.Close();
  }

  private void manufactureListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    List<string> filteredTemplateList = this.getFilteredTemplateList(dataContext);
    if (filteredTemplateList.Count > 0)
    {
      dataContext.TemplateList = (ObservableCollection<ComboBoxItemString>) null;
      foreach (string str in filteredTemplateList)
        dataContext.TemplateList.Add(new ComboBoxItemString(str));
    }
    if (dataContext.TemplateList.Count > 0)
    {
      this.templateListComboBox.IsEnabled = true;
      this.templateListComboBox.SelectedIndex = 0;
    }
    this.templateListComboBox.Items.Refresh();
  }

  private void templateListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    if (string.IsNullOrWhiteSpace(dataContext.TemplateString) || dataContext.TemplateString == HardwareDetailWindowViewModel.templateDefaultString)
    {
      this.scaleComboBox.IsEnabled = false;
      this.titleBlockComboBox.IsEnabled = false;
      dataContext.ScaleList = (ObservableCollection<ComboBoxItemString>) null;
      dataContext.TitleBlockList = (ObservableCollection<ComboBoxItemString>) null;
      this.scaleComboBox.SelectedIndex = 0;
      this.titleBlockComboBox.SelectedIndex = 0;
      this.selectedTemplates.Clear();
    }
    else
    {
      List<string> filteredTemplateList = this.getFilteredTemplateList(dataContext);
      bool flag = false;
      List<string> list = dataContext.TemplateList.Select<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (x => x.ValueString)).ToList<string>();
      foreach (string str1 in list)
      {
        string str = str1;
        if (!filteredTemplateList.Contains(str) && dataContext.TemplateString != str)
        {
          int index = dataContext.TemplateList.ToList<ComboBoxItemString>().FindIndex((Predicate<ComboBoxItemString>) (x => x.ValueString == str));
          if (index > -1)
          {
            dataContext.TemplateList.RemoveAt(index);
            flag = true;
          }
        }
      }
      if (flag)
        this.templateListComboBox.Items.Refresh();
      if (dataContext.TemplateString != this.multipleTemplates)
      {
        this.selectedTemplates.Clear();
        if (list.Contains(dataContext.TemplateString))
          this.selectedTemplates.Add(dataContext.TemplateString);
      }
      this.fillTemplateFormData(dataContext);
    }
  }

  private void populateButton_Click(object sender, RoutedEventArgs e)
  {
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    if (string.IsNullOrWhiteSpace(dataContext.TemplateString) || dataContext.TemplateString == HardwareDetailWindowViewModel.templateDefaultString)
    {
      int num1 = (int) System.Windows.Forms.MessageBox.Show("Please select a template to continue.");
    }
    else if (string.IsNullOrWhiteSpace(dataContext.ScaleString) || dataContext.ScaleString == HardwareDetailWindowViewModel.scaleDefaultString)
    {
      int num2 = (int) System.Windows.Forms.MessageBox.Show("Please select a scale to continue.");
    }
    else if (string.IsNullOrWhiteSpace(dataContext.TitleBlockString) || !this.titleBlockNames.Contains(dataContext.TitleBlockString) && dataContext.TitleBlockString != "<<Varies>>")
    {
      if (this.selectedTemplates.Count<string>() > 1)
      {
        int num3 = (int) System.Windows.Forms.MessageBox.Show("One or more of the selected title blocks is not in the list of available title blocks. Please check selection.");
      }
      else
      {
        int num4 = (int) System.Windows.Forms.MessageBox.Show("Selected title block is not in the list of available title blocks. Please check selection.");
      }
    }
    else if (dataContext.TitleBlockString.StartsWith("<missing ") || dataContext.TitleBlockString == "<template title block families are missing from the project>")
    {
      new TaskDialog("Hardware Detail Warning")
      {
        MainInstruction = "Selected title block is not in the list of available title blocks. Please check selection"
      }.Show();
    }
    else
    {
      this.populate = true;
      this.canceled = false;
      this.Close();
    }
  }

  private void selectMultipleButton_Click(object sender, RoutedEventArgs e)
  {
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    HWDSelectMultipleWindow selectMultipleWindow = new HWDSelectMultipleWindow(this.getTemplateList(), this.selectedTemplates);
    selectMultipleWindow.ShowDialog();
    this.selectedTemplates = selectMultipleWindow.GetSelectedTemplates();
    if (this.selectedTemplates.Count > 1)
    {
      if (!dataContext.TemplateList.Select<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (x => x.ValueString)).Contains<string>(this.multipleTemplates))
        dataContext.TemplateList.Add(new ComboBoxItemString(this.multipleTemplates));
      dataContext.TemplateString = this.multipleTemplates;
      this.templateListComboBox.Items.Refresh();
    }
    else if (this.selectedTemplates.Count == 1)
    {
      dataContext.TemplateString = this.selectedTemplates[0];
      this.templateListComboBox.Items.Refresh();
    }
    else
      dataContext.TemplateString = HardwareDetailWindowViewModel.templateDefaultString;
  }

  private List<string> getTemplateList()
  {
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    List<string> templateList = new List<string>();
    foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) dataContext.TemplateList)
    {
      if (template.ValueString != HardwareDetailWindowViewModel.templateDefaultString && template.ValueString != this.multipleTemplates)
        templateList.Add(template.ValueString);
    }
    return templateList;
  }

  private void fillTemplateFormData(HardwareDetailWindowViewModel dataContext)
  {
    if (this.selectedTemplates.Count == 0)
      return;
    Dictionary<int, List<HWDetailTemplate>> dictionary1 = new Dictionary<int, List<HWDetailTemplate>>();
    List<HWDetailTemplate> source1 = new List<HWDetailTemplate>();
    foreach (HWDetailTemplate template1 in this.templateSettings.Templates)
    {
      HWDetailTemplate template = template1;
      if (this.selectedTemplates.Any<string>((Func<string, bool>) (e => e.Equals(template.TemplateName))))
        source1.Add(template);
    }
    Dictionary<int, List<HWDetailTemplate>> dictionary2 = new Dictionary<int, List<HWDetailTemplate>>();
    Dictionary<int, List<HWDetailTemplate>> dictionary3 = new Dictionary<int, List<HWDetailTemplate>>();
    foreach (HWDetailTemplate hwDetailTemplate in source1)
    {
      bool flag = true;
      foreach (AnchorGroup anchorGroup in hwDetailTemplate.AnchorGroups)
      {
        if (anchorGroup.MainView != null && anchorGroup.MainView.originalScale != hwDetailTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.TopView != null && anchorGroup.TopView.originalScale != hwDetailTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.BottomView != null && anchorGroup.BottomView.originalScale != hwDetailTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.LeftView != null && anchorGroup.LeftView.originalScale != hwDetailTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.RightView != null && anchorGroup.RightView.originalScale != hwDetailTemplate.TemplateScale)
          flag = false;
      }
      if (flag)
      {
        if (dictionary2.ContainsKey(hwDetailTemplate.TemplateScale))
          dictionary2[hwDetailTemplate.TemplateScale].Add(hwDetailTemplate);
        else
          dictionary2.Add(hwDetailTemplate.TemplateScale, new List<HWDetailTemplate>()
          {
            hwDetailTemplate
          });
      }
      else if (dictionary3.ContainsKey(hwDetailTemplate.TemplateScale))
        dictionary3[hwDetailTemplate.TemplateScale].Add(hwDetailTemplate);
      else
        dictionary3.Add(hwDetailTemplate.TemplateScale, new List<HWDetailTemplate>()
        {
          hwDetailTemplate
        });
    }
    if (dictionary2.Count + dictionary3.Count == 1)
    {
      List<HWDetailTemplate> source2 = new List<HWDetailTemplate>();
      foreach (KeyValuePair<int, List<HWDetailTemplate>> keyValuePair in dictionary2)
      {
        foreach (HWDetailTemplate hwDetailTemplate in keyValuePair.Value)
          source2.Add(hwDetailTemplate);
      }
      foreach (KeyValuePair<int, List<HWDetailTemplate>> keyValuePair in dictionary3)
      {
        foreach (HWDetailTemplate hwDetailTemplate in keyValuePair.Value)
          source2.Add(hwDetailTemplate);
      }
      HWDetailTemplate hwDetailTemplate1 = source2.First<HWDetailTemplate>();
      int num = hwDetailTemplate1.TemplateScale;
      if (dictionary3.Count > 0)
        num = -1;
      this.ScaleUnits = hwDetailTemplate1.ScaleUnits;
      List<ComboBoxItemString> list = ScalesManager.GetScalesList(hwDetailTemplate1.ScaleUnits).Select<string, ComboBoxItemString>((Func<string, ComboBoxItemString>) (s => new ComboBoxItemString(s))).ToList<ComboBoxItemString>();
      bool flag1 = true;
      string templateTitleBlock = hwDetailTemplate1.TitleBlockName;
      if (num > 0)
      {
        dataContext.ScaleList.Clear();
        foreach (ComboBoxItemString comboBoxItemString in list)
          dataContext.ScaleList.Add(comboBoxItemString);
        string strTemplateScale;
        if (ScalesManager.GetScaleStringForScaleFactor(hwDetailTemplate1.TemplateScale, hwDetailTemplate1.ScaleUnits, out strTemplateScale))
          dataContext.ScaleString = strTemplateScale;
        else if (dataContext.ScaleList.Where<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (x => x.ValueString == strTemplateScale)).Any<ComboBoxItemString>())
        {
          dataContext.ScaleString = strTemplateScale;
        }
        else
        {
          dataContext.ScaleList.Add(new ComboBoxItemString(strTemplateScale));
          dataContext.ScaleString = strTemplateScale;
        }
        if (source2.Count > 1 && !source2.All<HWDetailTemplate>((Func<HWDetailTemplate, bool>) (t => t.TitleBlockName == templateTitleBlock)))
        {
          flag1 = false;
          bool flag2 = true;
          foreach (HWDetailTemplate hwDetailTemplate2 in source2)
          {
            if (!dataContext.TitleBlockList.Select<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (x => x.ValueString)).Contains<string>(hwDetailTemplate2.TitleBlockName))
            {
              flag2 = false;
              break;
            }
          }
          dataContext.TitleBlockList.Clear();
          foreach (string titleBlockName in this.titleBlockNames)
            dataContext.TitleBlockList.Add(new ComboBoxItemString(titleBlockName));
          string newTitleBlockValue = string.Empty;
          newTitleBlockValue = !flag2 ? "<template title block families are missing from the project>" : "<<Varies>>";
          if (dataContext.TitleBlockList.Where<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (x => x.ValueString == newTitleBlockValue)).Any<ComboBoxItemString>())
          {
            dataContext.TitleBlockString = newTitleBlockValue;
          }
          else
          {
            dataContext.TitleBlockList.Add(new ComboBoxItemString(newTitleBlockValue));
            dataContext.TitleBlockString = newTitleBlockValue;
          }
        }
      }
      else if (num != -1)
      {
        HardwareDetailWindowViewModel detailWindowViewModel = dataContext;
        ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
        observableCollection.Add(new ComboBoxItemString("<No Scale Required>"));
        detailWindowViewModel.ScaleList = observableCollection;
        dataContext.ScaleString = "<No Scale Required>";
      }
      else if (num == -1)
      {
        dataContext.ScaleList.Clear();
        foreach (ComboBoxItemString comboBoxItemString in list)
          dataContext.ScaleList.Add(comboBoxItemString);
        dataContext.ScaleList.Add(new ComboBoxItemString("<<Varies>>"));
        dataContext.ScaleString = "<<Varies>>";
      }
      if (flag1)
      {
        dataContext.TitleBlockList.Clear();
        foreach (string titleBlockName in this.titleBlockNames)
          dataContext.TitleBlockList.Add(new ComboBoxItemString(titleBlockName));
        if (dataContext.TitleBlockList.Select<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (x => x.ValueString)).Contains<string>(templateTitleBlock))
        {
          dataContext.TitleBlockString = templateTitleBlock;
        }
        else
        {
          dataContext.TitleBlockList.Add(new ComboBoxItemString($"<Missing {templateTitleBlock}>"));
          dataContext.TitleBlockString = $"<Missing {templateTitleBlock}>";
        }
      }
    }
    else if (dictionary2.Count + dictionary3.Count > 1)
    {
      dataContext.ScaleList.Clear();
      List<ComboBoxItemString> comboBoxItemStringList = (List<ComboBoxItemString>) null;
      if (source1.All<HWDetailTemplate>((Func<HWDetailTemplate, bool>) (t => t.ScaleUnits == ScaleUnits.US)))
        comboBoxItemStringList = ScalesManager.GetScalesList(ScaleUnits.US).Select<string, ComboBoxItemString>((Func<string, ComboBoxItemString>) (s => new ComboBoxItemString(s))).ToList<ComboBoxItemString>();
      if (source1.All<HWDetailTemplate>((Func<HWDetailTemplate, bool>) (t => t.ScaleUnits == ScaleUnits.Metric)))
        comboBoxItemStringList = ScalesManager.GetScalesList(ScaleUnits.Metric).Select<string, ComboBoxItemString>((Func<string, ComboBoxItemString>) (s => new ComboBoxItemString(s))).ToList<ComboBoxItemString>();
      if (comboBoxItemStringList != null && comboBoxItemStringList.Count > 0)
      {
        foreach (ComboBoxItemString comboBoxItemString in comboBoxItemStringList)
          dataContext.ScaleList.Add(comboBoxItemString);
      }
      if (dataContext.ScaleList.Select<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (x => x.ValueString)).Contains<string>("<<Varies>>"))
      {
        dataContext.ScaleString = "<<Varies>>";
      }
      else
      {
        dataContext.ScaleList.Add(new ComboBoxItemString("<<Varies>>"));
        dataContext.ScaleString = "<<Varies>>";
      }
      bool flag3 = false;
      string ticketTitleBlockSampleName = source1.First<HWDetailTemplate>().TitleBlockName;
      if (source1.All<HWDetailTemplate>((Func<HWDetailTemplate, bool>) (t => t.TitleBlockName.Equals(ticketTitleBlockSampleName))))
        flag3 = true;
      if (flag3)
      {
        dataContext.TitleBlockList.Clear();
        foreach (string titleBlockName in this.titleBlockNames)
          dataContext.TitleBlockList.Add(new ComboBoxItemString(titleBlockName));
        if (dataContext.TitleBlockList.Select<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (x => x.ValueString)).Contains<string>(ticketTitleBlockSampleName))
        {
          dataContext.TitleBlockString = ticketTitleBlockSampleName;
        }
        else
        {
          dataContext.TitleBlockList.Add(new ComboBoxItemString($"<Missing {ticketTitleBlockSampleName}>"));
          dataContext.TitleBlockString = $"<Missing {ticketTitleBlockSampleName}>";
        }
      }
      else
      {
        bool flag4 = true;
        foreach (HWDetailTemplate hwDetailTemplate in source1)
        {
          if (!dataContext.TitleBlockList.Select<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (x => x.ValueString)).Contains<string>(hwDetailTemplate.TitleBlockName))
          {
            flag4 = false;
            break;
          }
        }
        string newTitleBlockValue = string.Empty;
        newTitleBlockValue = !flag4 ? "<template title block families are missing from the project>" : "<<Varies>>";
        if (dataContext.TitleBlockList.Where<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (x => x.ValueString == newTitleBlockValue)).Any<ComboBoxItemString>())
        {
          dataContext.TitleBlockString = newTitleBlockValue;
        }
        else
        {
          dataContext.TitleBlockList.Add(new ComboBoxItemString(newTitleBlockValue));
          dataContext.TitleBlockString = newTitleBlockValue;
        }
      }
    }
    else
    {
      int num1 = (int) System.Windows.Forms.MessageBox.Show("Unable to find template in template settings! ");
    }
    this.scaleComboBox.Items.Refresh();
    if (dataContext.ScaleList.Count > 1)
      this.scaleComboBox.IsEnabled = true;
    else
      this.scaleComboBox.IsEnabled = false;
    this.titleBlockComboBox.Items.Refresh();
    if (dataContext.ScaleList.Count > 1)
      this.titleBlockComboBox.IsEnabled = true;
    else
      this.titleBlockComboBox.IsEnabled = false;
  }

  private List<string> getFilteredTemplateList(HardwareDetailWindowViewModel dataContext)
  {
    List<string> filteredTemplateList = new List<string>();
    if (dataContext.ManufacturerString == HardwareDetailWindowViewModel.manufacturerDefaultString)
      filteredTemplateList.AddRange((IEnumerable<string>) this.templateSettings.Templates.Select<HWDetailTemplate, string>((Func<HWDetailTemplate, string>) (x => x.TemplateName)).NatrualSort<string>((Func<string, string>) (x => x)).ToList<string>());
    else
      filteredTemplateList.AddRange((IEnumerable<string>) this.templateSettings.Templates.Where<HWDetailTemplate>((Func<HWDetailTemplate, bool>) (x => x.TemplateManufacturerName == dataContext.ManufacturerString)).Select<HWDetailTemplate, string>((Func<HWDetailTemplate, string>) (x => x.TemplateName)).NatrualSort<string>((Func<string, string>) (x => x)).ToList<string>());
    return filteredTemplateList;
  }

  public List<HWDetailTemplate> getTemplates(out string errorMessage)
  {
    errorMessage = (string) null;
    List<HWDetailTemplate> templates = new List<HWDetailTemplate>();
    if (this.selectedTemplates.Count == 0)
    {
      errorMessage = "No templates selected.";
      return templates;
    }
    foreach (string selectedTemplate in this.selectedTemplates)
    {
      string temp = selectedTemplate;
      int index = this.templateSettings.Templates.FindIndex((Predicate<HWDetailTemplate>) (x => x.TemplateName == temp));
      if (index < 0)
        errorMessage = "One or more templates selected does not exist.";
      else
        templates.Add(this.templateSettings.Templates[index]);
    }
    return templates;
  }

  public int getSelectedScale()
  {
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    try
    {
      return ScalesManager.GetRatioForScale(dataContext.ScaleString, this.ScaleUnits);
    }
    catch (Exception ex)
    {
      return -1;
    }
  }

  public bool getTitleBlock(out string titleBlock)
  {
    HardwareDetailWindowViewModel dataContext = this.DataContext as HardwareDetailWindowViewModel;
    titleBlock = string.Empty;
    if (this.titleBlockNames.Contains(dataContext.TitleBlockString))
    {
      if (this.selectedTemplates.Count<string>() == 1)
      {
        HWDetailTemplate hwDetailTemplate = this.templateSettings.Templates.Where<HWDetailTemplate>((Func<HWDetailTemplate, bool>) (x => x.TemplateName == this.selectedTemplates.FirstOrDefault<string>())).FirstOrDefault<HWDetailTemplate>();
        if (hwDetailTemplate == null || hwDetailTemplate.TitleBlockName == dataContext.TitleBlockString)
          return false;
        titleBlock = dataContext.TitleBlockString;
        return true;
      }
      titleBlock = dataContext.TitleBlockString;
      return true;
    }
    titleBlock = dataContext.TitleBlockString;
    return true;
  }

  private void clearButton_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HardwareDetailWindowViewModel).CopyString = HardwareDetailWindowViewModel.copyDefaultString;
  }

  public string getCopyString() => (this.DataContext as HardwareDetailWindowViewModel).CopyString;

  private void Window_Closing(object sender, CancelEventArgs e)
  {
    if (this.populate)
      return;
    this.canceled = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/hardwaredetail/views/hardwaredetailwindow.xaml", UriKind.Relative));
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
        this.templateManufactureLabel = (System.Windows.Controls.Label) target;
        break;
      case 3:
        this.manufactureListComboBox = (System.Windows.Controls.ComboBox) target;
        this.manufactureListComboBox.SelectionChanged += new SelectionChangedEventHandler(this.manufactureListComboBox_SelectionChanged);
        break;
      case 4:
        this.templateLabel = (System.Windows.Controls.Label) target;
        break;
      case 5:
        this.templateListComboBox = (System.Windows.Controls.ComboBox) target;
        this.templateListComboBox.SelectionChanged += new SelectionChangedEventHandler(this.templateListComboBox_SelectionChanged);
        break;
      case 6:
        this.selectMultipleButton = (System.Windows.Controls.Button) target;
        this.selectMultipleButton.Click += new RoutedEventHandler(this.selectMultipleButton_Click);
        break;
      case 7:
        this.scaleLabel = (System.Windows.Controls.Label) target;
        break;
      case 8:
        this.scaleComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 9:
        this.titleBlockLabel = (System.Windows.Controls.Label) target;
        break;
      case 10:
        this.titleBlockComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 11:
        this.copyLabel = (System.Windows.Controls.Label) target;
        break;
      case 12:
        this.copySourceComboBox = (System.Windows.Controls.ComboBox) target;
        break;
      case 13:
        this.clearButton = (System.Windows.Controls.Button) target;
        this.clearButton.Click += new RoutedEventHandler(this.clearButton_Click);
        break;
      case 14:
        this.populateButton = (System.Windows.Controls.Button) target;
        this.populateButton.Click += new RoutedEventHandler(this.populateButton_Click);
        break;
      case 15:
        this.cancelButton = (System.Windows.Controls.Button) target;
        this.cancelButton.Click += new RoutedEventHandler(this.cancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
