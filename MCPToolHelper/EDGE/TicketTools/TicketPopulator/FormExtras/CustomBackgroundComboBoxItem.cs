// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.FormExtras.CustomBackgroundComboBoxItem
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Drawing;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator.FormExtras;

public class CustomBackgroundComboBoxItem
{
  public string Text { get; set; }

  public int Scale { get; set; }

  public Color BackColor { get; set; }

  public CustomBackgroundComboBoxItem(string itemText, int scale)
  {
    this.Text = itemText;
    this.Scale = scale;
    this.BackColor = Color.AntiqueWhite;
  }

  public CustomBackgroundComboBoxItem(string itemText, int scale, Color color)
  {
    this.Text = itemText;
    this.Scale = scale;
    this.BackColor = color;
  }

  public override string ToString() => this.Text;
}
