// Decompiled with JetBrains decompiler
// Type: EDGE.EULAForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

#nullable disable
namespace EDGE;

public class EULAForm : Form
{
  public bool accepted;
  private IContainer components;
  private Label label1;
  private RichTextBox richTextBox1;
  private Button button1;
  private Button button2;

  public EULAForm()
  {
    string str = "2024";
    Path.GetDirectoryName(typeof (App).Assembly.Location);
    string path = $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{str}\\PTAC_EDGE_BUNDLE\\Enduser_License_Agreement.rtf";
    this.InitializeComponent();
    this.ControlBox = false;
    this.richTextBox1.LoadFile(path);
    this.FormClosing += new FormClosingEventHandler(this.EULAForm_FormClosing);
  }

  private void EULAForm_FormClosing(object sender, FormClosingEventArgs e)
  {
    e.Cancel = true;
    this.Dispose();
    this.Close();
  }

  private void EULAForm_Cancel(object sender, EventArgs e)
  {
    this.accepted = false;
    this.Dispose();
    this.Close();
  }

  private void button1_Click(object sender, EventArgs e)
  {
    this.accepted = true;
    this.Dispose();
    this.Close();
  }

  private void button2_Click(object sender, EventArgs e)
  {
    this.accepted = false;
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
    this.richTextBox1 = new RichTextBox();
    this.button1 = new Button();
    this.button2 = new Button();
    this.SuspendLayout();
    this.label1.AutoSize = true;
    this.label1.Location = new Point(7, 9);
    this.label1.Margin = new Padding(2, 0, 2, 0);
    this.label1.MaximumSize = new Size(483, 0);
    this.label1.Name = "label1";
    this.label1.Size = new Size(483, 13);
    this.label1.TabIndex = 0;
    this.label1.Text = "Before you may use the software, you must read and accept the End User License Agreement below. ";
    this.richTextBox1.Location = new Point(10, 40);
    this.richTextBox1.Margin = new Padding(2);
    this.richTextBox1.Name = "richTextBox1";
    this.richTextBox1.Size = new Size(481, 343);
    this.richTextBox1.TabIndex = 1;
    this.richTextBox1.Text = "";
    this.button1.Location = new Point(259, 387);
    this.button1.Margin = new Padding(2);
    this.button1.Name = "button1";
    this.button1.Size = new Size(109, 25);
    this.button1.TabIndex = 2;
    this.button1.Text = "I Agree";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.button1_Click);
    this.button2.Location = new Point(382, 387);
    this.button2.Margin = new Padding(2);
    this.button2.Name = "button2";
    this.button2.Size = new Size(109, 25);
    this.button2.TabIndex = 3;
    this.button2.Text = "I Do Not Agree";
    this.button2.UseVisualStyleBackColor = true;
    this.button2.Click += new EventHandler(this.button2_Click);
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.AutoScroll = true;
    this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    this.ClientSize = new Size(502, 426);
    this.Controls.Add((Control) this.button2);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.richTextBox1);
    this.Controls.Add((Control) this.label1);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.Margin = new Padding(2);
    this.MinimumSize = new Size(518, 465);
    this.Name = nameof (EULAForm);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Text = "EDGE^R License Agreement";
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
