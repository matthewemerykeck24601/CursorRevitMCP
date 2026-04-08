// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TitleBlockPopulator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class TitleBlockPopulator : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    using (Transaction transaction = new Transaction(activeUiDocument.Document, "Title Block Populator"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        if (!(activeUiDocument.ActiveView is ViewSheet) || activeUiDocument.ActiveView.AssociatedAssemblyInstanceId == ElementId.InvalidElementId || activeUiDocument.ActiveView.AssociatedAssemblyInstanceId == (ElementId) null)
        {
          new TaskDialog("EDGE Error")
          {
            MainInstruction = "Current View is not an Assembly Sheet View",
            MainContent = "Title Block Populator should be run in an assembly sheet view.  Please switch to a sheet view and re-run the tool.",
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show();
          return (Result) 1;
        }
        if (activeUiDocument.Document.GetElement(activeUiDocument.ActiveView.AssociatedAssemblyInstanceId) is AssemblyInstance element && Utils.ElementUtils.Parameters.GetParameterAsBool((Element) element, "HARDWARE_DETAIL"))
        {
          new TaskDialog("EDGE Error")
          {
            MainInstruction = "Current View is a Hardware Detail Assembly Sheet View",
            MainContent = "Title Block Populator should be run in a ticket assembly sheet view.  Please switch to a sheet view that is not a hardware detail and re-run the tool. To update the title block of a hardware detail assembly sheet view please use the Hardware Detail tool.",
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show();
          return (Result) 1;
        }
        if (element != null && element.GetStructuralFramingElement() == null)
        {
          new TaskDialog("EDGE Error")
          {
            MainInstruction = "Current View is a invalid Assembly Sheet View",
            MainContent = "Title Block Populator should be run in a ticket assembly sheet view of an a structural framing assembly.  Please switch to a sheet view of a structural framing assembly and re-run the tool.",
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
          }.Show();
          return (Result) 1;
        }
        List<Element> invalidWeightInputsOut = new List<Element>();
        List<Element> weightParametesDoNotExistOut = new List<Element>();
        Result result = TitleblockPopCore.PopulateTicketTitleBlock(activeUiDocument, ref message, activeUiDocument.ActiveView as ViewSheet, out invalidWeightInputsOut, out weightParametesDoNotExistOut);
        if (result != null)
        {
          int num2 = (int) transaction.RollBack();
          return result;
        }
        if (weightParametesDoNotExistOut.Count > 0)
        {
          TaskDialog taskDialog = new TaskDialog("Ticket Title Block Populator Warning");
          taskDialog.MainContent = "Neither the UNIT_WEIGHT or WEIGHT_PER_UNIT parameters exist for one or more of the elements (shown below). This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
          taskDialog.ExpandedContent += "Elements:\n";
          List<string> stringList = new List<string>();
          foreach (Element elem in weightParametesDoNotExistOut)
          {
            string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
            if (!stringList.Contains(str))
              stringList.Add(str);
          }
          stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
          foreach (string str in stringList)
            taskDialog.ExpandedContent += str;
          taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
          if (taskDialog.Show() != 1)
          {
            int num3 = (int) transaction.RollBack();
            return (Result) 1;
          }
        }
        if (invalidWeightInputsOut.Count > 0)
        {
          TaskDialog taskDialog = new TaskDialog("Ticket Title Block Populator Warning");
          taskDialog.MainContent = "There are one or more elements (shown below) in which the UNIT_WEIGHT or WEIGHT_PER_UNIT has a value equal to or less than zero. This may result in your weight values being incorrectly calculated. Would you like to continue running the tool?";
          taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
          taskDialog.ExpandedContent += "Elements:\n";
          List<string> stringList = new List<string>();
          foreach (Element elem in invalidWeightInputsOut)
          {
            string str = $"{elem.GetControlMark()}: {elem.Id.ToString()}\n";
            if (!stringList.Contains(str))
              stringList.Add(str);
          }
          stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
          foreach (string str in stringList)
            taskDialog.ExpandedContent += str;
          if (taskDialog.Show() != 1)
          {
            int num4 = (int) transaction.RollBack();
            return (Result) 1;
          }
        }
        return result;
      }
      catch (Exception ex)
      {
        TaskDialog.Show("Error", "Exception in TitleBlock Populator.  Please Contact Support: " + ex.Message);
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
        if (transaction.HasStarted() && transaction.Commit() != TransactionStatus.Committed)
          TaskDialog.Show("Error", "Transaction didn't committed!");
      }
    }
  }
}
