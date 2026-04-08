// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ViewModels.HistoryWindowViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketManager.Resources;
using EDGE.TicketTools.TicketManager.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager.ViewModels;

public class HistoryWindowViewModel
{
  public AssemblyViewModel SelectedItem { get; set; }

  public string WindowTitle { get; set; }

  public string DataGridLabel { get; set; }

  public List<Comment> Comments { get; set; }

  public Command CloseCommand { get; set; }

  public Command AcceptEditsCommand { get; set; }

  [DebuggerHidden]
  public bool CanExecuteClose(object parameter) => true;

  public void ExecuteClose(object parameter)
  {
    if (!(parameter is HistoryWindow))
      return;
    (parameter as HistoryWindow).Close();
  }

  public HistoryWindowViewModel()
  {
  }

  public HistoryWindowViewModel(AssemblyViewModel model)
  {
    this.WindowTitle = "Edit History - " + model.MarkNumber;
    this.DataGridLabel = $"{model.MarkNumber} - {model.Description}";
    this.Comments = HistoryWindowViewModel.ParseComments(model);
    this.CloseCommand = new Command(new Command.ICommandOnExecute(this.ExecuteClose), new Command.ICommandOnCanExecute(this.CanExecuteClose));
  }

  public static List<Comment> ParseComments(AssemblyViewModel model)
  {
    List<Comment> comments = new List<Comment>();
    string[] strArray1 = Parameters.GetParameterAsString((Element) model.Assembly, "TICKET_EDIT_COMMENT").Split(new string[1]
    {
      "|"
    }, StringSplitOptions.RemoveEmptyEntries);
    for (int index1 = 0; index1 < strArray1.Length; ++index1)
    {
      string[] strArray2 = new string[5]
      {
        (index1 + 1).ToString(),
        "N/A",
        "N/A",
        "N/A",
        "N/A"
      };
      string[] strArray3 = strArray1[index1].Split(new string[1]
      {
        ";"
      }, StringSplitOptions.RemoveEmptyEntries);
      for (int index2 = 0; index2 < strArray3.Length; ++index2)
        strArray2[index2 + 1] = strArray3[index2];
      comments.Add(new Comment(strArray2[0], strArray2[1], strArray2[2], strArray2[3], strArray2[4]));
    }
    comments.Reverse();
    return comments;
  }
}
