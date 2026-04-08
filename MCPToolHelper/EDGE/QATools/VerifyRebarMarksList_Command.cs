// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.VerifyRebarMarksList_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#nullable disable
namespace EDGE.QATools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class VerifyRebarMarksList_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    if (App.RebarManager.Manager(document.PathName).VerifyMarkLists(document))
      return (Result) 0;
    message = "Mark Verification Failed for current manager!";
    return (Result) -1;
  }
}
