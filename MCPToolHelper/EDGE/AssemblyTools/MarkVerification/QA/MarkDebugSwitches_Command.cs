// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.QA.MarkDebugSwitches_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.QA;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class MarkDebugSwitches_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    MarkQA markQa = new MarkQA();
    DebugModeSwitches debugModeSwitches = new DebugModeSwitches();
    debugModeSwitches.debugSwitches = markQa.GetSwitches();
    int num = (int) debugModeSwitches.ShowDialog();
    markQa.SetSwitches(debugModeSwitches.debugSwitches);
    return (Result) 0;
  }
}
