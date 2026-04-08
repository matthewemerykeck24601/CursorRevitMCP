// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.FormExtras.CustomDrawComboBox
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator.FormExtras;

public class CustomDrawComboBox : ComboBox
{
  public CustomDrawComboBox() => this.DrawMode = DrawMode.OwnerDrawFixed;

  protected override void OnDrawItem(DrawItemEventArgs e)
  {
    try
    {
      base.OnDrawItem(e);
      if (e == null)
      {
        QA.LogError(nameof (CustomDrawComboBox), "DrawItemEventArgs was null");
      }
      else
      {
        e.DrawBackground();
        CustomBackgroundComboBoxItem backgroundComboBoxItem = (CustomBackgroundComboBoxItem) this.Items[e.Index];
        Brush brush1 = (Brush) new SolidBrush(backgroundComboBoxItem.BackColor);
        Brush brush2 = (Brush) new SolidBrush(Color.Black);
        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
          brush1 = Brushes.CornflowerBlue;
        e.Graphics.FillRectangle(brush1, new Rectangle()
        {
          X = e.Bounds.X,
          Y = e.Bounds.Y,
          Height = e.Bounds.Height,
          Width = e.Bounds.Width
        });
        e.Graphics.DrawString(backgroundComboBoxItem.Text, this.Font, brush2, (float) e.Bounds.X, (float) e.Bounds.Y);
      }
    }
    catch (Exception ex)
    {
    }
  }
}
