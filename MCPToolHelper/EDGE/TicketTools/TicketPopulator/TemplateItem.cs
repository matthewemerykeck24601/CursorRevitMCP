// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.TemplateItem
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TemplateToolsBase;
using System;
using System.Windows.Forms;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class TemplateItem : ListViewItem, IComparable
{
  public int Priority { get; private set; }

  public TicketTemplate Template { get; private set; }

  public string name { get; private set; }

  public TemplateItem(TicketTemplate template, int priority)
  {
    this.Template = template;
    ListViewItem.ListViewSubItem listViewSubItem1 = new ListViewItem.ListViewSubItem((ListViewItem) this, "Template Name");
    listViewSubItem1.Name = nameof (Template);
    listViewSubItem1.Text = template.TemplateName;
    listViewSubItem1.Tag = (object) template;
    this.name = template.TemplateName;
    this.Priority = priority;
    ListViewItem.ListViewSubItem listViewSubItem2 = new ListViewItem.ListViewSubItem();
    listViewSubItem2.Name = nameof (Priority);
    listViewSubItem2.Text = priority == -1 ? "" : priority.ToString();
    listViewSubItem2.Tag = (object) priority;
    this.SubItems.Add(listViewSubItem1);
    this.SubItems.Add(listViewSubItem2);
    this.Tag = (object) this.Priority;
  }

  public int assignPriority(int newPriority)
  {
    this.Priority = newPriority;
    this.SubItems["Priority"].Text = newPriority == -1 ? "" : newPriority.ToString();
    this.SubItems["Priority"].Tag = (object) newPriority;
    this.Tag = (object) this.Priority;
    return this.Priority;
  }

  public int CompareTo(object other)
  {
    TemplateItem templateItem = other as TemplateItem;
    if (this.Priority == templateItem.Priority)
      return this.Template.TemplateName.CompareTo(templateItem.Template.TemplateName);
    return this.Priority == -1 || templateItem.Priority != -1 && this.Priority > templateItem.Priority ? 1 : -1;
  }
}
