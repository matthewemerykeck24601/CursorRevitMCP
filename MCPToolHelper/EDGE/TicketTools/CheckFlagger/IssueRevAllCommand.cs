// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CheckFlagger.IssueRevAllCommand
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.CheckFlagger;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class IssueRevAllCommand : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    List<ElementId> list = activeUiDocument.Selection.GetElementIds().ToList<ElementId>();
    List<AssemblyInstance> assemblies = new List<AssemblyInstance>();
    if (list.Count > 0 && !CheckFlaggerToolUtils.CheckEnvironmentGeneral(activeUiDocument, list, out string _, out assemblies))
      return (Result) 1;
    if (list.Count<ElementId>() == 0)
    {
      TaskDialog.Show("No elements selected", "Please pre-select the assembly or assemblies you want to check.");
      return (Result) 1;
    }
    List<AssemblyInstance> affected;
    if (!FlaggerUtils.SetNeedsRevAll(activeUiDocument, false, assemblies, out affected))
      return (Result) 1;
    if (affected.Count > 0)
    {
      TaskDialog successDialog = CheckFlaggerToolUtils.GenerateSuccessDialog(affected);
      successDialog.Title = "Issue Revision All";
      if (affected.Count == assemblies.Count)
      {
        successDialog.MainContent = "Revision issued for all selected assemblies. Expand for details";
      }
      else
      {
        successDialog.MainContent = "Revision issued for some of the selected assemblies. Expand for details";
        successDialog.ExpandedContent += Environment.NewLine;
        TaskDialog taskDialog1 = successDialog;
        taskDialog1.ExpandedContent = $"{taskDialog1.ExpandedContent}Assemblies that did not need revision:{Environment.NewLine}";
        foreach (AssemblyInstance assemblyInstance in assemblies)
        {
          AssemblyInstance assembly = assemblyInstance;
          if (!affected.Any<AssemblyInstance>((Func<AssemblyInstance, bool>) (a => a.UniqueId.Equals(assembly.UniqueId))))
          {
            TaskDialog taskDialog2 = successDialog;
            taskDialog2.ExpandedContent = taskDialog2.ExpandedContent + assembly.AssemblyTypeName + Environment.NewLine;
          }
        }
      }
      successDialog.Show();
    }
    else
      TaskDialog.Show("Issue Revision All", "None of the selected piece tickets needed revision, therefore no revisions were issued.");
    return (Result) 0;
  }
}
