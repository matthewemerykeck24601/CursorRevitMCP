// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationRemoval.MainInsulationRemoval
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#nullable disable
namespace EDGE.InsulationTools.InsulationRemoval;

[Transaction(TransactionMode.Manual)]
public class MainInsulationRemoval : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Insulation Removal Tool must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Insulation Removal Tool must be run in the project environment.  Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(document, "Insulation Removal"))
    {
      int num1 = (int) transactionGroup.Start();
      TaskDialog taskDialog = new TaskDialog("Insulation Removal");
      taskDialog.Title = "Insulation Removal";
      taskDialog.TitleAutoPrefix = true;
      taskDialog.AllowCancellation = true;
      taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
      taskDialog.MainInstruction = "Insulation Removal";
      taskDialog.MainContent = "Select the scope for removing insulation from Structural Framing Element(s).";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Remove Insulation for the Whole Project.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Remove Insulation for the Active View.");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Remove Insulation for a Selection Group.");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      Result result;
      if (taskDialogResult == 1001)
        result = InsulationRemoveWholeModel.WholeModel(document);
      else if (taskDialogResult == 1002)
        result = InsulationRemovalActiveView.ActiveView(document);
      else if (taskDialogResult == 1003)
      {
        result = InsulationRemovalSelectionGroup.SelectionGroup(document, application, activeUiDocument);
      }
      else
      {
        int num2 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      if (result == 1 || result == -1)
      {
        int num3 = (int) transactionGroup.RollBack();
        return result;
      }
      int num4 = (int) transactionGroup.Commit();
      return result;
    }
  }
}
