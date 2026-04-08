// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoTicketForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketPopulator;
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
namespace EDGE.TicketTools.AutoDimensioning;

public class AutoTicketForm : System.Windows.Forms.Form
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
  public string defaultTemplateName = "";
  private Document revitDoc;
  private string alreadySelectedTitleBlockName = "<none>";
  private Tuple<double, double> titleBlockDims;
  private const double _SheetSizeReduction = 0.75;
  public TicketTemplate SelectedTemplate;
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
  private Label label3;
  private Label label4;
  private ComboBox cmbTitleBlockNames;
  private Label label5;
  private TextBox txtMultiSheetNumber;
  private Label lblAssemblyName;
  private OpenFileDialog openFileDialog1;
  private Label lblConstructionProduct;
  private Label manufacturerLabel;
  private ComboBox cmbTemplateManufacturerName;
  private CustomDrawComboBox cmbScale;

  public AutoTicketForm(Document doc)
  {
    this.InitializeComponent();
    this.bMultiSheet = false;
    this.mTemplateSettings = new TicketTemplateSettings();
    this.revitDoc = doc;
  }

  private void AutoTicketForm_Load(object sender, EventArgs e)
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
    this.lblAssemblyName.Text = this.AssemblyName;
    this.lblConstructionProduct.Text = this.ConstructionProduct;
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
    if (this.cmbTemplateName.Items.Contains((object) this.defaultTemplateName))
    {
      this.cmbTemplateName.SelectedIndex = this.cmbTemplateName.Items.IndexOf((object) this.defaultTemplateName);
    }
    else
    {
      this.cmbTemplateName.SelectedIndex = -1;
      this.cmbTemplateName.Text = "<select template>";
    }
    this.txtMultiSheetNumber.Text = this.strMultiSheetNumberOverride;
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
    if (this.cmbTitleBlockNames.SelectedIndex < 0)
    {
      TaskDialog.Show("Form Error", "Selected Titleblock is not in the list of available titleblocks.  Please check selection");
      return false;
    }
    if (this.cmbTemplateName.SelectedIndex < 0)
    {
      TaskDialog.Show("Form Error", "No template selected.  Please check selection");
      return false;
    }
    if (!this.TitleBlockNames.Contains(this.cmbTitleBlockNames.SelectedItem.ToString()))
    {
      TaskDialog.Show("Form Error", "Selected Titleblock is not in the list of available titleblocks.  Please check selection");
      return false;
    }
    if (this.SelectedTemplate.TitleBlockName != this.cmbTitleBlockNames.SelectedItem.ToString() && App.DialogSwitches.ShowTicketPopulatorTitleBlockMismatchMessage)
    {
      TaskDialog taskDialog = new TaskDialog("Title Block Mismatch");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AllowCancellation = false;
      taskDialog.MainInstruction = "Title block mismatch";
      taskDialog.MainContent = $"Selected Titleblock: {this.cmbTitleBlockNames.SelectedItem.ToString()} does not match titleblock in Template.";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Use selected titleblock", this.cmbTitleBlockNames.SelectedItem.ToString());
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Use template titleblock", this.SelectedTemplate.TitleBlockName);
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Return to Ticket Populator Dialog");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1001)
      {
        this.SelectedTitleBlock = this.cmbTitleBlockNames.SelectedItem.ToString();
      }
      else
      {
        if (taskDialogResult != 1002)
          return false;
        this.SelectedTitleBlock = this.SelectedTemplate.TitleBlockName;
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
    if (this.cmbScale.SelectedIndex == -1)
    {
      if (!int.TryParse(this.cmbScale.Text, out int _) && !this.cmbScale.Text.Equals("<No Scale Required>"))
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
      else if (!this.cmbScale.Text.Equals("<No Scale Required>"))
      {
        int num = (int) MessageBox.Show($"unable to parse custom scale: {this.cmbScale.Text} Using 1:1");
      }
    }
    else
      this.SelectedScaleFactor = ScalesManager.GetRatioForScale(this.cmbScale.SelectedItem.ToString(), this.ScaleUnits);
    this.strMultiSheetNumberOverride = this.txtMultiSheetNumber.Text;
    this.DialogResult = DialogResult.OK;
    this.Close();
  }

  private void cmbTemplateName_SelectedIndexChanged(object sender, EventArgs e)
  {
    string templateName = this.cmbTemplateName.SelectedItem.ToString();
    if (templateName != null)
      this.FillTemplateData(templateName);
    this.UpdateCmbScaleWarningState();
  }

  private void templateManufacturerName_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.cmbTemplateName.Text = "<selectTemplate>";
    this.cmbTemplateName.Items.Clear();
    this.cmbTemplateName.Items.AddRange((object[]) this.mTemplateSettings.Templates.Where<TicketTemplate>((Func<TicketTemplate, bool>) (tt => tt.TemplateManufacturerName != null && tt.TemplateManufacturerName.Equals(this.cmbTemplateManufacturerName.SelectedItem) || this.cmbTemplateManufacturerName.SelectedItem.ToString().Equals("<no filter>"))).Select<TicketTemplate, string>((Func<TicketTemplate, string>) (tt => tt.TemplateName)).OrderBy<string, string>((Func<string, string>) (str => str)).ToArray<string>());
  }

  private void FillTemplateData(string templateName)
  {
    IEnumerable<TicketTemplate> source = this.mTemplateSettings.Templates.Where<TicketTemplate>((Func<TicketTemplate, bool>) (s => s.TemplateName == templateName));
    if (source.Any<TicketTemplate>())
    {
      TicketTemplate template = source.First<TicketTemplate>();
      this.SelectedTemplate = template;
      this.ScaleUnits = template.ScaleUnits;
      object[] array = (object[]) ScalesManager.GetScalesList(template.ScaleUnits).Select<string, CustomBackgroundComboBoxItem>((Func<string, CustomBackgroundComboBoxItem>) (s => new CustomBackgroundComboBoxItem(s, ScalesManager.GetRatioForScale(s, template.ScaleUnits), SystemColors.Control))).ToArray<CustomBackgroundComboBoxItem>();
      if (template.TemplateScale > 0)
      {
        try
        {
          this.cmbScale.Items.Clear();
          this.cmbScale.Items.AddRange(array);
          this.UpdateCmbScaleWarningState();
          string stringRep;
          if (ScalesManager.GetScaleStringForScaleFactor(template.TemplateScale, template.ScaleUnits, out stringRep))
          {
            int num1 = -1;
            int num2 = 0;
            backgroundComboBoxItem = (CustomBackgroundComboBoxItem) null;
            foreach (object obj in this.cmbScale.Items)
            {
              if (obj is CustomBackgroundComboBoxItem backgroundComboBoxItem && stringRep == backgroundComboBoxItem.Text)
              {
                num1 = num2;
                break;
              }
              ++num2;
            }
            this.cmbScale.SelectedIndex = num1;
            if (backgroundComboBoxItem != null)
              this.cmbScale.BackColor = backgroundComboBoxItem.BackColor;
          }
          else
            this.cmbScale.Text = stringRep;
        }
        catch
        {
          this.cmbScale.Text = $"<Scale Error: {template.TemplateScale.ToString()}>";
        }
      }
      else
      {
        this.cmbScale.Items.Clear();
        this.UpdateCmbScaleWarningState();
        this.cmbScale.Text = "<No Scale Required>";
      }
      string titleBlockName = template.TitleBlockName;
      if (this.cmbTitleBlockNames.Items.Contains((object) titleBlockName))
      {
        this.cmbTitleBlockNames.SelectedIndex = this.cmbTitleBlockNames.Items.IndexOf((object) titleBlockName);
        this.alreadySelectedTitleBlockName = titleBlockName;
      }
      else
      {
        this.cmbTitleBlockNames.SelectedIndex = -1;
        this.cmbTitleBlockNames.Text = $"<Missing {titleBlockName}>";
      }
    }
    else
    {
      int num = (int) MessageBox.Show("Unable to find template in template settings! " + templateName);
    }
  }

  private void cmbScale_SelectedIndexChanged(object sender, EventArgs e)
  {
    if (!(sender is ComboBox comboBox) || !(comboBox.SelectedItem is CustomBackgroundComboBoxItem selectedItem))
      return;
    comboBox.BackColor = selectedItem.BackColor;
  }

  private AutoTicketForm.SizeWarning CheckSelectedScaleAgainstBBox(int scale)
  {
    if (this.assemblyBoundingBox == null)
      return AutoTicketForm.SizeWarning.OK;
    Tuple<double, double> titleblockWidthHeight = this.GetSelectedTitleblockWidthHeight();
    if (scale <= 0 || titleblockWidthHeight == null)
      return AutoTicketForm.SizeWarning.Error;
    double num1 = this.assemblyBoundingBox.Max.DistanceTo(this.assemblyBoundingBox.Min);
    double num2 = titleblockWidthHeight.Item1 * 0.75;
    double num3 = titleblockWidthHeight.Item2 * 0.75;
    double num4 = Convert.ToDouble(scale);
    double num5 = num1 / num4;
    double num6 = 0.0;
    double num7 = 0.0;
    bool flag = false;
    if (this.SelectedTemplate != null && this.SelectedTemplate.AnchorGroups != null && this.SelectedTemplate.AnchorGroups.Any<AnchorGroup>())
    {
      AnchorGroup anchorGroup = this.SelectedTemplate.AnchorGroups.First<AnchorGroup>();
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
    return (flag ? num6 : num7) > num3 | (flag ? num7 : num6) > num2 ? AutoTicketForm.SizeWarning.Error : AutoTicketForm.SizeWarning.OK;
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
          case AutoTicketForm.SizeWarning.OK:
            backgroundComboBoxItem.BackColor = SystemColors.Window;
            continue;
          case AutoTicketForm.SizeWarning.Warning:
            backgroundComboBoxItem.BackColor = System.Drawing.Color.Yellow;
            continue;
          case AutoTicketForm.SizeWarning.Error:
            backgroundComboBoxItem.BackColor = System.Drawing.Color.Red;
            continue;
          default:
            continue;
        }
      }
      else
        QA.LogError("AutoTicketForm: UpdateCmbScaleWarningState", "Unable to cast comboBox item to customBackGroundComboBoxItem");
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
    this.label3 = new Label();
    this.label4 = new Label();
    this.cmbTitleBlockNames = new ComboBox();
    this.label5 = new Label();
    this.txtMultiSheetNumber = new TextBox();
    this.lblAssemblyName = new Label();
    this.openFileDialog1 = new OpenFileDialog();
    this.lblConstructionProduct = new Label();
    this.cmbScale = new CustomDrawComboBox();
    this.SuspendLayout();
    this.cmbTemplateManufacturerName.DropDownWidth = 900;
    this.cmbTemplateManufacturerName.FormattingEnabled = true;
    this.cmbTemplateManufacturerName.Location = new System.Drawing.Point(21, 97);
    this.cmbTemplateManufacturerName.Margin = new Padding(4, 4, 4, 4);
    this.cmbTemplateManufacturerName.Name = "cmbTemplateManufacturerName";
    this.cmbTemplateManufacturerName.Size = new Size(460, 24);
    this.cmbTemplateManufacturerName.TabIndex = 11;
    this.cmbTemplateManufacturerName.SelectedIndexChanged += new EventHandler(this.templateManufacturerName_SelectedIndexChanged);
    this.cmbTemplateManufacturerName.KeyPress += new KeyPressEventHandler(this.cmbTemplateManufacturerName_KeyPress);
    this.manufacturerLabel.AutoSize = true;
    this.manufacturerLabel.Location = new System.Drawing.Point(15, 78);
    this.manufacturerLabel.Margin = new Padding(4, 0, 4, 0);
    this.manufacturerLabel.Name = "manufacturerLabel";
    this.manufacturerLabel.Size = new Size(155, 17);
    this.manufacturerLabel.TabIndex = 10;
    this.manufacturerLabel.Text = "Template Manufacturer";
    this.cmbTemplateName.DropDownWidth = 900;
    this.cmbTemplateName.FormattingEnabled = true;
    this.cmbTemplateName.Location = new System.Drawing.Point(21, 146);
    this.cmbTemplateName.Margin = new Padding(4, 4, 4, 4);
    this.cmbTemplateName.Name = "cmbTemplateName";
    this.cmbTemplateName.Size = new Size(460, 24);
    this.cmbTemplateName.TabIndex = 0;
    this.cmbTemplateName.SelectedIndexChanged += new EventHandler(this.cmbTemplateName_SelectedIndexChanged);
    this.cmbTemplateName.KeyPress += new KeyPressEventHandler(this.cmbTemplateName_KeyPress);
    this.label1.AutoSize = true;
    this.label1.Location = new System.Drawing.Point(15, (int) sbyte.MaxValue);
    this.label1.Margin = new Padding(4, 0, 4, 0);
    this.label1.Name = "label1";
    this.label1.Size = new Size(67, 17);
    this.label1.TabIndex = 1;
    this.label1.Text = "Template";
    this.label2.AutoSize = true;
    this.label2.Location = new System.Drawing.Point(19, 193);
    this.label2.Margin = new Padding(4, 0, 4, 0);
    this.label2.Name = "label2";
    this.label2.Size = new Size(43, 17);
    this.label2.TabIndex = 1;
    this.label2.Text = "Scale";
    this.btnPopulate.Location = new System.Drawing.Point(271, 300);
    this.btnPopulate.Margin = new Padding(4, 4, 4, 4);
    this.btnPopulate.Name = "btnPopulate";
    this.btnPopulate.Size = new Size(100, 28);
    this.btnPopulate.TabIndex = 3;
    this.btnPopulate.Text = "Populate";
    this.btnPopulate.UseVisualStyleBackColor = true;
    this.btnPopulate.Click += new EventHandler(this.btnPopulate_Click);
    this.button2.DialogResult = DialogResult.Cancel;
    this.button2.Location = new System.Drawing.Point(380, 300);
    this.button2.Margin = new Padding(4, 4, 4, 4);
    this.button2.Name = "button2";
    this.button2.Size = new Size(100, 28);
    this.button2.TabIndex = 3;
    this.button2.Text = "Cancel";
    this.button2.UseVisualStyleBackColor = true;
    this.label3.AutoSize = true;
    this.label3.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.label3.Location = new System.Drawing.Point(17, 16 /*0x10*/);
    this.label3.Margin = new Padding(4, 0, 4, 0);
    this.label3.Name = "label3";
    this.label3.Size = new Size(58, 29);
    this.label3.TabIndex = 4;
    this.label3.Text = "Info:";
    this.label4.AutoSize = true;
    this.label4.Location = new System.Drawing.Point(19, 226);
    this.label4.Margin = new Padding(4, 0, 4, 0);
    this.label4.Name = "label4";
    this.label4.Size = new Size(68, 17);
    this.label4.TabIndex = 6;
    this.label4.Text = "Titleblock";
    this.cmbTitleBlockNames.DropDownWidth = 600;
    this.cmbTitleBlockNames.FormattingEnabled = true;
    this.cmbTitleBlockNames.Location = new System.Drawing.Point(93, 226);
    this.cmbTitleBlockNames.Margin = new Padding(4, 4, 4, 4);
    this.cmbTitleBlockNames.Name = "cmbTitleBlockNames";
    this.cmbTitleBlockNames.Size = new Size(388, 24);
    this.cmbTitleBlockNames.TabIndex = 5;
    this.cmbTitleBlockNames.SelectedIndexChanged += new EventHandler(this.cmbTitleBlockNames_SelectedIndexChanged);
    this.cmbTitleBlockNames.KeyPress += new KeyPressEventHandler(this.cmbTitleBlockNames_KeyPress);
    this.label5.AutoSize = true;
    this.label5.Location = new System.Drawing.Point(19, 260);
    this.label5.Margin = new Padding(4, 0, 4, 0);
    this.label5.Name = "label5";
    this.label5.Size = new Size(53, 17);
    this.label5.TabIndex = 1;
    this.label5.Text = "Sheet#";
    this.label5.Click += new EventHandler(this.label5_Click);
    this.txtMultiSheetNumber.Enabled = false;
    this.txtMultiSheetNumber.Location = new System.Drawing.Point(93, 260);
    this.txtMultiSheetNumber.Margin = new Padding(4, 4, 4, 4);
    this.txtMultiSheetNumber.Name = "txtMultiSheetNumber";
    this.txtMultiSheetNumber.Size = new Size(109, 22);
    this.txtMultiSheetNumber.TabIndex = 7;
    this.lblAssemblyName.AutoSize = true;
    this.lblAssemblyName.Font = new Font("Microsoft Sans Serif", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.lblAssemblyName.Location = new System.Drawing.Point(88, 16 /*0x10*/);
    this.lblAssemblyName.Margin = new Padding(4, 0, 4, 0);
    this.lblAssemblyName.Name = "lblAssemblyName";
    this.lblAssemblyName.Size = new Size(209, 29);
    this.lblAssemblyName.TabIndex = 8;
    this.lblAssemblyName.Text = "<assembly name>";
    this.openFileDialog1.FileName = "openFileDialog1";
    this.openFileDialog1.Title = "Select Ticket Template Settings File";
    this.lblConstructionProduct.AutoSize = true;
    this.lblConstructionProduct.Location = new System.Drawing.Point(23, 46);
    this.lblConstructionProduct.Name = "lblConstructionProduct";
    this.lblConstructionProduct.Size = new Size(46, 17);
    this.lblConstructionProduct.TabIndex = 9;
    this.lblConstructionProduct.Text = "label6";
    this.cmbScale.DrawMode = DrawMode.OwnerDrawFixed;
    this.cmbScale.DropDownWidth = 300;
    this.cmbScale.FormattingEnabled = true;
    this.cmbScale.Location = new System.Drawing.Point(93, 193);
    this.cmbScale.Margin = new Padding(4, 4, 4, 4);
    this.cmbScale.Name = "cmbScale";
    this.cmbScale.Size = new Size(388, 23);
    this.cmbScale.TabIndex = 0;
    this.cmbScale.SelectedIndexChanged += new EventHandler(this.cmbScale_SelectedIndexChanged);
    this.cmbScale.KeyPress += new KeyPressEventHandler(this.cmbScale_KeyPress);
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(497, 334);
    this.Controls.Add((System.Windows.Forms.Control) this.lblConstructionProduct);
    this.Controls.Add((System.Windows.Forms.Control) this.lblAssemblyName);
    this.Controls.Add((System.Windows.Forms.Control) this.txtMultiSheetNumber);
    this.Controls.Add((System.Windows.Forms.Control) this.label4);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbTitleBlockNames);
    this.Controls.Add((System.Windows.Forms.Control) this.label3);
    this.Controls.Add((System.Windows.Forms.Control) this.button2);
    this.Controls.Add((System.Windows.Forms.Control) this.btnPopulate);
    this.Controls.Add((System.Windows.Forms.Control) this.label5);
    this.Controls.Add((System.Windows.Forms.Control) this.label2);
    this.Controls.Add((System.Windows.Forms.Control) this.label1);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbScale);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbTemplateName);
    this.Controls.Add((System.Windows.Forms.Control) this.cmbTemplateManufacturerName);
    this.Controls.Add((System.Windows.Forms.Control) this.manufacturerLabel);
    this.Margin = new Padding(4, 4, 4, 4);
    this.Name = nameof (AutoTicketForm);
    this.StartPosition = FormStartPosition.CenterParent;
    this.Text = "EDGE^R Ticket Populator";
    this.Load += new EventHandler(this.AutoTicketForm_Load);
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
