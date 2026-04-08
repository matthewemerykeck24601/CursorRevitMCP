// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.QA.DebugModeSwitches
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.QA;

public class DebugModeSwitches : Form
{
  public bool[] debugSwitches = new bool[8];
  private IContainer components;
  private CheckBox checkBox1;
  private CheckBox checkBox2;
  private CheckBox checkBox3;
  private CheckBox checkBox4;
  private CheckBox checkBox5;
  private CheckBox checkBox6;
  private CheckBox checkBox7;
  private CheckBox checkBox8;
  private Button button1;

  public DebugModeSwitches() => this.InitializeComponent();

  private void DebugModeSwitches_Load(object sender, EventArgs e)
  {
  }

  private void button1_Click(object sender, EventArgs e)
  {
    this.debugSwitches[0] = this.checkBox1.Checked;
    this.debugSwitches[1] = this.checkBox2.Checked;
    this.debugSwitches[2] = this.checkBox3.Checked;
    this.debugSwitches[3] = this.checkBox4.Checked;
    this.debugSwitches[4] = this.checkBox5.Checked;
    this.debugSwitches[5] = this.checkBox6.Checked;
    this.debugSwitches[6] = this.checkBox7.Checked;
    this.debugSwitches[7] = this.checkBox8.Checked;
  }

  private void DebugModeSwitches_Shown(object sender, EventArgs e)
  {
    this.checkBox1.Checked = this.debugSwitches[0];
    this.checkBox2.Checked = this.debugSwitches[1];
    this.checkBox3.Checked = this.debugSwitches[2];
    this.checkBox4.Checked = this.debugSwitches[3];
    this.checkBox5.Checked = this.debugSwitches[4];
    this.checkBox6.Checked = this.debugSwitches[5];
    this.checkBox7.Checked = this.debugSwitches[6];
    this.checkBox8.Checked = this.debugSwitches[7];
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.checkBox1 = new CheckBox();
    this.checkBox2 = new CheckBox();
    this.checkBox3 = new CheckBox();
    this.checkBox4 = new CheckBox();
    this.checkBox5 = new CheckBox();
    this.checkBox6 = new CheckBox();
    this.checkBox7 = new CheckBox();
    this.checkBox8 = new CheckBox();
    this.button1 = new Button();
    this.SuspendLayout();
    this.checkBox1.AutoSize = true;
    this.checkBox1.Location = new Point(13, 13);
    this.checkBox1.Name = "checkBox1";
    this.checkBox1.Size = new Size(145, 17);
    this.checkBox1.TabIndex = 0;
    this.checkBox1.Text = "Family Types Comparison";
    this.checkBox1.UseVisualStyleBackColor = true;
    this.checkBox2.AutoSize = true;
    this.checkBox2.Location = new Point(12, 36);
    this.checkBox2.Name = "checkBox2";
    this.checkBox2.Size = new Size(205, 17);
    this.checkBox2.TabIndex = 0;
    this.checkBox2.Text = "Instance/Type Parameter Comparison";
    this.checkBox2.UseVisualStyleBackColor = true;
    this.checkBox3.AutoSize = true;
    this.checkBox3.Location = new Point(12, 59);
    this.checkBox3.Name = "checkBox3";
    this.checkBox3.Size = new Size(252, 17);
    this.checkBox3.TabIndex = 0;
    this.checkBox3.Text = "Structural Framing Material Volumes Comparison";
    this.checkBox3.UseVisualStyleBackColor = true;
    this.checkBox4.AutoSize = true;
    this.checkBox4.Location = new Point(12, 82);
    this.checkBox4.Name = "checkBox4";
    this.checkBox4.Size = new Size(246, 17);
    this.checkBox4.TabIndex = 0;
    this.checkBox4.Text = "Addon Family Type Count and Material Volume";
    this.checkBox4.UseVisualStyleBackColor = true;
    this.checkBox5.AutoSize = true;
    this.checkBox5.Location = new Point(12, 105);
    this.checkBox5.Name = "checkBox5";
    this.checkBox5.Size = new Size(176 /*0xB0*/, 17);
    this.checkBox5.TabIndex = 0;
    this.checkBox5.Text = "Plate Names Family Type Count";
    this.checkBox5.UseVisualStyleBackColor = true;
    this.checkBox6.AutoSize = true;
    this.checkBox6.Location = new Point(12, 128 /*0x80*/);
    this.checkBox6.Name = "checkBox6";
    this.checkBox6.Size = new Size(176 /*0xB0*/, 17);
    this.checkBox6.TabIndex = 0;
    this.checkBox6.Text = "Addon Location and Orientation";
    this.checkBox6.UseVisualStyleBackColor = true;
    this.checkBox7.AutoSize = true;
    this.checkBox7.Location = new Point(13, 151);
    this.checkBox7.Name = "checkBox7";
    this.checkBox7.Size = new Size(169, 17);
    this.checkBox7.TabIndex = 0;
    this.checkBox7.Text = "Plate Location and Orientation";
    this.checkBox7.UseVisualStyleBackColor = true;
    this.checkBox8.AutoSize = true;
    this.checkBox8.Location = new Point(12, 174);
    this.checkBox8.Name = "checkBox8";
    this.checkBox8.Size = new Size(227, 17);
    this.checkBox8.TabIndex = 0;
    this.checkBox8.Text = "Structural Framing Solid Faces Comparison";
    this.checkBox8.UseVisualStyleBackColor = true;
    this.button1.DialogResult = DialogResult.OK;
    this.button1.Location = new Point(215, 331);
    this.button1.Name = "button1";
    this.button1.Size = new Size(75, 23);
    this.button1.TabIndex = 1;
    this.button1.Text = "OK";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.button1_Click);
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(302, 366);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.checkBox8);
    this.Controls.Add((Control) this.checkBox7);
    this.Controls.Add((Control) this.checkBox6);
    this.Controls.Add((Control) this.checkBox5);
    this.Controls.Add((Control) this.checkBox4);
    this.Controls.Add((Control) this.checkBox3);
    this.Controls.Add((Control) this.checkBox2);
    this.Controls.Add((Control) this.checkBox1);
    this.Name = nameof (DebugModeSwitches);
    this.Text = nameof (DebugModeSwitches);
    this.Load += new EventHandler(this.DebugModeSwitches_Load);
    this.Shown += new EventHandler(this.DebugModeSwitches_Shown);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
