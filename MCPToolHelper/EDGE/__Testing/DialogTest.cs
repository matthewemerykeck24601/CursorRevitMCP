// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.DialogTest
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Utils.Forms.EDGEDialogs;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class DialogTest : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    DialogTestInput dialogTestInput = new DialogTestInput();
    dialogTestInput.ShowDialog();
    string title = string.IsNullOrEmpty(dialogTestInput._Title) ? "Test Dialog - ERROR" : dialogTestInput._Title;
    string instruction = dialogTestInput._Instruction;
    string content = dialogTestInput._Content;
    string help = dialogTestInput._Help;
    UniversalEDGEDialog universalEdgeDialog = new UniversalEDGEDialog(commandData.Application, title, instruction);
    universalEdgeDialog.DialogContent = content;
    universalEdgeDialog.DialogButtons = dialogTestInput._Buttons;
    universalEdgeDialog.Help = help;
    universalEdgeDialog.ShowDialog();
    TaskDialog.Show("Outcome", "Selected option was: " + universalEdgeDialog.selectedOutcome.ToString());
    return (Result) 0;
  }
}
