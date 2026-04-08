// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarMatchProperties
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using Utils.Exceptions;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.RebarTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class RebarMatchProperties : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    using (Transaction transaction = new Transaction(activeUiDocument.Document, "Rebar Match Properties"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        Reference reference;
        try
        {
          reference = activeUiDocument.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new RebarSelectionFilter(), "Select a Rebar To Copy From");
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          return (Result) 1;
        }
        catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
        {
          ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
          return (Result) 1;
        }
        if (reference != null)
        {
          int num2 = 1;
          do
            ;
          while (num2 == 1);
          return (Result) 0;
        }
        int num3 = (int) transaction.Commit();
      }
      catch (Exception ex)
      {
        new TaskDialog("Rebar Match Properties Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainContent = "There was an error updating the rebar with the parameters copied from the host rebar element."
        }.Show();
        return (Result) 1;
      }
    }
    return (Result) 1;
  }
}
