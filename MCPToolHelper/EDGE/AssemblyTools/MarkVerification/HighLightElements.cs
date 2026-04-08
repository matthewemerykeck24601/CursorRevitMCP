// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.HighLightElements
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.AssemblyTools.MarkVerification.ResultsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification;

public class HighLightElements : IExternalEventHandler
{
  public void Execute(UIApplication uiapp)
  {
    using (TransactionGroup transactionGroup = new TransactionGroup(uiapp.ActiveUIDocument.Document, "Problem Solid"))
    {
      int num1 = (int) transactionGroup.Start();
      using (Transaction transaction1 = new Transaction(uiapp.ActiveUIDocument.Document, "highlight solid"))
      {
        int num2 = (int) transaction1.Start();
        try
        {
          Document document = uiapp.ActiveUIDocument.Document;
          bool directShapeExists = false;
          List<ElementId> listOfIds = App.MarkVerificationSolidToBeHighlighted.ListOfIds;
          if (App.MarkVerificationSolidToBeHighlighted.directShape != ElementId.InvalidElementId)
          {
            directShapeExists = true;
            listOfIds.Add(App.MarkVerificationSolidToBeHighlighted.directShape);
          }
          if (App.MarkVerificationSolidToBeHighlighted.isolate)
            uiapp.ActiveUIDocument.ActiveView.IsolateElementsTemporary((ICollection<ElementId>) App.MarkVerificationSolidToBeHighlighted.ListOfIds);
          App.DialogSwitches.SuspendEntourageWarning = true;
          this.highlightexecutor(uiapp, listOfIds, App.MarkVerificationSolidToBeHighlighted.directShape, directShapeExists);
        }
        catch (Exception ex)
        {
          if (ex.Message.Contains("Sequence"))
          {
            uiapp.ActiveUIDocument.Document.Delete(App.MarkVerificationSolidToBeHighlighted.ListOfIds.Last<ElementId>());
            uiapp.ActiveUIDocument.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
          }
        }
        int num3 = (int) transaction1.Commit();
        using (Transaction transaction2 = new Transaction(uiapp.ActiveUIDocument.Document, "lock view"))
        {
          int num4 = (int) transaction2.Start();
          try
          {
            App.MarkVerificationSolidToBeHighlighted.detailWindow.Hide();
            App.MarkVerificationSolidToBeHighlighted.overallResults.Hide();
            uiapp.ActiveUIDocument.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new allElements(), "Press Esc to exit member geometry comparison");
          }
          catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
          {
            uiapp.ActiveUIDocument.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
            App.MarkVerificationSolidToBeHighlighted.overallResults.Show();
            App.MarkVerificationSolidToBeHighlighted.overallResults.IsHitTestVisible = true;
            App.MarkVerificationSolidToBeHighlighted.detailWindow.Show();
            App.MarkVerificationSolidToBeHighlighted.detailWindow.IsHitTestVisible = true;
          }
          int num5 = (int) transaction2.Commit();
        }
      }
      App.MarkVerificationSolidToBeHighlighted.detailWindow = (MKExistingDetails) null;
      App.MarkVerificationSolidToBeHighlighted.overallResults = (MKVerificationResults_Existing) null;
      App.MarkVerificationSolidToBeHighlighted.ListOfIds.Clear();
      int num6 = (int) transactionGroup.RollBack();
    }
  }

  public string GetName() => "Highlight Elements in Geometry Comparison";

  private void highlightexecutor(
    UIApplication uiapp,
    List<ElementId> ids,
    ElementId directShape,
    bool directShapeExists)
  {
    List<ElementId> source = new List<ElementId>();
    source.Add(directShape);
    uiapp.ActiveUIDocument.Selection.SetElementIds((ICollection<ElementId>) source);
    uiapp.ActiveUIDocument.ShowElements((ICollection<ElementId>) ids);
    if (!directShapeExists)
      return;
    Color color1 = new Color((byte) 0, (byte) 0, (byte) 250);
    Color color2 = new Color((byte) 0, (byte) 250, (byte) 0);
    OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
    Color color3 = new Color((byte) 250, (byte) 0, (byte) 0);
    overrideGraphicSettings.SetCutForegroundPatternColor(color3);
    overrideGraphicSettings.SetCutBackgroundPatternColor(color3);
    overrideGraphicSettings.SetCutLineColor(color3);
    overrideGraphicSettings.SetCutForegroundPatternVisible(true);
    overrideGraphicSettings.SetCutBackgroundPatternVisible(true);
    overrideGraphicSettings.SetSurfaceForegroundPatternColor(color3);
    overrideGraphicSettings.SetSurfaceTransparency(0);
    overrideGraphicSettings.SetSurfaceBackgroundPatternColor(color3);
    overrideGraphicSettings.SetProjectionLineColor(color1);
    overrideGraphicSettings.SetSurfaceForegroundPatternVisible(true);
    overrideGraphicSettings.SetSurfaceBackgroundPatternVisible(true);
    FillPatternElement patternElementByName = FillPatternElement.GetFillPatternElementByName(uiapp.ActiveUIDocument.Document, FillPatternTarget.Drafting, "<Solid fill>");
    overrideGraphicSettings.SetSurfaceBackgroundPatternId(patternElementByName.Id);
    uiapp.ActiveUIDocument.ActiveView.SetElementOverrides(source.First<ElementId>(), overrideGraphicSettings);
  }
}
