// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.BatchPopulatorForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketPopulator.FormExtras;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class BatchPopulatorForm : System.Windows.Forms.Form
{
  public List<string> TitleBlockNames;
  public List<string> ViewTemplates;
  public string SelectedTitleBlock;
  public string SelectedScale;
  public int SelectedScaleFactor;
  public bool bMultiSheet;
  public string strMultiSheetNumberOverride;
  private TicketTemplateSettings mTemplateSettings;
  public string TicketTemplateSettingsPath;
  public bool bStartWithOpenDialog = true;
  public ScaleUnits ScaleUnits;
  public string ConstructionProduct = "";
  public string AssemblyName = "";
  public BoundingBoxXYZ assemblyBoundingBox;
  public List<string> defaultTemplateNames = new List<string>();
  private Document revitDoc;
  private string alreadySelectedTitleBlockName = "<none>";
  private Tuple<double, double> titleBlockDims;
  public SourceAssembly CopySource;
  public bool ViewSpreadSheetEnhancement = true;
  private const double _SheetSizeReduction = 0.75;
  private List<TicketTemplate> availableTemplates = new List<TicketTemplate>();
  public List<TicketTemplate> SelectedTemplates = new List<TicketTemplate>();
  internal Transform assemblyTransform;
  private XYZ assemblyDiagonalVector = XYZ.Zero;
  private bool bAssemblyIsVeritcalElement;
  private double basisX_LengthOfAssembly;
  private double basisY_LengthOfAssembly;
  private IContainer components;
  private ComboBox cmbTemplateName;
  private Label label1;
  private Label label2;
  private Button btnPopulate;
  private Button button2;
  private Label label4;
  private ComboBox cmbTitleBlockNames;
  private OpenFileDialog openFileDialog1;
  private Label manufacturerLabel;
  private ComboBox cmbTemplateManufacturerName;
  private CustomDrawComboBox cmbScale;
  private Button MultiTemplateBtn;
  private ComboBox CopySource_DropDown;
  private Label CopySource_LBL;
  private Button CopySource_CLEAR;
  private CheckBox checkBox1;

  public BatchPopulatorForm(Document doc, bool spreadsheet, bool bAutoTicket = false)
  {
    this.InitializeComponent();
    this.bMultiSheet = false;
    this.mTemplateSettings = new TicketTemplateSettings();
    this.revitDoc = doc;
    if (bAutoTicket)
    {
      this.Text = "Automatic Ticket Generator";
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.CopySource_DropDown.Show();
      this.CopySource_LBL.Show();
      this.CopySource_CLEAR.Show();
      this.ViewSpreadSheetEnhancement = spreadsheet;
      this.SpreadsheetCheckbox(spreadsheet);
      this.InitAutoTicketElements();
    }
    else
    {
      this.checkBox1.Visible = false;
      this.CopySource_DropDown.Hide();
      this.CopySource_LBL.Hide();
      this.CopySource_CLEAR.Hide();
    }
  }

  private void InitAutoTicketElements()
  {
    List<Autodesk.Revit.DB.View> list = new FilteredElementCollector(this.revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Where<Element>((Func<Element, bool>) (view => (view as Autodesk.Revit.DB.View).AssociatedAssemblyInstanceId != ElementId.InvalidElementId)).Cast<Autodesk.Revit.DB.View>().ToList<Autodesk.Revit.DB.View>();
    list.Sort((Comparison<Autodesk.Revit.DB.View>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(this.revitDoc.GetElement(p.AssociatedAssemblyInstanceId).Name, this.revitDoc.GetElement(q.AssociatedAssemblyInstanceId).Name)));
    foreach (Autodesk.Revit.DB.View view1 in list)
    {
      Autodesk.Revit.DB.View view = view1;
      AssemblyInstance assemInstance = this.revitDoc.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance;
      if (Parameters.GetParameterAsInt((Element) assemInstance, "HARDWARE_DETAIL") != 1)
      {
        SourceAssembly sourceAssembly = this.CopySource_DropDown.Items.Cast<SourceAssembly>().FirstOrDefault<SourceAssembly>((Func<SourceAssembly, bool>) (sA => sA.AssemID.IntegerValue == assemInstance.Id.IntegerValue));
        if (sourceAssembly == null)
        {
          sourceAssembly = new SourceAssembly(assemInstance);
          this.CopySource_DropDown.Items.Add((object) sourceAssembly);
        }
        if (view is ViewSheet)
        {
          if (!sourceAssembly.Sheets.Any<ViewSheet>((Func<ViewSheet, bool>) (v => v.Id.IntegerValue == view.Id.IntegerValue)))
            sourceAssembly.Sheets.Add(view as ViewSheet);
        }
        else if (!sourceAssembly.Views.Any<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (v => v.Id.IntegerValue == view.Id.IntegerValue)))
          sourceAssembly.Views.Add(view);
      }
    }
    this.CopySource_DropDown.DisplayMember = "Name";
  }

  private void PopulatorForm_Load(object sender, EventArgs e)
  {
    while (!this.LoadSettings())
    {
      string empty1 = string.Empty;
      string str1 = !string.IsNullOrWhiteSpace(this.TicketTemplateSettingsPath) ? $"Application Ticket Template Settings have NOT been found in the file:{Environment.NewLine}{Environment.NewLine}{this.TicketTemplateSettingsPath}" : "Application Ticket Template Settings File has NOT been defined.";
      string empty2 = string.Empty;
      string str2 = $"Ticket Template Settings Filename is stored in the file:{Environment.NewLine}{Environment.NewLine}     C:\\EDGEforREVIT\\EDGE_Preferences.txt";
      TaskDialog taskDialog = new TaskDialog("Application Settings not found.");
      taskDialog.MainInstruction = str1;
      taskDialog.FooterText = str2;
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Choose an alternate template settings file.", "Browse to a file on disk which contains Ticket Template Settings");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      if (taskDialog.Show() == 2)
      {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
        return;
      }
      if (this.openFileDialog1.ShowDialog() == DialogResult.Cancel)
      {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
        return;
      }
      this.TicketTemplateSettingsPath = this.openFileDialog1.FileName;
    }
    this.cmbTitleBlockNames.Items.AddRange((object[]) this.TitleBlockNames.ToArray());
    if (this.cmbTitleBlockNames.Items.Count > 0)
      this.cmbTitleBlockNames.SelectedIndex = 0;
    this.cmbScale.Text = "<selectTemplate>";
    this.cmbTemplateManufacturerName.Items.AddRange((object[]) this.mTemplateSettings.Templates.Where<TicketTemplate>((Func<TicketTemplate, bool>) (s => s.TemplateManufacturerName != null && s.TemplateManufacturerName != "<TemplateManufacturer>")).Select<TicketTemplate, string>((Func<TicketTemplate, string>) (s => s.TemplateManufacturerName)).Distinct<string>().OrderBy<string, string>((Func<string, string>) (str => str)).ToArray<string>());
    this.cmbTemplateManufacturerName.Items.Add((object) "<no filter>");
    Parameter parameter = this.revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
    {
      string str = parameter.AsString();
      if (this.cmbTemplateManufacturerName.Items.Contains((object) str))
        this.cmbTemplateManufacturerName.SelectedIndex = this.cmbTemplateManufacturerName.Items.IndexOf((object) str);
      else
        this.cmbTemplateManufacturerName.SelectedIndex = this.cmbTemplateManufacturerName.Items.IndexOf((object) "<no filter>");
    }
    this.SelectedTemplates = new List<TicketTemplate>();
    this.cmbTemplateName.SelectedIndex = -1;
    this.cmbTemplateName.Text = "<select template>";
    if (!this.SelectedScale.Equals("", StringComparison.OrdinalIgnoreCase))
    {
      int num = this.cmbScale.Items.IndexOf((object) this.SelectedScale);
      if (num > -1)
        this.cmbScale.SelectedIndex = num;
    }
    if (this.assemblyTransform.BasisZ.Normalize().CrossProduct(new XYZ(0.0, 0.0, 1.0)).IsUnitLength())
      this.bAssemblyIsVeritcalElement = true;
    this.assemblyDiagonalVector = new XYZ(Math.Abs(this.assemblyBoundingBox.Max.X - this.assemblyBoundingBox.Min.X), Math.Abs(this.assemblyBoundingBox.Max.Y - this.assemblyBoundingBox.Min.Y), Math.Abs(this.assemblyBoundingBox.Max.Z - this.assemblyBoundingBox.Min.Z));
    this.UpdateCmbScaleWarningState();
  }

  private bool LoadSettings()
  {
    return this.mTemplateSettings.LoadTicketTemplateSettings(this.TicketTemplateSettingsPath);
  }

  private bool ValidateData()
  {
    if (this.SelectedTemplates.Count == 0)
    {
      TaskDialog.Show("Form Error", "No template selected.  Please check selection");
      return false;
    }
    if (this.cmbTitleBlockNames.SelectedIndex < 0)
    {
      bool flag = false;
      if (this.cmbTitleBlockNames.Text == "<template titleblock families are missing from the project>")
      {
        string str = "";
        foreach (TicketTemplate selectedTemplate in this.SelectedTemplates)
        {
          if (!this.cmbTitleBlockNames.Items.Contains((object) selectedTemplate.TitleBlockName))
            str = $"{str}{selectedTemplate.TemplateName} - {selectedTemplate.TitleBlockName}\n";
        }
        new TaskDialog("Form Error")
        {
          MainInstruction = "Selected Titleblocks are not in the list of available titleblocks.  Please check selection",
          ExpandedContent = str
        }.Show();
      }
      else if (this.cmbTitleBlockNames.Text == "<<Varies>>")
      {
        foreach (TicketTemplate selectedTemplate in this.SelectedTemplates)
        {
          if (!this.cmbTitleBlockNames.Items.Contains((object) selectedTemplate.TitleBlockName))
          {
            flag = true;
            break;
          }
        }
        if (!flag)
          return true;
      }
      else
        flag = true;
      if (flag)
        TaskDialog.Show("Form Error", "Selected Titleblock is not in the list of available titleblocks.  Please check selection");
      return false;
    }
    if (!this.TitleBlockNames.Contains(this.cmbTitleBlockNames.SelectedItem.ToString()))
    {
      TaskDialog.Show("Form Error", "Selected Titleblock is not in the list of available titleblocks.  Please check selection");
      return false;
    }
    foreach (TicketTemplate selectedTemplate in this.SelectedTemplates)
    {
      if (selectedTemplate.TitleBlockName != this.cmbTitleBlockNames.SelectedItem.ToString() && App.DialogSwitches.ShowTicketPopulatorTitleBlockMismatchMessage)
      {
        TaskDialog taskDialog = new TaskDialog("Title Block Mismatch");
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AllowCancellation = false;
        taskDialog.MainInstruction = "Title block mismatch";
        taskDialog.MainContent = $"Selected Titleblock: {this.cmbTitleBlockNames.SelectedItem.ToString()} does not match titleblock in Template.";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Use selected titleblock", this.cmbTitleBlockNames.SelectedItem.ToString());
        if (this.SelectedTemplates.Count == 1)
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Use template titleblock", selectedTemplate.TitleBlockName);
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Return to Ticket Populator Dialog");
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          this.SelectedTitleBlock = this.cmbTitleBlockNames.SelectedItem.ToString();
          break;
        }
        if (taskDialogResult != 1002)
          return false;
        this.SelectedTitleBlock = selectedTemplate.TitleBlockName;
        break;
      }
    }
    if (!this.TitleBlockNames.Contains(this.SelectedTitleBlock))
    {
      if (!new TaskDialog("Title Block Not Available")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "The selected Titleblock is not available. Do you want to continue populating the ticket without title block?",
        CommonButtons = ((TaskDialogCommonButtons) 9)
      }.Show().Equals((object) (TaskDialogResult) 1))
        return false;
    }
    int result;
    if (this.cmbScale.SelectedIndex == -1)
    {
      if (this.cmbScale.Text.Contains("Varies"))
      {
        if (!int.TryParse(this.cmbScale.Text, out result) && !this.cmbScale.Text.Equals("<<Varies>>"))
        {
          TaskDialog.Show("Form Error", $"Manually entered Scale Value: {this.cmbScale.Text} is not an integer.");
          return false;
        }
      }
      else if (!int.TryParse(this.cmbScale.Text, out result) && !this.cmbScale.Text.Equals("<No Scale Required>"))
      {
        TaskDialog.Show("Form Error", $"Manually entered Scale Value: {this.cmbScale.Text} is not an integer.");
        return false;
      }
    }
    else if (!ScalesManager.GetScalesList(this.ScaleUnits).Contains(this.cmbScale.SelectedItem.ToString()))
    {
      TaskDialog.Show("Form Error", "Selected scale is not in the list of available scales. Please check selection");
      return false;
    }
    if (!this.bMultiSheet || !(this.strMultiSheetNumberOverride == ""))
      return true;
    TaskDialog.Show("Form Error", "Assembly to be ticketed already has an existing sheet and sheet number override is empty.  Please provide a value for sheet number.");
    return false;
  }

  private void btnPopulate_Click(object sender, EventArgs e)
  {
    if (this.cmbTitleBlockNames.SelectedIndex > -1)
      this.SelectedTitleBlock = this.cmbTitleBlockNames.SelectedItem.ToString();
    if (!this.ValidateData())
      return;
    if (this.cmbScale.SelectedIndex == -1)
    {
      int result;
      if (int.TryParse(this.cmbScale.Text, out result))
        this.SelectedScaleFactor = result;
      else if (this.cmbScale.Text.Contains("Varies"))
      {
        if (!this.cmbScale.Text.Equals("<<Varies>>"))
        {
          int num1 = (int) MessageBox.Show($"unable to parse custom scale: {this.cmbScale.Text} Using 1:1");
        }
        else
          this.SelectedScaleFactor = -1;
      }
      else if (!this.cmbScale.Text.Equals("<No Scale Required>"))
      {
        int num2 = (int) MessageBox.Show($"unable to parse custom scale: {this.cmbScale.Text} Using 1:1");
      }
    }
    else
      this.SelectedScaleFactor = ScalesManager.GetRatioForScale(this.cmbScale.SelectedItem.ToString(), this.ScaleUnits);
    this.DialogResult = DialogResult.OK;
    this.Close();
  }

  private void cmbTemplateName_SelectedIndexChanged(object sender, EventArgs e)
  {
    string str = this.cmbTemplateName.SelectedItem.ToString();
    if (str != null)
    {
      TicketTemplate SelectedTemplate;
      this.FillTemplateData(new List<string>() { str }, out SelectedTemplate);
      this.SelectedTemplates = new List<TicketTemplate>()
      {
        SelectedTemplate
      };
    }
    this.UpdateCmbScaleWarningState();
  }

  private void templateManufacturerName_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.cmbTemplateName.Text = "<selectTemplate>";
    this.cmbTemplateName.Items.Clear();
    this.SelectedTemplates.Clear();
    this.availableTemplates = this.mTemplateSettings.Templates.Where<TicketTemplate>((Func<TicketTemplate, bool>) (tt => tt.TemplateManufacturerName != null && tt.TemplateManufacturerName.Equals(this.cmbTemplateManufacturerName.SelectedItem) || this.cmbTemplateManufacturerName.SelectedItem.ToString().Equals("<no filter>"))).ToList<TicketTemplate>();
    this.cmbTemplateName.Items.AddRange((object[]) this.availableTemplates.Select<TicketTemplate, string>((Func<TicketTemplate, string>) (tt => tt.TemplateName)).OrderBy<string, string>((Func<string, string>) (str => str)).ToArray<string>());
  }

  private void FillTemplateData(List<string> templateNames, out TicketTemplate SelectedTemplate)
  {
    SelectedTemplate = (TicketTemplate) null;
    Dictionary<int, List<TicketTemplate>> dictionary1 = new Dictionary<int, List<TicketTemplate>>();
    List<TicketTemplate> source1 = new List<TicketTemplate>();
    foreach (TicketTemplate template1 in this.mTemplateSettings.Templates)
    {
      TicketTemplate template = template1;
      if (templateNames.Any<string>((Func<string, bool>) (e => e.Equals(template.TemplateName))))
        source1.Add(template);
    }
    Dictionary<int, List<TicketTemplate>> dictionary2 = new Dictionary<int, List<TicketTemplate>>();
    Dictionary<int, List<TicketTemplate>> dictionary3 = new Dictionary<int, List<TicketTemplate>>();
    foreach (TicketTemplate ticketTemplate in source1)
    {
      bool flag = true;
      foreach (AnchorGroup anchorGroup in ticketTemplate.AnchorGroups)
      {
        if (anchorGroup.MainView != null && anchorGroup.MainView.originalScale != ticketTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.TopView != null && anchorGroup.TopView.originalScale != ticketTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.BottomView != null && anchorGroup.BottomView.originalScale != ticketTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.LeftView != null && anchorGroup.LeftView.originalScale != ticketTemplate.TemplateScale)
          flag = false;
        if (anchorGroup.RightView != null && anchorGroup.RightView.originalScale != ticketTemplate.TemplateScale)
          flag = false;
      }
      if (flag)
      {
        if (dictionary2.ContainsKey(ticketTemplate.TemplateScale))
          dictionary2[ticketTemplate.TemplateScale].Add(ticketTemplate);
        else
          dictionary2.Add(ticketTemplate.TemplateScale, new List<TicketTemplate>()
          {
            ticketTemplate
          });
      }
      else if (dictionary3.ContainsKey(ticketTemplate.TemplateScale))
        dictionary3[ticketTemplate.TemplateScale].Add(ticketTemplate);
      else
        dictionary3.Add(ticketTemplate.TemplateScale, new List<TicketTemplate>()
        {
          ticketTemplate
        });
    }
    if (dictionary2.Count + dictionary3.Count == 1)
    {
      List<TicketTemplate> source2 = new List<TicketTemplate>();
      foreach (KeyValuePair<int, List<TicketTemplate>> keyValuePair in dictionary2)
      {
        foreach (TicketTemplate ticketTemplate in keyValuePair.Value)
          source2.Add(ticketTemplate);
      }
      foreach (KeyValuePair<int, List<TicketTemplate>> keyValuePair in dictionary3)
      {
        foreach (TicketTemplate ticketTemplate in keyValuePair.Value)
          source2.Add(ticketTemplate);
      }
      TicketTemplate template = source2.First<TicketTemplate>();
      SelectedTemplate = template;
      int num1 = template.TemplateScale;
      if (dictionary3.Count > 0)
        num1 = -1;
      this.ScaleUnits = template.ScaleUnits;
      object[] array = (object[]) ScalesManager.GetScalesList(template.ScaleUnits).Select<string, CustomBackgroundComboBoxItem>((Func<string, CustomBackgroundComboBoxItem>) (s => new CustomBackgroundComboBoxItem(s, ScalesManager.GetRatioForScale(s, template.ScaleUnits), SystemColors.Control))).ToArray<CustomBackgroundComboBoxItem>();
      bool flag1 = true;
      string templateTitleBlock = template.TitleBlockName;
      if (num1 > 0)
      {
        this.cmbScale.Items.Clear();
        this.cmbScale.Items.AddRange(array);
        this.UpdateCmbScaleWarningState();
        string stringRep;
        if (ScalesManager.GetScaleStringForScaleFactor(template.TemplateScale, template.ScaleUnits, out stringRep))
        {
          int num2 = -1;
          int num3 = 0;
          backgroundComboBoxItem = (CustomBackgroundComboBoxItem) null;
          foreach (object obj in this.cmbScale.Items)
          {
            if (obj is CustomBackgroundComboBoxItem backgroundComboBoxItem && stringRep == backgroundComboBoxItem.Text)
            {
              num2 = num3;
              break;
            }
            ++num3;
          }
          this.cmbScale.SelectedIndex = num2;
          if (backgroundComboBoxItem != null)
            this.cmbScale.BackColor = backgroundComboBoxItem.BackColor;
        }
        else
          this.cmbScale.Text = stringRep;
        if (source2.Count > 1 && !source2.All<TicketTemplate>((Func<TicketTemplate, bool>) (t => t.TitleBlockName == templateTitleBlock)))
        {
          this.cmbTitleBlockNames.SelectedIndex = -1;
          flag1 = false;
          bool flag2 = true;
          foreach (TicketTemplate ticketTemplate in source2)
          {
            if (!this.cmbTitleBlockNames.Items.Contains((object) ticketTemplate.TitleBlockName))
            {
              flag2 = false;
              break;
            }
          }
          if (flag2)
            this.cmbTitleBlockNames.Text = "<<Varies>>";
          else
            this.cmbTitleBlockNames.Text = "<template titleblock families are missing from the project>";
        }
      }
      else if (num1 != -1)
      {
        this.cmbScale.Items.Clear();
        this.UpdateCmbScaleWarningState();
        this.cmbScale.Text = "<No Scale Required>";
      }
      else if (num1 == -1)
      {
        this.cmbScale.Items.Clear();
        this.cmbScale.Items.AddRange(array);
        this.UpdateCmbScaleWarningState();
        this.cmbScale.Text = "<<Varies>>";
      }
      if (!flag1)
        return;
      if (this.cmbTitleBlockNames.Items.Contains((object) templateTitleBlock))
      {
        this.cmbTitleBlockNames.SelectedIndex = this.cmbTitleBlockNames.Items.IndexOf((object) templateTitleBlock);
        this.alreadySelectedTitleBlockName = templateTitleBlock;
      }
      else
      {
        this.cmbTitleBlockNames.SelectedIndex = -1;
        this.cmbTitleBlockNames.Text = $"<Missing {templateTitleBlock}>";
      }
    }
    else if (dictionary2.Count + dictionary3.Count > 1)
    {
      this.cmbScale.Items.Clear();
      object[] items = (object[]) null;
      if (source1.All<TicketTemplate>((Func<TicketTemplate, bool>) (t => t.ScaleUnits == ScaleUnits.US)))
        items = (object[]) ScalesManager.GetScalesList(ScaleUnits.US).Select<string, CustomBackgroundComboBoxItem>((Func<string, CustomBackgroundComboBoxItem>) (s => new CustomBackgroundComboBoxItem(s, ScalesManager.GetRatioForScale(s, ScaleUnits.US), SystemColors.Control))).ToArray<CustomBackgroundComboBoxItem>();
      if (source1.All<TicketTemplate>((Func<TicketTemplate, bool>) (t => t.ScaleUnits == ScaleUnits.Metric)))
        items = (object[]) ScalesManager.GetScalesList(ScaleUnits.Metric).Select<string, CustomBackgroundComboBoxItem>((Func<string, CustomBackgroundComboBoxItem>) (s => new CustomBackgroundComboBoxItem(s, ScalesManager.GetRatioForScale(s, ScaleUnits.Metric), SystemColors.Control))).ToArray<CustomBackgroundComboBoxItem>();
      if (items != null && items.Length != 0)
        this.cmbScale.Items.AddRange(items);
      this.UpdateCmbScaleWarningState();
      this.cmbScale.Text = "<<Varies>>";
      this.cmbScale.BackColor = System.Drawing.Color.White;
      bool flag3 = false;
      string ticketTitleBlockSampleName = source1.First<TicketTemplate>().TitleBlockName;
      if (source1.All<TicketTemplate>((Func<TicketTemplate, bool>) (t => t.TitleBlockName.Equals(ticketTitleBlockSampleName))))
        flag3 = true;
      if (flag3)
      {
        if (this.cmbTitleBlockNames.Items.Contains((object) ticketTitleBlockSampleName))
        {
          this.cmbTitleBlockNames.SelectedIndex = this.cmbTitleBlockNames.Items.IndexOf((object) ticketTitleBlockSampleName);
          this.alreadySelectedTitleBlockName = ticketTitleBlockSampleName;
        }
        else
        {
          this.cmbTitleBlockNames.SelectedIndex = -1;
          this.cmbTitleBlockNames.Text = $"<Missing {ticketTitleBlockSampleName}>";
        }
      }
      else
      {
        this.cmbTitleBlockNames.SelectedIndex = -1;
        bool flag4 = true;
        foreach (TicketTemplate ticketTemplate in source1)
        {
          if (!this.cmbTitleBlockNames.Items.Contains((object) ticketTemplate.TitleBlockName))
          {
            flag4 = false;
            break;
          }
        }
        if (flag4)
          this.cmbTitleBlockNames.Text = "<<Varies>>";
        else
          this.cmbTitleBlockNames.Text = "<template titleblock families are missing from the project>";
      }
    }
    else
    {
      int num = (int) MessageBox.Show("Unable to find template in template settings! ");
    }
  }

  private void cmbScale_SelectedIndexChanged(object sender, EventArgs e)
  {
    if (!(sender is ComboBox comboBox) || !(comboBox.SelectedItem is CustomBackgroundComboBoxItem selectedItem))
      return;
    comboBox.BackColor = selectedItem.BackColor;
  }

  private BatchPopulatorForm.SizeWarning CheckSelectedScaleAgainstBBox(int scale)
  {
    if (this.SelectedTemplates.Count > 1 || this.assemblyBoundingBox == null)
      return BatchPopulatorForm.SizeWarning.OK;
    Tuple<double, double> titleblockWidthHeight = this.GetSelectedTitleblockWidthHeight();
    if (scale <= 0 || titleblockWidthHeight == null)
      return BatchPopulatorForm.SizeWarning.Error;
    double num1 = this.assemblyBoundingBox.Max.DistanceTo(this.assemblyBoundingBox.Min);
    double num2 = titleblockWidthHeight.Item1 * 0.75;
    double num3 = titleblockWidthHeight.Item2 * 0.75;
    double num4 = Convert.ToDouble(scale);
    double num5 = num1 / num4;
    double num6 = 0.0;
    double num7 = 0.0;
    TicketTemplate ticketTemplate = this.SelectedTemplates.FirstOrDefault<TicketTemplate>();
    bool flag = false;
    if (ticketTemplate != null && ticketTemplate.AnchorGroups != null && ticketTemplate.AnchorGroups.Any<AnchorGroup>())
    {
      AnchorGroup anchorGroup = ticketTemplate.AnchorGroups.First<AnchorGroup>();
      ViewportRotation rotationOnSheet = anchorGroup.MainView.RotationOnSheet;
      double viewCropRotation = anchorGroup.MainView.SectionViewCropRotation;
      Transform transformForView = this.GetOrientedTransformForView(anchorGroup.MainView.mSectionViewOrientation, this.assemblyTransform);
      this.basisX_LengthOfAssembly = Math.Abs(this.assemblyDiagonalVector.DotProduct(transformForView.BasisX.Normalize()));
      this.basisY_LengthOfAssembly = Math.Abs(this.assemblyDiagonalVector.DotProduct(transformForView.BasisY.Normalize()));
      num6 = this.basisX_LengthOfAssembly / Convert.ToDouble(scale);
      num7 = this.basisY_LengthOfAssembly / Convert.ToDouble(scale);
      switch (rotationOnSheet)
      {
        case ViewportRotation.Clockwise:
          viewCropRotation += Math.PI / 2.0;
          break;
        case ViewportRotation.Counterclockwise:
          viewCropRotation -= Math.PI / 2.0;
          break;
      }
      if (Math.Abs(viewCropRotation) > Math.PI / 10.0)
        flag = true;
    }
    return (flag ? num6 : num7) > num3 | (flag ? num7 : num6) > num2 ? BatchPopulatorForm.SizeWarning.Error : BatchPopulatorForm.SizeWarning.OK;
  }

  private Tuple<double, double> GetSelectedTitleblockWidthHeight()
  {
    string strTitleBlockName = this.cmbTitleBlockNames.SelectedItem != null ? this.cmbTitleBlockNames.SelectedItem.ToString() : "";
    if (strTitleBlockName == this.alreadySelectedTitleBlockName)
      return this.titleBlockDims;
    IEnumerable<Element> revitTitleblock = this.GetRevitTitleblock(strTitleBlockName);
    if (revitTitleblock == null)
    {
      this.titleBlockDims = new Tuple<double, double>(17.0 / 12.0, 11.0 / 12.0);
      return this.titleBlockDims;
    }
    if (revitTitleblock.Any<Element>())
    {
      this.titleBlockDims = new Tuple<double, double>(Parameters.GetParameterAsDouble(revitTitleblock.First<Element>(), "Sheet Width"), Parameters.GetParameterAsDouble(revitTitleblock.First<Element>(), "Sheet Height"));
      return this.titleBlockDims;
    }
    this.titleBlockDims = new Tuple<double, double>(17.0 / 12.0, 11.0 / 12.0);
    return this.titleBlockDims;
  }

  private IEnumerable<Element> GetRevitTitleblock(string strTitleBlockName)
  {
    return this.revitDoc == null ? (IEnumerable<Element>) null : new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (s => s.Name.ToUpper() == strTitleBlockName.ToUpper()));
  }

  private void label5_Click(object sender, EventArgs e)
  {
  }

  private void cmbTitleBlockNames_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.UpdateCmbScaleWarningState();
    this.alreadySelectedTitleBlockName = (sender as ComboBox).SelectedText;
  }

  private void UpdateCmbScaleWarningState()
  {
    foreach (object obj in this.cmbScale.Items)
    {
      if (obj is CustomBackgroundComboBoxItem backgroundComboBoxItem)
      {
        switch (this.CheckSelectedScaleAgainstBBox(backgroundComboBoxItem.Scale))
        {
          case BatchPopulatorForm.SizeWarning.OK:
            backgroundComboBoxItem.BackColor = SystemColors.Window;
            continue;
          case BatchPopulatorForm.SizeWarning.Warning:
            backgroundComboBoxItem.BackColor = System.Drawing.Color.Yellow;
            continue;
          case BatchPopulatorForm.SizeWarning.Error:
            backgroundComboBoxItem.BackColor = System.Drawing.Color.Red;
            continue;
          default:
            continue;
        }
      }
      else
        QA.LogError("PopulatorForm: UpdateCmbScaleWarningState", "Unable to cast comboBox item to customBackGroundComboBoxItem");
    }
    if (!(this.cmbScale.SelectedItem is CustomBackgroundComboBoxItem selectedItem))
      return;
    this.cmbScale.BackColor = selectedItem.BackColor;
  }

  private void button1_Click(object sender, EventArgs e)
  {
    Process.Start("http://www.edge.ptac.com/#!ticket-populator/rkd95");
  }

  private void cmbTemplateManufacturerName_KeyPress(object sender, KeyPressEventArgs e)
  {
    e.Handled = true;
  }

  private void cmbTemplateName_KeyPress(object sender, KeyPressEventArgs e) => e.Handled = true;

  private void cmbScale_KeyPress(object sender, KeyPressEventArgs e) => e.Handled = true;

  private void cmbTitleBlockNames_KeyPress(object sender, KeyPressEventArgs e) => e.Handled = true;

  private Transform GetOrientedTransformForView(
    TemplateViewOrientation associatedViewOrientation,
    Transform AssemblyTransform)
  {
    Transform identity = Transform.Identity;
    AssemblyTransform.BasisY.Negate();
    XYZ basisY1 = AssemblyTransform.BasisY;
    XYZ basisX1 = AssemblyTransform.BasisX;
    AssemblyTransform.BasisX.Negate();
    XYZ basisX2 = AssemblyTransform.BasisX;
    XYZ basisX3 = AssemblyTransform.BasisX;
    AssemblyTransform.BasisX.Negate();
    XYZ basisX4 = AssemblyTransform.BasisX;
    AssemblyTransform.BasisY.Negate();
    XYZ basisY2 = AssemblyTransform.BasisY;
    XYZ basisZ = AssemblyTransform.BasisZ;
    AssemblyTransform.BasisZ.Negate();
    if (AssemblyTransform.BasisY.IsAlmostEqualTo(new XYZ(0.0, 0.0, 1.0)))
    {
      switch (associatedViewOrientation)
      {
        case TemplateViewOrientation.ElevationTop:
        case TemplateViewOrientation.ElevationBottom:
          identity.BasisX = AssemblyTransform.BasisX;
          identity.BasisZ = AssemblyTransform.BasisZ;
          break;
        case TemplateViewOrientation.ElevationFront:
        case TemplateViewOrientation.ElevationBack:
          identity.BasisX = AssemblyTransform.BasisX;
          identity.BasisZ = AssemblyTransform.BasisY;
          break;
        case TemplateViewOrientation.ElevationLeft:
        case TemplateViewOrientation.ElevationRight:
          identity.BasisX = AssemblyTransform.BasisZ;
          identity.BasisZ = AssemblyTransform.BasisX;
          break;
      }
    }
    else if (AssemblyTransform.BasisZ.IsAlmostEqualTo(new XYZ(0.0, 0.0, 1.0)))
    {
      switch (associatedViewOrientation)
      {
        case TemplateViewOrientation.ElevationTop:
        case TemplateViewOrientation.ElevationBottom:
          identity.BasisX = AssemblyTransform.BasisX;
          identity.BasisZ = AssemblyTransform.BasisZ;
          break;
        case TemplateViewOrientation.ElevationFront:
        case TemplateViewOrientation.ElevationBack:
          identity.BasisX = AssemblyTransform.BasisX;
          identity.BasisZ = AssemblyTransform.BasisY;
          break;
        case TemplateViewOrientation.ElevationLeft:
        case TemplateViewOrientation.ElevationRight:
          identity.BasisX = AssemblyTransform.BasisY;
          identity.BasisZ = AssemblyTransform.BasisX;
          break;
      }
    }
    identity.BasisY = identity.BasisZ.CrossProduct(identity.BasisX).Normalize();
    return identity;
  }

  private void MultiTemplateBtn_Click(object sender, EventArgs e)
  {
    TemplateForm templateForm = new TemplateForm(this.revitDoc, this.availableTemplates, this.SelectedTemplates);
    if (!templateForm.ShowDialog().Equals((object) DialogResult.OK) || templateForm.SelectedTemplateList.Count <= 0)
      return;
    this.SelectedTemplates = templateForm.SelectedTemplateList.ToList<TicketTemplate>();
    if (this.SelectedTemplates.Count == 1)
    {
      string templateName = this.SelectedTemplates.First<TicketTemplate>().TemplateName;
      if (this.cmbTemplateName.Items.Contains((object) templateName))
      {
        this.cmbTemplateName.SelectedIndex = this.cmbTemplateName.Items.IndexOf((object) templateName);
        this.cmbTemplateName.Text = this.cmbTemplateName.Items[this.cmbTemplateName.SelectedIndex].ToString();
      }
      else
      {
        this.cmbTemplateName.SelectedIndex = -1;
        this.cmbTemplateName.Text = "<select template>";
      }
    }
    else
      this.cmbTemplateName.Text = "<<Multiple templates selected>>";
    this.FillTemplateData(this.SelectedTemplates.Select<TicketTemplate, string>((Func<TicketTemplate, string>) (t => t.TemplateName)).ToList<string>(), out TicketTemplate _);
  }

  private void CopySource_DropDown_SelectedIndexChanged(object sender, EventArgs e)
  {
    if (this.CopySource_DropDown.SelectedItem == null)
      return;
    this.CopySource = this.CopySource_DropDown.SelectedItem as SourceAssembly;
  }

  private void CopySource_CLEAR_Click(object sender, EventArgs e)
  {
    this.CopySource_DropDown.SelectedItem = (object) null;
    this.CopySource_DropDown.Text = "<<Select Source>>";
  }

  private void CheckBox1_CheckedChanged(object sender, EventArgs e)
  {
    if (this.checkBox1.Checked)
      this.ViewSpreadSheetEnhancement = true;
    else
      this.ViewSpreadSheetEnhancement = false;
  }

  private void SpreadsheetCheckbox(bool isChecked)
  {
    if (isChecked)
      this.checkBox1.Checked = true;
    else
      this.checkBox1.Checked = false;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.cmbTemplateManufacturerName = new ComboBox();
    this.manufacturerLabel = new Label();
    this.cmbTemplateName = new ComboBox();
    this.label1 = new Label();
    this.label2 = new Label();
    this.btnPopulate = new Button();
    this.button2 = new Button();
    this.label4 = new Label();
    this.cmbTitleBlockNames = new ComboBox();
    this.openFileDialog1 = new OpenFileDialog();
    this.MultiTemplateBtn = new Button();
    this.CopySource_DropDown = new ComboBox();
    this.CopySource_LBL = new Label();
    this.CopySource_CLEAR = new Button();
    this.cmbScale = new CustomDrawComboBox();
    this.checkBox1 = new CheckBox();
    this.SuspendLayout();
    this.cmbTemplateManufacturerName.DropDownWidth = 900;
    this.cmbTemplateManufacturerName.FormattingEnabled = true;
    this.cmbTemplateManufacturerName.Location = new System.Drawing.Point(13, 27);
    this.cmbTemplateManufacturerName.Margin = new Padding(4);
    this.cmbTemplateManufacturerName.Name = "cmbTemplateManufacturerName";
    this.cmbTemplateManufacturerName.Size = new Size(460, 24);
    this.cmbTemplateManufacturerName.TabIndex = 11;
    this.cmbTemplateManufacturerName.SelectedIndexChanged += new EventHandler(this.templateManufacturerName_SelectedIndexChanged);
    this.cmbTemplateManufacturerName.KeyPress += new KeyPressEventHandler(this.cmbTemplateManufacturerName_KeyPress);
    this.manufacturerLabel.AutoSize = true;
    this.manufacturerLabel.Location = new System.Drawing.Point(7, 8);
    this.manufacturerLabel.Margin = new Padding(4, 0, 4, 0);
    this.manufacturerLabel.Name = "manufacturerLabel";
    this.manufacturerLabel.Size = new Size(145, 16 /*0x10*/);
    this.manufacturerLabel.TabIndex = 10;
    this.manufacturerLabel.Text = "Template Manufacturer";
    this.cmbTemplateName.DropDownWidth = 459;
    this.cmbTemplateName.FormattingEnabled = true;
    this.cmbTemplateName.Location = new System.Drawing.Point(13, 76);
    this.cmbTemplateName.Margin = new Padding(4);
    this.cmbTemplateName.Name = "cmbTemplateName";
    this.cmbTemplateName.Size = new Size(301, 24);
    this.cmbTemplateName.TabIndex = 0;
    this.cmbTemplateName.SelectedIndexChanged += new EventHandler(this.cmbTemplateName_SelectedIndexChanged);
    this.cmbTemplateName.KeyPress += new KeyPressEventHandler(this.cmbTemplateName_KeyPress);
    this.label1.AutoSize = true;
    this.label1.Location = new System.Drawing.Point(7, 57);
    this.label1.Margin = new Padding(4, 0, 4, 0);
    this.label1.Name = "label1";
    this.label1.Size = new Size(65, 16 /*0x10*/);
    this.label1.TabIndex = 1;
    this.label1.Text = "Template";
    this.label2.Anchor = AnchorStyles.Bottom;
    this.label2.AutoSize = true;
    this.label2.Location = new System.Drawing.Point(11, 108);
    this.label2.Margin = new Padding(4, 0, 4, 0);
    this.label2.Name = "label2";
    this.label2.Size = new Size(42, 16 /*0x10*/);
    this.label2.TabIndex = 1;
    this.label2.Text = "Scale";
    this.btnPopulate.Anchor = AnchorStyles.Bottom;
    this.btnPopulate.Location = new System.Drawing.Point(279, 225);
    this.btnPopulate.Margin = new Padding(10, 4, 0, 4);
    this.btnPopulate.Name = "btnPopulate";
    this.btnPopulate.Size = new Size(100, 28);
    this.btnPopulate.TabIndex = 3;
    this.btnPopulate.Text = "Populate";
    this.btnPopulate.UseVisualStyleBackColor = true;
    this.btnPopulate.Click += new EventHandler(this.btnPopulate_Click);
    this.button2.Anchor = AnchorStyles.Bottom;
    this.button2.DialogResult = DialogResult.Cancel;
    this.button2.Location = new System.Drawing.Point(383, 225);
    this.button2.Margin = new Padding(4);
    this.button2.Name = "button2";
    this.button2.Size = new Size(100, 28);
    this.button2.TabIndex = 3;
    this.button2.Text = "Cancel";
    this.button2.UseVisualStyleBackColor = true;
    this.label4.Anchor = AnchorStyles.Bottom;
    this.label4.AutoSize = true;
    this.label4.Location = new System.Drawing.Point(11, 141);
    this.label4.Margin = new Padding(4, 0, 4, 0);
    this.label4.Name = "label4";
    this.label4.Size = new Size(66, 16 /*0x10*/);
    this.label4.TabIndex = 6;
    this.label4.Text = "Titleblock";
    this.cmbTitleBlockNames.Anchor = AnchorStyles.Bottom;
    this.cmbTitleBlockNames.DropDownWidth = 388;
    this.cmbTitleBlockNames.FormattingEnabled = true;
    this.cmbTitleBlockNames.Location = new System.Drawing.Point(85, 141);
    this.cmbTitleBlockNames.Margin = new Padding(4);
    this.cmbTitleBlockNames.Name = "cmbTitleBlockNames";
    this.cmbTitleBlockNames.Size = new Size(388, 24);
    this.cmbTitleBlockNames.TabIndex = 5;
    this.cmbTitleBlockNames.SelectedIndexChanged += new EventHandler(this.cmbTitleBlockNames_SelectedIndexChanged);
    this.cmbTitleBlockNames.KeyPress += new KeyPressEventHandler(this.cmbTitleBlockNames_KeyPress);
    this.openFileDialog1.FileName = "openFileDialog1";
    this.openFileDialog1.Title = "Select Ticket Template Settings File";
    this.MultiTemplateBtn.Location = new System.Drawing.Point(322, 73);
    this.MultiTemplateBtn.Margin = new Padding(4);
    this.MultiTemplateBtn.Name = "MultiTemplateBtn";
    this.MultiTemplateBtn.Size = new Size(150, 28);
    this.MultiTemplateBtn.TabIndex = 13;
    this.MultiTemplateBtn.Text = "Select Multiple";
    this.MultiTemplateBtn.UseVisualStyleBackColor = true;
    this.MultiTemplateBtn.Click += new EventHandler(this.MultiTemplateBtn_Click);
    this.CopySource_DropDown.Anchor = AnchorStyles.Bottom;
    this.CopySource_DropDown.DropDownWidth = 459;
    this.CopySource_DropDown.FormattingEnabled = true;
    this.CopySource_DropDown.Location = new System.Drawing.Point(16 /*0x10*/, 192 /*0xC0*/);
    this.CopySource_DropDown.Margin = new Padding(4);
    this.CopySource_DropDown.Name = "CopySource_DropDown";
    this.CopySource_DropDown.Size = new Size(389, 24);
    this.CopySource_DropDown.TabIndex = 14;
    this.CopySource_DropDown.Text = "<<Select Source>>";
    this.CopySource_DropDown.SelectedIndexChanged += new EventHandler(this.CopySource_DropDown_SelectedIndexChanged);
    this.CopySource_LBL.Anchor = AnchorStyles.Bottom;
    this.CopySource_LBL.AutoSize = true;
    this.CopySource_LBL.Location = new System.Drawing.Point(10, 169);
    this.CopySource_LBL.Margin = new Padding(4, 0, 4, 0);
    this.CopySource_LBL.Name = "CopySource_LBL";
    this.CopySource_LBL.Size = new Size(136, 16 /*0x10*/);
    this.CopySource_LBL.TabIndex = 15;
    this.CopySource_LBL.Text = "Copy Sheet Elements";
    this.CopySource_CLEAR.Anchor = AnchorStyles.Bottom;
    this.CopySource_CLEAR.Location = new System.Drawing.Point(413, 189);
    this.CopySource_CLEAR.Margin = new Padding(4);
    this.CopySource_CLEAR.Name = "CopySource_CLEAR";
    this.CopySource_CLEAR.Size = new Size(59, 28);
    this.CopySource_CLEAR.TabIndex = 16 /*0x10*/;
    this.CopySource_CLEAR.Text = "Clear";
    this.CopySource_CLEAR.UseVisualStyleBackColor = true;
    this.CopySource_CLEAR.Click += new EventHandler(this.CopySource_CLEAR_Click);
    this.cmbScale.Anchor = AnchorStyles.Bottom;
    this.cmbScale.DrawMode = DrawMode.OwnerDrawFixed;
    this.cmbScale.DropDownWidth = 388;
    this.cmbScale.FormattingEnabled = true;
    this.cmbScale.Location = new System.Drawing.Point(85, 108);
    this.cmbScale.Margin = new Padding(4);
    this.cmbScale.Name = "cmbScale";
    this.cmbScale.Size = new Size(388, 23);
    this.cmbScale.TabIndex = 0;
    this.cmbScale.SelectedIndexChanged += new EventHandler(this.cmbScale_SelectedIndexChanged);
    this.cmbScale.KeyPress += new KeyPressEventHandler(this.cmbScale_KeyPress);
    this.checkBox1.AutoSize = true;
    this.checkBox1.Location = new System.Drawing.Point(16 /*0x10*/, 233);
    this.checkBox1.Name = "checkBox1";
    this.checkBox1.Size = new Size(249, 20);
    this.checkBox1.TabIndex = 17;
    this.checkBox1.Text = "Dimension and Callout Customization";
    this.checkBox1.UseVisualStyleBackColor = true;
    this.checkBox1.CheckedChanged += new EventHandler(this.CheckBox1_CheckedChanged);
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(497, 266);
    this.Controls.Add((System.Windows.Forms.Control) this.checkBox1);
    this.Controls.Add((System.Windows.Forms.Control) this.CopySource_CLEAR);
    this.Controls.Add((System.Windows.Forms.Control) this.CopySource_LBL);
    this.Controls.Add((System.Windows.Forms.Control) this.CopySource_DropDown);
    this.Controls.Add((System.Windows.Forms.Control) this.MultiTemplateBtn);
    this.Controls.Add((System.Windows.Forms.Control) this.label4);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbTitleBlockNames);
    this.Controls.Add((System.Windows.Forms.Control) this.button2);
    this.Controls.Add((System.Windows.Forms.Control) this.btnPopulate);
    this.Controls.Add((System.Windows.Forms.Control) this.label2);
    this.Controls.Add((System.Windows.Forms.Control) this.label1);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbScale);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbTemplateName);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbTemplateManufacturerName);
    this.Controls.Add((System.Windows.Forms.Control) this.manufacturerLabel);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.Margin = new Padding(4);
    this.MaximizeBox = false;
    this.Name = nameof (BatchPopulatorForm);
    this.StartPosition = FormStartPosition.CenterParent;
    this.Text = "Batch Ticket Populator";
    this.Load += new EventHandler(this.PopulatorForm_Load);
    this.ResumeLayout(false);
    this.PerformLayout();
  }

  private enum SizeWarning
  {
    OK,
    Warning,
    Error,
  }
}
