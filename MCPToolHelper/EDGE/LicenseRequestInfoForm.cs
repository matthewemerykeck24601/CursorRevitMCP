// Decompiled with JetBrains decompiler
// Type: EDGE.LicenseRequestInfoForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE;

public class LicenseRequestInfoForm : Form
{
  private IContainer components;
  private Label label1;
  private RichTextBox richTextBox1;
  private Button button1;

  public LicenseRequestInfoForm(string emailText)
  {
    this.InitializeComponent(emailText);
    this.ControlBox = false;
  }

  private void button1_Click(object sender, EventArgs e) => this.Close();

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent(string emailText)
  {
    this.label1 = new Label();
    this.richTextBox1 = new RichTextBox();
    this.button1 = new Button();
    this.SuspendLayout();
    this.label1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
    this.label1.AutoSize = true;
    this.label1.Location = new Point(12, 9);
    this.label1.Name = "label1";
    this.label1.Size = new Size(508, 17);
    this.label1.TabIndex = 0;
    this.label1.Text = "Please copy the following text to your email client of choice to request a license:";
    this.richTextBox1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.richTextBox1.Location = new Point(12, 34);
    this.richTextBox1.Name = "richTextBox1";
    this.richTextBox1.Size = new Size(539, 440);
    this.richTextBox1.TabIndex = 1;
    this.richTextBox1.AppendText(emailText);
    this.button1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
    this.button1.Location = new Point(404, 485);
    this.button1.Name = "button1";
    this.button1.Size = new Size(147, 36);
    this.button1.TabIndex = 2;
    this.button1.Text = "Close";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.button1_Click);
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.None;
    this.MinimumSize = new Size(300, 300);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.ClientSize = new Size(563, 541);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.richTextBox1);
    this.Controls.Add((Control) this.label1);
    this.MaximizeBox = false;
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Name = nameof (LicenseRequestInfoForm);
    this.Text = "License Request";
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
