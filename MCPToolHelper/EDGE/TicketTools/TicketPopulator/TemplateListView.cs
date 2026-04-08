// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.TemplateListView
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Windows.Forms;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class TemplateListView : ListView
{
  protected override void OnKeyDown(KeyEventArgs e)
  {
    if (e.KeyCode == Keys.Prior)
    {
      e.Handled = e.SuppressKeyPress = true;
      ((TemplateForm) this.FindForm()).Bump(this.SelectedItems[0], true);
    }
    else if (e.KeyCode == Keys.Next)
    {
      e.Handled = e.SuppressKeyPress = true;
      ((TemplateForm) this.FindForm()).Bump(this.SelectedItems[0], false);
    }
    else
      base.OnKeyDown(e);
  }

  protected override void OnKeyUp(KeyEventArgs e)
  {
    if (e.KeyCode == Keys.Prior || e.KeyCode == Keys.Next)
      return;
    base.OnKeyUp(e);
  }

  public void SetupColumns()
  {
    this.Columns[0].Width = (int) Math.Floor((double) this.Width * 0.07);
    this.Columns[1].Width = (int) Math.Floor((double) this.Width * 0.75);
    this.Columns[2].Width = (int) Math.Floor((double) (this.Width - this.Columns[0].Width - this.Columns[1].Width) - (double) this.Width * 0.01);
  }
}
