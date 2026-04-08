// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingSettingsForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

public class ModelLockingSettingsForm : System.Windows.Forms.Form
{
  public List<string> Permissions_AllAllowed;
  public List<string> Permissions_GeometryAllowed;
  public List<string> Permissions_ConnectionHardwareAllowed;
  public List<string> Permissions_RebarHandlingAllowed;
  public List<string> Permissions_AssembliesAllowed;
  private Document doc;
  private const char delimiter = ';';
  private IContainer components;
  private Button btnOK;
  private Label label1;
  private TextBox txtPermAll;
  private Label label2;
  private TextBox txtPermGeom;
  private Label label3;
  private TextBox txtPermConn;
  private Label label4;
  private TextBox txtPermRebar;
  private Button btnCancel;
  private Label label5;
  private Label label6;
  private Label label7;
  private TextBox txtAssemblies;
  private TextBox txtProdAdmin;
  private Label label8;
  private Label AddUser_Divider;
  private Label AddUser_Title;
  private Label AddUser_UsernameLbl;
  private TextBox AddUser_UsernameTXT;
  private CheckBox AddUser_ChkBoxAdmin;
  private CheckBox AddUser_ChkBoxGeometry;
  private CheckBox AddUser_ChkBoxConnx;
  private CheckBox AddUser_ChkBoxRebar;
  private CheckBox AddUser_ChkBoxAssemblies;
  private Button AddUser_Button;
  private Button AddUser_ManageAdmins;
  private Label Lbl_CurrUser;

  public ModelLockingSettingsForm(Document doc = null)
  {
    this.doc = doc;
    this.InitializeComponent();
    this.txtProdAdmin.Text = this.ConvertUserListToString(App.ModelLockingAdminList);
    this.txtProdAdmin.Enabled = false;
    this.txtPermAll.Enabled = false;
    this.txtPermGeom.Enabled = false;
    this.txtPermConn.Enabled = false;
    this.txtPermRebar.Enabled = false;
    this.txtAssemblies.Enabled = false;
    this.btnOK.Enabled = false;
    this.Lbl_CurrUser.Text = "Current User: " + doc.Application.Username.ToUpper();
  }

  private string ConvertUserListToString(List<string> users)
  {
    string str = "";
    foreach (string user in users)
      str = $"{str}{user};";
    return str;
  }

  private void EnableForAdmins()
  {
    if (this.doc == null)
      return;
    if (ModelLockingUtils.ModelLockingEnabled(this.doc) && ((ModelLockingPermissionsSchema.GetModelPermissionsForUser(this.doc.Application.Username, this.doc.ProjectInformation) & ModelPermissionsCategory.Admin) == ModelPermissionsCategory.Admin || this.doc.ProjectInformation.LookupParameter("ML_OVERRIDE") != null && this.doc.ProjectInformation.LookupParameter("ML_OVERRIDE").AsInteger() > 0))
    {
      if (App.ModelLockingAdminList.Contains(this.doc.Application.Username.ToUpper()))
        this.AddUser_ManageAdmins.Visible = true;
      else
        this.AddUser_ManageAdmins.Visible = false;
      this.btnOK.Visible = true;
      this.AddUser_Divider.Visible = true;
      this.AddUser_Title.Visible = true;
      this.AddUser_UsernameLbl.Visible = true;
      this.AddUser_UsernameTXT.Visible = true;
      this.AddUser_Button.Visible = true;
      this.AddUser_ChkBoxAdmin.Visible = true;
      this.AddUser_ChkBoxGeometry.Visible = true;
      this.AddUser_ChkBoxConnx.Visible = true;
      this.AddUser_ChkBoxRebar.Visible = true;
      this.AddUser_ChkBoxAssemblies.Visible = true;
    }
    else
    {
      this.btnOK.Visible = false;
      this.AddUser_ManageAdmins.Visible = false;
      this.AddUser_Divider.Visible = false;
      this.AddUser_Title.Visible = false;
      this.AddUser_UsernameLbl.Visible = false;
      this.AddUser_UsernameTXT.Visible = false;
      this.AddUser_Button.Visible = false;
      this.AddUser_ChkBoxAdmin.Visible = false;
      this.AddUser_ChkBoxGeometry.Visible = false;
      this.AddUser_ChkBoxConnx.Visible = false;
      this.AddUser_ChkBoxRebar.Visible = false;
      this.AddUser_ChkBoxAssemblies.Visible = false;
    }
  }

  private void btnOK_Click(object sender, EventArgs e)
  {
    if (!this.ValidateInput())
      return;
    this.DialogResult = DialogResult.OK;
  }

  private bool ValidateInput() => true;

  private void ModelLockingSettingsForm_Shown(object sender, EventArgs e)
  {
    this.txtPermAll.Text = this.ConvertUserListToString(this.Permissions_AllAllowed);
    this.txtPermGeom.Text = this.ConvertUserListToString(this.Permissions_GeometryAllowed);
    this.txtPermConn.Text = this.ConvertUserListToString(this.Permissions_ConnectionHardwareAllowed);
    this.txtPermRebar.Text = this.ConvertUserListToString(this.Permissions_RebarHandlingAllowed);
    this.txtAssemblies.Text = this.ConvertUserListToString(this.Permissions_AssembliesAllowed);
    this.EnableForAdmins();
  }

  private void ModelLockingSettingsForm_Resize(object sender, EventArgs e)
  {
  }

  private void btnCancel_Click(object sender, EventArgs e)
  {
  }

  private PermissionStatus addOrRemovePermission(
    PermissionFlags permission,
    PermissionFlags checkedPermissions,
    PermissionFlags userPermissions)
  {
    int num = (permission & checkedPermissions) == permission ? 1 : 0;
    bool flag = (permission & userPermissions) == permission;
    if (num != 0)
    {
      if (!flag)
        return PermissionStatus.Add;
    }
    else if (flag)
      return PermissionStatus.Delete;
    return PermissionStatus.Match;
  }

  private void AddUser_ManageAdmins_Click(object sender, EventArgs e)
  {
    string path = App.MLFolderPath + "\\ModelLockingAdminRoster.txt";
    if (File.Exists(path))
    {
      ManageAdmins manageAdmins = new ManageAdmins(this.doc, path);
      if (manageAdmins.bFileFailed)
        return;
      int num = (int) manageAdmins.ShowDialog();
      this.txtProdAdmin.Text = this.ConvertUserListToString(App.ModelLockingAdminList);
    }
    else
      TaskDialog.Show("Model Locking", $"Could not find Model Locking producer admin definition file at {path}. Please create this file and assign at least one Model Locking administrator");
  }

  private void AddUser_Button_Click(object sender, EventArgs e)
  {
    PermissionFlags checkedPermissions = this.currentCheckedFlags();
    string upper = this.AddUser_UsernameTXT.Text.ToUpper();
    PermissionFlags userPermissions = this.currentUserFlags(upper);
    switch (this.addOrRemovePermission(PermissionFlags.Admin, checkedPermissions, userPermissions))
    {
      case PermissionStatus.Add:
        this.Permissions_AllAllowed.Add(upper);
        break;
      case PermissionStatus.Delete:
        this.Permissions_AllAllowed.Remove(upper);
        break;
    }
    switch (this.addOrRemovePermission(PermissionFlags.Geometry, checkedPermissions, userPermissions))
    {
      case PermissionStatus.Add:
        this.Permissions_GeometryAllowed.Add(upper);
        break;
      case PermissionStatus.Delete:
        this.Permissions_GeometryAllowed.Remove(upper);
        break;
    }
    switch (this.addOrRemovePermission(PermissionFlags.ConnectionsHardware, checkedPermissions, userPermissions))
    {
      case PermissionStatus.Add:
        this.Permissions_ConnectionHardwareAllowed.Add(upper);
        break;
      case PermissionStatus.Delete:
        this.Permissions_ConnectionHardwareAllowed.Remove(upper);
        break;
    }
    switch (this.addOrRemovePermission(PermissionFlags.RebarHandling, checkedPermissions, userPermissions))
    {
      case PermissionStatus.Add:
        this.Permissions_RebarHandlingAllowed.Add(upper);
        break;
      case PermissionStatus.Delete:
        this.Permissions_RebarHandlingAllowed.Remove(upper);
        break;
    }
    switch (this.addOrRemovePermission(PermissionFlags.Assemblies, checkedPermissions, userPermissions))
    {
      case PermissionStatus.Add:
        this.Permissions_AssembliesAllowed.Add(upper);
        break;
      case PermissionStatus.Delete:
        this.Permissions_AssembliesAllowed.Remove(upper);
        break;
    }
    this.UpdateText();
    this.AddUser_Button.Enabled = false;
    this.btnOK.Enabled = true;
  }

  private void UpdateText()
  {
    this.txtPermAll.Text = this.ConvertUserListToString(this.Permissions_AllAllowed);
    this.txtPermGeom.Text = this.ConvertUserListToString(this.Permissions_GeometryAllowed);
    this.txtPermConn.Text = this.ConvertUserListToString(this.Permissions_ConnectionHardwareAllowed);
    this.txtPermRebar.Text = this.ConvertUserListToString(this.Permissions_RebarHandlingAllowed);
    this.txtAssemblies.Text = this.ConvertUserListToString(this.Permissions_AssembliesAllowed);
  }

  private void updateAddButton()
  {
    if (!string.IsNullOrWhiteSpace(this.AddUser_UsernameTXT.Text))
    {
      if (this.AddUser_UsernameTXT.Text.Contains<char>(';'))
      {
        this.AddUser_Button.Text = "Username cannot contain ;";
        this.AddUser_Button.Enabled = false;
      }
      else
      {
        PermissionFlags permissionFlags1 = this.currentCheckedFlags();
        PermissionFlags permissionFlags2 = this.currentUserFlags(this.AddUser_UsernameTXT.Text);
        this.AddUser_Button.Enabled = true;
        if (permissionFlags1 == PermissionFlags.None)
        {
          if (permissionFlags2 == PermissionFlags.None)
          {
            this.AddUser_Button.Text = "Add user for permissions";
            this.AddUser_Button.Enabled = false;
          }
          else
            this.AddUser_Button.Text = "Delete user permissions";
        }
        else if (permissionFlags2 == PermissionFlags.None)
          this.AddUser_Button.Text = "Add user for permissions";
        else if (permissionFlags1 == permissionFlags2)
        {
          this.AddUser_Button.Text = "User exists for these permissions";
          this.AddUser_Button.Enabled = false;
        }
        else
          this.AddUser_Button.Text = "Modify user permissions";
      }
    }
    else
    {
      this.AddUser_Button.Text = "Add user for permissions";
      this.AddUser_Button.Enabled = false;
    }
  }

  private void AddUser_UsernameTXT_TextChanged(object sender, EventArgs e)
  {
    this.updateAddButton();
  }

  private PermissionFlags currentUserFlags(string username)
  {
    string upper = username.ToUpper();
    PermissionFlags permissionFlags = PermissionFlags.None;
    if (this.Permissions_AllAllowed.Contains(upper))
    {
      permissionFlags |= PermissionFlags.Admin;
    }
    else
    {
      if (this.Permissions_GeometryAllowed.Contains(upper))
        permissionFlags |= PermissionFlags.Geometry;
      if (this.Permissions_ConnectionHardwareAllowed.Contains(upper))
        permissionFlags |= PermissionFlags.ConnectionsHardware;
      if (this.Permissions_RebarHandlingAllowed.Contains(upper))
        permissionFlags |= PermissionFlags.RebarHandling;
      if (this.Permissions_AssembliesAllowed.Contains(upper))
        permissionFlags |= PermissionFlags.Assemblies;
    }
    return permissionFlags;
  }

  private PermissionFlags currentCheckedFlags()
  {
    PermissionFlags permissionFlags = PermissionFlags.None;
    if (this.AddUser_ChkBoxAdmin.Checked)
    {
      permissionFlags |= PermissionFlags.Admin;
    }
    else
    {
      if (this.AddUser_ChkBoxGeometry.Checked)
        permissionFlags |= PermissionFlags.Geometry;
      if (this.AddUser_ChkBoxConnx.Checked)
        permissionFlags |= PermissionFlags.ConnectionsHardware;
      if (this.AddUser_ChkBoxRebar.Checked)
        permissionFlags |= PermissionFlags.RebarHandling;
      if (this.AddUser_ChkBoxAssemblies.Checked)
        permissionFlags |= PermissionFlags.Assemblies;
    }
    return permissionFlags;
  }

  private void AddUser_ChkBoxAdmin_CheckedChanged(object sender, EventArgs e)
  {
    this.updateAddButton();
    if (this.AddUser_ChkBoxAdmin.Checked)
    {
      this.AddUser_ChkBoxGeometry.Checked = true;
      this.AddUser_ChkBoxConnx.Checked = true;
      this.AddUser_ChkBoxRebar.Checked = true;
      this.AddUser_ChkBoxAssemblies.Checked = true;
      this.AddUser_ChkBoxGeometry.Enabled = false;
      this.AddUser_ChkBoxConnx.Enabled = false;
      this.AddUser_ChkBoxRebar.Enabled = false;
      this.AddUser_ChkBoxAssemblies.Enabled = false;
    }
    else
    {
      this.AddUser_ChkBoxGeometry.Checked = false;
      this.AddUser_ChkBoxConnx.Checked = false;
      this.AddUser_ChkBoxRebar.Checked = false;
      this.AddUser_ChkBoxAssemblies.Checked = false;
      this.AddUser_ChkBoxGeometry.Enabled = true;
      this.AddUser_ChkBoxConnx.Enabled = true;
      this.AddUser_ChkBoxRebar.Enabled = true;
      this.AddUser_ChkBoxAssemblies.Enabled = true;
    }
  }

  private void AddUser_ChkBoxGeometry_CheckedChanged(object sender, EventArgs e)
  {
    this.updateAddButton();
  }

  private void AddUser_ChkBoxConnx_CheckedChanged(object sender, EventArgs e)
  {
    this.updateAddButton();
  }

  private void AddUser_ChkBoxRebar_CheckedChanged(object sender, EventArgs e)
  {
    this.updateAddButton();
  }

  private void AddUser_ChkBoxAssemblies_CheckedChanged(object sender, EventArgs e)
  {
    this.updateAddButton();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.btnOK = new Button();
    this.label1 = new Label();
    this.txtPermAll = new TextBox();
    this.label2 = new Label();
    this.txtPermGeom = new TextBox();
    this.label3 = new Label();
    this.txtPermConn = new TextBox();
    this.label4 = new Label();
    this.txtPermRebar = new TextBox();
    this.btnCancel = new Button();
    this.label5 = new Label();
    this.label6 = new Label();
    this.label7 = new Label();
    this.txtAssemblies = new TextBox();
    this.txtProdAdmin = new TextBox();
    this.label8 = new Label();
    this.AddUser_Divider = new Label();
    this.AddUser_Title = new Label();
    this.AddUser_UsernameLbl = new Label();
    this.AddUser_UsernameTXT = new TextBox();
    this.AddUser_ChkBoxAdmin = new CheckBox();
    this.AddUser_ChkBoxGeometry = new CheckBox();
    this.AddUser_ChkBoxConnx = new CheckBox();
    this.AddUser_ChkBoxRebar = new CheckBox();
    this.AddUser_ChkBoxAssemblies = new CheckBox();
    this.AddUser_Button = new Button();
    this.AddUser_ManageAdmins = new Button();
    this.Lbl_CurrUser = new Label();
    this.SuspendLayout();
    this.btnOK.Anchor = AnchorStyles.Bottom;
    this.btnOK.Enabled = false;
    this.btnOK.Location = new System.Drawing.Point(735, 305);
    this.btnOK.Margin = new Padding(3, 2, 3, 2);
    this.btnOK.MaximumSize = new Size((int) sbyte.MaxValue, 35);
    this.btnOK.Name = "btnOK";
    this.btnOK.Size = new Size((int) sbyte.MaxValue, 35);
    this.btnOK.TabIndex = 5;
    this.btnOK.Text = "Save";
    this.btnOK.UseVisualStyleBackColor = true;
    this.btnOK.Click += new EventHandler(this.btnOK_Click);
    this.label1.AutoSize = true;
    this.label1.Location = new System.Drawing.Point(15, 76);
    this.label1.Name = "label1";
    this.label1.Size = new Size(47, 17);
    this.label1.TabIndex = 2;
    this.label1.Text = "Admin";
    this.txtPermAll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.txtPermAll.CharacterCasing = CharacterCasing.Upper;
    this.txtPermAll.Location = new System.Drawing.Point(196, 76);
    this.txtPermAll.Margin = new Padding(3, 2, 3, 2);
    this.txtPermAll.Name = "txtPermAll";
    this.txtPermAll.Size = new Size(794, 22);
    this.txtPermAll.TabIndex = 0;
    this.label2.AutoSize = true;
    this.label2.Location = new System.Drawing.Point(15, 102);
    this.label2.Name = "label2";
    this.label2.Size = new Size(70, 17);
    this.label2.TabIndex = 2;
    this.label2.Text = "Geometry";
    this.txtPermGeom.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.txtPermGeom.CharacterCasing = CharacterCasing.Upper;
    this.txtPermGeom.Location = new System.Drawing.Point(196, 102);
    this.txtPermGeom.Margin = new Padding(3, 2, 3, 2);
    this.txtPermGeom.Name = "txtPermGeom";
    this.txtPermGeom.Size = new Size(794, 22);
    this.txtPermGeom.TabIndex = 1;
    this.label3.AutoSize = true;
    this.label3.Location = new System.Drawing.Point(15, 128 /*0x80*/);
    this.label3.Name = "label3";
    this.label3.Size = new Size(159, 17);
    this.label3.TabIndex = 2;
    this.label3.Text = "Connections / Hardware";
    this.txtPermConn.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.txtPermConn.CharacterCasing = CharacterCasing.Upper;
    this.txtPermConn.Location = new System.Drawing.Point(196, 128 /*0x80*/);
    this.txtPermConn.Margin = new Padding(3, 2, 3, 2);
    this.txtPermConn.Name = "txtPermConn";
    this.txtPermConn.Size = new Size(794, 22);
    this.txtPermConn.TabIndex = 2;
    this.label4.AutoSize = true;
    this.label4.Location = new System.Drawing.Point(15, 153);
    this.label4.Name = "label4";
    this.label4.Size = new Size(115, 17);
    this.label4.TabIndex = 2;
    this.label4.Text = "Rebar / Handling";
    this.txtPermRebar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.txtPermRebar.CharacterCasing = CharacterCasing.Upper;
    this.txtPermRebar.Location = new System.Drawing.Point(196, 153);
    this.txtPermRebar.Margin = new Padding(3, 2, 3, 2);
    this.txtPermRebar.Name = "txtPermRebar";
    this.txtPermRebar.Size = new Size(794, 22);
    this.txtPermRebar.TabIndex = 3;
    this.btnCancel.Anchor = AnchorStyles.Bottom;
    this.btnCancel.DialogResult = DialogResult.Cancel;
    this.btnCancel.Location = new System.Drawing.Point(864, 305);
    this.btnCancel.Margin = new Padding(3, 2, 3, 2);
    this.btnCancel.MaximumSize = new Size((int) sbyte.MaxValue, 35);
    this.btnCancel.Name = "btnCancel";
    this.btnCancel.Size = new Size((int) sbyte.MaxValue, 35);
    this.btnCancel.TabIndex = 6;
    this.btnCancel.Text = "Close";
    this.btnCancel.UseVisualStyleBackColor = true;
    this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
    this.label5.AutoSize = true;
    this.label5.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.label5.Location = new System.Drawing.Point(14, 17);
    this.label5.Name = "label5";
    this.label5.Size = new Size(100, 25);
    this.label5.TabIndex = 2;
    this.label5.Text = "Category";
    this.label6.AutoSize = true;
    this.label6.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.label6.Location = new System.Drawing.Point(191, 17);
    this.label6.Name = "label6";
    this.label6.Size = new Size(150, 25);
    this.label6.TabIndex = 2;
    this.label6.Text = "Allowed Users";
    this.label7.AutoSize = true;
    this.label7.Location = new System.Drawing.Point(15, 179);
    this.label7.Name = "label7";
    this.label7.Size = new Size(79, 17);
    this.label7.TabIndex = 2;
    this.label7.Text = "Assemblies";
    this.txtAssemblies.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.txtAssemblies.CharacterCasing = CharacterCasing.Upper;
    this.txtAssemblies.Location = new System.Drawing.Point(196, 179);
    this.txtAssemblies.Margin = new Padding(3, 2, 3, 2);
    this.txtAssemblies.Name = "txtAssemblies";
    this.txtAssemblies.Size = new Size(794, 22);
    this.txtAssemblies.TabIndex = 4;
    this.txtProdAdmin.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.txtProdAdmin.CharacterCasing = CharacterCasing.Upper;
    this.txtProdAdmin.Location = new System.Drawing.Point(196, 50);
    this.txtProdAdmin.Margin = new Padding(3, 2, 3, 2);
    this.txtProdAdmin.Name = "txtProdAdmin";
    this.txtProdAdmin.Size = new Size(794, 22);
    this.txtProdAdmin.TabIndex = 7;
    this.txtProdAdmin.TabStop = false;
    this.label8.AutoSize = true;
    this.label8.Location = new System.Drawing.Point(15, 50);
    this.label8.Name = "label8";
    this.label8.Size = new Size(109, 17);
    this.label8.TabIndex = 8;
    this.label8.Text = "Producer Admin";
    this.AddUser_Divider.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
    this.AddUser_Divider.AutoSize = true;
    this.AddUser_Divider.BorderStyle = BorderStyle.Fixed3D;
    this.AddUser_Divider.Location = new System.Drawing.Point(6, 215);
    this.AddUser_Divider.MaximumSize = new Size(0, 2);
    this.AddUser_Divider.MinimumSize = new Size(1000, 0);
    this.AddUser_Divider.Name = "AddUser_Divider";
    this.AddUser_Divider.Size = new Size(1000, 2);
    this.AddUser_Divider.TabIndex = 9;
    this.AddUser_Title.Anchor = AnchorStyles.Bottom;
    this.AddUser_Title.AutoSize = true;
    this.AddUser_Title.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.AddUser_Title.Location = new System.Drawing.Point(13, 227);
    this.AddUser_Title.Name = "AddUser_Title";
    this.AddUser_Title.Size = new Size(310, 25);
    this.AddUser_Title.TabIndex = 10;
    this.AddUser_Title.Text = "Add/Remove User Permissions";
    this.AddUser_UsernameLbl.Anchor = AnchorStyles.Bottom;
    this.AddUser_UsernameLbl.AutoSize = true;
    this.AddUser_UsernameLbl.Location = new System.Drawing.Point(16 /*0x10*/, 262);
    this.AddUser_UsernameLbl.Name = "AddUser_UsernameLbl";
    this.AddUser_UsernameLbl.Size = new Size(73, 17);
    this.AddUser_UsernameLbl.TabIndex = 11;
    this.AddUser_UsernameLbl.Text = "Username";
    this.AddUser_UsernameTXT.Anchor = AnchorStyles.Bottom;
    this.AddUser_UsernameTXT.CharacterCasing = CharacterCasing.Upper;
    this.AddUser_UsernameTXT.Location = new System.Drawing.Point(95, 262);
    this.AddUser_UsernameTXT.Margin = new Padding(3, 2, 3, 2);
    this.AddUser_UsernameTXT.MaximumSize = new Size(300, 22);
    this.AddUser_UsernameTXT.MinimumSize = new Size(300, 22);
    this.AddUser_UsernameTXT.Name = "AddUser_UsernameTXT";
    this.AddUser_UsernameTXT.Size = new Size(300, 22);
    this.AddUser_UsernameTXT.TabIndex = 12;
    this.AddUser_UsernameTXT.TabStop = false;
    this.AddUser_UsernameTXT.TextChanged += new EventHandler(this.AddUser_UsernameTXT_TextChanged);
    this.AddUser_ChkBoxAdmin.Anchor = AnchorStyles.Bottom;
    this.AddUser_ChkBoxAdmin.AutoSize = true;
    this.AddUser_ChkBoxAdmin.Location = new System.Drawing.Point(401, 263);
    this.AddUser_ChkBoxAdmin.MinimumSize = new Size(0, 22);
    this.AddUser_ChkBoxAdmin.Name = "AddUser_ChkBoxAdmin";
    this.AddUser_ChkBoxAdmin.Size = new Size(69, 22);
    this.AddUser_ChkBoxAdmin.TabIndex = 13;
    this.AddUser_ChkBoxAdmin.Text = "Admin";
    this.AddUser_ChkBoxAdmin.UseVisualStyleBackColor = true;
    this.AddUser_ChkBoxAdmin.CheckedChanged += new EventHandler(this.AddUser_ChkBoxAdmin_CheckedChanged);
    this.AddUser_ChkBoxGeometry.Anchor = AnchorStyles.Bottom;
    this.AddUser_ChkBoxGeometry.AutoSize = true;
    this.AddUser_ChkBoxGeometry.Location = new System.Drawing.Point(476, 263);
    this.AddUser_ChkBoxGeometry.MinimumSize = new Size(0, 22);
    this.AddUser_ChkBoxGeometry.Name = "AddUser_ChkBoxGeometry";
    this.AddUser_ChkBoxGeometry.Size = new Size(92, 22);
    this.AddUser_ChkBoxGeometry.TabIndex = 13;
    this.AddUser_ChkBoxGeometry.Text = "Geometry";
    this.AddUser_ChkBoxGeometry.UseVisualStyleBackColor = true;
    this.AddUser_ChkBoxGeometry.CheckedChanged += new EventHandler(this.AddUser_ChkBoxGeometry_CheckedChanged);
    this.AddUser_ChkBoxConnx.Anchor = AnchorStyles.Bottom;
    this.AddUser_ChkBoxConnx.AutoSize = true;
    this.AddUser_ChkBoxConnx.Location = new System.Drawing.Point(574, 263);
    this.AddUser_ChkBoxConnx.MinimumSize = new Size(0, 22);
    this.AddUser_ChkBoxConnx.Name = "AddUser_ChkBoxConnx";
    this.AddUser_ChkBoxConnx.Size = new Size(181, 22);
    this.AddUser_ChkBoxConnx.TabIndex = 13;
    this.AddUser_ChkBoxConnx.Text = "Connections / Hardware";
    this.AddUser_ChkBoxConnx.UseVisualStyleBackColor = true;
    this.AddUser_ChkBoxConnx.CheckedChanged += new EventHandler(this.AddUser_ChkBoxConnx_CheckedChanged);
    this.AddUser_ChkBoxRebar.Anchor = AnchorStyles.Bottom;
    this.AddUser_ChkBoxRebar.AutoSize = true;
    this.AddUser_ChkBoxRebar.Location = new System.Drawing.Point(761, 263);
    this.AddUser_ChkBoxRebar.MinimumSize = new Size(0, 22);
    this.AddUser_ChkBoxRebar.Name = "AddUser_ChkBoxRebar";
    this.AddUser_ChkBoxRebar.Size = new Size(137, 22);
    this.AddUser_ChkBoxRebar.TabIndex = 13;
    this.AddUser_ChkBoxRebar.Text = "Rebar / Handling";
    this.AddUser_ChkBoxRebar.UseVisualStyleBackColor = true;
    this.AddUser_ChkBoxRebar.CheckedChanged += new EventHandler(this.AddUser_ChkBoxRebar_CheckedChanged);
    this.AddUser_ChkBoxAssemblies.Anchor = AnchorStyles.Bottom;
    this.AddUser_ChkBoxAssemblies.AutoSize = true;
    this.AddUser_ChkBoxAssemblies.Location = new System.Drawing.Point(904, 263);
    this.AddUser_ChkBoxAssemblies.MinimumSize = new Size(0, 22);
    this.AddUser_ChkBoxAssemblies.Name = "AddUser_ChkBoxAssemblies";
    this.AddUser_ChkBoxAssemblies.Size = new Size(101, 22);
    this.AddUser_ChkBoxAssemblies.TabIndex = 13;
    this.AddUser_ChkBoxAssemblies.Text = "Assemblies";
    this.AddUser_ChkBoxAssemblies.UseVisualStyleBackColor = true;
    this.AddUser_ChkBoxAssemblies.CheckedChanged += new EventHandler(this.AddUser_ChkBoxAssemblies_CheckedChanged);
    this.AddUser_Button.Anchor = AnchorStyles.Bottom;
    this.AddUser_Button.Enabled = false;
    this.AddUser_Button.Location = new System.Drawing.Point(19, 305);
    this.AddUser_Button.Margin = new Padding(3, 2, 3, 2);
    this.AddUser_Button.MaximumSize = new Size(376, 35);
    this.AddUser_Button.Name = "AddUser_Button";
    this.AddUser_Button.Size = new Size(376, 35);
    this.AddUser_Button.TabIndex = 14;
    this.AddUser_Button.Text = "Add user for permissions";
    this.AddUser_Button.UseVisualStyleBackColor = true;
    this.AddUser_Button.Click += new EventHandler(this.AddUser_Button_Click);
    this.AddUser_ManageAdmins.Anchor = AnchorStyles.Bottom;
    this.AddUser_ManageAdmins.Location = new System.Drawing.Point(401, 305);
    this.AddUser_ManageAdmins.Margin = new Padding(3, 2, 3, 2);
    this.AddUser_ManageAdmins.MaximumSize = new Size(181, 35);
    this.AddUser_ManageAdmins.Name = "AddUser_ManageAdmins";
    this.AddUser_ManageAdmins.Size = new Size(181, 35);
    this.AddUser_ManageAdmins.TabIndex = 14;
    this.AddUser_ManageAdmins.Text = "Manage Producer Admins";
    this.AddUser_ManageAdmins.UseVisualStyleBackColor = true;
    this.AddUser_ManageAdmins.Click += new EventHandler(this.AddUser_ManageAdmins_Click);
    this.Lbl_CurrUser.AutoSize = true;
    this.Lbl_CurrUser.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.Lbl_CurrUser.Location = new System.Drawing.Point(557, -5);
    this.Lbl_CurrUser.MaximumSize = new Size(670, 47);
    this.Lbl_CurrUser.MinimumSize = new Size(0, 47);
    this.Lbl_CurrUser.Name = "Lbl_CurrUser";
    this.Lbl_CurrUser.Padding = new Padding(0, 22, 30, 0);
    this.Lbl_CurrUser.Size = new Size(172, 47);
    this.Lbl_CurrUser.TabIndex = 15;
    this.Lbl_CurrUser.Text = "Current User:";
    this.AutoScaleDimensions = new SizeF(120f, 120f);
    this.AutoScaleMode = AutoScaleMode.Dpi;
    this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    this.CancelButton = (IButtonControl) this.btnCancel;
    this.ClientSize = new Size(1022, 357);
    this.ControlBox = false;
    this.Controls.Add((System.Windows.Forms.Control) this.Lbl_CurrUser);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_ManageAdmins);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_Button);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_ChkBoxAssemblies);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_ChkBoxRebar);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_ChkBoxConnx);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_ChkBoxGeometry);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_ChkBoxAdmin);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_UsernameTXT);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_UsernameLbl);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_Title);
    this.Controls.Add((System.Windows.Forms.Control) this.AddUser_Divider);
    this.Controls.Add((System.Windows.Forms.Control) this.txtProdAdmin);
    this.Controls.Add((System.Windows.Forms.Control) this.label8);
    this.Controls.Add((System.Windows.Forms.Control) this.txtAssemblies);
    this.Controls.Add((System.Windows.Forms.Control) this.label7);
    this.Controls.Add((System.Windows.Forms.Control) this.txtPermRebar);
    this.Controls.Add((System.Windows.Forms.Control) this.label4);
    this.Controls.Add((System.Windows.Forms.Control) this.txtPermConn);
    this.Controls.Add((System.Windows.Forms.Control) this.label3);
    this.Controls.Add((System.Windows.Forms.Control) this.txtPermGeom);
    this.Controls.Add((System.Windows.Forms.Control) this.label2);
    this.Controls.Add((System.Windows.Forms.Control) this.txtPermAll);
    this.Controls.Add((System.Windows.Forms.Control) this.label6);
    this.Controls.Add((System.Windows.Forms.Control) this.label5);
    this.Controls.Add((System.Windows.Forms.Control) this.label1);
    this.Controls.Add((System.Windows.Forms.Control) this.btnCancel);
    this.Controls.Add((System.Windows.Forms.Control) this.btnOK);
    this.Margin = new Padding(3, 2, 3, 2);
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    this.MinimumSize = new Size(1040, 404);
    this.Name = nameof (ModelLockingSettingsForm);
    this.StartPosition = FormStartPosition.CenterParent;
    this.Text = "Model Locking Permissions";
    this.Shown += new EventHandler(this.ModelLockingSettingsForm_Shown);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
