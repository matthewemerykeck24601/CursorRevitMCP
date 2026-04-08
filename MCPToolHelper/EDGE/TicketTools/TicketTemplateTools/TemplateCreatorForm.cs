// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketTemplateTools.TemplateCreatorForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

#nullable disable
namespace EDGE.TicketTools.TicketTemplateTools;

public class TemplateCreatorForm : Form
{
  public TicketTemplateSettings TemplateSettings;
  public string TemplateName = "";
  public string TemplateManufacturerName = "";
  public BOMJustification BOMJustification;
  public bool bStackSchedules = true;
  private string strEmptyTemplateName = "<TemplateName>";
  private string strEmptyManufacturerName = "<TemplateManufacturer>";
  public string ConstructionProduct = "";
  private IContainer components;
  private SaveFileDialog saveFileDialog1;
  private TextBox txtSaveFilePath;
  private Label label1;
  private Button btnSettingsSaveLocation;
  private ComboBox cmbTicketTempalteName;
  private Label label2;
  private GroupBox groupBox1;
  private RadioButton rbBottomAlign;
  private RadioButton rbTopAlign;
  private Button btnCreate;
  private Button btnCancel;
  private Button btnHelp;
  private Label lblConstructionProduct;
  private GroupBox groupBox2;
  private RadioButton rbIndependentBOM;
  private RadioButton rbStackBOM;
  private Label manufacturerLabel;
  private ComboBox templateManufacturerName;

  public TemplateCreatorForm() => this.InitializeComponent();

  private void TemplateCreatorForm_Load(object sender, EventArgs e)
  {
    this.btnCreate.Enabled = false;
    if (App.TicketTemplateSettingsPath != "")
    {
      this.txtSaveFilePath.Text = App.TicketTemplateSettingsPath;
      this.ReadTempalteSettings();
    }
    if (!(this.ConstructionProduct != ""))
      return;
    this.lblConstructionProduct.Text = "Construction Product: " + this.ConstructionProduct;
  }

  private void rbTopAlign_CheckedChanged(object sender, EventArgs e)
  {
    this.rbBottomAlign.Checked = !this.rbTopAlign.Checked;
    this.BOMJustification = this.rbTopAlign.Checked ? BOMJustification.Top : BOMJustification.Bottom;
  }

  private void rbBottomAlign_CheckedChanged(object sender, EventArgs e)
  {
    this.rbTopAlign.Checked = !this.rbBottomAlign.Checked;
    this.BOMJustification = this.rbTopAlign.Checked ? BOMJustification.Top : BOMJustification.Bottom;
  }

  private void btnSettingsSaveLocation_Click(object sender, EventArgs e)
  {
    this.saveFileDialog1.AddExtension = true;
    this.saveFileDialog1.CheckPathExists = true;
    this.saveFileDialog1.DefaultExt = ".xml";
    if (File.Exists(this.txtSaveFilePath.Text))
      this.saveFileDialog1.FileName = this.txtSaveFilePath.Text;
    this.saveFileDialog1.Filter = "Settings Files (*.xml, *.XML)|*.xml;*.XML";
    this.saveFileDialog1.Title = "Select settings file or specify new settings file name";
    if (this.saveFileDialog1.ShowDialog() != DialogResult.OK)
      return;
    this.txtSaveFilePath.Text = this.saveFileDialog1.FileName;
    this.ReadTempalteSettings();
  }

  private bool ReadTempalteSettings()
  {
    bool flag = true;
    if (this.TemplateSettings == null)
      this.TemplateSettings = new TicketTemplateSettings();
    string text1 = this.cmbTicketTempalteName.Text;
    string text2 = this.templateManufacturerName.Text;
    FileInfo fileInfo = new FileInfo(this.txtSaveFilePath.Text);
    if (fileInfo.Exists)
    {
      if (!this.TemplateSettings.LoadTicketTemplateSettings(fileInfo.FullName) || fileInfo.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
        return false;
      this.cmbTicketTempalteName.Items.Clear();
      this.templateManufacturerName.Items.Clear();
      HashSet<string> source1 = new HashSet<string>();
      List<string> source2 = new List<string>();
      foreach (TicketTemplate template in this.TemplateSettings.Templates)
      {
        source2.Add(template.TemplateName);
        if (template.TemplateManufacturerName != null)
          source1.Add(template.TemplateManufacturerName);
      }
      try
      {
        if (source2 != null && source2.Count != 0)
          this.cmbTicketTempalteName.Items.AddRange((object[]) ((IEnumerable<string>) source2.ToArray<string>()).OrderBy<string, string>((Func<string, string>) (w => w), (IComparer<string>) StringComparer.OrdinalIgnoreCase).ToArray<string>());
        if (source1 != null)
        {
          if (source1.Count != 0)
            this.templateManufacturerName.Items.AddRange((object[]) ((IEnumerable<string>) source1.ToArray<string>()).OrderBy<string, string>((Func<string, string>) (w => w), (IComparer<string>) StringComparer.OrdinalIgnoreCase).ToArray<string>());
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.ToString());
      }
      flag = true;
    }
    else
    {
      TaskDialog.Show("Error", $"File not found at Template Settings Path Location: {this.txtSaveFilePath.Text}.New settings will be saved here, or you can specify a new location.");
      App.TicketTemplateSettingsPath = this.txtSaveFilePath.Text;
      this.TemplateSettings.Clear();
    }
    this.cmbTicketTempalteName.Text = text1 == "" ? this.strEmptyTemplateName : text1;
    this.templateManufacturerName.Text = text2 == "" ? this.strEmptyManufacturerName : text2;
    this.btnCreate.Enabled = true;
    return flag;
  }

  public bool AddOrModifyTemplate(TicketTemplate template)
  {
    IEnumerable<TicketTemplate> source = this.TemplateSettings.Templates.Where<TicketTemplate>((Func<TicketTemplate, bool>) (s => s.TemplateName == template.TemplateName));
    if (source.Any<TicketTemplate>())
    {
      try
      {
        string templateName = source.ToList<TicketTemplate>().First<TicketTemplate>().TemplateName;
        this.TemplateSettings.Templates.Remove(this.TemplateSettings.Templates.Find((Predicate<TicketTemplate>) (s => s.TemplateName == templateName)));
        this.TemplateSettings.Templates.Add(template);
        return true;
      }
      catch
      {
        return false;
      }
    }
    else
    {
      this.TemplateSettings.Templates.Add(template);
      return true;
    }
  }

  private void cmbTicketTempalteName_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.TemplateName = this.cmbTicketTempalteName.SelectedItem as string;
  }

  private void templateManufacturerName_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.TemplateManufacturerName = this.templateManufacturerName.SelectedItem as string;
  }

  internal bool SaveTempalteSettings()
  {
    return this.TemplateSettings.SaveTemplateSettings(this.txtSaveFilePath.Text);
  }

  private void btnCreate_Click(object sender, EventArgs e)
  {
    if (this.txtSaveFilePath.Text != "" && this.TemplateName != this.strEmptyTemplateName)
    {
      if (!this.txtSaveFilePath.Text.Contains(".xml"))
      {
        int num1 = (int) MessageBox.Show("Settings file specified is not a valid xml file.");
      }
      else
      {
        this.DialogResult = DialogResult.OK;
        this.Close();
      }
    }
    else if (this.txtSaveFilePath.Text == "")
    {
      int num2 = (int) MessageBox.Show("Settings file specified is not valid");
    }
    else
    {
      int num3 = (int) MessageBox.Show("Empty Template Name, please specify a name for this template.");
    }
  }

  private void cmbTicketTempalteName_TextChanged(object sender, EventArgs e)
  {
    this.TemplateName = this.cmbTicketTempalteName.Text;
  }

  private void templateManufacturerName_TextChanged(object sender, EventArgs e)
  {
    this.TemplateManufacturerName = this.templateManufacturerName.Text;
  }

  private void rbStackBOM_CheckedChanged(object sender, EventArgs e)
  {
    this.rbIndependentBOM.Checked = !this.rbStackBOM.Checked;
    this.bStackSchedules = this.rbStackBOM.Checked;
  }

  private void rbIndependentBOM_CheckedChanged(object sender, EventArgs e)
  {
    this.rbStackBOM.Checked = !this.rbIndependentBOM.Checked;
    this.bStackSchedules = this.rbStackBOM.Checked;
  }

  private void btnHelp_Click(object sender, EventArgs e)
  {
    Process.Start("http://www.edge.ptac.com/#!ticket-template-creator/c43bl");
  }

  private void txtSaveFilePath_TextChanged(object sender, EventArgs e)
  {
  }

  private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
  {
  }

  private void txtSaveFilePath_TextChanged_1(object sender, EventArgs e)
  {
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.templateManufacturerName = new ComboBox();
    this.manufacturerLabel = new Label();
    this.saveFileDialog1 = new SaveFileDialog();
    this.txtSaveFilePath = new TextBox();
    this.label1 = new Label();
    this.btnSettingsSaveLocation = new Button();
    this.cmbTicketTempalteName = new ComboBox();
    this.label2 = new Label();
    this.groupBox1 = new GroupBox();
    this.rbBottomAlign = new RadioButton();
    this.rbTopAlign = new RadioButton();
    this.btnCreate = new Button();
    this.btnCancel = new Button();
    this.btnHelp = new Button();
    this.lblConstructionProduct = new Label();
    this.groupBox2 = new GroupBox();
    this.rbIndependentBOM = new RadioButton();
    this.rbStackBOM = new RadioButton();
    this.groupBox1.SuspendLayout();
    this.groupBox2.SuspendLayout();
    this.SuspendLayout();
    this.templateManufacturerName.FormattingEnabled = true;
    this.templateManufacturerName.Location = new Point(23, 153);
    this.templateManufacturerName.Margin = new Padding(3, 2, 3, 2);
    this.templateManufacturerName.Name = "templateManufacturerName";
    this.templateManufacturerName.Size = new Size(443, 24);
    this.templateManufacturerName.TabIndex = 11;
    this.templateManufacturerName.SelectedIndexChanged += new EventHandler(this.templateManufacturerName_SelectedIndexChanged);
    this.templateManufacturerName.TextChanged += new EventHandler(this.templateManufacturerName_TextChanged);
    this.manufacturerLabel.AutoSize = true;
    this.manufacturerLabel.Location = new Point(23, 132);
    this.manufacturerLabel.Name = "manufacturerLabel";
    this.manufacturerLabel.Size = new Size(148, 16 /*0x10*/);
    this.manufacturerLabel.TabIndex = 10;
    this.manufacturerLabel.Text = "Template Manufacturer:";
    this.saveFileDialog1.FileOk += new CancelEventHandler(this.saveFileDialog1_FileOk);
    this.txtSaveFilePath.Location = new Point(23, 50);
    this.txtSaveFilePath.Margin = new Padding(3, 2, 3, 2);
    this.txtSaveFilePath.Name = "txtSaveFilePath";
    this.txtSaveFilePath.Size = new Size(790, 22);
    this.txtSaveFilePath.TabIndex = 0;
    this.txtSaveFilePath.TextChanged += new EventHandler(this.txtSaveFilePath_TextChanged_1);
    this.label1.AutoSize = true;
    this.label1.Location = new Point(20, 30);
    this.label1.Name = "label1";
    this.label1.Size = new Size(181, 16 /*0x10*/);
    this.label1.TabIndex = 1;
    this.label1.Text = "Ticket Template Settings File";
    this.btnSettingsSaveLocation.Location = new Point(818, 50);
    this.btnSettingsSaveLocation.Margin = new Padding(3, 2, 3, 2);
    this.btnSettingsSaveLocation.Name = "btnSettingsSaveLocation";
    this.btnSettingsSaveLocation.Size = new Size(29, 21);
    this.btnSettingsSaveLocation.TabIndex = 2;
    this.btnSettingsSaveLocation.Text = "...";
    this.btnSettingsSaveLocation.UseVisualStyleBackColor = true;
    this.btnSettingsSaveLocation.Click += new EventHandler(this.btnSettingsSaveLocation_Click);
    this.cmbTicketTempalteName.FormattingEnabled = true;
    this.cmbTicketTempalteName.Location = new Point(23, 222);
    this.cmbTicketTempalteName.Margin = new Padding(3, 2, 3, 2);
    this.cmbTicketTempalteName.Name = "cmbTicketTempalteName";
    this.cmbTicketTempalteName.Size = new Size(443, 24);
    this.cmbTicketTempalteName.TabIndex = 3;
    this.cmbTicketTempalteName.SelectedIndexChanged += new EventHandler(this.cmbTicketTempalteName_SelectedIndexChanged);
    this.cmbTicketTempalteName.TextChanged += new EventHandler(this.cmbTicketTempalteName_TextChanged);
    this.label2.AutoSize = true;
    this.label2.Location = new Point(23, 202);
    this.label2.Name = "label2";
    this.label2.Size = new Size(425, 16 /*0x10*/);
    this.label2.TabIndex = 4;
    this.label2.Text = "Ticket Template (specify a new name or select an existing to overwrite)";
    this.groupBox1.Controls.Add((Control) this.rbBottomAlign);
    this.groupBox1.Controls.Add((Control) this.rbTopAlign);
    this.groupBox1.Location = new Point(23, 283);
    this.groupBox1.Margin = new Padding(3, 2, 3, 2);
    this.groupBox1.Name = "groupBox1";
    this.groupBox1.Padding = new Padding(3, 2, 3, 2);
    this.groupBox1.Size = new Size(178, 80 /*0x50*/);
    this.groupBox1.TabIndex = 5;
    this.groupBox1.TabStop = false;
    this.groupBox1.Text = "BOM Alignment";
    this.rbBottomAlign.AutoSize = true;
    this.rbBottomAlign.Location = new Point(7, 46);
    this.rbBottomAlign.Margin = new Padding(3, 2, 3, 2);
    this.rbBottomAlign.Name = "rbBottomAlign";
    this.rbBottomAlign.Size = new Size(103, 20);
    this.rbBottomAlign.TabIndex = 1;
    this.rbBottomAlign.Text = "Align Bottom";
    this.rbBottomAlign.UseVisualStyleBackColor = true;
    this.rbBottomAlign.CheckedChanged += new EventHandler(this.rbBottomAlign_CheckedChanged);
    this.rbTopAlign.AutoSize = true;
    this.rbTopAlign.Checked = true;
    this.rbTopAlign.Location = new Point(7, 21);
    this.rbTopAlign.Margin = new Padding(3, 2, 3, 2);
    this.rbTopAlign.Name = "rbTopAlign";
    this.rbTopAlign.Size = new Size(86, 20);
    this.rbTopAlign.TabIndex = 0;
    this.rbTopAlign.TabStop = true;
    this.rbTopAlign.Text = "Align Top";
    this.rbTopAlign.UseVisualStyleBackColor = true;
    this.rbTopAlign.CheckedChanged += new EventHandler(this.rbTopAlign_CheckedChanged);
    this.btnCreate.Location = new Point(685, 370);
    this.btnCreate.Margin = new Padding(3, 2, 3, 2);
    this.btnCreate.Name = "btnCreate";
    this.btnCreate.Size = new Size(82, 34);
    this.btnCreate.TabIndex = 6;
    this.btnCreate.Text = "Create";
    this.btnCreate.UseVisualStyleBackColor = true;
    this.btnCreate.Click += new EventHandler(this.btnCreate_Click);
    this.btnCancel.DialogResult = DialogResult.Cancel;
    this.btnCancel.Location = new Point(771, 370);
    this.btnCancel.Margin = new Padding(3, 2, 3, 2);
    this.btnCancel.Name = "btnCancel";
    this.btnCancel.Size = new Size(78, 34);
    this.btnCancel.TabIndex = 7;
    this.btnCancel.Text = "Cancel";
    this.btnCancel.UseVisualStyleBackColor = true;
    this.btnHelp.Location = new Point(592, 370);
    this.btnHelp.Margin = new Padding(3, 2, 3, 2);
    this.btnHelp.Name = "btnHelp";
    this.btnHelp.Size = new Size(88, 34);
    this.btnHelp.TabIndex = 8;
    this.btnHelp.Text = "Help";
    this.btnHelp.UseVisualStyleBackColor = true;
    this.btnHelp.Click += new EventHandler(this.btnHelp_Click);
    this.lblConstructionProduct.AutoSize = true;
    this.lblConstructionProduct.Location = new Point(20, 94);
    this.lblConstructionProduct.Name = "lblConstructionProduct";
    this.lblConstructionProduct.Size = new Size(135, 16 /*0x10*/);
    this.lblConstructionProduct.TabIndex = 9;
    this.lblConstructionProduct.Text = "Construction Product: ";
    this.groupBox2.Controls.Add((Control) this.rbIndependentBOM);
    this.groupBox2.Controls.Add((Control) this.rbStackBOM);
    this.groupBox2.Location = new Point(222, 283);
    this.groupBox2.Margin = new Padding(3, 2, 3, 2);
    this.groupBox2.Name = "groupBox2";
    this.groupBox2.Padding = new Padding(3, 2, 3, 2);
    this.groupBox2.Size = new Size(244, 80 /*0x50*/);
    this.groupBox2.TabIndex = 10;
    this.groupBox2.TabStop = false;
    this.groupBox2.Text = "BOM Stacking";
    this.rbIndependentBOM.AutoSize = true;
    this.rbIndependentBOM.Location = new Point(5, 46);
    this.rbIndependentBOM.Margin = new Padding(3, 2, 3, 2);
    this.rbIndependentBOM.Name = "rbIndependentBOM";
    this.rbIndependentBOM.Size = new Size(136, 20);
    this.rbIndependentBOM.TabIndex = 0;
    this.rbIndependentBOM.Text = "Independent BOM";
    this.rbIndependentBOM.UseVisualStyleBackColor = true;
    this.rbIndependentBOM.CheckedChanged += new EventHandler(this.rbIndependentBOM_CheckedChanged);
    this.rbStackBOM.AutoSize = true;
    this.rbStackBOM.Checked = true;
    this.rbStackBOM.Location = new Point(7, 20);
    this.rbStackBOM.Margin = new Padding(3, 2, 3, 2);
    this.rbStackBOM.Name = "rbStackBOM";
    this.rbStackBOM.Size = new Size(95, 20);
    this.rbStackBOM.TabIndex = 0;
    this.rbStackBOM.TabStop = true;
    this.rbStackBOM.Text = "Stack BOM";
    this.rbStackBOM.UseVisualStyleBackColor = true;
    this.rbStackBOM.CheckedChanged += new EventHandler(this.rbStackBOM_CheckedChanged);
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(866, 406);
    this.Controls.Add((Control) this.groupBox2);
    this.Controls.Add((Control) this.lblConstructionProduct);
    this.Controls.Add((Control) this.btnHelp);
    this.Controls.Add((Control) this.btnCancel);
    this.Controls.Add((Control) this.btnCreate);
    this.Controls.Add((Control) this.groupBox1);
    this.Controls.Add((Control) this.label2);
    this.Controls.Add((Control) this.cmbTicketTempalteName);
    this.Controls.Add((Control) this.btnSettingsSaveLocation);
    this.Controls.Add((Control) this.label1);
    this.Controls.Add((Control) this.txtSaveFilePath);
    this.Controls.Add((Control) this.manufacturerLabel);
    this.Controls.Add((Control) this.templateManufacturerName);
    this.Margin = new Padding(3, 2, 3, 2);
    this.Name = nameof (TemplateCreatorForm);
    this.StartPosition = FormStartPosition.CenterParent;
    this.Text = "Ticket Template Creator";
    this.Load += new EventHandler(this.TemplateCreatorForm_Load);
    this.groupBox1.ResumeLayout(false);
    this.groupBox1.PerformLayout();
    this.groupBox2.ResumeLayout(false);
    this.groupBox2.PerformLayout();
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
