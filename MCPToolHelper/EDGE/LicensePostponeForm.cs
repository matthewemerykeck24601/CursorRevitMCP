// Decompiled with JetBrains decompiler
// Type: EDGE.LicensePostponeForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE;

public class LicensePostponeForm : Form
{
  public int PostPoneDays;
  public bool cancelled;
  private IContainer components;
  private Label label1;
  private ComboBox comboBox1;
  private Button button1;
  private Button button2;

  public LicensePostponeForm()
  {
    this.InitializeComponent();
    this.ControlBox = false;
    this.FormClosing += new FormClosingEventHandler(this.LicensePostponeForm_FormClosing);
  }

  private void LicensePostponeForm_FormClosing(object sender, FormClosingEventArgs e)
  {
    e.Cancel = true;
    this.Dispose();
    this.Close();
  }

  private void button1_Click(object sender, EventArgs e)
  {
    string text = this.comboBox1.Text;
    if (text.Equals(""))
    {
      new TaskDialog("Nothing Selected")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        TitleAutoPrefix = false,
        AllowCancellation = false,
        MainInstruction = "Nothing Selected",
        MainContent = "Please select the number of days you wish to postpone before clicking continue. "
      }.Show();
      this.FormClosing += new FormClosingEventHandler(this.LicensePostponeForm_FormClosing);
    }
    else
    {
      int.TryParse(text, out this.PostPoneDays);
      if (this.PostPoneDays == 0)
      {
        new TaskDialog("Nothing Selected")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          TitleAutoPrefix = false,
          AllowCancellation = false,
          MainInstruction = "Nothing Selected",
          MainContent = "Please select the number of days you wish to postpone before clicking continue. "
        }.Show();
        this.FormClosing += new FormClosingEventHandler(this.LicensePostponeForm_FormClosing);
      }
      else
      {
        this.Dispose();
        this.Close();
      }
    }
  }

  private void button2_Click(object sender, EventArgs e)
  {
    this.PostPoneDays = 0;
    this.cancelled = true;
    this.Dispose();
    this.Close();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.label1 = new Label();
    this.comboBox1 = new ComboBox();
    this.button1 = new Button();
    this.button2 = new Button();
    this.SuspendLayout();
    this.label1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.label1.AutoSize = true;
    this.label1.Location = new Point(36, 9);
    this.label1.Margin = new Padding(2, 0, 2, 0);
    this.label1.MaximumSize = new Size(292, 0);
    this.label1.Name = "label1";
    this.label1.Size = new Size(281, 13);
    this.label1.TabIndex = 0;
    this.label1.Text = "How many days would you like to postpone this message?";
    this.label1.TextAlign = ContentAlignment.TopCenter;
    this.comboBox1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
    this.comboBox1.FormattingEnabled = true;
    this.comboBox1.Items.AddRange(new object[4]
    {
      (object) "5",
      (object) "10",
      (object) "15",
      (object) "30"
    });
    this.comboBox1.Location = new Point(97, 45);
    this.comboBox1.Margin = new Padding(2);
    this.comboBox1.Name = "comboBox1";
    this.comboBox1.Size = new Size(124, 21);
    this.comboBox1.TabIndex = 1;
    this.button1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.button1.Location = new Point(199, 78);
    this.button1.Margin = new Padding(2);
    this.button1.Name = "button1";
    this.button1.Size = new Size(108, 28);
    this.button1.TabIndex = 2;
    this.button1.Text = "Continue";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.button1_Click);
    this.button2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.button2.Location = new Point(20, 78);
    this.button2.Margin = new Padding(2);
    this.button2.Name = "button2";
    this.button2.Size = new Size(108, 28);
    this.button2.TabIndex = 3;
    this.button2.Text = "Cancel";
    this.button2.UseVisualStyleBackColor = true;
    this.button2.Click += new EventHandler(this.button2_Click);
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(328, 111);
    this.Controls.Add((Control) this.button2);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.comboBox1);
    this.Controls.Add((Control) this.label1);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.Margin = new Padding(2);
    this.MinimumSize = new Size(344, 150);
    this.Name = nameof (LicensePostponeForm);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Text = "Postpone Message";
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
