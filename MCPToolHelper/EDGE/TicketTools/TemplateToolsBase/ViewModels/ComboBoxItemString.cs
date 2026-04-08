// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.ViewModels.ComboBoxItemString
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase.ViewModels;

public class ComboBoxItemString
{
  public string ValueString { get; set; }

  public ComboBoxItemString() => this.ValueString = (string) null;

  public ComboBoxItemString(string value) => this.ValueString = value;
}
