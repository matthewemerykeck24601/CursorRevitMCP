// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.ResetBPH
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.AdminUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ResetBPH : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    ActiveModel.GetInformation(activeUiDocument);
    using (Transaction transaction = new Transaction(document, "Reset BPH Parameters"))
    {
      try
      {
        if (new TaskDialog("Reset BOM_PRODUCT_HOST Parameters.")
        {
          MainInstruction = "Are you sure you want to rest BOM_PRODUCT_HOST Parameter values for all Elements in the model?",
          CommonButtons = ((TaskDialogCommonButtons) 6),
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          DefaultButton = ((TaskDialogResult) 7)
        }.Show() != 6)
          return (Result) 1;
        int num1 = (int) transaction.Start();
        Reset.BOMProductHost(document);
        new TaskDialog("ResetBPH Parameters")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "Success.  BOM_PRODUCT_HOST parameter values have been reset"
        }.Show();
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
