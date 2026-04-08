// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.TicketScheduleInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Text.RegularExpressions;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase;

public class TicketScheduleInfo
{
  public string BOMScheduleNameString = "Error:NameStringNotSet";
  public BOMJustification BOMJustification;
  public SimpleVector vectorToAnchorPoint;
  public int iScheduleOrderIndex;

  public void SetBOMScheduleNameStringFromScheduleName(
    string scheduleName,
    string assemblyName,
    out bool Conforms)
  {
    this.BOMScheduleNameString = this.GetBOMScheduleNameString(scheduleName, assemblyName, out Conforms);
  }

  public string GetNewScheduleName(string assemblyName)
  {
    return new Regex("^z_").Match(this.BOMScheduleNameString).Success ? this.BOMScheduleNameString : $"ZZ_{assemblyName}_{this.BOMScheduleNameString}";
  }

  private string GetBOMScheduleNameString(
    string scheduleNameFromMasterTemplate,
    string assemblyName,
    out bool Conforms)
  {
    Conforms = true;
    Match match1 = new Regex("^(?<schedName>^z_.*)", RegexOptions.IgnoreCase).Match(scheduleNameFromMasterTemplate);
    if (match1.Success)
    {
      string scheduleNameString = match1.Groups["schedName"].Value;
      if (!string.IsNullOrWhiteSpace(scheduleNameString))
        return scheduleNameString;
      QA.InHouseMessage("Unable to match schedule name from string: " + scheduleNameFromMasterTemplate);
      return "scheduleNameFromMasterTemplate: Error: " + scheduleNameFromMasterTemplate;
    }
    Match match2 = new Regex($"^ZZ_{assemblyName}_(?<schedName>.*)").Match(scheduleNameFromMasterTemplate);
    Match match3 = new Regex($"^ZZ_{assemblyName.ToUpper()}_(?<schedName>.*)").Match(scheduleNameFromMasterTemplate);
    if (match2.Success || match3.Success)
    {
      string scheduleNameString = match2.Groups["schedName"].Value;
      if (string.IsNullOrWhiteSpace(scheduleNameString))
        scheduleNameString = match3.Groups["schedName"].Value;
      if (!string.IsNullOrWhiteSpace(scheduleNameString))
        return scheduleNameString;
      QA.InHouseMessage("Unable to match schedule name from string: " + scheduleNameFromMasterTemplate);
      return "scheduleNameFromMasterTemplate: Error: " + scheduleNameFromMasterTemplate;
    }
    QA.InHouseMessage("Failed to parse schedule name from template: " + scheduleNameFromMasterTemplate);
    Conforms = false;
    return "Failed to parse schedule name from template: " + scheduleNameFromMasterTemplate;
  }
}
