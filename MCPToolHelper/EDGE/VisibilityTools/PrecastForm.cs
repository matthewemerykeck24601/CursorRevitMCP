// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.PrecastForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE.VisibilityTools;

public class PrecastForm : Form
{
  private int precastOpacity = 100;
  private IContainer components;
  public Label PrecastLbl;
  public TextBox PrecastTxt;
  public TrackBar PrecastSlider;
  private Button CancelBtn;
  private Button AcceptBtn;

  public PrecastForm()
  {
    this.precastOpacity = 100;
    this.InitializeComponent();
  }

  public PrecastForm(int opacity)
  {
    if (opacity >= 0 || opacity <= 100)
      this.precastOpacity = opacity;
    this.InitializeComponent();
  }

  private void PrecastForm_Load(object sender, EventArgs e)
  {
    this.PrecastSlider.Value = this.precastOpacity;
    this.PrecastTxt.Text = this.PrecastSlider.Value.ToString() + "%";
  }

  private void PrecastSlider_Scroll(object sender, EventArgs e)
  {
    this.PrecastTxt.Text = this.PrecastSlider.Value.ToString() + "%";
  }

  private void AcceptBtn_Click(object sender, EventArgs e)
  {
    if (this.ParseOpacity() <= -1)
      return;
    App.DialogSwitches.opacity = this.ParseOpacity();
    Form parent = (sender as Control).Parent as Form;
    parent.DialogResult = DialogResult.OK;
    parent.Close();
  }

  private void CancelBtn_Click(object sender, EventArgs e)
  {
    Form parent = (sender as Control).Parent as Form;
    parent.DialogResult = DialogResult.Cancel;
    parent.Close();
  }

  public int ParseOpacity()
  {
    string s = this.PrecastTxt.Text.Replace("%", "");
    try
    {
      if (int.Parse(s) <= 0)
        s = "0";
      if (int.Parse(s) >= 100)
        s = "100";
    }
    catch (Exception ex)
    {
      TaskDialog.Show("Invalid Input", "You entered an invalid value for opacity, accepted values are percentages between 0 and 100.");
      Console.WriteLine(ex.Message);
      return -1;
    }
    return int.Parse(s);
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.PrecastLbl = new Label();
    this.PrecastTxt = new TextBox();
    this.PrecastSlider = new TrackBar();
    this.CancelBtn = new Button();
    this.AcceptBtn = new Button();
    this.PrecastSlider.BeginInit();
    this.SuspendLayout();
    this.PrecastLbl.Anchor = AnchorStyles.None;
    this.PrecastLbl.AutoSize = true;
    this.PrecastLbl.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.PrecastLbl.Location = new Point(144 /*0x90*/, 9);
    this.PrecastLbl.Name = "PrecastLbl";
    this.PrecastLbl.Size = new Size(222, 25);
    this.PrecastLbl.TabIndex = 21;
    this.PrecastLbl.Text = "Precast Product Opacity";
    this.PrecastTxt.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.PrecastTxt.Location = new Point(432, 46);
    this.PrecastTxt.Name = "PrecastTxt";
    this.PrecastTxt.Size = new Size(56, 30);
    this.PrecastTxt.TabIndex = 20;
    this.PrecastTxt.Text = "50%";
    this.PrecastSlider.LargeChange = 10;
    this.PrecastSlider.Location = new Point(48 /*0x30*/, 46);
    this.PrecastSlider.Maximum = 100;
    this.PrecastSlider.Name = "PrecastSlider";
    this.PrecastSlider.Size = new Size(378, 56);
    this.PrecastSlider.TabIndex = 19;
    this.PrecastSlider.TickStyle = TickStyle.None;
    this.PrecastSlider.Value = 50;
    this.PrecastSlider.Scroll += new EventHandler(this.PrecastSlider_Scroll);
    this.CancelBtn.DialogResult = DialogResult.Cancel;
    this.CancelBtn.Location = new Point(12, 96 /*0x60*/);
    this.CancelBtn.Name = "CancelBtn";
    this.CancelBtn.Size = new Size(90, 25);
    this.CancelBtn.TabIndex = 23;
    this.CancelBtn.Text = "Cancel";
    this.CancelBtn.TextAlign = ContentAlignment.TopCenter;
    this.CancelBtn.UseVisualStyleBackColor = true;
    this.CancelBtn.Click += new EventHandler(this.CancelBtn_Click);
    this.AcceptBtn.Location = new Point(419, 96 /*0x60*/);
    this.AcceptBtn.Name = "AcceptBtn";
    this.AcceptBtn.Size = new Size(90, 25);
    this.AcceptBtn.TabIndex = 22;
    this.AcceptBtn.Text = "Accept";
    this.AcceptBtn.TextAlign = ContentAlignment.TopCenter;
    this.AcceptBtn.UseVisualStyleBackColor = true;
    this.AcceptBtn.Click += new EventHandler(this.AcceptBtn_Click);
    this.AcceptButton = (IButtonControl) this.AcceptBtn;
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.CancelButton = (IButtonControl) this.CancelBtn;
    this.ClientSize = new Size(521, 133);
    this.Controls.Add((Control) this.CancelBtn);
    this.Controls.Add((Control) this.AcceptBtn);
    this.Controls.Add((Control) this.PrecastLbl);
    this.Controls.Add((Control) this.PrecastTxt);
    this.Controls.Add((Control) this.PrecastSlider);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    this.Name = nameof (PrecastForm);
    this.ShowIcon = false;
    this.StartPosition = FormStartPosition.CenterParent;
    this.Text = "Precast Opacity";
    this.Load += new EventHandler(this.PrecastForm_Load);
    this.PrecastSlider.EndInit();
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
