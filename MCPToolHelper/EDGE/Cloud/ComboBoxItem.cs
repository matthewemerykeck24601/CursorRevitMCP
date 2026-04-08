// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.ComboBoxItem
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.Cloud;

internal class ComboBoxItem
{
  public string Value { get; set; }

  public string Text { get; set; }

  public string Text1 { get; set; }

  public string Text2 { get; set; }

  public override string ToString() => this.Text;
}
