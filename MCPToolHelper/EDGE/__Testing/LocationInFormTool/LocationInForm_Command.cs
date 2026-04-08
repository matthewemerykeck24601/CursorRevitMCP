// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.LocationInFormTool.LocationInForm_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.LocationInFormTool;

[Transaction(TransactionMode.Manual)]
public class LocationInForm_Command : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    if (!(document.ActiveView is ViewSheet) || !(document.ActiveView as ViewSheet).IsAssemblyView)
    {
      message = "Current view is not an assembly view sheet";
      return (Result) -1;
    }
    LocationInFormAnalyzer analyzer = new LocationInFormAnalyzer(activeUiDocument.Document.GetElement(activeUiDocument.ActiveView.AssociatedAssemblyInstanceId) as AssemblyInstance, 0.75);
    TaskDialog taskDialog = new TaskDialog("Pick Level");
    taskDialog.MainInstruction = "Pick Level";
    taskDialog.MainContent = "select in form level";
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Down in Form");
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Side in Form");
    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Top in Form");
    taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
    TaskDialogResult taskDialogResult = (TaskDialogResult) 1;
    while (taskDialogResult != 2)
    {
      taskDialogResult = taskDialog.Show();
      FormSide sideRequested = FormSide.Top;
      if (taskDialogResult == 1001)
        sideRequested = FormSide.Down;
      else if (taskDialogResult == 1002)
        sideRequested = FormSide.Side;
      this.HighlightSideElements(activeUiDocument, sideRequested, analyzer);
    }
    return (Result) 0;
  }

  private void HighlightSideElements(
    UIDocument uiDoc,
    FormSide sideRequested,
    LocationInFormAnalyzer analyzer)
  {
    List<ElementId> elementIdList = new List<ElementId>();
    switch (sideRequested)
    {
      case FormSide.Top:
        elementIdList = analyzer.ElementsInTopFaces;
        break;
      case FormSide.Down:
        elementIdList = analyzer.ElementsInDownFaces;
        break;
      case FormSide.Side:
        elementIdList = analyzer.ElementsInSideFaces;
        break;
    }
    uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }
}
