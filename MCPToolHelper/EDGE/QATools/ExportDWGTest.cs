// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.ExportDWGTest
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

#nullable disable
namespace EDGE.QATools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ExportDWGTest : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      UIApplication application = commandData.Application;
      Document document = application.ActiveUIDocument.Document;
      new ExportDWGTestWindow(application).ShowDialog();
      return (Result) 0;
    }
    catch (Exception ex)
    {
      return (Result) 1;
    }
  }
}
