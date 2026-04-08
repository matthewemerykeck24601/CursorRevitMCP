// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.DisplayDebugInfoForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE.__Testing;

public class DisplayDebugInfoForm : Form
{
  public string displayText = "*error*";
  private IContainer components;
  private TextBox txtInfo;
  private Button button1;

  public DisplayDebugInfoForm() => this.InitializeComponent();

  private void DisplayRebarDebugInfoForm_Load(object sender, EventArgs e)
  {
    this.txtInfo.Text = this.displayText;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.txtInfo = new TextBox();
    this.button1 = new Button();
    this.SuspendLayout();
    this.txtInfo.Location = new Point(51, 31 /*0x1F*/);
    this.txtInfo.Multiline = true;
    this.txtInfo.Name = "txtInfo";
    this.txtInfo.ScrollBars = ScrollBars.Both;
    this.txtInfo.Size = new Size(1387, 554);
    this.txtInfo.TabIndex = 0;
    this.button1.DialogResult = DialogResult.OK;
    this.button1.Location = new Point(1283, 632);
    this.button1.Name = "button1";
    this.button1.Size = new Size(129, 45);
    this.button1.TabIndex = 1;
    this.button1.Text = "OK";
    this.button1.UseVisualStyleBackColor = true;
    this.AutoScaleDimensions = new SizeF(9f, 20f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(1511, 701);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.txtInfo);
    this.Name = "DisplayRebarDebugInfoForm";
    this.Text = "DisplayRebarDebugInfoForm";
    this.Load += new EventHandler(this.DisplayRebarDebugInfoForm_Load);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
