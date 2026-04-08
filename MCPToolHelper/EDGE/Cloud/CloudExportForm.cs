// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.CloudExportForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE.Cloud;

public class CloudExportForm : Form
{
  public Action<string, string, string, string, string> ExportModel;
  public AssemblyInfo AssemblyInfo;
  public static int CloudId = 1;
  private IContainer components;
  private TableLayoutPanel tableLayoutPanel2;
  private Label lblServer;
  private Label lblUsername;
  private Label lblPassword;
  private TextBox txtUsername;
  private TextBox txtPassword;
  private ComboBox cboServer;
  private TableLayoutPanel tableLayoutPanel1;
  private Button btnExport;
  private LinkLabel lnkStatus;
  private TextBox txtInfo;
  private ProgressBar progressBar;
  private Label lblTitle;
  private TableLayoutPanel tableLayoutPanel3;
  private TableLayoutPanel tableLayoutPanel4;
  private CheckBox chkView;
  private CheckBox chkSheet;

  public string Token { get; set; }

  public string AuthenticationUrl => CloudExportCommand.AuthenticationUrl;

  public string ServiceUrl => CloudExportCommand.ServiceUrl;

  public string WebUrl => CloudExportCommand.WebUrl;

  public string Username
  {
    set => this.txtUsername.Text = value;
    get => this.txtUsername.Text;
  }

  public string Password
  {
    set => this.txtPassword.Text = value;
    get => this.txtPassword.Text;
  }

  public CloudExportForm()
  {
    this.InitializeComponent();
    this.InitComponent();
  }

  private void MainForm_Load(object sender, EventArgs e)
  {
    this.Token = CloudExportCommand.Token;
    if (!string.IsNullOrEmpty(this.Token))
    {
      this.Username = CloudExportCommand.Username;
      this.Password = CloudExportCommand.Password;
    }
    this.HideAuthentication(this.Token);
    if (this.AssemblyInfo != null)
      this.txtInfo.Text = $"Producer: {this.AssemblyInfo.ProducerName}\r\nPlant: {this.AssemblyInfo.PlantName}\r\nProject: {this.AssemblyInfo.ProjectName}";
    this.cboServer.SelectedIndex = CloudExportForm.CloudId;
    this.txtUsername.Text = this.Username;
    this.txtPassword.Text = this.Password;
    AsyncDialog.Done = true;
    this.LockControls(false);
  }

  private void InitComponent()
  {
    this.cboServer.Items.Clear();
    for (int index = 0; index < CloudExportCommand.CloudNames.Length; ++index)
      this.cboServer.Items.Add((object) new ComboBoxItem()
      {
        Value = index.ToString(),
        Text = CloudExportCommand.CloudNames[index]
      });
    this.chkSheet.Checked = true;
  }

  public void SendToCloud()
  {
    if (this.ExportModel == null)
    {
      this.Close();
    }
    else
    {
      this.progressBar.Value = 0;
      string str = "";
      if (!this.chkView.Checked && this.chkSheet.Checked)
        str = "01";
      if (this.chkView.Checked && !this.chkSheet.Checked)
        str = "10";
      if (this.chkView.Checked && this.chkSheet.Checked)
        str = "11";
      this.ExportModel(this.ServiceUrl, this.AuthenticationUrl, this.Username, this.Password, str);
      CloudExportForm.CloudId = this.cboServer.SelectedIndex;
      this.progressBar.Value = 100;
    }
  }

  private void btnExport_Click(object sender, EventArgs e)
  {
    AsyncDialog.Done = false;
    this.LockControls(true);
    this.SendToCloud();
  }

  public void LockControls(bool locked) => this.btnExport.Enabled = !locked;

  public void ShowControls()
  {
    this.lblServer.Visible = true;
    this.cboServer.Visible = true;
    this.tableLayoutPanel2.RowStyles[0].Height = 30f;
  }

  public void HideCloudSelection()
  {
    this.lblServer.Visible = false;
    this.cboServer.Visible = false;
    this.tableLayoutPanel2.RowStyles[0].Height = 0.0f;
    this.tableLayoutPanel2.Height -= 30;
  }

  public void HideAuthentication(string token = "")
  {
    if (string.IsNullOrEmpty(token))
      return;
    this.lblUsername.Visible = false;
    this.txtUsername.Visible = false;
    this.lblPassword.Visible = false;
    this.txtPassword.Visible = false;
    this.tableLayoutPanel2.RowStyles[0].Height = 0.0f;
    this.tableLayoutPanel2.RowStyles[1].Height = 0.0f;
    this.tableLayoutPanel2.RowStyles[2].Height = 0.0f;
    this.tableLayoutPanel1.RowStyles[1].Height = 0.0f;
    this.FormBorderStyle = FormBorderStyle.Sizable;
    this.ClientSize = new Size(282, 193);
    this.MinimumSize = new Size(300, 240 /*0xF0*/);
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
  }

  private void cboServer_SelectedIndexChanged(object sender, EventArgs e)
  {
    CloudExportCommand.GetServerUrlsFromCloudId(int.Parse((this.cboServer.SelectedItem as ComboBoxItem).Value));
    this.lnkStatus.Text = this.WebUrl;
  }

  private void lnkStatus_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
  {
    string text = this.lnkStatus.Text;
    if (string.IsNullOrEmpty(text))
      return;
    Process.Start(text);
  }

  private void CloudExportForm_FormClosing(object sender, FormClosingEventArgs e)
  {
    if (!AsyncDialog.Done)
    {
      int num = (int) MessageBox.Show("The export is still in progress, terminate for now...");
    }
    AsyncDialog.Done = true;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.tableLayoutPanel2 = new TableLayoutPanel();
    this.lblServer = new Label();
    this.lblUsername = new Label();
    this.lblPassword = new Label();
    this.cboServer = new ComboBox();
    this.txtUsername = new TextBox();
    this.txtPassword = new TextBox();
    this.tableLayoutPanel1 = new TableLayoutPanel();
    this.lnkStatus = new LinkLabel();
    this.progressBar = new ProgressBar();
    this.lblTitle = new Label();
    this.txtInfo = new TextBox();
    this.tableLayoutPanel3 = new TableLayoutPanel();
    this.btnExport = new Button();
    this.tableLayoutPanel4 = new TableLayoutPanel();
    this.chkView = new CheckBox();
    this.chkSheet = new CheckBox();
    this.tableLayoutPanel2.SuspendLayout();
    this.tableLayoutPanel1.SuspendLayout();
    this.tableLayoutPanel3.SuspendLayout();
    this.tableLayoutPanel4.SuspendLayout();
    this.SuspendLayout();
    this.tableLayoutPanel2.ColumnCount = 2;
    this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
    this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel2.Controls.Add((Control) this.lblServer, 0, 0);
    this.tableLayoutPanel2.Controls.Add((Control) this.lblUsername, 0, 1);
    this.tableLayoutPanel2.Controls.Add((Control) this.lblPassword, 0, 2);
    this.tableLayoutPanel2.Controls.Add((Control) this.cboServer, 1, 0);
    this.tableLayoutPanel2.Controls.Add((Control) this.txtUsername, 1, 1);
    this.tableLayoutPanel2.Controls.Add((Control) this.txtPassword, 1, 2);
    this.tableLayoutPanel2.Dock = DockStyle.Fill;
    this.tableLayoutPanel2.Location = new Point(3, 33);
    this.tableLayoutPanel2.Name = "tableLayoutPanel2";
    this.tableLayoutPanel2.RowCount = 3;
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel2.Size = new Size(276, 89);
    this.tableLayoutPanel2.TabIndex = 0;
    this.lblServer.AutoSize = true;
    this.lblServer.Dock = DockStyle.Left;
    this.lblServer.Location = new Point(3, 0);
    this.lblServer.Name = "lblServer";
    this.lblServer.Size = new Size(50, 30);
    this.lblServer.TabIndex = 0;
    this.lblServer.Text = "Server";
    this.lblUsername.AutoSize = true;
    this.lblUsername.Dock = DockStyle.Left;
    this.lblUsername.Location = new Point(3, 30);
    this.lblUsername.Name = "lblUsername";
    this.lblUsername.Size = new Size(73, 30);
    this.lblUsername.TabIndex = 1;
    this.lblUsername.Text = "Username";
    this.lblPassword.AutoSize = true;
    this.lblPassword.Dock = DockStyle.Left;
    this.lblPassword.Location = new Point(3, 60);
    this.lblPassword.Name = "lblPassword";
    this.lblPassword.Size = new Size(69, 30);
    this.lblPassword.TabIndex = 2;
    this.lblPassword.Text = "Password";
    this.cboServer.Dock = DockStyle.Fill;
    this.cboServer.DropDownStyle = ComboBoxStyle.DropDownList;
    this.cboServer.FormattingEnabled = true;
    this.cboServer.Location = new Point(103, 3);
    this.cboServer.Name = "cboServer";
    this.cboServer.Size = new Size(170, 24);
    this.cboServer.TabIndex = 5;
    this.cboServer.SelectedIndexChanged += new EventHandler(this.cboServer_SelectedIndexChanged);
    this.txtUsername.Dock = DockStyle.Fill;
    this.txtUsername.Location = new Point(103, 33);
    this.txtUsername.Name = "txtUsername";
    this.txtUsername.Size = new Size(170, 22);
    this.txtUsername.TabIndex = 3;
    this.txtPassword.Dock = DockStyle.Fill;
    this.txtPassword.Location = new Point(103, 63 /*0x3F*/);
    this.txtPassword.Name = "txtPassword";
    this.txtPassword.PasswordChar = '*';
    this.txtPassword.Size = new Size(170, 22);
    this.txtPassword.TabIndex = 4;
    this.tableLayoutPanel1.ColumnCount = 1;
    this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel1.Controls.Add((Control) this.lnkStatus, 0, 4);
    this.tableLayoutPanel1.Controls.Add((Control) this.progressBar, 0, 5);
    this.tableLayoutPanel1.Controls.Add((Control) this.lblTitle, 0, 0);
    this.tableLayoutPanel1.Controls.Add((Control) this.tableLayoutPanel2, 0, 1);
    this.tableLayoutPanel1.Controls.Add((Control) this.txtInfo, 0, 2);
    this.tableLayoutPanel1.Controls.Add((Control) this.tableLayoutPanel3, 0, 3);
    this.tableLayoutPanel1.Dock = DockStyle.Fill;
    this.tableLayoutPanel1.Location = new Point(0, 0);
    this.tableLayoutPanel1.Name = "tableLayoutPanel1";
    this.tableLayoutPanel1.RowCount = 6;
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 95f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
    this.tableLayoutPanel1.Size = new Size(282, 323);
    this.tableLayoutPanel1.TabIndex = 1;
    this.lnkStatus.Anchor = AnchorStyles.Left;
    this.lnkStatus.AutoSize = true;
    this.lnkStatus.Location = new Point(3, 289);
    this.lnkStatus.Name = "lnkStatus";
    this.lnkStatus.Size = new Size(0, 17);
    this.lnkStatus.TabIndex = 2;
    this.lnkStatus.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lnkStatus_LinkClicked);
    this.progressBar.Dock = DockStyle.Fill;
    this.progressBar.Location = new Point(3, 316);
    this.progressBar.Name = "progressBar";
    this.progressBar.Size = new Size(276, 4);
    this.progressBar.TabIndex = 4;
    this.lblTitle.Anchor = AnchorStyles.Left;
    this.lblTitle.AutoSize = true;
    this.lblTitle.Location = new Point(3, 6);
    this.lblTitle.Name = "lblTitle";
    this.lblTitle.Size = new Size(213, 17);
    this.lblTitle.TabIndex = 5;
    this.lblTitle.Text = "Export assembly to EDGE^Cloud";
    this.txtInfo.Dock = DockStyle.Fill;
    this.txtInfo.Location = new Point(3, 128 /*0x80*/);
    this.txtInfo.Multiline = true;
    this.txtInfo.Name = "txtInfo";
    this.txtInfo.ReadOnly = true;
    this.txtInfo.ScrollBars = ScrollBars.Vertical;
    this.txtInfo.Size = new Size(276, 102);
    this.txtInfo.TabIndex = 3;
    this.tableLayoutPanel3.ColumnCount = 2;
    this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
    this.tableLayoutPanel3.Controls.Add((Control) this.btnExport, 1, 0);
    this.tableLayoutPanel3.Controls.Add((Control) this.tableLayoutPanel4, 0, 0);
    this.tableLayoutPanel3.Dock = DockStyle.Fill;
    this.tableLayoutPanel3.Location = new Point(0, 233);
    this.tableLayoutPanel3.Margin = new Padding(0);
    this.tableLayoutPanel3.Name = "tableLayoutPanel3";
    this.tableLayoutPanel3.RowCount = 1;
    this.tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel3.Size = new Size(282, 50);
    this.tableLayoutPanel3.TabIndex = 6;
    this.btnExport.Dock = DockStyle.Fill;
    this.btnExport.Location = new Point(205, 3);
    this.btnExport.Name = "btnExport";
    this.btnExport.Size = new Size(74, 44);
    this.btnExport.TabIndex = 1;
    this.btnExport.Text = "Export";
    this.btnExport.UseVisualStyleBackColor = true;
    this.btnExport.Click += new EventHandler(this.btnExport_Click);
    this.tableLayoutPanel4.ColumnCount = 1;
    this.tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel4.Controls.Add((Control) this.chkView, 0, 0);
    this.tableLayoutPanel4.Controls.Add((Control) this.chkSheet, 0, 1);
    this.tableLayoutPanel4.Dock = DockStyle.Fill;
    this.tableLayoutPanel4.Location = new Point(0, 0);
    this.tableLayoutPanel4.Margin = new Padding(0);
    this.tableLayoutPanel4.Name = "tableLayoutPanel4";
    this.tableLayoutPanel4.RowCount = 2;
    this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
    this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
    this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel4.Size = new Size(202, 50);
    this.tableLayoutPanel4.TabIndex = 2;
    this.chkView.AutoSize = true;
    this.chkView.Dock = DockStyle.Fill;
    this.chkView.Location = new Point(3, 3);
    this.chkView.Name = "chkView";
    this.chkView.Size = new Size(196, 19);
    this.chkView.TabIndex = 0;
    this.chkView.Text = "Export views";
    this.chkView.UseVisualStyleBackColor = true;
    this.chkSheet.AutoSize = true;
    this.chkSheet.Dock = DockStyle.Left;
    this.chkSheet.Location = new Point(3, 28);
    this.chkSheet.Name = "chkSheet";
    this.chkSheet.Size = new Size(116, 19);
    this.chkSheet.TabIndex = 1;
    this.chkSheet.Text = "Export sheets";
    this.chkSheet.UseVisualStyleBackColor = true;
    this.AcceptButton = (IButtonControl) this.btnExport;
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(282, 323);
    this.Controls.Add((Control) this.tableLayoutPanel1);
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    this.MinimumSize = new Size(300, 320);
    this.Name = nameof (CloudExportForm);
    this.Text = "EDGE^Cloud";
    this.FormClosing += new FormClosingEventHandler(this.CloudExportForm_FormClosing);
    this.Load += new EventHandler(this.MainForm_Load);
    this.tableLayoutPanel2.ResumeLayout(false);
    this.tableLayoutPanel2.PerformLayout();
    this.tableLayoutPanel1.ResumeLayout(false);
    this.tableLayoutPanel1.PerformLayout();
    this.tableLayoutPanel3.ResumeLayout(false);
    this.tableLayoutPanel4.ResumeLayout(false);
    this.tableLayoutPanel4.PerformLayout();
    this.ResumeLayout(false);
  }
}
