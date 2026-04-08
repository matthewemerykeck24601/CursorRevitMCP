// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.TemplateForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TemplateToolsBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class TemplateForm : System.Windows.Forms.Form
{
  private bool bLoading;
  private Dictionary<int, TemplateItem> itemsByPriority = new Dictionary<int, TemplateItem>();
  private List<TicketTemplate> TicketTemplateList = new List<TicketTemplate>();
  public List<TicketTemplate> SelectedTemplateList = new List<TicketTemplate>();
  private IContainer components;
  private TemplateListView TemplateList;
  private ColumnHeader TemplateHeader;
  private ColumnHeader IndexHeader;
  private ColumnHeader CheckHeader;
  private Button Btn_OK;
  private Button Btn_Cancel;
  private TextBox textBox1;

  public TemplateForm(
    Document revitDoc,
    List<TicketTemplate> templates,
    List<TicketTemplate> selected)
  {
    this.TicketTemplateList = templates;
    this.SelectedTemplateList = selected;
    this.InitializeComponent();
    this.TemplateList.ListViewItemSorter = (IComparer) new TemplateItemComparer();
  }

  private void Form1_Load(object sender, EventArgs e)
  {
    this.bLoading = true;
    foreach (TicketTemplate template in this.TicketTemplateList.Where<TicketTemplate>((Func<TicketTemplate, bool>) (template => !this.SelectedTemplateList.Contains(template))).ToList<TicketTemplate>())
      this.TemplateList.Items.Add((ListViewItem) new TemplateItem(template, -1));
    foreach (TicketTemplate selectedTemplate in this.SelectedTemplateList)
    {
      TemplateItem templateItem = new TemplateItem(selectedTemplate, -1);
      this.TemplateList.Items.Add((ListViewItem) templateItem);
      templateItem.Checked = true;
    }
    this.sortItems();
    this.TemplateList.SetupColumns();
    this.bLoading = false;
  }

  private void TemplateForm_Checked(object sender, ItemCheckedEventArgs e)
  {
    if (this.bLoading && !e.Item.Checked)
      return;
    TemplateItem templateItem = e.Item as TemplateItem;
    if (e.Item.Checked)
    {
      int num = this.itemsByPriority.Keys.Count == 0 ? 1 : this.itemsByPriority.Keys.Max() + 1;
      templateItem.assignPriority(num);
      this.itemsByPriority.Add(num, templateItem);
    }
    else if (this.itemsByPriority.ContainsKey(templateItem.Priority))
    {
      this.itemsByPriority.Remove(templateItem.Priority);
      templateItem.assignPriority(-1);
    }
    this.sortItems();
  }

  private void sortItems()
  {
    this.TemplateList.Sort();
    int num = 1;
    this.itemsByPriority.Clear();
    foreach (TemplateItem templateItem in this.TemplateList.Items)
    {
      if (templateItem.Priority != -1)
      {
        templateItem.assignPriority(num);
        this.itemsByPriority.Add(num, templateItem);
        ++num;
      }
    }
  }

  public void Bump(ListViewItem item, bool bUp)
  {
    int num1 = bUp ? 1 : 0;
    TemplateItem templateItem1 = item as TemplateItem;
    int num2 = this.TemplateList.Items.IndexOf(item);
    if (bUp && num2 == 0 || !bUp && num2 == this.TemplateList.Items.Count - 1)
      return;
    TemplateItem templateItem2 = (bUp ? this.TemplateList.Items[this.TemplateList.Items.IndexOf((ListViewItem) templateItem1) - 1] : this.TemplateList.Items[this.TemplateList.Items.IndexOf((ListViewItem) templateItem1) + 1]) as TemplateItem;
    if (templateItem1.Priority == -1 || templateItem2.Priority == -1)
      return;
    int priority1 = templateItem1.Priority;
    int priority2 = templateItem2.Priority;
    templateItem1.assignPriority(priority2);
    templateItem2.assignPriority(priority1);
    this.sortItems();
  }

  private void TemplateForm_ResizeEnd(object sender, EventArgs e)
  {
    this.TemplateList.SetupColumns();
  }

  private void Btn_OK_Click(object sender, EventArgs e)
  {
    List<TemplateItem> source = new List<TemplateItem>();
    foreach (TemplateItem templateItem in this.TemplateList.Items)
    {
      if (templateItem.Checked)
        source.Add(templateItem);
    }
    this.SelectedTemplateList = source.OrderBy<TemplateItem, int>((Func<TemplateItem, int>) (item => item.Priority)).Select<TemplateItem, TicketTemplate>((Func<TemplateItem, TicketTemplate>) (item => item.Template)).ToList<TicketTemplate>();
    this.DialogResult = DialogResult.OK;
    this.Close();
  }

  private void Btn_Cancel_Click(object sender, EventArgs e)
  {
    this.DialogResult = DialogResult.Cancel;
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
    this.Btn_OK = new Button();
    this.Btn_Cancel = new Button();
    this.textBox1 = new TextBox();
    this.TemplateList = new TemplateListView();
    this.CheckHeader = new ColumnHeader();
    this.TemplateHeader = new ColumnHeader();
    this.IndexHeader = new ColumnHeader();
    this.SuspendLayout();
    this.Btn_OK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
    this.Btn_OK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    this.Btn_OK.Location = new System.Drawing.Point(27, 405);
    this.Btn_OK.Name = "Btn_OK";
    this.Btn_OK.Size = new Size(160 /*0xA0*/, 32 /*0x20*/);
    this.Btn_OK.TabIndex = 1;
    this.Btn_OK.Text = "OK";
    this.Btn_OK.UseVisualStyleBackColor = true;
    this.Btn_OK.Click += new EventHandler(this.Btn_OK_Click);
    this.Btn_Cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
    this.Btn_Cancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
    this.Btn_Cancel.Location = new System.Drawing.Point(289, 405);
    this.Btn_Cancel.Name = "Btn_Cancel";
    this.Btn_Cancel.Size = new Size(160 /*0xA0*/, 32 /*0x20*/);
    this.Btn_Cancel.TabIndex = 2;
    this.Btn_Cancel.Text = "Cancel";
    this.Btn_Cancel.UseVisualStyleBackColor = true;
    this.Btn_Cancel.Click += new EventHandler(this.Btn_Cancel_Click);
    this.textBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.textBox1.BackColor = SystemColors.Menu;
    this.textBox1.BorderStyle = BorderStyle.None;
    this.textBox1.Enabled = false;
    this.textBox1.Location = new System.Drawing.Point(27, 352);
    this.textBox1.Multiline = true;
    this.textBox1.Name = "textBox1";
    this.textBox1.ReadOnly = true;
    this.textBox1.Size = new Size(421, 47);
    this.textBox1.TabIndex = 3;
    this.textBox1.Text = "Use Page Up and Page Down to move templates up/down in template sequence";
    this.textBox1.TextAlign = HorizontalAlignment.Center;
    this.TemplateList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.TemplateList.CheckBoxes = true;
    this.TemplateList.Columns.AddRange(new ColumnHeader[3]
    {
      this.CheckHeader,
      this.TemplateHeader,
      this.IndexHeader
    });
    this.TemplateList.FullRowSelect = true;
    this.TemplateList.HideSelection = false;
    this.TemplateList.Location = new System.Drawing.Point(27, 25);
    this.TemplateList.Margin = new Padding(20, 18, 20, 18);
    this.TemplateList.Name = "TemplateList";
    this.TemplateList.Size = new Size(422, 314);
    this.TemplateList.Sorting = SortOrder.Ascending;
    this.TemplateList.TabIndex = 0;
    this.TemplateList.UseCompatibleStateImageBehavior = false;
    this.TemplateList.View = System.Windows.Forms.View.Details;
    this.TemplateList.ItemChecked += new ItemCheckedEventHandler(this.TemplateForm_Checked);
    this.CheckHeader.Text = "";
    this.CheckHeader.Width = 21;
    this.TemplateHeader.Text = "Template";
    this.TemplateHeader.Width = 312;
    this.IndexHeader.Text = "Order";
    this.IndexHeader.Width = 84;
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(476, 449);
    this.Controls.Add((System.Windows.Forms.Control) this.textBox1);
    this.Controls.Add((System.Windows.Forms.Control) this.Btn_Cancel);
    this.Controls.Add((System.Windows.Forms.Control) this.Btn_OK);
    this.Controls.Add((System.Windows.Forms.Control) this.TemplateList);
    this.Margin = new Padding(4);
    this.Name = nameof (TemplateForm);
    this.Padding = new Padding(27, 25, 27, 25);
    this.Text = "Select Templates";
    this.Load += new EventHandler(this.Form1_Load);
    this.ResizeEnd += new EventHandler(this.TemplateForm_ResizeEnd);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
