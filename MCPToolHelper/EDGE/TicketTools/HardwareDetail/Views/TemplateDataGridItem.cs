// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.TemplateDataGridItem
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class TemplateDataGridItem
{
  private bool selected;
  private string template;
  private string order = " ";

  public TemplateDataGridItem(string name, bool selectedBool = false, string orderString = " ")
  {
    this.template = name;
    if (!string.IsNullOrWhiteSpace(orderString))
      this.order = orderString;
    if (!selectedBool)
      return;
    this.selected = true;
  }

  public bool Selected
  {
    get => this.selected;
    set => this.selected = value;
  }

  public string Template
  {
    get => this.template;
    set => this.template = value;
  }

  public string Order
  {
    get => this.order;
    set => this.order = value;
  }
}
