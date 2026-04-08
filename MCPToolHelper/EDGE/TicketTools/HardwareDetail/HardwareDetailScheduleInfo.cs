// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.HardwareDetailScheduleInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TemplateToolsBase;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail;

public class HardwareDetailScheduleInfo
{
  public string BOMScheduleName = "Error:NameStringNotSet";
  public int iScheduleOrderIndex;
  public BOMJustification BOMJustification;
  public SimpleVector vectorToAnchorPoint;
  public string ViewTemplateName;
  public bool isNoteSchedule;
}
