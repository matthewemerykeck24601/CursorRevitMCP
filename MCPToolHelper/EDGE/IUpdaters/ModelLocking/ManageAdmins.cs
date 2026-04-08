// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ManageAdmins
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

public class ManageAdmins : System.Windows.Forms.Form
{
  public bool bFileFailed;
  private const char delimiter = ';';
  private string currentUser = "";
  private string filePath = "";
  private IContainer components;
  private ListBox List_Admins;
  private Button Btn_DeleteAdmin;
  private Button Btn_AddAdmin;
  private Button Btn_Cancel;
  private TextBox TXT_Username;
  private Button Btn_SaveAndClose;
  private Label TXT_Output;
  private Label label2;
  private Label Lbl_CurrUser;
  private RichTextBox TXT_FilePath;

  public ManageAdmins(Document revitDoc)
  {
    this.InitializeComponent();
    this.currentUser = revitDoc.Application.Username.ToUpper();
  }

  public ManageAdmins(Document revitDoc, string path)
  {
    this.bFileFailed = false;
    this.InitializeComponent();
    if (!this.initializeList(path))
      return;
    this.filePath = path;
    this.TXT_FilePath.Text = this.filePath;
    this.TXT_FilePath.SelectAll();
    this.TXT_FilePath.SelectionAlignment = HorizontalAlignment.Center;
    this.TXT_FilePath.DeselectAll();
    this.currentUser = revitDoc.Application.Username.ToUpper();
    this.Lbl_CurrUser.Text = "Current User: " + this.currentUser;
  }

  private bool initializeList(string path)
  {
    try
    {
      if (File.Exists(path))
      {
        string[] strArray = File.ReadAllLines(path);
        this.List_Admins.Items.Clear();
        foreach (string str in strArray)
          this.List_Admins.Items.Add((object) str.ToUpper());
      }
    }
    catch (IOException ex)
    {
      this.bFileFailed = true;
      TaskDialog.Show("Manage Model Locking Admins", "Could not read producer admin list file. Please ensure that this file is not currently open or otherwise inaccessible.");
      return false;
    }
    return true;
  }

  public bool HasItems() => this.List_Admins.Items.Count > 0;

  private void Btn_AddAdmin_Click(object sender, EventArgs e)
  {
    if (this.TXT_Username.Text.Contains<char>(';'))
    {
      this.TXT_Output.ForeColor = System.Drawing.Color.DarkRed;
      this.TXT_Output.Text = "Username cannot contain ;";
    }
    else
    {
      this.List_Admins.Items.Add((object) this.TXT_Username.Text.ToUpper());
      this.Btn_SaveAndClose.Enabled = true;
      this.TXT_Output.ForeColor = System.Drawing.Color.ForestGreen;
      this.TXT_Output.Text = $"User {this.TXT_Username.Text} added as producer admin.";
      this.TXT_Username.Text = "";
    }
  }

  private void Btn_DeleteAdmin_Click(object sender, EventArgs e)
  {
    if (this.List_Admins.Items.Count > 1)
    {
      string selected = this.getSelected();
      this.List_Admins.Items.RemoveAt(this.List_Admins.SelectedIndex);
      this.Btn_SaveAndClose.Enabled = true;
      this.TXT_Output.ForeColor = System.Drawing.Color.Green;
      this.TXT_Output.Text = $"User {selected} deleted.";
    }
    else
    {
      this.TXT_Output.ForeColor = System.Drawing.Color.DarkRed;
      this.TXT_Output.Text = "Cannot delete last producer admin.";
    }
  }

  private void Form1_Load(object sender, EventArgs e)
  {
  }

  private void Btn_Cancel_Click(object sender, EventArgs e)
  {
    this.DialogResult = DialogResult.Cancel;
    this.Close();
  }

  private void Btn_SaveAndClose_Click(object sender, EventArgs e)
  {
    FileInfo fileInfo = new FileInfo(this.filePath);
    if (fileInfo.Exists && fileInfo.IsReadOnly)
    {
      this.TXT_Output.ForeColor = System.Drawing.Color.DarkRed;
      this.TXT_Output.Text = $"File at {this.filePath} is read-only.";
    }
    else
    {
      try
      {
        File.WriteAllLines(this.filePath, this.List_Admins.Items.Cast<string>());
        this.DialogResult = DialogResult.OK;
      }
      catch (UnauthorizedAccessException ex)
      {
        this.TXT_Output.ForeColor = System.Drawing.Color.DarkRed;
        this.TXT_Output.Text = this.filePath + " could not be written to or created.";
        return;
      }
      App.ModelLockingAdminList = new List<string>();
      foreach (string str in this.List_Admins.Items)
      {
        if (!App.ModelLockingAdminList.Contains(str))
          App.ModelLockingAdminList.Add(str);
      }
      this.Close();
    }
  }

  private void TXT_Username_TextChanged(object sender, EventArgs e)
  {
    if (string.IsNullOrWhiteSpace(this.TXT_Username.Text) || this.List_Admins.Items.Contains((object) this.TXT_Username.Text))
    {
      this.Btn_AddAdmin.Enabled = false;
    }
    else
    {
      if (this.Btn_AddAdmin.Enabled)
        return;
      this.Btn_AddAdmin.Enabled = true;
    }
  }

  private void List_Admins_SelectedIndexChanged(object sender, EventArgs e)
  {
    string selected = this.getSelected();
    if (string.IsNullOrEmpty(selected))
      this.Btn_DeleteAdmin.Enabled = false;
    else if (selected != this.currentUser)
      this.Btn_DeleteAdmin.Enabled = true;
    else
      this.Btn_DeleteAdmin.Enabled = false;
  }

  private string getSelected()
  {
    return this.List_Admins.SelectedItem != null ? this.List_Admins.SelectedItem.ToString() : "";
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.List_Admins = new ListBox();
    this.Btn_DeleteAdmin = new Button();
    this.Btn_AddAdmin = new Button();
    this.Btn_Cancel = new Button();
    this.TXT_Username = new TextBox();
    this.Btn_SaveAndClose = new Button();
    this.TXT_Output = new Label();
    this.label2 = new Label();
    this.Lbl_CurrUser = new Label();
    this.TXT_FilePath = new RichTextBox();
    this.SuspendLayout();
    this.List_Admins.FormattingEnabled = true;
    this.List_Admins.ItemHeight = 16 /*0x10*/;
    this.List_Admins.Location = new System.Drawing.Point(15, (int) sbyte.MaxValue);
    this.List_Admins.Margin = new Padding(10, 20, 10, 20);
    this.List_Admins.Name = "List_Admins";
    this.List_Admins.Size = new Size(327, 292);
    this.List_Admins.TabIndex = 0;
    this.List_Admins.TabStop = false;
    this.List_Admins.SelectedIndexChanged += new EventHandler(this.List_Admins_SelectedIndexChanged);
    this.Btn_DeleteAdmin.Enabled = false;
    this.Btn_DeleteAdmin.Location = new System.Drawing.Point(15, 447);
    this.Btn_DeleteAdmin.Name = "Btn_DeleteAdmin";
    this.Btn_DeleteAdmin.Size = new Size(327, 30);
    this.Btn_DeleteAdmin.TabIndex = 4;
    this.Btn_DeleteAdmin.Text = "Delete Admin";
    this.Btn_DeleteAdmin.UseVisualStyleBackColor = true;
    this.Btn_DeleteAdmin.Click += new EventHandler(this.Btn_DeleteAdmin_Click);
    this.Btn_AddAdmin.Enabled = false;
    this.Btn_AddAdmin.Location = new System.Drawing.Point(15, 539);
    this.Btn_AddAdmin.Name = "Btn_AddAdmin";
    this.Btn_AddAdmin.Size = new Size(327, 30);
    this.Btn_AddAdmin.TabIndex = 1;
    this.Btn_AddAdmin.Text = "Add Admin";
    this.Btn_AddAdmin.UseVisualStyleBackColor = true;
    this.Btn_AddAdmin.Click += new EventHandler(this.Btn_AddAdmin_Click);
    this.Btn_Cancel.DialogResult = DialogResult.Cancel;
    this.Btn_Cancel.Dock = DockStyle.Bottom;
    this.Btn_Cancel.Location = new System.Drawing.Point(15, 668);
    this.Btn_Cancel.Name = "Btn_Cancel";
    this.Btn_Cancel.Size = new Size(327, 30);
    this.Btn_Cancel.TabIndex = 3;
    this.Btn_Cancel.Text = "Cancel";
    this.Btn_Cancel.UseVisualStyleBackColor = true;
    this.Btn_Cancel.Click += new EventHandler(this.Btn_Cancel_Click);
    this.TXT_Username.CharacterCasing = CharacterCasing.Upper;
    this.TXT_Username.Location = new System.Drawing.Point(15, 511 /*0x01FF*/);
    this.TXT_Username.Name = "TXT_Username";
    this.TXT_Username.Size = new Size(327, 22);
    this.TXT_Username.TabIndex = 0;
    this.TXT_Username.TextChanged += new EventHandler(this.TXT_Username_TextChanged);
    this.Btn_SaveAndClose.Dock = DockStyle.Bottom;
    this.Btn_SaveAndClose.Enabled = false;
    this.Btn_SaveAndClose.Location = new System.Drawing.Point(15, 638);
    this.Btn_SaveAndClose.Name = "Btn_SaveAndClose";
    this.Btn_SaveAndClose.Size = new Size(327, 30);
    this.Btn_SaveAndClose.TabIndex = 2;
    this.Btn_SaveAndClose.Text = "Save And Close";
    this.Btn_SaveAndClose.UseVisualStyleBackColor = true;
    this.Btn_SaveAndClose.Click += new EventHandler(this.Btn_SaveAndClose_Click);
    this.TXT_Output.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
    this.TXT_Output.AutoSize = true;
    this.TXT_Output.BackColor = SystemColors.Control;
    this.TXT_Output.BorderStyle = BorderStyle.Fixed3D;
    this.TXT_Output.ForeColor = System.Drawing.Color.Green;
    this.TXT_Output.Location = new System.Drawing.Point(15, 572);
    this.TXT_Output.MaximumSize = new Size(327, 60);
    this.TXT_Output.MinimumSize = new Size(327, 60);
    this.TXT_Output.Name = "TXT_Output";
    this.TXT_Output.Padding = new Padding(3);
    this.TXT_Output.Size = new Size(327, 60);
    this.TXT_Output.TabIndex = 3;
    this.TXT_Output.TextAlign = ContentAlignment.TopCenter;
    this.label2.Anchor = AnchorStyles.Top;
    this.label2.AutoSize = true;
    this.label2.Location = new System.Drawing.Point(15, 38);
    this.label2.MaximumSize = new Size(327, 0);
    this.label2.MinimumSize = new Size(327, 0);
    this.label2.Name = "label2";
    this.label2.Size = new Size(327, 17);
    this.label2.TabIndex = 6;
    this.label2.Text = "Model Locking Admin File Path";
    this.label2.TextAlign = ContentAlignment.TopCenter;
    this.Lbl_CurrUser.Anchor = AnchorStyles.Top;
    this.Lbl_CurrUser.AutoSize = true;
    this.Lbl_CurrUser.Font = new Font("Microsoft Sans Serif", 10.2f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.Lbl_CurrUser.Location = new System.Drawing.Point(3, 7);
    this.Lbl_CurrUser.MaximumSize = new Size(351, 0);
    this.Lbl_CurrUser.MinimumSize = new Size(351, 0);
    this.Lbl_CurrUser.Name = "Lbl_CurrUser";
    this.Lbl_CurrUser.Size = new Size(351, 20);
    this.Lbl_CurrUser.TabIndex = 7;
    this.Lbl_CurrUser.Text = "Current User";
    this.Lbl_CurrUser.TextAlign = ContentAlignment.TopCenter;
    this.TXT_FilePath.BorderStyle = BorderStyle.None;
    this.TXT_FilePath.Location = new System.Drawing.Point(15, 59);
    this.TXT_FilePath.MaximumSize = new Size(326, 0);
    this.TXT_FilePath.MinimumSize = new Size(326, 60);
    this.TXT_FilePath.Name = "TXT_FilePath";
    this.TXT_FilePath.ReadOnly = true;
    this.TXT_FilePath.ScrollBars = RichTextBoxScrollBars.Vertical;
    this.TXT_FilePath.Size = new Size(326, 60);
    this.TXT_FilePath.TabIndex = 8;
    this.TXT_FilePath.TabStop = false;
    this.TXT_FilePath.Text = "";
    this.AcceptButton = (IButtonControl) this.Btn_AddAdmin;
    this.AutoScaleDimensions = new SizeF(120f, 120f);
    this.AutoScaleMode = AutoScaleMode.Dpi;
    this.CancelButton = (IButtonControl) this.Btn_Cancel;
    this.ClientSize = new Size(357, 713);
    this.Controls.Add((System.Windows.Forms.Control) this.TXT_FilePath);
    this.Controls.Add((System.Windows.Forms.Control) this.Lbl_CurrUser);
    this.Controls.Add((System.Windows.Forms.Control) this.label2);
    this.Controls.Add((System.Windows.Forms.Control) this.TXT_Output);
    this.Controls.Add((System.Windows.Forms.Control) this.TXT_Username);
    this.Controls.Add((System.Windows.Forms.Control) this.Btn_SaveAndClose);
    this.Controls.Add((System.Windows.Forms.Control) this.Btn_Cancel);
    this.Controls.Add((System.Windows.Forms.Control) this.Btn_AddAdmin);
    this.Controls.Add((System.Windows.Forms.Control) this.Btn_DeleteAdmin);
    this.Controls.Add((System.Windows.Forms.Control) this.List_Admins);
    this.MaximizeBox = false;
    this.MaximumSize = new Size(375, 760);
    this.MinimizeBox = false;
    this.MinimumSize = new Size(375, 760);
    this.Name = nameof (ManageAdmins);
    this.Padding = new Padding(15);
    this.ShowIcon = false;
    this.SizeGripStyle = SizeGripStyle.Hide;
    this.Text = "Manage Model Locking Producer Admins";
    this.TopMost = true;
    this.Load += new EventHandler(this.Form1_Load);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
