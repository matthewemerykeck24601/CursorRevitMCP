// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ViewModels.Comment
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.TicketTools.TicketManager.ViewModels;

public class Comment
{
  public string EditNumber { get; set; }

  public string Action { get; set; }

  public string Date { get; set; }

  public string User { get; set; }

  public string Description { get; set; }

  public Comment(string editNumber, string action, string date, string user, string description)
  {
    this.EditNumber = editNumber;
    this.Action = action;
    this.Date = date;
    this.User = user;
    this.Description = description;
  }

  public Comment()
  {
    this.Action = "N/A";
    this.Date = "N/A";
    this.User = "N/A";
    this.Description = "N/A";
  }
}
