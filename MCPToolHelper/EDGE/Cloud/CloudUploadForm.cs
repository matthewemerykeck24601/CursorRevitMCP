// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.CloudUploadForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.Cloud.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

#nullable disable
namespace EDGE.Cloud;

public class CloudUploadForm : System.Windows.Forms.Form
{
  public Action<string, string, string, string, string> ExportModel;
  public Func<View3D, List<ElementId>, List<EDGE.Cloud.Models.View>, IRestResponse> SaveSheets;
  public AssemblyInfo AssemblyInfo;
  public static int CloudId = 1;
  private string _folderPath = string.Empty;
  private List<string> _fileNames = new List<string>();
  private List<ComboBoxItem> _projectItems = new List<ComboBoxItem>();
  private bool _run;
  private IContainer components;
  private TableLayoutPanel tableLayoutPanel2;
  private Label lblServer;
  private Label lblUsername;
  private Label lblPassword;
  private TextBox txtUsername;
  private TextBox txtPassword;
  private ComboBox cboServer;
  private TableLayoutPanel tableLayoutPanel1;
  private Button btnUpload;
  private LinkLabel lnkStatus;
  private ProgressBar progressBar;
  private Label lblTitle;
  private TableLayoutPanel tableLayoutPanel3;
  private TableLayoutPanel tableLayoutPanel5;
  private Label lblProducer;
  private Label lblPlant;
  private Label lblProject;
  private ComboBox cboProducer;
  private ComboBox cboPlant;
  private ComboBox cboProject;
  private Label lblAssembly;
  private OpenFileDialog openFileDialog;
  private Button btnBrowse;
  private SplitContainer splitContainer1;
  private TableLayoutPanel tableLayoutPanel4;
  private TableLayoutPanel tableLayoutPanel6;
  private TableLayoutPanel tableLayoutPanel7;
  private Button btnLogin;
  private ComboBox cboAssembly;
  private Label lblProjectNumber;
  private ComboBox cboProjectNumber;
  private Button cancel;
  private Button cancel2;

  public Action ExportSheetsToPDF { get; set; }

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

  public string ProducerName => this.cboProducer.Text;

  public string PlantName => this.cboPlant.Text;

  public string ProjectName => this.cboProject.Text;

  public string ProjectNumber => this.cboProjectNumber.Text;

  public string AssemblyName => this.cboAssembly.Text;

  public CloudUploadForm()
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
      this.LoadProducers();
    }
    this.HideAuthentication(this.Token);
    this.cboServer.SelectedIndex = CloudUploadForm.CloudId;
    this.txtUsername.Text = this.Username;
    this.txtPassword.Text = this.Password;
    this.btnUpload.Enabled = false;
    this._fileNames.Clear();
    if (!string.IsNullOrEmpty(this.cboProjectNumber.Text))
    {
      string text = this.cboProjectNumber.Text;
      this.cboPlant_SelectedIndexChanged((object) null, (EventArgs) null);
      this.cboProjectNumber.Text = text;
    }
    if (!string.IsNullOrEmpty(this.cboProject.Text))
    {
      string text = this.cboProject.Text;
      this.cboProjectNumber_SelectedIndexChanged((object) null, (EventArgs) null);
      this.cboProject.Text = text;
    }
    if (!string.IsNullOrEmpty(this.cboAssembly.Text))
    {
      this.cboProject_SelectedIndexChanged((object) null, (EventArgs) null);
      this.cboAssembly.Text = string.Empty;
      this.cboAssembly.SelectedIndex = -1;
    }
    this._run = false;
    AsyncDialog.Done = true;
    this.LockControls(false);
    this.SetControlsByHierarchy();
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
      this.ExportModel(this.ServiceUrl, this.AuthenticationUrl, this.Username, this.Password, "");
      CloudUploadForm.CloudId = this.cboServer.SelectedIndex;
      this.progressBar.Value = 100;
    }
  }

  public void LockControls(bool locked)
  {
    this.cboProducer.Enabled = !locked;
    this.cboPlant.Enabled = !locked;
    this.cboProjectNumber.Enabled = !locked;
    this.cboProject.Enabled = !locked;
    this.cboAssembly.Enabled = !locked;
    this.btnBrowse.Enabled = !locked;
    this.btnUpload.Enabled = !locked;
    this.cancel.Enabled = !locked;
    this.cancel2.Enabled = !locked;
  }

  public void SetControlsByHierarchy()
  {
    this.cboProducer.Enabled = true;
    this.cboPlant.Enabled = !string.IsNullOrEmpty(this.cboProducer.Text) && !this.cboProducer.Text.Contains("No producer found");
    this.cboProjectNumber.Enabled = !string.IsNullOrEmpty(this.cboPlant.Text) && !this.cboPlant.Text.Contains("No plant found");
    this.cboProject.Enabled = !string.IsNullOrEmpty(this.cboProjectNumber.Text);
    this.cboAssembly.Enabled = !string.IsNullOrEmpty(this.cboProject.Text) && !string.IsNullOrEmpty(this.cboProjectNumber.Text);
    this.btnUpload.Enabled = this._fileNames.Count > 0 && !string.IsNullOrEmpty(this.cboProducer.Text) && !string.IsNullOrEmpty(this.cboPlant.Text) && !string.IsNullOrEmpty(this.cboProjectNumber.Text) && !string.IsNullOrEmpty(this.cboProject.Text) && !string.IsNullOrEmpty(this.cboAssembly.Text);
  }

  private void btnLogin_Click(object sender, EventArgs e)
  {
    this.Token = UploadAssembly.GetToken(this.AuthenticationUrl, this.Username, this.Password);
    if (string.IsNullOrEmpty(this.Token))
    {
      int num = (int) MessageBox.Show("Username and Password are not valid. Please try again.", "EDGE^Cloud");
    }
    this.HideAuthentication(this.Token);
    CloudExportCommand.Token = this.Token;
    if (!string.IsNullOrEmpty(this.Token))
    {
      CloudExportCommand.Username = this.Username;
      CloudExportCommand.Password = this.Password;
    }
    this.LoadProducers();
  }

  private void btnBrowse_Click(object sender, EventArgs e)
  {
    OpenFileDialog openFileDialog1 = new OpenFileDialog();
    openFileDialog1.Multiselect = true;
    openFileDialog1.Filter = "DWF files (*.dwf, *.dwfx)|*.dwf; *.dwfx|AutoCAD files (*.dwg)|*.dwg";
    openFileDialog1.FilterIndex = 0;
    OpenFileDialog openFileDialog2 = openFileDialog1;
    if (openFileDialog2.ShowDialog() == DialogResult.OK)
    {
      this._fileNames.Clear();
      string[] fileNames = openFileDialog2.FileNames;
      for (int index = 0; index < fileNames.Length; ++index)
      {
        this._folderPath = Path.GetDirectoryName(fileNames[index]);
        this._fileNames.Add(Path.GetFileName(fileNames[index]));
      }
    }
    this.SetControlsByHierarchy();
  }

  private void btnUpload_Click(object sender, EventArgs e)
  {
    bool flag = false;
    if (string.IsNullOrEmpty(this.cboProducer.Text))
    {
      flag = true;
      int num = (int) MessageBox.Show("Please select producer from the list.", "EDGE^Cloud");
    }
    else if (string.IsNullOrEmpty(this.cboPlant.Text))
    {
      flag = true;
      int num = (int) MessageBox.Show("Please select plant from the list.", "EDGE^Cloud");
    }
    else if (string.IsNullOrEmpty(this.cboProject.Text))
    {
      flag = true;
      int num = (int) MessageBox.Show("Please select project from the list, or type a new project name.", "EDGE^Cloud");
    }
    else if (string.IsNullOrEmpty(this.cboAssembly.Text))
    {
      flag = true;
      int num = (int) MessageBox.Show("Please select assembly from the list, or type a new assembly name.", "EDGE^Cloud");
    }
    if (this._projectItems.Where<ComboBoxItem>((Func<ComboBoxItem, bool>) (pI => pI.Text2 == this.ProjectNumber && pI.Text1 == this.ProjectName)).Count<ComboBoxItem>() == 0)
    {
      switch (MessageBox.Show("This will create a new project, are you sure you wish to continue?", "EDGE^Cloud Uploader", MessageBoxButtons.OKCancel))
      {
        case DialogResult.None:
        case DialogResult.Cancel:
        case DialogResult.Abort:
          flag = true;
          break;
      }
    }
    if (flag || this._run)
      return;
    if (UploadAssembly.UploadAssemblyPermission(this.ServiceUrl, this.Token, this.ProducerName, this.PlantName, this.ProjectName, this.ProjectNumber))
    {
      this.LockControls(true);
      AsyncDialog asyncDialog = new AsyncDialog(1);
      ForgeExporter forgeExporter = new ForgeExporter(this.ServiceUrl)
      {
        AsyncDialog = asyncDialog
      };
      List<EDGE.Cloud.Models.View> views = new List<EDGE.Cloud.Models.View>();
      foreach (string fileName in this._fileNames)
        views.Add(new EDGE.Cloud.Models.View() { FileName = fileName });
      forgeExporter.UploadViews(this._folderPath, views, (View3D) null, (List<ElementId>) null, this.SaveSheets);
      this._run = true;
    }
    else
    {
      int num1 = (int) MessageBox.Show("This user does not have enough permissions to upload.", "EDGE^Cloud");
    }
  }

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
  }

  public void HideAuthentication(string token = "")
  {
    if (!string.IsNullOrEmpty(token))
    {
      this.splitContainer1.Panel1Collapsed = true;
      this.splitContainer1.Panel2Collapsed = false;
      this.FormBorderStyle = FormBorderStyle.Sizable;
      this.ClientSize = new Size(282, 293);
      this.MinimumSize = new Size(300, 340);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
    }
    else
    {
      this.splitContainer1.Panel1Collapsed = false;
      this.splitContainer1.Panel2Collapsed = true;
      this.FormBorderStyle = FormBorderStyle.Sizable;
      this.ClientSize = new Size(282, 293);
      this.MinimumSize = new Size(300, 340);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
    }
  }

  private void LoadProducers()
  {
    if (string.IsNullOrEmpty(this.Token) || this.cboProducer.Items.Count != 0)
      return;
    List<Manufacture> producers = UploadAssembly.GetProducers(this.ServiceUrl, this.Token);
    if (producers == null)
      return;
    if (producers.Count == 0)
    {
      this.cboProducer.Enabled = false;
      this.cboProducer.Text = "No producer found";
    }
    else
    {
      this.cboProducer.Enabled = true;
      this.cboProducer.Items.Clear();
      foreach (Manufacture manufacture in producers)
        this.cboProducer.Items.Add((object) new ComboBoxItem()
        {
          Value = manufacture.Id.ToString(),
          Text = manufacture.Name
        });
    }
  }

  private void cboServer_SelectedIndexChanged(object sender, EventArgs e)
  {
    CloudExportCommand.GetServerUrlsFromCloudId(int.Parse((this.cboServer.SelectedItem as ComboBoxItem).Value));
    this.lnkStatus.Text = this.WebUrl;
  }

  private void cboProducer_SelectedIndexChanged(object sender, EventArgs e)
  {
    int producerId = -1;
    if (this.cboProducer.SelectedItem != null)
      producerId = int.Parse(((ComboBoxItem) this.cboProducer.SelectedItem).Value);
    List<Plant> plants = UploadAssembly.GetPlants(this.ServiceUrl, this.Token, producerId);
    if (plants == null || plants.Count == 0)
    {
      this.cboPlant.Enabled = false;
      this.cboPlant.Text = "No plant found";
    }
    else
    {
      this.cboPlant.Items.Clear();
      foreach (Plant plant in plants)
        this.cboPlant.Items.Add((object) new ComboBoxItem()
        {
          Value = plant.Id.ToString(),
          Text = plant.Name
        });
      this.cboPlant.Text = "";
      this.cboProjectNumber.Items.Clear();
      this.cboProjectNumber.Text = "";
      this.cboProject.Items.Clear();
      this.cboProject.Text = "";
      this.cboAssembly.Items.Clear();
      this.cboAssembly.Text = "";
    }
    this.SetControlsByHierarchy();
  }

  private void cboPlant_SelectedIndexChanged(object sender, EventArgs e)
  {
    int result1 = -1;
    this.cboProjectNumber.Items.Clear();
    if (this.cboProducer.SelectedItem != null)
      int.TryParse(((ComboBoxItem) this.cboProducer.SelectedItem).Value, out result1);
    int result2 = -1;
    if (this.cboPlant.SelectedItem != null)
      int.TryParse(((ComboBoxItem) this.cboPlant.SelectedItem).Value, out result2);
    List<Project> projects = UploadAssembly.GetProjects(this.ServiceUrl, this.Token, result1, result2);
    if (projects != null && projects.Count != 0)
    {
      List<Project> list = projects.OrderBy<Project, string>((Func<Project, string>) (p => p.Name)).ToList<Project>();
      IEnumerable<string> source = (IEnumerable<string>) list.Where<Project>((Func<Project, bool>) (p => !string.IsNullOrEmpty(p.Number))).Select<Project, string>((Func<Project, string>) (p => p.Number)).Distinct<string>().OrderBy<string, string>((Func<string, string>) (x => x));
      foreach (string str in source)
        this.cboProjectNumber.Items.Add((object) new ComboBoxItem()
        {
          Value = str,
          Text = str
        });
      if (!source.Contains<string>(this.ProjectNumber))
        this.cboProjectNumber.Text = "";
      this._projectItems.Clear();
      foreach (Project project in list)
      {
        if (!string.IsNullOrEmpty(project.Number))
        {
          string str1 = $"{project.Number} - {project.Name}";
        }
        else
        {
          string name = project.Name;
        }
        string projectName = project.Name;
        int projectType = (int) project.ProjectType;
        if (projectType == 175)
        {
          projectName = Project.GetProjectName(projectName);
          string str2 = " - CAD";
          if (projectName.EndsWith(str2))
            projectName = projectName.Substring(0, projectName.Length - str2.Length);
        }
        if (projectType != 174)
          this._projectItems.Add(new ComboBoxItem()
          {
            Value = project.Id.ToString(),
            Text = projectName,
            Text1 = projectName,
            Text2 = project.Number
          });
      }
      if (this._projectItems.Where<ComboBoxItem>((Func<ComboBoxItem, bool>) (pI => pI.Text == this.ProjectName && pI.Text2 == this.ProjectNumber)).Count<ComboBoxItem>() == 0)
      {
        this.cboProject.Items.Clear();
        this.cboProject.Text = "";
      }
      this.cboAssembly.Items.Clear();
      this.cboAssembly.Text = "";
    }
    this.SetControlsByHierarchy();
  }

  private void cboProject_SelectedIndexChanged(object sender, EventArgs e)
  {
    int result1 = -1;
    if (this.cboProducer.SelectedItem != null)
      int.TryParse(((ComboBoxItem) this.cboProducer.SelectedItem).Value, out result1);
    int result2 = -1;
    if (this.cboPlant.SelectedItem != null)
      int.TryParse(((ComboBoxItem) this.cboPlant.SelectedItem).Value, out result2);
    int result3 = -1;
    if (this.cboProject.SelectedItem != null)
      int.TryParse(((ComboBoxItem) this.cboProject.SelectedItem).Value, out result3);
    List<Assembly> assemblies = UploadAssembly.GetAssemblies(this.ServiceUrl, this.Token, result1, result2, result3);
    this.cboAssembly.Items.Clear();
    if (assemblies != null)
    {
      foreach (Assembly assembly in assemblies)
        this.cboAssembly.Items.Add((object) new ComboBoxItem()
        {
          Value = assembly.Id.ToString(),
          Text = assembly.Name
        });
    }
    this.cboAssembly.Text = "";
    if (this.cboProject.SelectedItem != null)
    {
      ComboBoxItem selectedItem = (ComboBoxItem) this.cboProject.SelectedItem;
    }
    this.SetControlsByHierarchy();
  }

  private void cboProject_TextChanged(object sender, EventArgs e)
  {
    this.cboAssembly.Items.Clear();
    this.cboAssembly.Enabled = !string.IsNullOrEmpty(this.cboProject.Text);
    this.SetControlsByHierarchy();
  }

  private void cboProjectNumber_SelectedIndexChanged(object sender, EventArgs e)
  {
    string projectNumber = string.Empty;
    if (this.cboProjectNumber.SelectedItem != null)
    {
      projectNumber = ((ComboBoxItem) this.cboProjectNumber.SelectedItem).Value;
      IEnumerable<ComboBoxItem> source = this._projectItems.Where<ComboBoxItem>((Func<ComboBoxItem, bool>) (p => p.Text2 == projectNumber));
      if (source.Count<ComboBoxItem>() > 0)
      {
        if (string.IsNullOrEmpty(this.cboProject.Text) || source.Where<ComboBoxItem>((Func<ComboBoxItem, bool>) (cbi => cbi.Text1 == this.cboProject.Text)).Count<ComboBoxItem>() == 0)
          this.cboProject.Text = "";
        this.cboProject.Items.Clear();
        this.cboProject.Items.AddRange((object[]) source.ToArray<ComboBoxItem>());
      }
    }
    this.SetControlsByHierarchy();
  }

  private void cboProjectNumber_TextChanged(object sender, EventArgs e)
  {
    this.cboProject.Items.Clear();
    this.cboProject.Enabled = !string.IsNullOrEmpty(this.cboProjectNumber.Text);
    this.SetControlsByHierarchy();
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

  private void btnExport_Click(object sender, EventArgs e) => this.ExportSheetsToPDF();

  private void cboAssembly_TextChanged(object sender, EventArgs e) => this.SetControlsByHierarchy();

  private void cboAssembly_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.SetControlsByHierarchy();
  }

  private void cancel_Click(object sender, EventArgs e) => this.Close();

  private void cancel2_Click(object sender, EventArgs e) => this.Close();

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
    this.splitContainer1 = new SplitContainer();
    this.tableLayoutPanel4 = new TableLayoutPanel();
    this.tableLayoutPanel7 = new TableLayoutPanel();
    this.cancel = new Button();
    this.btnLogin = new Button();
    this.tableLayoutPanel6 = new TableLayoutPanel();
    this.tableLayoutPanel3 = new TableLayoutPanel();
    this.btnUpload = new Button();
    this.btnBrowse = new Button();
    this.tableLayoutPanel5 = new TableLayoutPanel();
    this.lblProducer = new Label();
    this.lblPlant = new Label();
    this.cboProducer = new ComboBox();
    this.cboPlant = new ComboBox();
    this.lblAssembly = new Label();
    this.cboAssembly = new ComboBox();
    this.lblProject = new Label();
    this.cboProject = new ComboBox();
    this.lblProjectNumber = new Label();
    this.cboProjectNumber = new ComboBox();
    this.lnkStatus = new LinkLabel();
    this.progressBar = new ProgressBar();
    this.lblTitle = new Label();
    this.openFileDialog = new OpenFileDialog();
    this.cancel2 = new Button();
    this.tableLayoutPanel2.SuspendLayout();
    this.tableLayoutPanel1.SuspendLayout();
    this.splitContainer1.BeginInit();
    this.splitContainer1.Panel1.SuspendLayout();
    this.splitContainer1.Panel2.SuspendLayout();
    this.splitContainer1.SuspendLayout();
    this.tableLayoutPanel4.SuspendLayout();
    this.tableLayoutPanel7.SuspendLayout();
    this.tableLayoutPanel6.SuspendLayout();
    this.tableLayoutPanel3.SuspendLayout();
    this.tableLayoutPanel5.SuspendLayout();
    this.SuspendLayout();
    this.tableLayoutPanel2.ColumnCount = 2;
    this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
    this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel2.Controls.Add((System.Windows.Forms.Control) this.lblServer, 0, 0);
    this.tableLayoutPanel2.Controls.Add((System.Windows.Forms.Control) this.lblUsername, 0, 1);
    this.tableLayoutPanel2.Controls.Add((System.Windows.Forms.Control) this.lblPassword, 0, 2);
    this.tableLayoutPanel2.Controls.Add((System.Windows.Forms.Control) this.cboServer, 1, 0);
    this.tableLayoutPanel2.Controls.Add((System.Windows.Forms.Control) this.txtUsername, 1, 1);
    this.tableLayoutPanel2.Controls.Add((System.Windows.Forms.Control) this.txtPassword, 1, 2);
    this.tableLayoutPanel2.Dock = DockStyle.Top;
    this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
    this.tableLayoutPanel2.Name = "tableLayoutPanel2";
    this.tableLayoutPanel2.RowCount = 3;
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel2.Size = new Size(288, 89);
    this.tableLayoutPanel2.TabIndex = 0;
    this.lblServer.AutoSize = true;
    this.lblServer.Dock = DockStyle.Left;
    this.lblServer.Location = new System.Drawing.Point(3, 0);
    this.lblServer.Name = "lblServer";
    this.lblServer.Size = new Size(50, 30);
    this.lblServer.TabIndex = 0;
    this.lblServer.Text = "Server";
    this.lblServer.TextAlign = ContentAlignment.MiddleLeft;
    this.lblUsername.AutoSize = true;
    this.lblUsername.Dock = DockStyle.Left;
    this.lblUsername.Location = new System.Drawing.Point(3, 30);
    this.lblUsername.Name = "lblUsername";
    this.lblUsername.Size = new Size(73, 30);
    this.lblUsername.TabIndex = 1;
    this.lblUsername.Text = "Username";
    this.lblUsername.TextAlign = ContentAlignment.MiddleLeft;
    this.lblPassword.AutoSize = true;
    this.lblPassword.Dock = DockStyle.Left;
    this.lblPassword.Location = new System.Drawing.Point(3, 60);
    this.lblPassword.Name = "lblPassword";
    this.lblPassword.Size = new Size(69, 30);
    this.lblPassword.TabIndex = 2;
    this.lblPassword.Text = "Password";
    this.lblPassword.TextAlign = ContentAlignment.MiddleLeft;
    this.cboServer.Dock = DockStyle.Fill;
    this.cboServer.DropDownStyle = ComboBoxStyle.DropDownList;
    this.cboServer.FormattingEnabled = true;
    this.cboServer.Location = new System.Drawing.Point(103, 3);
    this.cboServer.Name = "cboServer";
    this.cboServer.Size = new Size(182, 24);
    this.cboServer.TabIndex = 5;
    this.cboServer.SelectedIndexChanged += new EventHandler(this.cboServer_SelectedIndexChanged);
    this.txtUsername.Dock = DockStyle.Fill;
    this.txtUsername.Location = new System.Drawing.Point(103, 33);
    this.txtUsername.Name = "txtUsername";
    this.txtUsername.Size = new Size(182, 22);
    this.txtUsername.TabIndex = 3;
    this.txtPassword.Dock = DockStyle.Fill;
    this.txtPassword.Location = new System.Drawing.Point(103, 63 /*0x3F*/);
    this.txtPassword.Name = "txtPassword";
    this.txtPassword.PasswordChar = '*';
    this.txtPassword.Size = new Size(182, 22);
    this.txtPassword.TabIndex = 4;
    this.tableLayoutPanel1.ColumnCount = 1;
    this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel1.Controls.Add((System.Windows.Forms.Control) this.splitContainer1, 0, 1);
    this.tableLayoutPanel1.Controls.Add((System.Windows.Forms.Control) this.lnkStatus, 0, 2);
    this.tableLayoutPanel1.Controls.Add((System.Windows.Forms.Control) this.progressBar, 0, 3);
    this.tableLayoutPanel1.Controls.Add((System.Windows.Forms.Control) this.lblTitle, 0, 0);
    this.tableLayoutPanel1.Dock = DockStyle.Fill;
    this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
    this.tableLayoutPanel1.Name = "tableLayoutPanel1";
    this.tableLayoutPanel1.RowCount = 4;
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel1.Size = new Size(294, 440);
    this.tableLayoutPanel1.TabIndex = 1;
    this.splitContainer1.Dock = DockStyle.Fill;
    this.splitContainer1.Location = new System.Drawing.Point(0, 30);
    this.splitContainer1.Margin = new Padding(0);
    this.splitContainer1.Name = "splitContainer1";
    this.splitContainer1.Orientation = Orientation.Horizontal;
    this.splitContainer1.Panel1.Controls.Add((System.Windows.Forms.Control) this.tableLayoutPanel4);
    this.splitContainer1.Panel2.Controls.Add((System.Windows.Forms.Control) this.tableLayoutPanel6);
    this.splitContainer1.Size = new Size(294, 370);
    this.splitContainer1.SplitterDistance = 165;
    this.splitContainer1.TabIndex = 2;
    this.tableLayoutPanel4.ColumnCount = 1;
    this.tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel4.Controls.Add((System.Windows.Forms.Control) this.tableLayoutPanel7, 0, 1);
    this.tableLayoutPanel4.Controls.Add((System.Windows.Forms.Control) this.tableLayoutPanel2, 0, 0);
    this.tableLayoutPanel4.Dock = DockStyle.Fill;
    this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
    this.tableLayoutPanel4.Name = "tableLayoutPanel4";
    this.tableLayoutPanel4.RowCount = 2;
    this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
    this.tableLayoutPanel4.Size = new Size(294, 165);
    this.tableLayoutPanel4.TabIndex = 0;
    this.tableLayoutPanel7.ColumnCount = 3;
    this.tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82f));
    this.tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82f));
    this.tableLayoutPanel7.Controls.Add((System.Windows.Forms.Control) this.cancel, 0, 0);
    this.tableLayoutPanel7.Controls.Add((System.Windows.Forms.Control) this.btnLogin, 2, 0);
    this.tableLayoutPanel7.Dock = DockStyle.Fill;
    this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 118);
    this.tableLayoutPanel7.Name = "tableLayoutPanel7";
    this.tableLayoutPanel7.RowCount = 1;
    this.tableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel7.Size = new Size(288, 44);
    this.tableLayoutPanel7.TabIndex = 0;
    this.cancel.Dock = DockStyle.Fill;
    this.cancel.Location = new System.Drawing.Point(3, 3);
    this.cancel.Name = "cancel";
    this.cancel.Size = new Size(76, 38);
    this.cancel.TabIndex = 1;
    this.cancel.Text = "Cancel";
    this.cancel.UseVisualStyleBackColor = true;
    this.cancel.Click += new EventHandler(this.cancel_Click);
    this.btnLogin.Dock = DockStyle.Fill;
    this.btnLogin.Location = new System.Drawing.Point(209, 3);
    this.btnLogin.Name = "btnLogin";
    this.btnLogin.Size = new Size(76, 38);
    this.btnLogin.TabIndex = 0;
    this.btnLogin.Text = "Login";
    this.btnLogin.UseVisualStyleBackColor = true;
    this.btnLogin.Click += new EventHandler(this.btnLogin_Click);
    this.tableLayoutPanel6.ColumnCount = 1;
    this.tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel6.Controls.Add((System.Windows.Forms.Control) this.tableLayoutPanel3, 0, 1);
    this.tableLayoutPanel6.Controls.Add((System.Windows.Forms.Control) this.tableLayoutPanel5, 0, 0);
    this.tableLayoutPanel6.Dock = DockStyle.Fill;
    this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 0);
    this.tableLayoutPanel6.Name = "tableLayoutPanel6";
    this.tableLayoutPanel6.RowCount = 2;
    this.tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
    this.tableLayoutPanel6.Size = new Size(294, 201);
    this.tableLayoutPanel6.TabIndex = 0;
    this.tableLayoutPanel3.ColumnCount = 4;
    this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
    this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
    this.tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
    this.tableLayoutPanel3.Controls.Add((System.Windows.Forms.Control) this.cancel2, 0, 0);
    this.tableLayoutPanel3.Controls.Add((System.Windows.Forms.Control) this.btnUpload, 3, 0);
    this.tableLayoutPanel3.Controls.Add((System.Windows.Forms.Control) this.btnBrowse, 2, 0);
    this.tableLayoutPanel3.Dock = DockStyle.Fill;
    this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 151);
    this.tableLayoutPanel3.Margin = new Padding(0);
    this.tableLayoutPanel3.Name = "tableLayoutPanel3";
    this.tableLayoutPanel3.RowCount = 1;
    this.tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel3.Size = new Size(294, 50);
    this.tableLayoutPanel3.TabIndex = 6;
    this.btnUpload.Dock = DockStyle.Fill;
    this.btnUpload.Enabled = false;
    this.btnUpload.Location = new System.Drawing.Point(217, 3);
    this.btnUpload.Name = "btnUpload";
    this.btnUpload.Size = new Size(74, 44);
    this.btnUpload.TabIndex = 1;
    this.btnUpload.Text = "Upload";
    this.btnUpload.UseVisualStyleBackColor = true;
    this.btnUpload.Click += new EventHandler(this.btnUpload_Click);
    this.btnBrowse.Dock = DockStyle.Fill;
    this.btnBrowse.Location = new System.Drawing.Point(137, 3);
    this.btnBrowse.Name = "btnBrowse";
    this.btnBrowse.Size = new Size(74, 44);
    this.btnBrowse.TabIndex = 2;
    this.btnBrowse.Text = "Browse";
    this.btnBrowse.UseVisualStyleBackColor = true;
    this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
    this.tableLayoutPanel5.ColumnCount = 2;
    this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
    this.tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.lblProducer, 0, 0);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.lblPlant, 0, 1);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.cboProducer, 1, 0);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.cboPlant, 1, 1);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.lblAssembly, 0, 4);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.cboAssembly, 1, 4);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.lblProject, 0, 3);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.cboProject, 1, 3);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.lblProjectNumber, 0, 2);
    this.tableLayoutPanel5.Controls.Add((System.Windows.Forms.Control) this.cboProjectNumber, 1, 2);
    this.tableLayoutPanel5.Dock = DockStyle.Fill;
    this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
    this.tableLayoutPanel5.Name = "tableLayoutPanel5";
    this.tableLayoutPanel5.RowCount = 5;
    this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
    this.tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 20f));
    this.tableLayoutPanel5.Size = new Size(288, 145);
    this.tableLayoutPanel5.TabIndex = 7;
    this.lblProducer.AutoSize = true;
    this.lblProducer.Dock = DockStyle.Left;
    this.lblProducer.Location = new System.Drawing.Point(3, 0);
    this.lblProducer.Name = "lblProducer";
    this.lblProducer.Size = new Size(66, 30);
    this.lblProducer.TabIndex = 0;
    this.lblProducer.Text = "Producer";
    this.lblProducer.TextAlign = ContentAlignment.MiddleLeft;
    this.lblPlant.AutoSize = true;
    this.lblPlant.Dock = DockStyle.Left;
    this.lblPlant.Location = new System.Drawing.Point(3, 30);
    this.lblPlant.Name = "lblPlant";
    this.lblPlant.Size = new Size(40, 30);
    this.lblPlant.TabIndex = 1;
    this.lblPlant.Text = "Plant";
    this.lblPlant.TextAlign = ContentAlignment.MiddleLeft;
    this.cboProducer.Dock = DockStyle.Fill;
    this.cboProducer.DropDownStyle = ComboBoxStyle.DropDownList;
    this.cboProducer.Enabled = false;
    this.cboProducer.FormattingEnabled = true;
    this.cboProducer.Location = new System.Drawing.Point(103, 3);
    this.cboProducer.Name = "cboProducer";
    this.cboProducer.Size = new Size(182, 24);
    this.cboProducer.TabIndex = 3;
    this.cboProducer.SelectedIndexChanged += new EventHandler(this.cboProducer_SelectedIndexChanged);
    this.cboPlant.Dock = DockStyle.Fill;
    this.cboPlant.DropDownStyle = ComboBoxStyle.DropDownList;
    this.cboPlant.Enabled = false;
    this.cboPlant.FormattingEnabled = true;
    this.cboPlant.Location = new System.Drawing.Point(103, 33);
    this.cboPlant.Name = "cboPlant";
    this.cboPlant.Size = new Size(182, 24);
    this.cboPlant.TabIndex = 4;
    this.cboPlant.SelectedIndexChanged += new EventHandler(this.cboPlant_SelectedIndexChanged);
    this.lblAssembly.AutoSize = true;
    this.lblAssembly.Dock = DockStyle.Left;
    this.lblAssembly.Location = new System.Drawing.Point(3, 120);
    this.lblAssembly.Name = "lblAssembly";
    this.lblAssembly.Size = new Size(68, 30);
    this.lblAssembly.TabIndex = 6;
    this.lblAssembly.Text = "Assembly";
    this.lblAssembly.TextAlign = ContentAlignment.MiddleLeft;
    this.cboAssembly.Dock = DockStyle.Fill;
    this.cboAssembly.Enabled = false;
    this.cboAssembly.FormattingEnabled = true;
    this.cboAssembly.Location = new System.Drawing.Point(103, 123);
    this.cboAssembly.Name = "cboAssembly";
    this.cboAssembly.Size = new Size(182, 24);
    this.cboAssembly.TabIndex = 7;
    this.cboAssembly.SelectedIndexChanged += new EventHandler(this.cboAssembly_SelectedIndexChanged);
    this.cboAssembly.TextChanged += new EventHandler(this.cboAssembly_TextChanged);
    this.lblProject.AutoSize = true;
    this.lblProject.Dock = DockStyle.Left;
    this.lblProject.Location = new System.Drawing.Point(3, 90);
    this.lblProject.Name = "lblProject";
    this.lblProject.Size = new Size(93, 30);
    this.lblProject.TabIndex = 2;
    this.lblProject.Text = "Project Name";
    this.lblProject.TextAlign = ContentAlignment.MiddleLeft;
    this.cboProject.Dock = DockStyle.Fill;
    this.cboProject.Enabled = false;
    this.cboProject.FormattingEnabled = true;
    this.cboProject.Location = new System.Drawing.Point(103, 93);
    this.cboProject.Name = "cboProject";
    this.cboProject.Size = new Size(182, 24);
    this.cboProject.TabIndex = 5;
    this.cboProject.SelectedIndexChanged += new EventHandler(this.cboProject_SelectedIndexChanged);
    this.cboProject.TextChanged += new EventHandler(this.cboProject_TextChanged);
    this.lblProjectNumber.AutoSize = true;
    this.lblProjectNumber.Dock = DockStyle.Left;
    this.lblProjectNumber.Location = new System.Drawing.Point(3, 60);
    this.lblProjectNumber.Name = "lblProjectNumber";
    this.lblProjectNumber.Size = new Size(64 /*0x40*/, 30);
    this.lblProjectNumber.TabIndex = 8;
    this.lblProjectNumber.Text = "Project #";
    this.lblProjectNumber.TextAlign = ContentAlignment.MiddleLeft;
    this.cboProjectNumber.Dock = DockStyle.Fill;
    this.cboProjectNumber.FormattingEnabled = true;
    this.cboProjectNumber.Location = new System.Drawing.Point(103, 63 /*0x3F*/);
    this.cboProjectNumber.Name = "cboProjectNumber";
    this.cboProjectNumber.Size = new Size(182, 24);
    this.cboProjectNumber.TabIndex = 9;
    this.cboProjectNumber.SelectedIndexChanged += new EventHandler(this.cboProjectNumber_SelectedIndexChanged);
    this.cboProjectNumber.TextChanged += new EventHandler(this.cboProjectNumber_TextChanged);
    this.lnkStatus.Anchor = AnchorStyles.Left;
    this.lnkStatus.AutoSize = true;
    this.lnkStatus.Location = new System.Drawing.Point(3, 406);
    this.lnkStatus.Name = "lnkStatus";
    this.lnkStatus.Size = new Size(0, 17);
    this.lnkStatus.TabIndex = 2;
    this.lnkStatus.LinkClicked += new LinkLabelLinkClickedEventHandler(this.lnkStatus_LinkClicked);
    this.progressBar.Dock = DockStyle.Fill;
    this.progressBar.Location = new System.Drawing.Point(3, 433);
    this.progressBar.Name = "progressBar";
    this.progressBar.Size = new Size(288, 4);
    this.progressBar.TabIndex = 4;
    this.lblTitle.Anchor = AnchorStyles.Left;
    this.lblTitle.AutoSize = true;
    this.lblTitle.Location = new System.Drawing.Point(3, 6);
    this.lblTitle.Name = "lblTitle";
    this.lblTitle.Size = new Size(184, 17);
    this.lblTitle.TabIndex = 5;
    this.lblTitle.Text = "Upload files to EDGE^Cloud";
    this.openFileDialog.FileName = "openFileDialog";
    this.cancel2.Dock = DockStyle.Fill;
    this.cancel2.Location = new System.Drawing.Point(3, 3);
    this.cancel2.Name = "cancel2";
    this.cancel2.Size = new Size(74, 44);
    this.cancel2.TabIndex = 3;
    this.cancel2.Text = "Cancel";
    this.cancel2.UseVisualStyleBackColor = true;
    this.cancel2.Click += new EventHandler(this.cancel2_Click);
    this.AcceptButton = (IButtonControl) this.btnLogin;
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(294, 440);
    this.ControlBox = false;
    this.Controls.Add((System.Windows.Forms.Control) this.tableLayoutPanel1);
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    this.MinimumSize = new Size(300, 320);
    this.Name = nameof (CloudUploadForm);
    this.StartPosition = FormStartPosition.CenterParent;
    this.Text = "EDGE^Cloud";
    this.FormClosing += new FormClosingEventHandler(this.CloudExportForm_FormClosing);
    this.Load += new EventHandler(this.MainForm_Load);
    this.tableLayoutPanel2.ResumeLayout(false);
    this.tableLayoutPanel2.PerformLayout();
    this.tableLayoutPanel1.ResumeLayout(false);
    this.tableLayoutPanel1.PerformLayout();
    this.splitContainer1.Panel1.ResumeLayout(false);
    this.splitContainer1.Panel2.ResumeLayout(false);
    this.splitContainer1.EndInit();
    this.splitContainer1.ResumeLayout(false);
    this.tableLayoutPanel4.ResumeLayout(false);
    this.tableLayoutPanel7.ResumeLayout(false);
    this.tableLayoutPanel6.ResumeLayout(false);
    this.tableLayoutPanel3.ResumeLayout(false);
    this.tableLayoutPanel5.ResumeLayout(false);
    this.tableLayoutPanel5.PerformLayout();
    this.ResumeLayout(false);
  }
}
