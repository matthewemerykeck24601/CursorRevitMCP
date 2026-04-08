// Decompiled with JetBrains decompiler
// Type: EDGE.LicenseForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE;

public class LicenseForm : Form
{
  public string licenseCode;
  public bool cancelled;
  private IContainer components;
  private TextBox textBox1;
  private Label label1;
  private Button button1;
  private Button button2;

  public LicenseForm()
  {
    this.InitializeComponent();
    this.ControlBox = false;
    this.FormClosing += new FormClosingEventHandler(this.LicenseForm_FormClosing);
  }

  private void LicenseForm_FormClosing(object sender, FormClosingEventArgs e)
  {
    e.Cancel = true;
    this.Dispose();
    this.Close();
  }

  private void button1_Click(object sender, EventArgs e)
  {
    this.licenseCode = this.textBox1.Text;
    this.Dispose();
    this.Close();
  }

  private void button2_Click(object sender, EventArgs e)
  {
    this.licenseCode = "";
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
    this.textBox1 = new TextBox();
    this.label1 = new Label();
    this.button1 = new Button();
    this.button2 = new Button();
    this.SuspendLayout();
    this.textBox1.Location = new Point(12, 49);
    this.textBox1.Margin = new Padding(3, 2, 3, 2);
    this.textBox1.Name = "textBox1";
    this.textBox1.Size = new Size(655, 22);
    this.textBox1.TabIndex = 0;
    this.label1.AutoSize = true;
    this.label1.Location = new Point(13, 14);
    this.label1.MaximumSize = new Size(653, 0);
    this.label1.Name = "label1";
    this.label1.Size = new Size(284, 17);
    this.label1.TabIndex = 1;
    this.label1.Text = "Please enter your license code for EDGE^R";
    this.button1.Location = new Point(517, 89);
    this.button1.Margin = new Padding(3, 2, 3, 2);
    this.button1.Name = "button1";
    this.button1.Size = new Size(149, 37);
    this.button1.TabIndex = 2;
    this.button1.Text = "Submit";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.button1_Click);
    this.button2.Location = new Point(12, 89);
    this.button2.Margin = new Padding(3, 2, 3, 2);
    this.button2.Name = "button2";
    this.button2.Size = new Size(149, 37);
    this.button2.TabIndex = 3;
    this.button2.Text = "Cancel";
    this.button2.UseVisualStyleBackColor = true;
    this.button2.Click += new EventHandler(this.button2_Click);
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(675, 137);
    this.Controls.Add((Control) this.button2);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.label1);
    this.Controls.Add((Control) this.textBox1);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.Margin = new Padding(3, 2, 3, 2);
    this.MinimumSize = new Size(693, 184);
    this.Name = nameof (LicenseForm);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Text = "Enter EDGE^R License Code";
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
