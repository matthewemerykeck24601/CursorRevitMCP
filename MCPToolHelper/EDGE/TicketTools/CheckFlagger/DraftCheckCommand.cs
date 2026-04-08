// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CheckFlagger.DraftCheckCommand
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.CheckFlagger;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class DraftCheckCommand : IExternalCommand
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
    if (!FlaggerUtils.CheckAll(activeUiDocument, InfoType.DraftCheck, CheckFlaggerToolUtils.GetUsername(document), assemblies))
      return (Result) 1;
    TaskDialog successDialog = CheckFlaggerToolUtils.GenerateSuccessDialog(assemblies);
    successDialog.Title = "Drafting Check All";
    successDialog.MainContent = "Drafting Check applied to all selected assemblies. Expand for details";
    successDialog.Show();
    return (Result) 0;
  }
}
