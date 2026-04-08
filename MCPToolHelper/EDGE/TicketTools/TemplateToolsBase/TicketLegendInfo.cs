// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.TicketLegendInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

public class TicketLegendInfo
{
  public string LegendName = "";
  public SimpleVector VectorToUpperLeft;
  public bool IsStrandPatternTemplate;

  public TicketLegendInfo()
  {
  }

  public TicketLegendInfo(
    string lengendName,
    SimpleVector vectorToUpperLeft,
    bool strandPatternTemplate = false)
  {
    this.LegendName = lengendName;
    this.VectorToUpperLeft = vectorToUpperLeft;
    this.IsStrandPatternTemplate = strandPatternTemplate;
  }
}
