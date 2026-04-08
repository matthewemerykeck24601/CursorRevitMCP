// Decompiled with JetBrains decompiler
// Type: EDGE.AnnotationTools.FreezeDrawingTools.FreezeDrawingCommand
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.ElementUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.AnnotationTools.FreezeDrawingTools;

[Transaction(TransactionMode.Manual)]
public class FreezeDrawingCommand : IExternalCommand
{
  private TaskDialogResult ShowDialog(
    string mainContent,
    string title = "Freeze View",
    TaskDialogCommonButtons buttons = 0)
  {
    TaskDialog taskDialog = new TaskDialog(title);
    taskDialog.MainContent = mainContent;
    if (buttons != null)
      taskDialog.CommonButtons = buttons;
    return taskDialog.Show();
  }

  private string ReplaceForbiddenSigns(string name)
  {
    name = name.Replace("[", "");
    name = name.Replace("]", "");
    name = name.Replace("}", "");
    name = name.Replace("{", "");
    name = name.Replace("|", "");
    name = name.Replace("?", "");
    name = name.Replace("'", "");
    name = name.Replace(":", "");
    name = name.Replace("\\", "");
    name = name.Replace("~", "");
    name = name.Replace(">", "");
    name = name.Replace("<", "");
    name = name.Replace(";", "");
    return name;
  }

  private void AssignName(View view, string name, string suffix = " (FROZEN)")
  {
    string str = name;
    int num = 1;
    while (true)
    {
      try
      {
        view.Name = str + suffix;
        break;
      }
      catch
      {
        str = $"{name}-{num.ToString()}";
        ++num;
      }
    }
  }

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document1 = commandData.Application.ActiveUIDocument.Document;
    if (document1.IsFamilyDocument)
    {
      this.ShowDialog("Freeze View cannot be run from the family editor.");
      return (Result) 1;
    }
    View activeView = document1.ActiveView;
    ElementId id = activeView.Id;
    if (!activeView.CanBePrinted)
    {
      this.ShowDialog("Current active view was invalid for Freeze View.");
      return (Result) 1;
    }
    if (document1.IsWorkshared)
    {
      if (!CheckElementsOwnership.CheckOwnershipBasic(document1, new List<ElementId>()
      {
        activeView.Id
      }))
      {
        this.ShowDialog("Active view is not editable due to worksharing issues. Please make sure you have ownership of the view you are trying to freeze before running the tool.");
        return (Result) 1;
      }
    }
    using (Transaction transaction = new Transaction(document1, "Import DWG"))
    {
      bool flag = false;
      int num1 = (int) transaction.Start();
      DWGExportOptions exOptions = new DWGExportOptions();
      exOptions.TargetUnit = ExportUnit.Foot;
      exOptions.LineScaling = LineScaling.ModelSpace;
      exOptions.SharedCoords = false;
      exOptions.PropOverrides = PropOverrideMode.ByEntity;
      exOptions.Colors = ExportColorMode.TrueColorPerView;
      DWGImportOptions dwgImportOptions = new DWGImportOptions();
      dwgImportOptions.Unit = ImportUnit.Foot;
      dwgImportOptions.ColorMode = ImportColorMode.BlackAndWhite;
      dwgImportOptions.Placement = ImportPlacement.Origin;
      dwgImportOptions.OrientToView = true;
      dwgImportOptions.VisibleLayersOnly = true;
      TaskDialogResult taskDialogResult;
      do
      {
        TaskDialog taskDialog = new TaskDialog("Freeze View");
        taskDialog.MainInstruction = "Freeze the Active View?";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Options");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Continue");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialogResult = taskDialog.Show();
        if (taskDialogResult != 1001)
        {
          if (taskDialogResult != 1002)
            return (Result) 1;
        }
        else
          new SettingsWin(exOptions, dwgImportOptions, commandData.Application.MainWindowHandle).ShowDialog();
      }
      while (taskDialogResult == 1001);
      ViewFamilyType viewFamilyType1 = (ViewFamilyType) null;
      List<Element> list = new FilteredElementCollector(document1).OfClass(typeof (ViewFamilyType)).Where<Element>((Func<Element, bool>) (e => ((ViewFamilyType) e).ViewFamily == ViewFamily.Drafting)).ToList<Element>();
      foreach (ViewFamilyType viewFamilyType2 in list)
      {
        if (viewFamilyType2.ViewFamily == ViewFamily.Drafting && viewFamilyType2.Name == "Freeze View")
        {
          viewFamilyType1 = viewFamilyType2;
          break;
        }
      }
      if (viewFamilyType1 == null)
      {
        if (!(list.FirstOrDefault<Element>() is ViewFamilyType viewFamilyType3))
        {
          this.ShowDialog("No valid drafting view types existed in the model, could not proceed with Freeze operation.");
          int num2 = (int) transaction.RollBack();
          return (Result) 1;
        }
        viewFamilyType1 = (ViewFamilyType) viewFamilyType3.Duplicate(viewFamilyType3.Name);
        viewFamilyType1.Name = "Freeze View";
      }
      string str = "_FROZEN - " + activeView.Name;
      if (activeView is View3D)
      {
        ViewSheet viewSheet = ViewSheet.Create(document1, ElementId.InvalidElementId);
        if (viewSheet != null)
        {
          XYZ point = new XYZ((viewSheet.Outline.Max.U - viewSheet.Outline.Min.U) / 2.0, (viewSheet.Outline.Max.V - viewSheet.Outline.Min.V) / 2.0, 0.0);
          try
          {
            Viewport.Create(document1, viewSheet.Id, activeView.Id, point);
          }
          catch (Exception ex)
          {
            TaskDialog taskDialog1 = new TaskDialog("Freeze View");
            taskDialog1.MainInstruction = "3D View invalid for Freeze View";
            if (activeView.GetPlacementOnSheetStatus() == 3)
            {
              taskDialog1.MainContent = "The active view could not be frozen due to being placed on a sheet.";
              string parameterAsString1 = Parameters.GetParameterAsString((Element) activeView, BuiltInParameter.VIEWPORT_SHEET_NUMBER);
              string parameterAsString2 = Parameters.GetParameterAsString((Element) activeView, BuiltInParameter.VIEWPORT_SHEET_NAME);
              if (!string.IsNullOrEmpty(parameterAsString1) && !string.IsNullOrEmpty(parameterAsString2))
              {
                taskDialog1.MainContent += " Expand for details.";
                taskDialog1.ExpandedContent = "Already placed on Sheet: ";
                TaskDialog taskDialog2 = taskDialog1;
                taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{parameterAsString1} - ";
                taskDialog1.ExpandedContent += parameterAsString2;
              }
            }
            else
              taskDialog1.MainContent = "The active view could not be frozen.";
            taskDialog1.Show();
            int num3 = (int) transaction.RollBack();
            return (Result) 1;
          }
          id = viewSheet.Id;
          document1.Regenerate();
          flag = true;
        }
      }
      if (!Directory.Exists("C:\\EDGEforRevit\\FreezeDwgTemp"))
      {
        try
        {
          Directory.CreateDirectory("C:\\EDGEforRevit\\FreezeDwgTemp");
        }
        catch (Exception ex)
        {
          TaskDialog taskDialog = new TaskDialog("Freeze View")
          {
            MainContent = "Failed to create temporary DWG export directory. Please make sure you have read/write access to C:\\EDGEforRevit\\FreezeDwgTemp",
            ExpandedContent = "Exception Details: " + Environment.NewLine
          };
          taskDialog.ExpandedContent = taskDialog.ExpandedContent + ex.Message + Environment.NewLine;
          taskDialog.ExpandedContent += ex.StackTrace;
          taskDialog.Show();
          return (Result) 1;
        }
      }
      else if (new TaskDialog("Freeze View")
      {
        MainInstruction = "Do you want to continue?",
        MainContent = "It looks like you have an existing directory at C:\\EDGEforRevit\\FreezeDwgTemp. Note that this directory will be deleted as part of the Freeze View export and cleanup process.",
        CommonButtons = ((TaskDialogCommonButtons) 6)
      }.Show() != 6)
        return (Result) 1;
      try
      {
        Document document2 = document1;
        string name = str;
        List<ElementId> views = new List<ElementId>();
        views.Add(id);
        DWGExportOptions options = exOptions;
        if (document2.Export("C:\\EDGEforRevit\\FreezeDwgTemp", name, (ICollection<ElementId>) views, options))
        {
          string file = $"C:\\EDGEforRevit\\FreezeDwgTemp\\{str}.dwg";
          BoundingBoxXYZ boundingBoxXyz = new BoundingBoxXYZ();
          ViewDrafting pDBView = ViewDrafting.Create(document1, viewFamilyType1.Id);
          this.AssignName((View) pDBView, this.ReplaceForbiddenSigns(document1.ActiveView.Name));
          document1.Regenerate();
          document1.Import(file, dwgImportOptions, (View) pDBView, out ElementId _);
          try
          {
            Directory.Delete("C:\\EDGEforRevit\\FreezeDwgTemp", true);
          }
          catch (Exception ex)
          {
            TaskDialog taskDialog = new TaskDialog("Freeze View")
            {
              MainContent = "Error deleting temp DWG files. Please check C:\\EDGEforRevit\\FreezeDwgTemp if you wish to delete them. Freeze procedure will continue.",
              ExpandedContent = "Exception Details: " + Environment.NewLine
            };
            taskDialog.ExpandedContent = taskDialog.ExpandedContent + ex.Message + Environment.NewLine;
            taskDialog.ExpandedContent += ex.StackTrace;
            taskDialog.Show();
          }
          if (flag)
            document1.Delete(id);
          int num4 = (int) transaction.Commit();
          if (transaction.GetStatus() == TransactionStatus.Committed)
          {
            if (new TaskDialog("Freeze View")
            {
              MainInstruction = "Freeze View Successful",
              ExpandedContent = $"Active View {activeView.Name} frozen and copied to view {pDBView.Name}",
              MainContent = "Switch to the new view?",
              CommonButtons = ((TaskDialogCommonButtons) 6)
            }.Show() == 6)
              commandData.Application.ActiveUIDocument.ActiveView = (View) pDBView;
            return (Result) 0;
          }
          new TaskDialog("Freeze View")
          {
            MainInstruction = "Freeze View Failed",
            MainContent = "Something went wrong and the Freeze Active View tool could not complete the operation. Please try again."
          }.Show();
          return (Result) -1;
        }
      }
      catch (Exception ex)
      {
        if (ex.Message.Contains("invalid characters"))
          new TaskDialog("Freeze View")
          {
            MainInstruction = "Freeze View Failed",
            MainContent = "The file name that the tool attempted to create contained invalid characters.",
            ExpandedContent = "Invalid characters include, but are not limited to: \\ {} [] <> | ` ~ ? ; : "
          }.Show();
      }
    }
    return (Result) 1;
  }
}
