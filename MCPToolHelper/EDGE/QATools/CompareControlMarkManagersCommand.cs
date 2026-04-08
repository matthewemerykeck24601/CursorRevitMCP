// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.CompareControlMarkManagersCommand
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#nullable disable
namespace EDGE.QATools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class CompareControlMarkManagersCommand : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    QA.LogLine("Begin Rebar Control Mark Manager Comparison");
    QA.LogLine(commandData.Application.ActiveUIDocument.Document, "");
    QA.LogLine("");
    return (Result) 0;
  }
}
