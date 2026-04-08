// Decompiled with JetBrains decompiler
// Type: EDGE.LicenseRequestForm
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

public class LicenseRequestForm : Form
{
  public string name;
  public string companyName;
  public string phone;
  public bool cancelled;
  private IContainer components;
  private Label label1;
  private Label label2;
  private TextBox textBox1;
  private Label label3;
  private TextBox textBox2;
  private Label label4;
  private TextBox textBox3;
  private Button button1;
  private Button button2;

  public LicenseRequestForm()
  {
    this.InitializeComponent();
    this.ControlBox = false;
    this.FormClosing += new FormClosingEventHandler(this.LicenseRequestForm_FormClosing);
  }

  private void LicenseRequestForm_FormClosing(object sender, FormClosingEventArgs e)
  {
    e.Cancel = true;
    this.Dispose();
    this.Close();
  }

  private void button1_Click(object sender, EventArgs e)
  {
    this.name = this.textBox1.Text;
    this.companyName = this.textBox2.Text;
    this.phone = this.textBox3.Text;
    if (this.name.Equals("") || this.companyName.Equals("") || this.phone.Equals(""))
    {
      new TaskDialog("License Request Fields Blank")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        TitleAutoPrefix = false,
        AllowCancellation = false,
        MainInstruction = "Fields Left Blank",
        MainContent = "Please enter values into all fields before clicking continue. "
      }.Show();
      this.FormClosing += new FormClosingEventHandler(this.LicenseRequestForm_FormClosing);
    }
    else
    {
      this.Dispose();
      this.Close();
    }
  }

  private void button2_Click(object sender, EventArgs e)
  {
    this.name = "";
    this.companyName = "";
    this.phone = "";
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
    this.label2 = new Label();
    this.textBox1 = new TextBox();
    this.label3 = new Label();
    this.textBox2 = new TextBox();
    this.label4 = new Label();
    this.textBox3 = new TextBox();
    this.button1 = new Button();
    this.button2 = new Button();
    this.SuspendLayout();
    this.label1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.label1.AutoSize = true;
    this.label1.Location = new Point(1, 5);
    this.label1.MaximumSize = new Size(385, 0);
    this.label1.Name = "label1";
    this.label1.Size = new Size(385, 17);
    this.label1.TabIndex = 0;
    this.label1.Text = "Please enter your personal information to request a license.";
    this.label2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.label2.AutoSize = true;
    this.label2.Location = new Point(9, 41);
    this.label2.Name = "label2";
    this.label2.Size = new Size(75, 17);
    this.label2.TabIndex = 1;
    this.label2.Text = "Full Name:";
    this.textBox1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.textBox1.Location = new Point(12, 61);
    this.textBox1.Name = "textBox1";
    this.textBox1.Size = new Size(328, 22);
    this.textBox1.TabIndex = 2;
    this.label3.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.label3.AutoSize = true;
    this.label3.Location = new Point(10, 104);
    this.label3.Name = "label3";
    this.label3.Size = new Size(112 /*0x70*/, 17);
    this.label3.TabIndex = 3;
    this.label3.Text = "Company Name:";
    this.textBox2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.textBox2.Location = new Point(13, 125);
    this.textBox2.Name = "textBox2";
    this.textBox2.Size = new Size(327, 22);
    this.textBox2.TabIndex = 4;
    this.label4.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.label4.AutoSize = true;
    this.label4.Location = new Point(13, 169);
    this.label4.Name = "label4";
    this.label4.Size = new Size(107, 17);
    this.label4.TabIndex = 5;
    this.label4.Text = "Phone Number:";
    this.textBox3.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.textBox3.Location = new Point(13, 189);
    this.textBox3.Name = "textBox3";
    this.textBox3.Size = new Size(327, 22);
    this.textBox3.TabIndex = 6;
    this.button1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.button1.Location = new Point(184, 236);
    this.button1.Name = "button1";
    this.button1.Size = new Size(107, 35);
    this.button1.TabIndex = 7;
    this.button1.Text = "Continue";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.button1_Click);
    this.button2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.button2.Location = new Point(62, 236);
    this.button2.Name = "button2";
    this.button2.Size = new Size(107, 35);
    this.button2.TabIndex = 8;
    this.button2.Text = "Cancel";
    this.button2.UseVisualStyleBackColor = true;
    this.button2.Click += new EventHandler(this.button2_Click);
    this.AutoScaleMode = AutoScaleMode.None;
    this.ClientSize = new Size(387, 278);
    this.Controls.Add((Control) this.button2);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.textBox3);
    this.Controls.Add((Control) this.label4);
    this.Controls.Add((Control) this.textBox2);
    this.Controls.Add((Control) this.label3);
    this.Controls.Add((Control) this.textBox1);
    this.Controls.Add((Control) this.label2);
    this.Controls.Add((Control) this.label1);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.MinimumSize = new Size(405, 325);
    this.Name = nameof (LicenseRequestForm);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Text = "License Request";
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
