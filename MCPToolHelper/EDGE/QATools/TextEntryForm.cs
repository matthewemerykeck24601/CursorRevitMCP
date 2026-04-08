// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.TextEntryForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE.QATools;

public class TextEntryForm : Form
{
  public string JournalComment = "";
  private IContainer components;
  private Label label1;
  private TextBox textBox1;
  private Button button1;

  public TextEntryForm() => this.InitializeComponent();

  private void button1_Click(object sender, EventArgs e)
  {
    this.JournalComment = this.textBox1.Text;
    this.DialogResult = DialogResult.OK;
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
    this.textBox1 = new TextBox();
    this.button1 = new Button();
    this.SuspendLayout();
    this.label1.AutoSize = true;
    this.label1.Location = new Point(13, 13);
    this.label1.Name = "label1";
    this.label1.Size = new Size(65, 16 /*0x10*/);
    this.label1.TabIndex = 0;
    this.label1.Text = "Comment";
    this.textBox1.Location = new Point(16 /*0x10*/, 33);
    this.textBox1.Multiline = true;
    this.textBox1.Name = "textBox1";
    this.textBox1.Size = new Size(968, 106);
    this.textBox1.TabIndex = 1;
    this.button1.Location = new Point(909, 145);
    this.button1.Name = "button1";
    this.button1.Size = new Size(75, 23);
    this.button1.TabIndex = 2;
    this.button1.Text = "OK";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.button1_Click);
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(1019, 194);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.textBox1);
    this.Controls.Add((Control) this.label1);
    this.Name = nameof (TextEntryForm);
    this.SizeGripStyle = SizeGripStyle.Hide;
    this.StartPosition = FormStartPosition.CenterParent;
    this.Text = nameof (TextEntryForm);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
